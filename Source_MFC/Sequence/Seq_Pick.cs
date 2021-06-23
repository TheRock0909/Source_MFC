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
    public class Seq_Pick : _SEQBASE
    {
        public Seq_Pick(MainCtrl main)
        {
            _ctrl = main;
            arg = new TASKARG(eSEQLIST.Pick);
        }

        override public void Run()
        {
            try
            {
                switch (arg.nStep)
                {
                    case DEF_CONST.SEQ_INIT:
                        ResetTime();
                        arg.nStep = 10;
                        arg.tSen.nDelay = _Data.Inst.sys.cfg.pio.nCommTimeout;
                        arg.tDly.nDelay = _Data.Inst.sys.cfg.pio.nSenDelay;
                        arg.nStatus = eSTATE.Working;                        
                        break;
                    // 트래이 센서를 확인하여 감지가 되면 에러를 발생한다.
                    case 10:
                        var err = _ctrl.Job_CheckTray();
                        switch (err)
                        {
                            case eERROR.None:
                                if ( arg.tDly.IsOver() )
                                {
                                    arg.nStep = 100;
                                }                                
                                break;                            
                            default:
                                if ( arg.tSen.IsOver() )
                                {
                                    SetErr(err);
                                }
                                else
                                {
                                    arg.tDly.Reset();
                                }
                                break;
                        }                        
                        break;
                    case 100:
                        ResetTime();
                        arg.tSen.nDelay = _Data.Inst.sys.cfg.pio.nFeedTimeOut_Work;
                        ConvRun(true, false);
                        JobSetState(eJOBST.Transferring);
                        arg.nStep = 105;
                        break;
                    case 105:
                        if ( _ctrl.IO_IN(eINPUT.MFC_Pre_TrayDtc) || _ctrl.IO_IN(eINPUT.MFC_Cen_TrayDtc) || _ctrl.IO_IN(eINPUT.MFC_End_TrayDtc))
                        {
                            if ( arg.tDly.IsOver() )
                            {
                                JobSetState(eJOBST.TransStart);
                                arg.nStep = 110;
                            }
                        }
                        else
                        {
                            arg.tDly.Reset();
                            if ( arg.tSen.IsOver() )
                            {
                                SetErr(eERROR.NoneofTrays, 100);
                            }
                        }
                        break;
                    case 110:
                        if ((_ctrl.IO_IN(eINPUT.MFC_Pre_TrayDtc) && _ctrl.IO_IN(eINPUT.MFC_Cen_TrayDtc)) || _ctrl.IO_IN(eINPUT.MFC_End_TrayDtc))
                        {
                            if (arg.tDly.IsOver())
                            {
                                ConvRun(true, true);
                                JobSetState(eJOBST.CarrierChanged);
                                arg.nStep = 120;
                            }
                        }
                        else
                        {
                            arg.tDly.Reset();
                            if (arg.tSen.IsOver())
                            {
                                SetErr(eERROR.TraysJaming, 100);
                            }
                        }
                        break;
                    case 120:
                        if (_ctrl.IO_IN(eINPUT.MFC_Pre_TrayDtc) && _ctrl.IO_IN(eINPUT.MFC_Cen_TrayDtc) && _ctrl.IO_IN(eINPUT.MFC_End_TrayDtc))
                        {
                            if (arg.tDly.IsOver())
                            {
                                ConvRun(false, false);
                                arg.nStep = 130;
                            }
                        }
                        else
                        {
                            arg.tDly.Reset();
                            if (arg.tSen.IsOver())
                            {
                                SetErr(eERROR.TraysJaming, 100);
                            }
                        }
                        break;
                    case 130:
                        JobSetState(eJOBST.TransComplete);
                        arg.nStep = 140;
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

        override public void SetErr(eERROR err, int nStep = DEF_CONST.SEQ_FINISH)
        {
            ConvRun(false, false);
            switch (arg.nErr)
            {
                case eERROR.None:
                    arg.SetErr(err);
                    switch (nStep)
                    {
                        case DEF_CONST.SEQ_FINISH: break;
                        default: arg.nStep = nStep; break;
                    }
                    break;
                default: break;
            }
        }

        private void JobSetState(eJOBST state)
        {
            var job = _ctrl._Order;
            if (true == job.opt.bSkipPIO)
            {
                _ctrl.Job_SetState(state);
            }
        }

        private void ConvRun(bool bTrg, bool bIsSlow)
        {
            _ctrl.CONV_SetSpd(bIsSlow ? _Data.Inst.sys.cfg.pio.nConvSpd_Slow : _Data.Inst.sys.cfg.pio.nConvSpd_Normal);
            _ctrl.CONV_Motor(bTrg, true);
        }        
    }
}
