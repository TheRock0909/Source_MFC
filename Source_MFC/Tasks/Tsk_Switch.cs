using Source_MFC.Global;
using Source_MFC.Sequence.SubTasks;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Tasks
{
    public class Tsk_Switch : _TSKBASE
    {
        class CHKERARG
        {
            MainCtrl ctrl;
            public eINPUT eID;
            public eERROR eErr;
            public TIMEARG tSen;
            public int nDelay;
            public bool bEnb;
            public bool bAxs;
            public CHKERARG(MainCtrl main, int delay, bool IsAxs = false)
            {
                ctrl = main;
                nDelay = delay;
                eID = eINPUT.MFC_EMG_UP;
                eErr = eERROR.None;
                bEnb = true;
                tSen = new TIMEARG();
                tSen.Reset();
                bAxs = IsAxs;
            }

            public void Reset()
            {
                tSen.Reset();
            }

            public bool ChkIN()
            {
                if (true == ctrl.IO_IN(eID))
                {
                    return (true == tSen.IsOver(nDelay)) ? true : false;
                }
                else
                {
                    Reset();
                    return false;
                }
            }
        }

        List<CHKERARG> _lstSwitch = new List<CHKERARG>();
        List<CHKERARG> _lstEMG = new List<CHKERARG>();
        public Tsk_Switch(MainCtrl main)
        {
            _ctrl = main;
            arg = new SUBTSKARG(eTASKLIST.SWITCH);
            var sendelay = (0 == _Data.Inst.sys.cfg.pio.nSenDelay) ? DEF_CONST.SENTIME : _Data.Inst.sys.cfg.pio.nSenDelay;
            _lstSwitch.Clear();
            _lstSwitch.Add(new CHKERARG(_ctrl, sendelay) { eID = eINPUT.MFC_START, eErr = eERROR.None, tSen = new TIMEARG(), bEnb = true });
            _lstSwitch.Add(new CHKERARG(_ctrl, sendelay) { eID = eINPUT.MFC_STOP, eErr = eERROR.None, tSen = new TIMEARG(), bEnb = true });
            _lstSwitch.Add(new CHKERARG(_ctrl, sendelay) { eID = eINPUT.MFC_RESET, eErr = eERROR.None, tSen = new TIMEARG(), bEnb = true });
            _lstSwitch.Add(new CHKERARG(_ctrl, sendelay) { eID = eINPUT.MFC_AUTO_MANUAL, eErr = eERROR.None, tSen = new TIMEARG(), bEnb = true });

            _lstEMG.Clear();
            _lstEMG.Add(new CHKERARG(_ctrl, sendelay) { eID = eINPUT.MFC_EMG_UP, eErr = eERROR.EMG, tSen = new TIMEARG(), bEnb = true });
            _lstEMG.Add(new CHKERARG(_ctrl, sendelay) { eID = eINPUT.MFC_EMG_LD, eErr = eERROR.EMG, tSen = new TIMEARG(), bEnb = true });
        }

        private void ChkSwitch()
        {
            switch (_ctrl._EQPStatus)
            {
                case eEQPSATUS.Stop:
                    {
                        var startSw = _lstSwitch.Single(s => s.eID == eINPUT.MFC_START);
                        if (true == startSw.ChkIN())
                        {
                            _ctrl.IO_OUT(eOUTPUT.MFC_SWLMP_START, true);
                            switch (_ctrl.CurrView)
                            {
                                case eVIWER.Monitor: _ctrl.Start(CHANGEMODEBY.SWITCH); break;                                
                                default: Logger.Inst.Write(CmdLogType.prdt, $"{eVIWER.Monitor}으로 변경해 주세요!!![CURR:{_ctrl.CurrView}]"); break;
                            }
                        }
                        else
                        {
                            _ctrl.IO_OUT(eOUTPUT.MFC_SWLMP_START, false);
                        }
                        break;
                    }
                case eEQPSATUS.Idle:
                case eEQPSATUS.Run:
                case eEQPSATUS.Error:
                case eEQPSATUS.EMG:
                    {
                        var stopSw = _lstSwitch.Single(s => s.eID == eINPUT.MFC_STOP);
                        if (true == stopSw.ChkIN())
                        {
                            _ctrl.IO_OUT(eOUTPUT.MFC_SWLMP_STOP, true);
                            switch (_ctrl.CurrView)
                            {
                                case eVIWER.Monitor: _ctrl.Stop(CHANGEMODEBY.SWITCH); break;
                                default: Logger.Inst.Write(CmdLogType.prdt, $"{eVIWER.Monitor}으로 변경해 주세요!!![CURR:{_ctrl.CurrView}]"); break;
                            }
                        }
                        else
                        {
                            _ctrl.IO_OUT(eOUTPUT.MFC_SWLMP_STOP, false);
                        }
                        break;
                    }
                default: break;
            }
            var resetSw = _lstSwitch.Single(s => s.eID == eINPUT.MFC_RESET);
            if (true == resetSw.ChkIN())
            {
                _ctrl.IO_OUT(eOUTPUT.MFC_SWLMP_RESET, true);
                _ctrl.Reset(CHANGEMODEBY.SWITCH);
            }
            else
            {
                _ctrl.IO_OUT(eOUTPUT.MFC_SWLMP_RESET, false);
            }
            _ctrl._status.bIsManual = !_ctrl.IO_IN(eINPUT.MFC_AUTO_MANUAL);
        }

        private void ChkEQPEMG()
        {
            switch (_ctrl._EQPStatus)
            {
                case eEQPSATUS.Error: case eEQPSATUS.EMG: case eEQPSATUS.Init: break;
                default:
                    {
                        foreach (var item in _lstEMG)
                        {                            
                            if (true == item.ChkIN() && true == item.bEnb)
                            {
                                _ctrl.SetErr(item.eErr, CHANGEMODEBY.PROCESS);
                            }
                        }
                        break;
                    }
            }
        }

        override public void Run()
        {
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
                        arg.tDly.nDelay = _Data.Inst.sys.cfg.pio.nSenDelay;
                        arg.tSen.nDelay = _Data.Inst.sys.lmp.blinkTime;
                        arg.ResetTactTime();
                        break;
                    case 100:
                        if (eCOMSTATUS.DISCONNECTED == _ctrl._status.devsCont.io) break;
                        ChkEQPEMG();
                        ChkSwitch();
                        if (arg.tSen.IsOver())
                        {
                            arg.bTrg = !arg.bTrg;
                        }
                        if ( false == _Data.Inst.sys.io._bDirectIO )
                        {
                            var lmp = _Data.Inst.sys.lmp.GetLmp(_ctrl._EQPStatus);
                            switch (lmp.Green)
                            {
                                case TWRLAMP.OFF: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_GREEN, false); break;
                                case TWRLAMP.ON: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_GREEN, true); break;
                                case TWRLAMP.BLINK: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_GREEN, arg.bTrg ? true : false); break;
                            }
                            switch (lmp.Yellow)
                            {
                                case TWRLAMP.OFF: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_YELLOW, false); break;
                                case TWRLAMP.ON: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_YELLOW, true); break;
                                case TWRLAMP.BLINK: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_YELLOW, arg.bTrg ? true : false); break;
                            }
                            switch (lmp.Red)
                            {
                                case TWRLAMP.OFF: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_RED, false); break;
                                case TWRLAMP.ON: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_RED, true); break;
                                case TWRLAMP.BLINK: _ctrl.IO_OUT(eOUTPUT.MFC_SIGNAL_LAMP_RED, arg.bTrg ? true : false); break;
                            }
                        }                        
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
    }
}
