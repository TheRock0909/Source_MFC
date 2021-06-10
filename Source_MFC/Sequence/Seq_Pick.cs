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
                        arg.nStep = 10;
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
