using EzVehicle.Ctrl;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Source_MFC.ViewModels
{
    class VM_UsCtrl_Dash_Moni : Notifier
    {
        MainCtrl _ctrl;
        STATUS _Status;
        public ICommand Evt_CmdBtnClicked { get; set; }
        public ICommand Evt_DismissComand { get; set; }
        public VM_UsCtrl_Dash_Moni(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _Status = _ctrl._status;
            _ctrl.Evt_Dash_Moni_DataExchange += On_DataExchange;
            Evt_CmdBtnClicked = new Command(On_CmdBtnClicked, CanExecute);
            Evt_DismissComand = new Command(On_DismissComand);            
        }

        private void On_DataExchange(object sender, (eDATAEXCHANGE dir, eUID4VM id) e)
        {
            switch (e.dir)
            {                
                case eDATAEXCHANGE.Model2View:
                    {
                        switch (e.id)
                        {
                            case eUID4VM.DASH_MONI_ALL:
                                {
                                    var status = sender as STATUS;
                                    b_EQPState = status.eqpState.ToString();
                                    UpdateManualMode(status.bIsManual);
                                    UpdateSOC(status.vecState.soc);
                                    UpdateVecStatus(status.vecState.state);
                                    switch (_Data.Inst.sys.cfg.fac.eqpType)
                                    {
                                        case eEQPTYPE.MFC:
                                            b_lstInput = new ObservableCollection<IOSRC>() ;
                                            b_InputSrc = new CollectionViewSource();
                                            b_lstInput.CollectionChanged += On_CollectionChanged;
                                            var listIn = _ctrl.IO_LstGet(eVIWER.Monitor, eIOTYPE.INPUT);
                                            b_lstInput.Clear();
                                            foreach (IOSRC item in listIn)
                                            {
                                                b_lstInput.Add(item);
                                            }
                                            b_InputSrc.Source = b_lstInput;

                                            b_lstOutput = new ObservableCollection<IOSRC>();
                                            b_OutputSrc = new CollectionViewSource();
                                            b_lstOutput.CollectionChanged += On_CollectionChanged;
                                            var listOut = _ctrl.IO_LstGet(eVIWER.Monitor, eIOTYPE.OUTPUT);
                                            b_lstOutput.Clear();
                                            foreach (IOSRC item in listOut)
                                            {
                                                b_lstOutput.Add(item);
                                            }
                                            b_OutputSrc.Source = b_lstOutput;
                                            break;
                                        default: break;
                                    }
                                    break;
                                }
                            case eUID4VM.DASH_MONI_EqpState: b_EQPState = $"{(eEQPSATUS)sender}"; break;
                            case eUID4VM.DASH_MONI_EqpMode: UpdateManualMode((bool)sender); break;
                            case eUID4VM.DASH_MONI_VECSTATE:
                                {
                                    var status = sender as STATUS;
                                    UpdateSOC(status.vecState.soc);
                                    UpdateVecStatus((EzVehicle.Ctrl.eSTATE)sender);
                                    break;
                                }
                            case eUID4VM.DASH_MONI_IO:
                                {
                                    var io = sender as IOINFO;
                                    foreach (IOSRC item in b_lstInput)
                                    {
                                        item.state = io.Get((eINPUT)item.eID).state;
                                    }
                                    b_InputSrc.View.Refresh();
                                    foreach (IOSRC item in b_lstOutput)
                                    {
                                        item.getOutput = io.Get((eOUTPUT)item.eID).getOutput;
                                    }
                                    b_OutputSrc.View.Refresh();
                                    switch (_Data.Inst.sys.cfg.fac.eqpType)
                                    {                                        
                                        case eEQPTYPE.MFC: b_AlignSen = _ctrl.IO_IN(eINPUT.MFC_POS_OK); break;                                        
                                        default: break;
                                    }                                    
                                    break;
                                }
                            case eUID4VM.DASH_MONI_START:                                
                                break;
                            case eUID4VM.DASH_MONI_STOP:
                                break;
                            case eUID4VM.DASH_MONI_RESET:
                                break;
                            case eUID4VM.DASH_MONI_DROPJOB:
                                break;
                            default: break;
                        }
                        break;
                    }
                case eDATAEXCHANGE.View2Model:
                    {
                        switch (e.id)
                        {
                            case eUID4VM.DASH_MONI_ALL:
                                break;
                            case eUID4VM.DASH_MONI_EqpState:

                                break;
                            case eUID4VM.DASH_MONI_EqpMode:
                                break;
                            case eUID4VM.DASH_MONI_VECSTATE:
                                break;
                            case eUID4VM.DASH_MONI_IO:
                                break;
                            case eUID4VM.DASH_MONI_START:                                
                                break;
                            case eUID4VM.DASH_MONI_STOP:
                                break;
                            case eUID4VM.DASH_MONI_RESET:
                                break;
                            case eUID4VM.DASH_MONI_DROPJOB:
                                break;
                            default: break;
                        }
                        break;
                    }
                default: break;
            }            
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

        private void On_DismissComand(object obj)
        {

        }

        private void On_CmdBtnClicked(object obj)
        {
            eUID4VM uid = (eUID4VM)Convert.ToInt32(obj);
            switch (uid)
            {
                case eUID4VM.DASH_MONI_START:                    
                case eUID4VM.DASH_MONI_STOP:                    
                case eUID4VM.DASH_MONI_RESET:                    
                case eUID4VM.DASH_MONI_DROPJOB:
                    On_DataExchange(obj, (eDATAEXCHANGE.View2Model, uid));
                    break;                
            }
        }

        public bool CanExecute(object parameter)
        {
            eUID4VM uid = (eUID4VM)Convert.ToInt32(parameter);
            switch (_Status.eqpState)
            {
                case eEQPSATUS.Init:
                case eEQPSATUS.Stopping: return false;                    
                case eEQPSATUS.Initing:
                case eEQPSATUS.Idle:                    
                case eEQPSATUS.Run:                
                    switch (uid)
                    {
                        case eUID4VM.DASH_MONI_START:
                        case eUID4VM.DASH_MONI_RESET:       return false;                            
                        case eUID4VM.DASH_MONI_STOP:        return true;
                        case eUID4VM.DASH_MONI_DROPJOB:
                            switch (_Status.eqpState)
                            {
                                case eEQPSATUS.Initing:
                                case eEQPSATUS.Idle:        return false;                                                                    
                                default:                    return true;
                            }                            
                    }
                    break;
                
                case eEQPSATUS.Stop:
                    switch (uid)
                    {
                        case eUID4VM.DASH_MONI_START: return true;
                        case eUID4VM.DASH_MONI_STOP:                            
                        case eUID4VM.DASH_MONI_RESET: return false;
                        case eUID4VM.DASH_MONI_DROPJOB:
                            switch (_Status.vecState.JobState)
                            {
                                case RobotState.None:                                
                                case RobotState.NotAssigned: return false;
                                default: return true;
                            }                            
                    }
                    break;
                case eEQPSATUS.Error:                    
                case eEQPSATUS.EMG:
                    switch (uid)
                    {
                        case eUID4VM.DASH_MONI_RESET: return true;
                        default: return false;
                    }
                default: return false;
            }
            return false;
        }

        private void UpdateManualMode(bool bMode)
        {
            b_IconManualMode = (true == bMode) ? PackIconKind.CarManualTransmission : PackIconKind.AutoAwesomeMotion ;
            b_ManualMode = (true == bMode) ? "MANUAL" : "AUTO";
        }

        private void UpdateSOC(int soc)
        {
            if ( 60 < soc && 100 >= soc )
            {
                b_ForeGroundSOC = new SolidColorBrush(Colors.Green);
            }
            else if ( 60 > soc && 30 < soc )
            {
                b_ForeGroundSOC = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                b_ForeGroundSOC = new SolidColorBrush(Colors.Tomato);
            }
            b_VecSOC = $"{soc} %";
        }

        private void UpdateVecStatus(EzVehicle.Ctrl.eSTATE st)
        {
            switch (st)
            {
                case EzVehicle.Ctrl.eSTATE.IDLE_PROCESSING:
                case EzVehicle.Ctrl.eSTATE.NONE:
                    b_ForeGroundVecState = new SolidColorBrush(Colors.DimGray);
                    IconVecState = PackIconKind.NotificationsNone;
                    break;

                case EzVehicle.Ctrl.eSTATE.STOPPING:
                case EzVehicle.Ctrl.eSTATE.GOING_TO:
                case EzVehicle.Ctrl.eSTATE.GOING_TO_POINT:
                case EzVehicle.Ctrl.eSTATE.GOING_TO_DOCK_AT:
                case EzVehicle.Ctrl.eSTATE.DRIVING:
                case EzVehicle.Ctrl.eSTATE.DRIVING_INTO_DOCK:
                case EzVehicle.Ctrl.eSTATE.TELEOP_DRIVING:
                case EzVehicle.Ctrl.eSTATE.MOVING:
                case EzVehicle.Ctrl.eSTATE.DOING_TASK_DELTAHEADING:
                case EzVehicle.Ctrl.eSTATE.DOING_TASK_MOVE:
                case EzVehicle.Ctrl.eSTATE.DOING_TASK_PAUSE:
                case EzVehicle.Ctrl.eSTATE.DONE_DRIVING:
                case EzVehicle.Ctrl.eSTATE.DOCKING:                    
                case EzVehicle.Ctrl.eSTATE.UNDOCKING:
                    b_ForeGroundVecState = new SolidColorBrush(Colors.Green);
                    IconVecState = PackIconKind.ShoeRunning;
                    break;

                case EzVehicle.Ctrl.eSTATE.FAILED_GOING_TO:
                case EzVehicle.Ctrl.eSTATE.ESTOP_PRESSED:
                case EzVehicle.Ctrl.eSTATE.ESTOP_RELIEVED:
                case EzVehicle.Ctrl.eSTATE.CANNOT_FIND_PATH:
                case EzVehicle.Ctrl.eSTATE.NO_ENTER:
                    b_ForeGroundVecState = new SolidColorBrush(Colors.Tomato);
                    IconVecState = PackIconKind.Error;
                    break;
               
                default:
                    b_ForeGroundVecState = new SolidColorBrush(Colors.Black);
                    IconVecState = PackIconKind.Stop;
                    break;
            }
            b_VecState = $"{st}";
        }

        string EQPState = string.Empty;
        public string b_EQPState
        {
            get { return EQPState; }
            set { EQPState = value; OnPropertyChanged("b_EQPState"); }
        }

        PackIconKind IconManualMode = PackIconKind.Stop;
        public PackIconKind b_IconManualMode
        {
            get { return IconManualMode; }
            set { IconManualMode = value; OnPropertyChanged("b_ManualModeIcon"); }
        }

        string ManualMode = string.Empty;
        public string b_ManualMode
        {
            get { return ManualMode; }
            set { ManualMode = value; OnPropertyChanged("b_ManualMode"); }
        }

        string VecSOC = string.Empty;
        public string b_VecSOC
        {
            get { return VecSOC; }
            set { VecSOC = value; OnPropertyChanged("b_VecSOC"); }
        }

        SolidColorBrush ForeGroundSOC;
        public SolidColorBrush b_ForeGroundSOC
        {
            get { return ForeGroundSOC; }
            set { ForeGroundSOC = value; OnPropertyChanged("b_ForeGroundSOC"); }
        }

        PackIconKind IconVecState = PackIconKind.Stop;
        public PackIconKind b_IconVecState
        {
            get { return IconVecState; }
            set { IconVecState = value; OnPropertyChanged("b_ManualModeIcon"); }
        }

        string VecState = string.Empty;
        public string b_VecState
        {
            get { return VecState; }
            set { VecState = value; OnPropertyChanged("b_VecState"); }
        }

        SolidColorBrush ForeGroundVecState;
        public SolidColorBrush b_ForeGroundVecState
        {
            get { return ForeGroundVecState; }
            set { ForeGroundVecState = value; OnPropertyChanged("b_ForeGroundVecState"); }
        }

        string JobID = string.Empty;
        public string b_JobID
        {
            get { return JobID; }
            set { JobID = value; OnPropertyChanged("b_JobID"); }
        }

        string JobType = string.Empty;
        public string b_JobType
        {
            get { return JobType; }
            set { JobType = value; OnPropertyChanged("b_JobType"); }
        }

        string TrayID = string.Empty;
        public string b_TrayID
        {
            get { return TrayID; }
            set { TrayID = value; OnPropertyChanged("b_TrayID"); }
        }        

        string Dest = string.Empty;
        public string b_Dest
        {
            get { return Dest; }
            set { Dest = value; OnPropertyChanged("b_Dest"); }
        }

        string JobState = string.Empty;
        public string b_JobState
        {
            get { return JobState; }
            set { JobState = value; OnPropertyChanged("b_JobState"); }
        }

        string AIVState = string.Empty;
        public string b_AIVState
        {
            get { return AIVState; }
            set { AIVState = value; OnPropertyChanged("b_AIVState"); }
        }

        bool AlignSen = false;
        public bool b_AlignSen
        {
            get { return AlignSen; }
            set {
                if (value == AlignSen) return;
                AlignSen = value; OnPropertyChanged("b_AlignSen");
            }
        }

        string PauseState = string.Empty;
        public string b_PauseState
        {
            get { return PauseState; }
            set { PauseState = value; OnPropertyChanged("b_PauseState"); }
        }

        public CollectionViewSource b_InputSrc { get; set; }
        private ObservableCollection<IOSRC> lst_Input;
        private ObservableCollection<IOSRC> b_lstInput
        {
            get {
                return lst_Input;
            }
            set {
                if (value == lst_Input) return;
                lst_Input = value;
                OnPropertyChanged("b_lstInput");
            }
        }

        public CollectionViewSource b_OutputSrc { get; set; }
        private ObservableCollection<IOSRC> lst_Output ;
        private ObservableCollection<IOSRC> b_lstOutput
        {
            get {
                return lst_Output;
            }
            set {
                if (value == lst_Output) return;
                lst_Output = value;
                OnPropertyChanged("b_lstOutput");
            }
        }

    }
}
