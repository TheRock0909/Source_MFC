using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Source_MFC.ViewModels
{
    class VM_UsCtrl_Dash_Manual : Notifier
    {
        MainCtrl _ctrl;
        GOALINFO _Goals;
        JOBOPT _opt;
        public ICommand Evt_BtnClicked { get; set; }
        public ICommand Evt_SelectedItem { get; set; }
        public VM_UsCtrl_Dash_Manual(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _Goals = _Data.Inst.sys.goal;
            _opt = ctrl._status.Order.opt;
            _ctrl.Evt_Dash_Manual_DataExchange += On_DataExchange;
            Evt_BtnClicked = new Command(On_BtnClicked, CanExecute);
            Evt_SelectedItem = new Command(On_SelectedItem);
        }

        public bool CanExecute(object parameter)
        {
            eUID4VM uid = (eUID4VM)Convert.ToInt32(parameter);
            switch (_ctrl._EQPStatus)
            {                
                case eEQPSATUS.Stop:                
                    switch (uid)
                    {
                        case eUID4VM.DASH_MNL_BTN_MAKEORDER:
                            switch (_ctrl._status.Order.state)
                            {
                                case CommandState.None: return true;
                                default: return false;
                            }
                        case eUID4VM.DASH_MNL_RDO_GoalType_0:                     
                        case eUID4VM.DASH_MNL_RDO_GoalType_1:                            
                        case eUID4VM.DASH_MNL_RDO_GoalType_2:                            
                        case eUID4VM.DASH_MNL_RDO_GoalType_3:
                            break;
                        default: break;
                    }
                    break;
                case eEQPSATUS.Init:
                    switch (uid)
                    {
                        case eUID4VM.DASH_MNL_BTN_MAKEORDER: return false;
                        case eUID4VM.DASH_MNL_RDO_GoalType_0:
                        case eUID4VM.DASH_MNL_RDO_GoalType_1:
                        case eUID4VM.DASH_MNL_RDO_GoalType_2:
                        case eUID4VM.DASH_MNL_RDO_GoalType_3:
                            break;
                        default: break;
                    }
                    break;
                default: return false;
            }
            return true;
        }

        private void UpdateGoalType(eGOALTYPE type)
        {
            goalType = type;
            b_SelGoalType = $"{goalType}";            
            On_DataExchange(null, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_GoalItem));
        }

        private void UpdateGoalList(eGOALTYPE type)
        {
            if ( goalType != type )
            {
                UpdateGoalType(type);                
                if (0 < _Goals.lst.Count)
                {
                    int idx = 1;
                    b_lstGoals = new ObservableCollection<GOAL_NODE>();
                    b_lstGoals.CollectionChanged += On_CollectionChanged;
                    b_lstGoals.Clear();
                    var lst = _Goals.lst.Where(g => g.type == type).OrderBy(t => t.line).ToList();
                    foreach (GOALITEM item in lst)
                    {
                        b_lstGoals.Add(new GOAL_NODE() { nNo = idx, label = item.label });
                        idx++;
                    }                                  
                }                
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


        private void UpdateGoalItem(GOAL_NODE item)
        {            
            
        }

        private void On_DataExchange(object sender, (eDATAEXCHANGE dir, eUID4VM id) e)
        {
            switch (e.dir)
            {                
                case eDATAEXCHANGE.Model2View:
                    switch (e.id)
                    {
                        case eUID4VM.DASH_MNL_ALL:
                            UpdateGoalList(eGOALTYPE.Pickup);
                            b_Opt_Skip_Move2 = _opt.bSkipGo2Dest;
                            b_Opt_Skip_PIO = _opt.bSkipPIO;
                            b_Opt_Skip_Transfer = _opt.bSkipTransfer;
                            break;
                        case eUID4VM.DASH_MNL_BTN_MAKEORDER:
                            break;
                        case eUID4VM.DASH_MNL_GoalItem:
                            _CurrItem = new GOAL_NODE();
                            b_SelGoalName = string.Empty;
                            b_Line = string.Empty;
                            b_MCType = string.Empty;
                            b_HostName = string.Empty;
                            b_GoalName = string.Empty;
                            b_Position = string.Empty;
                            b_EscapeDist = string.Empty;
                            break;
                        case eUID4VM.DASH_MNL_RDO_GoalType_0:                            
                        case eUID4VM.DASH_MNL_RDO_GoalType_1:
                        case eUID4VM.DASH_MNL_RDO_GoalType_2:
                        case eUID4VM.DASH_MNL_RDO_GoalType_3:                            
                            break;                       
                        default: break;
                    }
                    break;
                case eDATAEXCHANGE.View2Model:
                    switch (e.id)
                    {
                        case eUID4VM.DASH_MNL_ALL:
                            break;
                        case eUID4VM.DASH_MNL_BTN_MAKEORDER:
                            break;
                        case eUID4VM.DASH_MNL_GoalItem:
                            {
                                if ( null != sender )
                                {
                                    var item = sender as GOAL_NODE;
                                    _CurrItem = new GOAL_NODE() { nNo = item.nNo, label = item.label };
                                    b_SelGoalName = _CurrItem.label;
                                    var goal = _Goals.Get(goalType, b_SelGoalName, eSRCHGOALBY.Label);
                                    if (null != goal)
                                    {
                                        b_Line = $"{goal.line}";
                                        b_MCType = $"{goal.mcType}";
                                        b_HostName = goal.hostName;
                                        b_GoalName = goal.name;
                                        b_Position = $"{goal.pos.x}, {goal.pos.y}, {goal.pos.r}";
                                        b_EscapeDist= $"{goal.escape.x}, {goal.escape.y}, {goal.escape.r}";
                                    }
                                    else
                                    {
                                        On_DataExchange(null, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_GoalItem));
                                    }
                                }
                                else
                                {
                                    On_DataExchange(null, (eDATAEXCHANGE.Model2View, eUID4VM.DASH_MNL_GoalItem));
                                }
                                break;
                            }
                        case eUID4VM.DASH_MNL_RDO_GoalType_0:                            
                        case eUID4VM.DASH_MNL_RDO_GoalType_1:                            
                        case eUID4VM.DASH_MNL_RDO_GoalType_2:                            
                        case eUID4VM.DASH_MNL_RDO_GoalType_3:
                            eGOALTYPE type = (eGOALTYPE)((int)e.id - (int)eUID4VM.DASH_MNL_RDO_GoalType_0);
                            UpdateGoalList(type);
                            break;
                        default: break;
                    }
                    break;                
                default: break;
            }
        }

        private void On_BtnClicked(object obj)
        {
            eUID4VM uid = (eUID4VM)Convert.ToInt32(obj);
            switch (uid)
            {               
                case eUID4VM.DASH_MNL_BTN_MAKEORDER:
                    break;
                case eUID4VM.DASH_MNL_RDO_GoalType_0:                    
                case eUID4VM.DASH_MNL_RDO_GoalType_1:                    
                case eUID4VM.DASH_MNL_RDO_GoalType_2:                    
                case eUID4VM.DASH_MNL_RDO_GoalType_3:
                    On_DataExchange(obj, (eDATAEXCHANGE.View2Model, uid));
                    break;
            }
        }

        GOAL_NODE _CurrItem = new GOAL_NODE();
        private void On_SelectedItem(object obj)
        {
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<GOAL_NODE>();
            var item = collection.FirstOrDefault();
            if ( null != item )
            {
                On_DataExchange(item, (eDATAEXCHANGE.View2Model, eUID4VM.DASH_MNL_GoalItem));
            }            
        }

        eGOALTYPE goalType = eGOALTYPE.Standby;
        string SelGoalType = string.Empty;
        public string b_SelGoalType
        {
            get { return SelGoalType; }
            set { SelGoalType = value; OnPropertyChanged("b_SelGoalType"); }
        }

        string SelGoalName = string.Empty;
        public string b_SelGoalName
        {
            get { return SelGoalName; }
            set { SelGoalName = value; OnPropertyChanged("b_SelGoalName"); }
        }

        string Line = string.Empty;
        public string b_Line
        {
            get { return Line; }
            set { Line = value; OnPropertyChanged("b_Line"); }
        }

        string MCType = string.Empty;
        public string b_MCType
        {
            get { return MCType; }
            set { MCType = value; OnPropertyChanged("b_MCType"); }
        }

        string HostName = string.Empty;
        public string b_HostName
        {
            get { return HostName; }
            set { HostName = value; OnPropertyChanged("b_HostName"); }
        }

        string Position = string.Empty;
        public string b_Position
        {
            get { return Position; }
            set { Position = value; OnPropertyChanged("b_Position"); }
        }

        string EscapeDist = string.Empty;
        public string b_EscapeDist
        {
            get { return EscapeDist; }
            set { EscapeDist = value; OnPropertyChanged("b_EscapeDist"); }
        }

        string GoalName = string.Empty;
        public string b_GoalName
        {
            get { return GoalName; }
            set { GoalName = value; OnPropertyChanged("b_GoalName"); }
        }

        public bool b_Opt_Skip_Move2
        {
            get { return _opt.bSkipGo2Dest; }
            set { _opt.bSkipGo2Dest = value; OnPropertyChanged("b_Opt_Skip_Move2"); }
        }

        public bool b_Opt_Skip_PIO
        {
            get { return _opt.bSkipPIO; }
            set { _opt.bSkipPIO = value; OnPropertyChanged("b_Opt_Skip_PIO"); }
        }

        public bool b_Opt_Skip_Transfer
        {
            get { return _opt.bSkipTransfer; }
            set { _opt.bSkipTransfer = value; OnPropertyChanged("b_Opt_Skip_Transfer"); }
        }


        private ObservableCollection<GOAL_NODE> lstGoal;
        public ObservableCollection<GOAL_NODE> b_lstGoals
        {
            get { return lstGoal; }
            set {
                if (value == lstGoal) return;
                lstGoal = value;
                OnPropertyChanged("b_lstGoals");
            }
        }
    }

    class GOAL_NODE
    {
        public int nNo { get; set; } = 0;
        public string label { get; set; } = string.Empty;
        public GOAL_NODE()
        {

        }
    }
}
