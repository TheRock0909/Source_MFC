using Source_MFC.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Sequence.SubTasks
{
    public class _TSKBASE
    {
        public MainCtrl _ctrl;
        public SUBTSKARG arg;

        virtual public void Run()
        {

        }

        virtual public bool ChkDone()
        {
            bool rtn = false;
            switch (arg.nStep)
            {
                case DEF_CONST.SEQ_FINISH: rtn = true; break;
                default:
                    if (true == arg.bStop)
                    {
                        rtn = true;
                    }
                    break;
            }
            return rtn;
        }

        public void ChkTactTime()
        {
            switch (arg.tWrk.nStart)
            {
                case 0: arg.tWrk.Reset(); break;
                default:
                    arg.tWrk.Check();
                    arg.tWrk.Reset();
                    break;
                case 500:
                    arg.nStatus = eSTATE.Done;
                    arg.nStep = DEF_CONST.SEQ_MAIN_FINISH;
                    break;
            }
        }

        public void SetErr(eERROR err)
        {
            switch (arg.nErr)
            {
                case eERROR.None:
                    arg.SetErr(err);
                    break;                
                default: break;
            }
        }

        public void Init()
        {
            arg.Init();
        }

        public void WorkTrg()
        {
            arg.WorkTrg();
        }

        public void Resume()
        {
            arg.Resume();
        }

        public void StopTrg()
        {
            arg.StopTrg();
        }

        public void ResetTime()
        {
            arg.ResetTactTime();
        }
    }
}
