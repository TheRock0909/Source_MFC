using KeyPad;
using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Source_MFC.ViewModels
{
    class VM_UsCtrl_Sys_Goal : Notifier
    {
        MainCtrl _ctrl;
        MsgBox msgBox = MsgBox.Inst;
        public ICommand Evt_treeSelectedChanged { get; set; }
        public ICommand Evt_btnAddClick { get; set; }
        public ICommand Evt_btnDelClick { get; set; }
        public ICommand Evt_cmbLineIDSelectedChanged { get; set; }
        public ICommand Evt_cmbMCTypeSelectedChanged { get; set; }
        public ICommand Evt_DataExchange4Goal { get; set; }
        public IEnumerable<eGOALTYPE> eType { get; set; }
        public IEnumerable<eLINE> eLine { get; set; }
        public IEnumerable<eMFC_MCTYPE> eMCType { get; set; }
        GOALINFO _Goals;
        public VM_UsCtrl_Sys_Goal(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _Goals = _Data.Inst.sys.goal;
            _ctrl.Evt_Sys_Goal_List_DataExchange += On_List_DataExchange;
            _ctrl.Evt_Sys_Goal_Item_DataExchange += On_DataExchange;
            Evt_treeSelectedChanged = new Command(On_ItemChangedMethod);
            Evt_btnAddClick = new Command(On_btnAddMethod);
            Evt_btnDelClick = new Command(On_btnDelMethod);
            Evt_cmbLineIDSelectedChanged = new Command(On_cmbLineChangeMethod);
            Evt_cmbMCTypeSelectedChanged = new Command(On_cmbMCTypeSelectedChanged);
            Evt_DataExchange4Goal = new Command(On_DataExchange4Goal);

            eLine = Enum.GetValues(typeof(eLINE)).Cast<eLINE>();
            eMCType = Enum.GetValues(typeof(eMFC_MCTYPE)).Cast<eMFC_MCTYPE>();
        }

        ~VM_UsCtrl_Sys_Goal()
        {
            _ctrl.Evt_Sys_Goal_List_DataExchange -= On_List_DataExchange;
            _ctrl.Evt_Sys_Goal_Item_DataExchange -= On_DataExchange;
        }


        private void Init()
        {
            currItem = new GOALITEM();
            b_TreeGoals.Clear();
            b_Goaltype = new eGOALTYPE();
            b_GoalInfo = string.Empty;
            b_LineSel = new eLINE();
            b_HostName = string.Empty;
            b_Label = string.Empty;
        }

        private void On_DataExchange4Goal(object obj)
        {
            switch (currLine)
            {
                case eLINE.None: break;                
                default: On_DataExchange(obj, (eDATAEXCHANGE.View2Model, eUID4VM.NONE)); break;
            }
        }

        private void On_List_DataExchange(object sender, GOALINFO e)
        {
            Init();
            foreach (eGOALTYPE item in Enum.GetValues(typeof(eGOALTYPE)))
            {
                ObservableCollection<TreeData> SubChild1 = new ObservableCollection<TreeData>();
                var lst = e.GetList(item);
                foreach (eLINE line in Enum.GetValues(typeof(eLINE)))
                {
                    if (eLINE.None == line) continue;
                    ObservableCollection<TreeData> SubChild2 = new ObservableCollection<TreeData>();
                    var linegoal = lst.Where(l => l.line == line).ToList();
                    foreach (var goal in linegoal)
                    {
                        SubChild2.Add(new TreeData() { b_Name = goal.label, b_Parent = item.ToString() });
                    }
                    SubChild1.Add(new TreeData() { b_Name = $"Line #{Ctrls.Remove_(line.ToString())}", b_Children = SubChild2, b_Parent = line.ToString() });
                }
                TreeData treeData = new TreeData() { b_Name = item.ToString(), b_Children = SubChild1, b_Parent = item.ToString() };
                b_TreeGoals.Add(treeData);
            }
            OnPropertyChanged();
        }

        private eLINE GetLine(string selitem)
        {
            foreach (eLINE line in Enum.GetValues(typeof(eLINE)))
            {
                var line4 = $"Line #{Ctrls.Remove_(line.ToString())}";
                var chk = selitem.Equals(line4);
                if ( true == chk)
                {
                    return line;
                }
            }
            return eLINE.None;
        }

        private void On_DataExchange(object sender, (eDATAEXCHANGE dir, eUID4VM uid) e)
        {
            if (sender != null)
            {
                switch (e.dir)
                {                    
                    case eDATAEXCHANGE.Model2View:
                        {
                            switch (e.uid)
                            {
                                case eUID4VM.GOAL_ITEMS:
                                    {
                                        var item = sender as GOALITEM;
                                        currItem = new GOALITEM()
                                        {
                                            type = item.type, hostName = item.hostName, mcType = item.mcType
                                          , label = item.label, line = item.line, name = item.name
                                          , pos = new nPOS_XYR() { x = item.pos.x, y = item.pos.y, r = item.pos.r }
                                          , escape = new nPOS_XYR() { x = item.escape.x, y = item.escape.y, r = item.escape.r }
                                        };
                                        currType = item.type; currLine = item.line;
                                        b_GoalInfo = $"{currType}, {currLine}, {item.label}";
                                        b_LineSel = item.line;
                                        b_HostName = item.hostName;
                                        b_Label = item.label;
                                        b_eMCType = item.mcType;
                                        b_PosX = $"{item.pos.x}"; b_PosY = $"{item.pos.y}"; b_PosR = $"{item.pos.r}";
                                        b_escapeX = $"{item.escape.x}"; b_escapeY = $"{item.escape.y}"; b_escapeR = $"{item.escape.r}";
                                        break;
                                    }
                                case eUID4VM.GOAL_ADD: case eUID4VM.GOAL_DEL: break;
                                case eUID4VM.GOLA_HostName: case eUID4VM.GOAL_Label:
                                case eUID4VM.GOAL_PosX: case eUID4VM.GOAL_PosY: case eUID4VM.GOAL_PosR:
                                case eUID4VM.GOAL_EscapeX: case eUID4VM.GOAL_EscapeY: case eUID4VM.GOAL_EscapeR: break;
                                default: break;
                            }
                            break;
                        }
                    case eDATAEXCHANGE.View2Model:
                        {
                            var uid = (eUID4VM)Convert.ToInt32(sender);
                            var datatype = eDATATYPE.NONE;
                            var strCurr = string.Empty;
                            switch (uid)
                            {
                                case eUID4VM.GOLA_HostName: 
                                case eUID4VM.GOAL_Label: datatype = eDATATYPE._str; break;                                    
                                case eUID4VM.GOAL_PosX: case eUID4VM.GOAL_PosY: case eUID4VM.GOAL_PosR:
                                case eUID4VM.GOAL_EscapeX: case eUID4VM.GOAL_EscapeY: case eUID4VM.GOAL_EscapeR: datatype = eDATATYPE._int; break;
                                case eUID4VM.GOAL_LIST: case eUID4VM.GOAL_ITEMS:
                                case eUID4VM.GOAL_ADD: case eUID4VM.GOAL_DEL:
                                default: break;
                            }

                            switch (datatype)
                            {                                                                
                                case eDATATYPE._int:
                                    switch (uid)
                                    {                                        
                                        case eUID4VM.GOAL_PosX: case eUID4VM.GOAL_PosY: case eUID4VM.GOAL_PosR:
                                        case eUID4VM.GOAL_EscapeX: case eUID4VM.GOAL_EscapeY: case eUID4VM.GOAL_EscapeR:
                                            {
                                                Keypad keypadWindow = new Keypad(strCurr);
                                                if (keypadWindow.ShowDialog() == true)
                                                {
                                                    var chk = _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.View2Model, uid, $"{currItem.type};{currItem.label};{keypadWindow.Result};");
                                                    if (true == chk)
                                                    {
                                                        var temp = new TreeData() { b_Name = currItem.label, b_Parent = currItem.type.ToString() };
                                                        _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.Model2View, eUID4VM.GOAL_ITEMS, temp);
                                                    }
                                                }
                                                break;
                                            }
                                        default: break;
                                    }
                                    break;
                                case eDATATYPE._str:
                                    {
                                        switch (uid)
                                        {
                                            case eUID4VM.GOLA_HostName: case eUID4VM.GOAL_Label:
                                                {
                                                    VirtualKeyboard keyboardWindow = new VirtualKeyboard(strCurr);
                                                    if (keyboardWindow.ShowDialog() == true)
                                                    {                                                        
                                                        var chk = _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.View2Model, uid, $"{currItem.type};{currItem.name};{keyboardWindow.Result};");
                                                        if (true == chk)
                                                        {
                                                            var temp = new TreeData() { b_Name = currItem.name, b_Parent = currItem.type.ToString() };
                                                            _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.Model2View
                                                                , uid == eUID4VM.GOLA_HostName ? eUID4VM.GOAL_ITEMS : eUID4VM.GOAL_LIST, temp);
                                                        }
                                                    }
                                                }
                                                break;
                                            default: break;
                                        }
                                        break;
                                    }
                                case eDATATYPE._bool:
                                    break;
                                case eDATATYPE._float:                             
                                case eDATATYPE._double:
                                    break;
                                default: break;
                            }
                            break;
                        }
                    default: break;
                }
                
            }
            else
            {
                currItem = null;
                b_Goaltype = new eGOALTYPE();
                b_LineSel = new eLINE();
                b_GoalInfo = b_HostName = b_Label = string.Empty;
                b_eMCType = eMFC_MCTYPE.NONE;
            }
        }

        GOALITEM currItem = new GOALITEM();
        eGOALTYPE currType = new eGOALTYPE();
        eLINE currLine = eLINE.None;
        private void On_ItemChangedMethod(object obj)
        {
            if (obj != null)
            {                
                TreeData SelTreeData = (TreeData)obj;                
                if (SelTreeData.b_Children.Count == 0)
                {
                    _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.View2Model, eUID4VM.GOAL_ITEMS, currItem);                    
                    _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.Model2View, eUID4VM.GOAL_ITEMS, SelTreeData);                    
                }  
                else
                {
                    currLine = GetLine(SelTreeData.b_Name);
                    if ( eLINE.None == currLine)
                    {
                        currType = SelTreeData.b_Name.ToEnum<eGOALTYPE>();
                        b_GoalInfo = $"{currType}, ----, ----";
                        b_Goaltype = new eGOALTYPE();
                        b_LineSel = new eLINE();
                        b_HostName = b_Label = string.Empty;
                    }       
                    else
                    {
                        b_GoalInfo = $"{currType}, {currLine}, ----";
                        b_Goaltype = new eGOALTYPE();
                        b_LineSel = new eLINE();
                        b_HostName = b_Label = string.Empty;
                    }
                }
            }
        }

        private void On_btnAddMethod(object obj)
        {
            switch (currLine)
            {
                case eLINE.None: msgBox.ShowDialog($"라인을 선택해 주세요.", MsgBox.MsgType.Warn, MsgBox.eBTNSTYLE.OK); break;                
                default:
                    VirtualKeyboard keyboardWindow = new VirtualKeyboard("");
                    if (keyboardWindow.ShowDialog() == true)
                    {
                        var rtn = _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.View2Model, eUID4VM.GOAL_ADD
                                                               , new GOALITEM() { name = keyboardWindow.Result, type = currItem.type, line = currLine, label = keyboardWindow.Result });
                        if (true == rtn)
                        {
                            _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.Model2View, eUID4VM.GOAL_LIST);
                        }
                        else
                        {
                            msgBox.ShowDialog($"{ currItem.type}-{keyboardWindow.Result}의 골 등록에 실패하였습니다.", MsgBox.MsgType.Warn, MsgBox.eBTNSTYLE.OK);
                        }
                    }
                    /*var edit = msgBox.ShowInputBoxDlg(MaterialDesignThemes.Wpf.PackIconKind.DatabaseEdit, $"Goal Editor");
                    switch (edit.rtn)
                    {
                        case MsgBox.eBTNTYPE.OK:
                            {
                                
                                break;
                            }
                        default: break;
                    }*/
                    break;
            }            
        }

        private void On_btnDelMethod(object obj)
        {
            switch (currLine)
            {
                case eLINE.None: msgBox.ShowDialog($"라인을 선택해 주세요.", MsgBox.MsgType.Warn, MsgBox.eBTNSTYLE.OK); break;
                default:
                    var del = msgBox.ShowDialog($"{currItem.type}-{currItem.name}의 골을 제거하시겟습니까?", MsgBox.MsgType.Warn, MsgBox.eBTNSTYLE.OK_CANCEL);
                    switch (del)
                    {
                        case MsgBox.eBTNTYPE.OK:
                            var rtn = _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.View2Model, eUID4VM.GOAL_DEL, currItem);
                            if (true == rtn)
                            {
                                _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.Model2View, eUID4VM.GOAL_LIST);
                            }
                            else
                            {
                                msgBox.ShowDialog($"{ currItem.type}-{currItem.name}의 골 제거에 실패하였습니다.", MsgBox.MsgType.Warn, MsgBox.eBTNSTYLE.OK);
                            }
                            break;
                        default: break;
                    }
                    break;
            }
        }

        private void On_cmbLineChangeMethod(object obj)
        {
            if (currItem.line != (eLINE)obj)
            {
                _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.View2Model, eUID4VM.GOAL_ITEMS, currItem);
                _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.Model2View, eUID4VM.GOAL_LIST);
            }            
        }

        private void On_cmbMCTypeSelectedChanged(object obj)
        {
            var type = (eMFC_MCTYPE)obj;
            if (currItem.mcType != type)
            {
                _ctrl.DoingDataExchage(eVIWER.Goal, eDATAEXCHANGE.View2Model, eUID4VM.GOAL_MCType, $"{currItem.type};{currItem.name};{type};");
            }
        }

        ObservableCollection<TreeData> treegoals = new ObservableCollection<TreeData>();
        public ObservableCollection<TreeData> b_TreeGoals
        {
            get { return treegoals; }
            set { treegoals = value; OnPropertyChanged("b_TreeGoals"); }
        }

        string goalinfo = string.Empty;
        public string b_GoalInfo
        {
            get { return goalinfo; }
            set { goalinfo = value; OnPropertyChanged("b_GoalInfo"); }
        }
        
        public eGOALTYPE b_Goaltype
        {
            get { return currItem.type; }
            set { currItem.type = value; OnPropertyChanged("b_Goaltype"); }
        }
        
        public eLINE b_LineSel
        {
            get { return currItem.line; }
            set { currItem.line = value; OnPropertyChanged("b_LineSel"); }
        }

        public eMFC_MCTYPE b_eMCType
        {
            get { return currItem.mcType; }
            set { currItem.mcType = value; OnPropertyChanged("b_MCType"); }
        }


        string hostname = string.Empty;
        public string b_HostName
        {
            get { return currItem.hostName; }
            set { currItem.hostName = value; OnPropertyChanged("b_HostName"); }
        }

        string label = string.Empty;
        public string b_Label
        {
            get { return currItem.label; }
            set { currItem.label = value; OnPropertyChanged("b_Label"); }
        }

        string posX = string.Empty;
        public string b_PosX
        {
            get { return posX; }
            set { posX = value; OnPropertyChanged("b_PosX"); }
        }

        string posY = string.Empty;
        public string b_PosY
        {
            get { return posY; }
            set { posY = value; OnPropertyChanged("b_PosY"); }
        }

        string posR = string.Empty;
        public string b_PosR
        {
            get { return posR; }
            set { posR = value; OnPropertyChanged("b_PosR"); }
        }

        string escapeX = string.Empty;
        public string b_escapeX
        {
            get { return escapeX; }
            set { escapeX = value; OnPropertyChanged("b_escapeX"); }
        }

        string escapeY = string.Empty;
        public string b_escapeY
        {
            get { return escapeY; }
            set { escapeY = value; OnPropertyChanged("b_escapeY"); }
        }

        string escapeR = string.Empty;
        public string b_escapeR
        {
            get { return escapeR; }
            set { escapeR = value; OnPropertyChanged("b_escapeR"); }
        }

    }
}
