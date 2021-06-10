using Source_MFC.Global;
using Source_MFC.HW;
using Source_MFC.HW.IO;
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
        
        public event EventHandler<(eDATAEXCHANGE dir, PIO data)> Evt_Sys_PIO_Item_DataExchange;
        public event EventHandler<(eDATAEXCHANGE dir, FAC data)> Evt_Sys_FAC_Item_DataExchange;

        public Thread thd_BackProc;
        private ConcurrentQueue<WriteLogArgs> m_quLogMsg = new ConcurrentQueue<WriteLogArgs>();        
        public string _EQPName => $"{_Data.Inst.sys.cfg.fac.eqpType}-{_Data.Inst.sys.cfg.fac.eqpName}";

        MsgBox msgBox = MsgBox.Inst;
        private _SEQBASE[] _Seqs;
        public List<_SEQBASE> _lstSeqs = new List<_SEQBASE>();
        private _TSKBASE[] _Tsks;
        public List<_TSKBASE> _lstTsks= new List<_TSKBASE>();

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

            }
            catch (Exception e)
            {
                rtn = false;
                _log.Write(CmdLogType.prdt, $"H/W 초기화에 실패하였습니다.[{_EQPName}:{_status.swVer}]-{e.ToString()}");
                
            }            

            if ( true == rtn )
            {
                _log.Write(CmdLogType.prdt, $"H/W 초기화를 시작합니다. [{_EQPName}:{_status.swVer}]");
            }            
            return rtn;
        }

        public bool Hw_Final()
        {
            var rtn = true;
            _log.Write(CmdLogType.prdt, $"H/W 종료를 시작합니다. [{_EQPName}:{_status.swVer}]");
            try
            {

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
            _Seqs[(int)eSEQLIST.Move2Dst] = new Seq_Move2Dst(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.Move2Dst]);
            _Seqs[(int)eSEQLIST.PIO] = new Seq_PIO(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.PIO]);
            _Seqs[(int)eSEQLIST.Pick] = new Seq_Pick(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.Pick]);
            _Seqs[(int)eSEQLIST.Drop] = new Seq_Drop(this);
            _lstSeqs.Add(_Seqs[(int)eSEQLIST.Drop]);
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
            _Tsks[(int)eTASKLIST.MPLUSCOMM] = new Tsk_MPlusComm(this);
            _lstTsks.Add(_Tsks[(int)eTASKLIST.MPLUSCOMM]);
            _Tsks[(int)eTASKLIST.VECCMD] = new Tsk_VecCmd(this);
            _lstTsks.Add(_Tsks[(int)eTASKLIST.VECCMD]);
        }

        public eEQPSATUS _EQPStatus
        {
            set 
            {
                if (_status.eqpState != value)
                {
                    _log.Write(CmdLogType.prdt, $"설비 상태를 {_status.eqpState.ToString()}에서 {value.ToString()}로 변경합니다.");
                }
                _status.eqpState = value;
                DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_EqpState);                
            }
            get { return _status.eqpState; }
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
            while(false == m_quLogMsg.IsEmpty)
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
                foreach (var item in this.IO_LstGet( eVIWER.IO, eIOTYPE.INPUT))
                {
                    item.state = !item.state;
                }

                foreach (var item in this.IO_LstGet(eVIWER.IO, eIOTYPE.OUTPUT))
                {
                    item.getOutput = !item.getOutput;
                }

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
                    case eVIWER.Monitor:
                        {
                            var arg = testArg;
                            switch (arg.nStep)
                            {
                                case 0:
                                    arg.nStep = 5;
                                    arg.tSen.nDelay = 500;
                                    arg.tSen.Reset();
                                    Job_SetState(CommandState.Assign);
                                    break;
                                case 5:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 10;
                                    arg.tSen.Reset();
                                    VEC_SetState4Job(RobotState.Enroute);
                                    break;
                                case 10:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 20;                                    
                                    arg.tSen.Reset();
                                    Job_SetState(CommandState.Enroute);
                                    break;
                                case 20:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 25;
                                    arg.tSen.Reset();
                                    Job_SetState(CommandState.Arrived);
                                    break;
                                case 25:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 30;
                                    arg.tSen.Reset();
                                    VEC_SetState4Job(RobotState.Acquiring);
                                    break;
                                case 30:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 40;
                                    arg.tSen.Reset();
                                    Job_SetState(CommandState.Transferring);
                                    break;
                                case 40:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 50;
                                    arg.tSen.Reset();
                                    Job_SetState(CommandState.TransStart);
                                    break;
                                case 50:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 60;
                                    arg.tSen.Reset();
                                    Job_SetState(CommandState.CarrierChanged);
                                    break;
                                case 60:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 70;
                                    arg.tSen.Reset();
                                    Job_SetState(CommandState.TransComplete);
                                    break;
                                case 70:
                                    if (false == arg.tSen.IsOver()) break;
                                    arg.nStep = 80;
                                    arg.tSen.Reset();
                                    VEC_SetState4Job(RobotState.NotAssigned);                                    
                                    break;
                                case 80: arg.nStep = 0; break;

                                default:
                                    break;
                            }
                            break;
                        }
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
                                                case CommandState.Arrived:
                                                    noti.nTemp = _sys.cfg.pio.nDockSenChkTime;
                                                    noti.msg = $"Position OK Sensor Checking...";
                                                    break;
                                                case CommandState.Transferring:
                                                    noti.nTemp = _sys.cfg.pio.nFeedTimeOut_Start;
                                                    noti.msg = $"Waiting for Valid Signal of PIO." ;
                                                    break;
                                                case CommandState.TransStart:
                                                    noti.nTemp = _sys.cfg.pio.nFeedTimeOut_Work ;
                                                    noti.msg = $"Waiting for transfer done of the tray." ;
                                                    break;
                                                case CommandState.CarrierChanged:
                                                case CommandState.TransComplete:
                                                    noti.nTemp = _sys.cfg.pio.nFeedTimeOut_Work;
                                                    noti.msg = $"Waiting for done of PIO.";
                                                    break;                                                
                                                default: break;
                                            }
                                            if ( 0 < noti.nTemp )
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
                                                Evt_Sys_Goal_Item_DataExchange?.Invoke(_sys.goal.Get(goaltype, goal.b_Name, eSRCHGOALBY.Label), (eDATAEXCHANGE.Model2View, eUID4VM.GOAL_ITEMS ) );
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
                                        case eUID4VM.GOAL_LIST:break;
                                        case eUID4VM.GOAL_ITEMS:
                                            {
                                                var item = obj as GOALITEM;
                                                var goal = _sys.goal.Get(item.type, item.label, eSRCHGOALBY.Label);
                                                if ( null != goal )
                                                {
                                                    goal.name = item.name;
                                                    goal.type = item.type;
                                                    goal.line = item.line;
                                                    goal.hostName = item.hostName;
                                                    goal.label = item.label;
                                                    goal.mcType = item.mcType;
                                                    goal.pos = new nPOS_XYR() { x = item.pos.x, y = item.pos.y, r = item.pos.r } ;
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
                                                if ( null != goal)
                                                {
                                                    switch (id)
                                                    {
                                                        case eUID4VM.GOLA_HostName: goal.hostName = work[2]; break;
                                                        case eUID4VM.GOAL_Label:
                                                            if (goal.label!= work[2]) { goal.label = work[2]; }
                                                            else { rtn = false; }
                                                            break;
                                                        case eUID4VM.GOAL_MCType: goal.mcType = work[2].ToEnum<eMFC_MCTYPE>(); break;
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
                                            if ( true == rtn )
                                            {
                                                switch (id)
                                                {                                                   
                                                    case eUID4VM.PIO_0: pio.nInterfaceTimeout = value; break;
                                                    case eUID4VM.PIO_1: pio.nDockSenChkTime = value; break;                                                        
                                                    case eUID4VM.PIO_2: pio.nFeedTimeOut_Start = value; break;                                                       
                                                    case eUID4VM.PIO_3: pio.nFeedTimeOut_Work = value; break;                                                       
                                                    case eUID4VM.PIO_4: pio.nFeedTimeOut_End = value; break;
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
                                            if ( true == rtn )
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

        public bool IO_IN(eINPUT id)
        {
            var src = _Data.Inst.sys.io.Get(id);
            return src.state;
        }        

        public bool IO_GETOUT(eOUTPUT id)
        {
            var src = _Data.Inst.sys.io.Get(id);
            return src.getOutput;
        }

        public void IO_OUT(eOUTPUT id, bool bTrg)
        {
            
        }

        public IOSRC IO_GetSrc_IN(eINPUT id)
        {
            var src = _Data.Inst.sys.io.Get(id);
            return src;
        }

        public IOSRC IO_GetSrc_OUT(eOUTPUT id)
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


        public bool PIO_IN_POSOK()
        {
            bool rtn = false;
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                    break;
                default: break;
            }
            return rtn;
        }

        public bool PIO_IN_Valid()
        {
            bool rtn = false;
            var job = _status.Order;
            switch (job.type)
            {                
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
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
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
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
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
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
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                    break;
                default: break;
            }
        }

        public void PIO_Out_Completed(bool bTrg)
        {
            var job = _status.Order;
            switch (job.type)
            {
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                    break;
                default: break;
            }
        }

        public void Job_SetState(CommandState state)
        {
            _Order.state = state;
            switch (_Order.state)
            {
                case CommandState.None:
                    DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Update);
                    Evt_Dash_Moni_DataExchange?.Invoke(new Noti() { msg = "TEST MVVM View update", nTemp = 5 }, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_PioStart));
                    break;
                case CommandState.Assign:
                    break;
                case CommandState.Enroute:
                    break;
                case CommandState.Arrived:
                    break;
                case CommandState.Transferring:
                    break;
                case CommandState.TransStart:
                    break;
                case CommandState.CarrierChanged:
                    break;
                case CommandState.TransComplete:
                case CommandState.UserStopped:
                    _Order.Init();
                    Job_SetState(CommandState.None);
                    break;                
            }
            DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Update);
        }

        public void VEC_SetState4Job(RobotState state)
        {
            var vec = _status.vecState;
            vec.JobState = state;
            switch (vec.JobState)
            {
                case RobotState.None:
                    DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Reset);
                    DoingDataExchage(eVIWER.Manual, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_ALL);
                    VEC_SetState4Job(RobotState.NotAssigned);                    
                    break;
                case RobotState.NotAssigned:
                case RobotState.Enroute:
                case RobotState.Parked:
                case RobotState.Acquiring:
                case RobotState.Depositing:                    
                case RobotState.Charging:                    
                case RobotState.Standby:
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
                    Logger.Inst.Write(CmdLogType.prdt, $"Sequence 프로세스(Seq Thread) 시작 - {oder.ToString()}, Kindof-{(bManual?"AUTO":"MANUAL")}");
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
                                        case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                                            switch (_Order.state)
                                            {
                                                case CommandState.None: _EQPStatus = eEQPSATUS.Idle; break;
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
                                    case eJOBTYPE.NONE: case eJOBTYPE.CANCEL:
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
                                            case CommandState.None: case CommandState.TransComplete: case CommandState.UserStopped:
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

                            switch (item.arg.GetID())
                            {
                                case eSEQLIST.Main:
                                    bWorkDone = item.IsCanStop();
                                    item.arg.bStop = bWorkDone;
                                    break;
                                default:
                                    if (true == item.IsCanStop())
                                    {
                                        item.arg.bStop = true;
                                    }
                                    bWorkDone &= item.arg.bStop;
                                    break;
                            }
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
                                tskMplus.SendErr(item.arg.nErr);
                                break;
                        }
                        msgBox.ShowDialog($"{item.arg.nErr}", MsgBox.MsgType.Error, MsgBox.eBTNSTYLE.OK);
                        
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
                        tskMplus.SendErr(eERROR.None);
                        break;
                    }
                default: _EQPStatus = eEQPSATUS.Stop; break;
            }            
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
                            Job_SetState(CommandState.UserStopped);
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
    }
}
