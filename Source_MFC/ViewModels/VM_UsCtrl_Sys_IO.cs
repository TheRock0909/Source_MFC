using MaterialDesignThemes.Wpf;
using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Source_MFC.ViewModels
{
    class VM_UsCtrl_Sys_IO : Notifier
    {
        public ICommand Evt_SelectedItem { get; set; }
        public ICommand Evt_CheckSingle { get; set; }
        MainCtrl _ctrl;
        IOINFO _ioInfo;
        DispatcherTimer _tmrUpdate;
        private List<SRC4MONI> lstInputs = new List<SRC4MONI>();
        private List<SRC4MONI> lstOutputs = new List<SRC4MONI>();
        public VM_UsCtrl_Sys_IO(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _ioInfo = _Data.Inst.sys.io;            
            _ctrl.Evt_Sys_IO_DataExchange += On_DataExchange;            
            Evt_SelectedItem = new Command(On_SelectedItem);

            _tmrUpdate = new DispatcherTimer();
            _tmrUpdate.Interval = TimeSpan.FromMilliseconds(10);    //시간간격 설정
            _tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가              
        }

        ~VM_UsCtrl_Sys_IO()
        {
            _ctrl.Evt_Sys_IO_DataExchange -= On_DataExchange;
            _tmrUpdate.Tick -= Tmr_Tick;
            _tmrUpdate.Stop();
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            On_DataExchange(null, (eDATAEXCHANGE.Model2View, eUID4VM.IO_RefreshList));
        }

        private void On_DataExchange(object sender, (eDATAEXCHANGE dir, eUID4VM id) e)
        {
            switch (e.dir)
            {                
                case eDATAEXCHANGE.Model2View:
                    switch (e.id)
                    {                       
                        case eUID4VM.IO_SetList:
                            {                                                                                                
                                var listIn = _ctrl.IO_LstGet(eVIWER.IO, eIOTYPE.INPUT);
                                lstInputs.Clear();
                                foreach (IOSRC item in listIn)
                                {
                                    lstInputs.Add(new SRC4MONI() { LABEL = item.Label, STATE = item.state, _strEnum = item.name4Enum});
                                }
                                _lstInputs = new ObservableCollection<SRC4MONI>(lstInputs);

                                var listOut = _ctrl.IO_LstGet(eVIWER.IO, eIOTYPE.OUTPUT);
                                lstOutputs.Clear();
                                foreach (IOSRC item in listOut)
                                {
                                    lstOutputs.Add(new SRC4MONI() { LABEL = item.Label, STATE = item.getOutput, _strEnum = item.name4Enum });
                                }
                                _lstOutputs = new ObservableCollection<SRC4MONI>(lstOutputs);

                                b_DirectIO = _ioInfo._bDirectIO;
                                _tmrUpdate.Start();
                                break;
                            }
                        case eUID4VM.IO_RefreshList:
                            {
                                foreach (var item in lstInputs)
                                {
                                    item.STATE = _ctrl.IO_IN(item.GetIn());
                                }
                                foreach (var item in lstOutputs)
                                {
                                    item.STATE = _ctrl.IO_GETOUT(item.GetOut());
                                }                              
                                break;
                            }
                        case eUID4VM.IO_ResetDirectIO: b_DirectIO = (bool)sender; break;
                        default: break;
                    }
                    break;
                case eDATAEXCHANGE.View2Model:
                    break;                
                default: break;
            }
            OnPropertyChanged();
        }

        private void On_SelectedItem(object obj)
        {                        
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<SRC4MONI>();
            var item = collection.First();
            _ctrl.IO_OUT(item.GetOut(), !_ctrl.IO_GETOUT(item.GetOut()));
        }


        private void On_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (INotifyPropertyChanged item in e.NewItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += On_PropertyChanged;
                }
            }
            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (INotifyPropertyChanged item in e.OldItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged -= On_PropertyChanged;
                }
            }
        }

        private void On_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var row = sender as GOAL_NODE;
        }


        PackIconKind icon = PackIconKind.Output;
        public PackIconKind b_OutputIcon
        {
            get { return icon; }
            set { icon = value; OnPropertyChanged(); }
        }

        public string b_OutputTitle
        {
            get { return "OUTPUT SIGNALS"; }
            set { OnPropertyChanged(); }
        }

        
        public bool b_DirectIO
        {
            get { return _ioInfo._bDirectIO;  }
            set {
                _ioInfo._bDirectIO = value;
                OnPropertyChanged();
            }
        }

        
        private ObservableCollection<SRC4MONI> _lstInputs = new ObservableCollection<SRC4MONI>();
        public ObservableCollection<SRC4MONI> b_Inputs
        {
            get => _lstInputs; 
            set { this.MutateVerbose(ref _lstInputs, value, RaisePropertyChanged()); }
        }

        private ObservableCollection<SRC4MONI> _lstOutputs = new ObservableCollection<SRC4MONI>();
        public ObservableCollection<SRC4MONI> b_Outputs
        {
            get => _lstOutputs;            
            set {                
                this.MutateVerbose(ref _lstOutputs, value, RaisePropertyChanged());
            }
        }
    }
}
