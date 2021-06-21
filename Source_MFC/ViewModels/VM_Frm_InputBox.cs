using KeyPad;
using MaterialDesignThemes.Wpf;
using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Source_MFC.ViewModels
{
    class VM_Frm_InputBox : Notifier
    {
        MsgBox msgBox = MsgBox.Inst;
        public Action CloseAction { get; set; }
        public Action DragMoveAction { get; set; }
        public ICommand Evt_CmdBarMouseDown { get; set; }
        public ICommand Evt_BtnClicked { get; set; }
        public ICommand Evt_DataExchange4InputBox { get; set; }
        public VM_Frm_InputBox()
        {
            msgBox.Evt_InputBoxDataInit += On_InputBoxDataInit;
            Evt_BtnClicked = new Command(On_BtnClicked);
            Evt_CmdBarMouseDown = new Command(On_BarMouseDown);
            Evt_DataExchange4InputBox = new Command(On_DataExchange4InputBox);
        }

        ~VM_Frm_InputBox()
        {
           
        }

        private void On_DataExchange4InputBox(object obj)
        {
            var uid = (eUID4VM)obj;
            On_DataExchange(null, (eDATAEXCHANGE.View2Model, uid));
        }

        private void On_DataExchange(object sender, (eDATAEXCHANGE dir, eUID4VM id) e)
        {
            switch (e.dir)
            {                
                case eDATAEXCHANGE.Model2View:
                    break;
                case eDATAEXCHANGE.View2Model:
                    {
                        if ( null != sender )
                        {
                            var datatype = eDATATYPE.NONE;
                            var strCurr = string.Empty;
                            switch (e.id)
                            {
                                case eUID4VM.ETC_INPUTBOX: datatype = eDATATYPE._str; break;
                                default: break;
                            }

                            switch (datatype)
                            {                                
                                case eDATATYPE._bool:
                                    break;
                                case eDATATYPE._int:
                                    break;
                                case eDATATYPE._str:
                                    {
                                        VirtualKeyboard keyboardWindow = new VirtualKeyboard(strCurr);
                                        if (keyboardWindow.ShowDialog() == true)
                                        {
                                            b_TxtContent = keyboardWindow.Result;
                                        }
                                        break;
                                    }
                                case eDATATYPE._float:
                                    break;
                                case eDATATYPE._double:
                                    break;
                                default: break;
                            }
                        }
                        break;
                    }
                default: break;
            }
        }

        private void On_BtnClicked(object obj)
        {
            msgBox.btnRlt = obj.ToString().ToEnum<MsgBox.eBTNTYPE>();
            msgBox.content = MsgBox.eBTNTYPE.CANCEL == msgBox.btnRlt ? null : b_TxtContent;
            if (string.IsNullOrEmpty(msgBox.content))
            {
                msgBox.btnRlt = MsgBox.eBTNTYPE.CANCEL;
                msgBox.content = null;
            }
            CloseAction();
        }

        private void On_BarMouseDown(object obj)
        {
            try
            {
                DragMoveAction();
            }
            catch
            {

            }
        }

        private void On_InputBoxDataInit(object sender, INPUTBOXDATA e)
        {
            b_IconKind = e.packIcon;
            b_Title = e.title;
        }

        PackIconKind iconKind;
        public PackIconKind b_IconKind
        {
            get {
                return iconKind;
            }
            set {
                iconKind = value;
                OnPropertyChanged();
            }
        }
        string title = string.Empty;
        public string b_Title
        {
            get {
                return title;
            }
            set {
                title = value;
                OnPropertyChanged();
            }
        }

        string txtContent = string.Empty;
        public string b_TxtContent
        {
            get {
                return txtContent;
            }
            set {
                txtContent = value;
                OnPropertyChanged();
            }
        }
    }
}
