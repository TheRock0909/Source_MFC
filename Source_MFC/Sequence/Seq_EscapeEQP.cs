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
    public class Seq_EscapeEQP : _SEQBASE
    {
        public Seq_EscapeEQP(MainCtrl main)
        {
            _ctrl = main;
            arg = new TASKARG(eSEQLIST.EscapeEQP);
        }

        override public void Run()
        {
            try
            {
                var job = _ctrl._status.Order;
                var seqMode = _Data.Inst.sys.cfg.fac.seqMode;
                switch (arg.nStep)
                {
                    case DEF_CONST.SEQ_INIT:
                        ResetTime();
                        arg.nStep = 10;                        
                        arg.nStatus = eSTATE.Working;
                        break;
                    case 10:
                        var chk = _ctrl.Job_CheckTray();
                        switch (chk)
                        {
                            case eERROR.None:
                                if (arg.tSen.IsOver(_Data.Inst.sys.cfg.pio.nSenDelay))
                                {
                                    arg.nStep = 100;
                                }
                                break;                            
                            default:
                                arg.tDly.Reset();
                                if ( arg.tSen.IsOver(_Data.Inst.sys.cfg.pio.nCommTimeout) )
                                {
                                    SetErr(chk);
                                }                                
                                break;
                        }
                        break;
                    
                    case 100:
                        switch (seqMode)
                        {
                            case eSCENARIOMODE.PC: arg.nStep = 110; break;                        
                            default: arg.nStep = 500; break;
                        }
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
    }
}
