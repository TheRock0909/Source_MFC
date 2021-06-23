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
    public class Seq_PIO : _SEQBASE
    {
        public Seq_PIO(MainCtrl main)
        {
            _ctrl = main;
            arg = new TASKARG(eSEQLIST.PIO);
        }

        private void WriteLog()
        {
            var order = _ctrl._Order;
            switch (arg.nStep)
            {
                case 10:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: 정위치 센서를 확인하여 PIO를 시작합니다. [SEQTIME:{arg.tSen._currBySec} sec, SENSINGTIME:{arg.tDly.nCurr} msec]");
                    break;
                case 105:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: MC→MFC = PIO Valid 신호를 확인하였습니다. [SEQTIME:{arg.tSen._currBySec} sec, SENSINGTIME:{arg.tDly.nCurr} msec]");
                    break;
                case 110:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: MC←MFC = PIO Ready 신호를 설비에게 출력합니다. [SEQTIME:{arg.tSen._currBySec} sec, SENSINGTIME:{arg.tDly.nCurr} msec]");
                    break;
                case 115:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: MC→MFC = PIO Ready 신호를 확인였습니다. [SEQTIME:{arg.tSen._currBySec} sec, SENSINGTIME:{arg.tDly.nCurr} msec]");
                    break;
                case 200:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: {(order.type == eJOBTYPE.LOADING ? "MC←MFC" : "MC→MFC")} = {order.type} 작업을 시작합니다.");
                    break;
                case 205:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: {(order.type == eJOBTYPE.LOADING ? "MC←MFC" : "MC→MFC")} = {order.type} 작업을 완료하였습니다. [SEQTIME:{arg.tSen._currBySec} sec]");
                    break;
                case 300:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: {(order.type == eJOBTYPE.LOADING ? "MC←MFC" : "MC→MFC")} = 작업을 완료하여 PIO 종료를 시작합니다.");
                    break;
                case 305:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: {(order.type == eJOBTYPE.LOADING ? "MC←MFC" : "MC→MFC")} = PIO 종료를 완료하였습니다. [SEQTIME:{arg.tSen._currBySec} sec]");
                    break;
                case 330:
                    Logger.Inst.Write(CmdLogType.prdt, $"{arg.GetID()}-{arg.nStep}: {(order.type == eJOBTYPE.LOADING ? "MC←MFC" : "MC→MFC")} = PIO 시퀀스를 종료합니다.");
                    break;
                default: break;
            }
        }

        override public void Run()
        {
            try
            {
                var order = _ctrl._Order;                
                var seqTransfer = Get(eJOBTYPE.UNLOADING == order.type ? eSEQLIST.Pick : eSEQLIST.Drop);
                switch (arg.nStep)
                {
                    case DEF_CONST.SEQ_INIT:
                        ResetTime();
                        arg.nStep = 10;
                        arg.tDly.nDelay = _Data.Inst.sys.cfg.pio.nDockSenChkTime;
                        arg.tSen.nDelay = _Data.Inst.sys.cfg.pio.nInterfaceTimeout;
                        arg.nStatus = eSTATE.Working;
                        break;
                    // + 정위치 센서확인 ===================================================
                    case 10:
                        if (true == arg.tSen.IsOver())
                        {
                            SetErr(eERROR.DockingFiled);
                        }
                        else
                        {
                            var chk = _ctrl.VEC_IN_POSOK();
                            if ( true == chk )
                            {
                                if ( true == arg.tDly.IsOver() )
                                {
                                    WriteLog();
                                    ResetTime();                                    
                                    arg.nStep = 100;                                    
                                }
                            }
                            else
                            {
                                arg.tDly.Reset();
                            }
                        }                        
                        break;
                    // - 정위치 센서확인 ===================================================

                    // + PIO 시작 시나리오 =================================================
                    case 100:                        
                        ResetTime();
                        arg.nStep = 105;
                        break;
                    case 105:
                        if (true == arg.tSen.IsOver())
                        {
                            SetErr(eERROR.PIO_Valid);
                        }
                        else
                        {
                            var chk = _ctrl.PIO_IN_Valid();
                            if (true == chk)
                            {
                                if (true == arg.tDly.IsOver())
                                {
                                    WriteLog();
                                    arg.nStep = 110;
                                }
                            }
                            else
                            {
                                arg.tDly.Reset();
                            }
                        }
                        break;
                    case 110:
                        WriteLog();
                        ResetTime();
                        arg.nStep = 115;
                        arg.tSen.nDelay = _Data.Inst.sys.cfg.pio.nFeedTimeOut_Start;
                        _ctrl.PIO_Out_Ready(true);
                        _ctrl.Job_SetState(eJOBST.Transferring);                        
                        break;
                    case 115:
                        if (true == arg.tSen.IsOver())
                        {
                            SetErr(eERROR.PIO_Ready);
                        }
                        else
                        {
                            var chk = _ctrl.PIO_IN_Ready();
                            if (true == chk)
                            {
                                if (true == arg.tDly.IsOver())
                                {
                                    WriteLog();
                                    arg.nStep = 200;
                                }
                            }
                            else
                            {
                                arg.tDly.Reset();
                            }
                        }
                        break;
                    // - PIO 시작 시나리오 =================================================

                    // + Tray 반입 반출 ====================================================
                    case 200:                        
                        WriteLog();
                        ResetTime();
                        arg.nStep = 205;
                        arg.tSen.nDelay = _Data.Inst.sys.cfg.pio.nFeedTimeOut_Work;
                        seqTransfer.WorkTrg();
                        _ctrl.PIO_Out_Ready(false);
                        _ctrl.Job_SetState(eJOBST.TransStart);
                        break;
                    case 205:
                        {
                            arg.tSen.Check();
                            var chk = seqTransfer.IsDone();
                            if (false == chk.rtn) break;
                            switch (chk.err)
                            {
                                case eERROR.None:
                                    WriteLog();
                                    arg.nStep = 300;
                                    break;
                                default: SetStop( 200); break;
                            }
                            break;
                        }
                    // - Tray 반입 반출 ====================================================

                    // + PIO 종료 시나리오 =================================================
                    case 300:
                        WriteLog();
                        ResetTime();
                        arg.nStep = 305;
                        arg.tSen.nDelay = _Data.Inst.sys.cfg.pio.nFeedTimeOut_End;
                        _ctrl.Job_SetState(eJOBST.CarrierChanged);
                        switch (order.type)
                        {                            
                            case eJOBTYPE.LOADING:
                                _ctrl.PIO_Out_Completed(true);
                                break;                            
                            default: break;
                        }
                        break;
                    case 305:
                        arg.tSen.Check();
                        if (true == arg.tSen.IsOver())
                        {
                            SetErr(eERROR.PIO_Complete);
                        }
                        else
                        {
                            var chk = _ctrl.PIO_IN_Completed();
                            if (true == chk)
                            {
                                if (true == arg.tDly.IsOver())
                                {
                                    WriteLog();
                                    arg.nStep = 310;
                                }
                            }
                            else
                            {
                                arg.tDly.Reset();
                            }
                        }
                        break;
                    case 310:
                        arg.nStep = 315;                        
                        arg.tDly.Reset();
                        switch (order.type)
                        {
                            case eJOBTYPE.UNLOADING:
                                _ctrl.PIO_Out_Completed(true);
                                break;
                            default: break;
                        }                        
                        break;
                    case 315:
                        if (false == arg.tDly.IsOver()) break;
                        arg.nStep = 320;                        
                        break;
                    case 320:
                        switch (_Data.Inst.sys.cfg.fac.seqMode)
                        {
                            case eSCENARIOMODE.PLC:
                                _ctrl.VEC_SendCmd(eVEC_CMD.PauseCancel, new SENDARG());
                                arg.nStep = 325;
                                break;
                            default: arg.nStep = 330; break;
                        }
                        break;
                    case 325:
                        if (false == _ctrl.VEC_PauseState) break;
                        arg.nStep = 330;
                        break;
                    case 330:
                        arg.tSen.Check();
                        WriteLog();
                        _ctrl.PIO_Out_Completed(false);
                        _ctrl.Job_SetState(eJOBST.TransComplete);
                        arg.nStep = 340;
                        break;
                    case 340: arg.nStep = 500; break;
                    
                    // - PIO 종료 시나리오 =================================================
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
    }
}
