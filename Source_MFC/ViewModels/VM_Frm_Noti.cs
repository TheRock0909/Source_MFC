using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.ViewModels
{
    class VM_Frm_Noti : Notifier
    {
        MsgBox msgBox = MsgBox.Inst;
        public Action CloseAction { get; set; }

        public VM_Frm_Noti(string msg)
        {
            b_TxtContent = msg;
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
