using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Source_MFC.Utils.MsgBox;

namespace Source_MFC.ViewModels
{
    class VM_Frm_User : Notifier
    {
        public ICommand Evt_BtnClicked { get; set; }
        public ICommand Evt_CmbSelechanged { get; set; }
        public IEnumerable<eOPRGRADE> eOprgrade { get; set; }
        public Action CloseAction { get; set; }

        MsgBox msgBox = MsgBox.Inst;
        private Logger _log = Logger.Inst;
        public USERINFO userinfo;
        MainCtrl _ctrl;        
        public VM_Frm_User(MainCtrl ctrl, bool bCreateAccount)
        {
            _ctrl = ctrl;
            userinfo = _Data.Inst.sys.user;
            Evt_BtnClicked = new Command(On_BtnClicked);
            Evt_CmbSelechanged = new Command(On_CmbSelchanged);
            if (bCreateAccount)
            {
                Vsb_LoginGrd = Visibility.Visible;
            }
            else
            {
                Vsb_Openregister = Visibility.Visible;
                On_BtnClicked("btn_openregister");
            }
        }

        eOPRGRADE cmb_CurrUser;
        private void On_CmbSelchanged(object obj)
        {
            cmb_CurrUser = (eOPRGRADE)obj;
        }

        private void On_BtnClicked(object obj)
        {
            switch (obj)
            {
                case "btn_Go":
                    var user = userinfo.LogIn(b_User, b_PassWord);
                    if (string.IsNullOrEmpty(user.msg))
                    {
                        _ctrl.UpdateUserData(user);
                        On_BtnClicked("btn_Exit");
                    }
                    else
                    {
                        var rtn = msgBox.ShowDialog(user.msg, MsgType.Warn, eBTNSTYLE.OK);
                    }
                    break;
                case "btn_openregister":
                    Vsb_RegisterGrd = Visibility.Visible;
                    Vsb_Openregister = Visibility.Collapsed;
                    b_ExitForeground = new SolidColorBrush(Colors.WhiteSmoke);
                    b_MainRectFill = new SolidColorBrush(Colors.PowderBlue);
                    eOprgrade = Enum.GetValues(typeof(eOPRGRADE)).Cast<eOPRGRADE>();
                    break;
                case "btn_Edit":
                    var newuser = new USER();
                    newuser.id = b_NewUser;
                    newuser.password = b_NewPassWord;
                    if (!string.IsNullOrEmpty(newuser.id) && !string.IsNullOrEmpty(newuser.password))
                    {
                        newuser.grade = cmb_CurrUser;
                        userinfo.Add(newuser);
                        _log.Write(CmdLogType.prdt, $"신규 User를 등록하였습니다. [{newuser.grade}/{newuser.id}]");
                        USERINFO.Save(userinfo);
                        On_BtnClicked("btn_Exit");
                    }
                    break;
                case "btn_Exit":
                    CloseAction();
                    break;
                case "btn_Back":
                    Vsb_RegisterGrd = Visibility.Collapsed;
                    Vsb_Openregister = Visibility.Visible;
                    b_ExitForeground = new SolidColorBrush(Color.FromRgb(236, 97, 97));
                    b_MainRectFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    break;
            }
        }

        string userName = string.Empty;
        public string b_User
        {
            get {
                return userName;
            }
            set {
                userName = value;
                OnPropertyChanged();
            }
        }

        string passWord = string.Empty;
        public string b_PassWord
        {
            get {
                return passWord;
            }
            set {
                passWord = value;
                OnPropertyChanged();
            }
        }

        string newuserName = string.Empty;
        public string b_NewUser
        {
            get {
                return newuserName;
            }
            set {
                newuserName = value;
                OnPropertyChanged();
            }
        }

        string newpassWord = string.Empty;
        public string b_NewPassWord
        {
            get {
                return newpassWord;
            }
            set {
                newpassWord = value;
                OnPropertyChanged();
            }
        }

        Visibility openregistervisibility = Visibility.Hidden;
        public Visibility Vsb_Openregister
        {
            get {
                return openregistervisibility;
            }
            set {
                openregistervisibility = value;
                OnPropertyChanged();
            }
        }

        Visibility backvisibility = Visibility.Hidden;
        public Visibility Vsb_Background
        {
            get {
                return backvisibility;
            }
            set {
                backvisibility = value;
                OnPropertyChanged();
            }
        }

        Visibility logingridvisibility = Visibility.Hidden;
        public Visibility Vsb_LoginGrd
        {
            get {
                return logingridvisibility;
            }
            set {
                logingridvisibility = value;
                OnPropertyChanged();
            }
        }

        Visibility registergridvisibility = Visibility.Hidden;
        public Visibility Vsb_RegisterGrd
        {
            get {
                return registergridvisibility;
            }
            set {
                registergridvisibility = value;
                OnPropertyChanged();
            }
        }


        SolidColorBrush exitforeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF285A8B"));
        public SolidColorBrush b_ExitForeground
        {
            get {
                return exitforeground;
            }
            set {
                exitforeground = value;
                OnPropertyChanged();
            }
        }

        SolidColorBrush mainrectanglefill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF4F4F5"));
        public SolidColorBrush b_MainRectFill
        {
            get {
                return mainrectanglefill;
            }
            set {
                mainrectanglefill = value;
                OnPropertyChanged();
            }
        }
    }
}
