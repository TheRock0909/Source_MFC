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

    }
}
