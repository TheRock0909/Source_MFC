using MaterialDesignThemes.Wpf;
using Source_MFC.Global;
using Source_MFC.HW.MobileRobot.LD;
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
using System.Windows.Threading;

namespace Source_MFC.ViewModels
{
    class VM_UsCtrl_Dash_Moni : Notifier
    {
        MainCtrl _ctrl;
        STATUS _Status;
        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        public ICommand Evt_CmdBtnClicked { get; set; }
        public ICommand Evt_DismissComand { get; set; }
        DispatcherTimer _tmrUpdate;
        private List<SRC4MONI> lstInputs = new List<SRC4MONI>();
        private List<SRC4MONI> lstOutputs = new List<SRC4MONI>();

        public VM_UsCtrl_Dash_Moni(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _Status = _ctrl._status;
            _ctrl.Evt_Dash_Moni_DataExchange += On_DataExchange;
            Evt_CmdBtnClicked = new Command(On_CmdBtnClicked, CanExecute);
            Evt_DismissComand = new Command(On_DismissComand);

            _tmrUpdate = new DispatcherTimer();
            _tmrUpdate.Interval = TimeSpan.FromMilliseconds(10);    //시간간격 설정
            _tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가              
        }

        ~VM_UsCtrl_Dash_Moni()
        {
            _ctrl.Evt_Dash_Moni_DataExchange -= On_DataExchange;
            _tmrUpdate.Tick -= Tmr_Tick;
            _tmrUpdate.Stop();
            b_lstInput = null;
            b_lstOutput = null;
        }

        private void _Finalize()
        {            
                  
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {            
            UpdateSOC(_Status.vecState.soc);
            UpdateVecStatus(_Status.vecState.state);
            UpdateManualMode(_Status.bIsManual);
            On_DataExchange(null, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_IO));
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
                                    var listIn = _ctrl.IO_LstGet(eVIWER.Monitor, eIOTYPE.INPUT);
                                    lstInputs.Clear();
                                    foreach (IOSRC item in listIn)
                                    {                                        
                                        lstInputs.Add(new SRC4MONI() { LABEL = item.Label, STATE = item.state, _strEnum = item.name4Enum });
                                    }
                                    _inputs = new ObservableCollection<SRC4MONI>(lstInputs);
                                    
                                    lstOutputs.Clear();
                                    var listOut = _ctrl.IO_LstGet(eVIWER.Monitor, eIOTYPE.OUTPUT);
                                    foreach (IOSRC item in listOut)
                                    {
                                        lstOutputs.Add(new SRC4MONI() { LABEL = item.Label, STATE = item.getOutput, _strEnum = item.name4Enum });
                                    }
                                    _Output = new ObservableCollection<SRC4MONI>(lstOutputs);

                                    On_DataExchange(null, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MONI_JOB_Reset));
                                    _tmrUpdate.Start();
                                    break;
                                }
                            case eUID4VM.DASH_MONI_EqpState:
                                b_EQPState = $"{_ctrl._EQPStatus}";
                                break;
                            case eUID4VM.DASH_MONI_VECSTATE:
                                {
                                    break;
                                }
                            case eUID4VM.DASH_MONI_IO:
                                {
                                    foreach (var item in lstInputs)
                                    {
                                        item.STATE = _ctrl.IO_IN(item.GetIn());
                                    }                             
                                    
                                    foreach (var item in lstOutputs)
                                    {
                                        item.STATE = _ctrl.IO_GETOUT(item.GetOut());
                                    }

                                    switch (_Data.Inst.sys.cfg.fac.eqpType)
                                    {
                                        case eEQPTYPE.MFC: b_jobMoni.SetPosGoodSen(_ctrl.IO_IN(eINPUT.MFC_POS_OK)); break;
                                        default: break;
                                    }
                                    break;
                                }
                            case eUID4VM.DASH_MONI_JOB_Assigned:
                            case eUID4VM.DASH_MONI_JOB_Reset:
                                {
                                    b_jobMoni.JobSet(_Status.Order, _Status.vecState);
                                    break;
                                }
                            case eUID4VM.DASH_MONI_JOB_Update:
                                {
                                    b_jobMoni.JobState(_Status.Order.state, _Status.vecState.JobState);
                                    break;
                                }
                            case eUID4VM.DASH_MONI_JOB_PioStart:
                                {
                                    var rtn = sender as Noti;
                                    b_jobMoni.StartProgress(rtn.msg, rtn.nTemp);
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
                                case eROBOTST.None:                                
                                case eROBOTST.NotAssigned: return false;
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

        private void UpdateVecStatus(eVECSTATE st)
        {
            switch (st)
            {
                case eVECSTATE.IDLE_PROCESSING:
                case eVECSTATE.NONE:
                    b_ForeGroundVecState = new SolidColorBrush(Colors.DimGray);
                    IconVecState = PackIconKind.NotificationsNone;
                    break;

                case eVECSTATE.STOPPING:
                case eVECSTATE.GOING_TO:
                case eVECSTATE.GOING_TO_POINT:
                case eVECSTATE.GOING_TO_DOCK_AT:
                case eVECSTATE.DRIVING:
                case eVECSTATE.DRIVING_INTO_DOCK:
                case eVECSTATE.TELEOP_DRIVING:
                case eVECSTATE.MOVING:
                case eVECSTATE.DOING_TASK_DELTAHEADING:
                case eVECSTATE.DOING_TASK_MOVE:
                case eVECSTATE.DOING_TASK_PAUSE:
                case eVECSTATE.DONE_DRIVING:
                case eVECSTATE.DOCKING:                    
                case eVECSTATE.UNDOCKING:
                    b_ForeGroundVecState = new SolidColorBrush(Colors.Green);
                    IconVecState = PackIconKind.ShoeRunning;
                    break;

                case eVECSTATE.FAILED_GOING_TO:
                case eVECSTATE.ESTOP_PRESSED:
                case eVECSTATE.ESTOP_RELIEVED:
                case eVECSTATE.CANNOT_FIND_PATH:
                case eVECSTATE.NO_ENTER:
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
            set {
                this.MutateVerbose(ref EQPState, value, RaisePropertyChanged());            
            }
        }

        PackIconKind IconManualMode = PackIconKind.Stop;
        public PackIconKind b_IconManualMode
        {
            get { return IconManualMode; }
            set { this.MutateVerbose(ref IconManualMode, value, RaisePropertyChanged()); }
        }

        string ManualMode = string.Empty;
        public string b_ManualMode
        {
            get { return ManualMode; }
            set { this.MutateVerbose(ref ManualMode, value, RaisePropertyChanged());}
        }

        string VecSOC = string.Empty;
        public string b_VecSOC
        {
            get { return VecSOC; }
            set { this.MutateVerbose(ref VecSOC, value, RaisePropertyChanged()); }
        }

        SolidColorBrush ForeGroundSOC;
        public SolidColorBrush b_ForeGroundSOC
        {
            get { return ForeGroundSOC; }
            set { this.MutateVerbose(ref ForeGroundSOC, value, RaisePropertyChanged()); }
        }

        PackIconKind IconVecState = PackIconKind.Stop;
        public PackIconKind b_IconVecState
        {
            get { return IconVecState; }
            set { this.MutateVerbose(ref IconVecState, value, RaisePropertyChanged()); }
        }

        string VecState = string.Empty;
        public string b_VecState
        {
            get { return VecState; }
            set { this.MutateVerbose(ref VecState, value, RaisePropertyChanged()); }
        }

        SolidColorBrush ForeGroundVecState;
        public SolidColorBrush b_ForeGroundVecState
        {
            get { return ForeGroundVecState; }
            set { this.MutateVerbose(ref ForeGroundVecState, value, RaisePropertyChanged()); }
        }

        private ObservableCollection<SRC4MONI> _inputs;        
        public ObservableCollection<SRC4MONI> b_lstInput
        {
            get =>_inputs;
            set { this.MutateVerbose(ref _inputs, value, RaisePropertyChanged()); }
        }
        
        private ObservableCollection<SRC4MONI> _Output ;
        public ObservableCollection<SRC4MONI> b_lstOutput
        {
            get { return _Output; }
            set {
                this.MutateVerbose(ref _Output, value, RaisePropertyChanged());
            }
        }

        JobMonitor jobMoni = new JobMonitor();
        public JobMonitor b_jobMoni
        {
            get { return jobMoni; }
            set { jobMoni = value; }
        }
    }
}
