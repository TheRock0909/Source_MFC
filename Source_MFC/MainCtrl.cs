using Source_MFC.Global;
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
        public MainCtrl(MainWindow main)
        {
            frmMain = main;
            Logger.Inst.MakeSrcHdl($"{_EQPName}");
            _sys = _Data.Inst.sys;
            _status = _Data.Inst.status;                       
            _log.Evt_WriteLog += On_WriteLogMsg;
            thd_BackProc = new Thread(msgBackProcess)
            {
                IsBackground = true
            };            
        }

        public void _Finalize()
        {
            StopBackProc();
        }
        
        public void DisignLoadComp()
        {            
            _EQPStatus = !_status.bSimul ? eEQPSATUS.Init : eEQPSATUS.Stop;
            UpdateUserData(new USER());            
            DoingDataExchage(eVIWER.IO, eDATAEXCHANGE.Model2View, eUID4VM.IO_SetList);
            DoingDataExchage(eVIWER.TowerLamp, eDATAEXCHANGE.Model2View);
            DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.Model2View, eUID4VM.GOAL_LIST);
            DoingDataExchage(eVIWER.PIO, eDATAEXCHANGE.Model2View);
            DoingDataExchage(eVIWER.FAC, eDATAEXCHANGE.Model2View);

            DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_ALL);
            DoingDataExchage(eVIWER.Manual, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_ALL);            
            thd_BackProc.Start();
            _log.Write(CmdLogType.prdt, $"Application을 시작합니다. [{_EQPName}:{_status.swVer}]");
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
                Evt_MainWin_DataExchange?.Invoke(_status.eqpState, (eDATAEXCHANGE.Model2View, eUID4VM.MAINWIN_EqpState));
                Evt_Dash_Moni_DataExchange?.Invoke(_status.eqpState, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_EqpState));
            }
            get { return _status.eqpState; }
        }

        public void UpdateUserData(USER userData)
        {
            Evt_MainWin_DataExchange?.Invoke(userData, (eDATAEXCHANGE.Model2View, eUID4VM.MAINWIN_User));            
        }


        private void On_WriteLogMsg(object sender, WriteLogArgs e)
        {
            m_quLogMsg.Enqueue(e);
        }

        private bool isThRun = true;
        private TIMEARG backProcScan = new TIMEARG();
        public long _backProcScan => backProcScan.nCurr;
        private void msgBackProcess()
        {
            backProcScan.Reset();
            while (isThRun)
            {                
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
        private void Proc4Viwer()
        {
            if (true == tmr4fast.IsOver())
            {
                switch (CurrView)
                {
                    case eVIWER.IO: DoingDataExchage(eVIWER.IO, eDATAEXCHANGE.Model2View, eUID4VM.IO_RefreshList); break;                    
                    default: break;
                }                
            }

            if (true == tmr4middle.IsOver())
            {
                switch (CurrView)
                {
                    case eVIWER.IO: break;
                    case eVIWER.Monitor: DoingDataExchage(eVIWER.Monitor, eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_IO); break;
                    default: break;
                }
            }

            if (true == tmr4slow.IsOver())
            {
                switch (CurrView)
                {
                    case eVIWER.IO: break;
                    default: break;
                }
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
                                Evt_MainWin_DataExchange?.Invoke(_status, (eDATAEXCHANGE.Model2View, eUID4VM.MAINWIN_ALL));
                                Evt_Dash_Moni_DataExchange?.Invoke(_status, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_ALL));                                
                                break;
                            case eVIWER.Manual:
                                Evt_Dash_Manual_DataExchange?.Invoke(_status, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_ALL));
                                break;
                            case eVIWER.IO: Evt_Sys_IO_DataExchange?.Invoke(this, (eDATAEXCHANGE.Model2View, id)); break;
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
                        CurrView = viwer;
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
            return (false == _status.bSimul) ? /*io.GetInput((int)id)*/ false : false;
        }

        public bool IO_GETOUT(eOUTPUT id)
        {
            return (false == _status.bSimul) ? /*io.GetOutput((int)id)*/ false : false;
        }

        public void IO_OUT(eOUTPUT id, bool bTrg)
        {
            if (false == _status.bSimul)
            {
                //io.SetOutput((int)id, bTrg);
            }
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

    }
}
