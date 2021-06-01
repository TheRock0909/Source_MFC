using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Source_MFC.ViewModels
{
    class VM_UsCtrl_Sys_IO : Notifier
    {
        public ICommand Evt_SelectedItem { get; set; }

        MainCtrl _ctrl;
        public VM_UsCtrl_Sys_IO(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _ctrl.Evt_Sys_IO_DataExchange += On_DataExchange;            
            Evt_SelectedItem = new Command(On_SelectedItem);
        }

        private void On_DataExchange(object sender, (eDATAEXCHANGE dir, eUID4VM id) e)
        {
            switch (e.dir)
            {                
                case eDATAEXCHANGE.Model2View:
                    switch (e.id)
                    {                       
                        case eUID4VM.IO_SetList:                            
                            b_lstInput = new ObservableCollection<IOSRC>();
                            b_InputSrc = new CollectionViewSource();
                            b_lstInput.CollectionChanged += On_CollectionChanged;
                            var listIn = _ctrl.IO_LstGet(eVIWER.IO, eIOTYPE.INPUT);
                            b_lstInput.Clear();
                            foreach (IOSRC item in listIn)
                            {
                                b_lstInput.Add(item);
                            }
                            b_InputSrc.Source = b_lstInput;

                            b_lstOutput = new ObservableCollection<IOSRC>();
                            b_OutputSrc = new CollectionViewSource();
                            b_lstOutput.CollectionChanged += On_CollectionChanged;
                            var listOut = _ctrl.IO_LstGet(eVIWER.IO, eIOTYPE.OUTPUT);
                            b_lstOutput.Clear();
                            foreach (IOSRC item in listOut)
                            {
                                b_lstOutput.Add(item);
                            }
                            b_OutputSrc.Source = b_lstOutput;
                            break;                        
                        default:
                            break;
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
            var collection = items.Cast<IOSRC>();
            var item = collection.First();
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

        public CollectionViewSource b_InputSrc { get; set; }
        private ObservableCollection<IOSRC> lst_Input = new ObservableCollection<IOSRC>();
        private ObservableCollection<IOSRC> b_lstInput
        {
            get {
                return lst_Input;
            }
            set {
                if (value == lst_Input) return;
                lst_Input = value;
                OnPropertyChanged();
            }
        }

        public CollectionViewSource b_OutputSrc { get; set; }
        private ObservableCollection<IOSRC> lst_Output = new ObservableCollection<IOSRC>();        
        private ObservableCollection<IOSRC> b_lstOutput
        {
            get {
                return lst_Output;
            }
            set {
                if (value == lst_Output) return;
                lst_Output = value;
                OnPropertyChanged();
            }
        }
    }
}
