using Source_MFC.Global;
using Source_MFC.Sequence.SubTasks;
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
    public class VM_UsrCtrl_TaskMoni : Notifier
    {
        MainCtrl _ctrl;
        DispatcherTimer _tmrUpdate;
        private List<SRC4ARG> lstTsks = new List<SRC4ARG>();
        public VM_UsrCtrl_TaskMoni(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _ctrl.Evt_TskMoni_DataExchange += On_DataExchange;
            _tmrUpdate = new DispatcherTimer();
            _tmrUpdate.Interval = TimeSpan.FromMilliseconds(500);    //시간간격 설정
            _tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가            
        }

        ~VM_UsrCtrl_TaskMoni()
        {
            _ctrl.Evt_TskMoni_DataExchange -= On_DataExchange;
            _tmrUpdate.Tick -= Tmr_Tick;
            _tmrUpdate.Stop();
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            On_DataExchange(this, eDATAEXCHANGE.StatusUpdate);
        }

        private SRC4ARG Get(_TSKBASE seq)
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
                    lstTsks.Clear();
                    foreach (eTASKLIST item in Enum.GetValues(typeof(eTASKLIST)))
                    {
                        if (eTASKLIST.MAX_SUB_SEQ == item) continue;
                        var seq = _ctrl.Tsk_Get(item);
                        lstTsks.Add(Get(seq));
                    }
                    b_Tsks = new ObservableCollection<SRC4ARG>(lstTsks);
                    _tmrUpdate.Start();
                    break;
                case eDATAEXCHANGE.View2Model:
                    break;
                case eDATAEXCHANGE.StatusUpdate:
                    if (true == _ctrl._status.bLoaded)
                    {                        
                        foreach (var item in lstTsks)
                        {
                            var seq = _ctrl.Tsk_Get(item.GetTskID());
                            var arg = seq.arg;
                            item.STATE = arg.nStatus.ToString();
                            item.STEP = arg.nStep.ToString();
                            item.ERROR = arg.nErr.ToString();
                            item.STOP = arg.bStop;
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

        private ObservableCollection<SRC4ARG> _lstTsks = new ObservableCollection<SRC4ARG>();
        public ObservableCollection<SRC4ARG> b_Tsks
        {
            get => _lstTsks;
            set { this.MutateVerbose(ref _lstTsks, value, RaisePropertyChanged()); }
        }
    }
}
