using KeyPad;
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
        public ICommand Evt_DataExchange4Manual { get; set; }
        public IEnumerable<eSEQLIST> eSeqlist { get; set; }
        public IEnumerable<eVEC_CMD> eVecCmdLst { get; set; }        
        public VM_UsCtrl_Dash_Manual(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _Goals = _Data.Inst.sys.goal;
            _opt = ctrl._status.Order.opt;
            eSeqlist = Enum.GetValues(typeof(eSEQLIST)).Cast<eSEQLIST>();
            eVecCmdLst = Enum.GetValues(typeof(eVEC_CMD)).Cast<eVEC_CMD>();
            _ctrl.Evt_Dash_Manual_DataExchange += On_DataExchange;
            Evt_BtnClicked = new Command(On_BtnClicked, CanExecute);
            Evt_SelectedItem = new Command(On_SelectedItem);
            Evt_DataExchange4Manual = new Command(On_DataExchange4Manual);
        }

        ~VM_UsCtrl_Dash_Manual()
        {
            _ctrl.Evt_Dash_Manual_DataExchange -= On_DataExchange;
            b_lstGoals.CollectionChanged -= On_CollectionChanged;
            b_lstGoals = null;
        }

        private void On_DataExchange4Manual(object obj)
        {
            var uid = (eUID4VM)Convert.ToInt32(obj);
            On_DataExchange(obj, (eDATAEXCHANGE.View2Model, uid));
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
                                case eJOBST.None: return true;
                                default: return false;
                            }
                        case eUID4VM.DASH_MNL_RDO_GoalType_0:                     
                        case eUID4VM.DASH_MNL_RDO_GoalType_1:                            
                        case eUID4VM.DASH_MNL_RDO_GoalType_2:                            
                        case eUID4VM.DASH_MNL_RDO_GoalType_3:
                            break;
                        case eUID4VM.DASH_MNL_SEQ_INIT:                            
                        case eUID4VM.DASH_MNL_SEQ_START:                            
                        case eUID4VM.DASH_MNL_SEQ_STOP:
                            switch (b_SeqList)
                            {                                
                                case eSEQLIST.EscapeEQP:
                                    switch (_Data.Inst.sys.cfg.fac.seqMode)
                                    {
                                        case eSCENARIOMODE.PC: return true;                                        
                                        default:return false;
                                    }
                                case eSEQLIST.Move2Dst:                                    
                                case eSEQLIST.PIO:                                    
                                case eSEQLIST.Pick:                                    
                                case eSEQLIST.Drop: return true;
                                case eSEQLIST.MAX_SEQ:                                    
                                default:return false;
                            }
                        case eUID4VM.DASH_MNL_VECTSK_INIT:
                        case eUID4VM.DASH_MNL_VECTSK_START:
                        case eUID4VM.DASH_MNL_VECTSK_STOP:
                            switch (b_vecCmdLst)
                            {
                                case eVEC_CMD.None:return false;                                
                                default:break ;
                            }
                            break;
                        case eUID4VM.DASH_MNL_VTSK_PARA_GOAL1:
                            switch (b_vecCmdLst)
                            {                                
                                case eVEC_CMD.Go2Goal:                
                                case eVEC_CMD.Go2Straight:
                                case eVEC_CMD.Dock:                                
                                case eVEC_CMD.GetDistBetween:
                                case eVEC_CMD.GetDistFromHere:
                                case eVEC_CMD.LocalizeAtGoal: break;
                                default: return false;
                            }
                            break;
                        case eUID4VM.DASH_MNL_VTSK_PARA_GOAL2:
                            switch (b_vecCmdLst)
                            {
                                case eVEC_CMD.GetDistBetween: break;
                                default: return false;
                            }
                            break;
                        case eUID4VM.DASH_MNL_VTSK_PARA_POSX:
                        case eUID4VM.DASH_MNL_VTSK_PARA_POSY:
                        case eUID4VM.DASH_MNL_VTSK_PARA_POSR:
                            switch (b_vecCmdLst)
                            {
                                case eVEC_CMD.Go2Point: case eVEC_CMD.Go2Straight: break;
                                default: return false;
                            }
                            break;
                        case eUID4VM.DASH_MNL_VTSK_PARA_MOVEX:
                        case eUID4VM.DASH_MNL_VTSK_PARA_SPEED:
                        case eUID4VM.DASH_MNL_VTSK_PARA_ACC:
                        case eUID4VM.DASH_MNL_VTSK_PARA_DCC:
                            switch (b_vecCmdLst)
                            {
                                case eVEC_CMD.MoveDeltaHeading: case eVEC_CMD.MoveFront: break;
                                default: return false;
                            }
                            break;
                        case eUID4VM.DASH_MNL_VTSK_PARA_MSG:
                            switch (b_vecCmdLst)
                            {
                                case eVEC_CMD.Say: case eVEC_CMD.SendMassage: break;
                                default: return false;
                            }
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
                        case eUID4VM.DASH_MNL_SEQ_INIT:
                        case eUID4VM.DASH_MNL_SEQ_START:
                        case eUID4VM.DASH_MNL_SEQ_STOP:
                        case eUID4VM.DASH_MNL_VECTSK_INIT:
                        case eUID4VM.DASH_MNL_VECTSK_START:
                        case eUID4VM.DASH_MNL_VECTSK_STOP:
                        case eUID4VM.DASH_MNL_VTSK_PARA_GOAL1:
                        case eUID4VM.DASH_MNL_VTSK_PARA_GOAL2:
                        case eUID4VM.DASH_MNL_VTSK_PARA_POSX:
                        case eUID4VM.DASH_MNL_VTSK_PARA_POSY:
                        case eUID4VM.DASH_MNL_VTSK_PARA_POSR:
                        case eUID4VM.DASH_MNL_VTSK_PARA_MOVEX:
                        case eUID4VM.DASH_MNL_VTSK_PARA_SPEED:
                        case eUID4VM.DASH_MNL_VTSK_PARA_ACC:
                        case eUID4VM.DASH_MNL_VTSK_PARA_DCC:
                        case eUID4VM.DASH_MNL_VTSK_PARA_MSG:                            
                            return false;
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
                        case eUID4VM.DASH_MNL_VTSK_PARA_GOAL1: case eUID4VM.DASH_MNL_VTSK_PARA_GOAL2: 
                        case eUID4VM.DASH_MNL_VTSK_PARA_POSX : case eUID4VM.DASH_MNL_VTSK_PARA_POSY : case eUID4VM.DASH_MNL_VTSK_PARA_POSR:
                        case eUID4VM.DASH_MNL_VTSK_PARA_MOVEX: case eUID4VM.DASH_MNL_VTSK_PARA_SPEED: case eUID4VM.DASH_MNL_VTSK_PARA_ACC:
                        case eUID4VM.DASH_MNL_VTSK_PARA_DCC  : case eUID4VM.DASH_MNL_VTSK_PARA_MSG  :
                            {                                
                                var strCurr = string.Empty;
                                switch (e.id)
                                {
                                    case eUID4VM.DASH_MNL_VTSK_PARA_GOAL1: strCurr = b_VParam_Goal1; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_GOAL2: strCurr = b_VParam_Goal2; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_POSX: strCurr = b_VParam_PosX; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_POSY: strCurr = b_VParam_PosY; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_POSR: strCurr = b_VParam_PosR; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_MOVEX: strCurr = b_VParam_Move; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_SPEED: strCurr = b_VParam_Spd; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_ACC: strCurr = b_VParam_Acc; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_DCC: strCurr = b_VParam_Dec; break;
                                    case eUID4VM.DASH_MNL_VTSK_PARA_MSG: strCurr = b_VParam_Msg; break;
                                    default: break;
                                }
                                
                                VirtualKeyboard keyboardWindow = new VirtualKeyboard(strCurr);
                                if (keyboardWindow.ShowDialog() == true)
                                {
                                    switch (e.id)
                                    {
                                        case eUID4VM.DASH_MNL_VTSK_PARA_GOAL1: b_VParam_Goal1 = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_GOAL2: b_VParam_Goal2 = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_POSX: b_VParam_PosX = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_POSY: b_VParam_PosY = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_POSR: b_VParam_PosR = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_MOVEX: b_VParam_Move = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_SPEED: b_VParam_Spd = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_ACC: b_VParam_Acc = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_DCC: b_VParam_Dec = keyboardWindow.Result; break;
                                        case eUID4VM.DASH_MNL_VTSK_PARA_MSG: b_VParam_Msg = keyboardWindow.Result; break;
                                        default: break;
                                    }
                                }
                                break;
                            }
                        default: break;
                    }
                    break;                
                default: break;
            }
        }

        MsgBox msgBox = MsgBox.Inst;
        private void On_BtnClicked(object obj)
        {
            eUID4VM uid = (eUID4VM)Convert.ToInt32(obj);
            switch (uid)
            {             
                case eUID4VM.DASH_MNL_SEQ_SELECTION:
                    break;
                case eUID4VM.DASH_MNL_RDO_GoalType_0:                    
                case eUID4VM.DASH_MNL_RDO_GoalType_1:                    
                case eUID4VM.DASH_MNL_RDO_GoalType_2:                    
                case eUID4VM.DASH_MNL_RDO_GoalType_3:
                    On_DataExchange(obj, (eDATAEXCHANGE.View2Model, uid));
                    break;
                case eUID4VM.DASH_MNL_BTN_MAKEORDER:        
                case eUID4VM.DASH_MNL_SEQ_INIT:
                case eUID4VM.DASH_MNL_SEQ_START:
                case eUID4VM.DASH_MNL_SEQ_STOP:
                case eUID4VM.DASH_MNL_VECTSK_INIT:
                case eUID4VM.DASH_MNL_VECTSK_START:
                case eUID4VM.DASH_MNL_VECTSK_STOP:
                    switch (_ctrl._EQPStatus)
                    {                        
                        case eEQPSATUS.Stop:
                            if ( _ctrl._status.bIsManual )
                            {
                                switch (_ctrl._Order.state)
                                {
                                    case eJOBST.None:
                                        switch (uid)
                                        {
                                            case eUID4VM.DASH_MNL_BTN_MAKEORDER:
                                                {
                                                    eJOBTYPE type = eJOBTYPE.NONE;
                                                    switch (goalType)
                                                    {
                                                        case eGOALTYPE.Pickup: type = eJOBTYPE.UNLOADING; break;
                                                        case eGOALTYPE.Dropoff: type = eJOBTYPE.LOADING; break;
                                                        case eGOALTYPE.Charge: type = eJOBTYPE.CAHRGE; break;
                                                        case eGOALTYPE.Standby: type = eJOBTYPE.STANDBY; break;
                                                        default: break;
                                                    }

                                                    switch (type)
                                                    {
                                                        case eJOBTYPE.NONE: break;                                                        
                                                        default:
                                                            {
                                                                MP_DISTBTW data = new MP_DISTBTW();
                                                                var goal = _Goals.Get(goalType, b_SelGoalName, eSRCHGOALBY.Label);
                                                                data.cmd = type.ToString();
                                                                data.goal1 = goal.name;
                                                                data.goal2 = goal.pos.ToString();
                                                                var job = _ctrl.MNL_GetJobOrderStr(data);
                                                                var opt = new Any64();
                                                                opt[0] = b_Opt_Skip_Move2;
                                                                opt[1] = b_Opt_Skip_PIO;
                                                                opt[2] = b_Opt_Skip_Transfer;
                                                                _ctrl.On_MplusRecvData(this, (job, true, opt.INT32_0));
                                                                break;
                                                            }
                                                    }                                                    
                                                    break;
                                                }
                                            case eUID4VM.DASH_MNL_SEQ_INIT:
                                                switch (b_SeqList)
                                                {
                                                    case eSEQLIST.PIO:
                                                        switch (b_SelGoalType.ToEnum<eGOALTYPE>())
                                                        {
                                                            case eGOALTYPE.Pickup:
                                                                _ctrl.Job_ManualStart(b_SeqList, eJOBTYPE.UNLOADING);
                                                                break;
                                                            case eGOALTYPE.Dropoff:
                                                                _ctrl.Job_ManualStart(b_SeqList, eJOBTYPE.LOADING);
                                                                break;
                                                            default: return;
                                                        }
                                                        break;
                                                    default: _ctrl.Job_ManualStart(b_SeqList); ; break;
                                                }
                                                break;
                                            case eUID4VM.DASH_MNL_SEQ_START:
                                                switch (b_SeqList)
                                                {
                                                    case eSEQLIST.EscapeEQP:
                                                        switch (_Data.Inst.sys.cfg.fac.seqMode)
                                                        {
                                                            case eSCENARIOMODE.PC: _ctrl.Job_ManualStart(b_SeqList); break;                                                            
                                                            default:return;
                                                        }
                                                        break;
                                                    case eSEQLIST.PIO:
                                                        switch (b_SelGoalType.ToEnum<eGOALTYPE>())
                                                        {
                                                            case eGOALTYPE.Pickup:
                                                                _ctrl.Job_ManualStart(b_SeqList, eJOBTYPE.UNLOADING);
                                                                break;
                                                            case eGOALTYPE.Dropoff:
                                                                _ctrl.Job_ManualStart(b_SeqList, eJOBTYPE.LOADING);
                                                                break;
                                                            default: return;
                                                        }
                                                        break;
                                                    case eSEQLIST.Pick: case eSEQLIST.Drop: _ctrl.Job_ManualStart(b_SeqList); break;
                                                    default:return ;
                                                }
                                                break;
                                            case eUID4VM.DASH_MNL_SEQ_STOP:
                                                switch (b_SeqList)
                                                {
                                                    case eSEQLIST.PIO:
                                                        switch (b_SelGoalType.ToEnum<eGOALTYPE>())
                                                        {
                                                            case eGOALTYPE.Pickup:
                                                                _ctrl.Job_ManualStop(b_SeqList, eJOBTYPE.UNLOADING);
                                                                break;
                                                            case eGOALTYPE.Dropoff:
                                                                _ctrl.Job_ManualStop(b_SeqList, eJOBTYPE.LOADING);
                                                                break;
                                                            default: return;
                                                        }
                                                        break;
                                                    default: _ctrl.Job_ManualStop(b_SeqList); ; break;
                                                }
                                                break;
                                            case eUID4VM.DASH_MNL_VECTSK_INIT:
                                                vecParam = new SENDARG();
                                                _ctrl.VEC_TskStop();
                                                break;
                                            case eUID4VM.DASH_MNL_VECTSK_START:
                                                _ctrl.VEC_TskStart(b_vecCmdLst, vecParam);
                                                break;
                                            case eUID4VM.DASH_MNL_VECTSK_STOP:
                                                _ctrl.VEC_TskStop();
                                                break;
                                        }
                                        break;
                                    default: msgBox.ShowDialog($"잡 수행중입니다. 잡 완료 후 가동가능합니다.", MsgBox.MsgType.Warn, MsgBox.eBTNSTYLE.OK); break;
                                }
                            }
                            else
                            {
                                msgBox.ShowDialog($"매뉴얼 모드로 변경 해 주십시오.", MsgBox.MsgType.Warn, MsgBox.eBTNSTYLE.OK);
                            }
                            break;                       
                        default: msgBox.ShowDialog($"설비상태를 정지상태로 변경 해 주십시오.", MsgBox.MsgType.Warn, MsgBox.eBTNSTYLE.OK); break;
                    }
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
            set { this.MutateVerbose(ref SelGoalType, value, RaisePropertyChanged()); }
        }

        string SelGoalName = string.Empty;
        public string b_SelGoalName
        {
            get { return SelGoalName; }
            set { this.MutateVerbose(ref SelGoalName, value, RaisePropertyChanged()); }
        }

        string Line = string.Empty;
        public string b_Line
        {
            get { return Line; }
            set { this.MutateVerbose(ref Line, value, RaisePropertyChanged()); }
        }

        string MCType = string.Empty;
        public string b_MCType
        {
            get { return MCType; }
            set { this.MutateVerbose(ref MCType, value, RaisePropertyChanged()); }
        }

        string HostName = string.Empty;
        public string b_HostName
        {
            get { return HostName; }
            set { this.MutateVerbose(ref HostName, value, RaisePropertyChanged()); }
        }

        string Position = string.Empty;
        public string b_Position
        {
            get { return Position; }
            set { this.MutateVerbose(ref Position, value, RaisePropertyChanged()); }
        }

        string EscapeDist = string.Empty;
        public string b_EscapeDist
        {
            get { return EscapeDist; }
            set { this.MutateVerbose(ref EscapeDist, value, RaisePropertyChanged()); }
        }

        string GoalName = string.Empty;
        public string b_GoalName
        {
            get { return GoalName; }
            set { this.MutateVerbose(ref GoalName, value, RaisePropertyChanged()); }
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
            set { this.MutateVerbose(ref lstGoal, value, RaisePropertyChanged()); }
        }

        
        eSEQLIST seqList = eSEQLIST.MAX_SEQ;
        public eSEQLIST b_SeqList
        {
            get { return seqList; }
            set { this.MutateVerbose(ref seqList, value, RaisePropertyChanged()); }
        }

        eVEC_CMD vecCmdLst = eVEC_CMD.None;
        public eVEC_CMD b_vecCmdLst
        {
            get { return vecCmdLst; }
            set { this.MutateVerbose(ref vecCmdLst, value, RaisePropertyChanged()); }
        }

        SENDARG vecParam = new SENDARG();
        public string b_VParam_Goal1
        {
            get { return vecParam.goal_1st; }
            set { vecParam.goal_1st = value; OnPropertyChanged(); }
        }

        public string b_VParam_Goal2
        {
            get { return vecParam.goal_2nd; }
            set { vecParam.goal_2nd = value; OnPropertyChanged(); }
        }

        public string b_VParam_Msg
        {
            get { return vecParam.msg; }
            set { vecParam.msg = value; OnPropertyChanged(); }
        }

        public string b_VParam_PosX
        {
            get { return vecParam.pos.x.ToString(); }
            set { int.TryParse(value, out vecParam.pos.x); OnPropertyChanged(); }
        }

        public string b_VParam_PosY
        {
            get { return vecParam.pos.y.ToString(); }
            set { int.TryParse(value, out vecParam.pos.y); OnPropertyChanged(); }
        }
        public string b_VParam_PosR
        {
            get { return vecParam.pos.r.ToString(); }
            set { int.TryParse(value, out vecParam.pos.r); OnPropertyChanged(); }
        }

        public string b_VParam_Move
        {
            get { return vecParam.move.ToString(); }
            set {
                int temp = 0;
                if ( int.TryParse(value, out temp))
                {
                    vecParam.move = temp;
                    OnPropertyChanged();
                }                                
            }
        }

        public string b_VParam_Spd
        {
            get { return vecParam.spd.ToString(); }
            set {
                int temp = 0;
                if (int.TryParse(value, out temp))
                {
                    vecParam.spd = temp;
                    OnPropertyChanged();
                }
            }
        }

        public string b_VParam_Acc
        {
            get { return vecParam.acc.ToString(); }
            set {
                int temp = 0;
                if (int.TryParse(value, out temp))
                {
                    vecParam.acc = temp;
                    OnPropertyChanged();
                }
            }
        }

        public string b_VParam_Dec
        {
            get { return vecParam.dec.ToString(); }
            set {
                int temp = 0;
                if (int.TryParse(value, out temp))
                {
                    vecParam.dec = temp;
                    OnPropertyChanged();
                }
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
