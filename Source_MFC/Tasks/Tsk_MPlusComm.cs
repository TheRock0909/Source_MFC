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

        public void SendErr(eERROR err, string discr = "")
        {
            bool bSet = false;
            var Status = _ctrl._status;
            switch (_ctrl._EQPStatus)
            {
                case eEQPSATUS.Error: case eEQPSATUS.EMG: bSet = true; break;
                default: break;
            }
            int code = (int)err;
            //Queue_Add(new QUE4MP() { cmd = eMPCMD.ERROR, err = new ErrReport() { bSet = bSet, nErr = code, dscrt = discr } });
        }
    }
}
