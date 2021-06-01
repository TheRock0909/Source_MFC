using MaterialDesignThemes.Wpf;
using Source_MFC.Views;
using Source_MFC.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace Source_MFC.Utils
{
    public class MsgBox
    {
        #region MsgBox.Inst 싱글톤 객체 생성
        private static volatile MsgBox instance;
        private static object syncObj = new object();
        public static MsgBox Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncObj)
                    {
                        if (instance == null)
                            instance = new MsgBox();
                    }
                }
                return instance;
            }
        }
        #endregion

        public event EventHandler<MSGBOXDATA> Evt_MsgBoxDataInit;
        public event EventHandler<INPUTBOXDATA> Evt_InputBoxDataInit;         

        public enum eBTNSTYLE { OK, OK_CANCEL, OK_CANCEL_RETRY, OK_CANCEL_RETRY_IGNORE }
        public enum eBTNTYPE { OK = 1, CANCEL, Retry, Ignore }
        public enum MsgType { Info, Warn, Error, }

        public eBTNTYPE btnRlt { get; set; }
        public string content { get; set; }

        public eBTNTYPE Show(string msg)
        {
            return Show(msg, MsgType.Info, eBTNSTYLE.OK);
        }

        public eBTNTYPE Show(string msg, MsgType type, eBTNSTYLE btns)
        {
            CloseAllMsgBox();
            frm_Msg msgBox = new frm_Msg();
            MSGBOXDATA msgBoxData = new MSGBOXDATA
            {
                message = msg,
                msgType = type,
                btnStyle = btns
            };

            Evt_MsgBoxDataInit?.Invoke(this, msgBoxData);
            msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            msgBox.Show();
            return btnRlt;
        }

        public eBTNTYPE ShowDialog(string msg)
        {
            CloseAllMsgBox();
            return ShowDialog(msg, MsgType.Info, eBTNSTYLE.OK);
        }

        public eBTNTYPE ShowDialog(string msg, MsgType type, eBTNSTYLE btns, Window win = null)
        {
            CloseAllMsgBox();
            frm_Msg msgBox = new frm_Msg();
            msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;            
            MSGBOXDATA msgBoxData = new MSGBOXDATA
            {
                message = msg,
                msgType = type,
                btnStyle = btns
            };
            Evt_MsgBoxDataInit?.Invoke(this, msgBoxData);
            //msgBox.SetTskProc(true);                                    
            msgBox.Topmost = true;            
            msgBox.ShowDialog();
            //msgBox.SetTskProc();
            return btnRlt;
        }

        public (eBTNTYPE rtn, string rlt) ShowInputBoxDlg(PackIconKind icon, string title)
        {
            frm_InputBox box = new frm_InputBox();
            INPUTBOXDATA inputBoxData = new INPUTBOXDATA
            {
                packIcon = icon,
                title = title
            };
            Evt_InputBoxDataInit?.Invoke(this, inputBoxData);
            box.Topmost = true;
            box.ShowDialog();
            return (btnRlt, content);
        }

        public void ShowNoti(string msg)
        {
            frm_Noti noti = new frm_Noti(msg);
            noti.Topmost = true;
            noti.Show();
        }

        public void ShowDialogNoti(string msg)
        {
            frm_Noti noti = new frm_Noti(msg);            
            noti.Topmost = true;
            noti.ShowDialog();            
        }

        public void CloseAllMsgBox()
        {
            foreach (frm_Msg window in Application.Current.Windows.OfType<frm_Msg>())
                window.Close();
            while (Application.Current.Windows.OfType<frm_Msg>().Count() > 0)
            {
                Thread.Sleep(1);
            }
        }

        public void CloseAllShortNoti()
        {
            foreach (frm_Noti window in Application.Current.Windows.OfType<frm_Noti>())
                window.Close();
            while (Application.Current.Windows.OfType<frm_Noti>().Count() > 0)
            {
                Thread.Sleep(1);
            }
        }

        public void CloseAllInputBox()
        {
            foreach (frm_InputBox window in Application.Current.Windows.OfType<frm_InputBox>())
                window.Close();
            while (Application.Current.Windows.OfType<frm_InputBox>().Count() > 0)
            {
                Thread.Sleep(1);
            }
        }

        public void CloseAllMessage()
        {
            CloseAllShortNoti();
            CloseAllMsgBox();
        }
    }
}
