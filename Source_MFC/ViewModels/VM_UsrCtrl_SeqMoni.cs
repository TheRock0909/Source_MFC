using Source_MFC.Global;
using Source_MFC.Sequence.MainTasks;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Source_MFC.ViewModels
{
    public class VM_UsrCtrl_SeqMoni : Notifier
    {
        MainCtrl _ctrl;
        DispatcherTimer _tmrUpdate;
        private List<SRC4ARG> lstSeqs = new List<SRC4ARG>();
        public VM_UsrCtrl_SeqMoni(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _ctrl.Evt_SeqMoni_DataExchange += On_DataExchange;
            _tmrUpdate = new DispatcherTimer();
            _tmrUpdate.Interval = TimeSpan.FromMilliseconds(500);    //시간간격 설정
            _tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가                 
        }

        ~VM_UsrCtrl_SeqMoni()
        {
            _ctrl.Evt_SeqMoni_DataExchange -= On_DataExchange;
            _tmrUpdate.Tick -= Tmr_Tick;
            _tmrUpdate.Stop();
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            On_DataExchange(this, eDATAEXCHANGE.StatusUpdate);
        }

        private SRC4ARG Get(_SEQBASE seq)
        {
            var arg = seq.arg;
            return new SRC4ARG()
            {
                ID = arg.GetID().ToString()
              , STATE = arg.nStatus.ToString()
              , STEP = arg.nStep.ToString()
              , ERROR = arg.nErr.ToString()
              , STOP = arg.bStop
              , RESULT = arg.result.ToString()
              , TRIGGER = arg.nTrg.ToString()
              , SENSING = arg.tSen._currBySec.ToString()
              , DELAY = arg.tDly._currBySec.ToString()
            };
        }
        
        private void On_DataExchange(object sender, eDATAEXCHANGE dir)
        {
            switch (dir)
            {
                case eDATAEXCHANGE.Model2View:
                    lstSeqs.Clear();
                    foreach (eSEQLIST item in Enum.GetValues(typeof(eSEQLIST)))
                    {
                        if (eSEQLIST.MAX_SEQ == item) continue;
                        var seq = _ctrl.Seq_Get(item);
                        lstSeqs.Add(Get(seq));
                    }
                    b_Seqs = new ObservableCollection<SRC4ARG>(lstSeqs);
                    _tmrUpdate.Start();
                    break;
                case eDATAEXCHANGE.View2Model:
                    break;
                case eDATAEXCHANGE.StatusUpdate:
                    if ( true == _ctrl._status.bLoaded )
                    {
                        foreach (var item in lstSeqs)
                        {
                            var seq = _ctrl.Seq_Get(item.GetSeqID());
                            var arg = seq.arg;
                            item.STATE = arg.nStatus.ToString();
                            item.STEP = arg.nStep.ToString();
                            item.ERROR = arg.nErr.ToString();
                            item.STOP = arg.bStop ;
                            item.RESULT = arg.result.ToString();
                            item.TRIGGER = arg.nTrg.ToString();
                            item.SENSING = arg.tSen._currBySec.ToString();
                            item.DELAY = arg.tDly._currBySec.ToString();
                        }
                    }
                    break;
                default: break;
            }
        }

        private ObservableCollection<SRC4ARG> _lstSeqs = new ObservableCollection<SRC4ARG>();
        public ObservableCollection<SRC4ARG> b_Seqs
        {
            get => _lstSeqs;
            set { this.MutateVerbose(ref _lstSeqs, value, RaisePropertyChanged()); }
        }
    }
}
