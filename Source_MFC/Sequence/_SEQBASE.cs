using Source_MFC.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Sequence.MainTasks
{
    public class _SEQBASE
    {
        public MainCtrl _ctrl;
        public TASKARG arg;
        public List<eTASKLIST> subTsks = new List<eTASKLIST>();

        virtual public void Run()
        {

        }

        public (bool rtn, eERROR err) IsDone()
        {
            bool rtn = false;
            eERROR eErr = eERROR.None;
            switch (arg.nStep)
            {
                case DEF_CONST.SEQ_FINISH: rtn = true; eErr = arg.nErr; break;
                default:
                    switch (arg.nErr)
                    {
                        case eERROR.None: break;
                        default: rtn = true; eErr = arg.nErr; break;
                    }
                    break;
            }
            return (rtn, eErr);
        }

        virtual public bool IsCanStop()
        {
            return false;
        }

        public _SEQBASE Get(eSEQLIST id)
        {
            return _ctrl.Seq_Get(id);            
        }

        virtual public void SetErr(eERROR err, int nStep = DEF_CONST.SEQ_FINISH)
        {
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

        public void SetStop(int nStep = DEF_CONST.SEQ_FINISH)
        {
            arg.bStop = true;
            switch (nStep)
            {
                case DEF_CONST.SEQ_FINISH: break;
                default: arg.nStep = nStep; break;
            }
        }

        public void WorkTrg()
        {
            arg.WorkTrg();
        }

        public void ResetTime()
        {
            arg.ResetTime();
        }


        virtual public void ChkSeqStop()
        {
            switch (_ctrl._EQPStatus)
            {                
                case eEQPSATUS.Stopping:
                    switch (arg.nStep)
                    {
                        case DEF_CONST.SEQ_FINISH:
                            if (false == arg.bStop)
                            {
                                arg.bStop = true;
                            }
                            break;
                        default: break;
                    }
                    break;                
                default: break;
            }
        }

    }
}
