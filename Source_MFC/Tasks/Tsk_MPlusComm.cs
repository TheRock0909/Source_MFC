using Source_MFC.Global;
using Source_MFC.HW.M_;
using Source_MFC.Sequence.SubTasks;
using Source_MFC.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Tasks
{
    public class Tsk_MPlusComm : _TSKBASE
    {
        public Tsk_MPlusComm(MainCtrl main)
        {
            _ctrl = main;
            arg = new SUBTSKARG(eTASKLIST.MPLUSCOMM);
        }

        bool b1stConnected = true;
        Task<bool> sendTsk;
        override public void Run()
        {
            MPlus mplus = _ctrl.mplus;
            if (null == mplus) return;
            if (_ctrl._status.devsCont.mPlus == eCOMSTATUS.DISCONNECTED) return;
            if (true == arg.bStop || eERROR.None != arg.nErr) return;

            try
            {                
                switch (arg.nStep)
                {
                    case DEF_CONST.SEQ_INIT:
                        arg.nStep = 10;
                        ChkTactTime();
                        arg.nStatus = eSTATE.Working;
                        break;
                    case 10:
                        arg.nStep = 100;
                        break;

                    // MFC 상태보고 구간.
                    case 100:
                        arg.nStep = 110;
                        arg.ResetTactTime();
                        break;
                    case 110:
                        var queDelay = (0 <= _QueueCnt) ? 1 * 1000 : 0;
                        if (false == arg.tSen.IsOver(queDelay)) break;
                        if (true == b1stConnected)
                        {
                            arg.nStep = 100;
                            b1stConnected = false;
                            _ctrl.Job_SetState(eJOBST.UserStopped);                            
                            Logger.Inst.Write(CmdLogType.prdt, $"첫번째 M+ 연결과 같아 {eJOBST.UserStopped.ToString()}를 전송합니다.");
                        }
                        else
                        {
                            arg.nStep = 120;
                            SetStatus();
                        }
                        break;
                    case 120:
                        {
                            sendTsk = SendSatate();                            
                            arg.nStep = 125;
                            break;
                        }
                    case 125:
                        if (false == sendTsk.IsCompleted) break;
                        arg.nStep = 200;
                        break;

                    // Queueing Data 보고구간.
                    case 200:
                        {
                            if (0 >= _QueueCnt)
                            {
                                arg.nStep = 100;
                            }
                            else
                            {
                                var rtn = Queue_SetCurr();
                                if (true == rtn)
                                {
                                    arg.nStep = 210;
                                    arg.nStatus = eSTATE.Checking;
                                }
                            }
                            break;
                        }
                    case 210:
                        {
                            switch (_CurrQueue.cmd)
                            {
                                case eCMD4MPLUS.DISTANCEBTW:
                                    {
                                        arg.nStep = 213;
                                        var data = new SENDARG() { goal_1st = _CurrQueue._distbtw.goal1, goal_2nd = _CurrQueue._distbtw.goal2 };
                                        _ctrl.VEC_TskStart(eVEC_CMD.GetDistBetween, data);
                                        break;
                                    }
                                default: arg.nStep = 215; break;
                            }
                            break;
                        }
                    case 213:
                        switch (_CurrQueue.cmd)
                        {
                            case eCMD4MPLUS.DISTANCEBTW:
                                {
                                    switch (arg.nStatus)
                                    {                                        
                                        case eSTATE.Checked: case eSTATE.Failed:
                                            arg.nStep = 215;
                                            arg.nStatus = eSTATE.Checking;
                                            _CurrQueue._distbtw.result = 0;
                                            switch (arg.nStatus)
                                            {
                                                case eSTATE.Checked: _CurrQueue._distbtw.result = _ctrl._status.dResult; break;
                                                default: Logger.Inst.Write(CmdLogType.prdt, $"코스트 계산실패-{_CurrQueue._distbtw.goal1}, {_CurrQueue._distbtw.goal2}", CommLogType.MPlus); break;
                                            }                                            
                                            _CurrQueue.msg = _CurrQueue._distbtw.GetMsg();
                                            break;                                        
                                        default: break;
                                    }                                    
                                    break;
                                }
                            default: arg.nStep = 215; break;
                        }
                        break;
                    case 215:
                        {
                            switch (arg.nStatus)
                            {
                                case eSTATE.Checking:
                                    arg.nStep = 216;
                                    sendTsk = mplus.Send(_CurrQueue.msg);
                                    break;                               
                                default: break;
                            }
                            break;
                        }
                    case 216:
                        if (false == sendTsk.IsCompleted) break;
                        if (true == sendTsk.Result)
                        {
                            switch (arg.nStatus)
                            {                                
                                case eSTATE.Checked:
                                    arg.nStep = 220;
                                    switch (_CurrQueue.cmd)
                                    {
                                        case eCMD4MPLUS.JOB:
                                            _ctrl.VEC_SetState4Job(_ReportedJobStatus);
                                            break;
                                        default: break;
                                    }
                                    break;                                
                                default:
                                    switch (_CurrQueue.cmd)
                                    {                                        
                                        case eCMD4MPLUS.STATUS: arg.nStep = 220; break;                                        
                                        default: break;
                                    }
                                    break;
                            }                            
                        }
                        else
                        {
                            arg.nStep = 218;
                            arg.nStatus = eSTATE.Failed;
                            arg.ResetTactTime();
                        }                        
                        break;
                    case 218:
                        arg.nStatus = eSTATE.Checking;
                        arg.nStep = 215;
                        break;
                    case 220:                        
                        arg.nStep = (0 <= _QueueCnt) ? 200 : 100;
                        break;


                    case 500:
                        arg.nStatus = eSTATE.Done;
                        arg.nStep = DEF_CONST.SEQ_MAIN_FINISH;
                        break;
                    case DEF_CONST.SEQ_MAIN_FINISH:
                        ChkTactTime();
                        arg.nStep = DEF_CONST.SEQ_FINISH;
                        break;
                    case DEF_CONST.SEQ_FINISH: break;
                }
            }
            catch (Exception e)
            {
                Logger.Inst.Write(CmdLogType.Debug, $"Exception : {arg.GetID().ToString()}, {arg.nStep}\r\n{e.ToString()}\r\n");
            }
        }

        public int _QueueCnt => _Queue.Count();
        private ConcurrentQueue<QUE4MP> _Queue = new ConcurrentQueue<QUE4MP>();
        private QUE4MP _CurrQueue = new QUE4MP();
        public eCMD4MPLUS _CurrCmd => _CurrQueue.cmd;
        public eJOBST _ReportedJobStatus => _CurrQueue._jobState.state;

        public void SetErr(eERROR err)
        {                                    
            var que = new QUE4MP();
            que.SetErr(_ctrl._status, err);
            switch (err)
            {
                case eERROR.None: break;                
                default:
                    que.cmd = eCMD4MPLUS.ERROR;
                    Queue_Add(que);
                    break;
            }            
            que.cmd = eCMD4MPLUS.STATUS;
            Queue_Add(que);
        }

        MP_STATUS _status4Mplus = new MP_STATUS();
        public void SetStatus()
        {
            var que = new QUE4MP();
            que.cmd = eCMD4MPLUS.STATUS;
            que.SetStatus(_ctrl._status);
            _status4Mplus = new MP_STATUS()
            {
                 cost = que._status.cost, state = que._status.state
               , DnBatt = que._status.DnBatt, mode = que._status.mode
               , LdTemp = que._status.LdTemp, UpBatt = que._status.UpBatt
               , UpCurr = que._status.UpCurr, UpVolt = que._status.UpVolt
               , Err = new MP_ERR() { state = que._status.Err.state, code = que._status.Err.code }
               , pos = new nPOS_XYR() { x = que._status.pos.x, y = que._status.pos.y, r = que._status.pos.r }
            };
        }

        public void SetDistBtw(MP_DISTBTW btw)
        {
            var que = new QUE4MP();
            que.cmd = eCMD4MPLUS.DISTANCEBTW;
            que._distbtw = new MP_DISTBTW() { goal1 = btw.goal1, goal2 = btw.goal2, result = btw.result };
            Queue_Add(que);            
        }

        public async Task<bool> SendSatate()
        {
            MPlus mplus = _ctrl.mplus;
            if (null == mplus) return false;
            if (false == mplus._Connected) return false;
            var rtn = await mplus.Send(_status4Mplus.GetMsg());            
            return rtn;
        }

        public void SetJobState()
        {
            var que = new QUE4MP();
            que.cmd = eCMD4MPLUS.JOB;
            que.SetJobState(_ctrl.status);
            Queue_Add(que);
        }

        public void SetAskInfoOfGoals(eJOBTYPE type)
        {
            var que = new QUE4MP();
            que.cmd = type == eJOBTYPE.LOADING ? eCMD4MPLUS.GOAL_LD : eCMD4MPLUS.GOAL_UL;
            que._askGoal = new MP_GOAL();
            que._askGoal.cmd = type;
            Queue_Add(que);
        }

        public void SetManlCarrierChange(eMNL_INST cmd)
        {
            var que = new QUE4MP();
            que.cmd = cmd == eMNL_INST.INSTALL ? eCMD4MPLUS.MANUAL_MAG_INST : eCMD4MPLUS.MANUAL_MAG_INST;
            que._trayChg = new MNL_CARRIRE_CHANGE();
            que._trayChg.Set(_ctrl._status, cmd);
            Queue_Add(que);
        }

        public void SetRemoteReply(MP_REMOTE remote)
        {
            var que = new QUE4MP();
            que.cmd = eCMD4MPLUS.REMOTE;
            que.remote = new MP_REMOTE() { mode = remote.mode, reply = remote.reply };
            Queue_Add(que);
        }

        private void Queue_Add(QUE4MP arg)
        {
            switch (arg.cmd)
            {
                case eCMD4MPLUS.STATUS:
                    _Queue.Enqueue(new QUE4MP()
                    {
                        cmd = arg.cmd
                      , _status = new MP_STATUS()
                      {
                          cost = arg._status.cost, state = arg._status.state
                        , DnBatt = arg._status.DnBatt, mode = arg._status.mode
                        , LdTemp = arg._status.LdTemp, UpBatt = arg._status.UpBatt
                        , UpCurr = arg._status.UpCurr, UpVolt = arg._status.UpVolt
                        , Err = new MP_ERR() { state = arg._status.Err.state, code = arg._status.Err.code }
                        , pos = new nPOS_XYR() { x = arg._status.pos.x, y = arg._status.pos.y, r = arg._status.pos.r }
                      }
                    });
                    break;
                case eCMD4MPLUS.JOB:
                    _Queue.Enqueue(new QUE4MP()
                    {
                          cmd = arg.cmd
                        , _jobState = new MP_JOBST() { cmdID = arg._jobState.cmdID, state = arg._jobState.state }
                    });
                    break;
                case eCMD4MPLUS.DISTANCEBTW:
                    _Queue.Enqueue(new QUE4MP()
                    {
                        cmd = arg.cmd
                        , _distbtw = new MP_DISTBTW() { cmd = arg._distbtw.cmd, goal1 = arg._distbtw.goal1, goal2 = arg._distbtw.goal2, result = arg._distbtw.result }
                    });
                    break;
                case eCMD4MPLUS.ERROR:
                    _Queue.Enqueue(new QUE4MP()
                    {
                        cmd = arg.cmd
                      , _err = new MP_ERR() { code = arg._err.code, state = arg._err.state }
                    });
                    break;
                case eCMD4MPLUS.MANUAL_MAG_INST: case eCMD4MPLUS.MANUAL_MAG_UNINST:
                    _Queue.Enqueue(new QUE4MP()
                    {
                        cmd = arg.cmd
                      , _trayChg = new MNL_CARRIRE_CHANGE() { cmd = arg._trayChg.cmd, CarrierID = arg._trayChg.CarrierID, nPartID = arg._trayChg.nPartID, nSize = arg._trayChg.nSize }
                    });
                    break;
                case eCMD4MPLUS.GOAL_LD: case eCMD4MPLUS.GOAL_UL:
                    _Queue.Enqueue(new QUE4MP()
                    {
                        cmd = arg.cmd
                      , _askGoal = new MP_GOAL() { cmd = arg._askGoal.cmd }
                    });
                    break;
                case eCMD4MPLUS.REMOTE:
                    _Queue.Enqueue(new QUE4MP()
                    {
                        cmd = arg.cmd
                      , remote = new MP_REMOTE() { mode = arg.remote.mode, reply = arg.remote.reply }
                    });
                    break;
                default: break;
            }
        }

        private bool Queue_SetCurr()
        {
            if (0 < _QueueCnt)
            {
                var rtn = _Queue.TryDequeue(out QUE4MP que);
                if (false == rtn) return false;
                _CurrQueue.cmd = que.cmd;
                switch (_CurrQueue.cmd)
                {
                    case eCMD4MPLUS.STATUS:
                        _CurrQueue._status = new MP_STATUS()
                        {
                              cost = que._status.cost, state = que._status.state
                            , DnBatt = que._status.DnBatt, mode = que._status.mode
                            , LdTemp = que._status.LdTemp, UpBatt = que._status.UpBatt
                            , UpCurr = que._status.UpCurr, UpVolt = que._status.UpVolt
                            , Err = new MP_ERR() { state = que._status.Err.state, code = que._status.Err.code }
                            , pos = new nPOS_XYR() { x = que._status.pos.x, y = que._status.pos.y, r = que._status.pos.r }
                        };
                        _CurrQueue.msg = _CurrQueue._status.GetMsg();
                        break;
                    case eCMD4MPLUS.JOB:
                        _CurrQueue._jobState = new MP_JOBST() { cmdID = que._jobState.cmdID, state = que._jobState.state };
                        _CurrQueue.msg = _CurrQueue._jobState.GetMsg();
                        break;
                    case eCMD4MPLUS.DISTANCEBTW:
                        _CurrQueue._distbtw = new MP_DISTBTW() { cmd = que._distbtw.cmd, goal1 = que._distbtw.goal1, goal2 = que._distbtw.goal2, result = que._distbtw.result };
                        _CurrQueue.msg = _CurrQueue._distbtw.GetMsg();
                        break;
                    case eCMD4MPLUS.ERROR:
                        _CurrQueue._err = new MP_ERR() { code = que._err.code, state = que._err.state };
                        _CurrQueue.msg = _CurrQueue._err.GetMsg();
                        break;
                    case eCMD4MPLUS.MANUAL_MAG_INST:
                    case eCMD4MPLUS.MANUAL_MAG_UNINST:
                        _CurrQueue._trayChg = new MNL_CARRIRE_CHANGE() { cmd = que._trayChg.cmd, CarrierID = que._trayChg.CarrierID, nPartID = que._trayChg.nPartID, nSize = que._trayChg.nSize };
                        _CurrQueue.msg = _CurrQueue._trayChg.GetMsg();
                        break;
                    case eCMD4MPLUS.GOAL_LD:
                    case eCMD4MPLUS.GOAL_UL:
                        _CurrQueue._askGoal = new MP_GOAL() { cmd = que._askGoal.cmd };
                        _CurrQueue.msg = _CurrQueue._askGoal.GetMsg();
                        break;
                    case eCMD4MPLUS.REMOTE:
                        _CurrQueue.remote = new MP_REMOTE() { mode = que.remote.mode, reply = que.remote.reply };
                        _CurrQueue.msg = _CurrQueue.remote.GetMsg();
                        break;
                    default: break;
                }
            }
            return true;
        }

    }
}
