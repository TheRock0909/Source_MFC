using EzVehicle;
using Source_MFC.Global;
using Source_MFC.HW;
using Source_MFC.HW.IO;
using Source_MFC.HW.M_;
using Source_MFC.HW.MobileRobot.LD;
using Source_MFC.Sequence;
using Source_MFC.Sequence.MainTasks;
using Source_MFC.Sequence.SubTasks;
using Source_MFC.Tasks;
using Source_MFC.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Source_MFC
{
    public class MainCtrl
    {
        private _Data _data = _Data.Inst;
        public MainWindow frmMain;
        private SYS _sys;
        public STATUS _status;
        public STATUS status => _status;
        public JOB _Order;

        private _IO io;
        private IVehicle vec;
        public MPlus mplus;
        private Logger _log = Logger.Inst;
        public event EventHandler<WriteLogArgs> Evt_WriteLog;
        public event EventHandler<(eDEV dev, bool bConnection)> Evt_UpdateConnection;

        public event EventHandler<(eDATAEXCHANGE dir, eUID4VM id)> Evt_MainWin_DataExchange;
        public event EventHandler<(eDATAEXCHANGE dir, eUID4VM id)> Evt_Dash_Moni_DataExchange;
        public event EventHandler<(eDATAEXCHANGE dir, eUID4VM id)> Evt_Dash_Manual_DataExchange;
        public event EventHandler<(eDATAEXCHANGE dir, eUID4VM id)> Evt_Sys_Goal_Item_DataExchange;

        public event EventHandler<(eDATAEXCHANGE dir, eUID4VM id)> Evt_Sys_IO_DataExchange;
        public event EventHandler<(eEQPSATUS state, LAMPINFO data)> Evt_Sys_Lamp_DataExchange;
        public event EventHandler<GOALINFO> Evt_Sys_Goal_List_DataExchange;
        public event EventHandler<eDATAEXCHANGE> Evt_SeqMoni_DataExchange;
        public event EventHandler<eDATAEXCHANGE> Evt_TskMoni_DataExchange;

        public event EventHandler<(eDATAEXCHANGE dir, PIO data)> Evt_Sys_PIO_Item_DataExchange;
        public event EventHandler<(eDATAEXCHANGE dir, FAC data)> Evt_Sys_FAC_Item_DataExchange;

        public Thread thd_BackProc;
        private ConcurrentQueue<WriteLogArgs> m_quLogMsg = new ConcurrentQueue<WriteLogArgs>();
        public string _EQPName => $"{_Data.Inst.sys.cfg.fac.eqpType}-{_Data.Inst.sys.cfg.fac.eqpName}";

        MsgBox msgBox = MsgBox.Inst;
        private _SEQBASE[] _Seqs;
        private List<_SEQBASE> _lstSeqs = new List<_SEQBASE>();
        private _TSKBASE[] _Tsks;
        private List<_TSKBASE> _lstTsks = new List<_TSKBASE>();
        
        public MainCtrl(MainWindow main)
        {
            frmMain = main;
            Logger.Inst.MakeSrcHdl($"{_EQPName}");
            _sys = _Data.Inst.sys;
            _status = _Data.Inst.status;
            _Order = _status.Order;
            _log.Evt_WriteLog += On_WriteLogMsg;
            thd_BackProc = new Thread(Trd_Background)
            {
                IsBackground = true
            };
            _sys.io._bDirectIO = true;
            _status.bSimul = true;
        }

        public void _Finalize()
        {
            Sw_Final();
            Hw_Final();
            StopBackProc();
        }

        public void View_Init()
        {
            _EQPStatus = !_status.bSimul ? eEQPSATUS.Init : eEQPSATUS.Stop;
            UpdateUserData(new USER());
            DoingDataExchage(eVIWER.IO, eDATAEXCHANGE.Model2View, eUID4VM.IO_SetList);
            DoingDataExchage(eVIWER.TowerLamp, eDATAEXCHANGE.Model2View);
            DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.Model2View, eUID4VM.GOAL_LIST);
            DoingDataExchage(eVIWER.PIO, eDATAEXCHANGE.Model2View);
            DoingDataExchage(eVIWER.FAC, eDATAEXCHANGE.Model2View);

            DoingDataExchage(eVIWER.Manual, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_ALL);
            // 중요 모니터를 맨마지막에 업데이트해줘야 함!!!
            DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_ALL);
            thd_BackProc.Start();
            _log.Write(CmdLogType.prdt, $"Application을 시작합니다. [{_EQPName}:{_status.swVer}]");
        }

        public bool Hw_Init()
        {
            var rtn = true;
            _log.Write(CmdLogType.prdt, $"H/W 초기화를 시작합니다. [{_EQPName}:{_status.swVer}]");
            try
            {
                _status.devsCont.Init();
                foreach (eDEV dev in Enum.GetValues(typeof(eDEV)))
                {
                    Evt_UpdateConnection?.Invoke(this, (dev, false));
                }
                io = new Crevis(this);
                var crv = io as Crevis;
                crv.Evt_Connected += On_Connection;
                crv.Evt_Status += On_IO_UpdateStatus;

                vec = new LD(_sys.cfg.fac.VecIP);
                vec.Evt_Connection += On_VecConnection;
                vec.Evt_UpdateData += On_VecUpdateData;

                mplus = new MPlus();
                mplus.Evt_Connection += On_MplusConnection;
                mplus.Evt_RecvData += On_MplusRecvData;

                if (false == _status.bSimul)
                {
                    rtn = crv.Open();
                    rtn = vec.Open();
                    mplus.Open(_sys.cfg.fac.mplusIP, _sys.cfg.fac.mplusPort);
                }
            }
            catch (Exception e)
            {
                rtn = false;
                _log.Write(CmdLogType.prdt, $"H/W 초기화에 실패하였습니다.[{_EQPName}:{_status.swVer}]-{e.ToString()}");
            }

            if (true == rtn)
            {
                _log.Write(CmdLogType.prdt, $"H/W 초기화를 완료하였습니다. [{_EQPName}:{_status.swVer}]");
            }
            return rtn;
        }

        public bool Hw_Final()
        {
            var rtn = true;
            _log.Write(CmdLogType.prdt, $"H/W 종료를 시작합니다. [{_EQPName}:{_status.swVer}]");
            try
            {
                var crv = io as Crevis;
                if (null != crv)
                {
                    crv.Evt_Connected -= On_Connection;
                    crv.Evt_Status -= On_IO_UpdateStatus;
                    crv.Close();
                }

                if (null != vec)
                {
                    vec.Evt_Connection -= On_VecConnection;
                    vec.Evt_UpdateData -= On_VecUpdateData;
                    vec.Close();
                    vec = null;
                }

                if (null != mplus)
                {
                    mplus.Evt_Connection -= On_MplusConnection;
                    mplus.Close();
                    mplus = null;
                }
            }
            catch (Exception e)
            {
                rtn = false;
                _log.Write(CmdLogType.prdt, $"H/W 종료를 실패하였습니다.[{_EQPName}:{_status.swVer}]-{e.ToString()}");
            }

            if (true == rtn)
            {
                _log.Write(CmdLogType.prdt, $"H/W 종료를 완료하였습니다. [{_EQPName}:{_status.swVer}]");
            }
            return rtn;
        }

        public bool Sw_Init()
        {
            var rtn = true;
            _log.Write(CmdLogType.prdt, $"S/W 초기화를 시작합니다. [{_EQPName}:{_status.swVer}]");
            try
            {
                Seqs_Init();
                Tsk_Init();
            }
            catch (Exception e)
            {
                rtn = false;
                _log.Write(CmdLogType.prdt, $"S/W 종료를 실패하였습니다.[{_EQPName}:{_status.swVer}]-{e.ToString()}");
            }

            if (true == rtn)
            {
                _log.Write(CmdLogType.prdt, $"S/W 초기화를 완료하였습니다. [{_EQPName}:{_status.swVer}]");
            }
            return rtn;
        }

        public bool Sw_Final()
        {
            var rtn = true;
            _lstSeqs.Clear();
            _lstSeqs = null;
            _lstTsks.Clear();
            _lstTsks = null;
            return rtn;
        }

        private void Seqs_Init()
        {
            _log.Write(CmdLogType.prdt, $"Sequence 의 Tasks 초기화를 시작합니다. [{_EQPName}:{_status.swVer}]");
            _lstSeqs.Clear();
            _Seqs = new _SEQBASE[(int)eSEQLIST.MAX_SEQ];
            foreach (eSEQLIST seq in Enum.GetValues(typeof(eSEQLIST)))
            {
                if (eSEQLIST.MAX_SEQ == seq) continue;
                _Seqs[(int)seq] = new _SEQBASE();
            }

            _Seqs[(int)eSEQLIST.Main] = new Seq_Main(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.Main]);
            _Seqs[(int)eSEQLIST.EscapeEQP] = new Seq_EscapeEQP(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.EscapeEQP]);
            _Seqs[(int)eSEQLIST.Move2Dst] = new Seq_Move2Dst(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.Move2Dst]);
            _Seqs[(int)eSEQLIST.PIO] = new Seq_PIO(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.PIO]);
            _Seqs[(int)eSEQLIST.Pick] = new Seq_Pick(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.Pick]);
            _Seqs[(int)eSEQLIST.Drop] = new Seq_Drop(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.Drop]);
            Evt_SeqMoni_DataExchange?.Invoke(_status, eDATAEXCHANGE.Model2View);
        }

        private void Tsk_Init()
        {
            _log.Write(CmdLogType.prdt, $"Sequence 의 SubTasks 초기화를 시작합니다. [{_EQPName}:{_status.swVer}]");
            _lstTsks.Clear();
            _Tsks = new _TSKBASE[(int)eTASKLIST.MAX_SUB_SEQ];
            foreach (eTASKLIST seq in Enum.GetValues(typeof(eTASKLIST)))
            {
                if (eTASKLIST.MAX_SUB_SEQ == seq) continue;
                _Tsks[(int)seq] = new _TSKBASE();
            }

            _Tsks[(int)eTASKLIST.SWITCH] = new Tsk_Switch(this);
            _lstTsks.Add(_Tsks[(int)eTASKLIST.SWITCH]);
            _Tsks[(int)eTASKLIST.SWITCH].WorkTrg();
            _Tsks[(int)eTASKLIST.MPLUSCOMM] = new Tsk_MPlusComm(this);
            _lstTsks.Add(_Tsks[(int)eTASKLIST.MPLUSCOMM]);
            _Tsks[(int)eTASKLIST.VECCMD] = new Tsk_VecCmd(this);
            _lstTsks.Add(_Tsks[(int)eTASKLIST.VECCMD]);
            Evt_TskMoni_DataExchange?.Invoke(_status, eDATAEXCHANGE.Model2View);
        }

        public eEQPSATUS _EQPStatus
        {
            set {
                if (_status.eqpState != value)
                {
                    _log.Write(CmdLogType.prdt, $"설비 상태를 {_status.eqpState.ToString()}에서 {value.ToString()}로 변경합니다.");
                }
                _status.eqpState = value;
                DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_EqpState);
            }
            get { return _status.eqpState; }
        }

        private void On_MplusConnection(object sender, bool Connection)
        {
            Evt_UpdateConnection?.Invoke(this, (eDEV.MPlus, Connection));
        }

        public void On_MplusRecvData(object sender, (string msg, bool bManual, int optAry) e)
        {
            Logger.Inst.Write(CmdLogType.Gem, $"R : {e.msg}");
            var rcvdata = new QUE4MP();
            var cmd = rcvdata.GetCmd(e.msg);
            var tskMplus = Tsk_Get(eTASKLIST.MPLUSCOMM) as Tsk_MPlusComm;
            switch (cmd)
            {
                case eCMD4MPLUS.SRC:    case eCMD4MPLUS.DST:
                case eCMD4MPLUS.STANDBY:case eCMD4MPLUS.CHARGE:
                    {
                        var need2Assign = false;
                        var job = new JOB();
                        rcvdata.SetJob(e.msg, _sys.goal, ref job);
                        switch (_sys.cfg.fac.seqMode)
                        {
                            case eSCENARIOMODE.PC:
                                switch (_EQPStatus)
                                {
                                    case eEQPSATUS.Stop: if (true == e.bManual) { need2Assign = true; } break;
                                    case eEQPSATUS.Idle: need2Assign = true; break;
                                    default: break;
                                }
                                break;
                            case eSCENARIOMODE.PLC:
                                switch (cmd)
                                {
                                    case eCMD4MPLUS.SRC: case eCMD4MPLUS.DST: need2Assign = true; break;
                                    default: break;
                                }
                                break;
                        }

                        if (true == need2Assign)
                        {
                            _status.Order.Set(job);
                            if (true == e.bManual)
                            {
                                var opt = new Any64();
                                opt.INT64 = e.optAry;
                                _status.bManualJob = true;
                                _status.Order.opt.bSkipGo2Dest = opt[0];
                                _status.Order.opt.bSkipPIO = opt[1];
                                _status.Order.opt.bSkipTransfer = opt[2];
                            }
                            else
                            {
                                _status.bManualJob = false;
                                _status.Order.opt.bSkipGo2Dest = false;
                                _status.Order.opt.bSkipTransfer = false;
                                switch (_status.Order.pio)
                                {
                                    case ePIOTYPE.NOUSE: _status.Order.opt.bSkipPIO = true; break;
                                    default: _status.Order.opt.bSkipPIO = false; break;
                                }
                            }
                            Job_SetState(eJOBST.Assign);
                            foreach (var item in _lstSeqs)
                            {
                                switch (item.arg.GetID())
                                {
                                    case eSEQLIST.Main:
                                    case eSEQLIST.MAX_SEQ: break;
                                    default: item.arg.Init(); break;
                                }
                            }

                            if ( true == _status.bManualJob)
                            {
                                Start(CHANGEMODEBY.MANUAL);
                            }
                        }
                        else
                        {
                            Logger.Inst.Write(CmdLogType.prdt, $"MFC를 가동상태 변경해 주십시오. [골이름:{rcvdata.order.Job.goal.label}, 상태:{_EQPStatus}, 가동모드:{_status.bDebug.ToString()}]");
                        }              
                        break;
                    }
                case eCMD4MPLUS.REMOTE:
                    {
                        var remote = MP_REMOTE.Parse(e.msg);
                        var rtn = Mplus_RemoteCmd(remote.mode);
                        if (true == rtn)
                        {
                            remote.SetReply();
                            var mplus = tskMplus as Tsk_MPlusComm;
                            mplus.SetRemoteReply(remote);
                        }
                        break;
                    }
                case eCMD4MPLUS.DISTANCEBTW:
                    {
                        var disbtw = MP_DISTBTW.Parse(e.msg);
                        tskMplus.SetDistBtw(disbtw);
                        break;
                    }
                case eCMD4MPLUS.JOB:
                case eCMD4MPLUS.ERROR:
                case eCMD4MPLUS.MANUAL_MAG_INST:
                case eCMD4MPLUS.MANUAL_MAG_UNINST:
                    switch (tskMplus.arg.nStatus)
                    {
                        case eSTATE.Checking: tskMplus.arg.nStatus = eSTATE.Checked; break;
                        default: break;
                    }
                    break;
                case eCMD4MPLUS.GOAL_LD:
                case eCMD4MPLUS.GOAL_UL:
                    {
                        switch (tskMplus.arg.nStatus)
                        {
                            case eSTATE.Checking:
                                bool need2Update = false;
                                var lstGoals = MP_GOAL.Parse(e.msg);
                                foreach (GOALITEM item in lstGoals.lstGoals)
                                {
                                    var goal = _sys.goal.Get(item.type, item.name, eSRCHGOALBY.Map);
                                    if (null != goal)
                                    {
                                        need2Update = true;
                                        var addGoal = new GOALITEM() { name = item.name, type = item.type, label = item.name, hostName = item.name, line = eLINE._23 };
                                        _sys.goal.Add(addGoal);
                                    }
                                }
                                if (true == need2Update)
                                {
                                    _Data.Inst.Save();
                                }
                                tskMplus.arg.nStatus = eSTATE.Checked;
                                break;
                            default: break;
                        }
                        break;
                    }
                case eCMD4MPLUS.MANUAL:
                case eCMD4MPLUS.STATUS: break;
                default: break;
            }

            
        }

        private void On_VecConnection(object sender, bool Connection)
        {
            Evt_UpdateConnection?.Invoke(this, (eDEV.Vehicle, Connection));
        }

        public string MNL_GetJobOrderStr(MP_DISTBTW data, bool bMadeByAuto = false)
        {
            var job = string.Empty;
            var type = data.cmd.ToEnum<eJOBTYPE>();
            switch (type)
            {
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                    job = $"{(eJOBTYPE.LOADING == type ? "DST" : "SRC")}_{DateTime.Now:yyyy-MM-dd HH:mm:ss};";
                    job += $"{data.goal1};";
                    job += $"TEST1234;1;1;A;FALSE;{data.goal2};";
                    break;
                case eJOBTYPE.STANDBY: case eJOBTYPE.CAHRGE:
                    switch (_sys.cfg.fac.seqMode)
                    {
                        case eSCENARIOMODE.PC:
                            job = $"{type};{data.goal1};";
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
            return job;
        }


        private void On_VecUpdateData(object sender, JCVT_VEC data)
        {
            switch (data.typeName)
            {
                case "LD_STATUS":
                    {
                        JCVT_VEC.Get(data.json, out LD_STATUS rtn);
                        switch (rtn.cmd)
                        {
                            case eVECST_CMD_TYPE.TRANSGO:
                            case eVECST_CMD_TYPE.SJUNLOADING: case eVECST_CMD_TYPE.SJLOADING:
                                {
                                    switch (_sys.cfg.fac.seqMode)
                                    {
                                        case eSCENARIOMODE.PLC:
                                            var job = MNL_GetJobOrderStr(rtn.dist);
                                            if (string.Empty != job)
                                            {
                                                On_MplusRecvData(this, (job, false, 0));
                                            }
                                            break;
                                        default: break;
                                    }
                                    break;
                                }
                            case eVECST_CMD_TYPE.AFTERSTART:
                            case eVECST_CMD_TYPE.KCUNLOADING: case eVECST_CMD_TYPE.KCLOADING:
                            case eVECST_CMD_TYPE.JRUNLOADING: case eVECST_CMD_TYPE.JRLOADING:
                                {

                                    break;
                                }
                            default:
                                {
                                    _status.vecState.state = rtn.state.st;
                                    switch (_status.vecState.state)
                                    {
                                        case eVECSTATE.AFTER_GOAL:
                                        case eVECSTATE.ARRIVED_AT:
                                        case eVECSTATE.GOING_TO:
                                        case eVECSTATE.FAILED_GOING_TO:
                                            _status.vecState.dst = rtn.state.dst;
                                            break;
                                        default: break;
                                    }
                                    _status.vecState.localize = (int)rtn.local;
                                    _status.vecState.temp = (int)rtn.temp;
                                    _status.vecState.pos = new nPOS_XYR() { x = rtn.pos.x, y = rtn.pos.y, r = rtn.pos.r };
                                    _status.vecState.msg = rtn.state.msg;
                                    _status.vecState.subMsg = rtn.state.subMsg;
                                    switch (_sys.cfg.fac.seqMode)
                                    {
                                        case eSCENARIOMODE.PC:
                                            var tskVec = Tsk_Get(eTASKLIST.VECCMD) as Tsk_VecCmd;
                                            switch (tskVec.currCmd)
                                            {
                                                case eVEC_CMD.GetDistBetween:
                                                case eVEC_CMD.GetDistFromHere:
                                                    switch (tskVec.arg.nStatus)
                                                    {
                                                        case eSTATE.Checking:
                                                            var chk = rtn.dist.cmd.ToEnum<eSTATE>();
                                                            tskVec.arg.nStatus = eSTATE.Done == chk ? eSTATE.Checked : eSTATE.Failed;
                                                            _status.dResult = rtn.dist.result;
                                                            break;
                                                        default: break;
                                                    }
                                                    break;
                                            }

                                            var tskMpComm = Tsk_Get(eTASKLIST.MPLUSCOMM) as Tsk_MPlusComm;
                                            switch (tskMpComm._CurrCmd)
                                            {
                                                case eCMD4MPLUS.DISTANCEBTW:
                                                    switch (tskMpComm.arg.nStatus)
                                                    {
                                                        case eSTATE.Checking:
                                                            tskMpComm.arg.nStatus = tskVec.arg.nStatus;
                                                            break;
                                                        default: break;
                                                    }
                                                    break;
                                                default: break;
                                            }
                                            break;
                                        default: break;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                default: break;
            }
        }

        public void UpdateUserData(USER userData)
        {
            Evt_MainWin_DataExchange?.Invoke(userData, (eDATAEXCHANGE.Model2View, eUID4VM.MAINWIN_User));
        }

        private void On_Connection(object sender, (eDEV dev, bool connection) e)
        {
            switch (e.dev)
            {
                case eDEV.IO:
                    if (_status.devsCont.io != (e.connection ? eCOMSTATUS.CONNECTED : eCOMSTATUS.DISCONNECTED))
                    {
                        Logger.Inst.Write(CmdLogType.prdt, $"{e.dev.ToString()} 연결을 {(true == e.connection ? "성공" : "실패")}하였습니다.");
                    }
                    _status.devsCont.io = e.connection ? eCOMSTATUS.CONNECTED : eCOMSTATUS.DISCONNECTED;
                    break;
                case eDEV.MPlus:
                    if (_status.devsCont.mPlus != (e.connection ? eCOMSTATUS.CONNECTED : eCOMSTATUS.DISCONNECTED))
                    {
                        Logger.Inst.Write(CmdLogType.prdt, $"{e.dev.ToString()} 연결을 {(true == e.connection ? "성공" : "실패")}하였습니다.");
                    }
                    _status.devsCont.mPlus = e.connection ? eCOMSTATUS.CONNECTED : eCOMSTATUS.DISCONNECTED;
                    break;
                case eDEV.Vehicle:
                    if (_status.devsCont.Vec != (e.connection ? eCOMSTATUS.CONNECTED : eCOMSTATUS.DISCONNECTED))
                    {
                        Logger.Inst.Write(CmdLogType.prdt, $"{e.dev.ToString()} 연결을 {(true == e.connection ? "성공" : "실패")}하였습니다.");
                    }
                    _status.devsCont.Vec = e.connection ? eCOMSTATUS.CONNECTED : eCOMSTATUS.DISCONNECTED;
                    break;
                default: break;
            }
            Evt_UpdateConnection?.Invoke(this, (e.dev, e.connection));
        }


        private void On_WriteLogMsg(object sender, WriteLogArgs e)
        {
            m_quLogMsg.Enqueue(e);
        }

        private bool isThRun = true;
        private TIMEARG backProcScan = new TIMEARG();
        public long _backProcScan => backProcScan.nCurr;
        private void Trd_Background()
        {
            backProcScan.Reset();
            while (isThRun)
            {
                Proc4Tsks();
                Proc4Viwer();
                Proc4Log();
                Thread.Sleep(1);
                backProcScan.Check();
            }
            isThRun = false;
        }

        private async void StopBackProc()
        {
            while (false == m_quLogMsg.IsEmpty)
            {
                await Task.Delay(10);
            }
            isThRun = false;
        }

        private void Proc4Log()
        {
            if (m_quLogMsg.Count > 0)
            {
                WriteLogArgs temp = new WriteLogArgs();
                if (!m_quLogMsg.TryDequeue(out temp))
                {
                    Thread.Sleep(1);
                    return;
                }
                if (temp == null) { return; }

                if (temp.msg.IndexOf("\r") < 0)
                {
                    temp.msg += "\r";
                }
                if (temp.msg.IndexOf("\n") < 0)
                {
                    temp.msg += "\n";
                }

                try
                {
                    Evt_WriteLog?.Invoke(this, temp);
                }
                catch (Exception e)
                {
                    Debug.Write(false, $"{e.ToString()}\r\n");
                }
            }
        }

        TIMEARG tmr4fast = new TIMEARG() { nDelay = 50 };
        TIMEARG tmr4middle = new TIMEARG() { nDelay = 500 };
        TIMEARG tmr4slow = new TIMEARG() { nDelay = 1 * 1000 };
        TASKARG testArg = new TASKARG(eSEQLIST.MAX_SEQ);
        private void Proc4Viwer()
        {
            if (true == tmr4fast.IsOver())
            {
                switch (CurrView)
                {
                    case eVIWER.IO: break;
                    case eVIWER.Monitor: break;
                    default: break;
                }
            }

            if (true == tmr4middle.IsOver())
            {
                switch (CurrView)
                {
                    case eVIWER.IO: break;
                    case eVIWER.Monitor: break;
                    default: break;
                }
            }

            if (true == tmr4slow.IsOver())
            {
                switch (CurrView)
                {
                    case eVIWER.IO: break;
                    case eVIWER.Monitor: break;
                    default: break;
                }
            }
        }

        private void Proc4Tsks()
        {
            if (false == _status.bLoaded || null == _lstTsks) return;
            foreach (var item in _lstTsks)
            {
                item.Run();
            }
        }

        public eVIWER CurrView = eVIWER.Manual;
        public bool DoingDataExchage(eVIWER viwer, eDATAEXCHANGE dir, eUID4VM id = eUID4VM.NONE, object obj = null)
        {
            bool rtn = true;
            switch (dir)
            {
                case eDATAEXCHANGE.Model2View:
                    {
                        switch (viwer)
                        {
                            case eVIWER.Monitor:
                                switch (id)
                                {
                                    case eUID4VM.DASH_MONI_ALL:
                                        Evt_MainWin_DataExchange?.Invoke(_status, (eDATAEXCHANGE.Model2View, eUID4VM.MAINWIN_ALL));
                                        Evt_Dash_Moni_DataExchange?.Invoke(_status, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_ALL));                                        
                                        break;
                                    case eUID4VM.DASH_MONI_EqpState:
                                        Evt_MainWin_DataExchange?.Invoke(_status.eqpState, (eDATAEXCHANGE.Model2View, eUID4VM.MAINWIN_EqpState));
                                        Evt_Dash_Moni_DataExchange?.Invoke(_status.eqpState, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_EqpState));
                                        break;
                                    case eUID4VM.DASH_MONI_VECSTATE:
                                        Evt_Dash_Moni_DataExchange?.Invoke(_status, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_VECSTATE));
                                        break;
                                    case eUID4VM.DASH_MONI_IO:
                                        Evt_Dash_Moni_DataExchange?.Invoke(_sys.io, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_IO));
                                        break;
                                    case eUID4VM.DASH_MONI_JOB_Assigned:
                                        Evt_Dash_Moni_DataExchange?.Invoke(_Order, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_IO));
                                        break;
                                    case eUID4VM.DASH_MONI_JOB_Update:
                                        Evt_Dash_Moni_DataExchange?.Invoke(_Order, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Update));
                                        break;
                                    case eUID4VM.DASH_MONI_JOB_Reset:
                                        Evt_Dash_Moni_DataExchange?.Invoke(_Order, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Reset));
                                        break;
                                    case eUID4VM.DASH_MONI_JOB_PioStart:
                                        {
                                            var noti = new Noti();
                                            switch (_Order.state)
                                            {
                                                case eJOBST.Arrived:
                                                    noti.nTemp = _sys.cfg.pio.nInterfaceTimeout;
                                                    noti.msg = $"Wating for Valid Signal of PIO";
                                                    break;
                                                case eJOBST.Transferring:
                                                    noti.nTemp = _sys.cfg.pio.nFeedTimeOut_Start;
                                                    noti.msg = $"Waiting for Valid Signal of PIO.";
                                                    break;
                                                case eJOBST.TransStart:
                                                    noti.nTemp = _sys.cfg.pio.nFeedTimeOut_Work;
                                                    noti.msg = $"Waiting for transfer done of the tray.";
                                                    break;
                                                case eJOBST.CarrierChanged:
                                                    noti.nTemp = _sys.cfg.pio.nFeedTimeOut_End;
                                                    noti.msg = $"Waiting for done of PIO.";
                                                    break;
                                                default: break;
                                            }
                                            if (0 < noti.nTemp)
                                            {
                                                Evt_Dash_Moni_DataExchange?.Invoke(noti, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_PioStart));
                                            }
                                            break;
                                        }
                                    case eUID4VM.DASH_MONI_START:
                                        break;
                                    case eUID4VM.DASH_MONI_STOP:
                                        break;
                                    case eUID4VM.DASH_MONI_RESET:
                                        break;
                                    case eUID4VM.DASH_MONI_DROPJOB:
                                        break;
                                    default: break;
                                }
                                break;
                            case eVIWER.Manual:
                                switch (id)
                                {
                                    case eUID4VM.DASH_MNL_ALL:
                                        Evt_Dash_Manual_DataExchange?.Invoke(_status, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_ALL));
                                        break;
                                    default: break;
                                }
                                break;
                            case eVIWER.IO:
                                switch (id)
                                {
                                    case eUID4VM.IO_SetList:
                                    case eUID4VM.IO_RefreshList:
                                        Evt_Sys_IO_DataExchange?.Invoke(obj, (eDATAEXCHANGE.Model2View, id));
                                        break;
                                    case eUID4VM.IO_ResetDirectIO:
                                        _sys.io._bDirectIO = false;
                                        Evt_Sys_IO_DataExchange?.Invoke(_sys.io._bDirectIO, (eDATAEXCHANGE.Model2View, id));
                                        break;
                                    default: break;
                                }

                                break;
                            case eVIWER.TowerLamp: Evt_Sys_Lamp_DataExchange?.Invoke(this, (_EQPStatus, _sys.lmp)); break;
                            case eVIWER.Goal:
                                {
                                    switch (id)
                                    {
                                        case eUID4VM.GOAL_LIST: Evt_Sys_Goal_List_DataExchange?.Invoke(this, _sys.goal); break;
                                        case eUID4VM.GOAL_ITEMS:
                                            {
                                                var goal = (TreeData)obj;
                                                var goaltype = goal.b_Parent.ToEnum<eGOALTYPE>();
                                                Evt_Sys_Goal_Item_DataExchange?.Invoke(_sys.goal.Get(goaltype, goal.b_Name, eSRCHGOALBY.Label), (eDATAEXCHANGE.Model2View, eUID4VM.GOAL_ITEMS));
                                                break;
                                            }
                                        default: break;
                                    }
                                    break;
                                }
                            case eVIWER.PIO: Evt_Sys_PIO_Item_DataExchange?.Invoke(this, (dir, _sys.cfg.pio)); break;
                            case eVIWER.FAC: Evt_Sys_FAC_Item_DataExchange?.Invoke(this, (dir, _sys.cfg.fac)); break;
                            default: break;
                        }
                        if (id != eUID4VM.IO_ResetDirectIO)
                        {
                            CurrView = viwer;
                        }
                        break;
                    }
                case eDATAEXCHANGE.View2Model:
                    {
                        switch (viwer)
                        {
                            case eVIWER.Monitor:
                                break;
                            case eVIWER.Manual:
                                break;
                            case eVIWER.IO:
                                {
                                    IOSRC src = obj as IOSRC;
                                    IO_OUT(src.name4Enum.ToEnum<eOUTPUT>(), !IO_GETOUT(src.name4Enum.ToEnum<eOUTPUT>()));
                                    break;
                                }
                            case eVIWER.TowerLamp:
                                {
                                    var status = (eEQPSATUS)obj;
                                    var lmp = _sys.lmp.lst.SingleOrDefault(l => l.status == status);
                                    if (null != lmp)
                                    {
                                        var lmpSt = TWRLAMP.OFF;
                                        switch (id)
                                        {
                                            case eUID4VM.TWR_GREEN_OFF: case eUID4VM.TWR_GREEN_ON: case eUID4VM.TWR_GRREN_BLINK:
                                                lmpSt = (TWRLAMP)(id - (int)eUID4VM.TWR_GREEN_OFF);
                                                lmp.Green = lmpSt;
                                                break;
                                            case eUID4VM.TWR_YELLOW_OFF: case eUID4VM.TWR_YELLOW_ON: case eUID4VM.TWR_YELLOW_BLINK:
                                                lmpSt = (TWRLAMP)(id - (int)eUID4VM.TWR_YELLOW_OFF);
                                                lmp.Yellow = lmpSt;
                                                break;
                                            case eUID4VM.TWR_RED_OFF: case eUID4VM.TWR_RED_ON: case eUID4VM.TWR_RED_BLINK:
                                                lmpSt = (TWRLAMP)(id - (int)eUID4VM.TWR_RED_OFF);
                                                lmp.Red = lmpSt;
                                                break;
                                            case eUID4VM.TWR_BUZZER_OFF: case eUID4VM.TWR_BUZZER_ON:
                                                lmp.Buzzer = (0 == (int)id - (int)eUID4VM.TWR_BUZZER_OFF) ? false : true;
                                                break;
                                        }
                                    }
                                    break;
                                }
                            case eVIWER.Goal:
                                {
                                    switch (id)
                                    {
                                        case eUID4VM.GOAL_LIST: break;
                                        case eUID4VM.GOAL_ITEMS:
                                            {
                                                var item = obj as GOALITEM;
                                                var goal = _sys.goal.Get(item.type, item.label, eSRCHGOALBY.Label);
                                                if (null != goal)
                                                {
                                                    goal.name = item.name;
                                                    goal.type = item.type;
                                                    goal.line = item.line;
                                                    goal.hostName = item.hostName;
                                                    goal.label = item.label;
                                                    goal.mcType = item.mcType;
                                                    goal.pos = new nPOS_XYR() { x = item.pos.x, y = item.pos.y, r = item.pos.r };
                                                    goal.escape = new nPOS_XYR() { x = item.escape.x, y = item.escape.y, r = item.escape.r };
                                                }
                                                break;
                                            }
                                        case eUID4VM.GOAL_ADD:
                                            {
                                                var item = obj as GOALITEM;
                                                rtn = _sys.goal.Add(item);
                                                break;
                                            }
                                        case eUID4VM.GOAL_DEL:
                                            {
                                                var item = obj as GOALITEM;
                                                rtn = _sys.goal.Remove(item);
                                                break;
                                            }
                                        case eUID4VM.GOLA_HostName: case eUID4VM.GOAL_Label: case eUID4VM.GOAL_MCType:
                                        case eUID4VM.GOAL_PosX: case eUID4VM.GOAL_PosY: case eUID4VM.GOAL_PosR:
                                        case eUID4VM.GOAL_EscapeX: case eUID4VM.GOAL_EscapeY: case eUID4VM.GOAL_EscapeR:
                                            {
                                                string[] work = $"{obj}".Split(';');
                                                var goal = _sys.goal.Get(work[0].ToEnum<eGOALTYPE>(), work[1], eSRCHGOALBY.Label);
                                                if (null != goal)
                                                {
                                                    switch (id)
                                                    {
                                                        case eUID4VM.GOLA_HostName: goal.hostName = work[2]; break;
                                                        case eUID4VM.GOAL_Label:
                                                            if (goal.label != work[2]) { goal.label = work[2]; }
                                                            else { rtn = false; }
                                                            break;
                                                        case eUID4VM.GOAL_MCType: goal.mcType = work[2].ToEnum<ePIOTYPE>(); break;
                                                        case eUID4VM.GOAL_PosX: case eUID4VM.GOAL_PosY: case eUID4VM.GOAL_PosR:
                                                        case eUID4VM.GOAL_EscapeX: case eUID4VM.GOAL_EscapeY: case eUID4VM.GOAL_EscapeR:
                                                            {
                                                                var value = 0;
                                                                rtn = int.TryParse($"{work[2]}", out value);
                                                                if (true == rtn)
                                                                {
                                                                    switch (id)
                                                                    {
                                                                        case eUID4VM.GOAL_PosX: goal.pos.x = value; break;
                                                                        case eUID4VM.GOAL_PosY: goal.pos.y = value; break;
                                                                        case eUID4VM.GOAL_PosR: goal.pos.r = value; break;
                                                                        case eUID4VM.GOAL_EscapeX: goal.escape.x = value; break;
                                                                        case eUID4VM.GOAL_EscapeY: goal.escape.y = value; break;
                                                                        case eUID4VM.GOAL_EscapeR: goal.escape.r = value; break;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                    }
                                                }
                                                else
                                                {
                                                    rtn = false;
                                                }
                                                break;
                                            }
                                        default: break;
                                    }
                                    break;
                                }
                            case eVIWER.PIO:
                                {
                                    var value = 0;
                                    var pio = _sys.cfg.pio;
                                    switch (id)
                                    {
                                        case eUID4VM.PIO_0: case eUID4VM.PIO_1: case eUID4VM.PIO_2: case eUID4VM.PIO_3: case eUID4VM.PIO_4:
                                            rtn = int.TryParse($"{obj}", out value);
                                            if (true == rtn)
                                            {
                                                switch (id)
                                                {
                                                    case eUID4VM.PIO_0: pio.nInterfaceTimeout = value; break;
                                                    case eUID4VM.PIO_1: pio.nDockSenChkTime = value; break;
                                                    case eUID4VM.PIO_2: pio.nFeedTimeOut_Start = value; break;
                                                    case eUID4VM.PIO_3: pio.nFeedTimeOut_Work = value; break;
                                                    case eUID4VM.PIO_4: pio.nFeedTimeOut_End = value; break;
                                                    case eUID4VM.SENDELAY: pio.nSenDelay = value; break;
                                                    case eUID4VM.COMM_TIMEOUT: pio.nCommTimeout = value; break;
                                                    case eUID4VM.CONVSPD_NORMAL: pio.nConvSpd_Normal = value; break;
                                                    case eUID4VM.CONVSPD_SLOW: pio.nConvSpd_Slow = value; break;
                                                }
                                            }
                                            break;
                                        default: break;
                                    }
                                    break;
                                }
                            case eVIWER.FAC:
                                {
                                    var value = 0;
                                    var fac = _sys.cfg.fac;
                                    switch (id)
                                    {
                                        case eUID4VM.FAC_EQPName: fac.eqpName = $"{obj}"; break;
                                        case eUID4VM.FAC_MPlusIP: fac.mplusIP = $"{obj}"; break;
                                        case eUID4VM.FAC_VehicleIP: fac.VecIP = $"{obj}"; break;
                                        case eUID4VM.FAC_MPlusPort:
                                            rtn = int.TryParse($"{obj}", out value);
                                            if (true == rtn)
                                            {
                                                fac.mplusPort = value;
                                            }
                                            break;
                                        default: break;
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                        CurrView = viwer;
                        break;
                    }
                case eDATAEXCHANGE.Load:
                    break;
                case eDATAEXCHANGE.Save:
                    break;
                case eDATAEXCHANGE.StatusUpdate:
                    break;
                default: break;
            }
            return rtn;
        }

        private void On_IO_UpdateStatus(object sender, (long nIn, long nGetout) e)
        {
            var _in = new Any64();
            var _getout = new Any64();
            _in.INT64 = e.nIn;
            _getout.INT64 = e.nGetout;

            var inputs = IO_LstGet(eVIWER.IO, eIOTYPE.INPUT);
            foreach (IOSRC item in inputs)
            {
                item.state = _in[item.RealID];
            }

            var outputs = IO_LstGet(eVIWER.IO, eIOTYPE.OUTPUT);
            foreach (IOSRC item in outputs)
            {
                item.getOutput = _getout[item.RealID];
            }

            var order = _status.Order;
            var vtrValid = _sys.io.Get(eINPUT.VTA_PIO_Valid);
            var vtrReady = _sys.io.Get(eINPUT.VTA_PIO_Ready);
            var vtrCompleted = _sys.io.Get(eINPUT.VTA_PIO_Completed);
            var vtrMCErr = _sys.io.Get(eINPUT.VTA_PIO_MC_ERROR);
            var vtrOutReady = _sys.io.Get(eOUTPUT.VTA_PIO_Ready);
            var vtrOutComplte = _sys.io.Get(eOUTPUT.VTA_PIO_Completed);
            switch (order.state)
            {
                case eJOBST.Assign:
                case eJOBST.Enroute:
                case eJOBST.Arrived:
                case eJOBST.Transferring:
                case eJOBST.TransStart:
                case eJOBST.CarrierChanged:
                    vtrValid.state = PIO_IN_Valid();
                    vtrReady.state = PIO_IN_Ready();
                    vtrCompleted.state = PIO_IN_Completed();
                    vtrMCErr.state = PIO_IN_ERR();
                    vtrOutReady.getOutput = PIO_GetOut_Ready();
                    vtrOutComplte.getOutput = PIO_GetOut_Completed();
                    break;
                case eJOBST.None:
                case eJOBST.TransComplete:
                case eJOBST.UserStopped:
                default:
                    vtrValid.state = false;
                    vtrReady.state = false;
                    vtrCompleted.state = false;
                    vtrMCErr.state = false;
                    vtrOutReady.getOutput = false;
                    vtrOutComplte.getOutput = false;
                    break;
            }
        }

        public bool IO_IN(eINPUT id)
        {
            var src = _sys.io.Get(id);
            return src.state;
        }

        public bool IO_GETOUT(eOUTPUT id)
        {
            var src = _sys.io.Get(id);
            return src.getOutput;
        }

        public void IO_OUT(eOUTPUT id, bool bTrg)
        {
            io.SetOutput(id, bTrg);
        }

        public IOSRC IO_SrcGet_IN(eINPUT id)
        {
            var src = _Data.Inst.sys.io.Get(id);
            return src;
        }

        public IOSRC IO_SrcGet_OUT(eOUTPUT id)
        {
            var src = _Data.Inst.sys.io.Get(id);
            return src;
        }

        public List<IOSRC> IO_LstGet(eVIWER view, eIOTYPE type)
        {
            List<IOSRC> rtn = new List<IOSRC>();
            rtn.Clear();
            switch (view)
            {
                case eVIWER.Monitor:
                    switch (type)
                    {
                        case eIOTYPE.INPUT:
                            rtn.Add(_sys.io.Get(eINPUT.VTA_PIO_Valid));
                            rtn.Add(_sys.io.Get(eINPUT.VTA_PIO_Ready));
                            rtn.Add(_sys.io.Get(eINPUT.VTA_PIO_Completed));
                            rtn.Add(_sys.io.Get(eINPUT.VTA_PIO_MC_ERROR));
                            rtn.Add(_sys.io.Get(eINPUT.MFC_Pre_TrayDtc));
                            rtn.Add(_sys.io.Get(eINPUT.MFC_Cen_TrayDtc));
                            rtn.Add(_sys.io.Get(eINPUT.MFC_End_TrayDtc));
                            break;
                        case eIOTYPE.OUTPUT:
                            rtn.Add(_sys.io.Get(eOUTPUT.VTA_PIO_Ready));
                            rtn.Add(_sys.io.Get(eOUTPUT.VTA_PIO_Completed));
                            rtn.Add(_sys.io.Get(eOUTPUT.MFC_Conv_Run));
                            rtn.Add(_sys.io.Get(eOUTPUT.MFC_Conv_Reverse));
                            break;
                    }
                    break;
                case eVIWER.IO:
                    switch (type)
                    {
                        case eIOTYPE.INPUT: rtn = _sys.io.lst.Where(p => p.Type == eIOTYPE.INPUT).ToList(); break;
                        case eIOTYPE.OUTPUT: rtn = _sys.io.lst.Where(p => p.Type == eIOTYPE.OUTPUT).ToList(); break;
                    }
                    break;
                default: break;
            }
            return rtn;
        }

        public bool VEC_IN_POSOK()
        {
            bool rtn = false;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                    rtn = IO_IN(eINPUT.MFC_POS_OK);
                    break;
                default: break;
            }
            return rtn;
        }

        public void CONV_SetSpd(int unit)
        {

        }

        public void CONV_Motor(bool bTrg, bool bDir=false)
        {
            if ( bTrg )
            {
                IO_OUT(eOUTPUT.MFC_Conv_Run, true);
                IO_OUT(eOUTPUT.MFC_Conv_Reverse, bDir);
            }
            else
            {
                IO_OUT(eOUTPUT.MFC_Conv_Run, false);
                IO_OUT(eOUTPUT.MFC_Conv_Reverse, false);
            }            
        }


        public bool PIO_IN_Valid()
        {
            bool rtn = false;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                    rtn = IO_IN(eINPUT.MFC_PIO_GO);
                    break;
                default: break;
            }
            return rtn;
        }

        public bool PIO_IN_Ready()
        {
            bool rtn = false;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            rtn = IO_IN(eINPUT.MFC_PIO_2);
                            break;
                        case ePIOTYPE.JR:
                            rtn = IO_IN(eINPUT.MFC_PIO_2);
                            break;
                        default: break;
                    }
                    break;
                case eJOBTYPE.UNLOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            rtn = IO_IN(eINPUT.MFC_PIO_6);
                            break;
                        case ePIOTYPE.JR:
                            rtn = IO_IN(eINPUT.MFC_PIO_5);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
            return rtn;
        }

        public bool PIO_IN_Completed()
        {
            bool rtn = false;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            rtn = IO_IN(eINPUT.MFC_PIO_3);
                            break;
                        case ePIOTYPE.JR:
                            rtn = IO_IN(eINPUT.MFC_PIO_3);
                            break;
                        default: break;
                    }
                    break;
                case eJOBTYPE.UNLOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            rtn = IO_IN(eINPUT.MFC_PIO_7);
                            break;
                        case ePIOTYPE.JR:
                            rtn = IO_IN(eINPUT.MFC_PIO_6);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
            return rtn;
        }

        public bool PIO_IN_ERR()
        {
            bool rtn = false;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                    rtn = IO_IN(eINPUT.MFC_PIO_8);
                    break;
                default: break;
            }
            return rtn;
        }

        public void PIO_Out_Ready(bool bTrg)
        {
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            IO_OUT(eOUTPUT.MFC_PIO_1, bTrg);
                            break;
                        case ePIOTYPE.JR:
                            IO_OUT(eOUTPUT.MFC_PIO_1, bTrg);
                            break;
                        default: break;
                    }
                    break;
                case eJOBTYPE.UNLOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            IO_OUT(eOUTPUT.MFC_PIO_5, bTrg);
                            break;
                        case ePIOTYPE.JR:
                            IO_OUT(eOUTPUT.MFC_PIO_5, bTrg);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }

        public bool PIO_GetOut_Ready()
        {
            var rtn = false;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            rtn = IO_GETOUT(eOUTPUT.MFC_PIO_1);
                            break;
                        case ePIOTYPE.JR:
                            rtn = IO_GETOUT(eOUTPUT.MFC_PIO_1);
                            break;
                        default: break;
                    }
                    break;
                case eJOBTYPE.UNLOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            rtn = IO_GETOUT(eOUTPUT.MFC_PIO_5);
                            break;
                        case ePIOTYPE.JR:
                            rtn = IO_GETOUT(eOUTPUT.MFC_PIO_5);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
            return rtn;
        }
        public void IO_AllOff4Seq()
        {
            IO_OUT(eOUTPUT.MFC_PIO_1, false);
            IO_OUT(eOUTPUT.MFC_PIO_2, false);
            IO_OUT(eOUTPUT.MFC_PIO_3, false);
            IO_OUT(eOUTPUT.MFC_PIO_4, false);
            IO_OUT(eOUTPUT.MFC_PIO_5, false);
            IO_OUT(eOUTPUT.MFC_PIO_6, false);
            IO_OUT(eOUTPUT.MFC_PIO_7, false);
            IO_OUT(eOUTPUT.MFC_PIO_8, false);
            CONV_Motor(false);
        }
        public void PIO_Out_Completed(bool bTrg)
        {
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            IO_OUT(eOUTPUT.MFC_PIO_2, bTrg);
                            break;
                        case ePIOTYPE.JR:
                            IO_OUT(eOUTPUT.MFC_PIO_2, bTrg);
                            break;
                        default: break;
                    }
                    break;
                case eJOBTYPE.UNLOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            IO_OUT(eOUTPUT.MFC_PIO_6, bTrg);
                            break;
                        case ePIOTYPE.JR:
                            IO_OUT(eOUTPUT.MFC_PIO_6, bTrg);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }

        public bool PIO_GetOut_Completed()
        {
            var rtn = false;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            rtn = IO_GETOUT(eOUTPUT.MFC_PIO_2);
                            break;
                        case ePIOTYPE.JR:
                            rtn = IO_GETOUT(eOUTPUT.MFC_PIO_2);
                            break;
                        default: break;
                    }
                    break;
                case eJOBTYPE.UNLOADING:
                    switch (job.goal.mcType)
                    {
                        case ePIOTYPE.KCH:
                        case ePIOTYPE.CLR:
                            rtn = IO_GETOUT(eOUTPUT.MFC_PIO_6);
                            break;
                        case ePIOTYPE.JR:
                            rtn = IO_GETOUT(eOUTPUT.MFC_PIO_6);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
            return rtn;
        }

        public void Job_ManualStart(eSEQLIST id, eJOBTYPE type = eJOBTYPE.NONE)
        {
            switch (_EQPStatus)
            {                
                case eEQPSATUS.Stop:
                    {
                        foreach (_SEQBASE item in _lstSeqs)
                        {
                            item.arg.Init();
                            switch (id)
                            {
                                case eSEQLIST.EscapeEQP:
                                    switch (_sys.cfg.fac.seqMode)
                                    {
                                        case eSCENARIOMODE.PC: break;
                                        default: item.SetStop(); break;
                                    }
                                    break;
                                case eSEQLIST.Move2Dst: break;
                                case eSEQLIST.PIO:
                                    if (id != eSEQLIST.PIO)
                                    {
                                        item.SetStop();
                                    }
                                    break;
                                case eSEQLIST.Pick:
                                case eSEQLIST.Drop: break;
                                case eSEQLIST.MAX_SEQ: break;
                                default: item.SetStop(); break;
                            }
                        }

                        var pick = Seq_Get(eSEQLIST.Pick);
                        var drop = Seq_Get(eSEQLIST.Drop);
                        switch (id)
                        {
                            case eSEQLIST.PIO:
                                switch (type)
                                {
                                    case eJOBTYPE.LOADING: pick.SetStop(); break;
                                    case eJOBTYPE.UNLOADING: drop.SetStop(); break;
                                    default: break;
                                }
                                break;

                            case eSEQLIST.Pick: drop.SetStop(); break;
                            case eSEQLIST.Drop: pick.SetStop(); break;
                            default: break;
                        }
                        _status.bManualSeq = true;
                        Start(CHANGEMODEBY.MANUAL);
                        break;
                    }
                default: break;
            }            
        }

        public void Job_MnaualInit(eSEQLIST id, eJOBTYPE type)
        {
            var seqsel = Seq_Get(id);
            seqsel.arg.Init();
            var pick = Seq_Get(eSEQLIST.Pick);
            var drop = Seq_Get(eSEQLIST.Drop);
            switch (id)
            {
                case eSEQLIST.PIO:
                    switch (type)
                    {
                        case eJOBTYPE.LOADING: drop.arg.Init(); break;
                        case eJOBTYPE.UNLOADING: pick.arg.Init(); break;
                        default: break;
                    }
                    break;

                default: seqsel.SetStop(); break;
            }
        }

        public void Job_ManualStop(eSEQLIST id, eJOBTYPE type = eJOBTYPE.NONE)
        {            
            var seqsel = Seq_Get(id);
            var pick = Seq_Get(eSEQLIST.Pick);
            var drop = Seq_Get(eSEQLIST.Drop);
            switch (id)
            {
                case eSEQLIST.PIO:
                    switch (type)
                    {
                        case eJOBTYPE.LOADING: drop.SetStop();  break;
                        case eJOBTYPE.UNLOADING: pick.SetStop(); break;
                        default: break;
                    }
                    break;
                
                default: seqsel.SetStop(); break;
            }
            _status.bManualSeq = true;
            Start(CHANGEMODEBY.MANUAL);
        }
        public eERROR Job_CheckTray()
        {
            var rtn = eERROR.None;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING:
                    if (IO_IN(eINPUT.MFC_Pre_TrayDtc) && IO_IN(eINPUT.MFC_Cen_TrayDtc) && IO_IN(eINPUT.MFC_End_TrayDtc))
                    {
                        rtn = eERROR.None;
                    }
                    else
                    {
                        rtn = eERROR.NoneofTrays;
                    }
                    break;
                case eJOBTYPE.UNLOADING:
                case eJOBTYPE.STANDBY:
                case eJOBTYPE.CAHRGE:
                    if (IO_IN(eINPUT.MFC_Pre_TrayDtc) || IO_IN(eINPUT.MFC_Cen_TrayDtc) || IO_IN(eINPUT.MFC_End_TrayDtc))
                    {
                        rtn = eERROR.TrayDetected;
                    }
                    break;
                default: break;
            }
            return rtn;
        }

        public void Job_SetState(eJOBST state)
        {
            _Order.state = state;
            switch (_Order.state)
            {
                case eJOBST.None:
                    _status.bIsManual = false;
                    switch (_EQPStatus)
                    {
                        case eEQPSATUS.Run: _EQPStatus = eEQPSATUS.Idle; break;
                        default: break;
                    }
                    IO_AllOff4Seq();
                    DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Reset);
                    break;
                case eJOBST.Assign:
                    _EQPStatus = eEQPSATUS.Run;
                    DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Assigned);
                    break;
                case eJOBST.Enroute:
                    break;
                case eJOBST.Arrived:
                case eJOBST.Transferring:
                case eJOBST.TransStart:
                case eJOBST.CarrierChanged:
                    DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_PioStart);
                    break;
                case eJOBST.TransComplete:
                case eJOBST.UserStopped:
                    _Order.Init();
                    Job_SetState(eJOBST.None);
                    break;
            }
            Job_ReportState2Host();
            DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Update);
        }

        public LD_STATUS VEC_GetCurrState()
        {
            var ld = vec as LD;
            return ld._CurrStatus;
        }

        public void VEC_ArclMsg(string msg, bool bFromJob = false)
        {
            var ld = vec as LD;
            ld.ArclMsg(msg, bFromJob);
        }

        private void Job_ReportState2Host()
        {
            switch (_sys.cfg.fac.seqMode)
            {
                case eSCENARIOMODE.PLC:
                    switch (_Order.state)
                    {
                        case eJOBST.Transferring:
                            switch (_Order.type)
                            {
                                case eJOBTYPE.LOADING: VEC_ArclMsg("P_001", true); break;
                                case eJOBTYPE.UNLOADING: VEC_ArclMsg("P_011", true); break;
                                default: break;
                            }
                            break;
                        case eJOBST.TransStart:
                            switch (_Order.type)
                            {
                                case eJOBTYPE.LOADING: VEC_ArclMsg("P_002", true); break;
                                case eJOBTYPE.UNLOADING: VEC_ArclMsg("P_012", true); break;
                                default: break;
                            }
                            break;
                        case eJOBST.CarrierChanged:
                            switch (_Order.type)
                            {
                                case eJOBTYPE.LOADING: VEC_ArclMsg("P_003", true); break;
                                case eJOBTYPE.UNLOADING: break;
                                default: break;
                            }
                            break;
                        case eJOBST.TransComplete:
                            switch (_Order.type)
                            {
                                case eJOBTYPE.LOADING: VEC_ArclMsg("P_004", true); break;
                                case eJOBTYPE.UNLOADING: VEC_ArclMsg("P_013", true); break;
                                default: break;
                            }
                            break;
                        default: break;
                    }
                    break;
                case eSCENARIOMODE.PC:
                    {
                        var mplus = Tsk_Get(eTASKLIST.MPLUSCOMM) as Tsk_MPlusComm;
                        mplus.SetJobState();
                        break;
                    }
                default: break;
            }

        }

        public bool VEC_ChkArrivedAtGoal()
        {
            var ld = vec as LD;
            var stopped = false;
            switch (_Data.Inst.sys.cfg.fac.seqMode)
            {
                case eSCENARIOMODE.PC:
                    switch (ld._CurrStatus.state.st)
                    {
                        case eVECSTATE.STOPPED:
                        case eVECSTATE.ARRIVED_AT: stopped = true; break;
                        default: break;
                    }
                    break;
                case eSCENARIOMODE.PLC:
                    switch (ld._CurrStatus.state.st)
                    {
                        case eVECSTATE.GOING_TO: stopped = VEC_PauseState; break;
                        default: break;
                    }
                    break;
            }
            return ld.bRobotPaused;
        }

        public bool VEC_PauseState  {
            get {
                var ld = vec as LD;
                return ld.bRobotPaused;
            }            
        }

        public void VEC_TskStart(eVEC_CMD cmd, SENDARG arg)
        {
            var tskVec = Tsk_Get(eTASKLIST.VECCMD) as Tsk_VecCmd;            
            tskVec.WrkStart(cmd, arg);
        }

        public void VEC_TskStop()
        {
            VEC_SendCmd(eVEC_CMD.Stop, new SENDARG());
            var tskVec = Tsk_Get(eTASKLIST.VECCMD) as Tsk_VecCmd;
            tskVec.Init();
        }

        public void VEC_SendCmd(eVEC_CMD cmd, SENDARG arg)
        {
            if ( null != arg )
            {
                var data = JCVT_VEC.Set<SENDARG>(arg);
                vec.Send(cmd, data);
            }
            else
            {
                vec.Send(cmd);
            }
        }

        public void VEC_SetState4Job(eJOBST state)
        {
            var vec = _status.vecState;
            switch (state)
            {
                case eJOBST.None:
                    DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Reset);
                    DoingDataExchage(eVIWER.Manual, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_ALL);
                    vec.JobState = eROBOTST.NotAssigned;                    
                    break;
                case eJOBST.Assign:
                    vec.JobState = eROBOTST.Enroute;
                    break;
                case eJOBST.Enroute:
                    switch (_status.Order.type)
                    {
                        case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING: vec.JobState = eROBOTST.Enroute; break;
                        case eJOBTYPE.STANDBY: vec.JobState = eROBOTST.Standby; break;
                        case eJOBTYPE.CAHRGE: vec.JobState = eROBOTST.Charging; break;
                        default: break;
                    }
                    break;
                case eJOBST.Arrived: break;
                case eJOBST.Transferring:                    
                case eJOBST.TransStart:
                case eJOBST.CarrierChanged:
                    vec.JobState = (eJOBTYPE.LOADING == _status.Order.type) ? eROBOTST.Depositing : eROBOTST.Acquiring;
                    break;
                case eJOBST.TransComplete: break;
                case eJOBST.UserStopped:
                    switch (_EQPStatus)
                    {
                        case eEQPSATUS.Init:
                        case eEQPSATUS.Stop:
                        case eEQPSATUS.Error:
                        case eEQPSATUS.EMG: break;
                        default: vec.JobState = eROBOTST.NotAssigned; break;
                    }
                    break;
                default: break;
            }
            
            switch (vec.JobState)
            {
                case eROBOTST.None: break;
                case eROBOTST.NotAssigned:
                case eROBOTST.Enroute:
                case eROBOTST.Parked:
                case eROBOTST.Acquiring:
                case eROBOTST.Depositing:                    
                case eROBOTST.Charging:                    
                case eROBOTST.Standby:
                    DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Update);                    
                    break;                
                default: break;
            }
        }

        private bool ChkAlarm()
        {
            var rtn = false;

            return rtn;
        }


        private TIMEARG SeqScan = new TIMEARG();
        public string _SeqScanTime => StopFlag ? "STOPPED" : $"{SeqScan.nCurr} msec";
        public bool StopFlag { get; set; } = true;
        public bool ThrdRun { get; set; } = false;
        public void Start(CHANGEMODEBY oder, bool bManual = false)
        {            
            StopFlag = false; ThrdRun = true;
            if (true == ChkAlarm())
            {                
                Logger.Inst.Write(CmdLogType.prdt, $"설비 알람이 발생하여 구동할 수 없습니다.");
                return;
            }

            switch (_EQPStatus)
            {                
                case eEQPSATUS.Stop:
                    if ( false == _status.bManualSeq )
                    {
                        Logger.Inst.Write(CmdLogType.prdt, $"Sequence 프로세스(Seq Thread) 시작 - {oder.ToString()}, Kindof-{(bManual ? "AUTO" : "MANUAL")}");
                        var main = _lstSeqs.Single(t => t.arg.GetID() == eSEQLIST.Main).arg;
                        switch (main.nStatus)
                        {
                            case eSTATE.None: case eSTATE.WorkTrg: _EQPStatus = eEQPSATUS.Idle; break;
                            default:
                                switch (main.nStep)
                                {
                                    case 10:
                                    case DEF_CONST.SEQ_INIT:
                                        switch (_Order.type)
                                        {
                                            case eJOBTYPE.LOADING:
                                            case eJOBTYPE.UNLOADING:
                                                switch (_Order.state)
                                                {
                                                    case eJOBST.None: _EQPStatus = eEQPSATUS.Idle; break;
                                                    default: _EQPStatus = eEQPSATUS.Run; break;
                                                }
                                                break;
                                            default: _EQPStatus = eEQPSATUS.Idle; break;
                                        }
                                        break;
                                    default: _EQPStatus = eEQPSATUS.Run; break;
                                }
                                break;
                        }

                        // 작업 시퀀스 상태확인하여 초기화.
                        foreach (var item in _lstSeqs)
                        {
                            switch (item.arg.GetID())
                            {
                                case eSEQLIST.MAX_SEQ: continue;
                                default:
                                    switch (_Order.type)
                                    {
                                        case eJOBTYPE.NONE:
                                            item.arg.Init(); // 작업이 없을경우 초기화한다.
                                            switch (item.arg.GetID())
                                            {
                                                case eSEQLIST.Main: item.arg.WorkTrg(); break;
                                                default: break;
                                            }
                                            break;
                                        default:
                                            switch (_Order.state)
                                            {
                                                case eJOBST.None:
                                                case eJOBST.TransComplete:
                                                case eJOBST.UserStopped:
                                                    item.arg.Init();
                                                    switch (item.arg.GetID())
                                                    {
                                                        case eSEQLIST.Main:
                                                            item.arg.WorkTrg();
                                                            break;
                                                        default: break;
                                                    }
                                                    break;
                                                default: item.arg.Reset(); break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Logger.Inst.Write(CmdLogType.prdt, $"Manual Sequence 프로세스(Seq Thread) 시작 - {oder.ToString()}");
                        _EQPStatus = eEQPSATUS.Stopping;
                    }
                    StartTasks();
                    break;                
                default: break;
            }
        }

        public void Stop(CHANGEMODEBY oder)
        {
            switch (_EQPStatus)
            {
                case eEQPSATUS.EMG: Reset(CHANGEMODEBY.PROCESS); break;
                case eEQPSATUS.Idle:
                case eEQPSATUS.Run:
                    _EQPStatus = eEQPSATUS.Stopping;
                    Logger.Inst.Write(CmdLogType.prdt, $"Sequence 프로세스(Tasks Thread) 중지를 시작합니다 - {oder.ToString()}");
                    break;
                case eEQPSATUS.Error:
                    _EQPStatus = eEQPSATUS.Stop;
                    break;
                default: break;
            }
        }

        public void Reset(CHANGEMODEBY oder)
        {
            switch (_EQPStatus)
            {
                case eEQPSATUS.Error:
                    Stop(oder);
                    Logger.Inst.Write(CmdLogType.prdt, $"Sequence 프로세스(Tasks Thread) 리셋합니다 - {oder.ToString()}");
                    break;
                case eEQPSATUS.EMG:
                    Stop(oder);
                    Logger.Inst.Write(CmdLogType.prdt, $"Sequence 프로세스(Tasks Thread) 리셋합니다 - {oder.ToString()}");
                    break;
                default: break;
            }
        }

        public async void StartTasks()
        {
            StopFlag = false; ThrdRun = true;            
            switch (_Order.type)
            {
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:break;
                default: break;
            }
            Logger.Inst.Write(CmdLogType.prdt, $"Sequence Process 작업 시작");
            await Tsk_Work();
            Logger.Inst.Write(CmdLogType.prdt, $"Sequence Process 작업 종료");
            ThrdRun = false;
        }
        
        public async Task Tsk_Work()
        {
            do
            {
                bool bWorkDone = false;                
                foreach (var item in _lstSeqs)
                {
                    switch (_EQPStatus)
                    {                        
                        case eEQPSATUS.Idle: case eEQPSATUS.Run:
                            if ( eSEQLIST.MAX_SEQ == item.arg.GetID()) continue;
                            item.Run();
                            break;                        
                        case eEQPSATUS.Stopping:
                            if (eSEQLIST.MAX_SEQ == item.arg.GetID()) continue;
                            if (false == item.arg.bStop)
                            {
                                item.Run();
                            }
                            bWorkDone &= item.arg.bStop;
                            break;
                        case eEQPSATUS.EMG: bWorkDone = true; break;
                        default: break;
                    }
                    if (true == bWorkDone) break;
                }
                await Task.Delay(1);
                SeqScan.Check();
                if (true == bWorkDone)
                {
                    StopFlag = true;
                }
            }
            while (!StopFlag);

            var tskMplus = Tsk_Get(eTASKLIST.MPLUSCOMM) as Tsk_MPlusComm;            
            switch (_EQPStatus)
            {
                case eEQPSATUS.Init:
                case eEQPSATUS.EMG:
                    var main = Seq_Get(eSEQLIST.Main);
                    switch (main.arg.nErr)
                    {
                        case eERROR.None: break; ;
                        default: msgBox.ShowDialog($"{main.arg.nErr}", MsgBox.MsgType.Error, MsgBox.eBTNSTYLE.OK); break;
                    }
                    break;
                default:
                    foreach (var item in _lstSeqs)
                    {
                        if (item.arg.nErr == eERROR.None) continue;
                        switch (_EQPStatus)
                        {
                            case eEQPSATUS.Init:
                            case eEQPSATUS.EMG:
                            case eEQPSATUS.Error: break;
                            default:
                                switch (item.arg.nErr)
                                {
                                    case eERROR.EMG: _EQPStatus = eEQPSATUS.EMG; break;
                                    default: _EQPStatus = eEQPSATUS.Error; break;
                                }
                                tskMplus.SetErr(item.arg.nErr);
                                break;
                        }
                        msgBox.ShowDialog($"{item.arg.nErr}", MsgBox.MsgType.Error, MsgBox.eBTNSTYLE.OK);
                        tskMplus.SetErr(eERROR.Clear);
                        if (eEQPSATUS.EMG == _EQPStatus || eEQPSATUS.Init == _EQPStatus) break;
                    }
                    break;
            }

            switch (_EQPStatus)
            {
                case eEQPSATUS.Init:
                case eEQPSATUS.Initing:
                case eEQPSATUS.EMG:
                    {
                        _EQPStatus = eEQPSATUS.Init;
                        tskMplus.SetErr(eERROR.None);
                        break;
                    }
                default: _EQPStatus = eEQPSATUS.Stop; break;
            }
            IO_AllOff4Seq();
            _status.bManualJob = _status.bManualSeq = false;
            Logger.Inst.Write(CmdLogType.prdt, $"Job 프로세스(Tasks Thread) 종료");
            ThrdRun = false;
        }        

        public _SEQBASE Seq_Get(eSEQLIST id)
        {
            return _lstSeqs.Single(t => t.arg.GetID() == id);
        }        

        public void Seq_Stop(eSEQLIST id)
        {
            var tsk = Seq_Get(id);
            tsk.arg.StopTrg();
        }

        public _TSKBASE Tsk_Get(eTASKLIST id)
        {
            return _lstTsks.Single(t => t.arg.GetID() == id);
        }

        public void SetErr(eERROR err, CHANGEMODEBY chg, bool bChgeStatus = true)
        {
            var main = _lstSeqs.Single(t => t.arg.GetID() == eSEQLIST.Main).arg;
            switch (_EQPStatus)
            {
                case eEQPSATUS.Stopping:
                case eEQPSATUS.Idle:
                case eEQPSATUS.Run:
                    main.SetErr(err);                    
                    switch (err)
                    {
                        case eERROR.EMG:
                            _EQPStatus = eEQPSATUS.EMG;
                            Job_SetState(eJOBST.UserStopped);
                            break;
                        default: Stop(CHANGEMODEBY.PROCESS); break;
                    }
                    break;
                case eEQPSATUS.Error:
                case eEQPSATUS.EMG:
                default:
                    eEQPSATUS beforeSt = eEQPSATUS.Initing;
                    if (true == bChgeStatus)
                    {
                        beforeSt = _EQPStatus;
                    }                    
                    switch (err)
                    {
                        case eERROR.EMG:                                                                                
                            break;                        
                        default: break;
                    }
                    Logger.Inst.Write(CmdLogType.Err, $"SetErr : {chg.ToString()}에서 {err.ToString()}에러를 발생하였습니다.");
                    switch (err)
                    {
                        case eERROR.EMG: _EQPStatus = eEQPSATUS.EMG; break;
                        default: _EQPStatus = eEQPSATUS.Error; break;
                    }
                    switch (beforeSt)
                    {
                        case eEQPSATUS.EMG:
                        case eEQPSATUS.Stop:
                            msgBox.ShowDialog($"{err}", MsgBox.MsgType.Error, MsgBox.eBTNSTYLE.OK);
                            if (true == bChgeStatus)
                            {
                                switch (beforeSt)
                                {
                                    case eEQPSATUS.Init:
                                    case eEQPSATUS.Initing: _EQPStatus = eEQPSATUS.Init; break;
                                    default:
                                        switch (err)
                                        {
                                            case eERROR.EMG: _EQPStatus = eEQPSATUS.Init; break;
                                            default: _EQPStatus = eEQPSATUS.Stop; break;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        private bool Mplus_RemoteCmd(eREMOTE_MODE mode)
        {
            bool rtn = false;
            return rtn;                
        }


    }
}
