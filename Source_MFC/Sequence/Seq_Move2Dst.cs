using Source_MFC.Global;
using Source_MFC.Sequence.MainTasks;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Sequence
{
    public class Seq_Move2Dst : _SEQBASE
    {
        public Seq_Move2Dst(MainCtrl main)
        {
            _ctrl = main;
            arg = new TASKARG(eSEQLIST.Move2Dst);
        }

        override public void Run()
        {
            try
            {
                var job = _ctrl._status.Order;                
                var seqMode = _Data.Inst.sys.cfg.fac.seqMode;
                var vecState = _ctrl.VEC_GetCurrState();
                switch (arg.nStep)
                {
                    case DEF_CONST.SEQ_INIT:
                        arg.nStep = 10;                       
                        break;
                    case 10:
                        _ctrl.Job_SetState(eJOBST.Enroute);
                        arg.nStatus = eSTATE.Working;
                        Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: 목적지[to:{job.goal.label}] 이동을 시작합니다. [ID:{job.cmdID}]");
                        switch (seqMode)
                        {
                            case eSCENARIOMODE.PC: arg.nStep = 100; break;                         
                            default: arg.nStep = 105; break;
                        }                        
                        break;
                    case 100:
                        ResetTime();
                        arg.nStep = 105;                        
                        _ctrl.VEC_SendCmd(eVEC_CMD.Go2Goal, new SENDARG() { goal_1st = job.goal.name });
                        break;
                    case 105:          
                        // 목표골에 도착했는지 확인
                        var chk = IsTargetAreaAlready(job.type, job.goal.name);
                        if ( true == chk )
                        {
                            arg.tSen.Check();
                            if (arg.tDly.IsOver(_Data.Inst.sys.cfg.pio.nSenDelay))
                            {
                                Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: 목적지[to:{job.goal.label}]에 도착하였습니다. [ID:{job.cmdID}, 이동시간:{arg.tSen._currBySec} sec]");
                                arg.nStep = 110;                                
                            }
                        }
                        else
                        {
                            arg.tDly.Reset();
                            switch (_ctrl._EQPStatus)
                            {
                                case eEQPSATUS.Run:
                                    switch (vecState.state.st)
                                    {
                                        case HW.MobileRobot.LD.eVECSTATE.ARRIVED_AT:
                                        case HW.MobileRobot.LD.eVECSTATE.STOPPED:                                        
                                        case HW.MobileRobot.LD.eVECSTATE.FAILED_GOING_TO:
                                        case HW.MobileRobot.LD.eVECSTATE.ESTOP_PRESSED:
                                        case HW.MobileRobot.LD.eVECSTATE.ESTOP_RELIEVED:
                                        case HW.MobileRobot.LD.eVECSTATE.CANNOT_FIND_PATH:
                                        case HW.MobileRobot.LD.eVECSTATE.NO_ENTER:
                                            SetErr(eERROR.VEC_Move2Failed, 10);
                                            break;
                                        case HW.MobileRobot.LD.eVECSTATE.PAUSING:                                            
                                        case HW.MobileRobot.LD.eVECSTATE.PAUSE_CANCELLED:                                            
                                        case HW.MobileRobot.LD.eVECSTATE.PAUSE_INTERRUPTED:                                            
                                        case HW.MobileRobot.LD.eVECSTATE.PARKING:                                            
                                        case HW.MobileRobot.LD.eVECSTATE.PARKED:
                                            SetErr(eERROR.VEC_Unnormal, 10);
                                            break;
                                        default: break;
                                    }
                                    break;
                                case eEQPSATUS.Stopping:
                                    switch (arg.nStatus)
                                    {
                                        case eSTATE.Working:
                                            arg.StopTrg();
                                            _ctrl.VEC_SendCmd(eVEC_CMD.Stop, new SENDARG());
                                            break;                                        
                                        case eSTATE.Stopping:
                                            switch (vecState.state.st)
                                            {
                                                case HW.MobileRobot.LD.eVECSTATE.STOPPED:
                                                    arg.nStep = 10;
                                                    arg.bStop = true;
                                                    arg.nStatus = eSTATE.Stopped;                                                    
                                                    break;
                                            }
                                            break;
                                        case eSTATE.Stopped: break;                                        
                                        default: break;
                                    }
                                    break;
                                case eEQPSATUS.Error:
                                case eEQPSATUS.EMG:
                                    arg.bStop = true;
                                    break;
                                default: break;
                            }
                        }                        
                        break;
                    case 110:
                        arg.nStep = 500;                        
                        _ctrl.Job_SetState(eJOBST.Arrived);
                        break;
                    case 500:
                        arg.nStatus = eSTATE.Done;
                        arg.nStep = DEF_CONST.SEQ_MAIN_FINISH;
                        break;
                    case DEF_CONST.SEQ_MAIN_FINISH:
                        arg.nStep = DEF_CONST.SEQ_FINISH;
                        break;
                    case DEF_CONST.SEQ_FINISH: ChkSeqStop(); break;
                }
            }
            catch (Exception e)
            {
                Logger.Inst.Write(CmdLogType.Debug, $"Exception : {arg.GetID().ToString()}, {arg.nStep}\r\n{e.ToString()}\r\n");
            }
        }

        private bool IsTargetAreaAlready(eJOBTYPE type, string goalname, int nTolarance = 100)
        {
            var rtn = false;
            eGOALTYPE goaltype = eGOALTYPE.Standby;
            switch (type)
            {                
                case eJOBTYPE.LOADING: goaltype = eGOALTYPE.Dropoff; break;
                case eJOBTYPE.UNLOADING: goaltype = eGOALTYPE.Pickup; break;                
                case eJOBTYPE.CAHRGE: goaltype = eGOALTYPE.Charge; break;
                default: break;
            }
            var stopped = _ctrl.VEC_ChkArrivedAtGoal();
            var posOk = _ctrl.VEC_IN_POSOK();
            if (true == stopped && true == posOk)
            {
                var goal = _Data.Inst.sys.goal.Get(goaltype, goalname, eSRCHGOALBY.Map);
                if (null != goal)
                {
                    var vecStatus = _ctrl.VEC_GetCurrState();
                    var nMaxX = goal.pos.x + nTolarance; var nMaxY = goal.pos.y + nTolarance;
                    var nMinX = goal.pos.x - nTolarance; var nMinY = goal.pos.y - nTolarance;
                    if (nMaxX >= vecStatus.pos.x && nMaxY >= vecStatus.pos.y
                     && nMinX <= vecStatus.pos.x && nMinY <= vecStatus.pos.y)
                    {
                        rtn = true;
                    }
                }
            }                
            return rtn;
        }
    }
}
