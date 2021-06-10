using MaterialDesignThemes.Wpf;
using Source_MFC.Global;
using Source_MFC.Utils;
using Source_MFC.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Source_MFC.ViewModels
{
    public class _VM_MainWindow : Notifier
    {
        public ICommand Evt_BtnMenuOpen { get; set; }
        public ICommand Evt_BtnMenuClose { get; set; }
        public ICommand Evt_BtnMainChange { get; set; }
        public ICommand Evt_BtnPopUp { get; set; }
        public ICommand Evt_BarMouseDown { get; set; }
        public Action DragMoveAction { get; set; }
        public Action CloseMenuAction { get; set; }

        MainWindow _mainWin;
        MainCtrl _ctrl;
        MsgBox msgBox = MsgBox.Inst;
        private Logger _log = Logger.Inst;
        private STATUS _status = _Data.Inst.status;        
        DispatcherTimer _tmrUpdate;
        private Dictionary<eDEV, VM_UsrCtrl_DevCont> _DicDevsConn = new Dictionary<eDEV, VM_UsrCtrl_DevCont>();        
        public _VM_MainWindow(MainWindow main)
        {                        
            _mainWin = main;
            _ctrl = new MainCtrl(main);
            msgBox.ShowNoti($"{_ctrl._EQPName} 프로그램 로딩 중...");            
            _mainWin.Visibility = Visibility.Collapsed;
            b_usrCtrl_Logs = new UsCtrl_Logs(_ctrl);
            b_MenuItem = new[]
            {
                  new ItemMenu(ePAGE.DashBoard, PackIconKind.ViewDashboard, Visibility.Collapsed, subItem = new[]
                  {
                      new SubItem( eVIWER.Monitor, PackIconKind.Monitor, new UsCtrl_Dash_Moni(_ctrl)),
                      new SubItem( eVIWER.Manual, PackIconKind.CarManualTransmission, new UsCtrl_Dash_Manual(_ctrl))
                  })
                , new ItemMenu(ePAGE.System, PackIconKind.Pencil, Visibility.Collapsed, subItem = new[]
                  {
                      new SubItem( eVIWER.IO, PackIconKind.Toolbox, new UsCtrl_Sys_IO(_ctrl)),
                      new SubItem( eVIWER.TowerLamp, PackIconKind.Lamps, new UsCtrl_Sys_Lamp(_ctrl)),
                      new SubItem( eVIWER.Goal, PackIconKind.Target, new UsCtrl_Sys_Goal(_ctrl)),
                      new SubItem( eVIWER.PIO, PackIconKind.NetworkInterfaceCard, new UsCtrl_Sys_PIO(_ctrl)),
                      new SubItem( eVIWER.FAC, PackIconKind.Settings, new UsCtrl_Sys_FAC(_ctrl))
                  })
            };
            userControl = b_MenuItem[0].SubItems[0].Screen;
            _Initialize();
            msgBox.CloseAllShortNoti();
            _mainWin.Visibility = Visibility.Visible;
        }

        ~_VM_MainWindow()
        {
            _ctrl.Evt_MainWin_DataExchange -= On_DataExchange;
            _ctrl.Evt_UpdateConnection -= On_DevConnection;
            _tmrUpdate.Tick -= Tmr_Tick;
            _tmrUpdate.Stop();
        }

        UsCtrl_DevCont[] _Dev;
        private void _Initialize()
        {
            _status.bLoaded = false;
            _ctrl.Evt_MainWin_DataExchange += On_DataExchange;
            _ctrl.Evt_UpdateConnection += On_DevConnection;

            Evt_BtnMenuOpen = new Command(On_OpenMenu, CanExecute4Btn);
            Evt_BtnMenuClose = new Command(On_CloseMenu, CanExecute4Btn);
            Evt_BtnMainChange = new Command(On_ChangeViewer, CanExecute4ListBox);
            Evt_BtnPopUp = new Command(On_PopUpMethod, CanExecute4Btn);
            Evt_BarMouseDown = new Command(On_WinMoveByMouseDown, CanExecute4Btn);

            _Dev = new UsCtrl_DevCont[Enum.GetNames(typeof(eDEV)).Length];
            _Dev[(int)eDEV.MPlus] = new UsCtrl_DevCont(eDEV.MPlus);
            _Dev[(int)eDEV.Vehicle] = new UsCtrl_DevCont(eDEV.Vehicle);
            _Dev[(int)eDEV.IO] = new UsCtrl_DevCont(eDEV.IO);

            _mainWin.pnl_Dev_0.Children.Clear();                        
            _mainWin.pnl_Dev_0.Children.Add(_Dev[(int)eDEV.MPlus]);
            _mainWin.pnl_Dev_0.Children.Add(_Dev[(int)eDEV.Vehicle]);

            _mainWin.pnl_Dev_1.Children.Clear();
            _mainWin.pnl_Dev_1.Children.Add(_Dev[(int)eDEV.IO]);

            _tmrUpdate = new DispatcherTimer();
            _tmrUpdate.Interval = TimeSpan.FromMilliseconds(10);    //시간간격 설정
            _tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가  
            _tmrUpdate.Start();

            // View 로딩완료
            _ctrl.View_Init(); 
            _ctrl.Hw_Init();
            _ctrl.Sw_Init();
            _status.bLoaded = true;
            _status.swVer = "Ver.20210430";
            b_title = $"{_ctrl._EQPName}";            
        }

        private void _Finalize()
        {
            _log.Write(CmdLogType.prdt, $"Application을 종료합니다. [{_ctrl._EQPName}:{_status.swVer}]");            
            _ctrl._Finalize();
            Application.Current.Shutdown();
        }

        bool bTogle = false;
        int nGCCallCnt = 0;
        TIMEARG _1secTimer = new TIMEARG() { nDelay = 1000 } ;
        TIMEARG _500msecTimer = new TIMEARG() { nDelay = 500 };
        private void Tmr_Tick(object sender, EventArgs e)
        {
            if (_1secTimer.IsOver(_1secTimer.nDelay))
            {
                b_CurrTime = $" {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                var memory = Ctrls.FormatBytes(Process.GetCurrentProcess().PrivateMemorySize64);
                b_SwVer = $" {_status.swVer} {memory}/{_ctrl._backProcScan} msec/{_ctrl._SeqScanTime}";
                switch (_status.eqpState)
                {
                    case eEQPSATUS.Init: b_brush_lmp = new SolidColorBrush(Colors.LightGray); break;
                    case eEQPSATUS.Stop: b_brush_lmp = new SolidColorBrush(Colors.Orange); break;
                    case eEQPSATUS.Initing: case eEQPSATUS.Stopping:
                        b_brush_lmp = bTogle ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.Lime);
                        break;
                    case eEQPSATUS.Run: b_brush_lmp = new SolidColorBrush(Colors.Lime); break;                                        
                    case eEQPSATUS.Idle:
                        b_brush_lmp = bTogle ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(Colors.Lime);
                        break;
                    case eEQPSATUS.Error: case eEQPSATUS.EMG:
                        b_brush_lmp = bTogle ? new SolidColorBrush(Colors.Tomato) : new SolidColorBrush(Colors.Red);
                        break;
                    default: break;
                }
                bTogle ^= true;

//                 nGCCallCnt++;
//                 if ( 5 <= nGCCallCnt)
//                 {
//                     nGCCallCnt = 0;
//                     System.GC.Collect();
//                     System.GC.WaitForPendingFinalizers();
//                }                
            }

            if (_500msecTimer.IsOver(_500msecTimer.nDelay))
            {

            }
        }

        private void On_OpenMenu(object obj)
        {
            Vsb_OpenMenu = Visibility.Collapsed;
            Vsb_CloseMenu = Visibility.Visible;
            Vsb_ExpanderMenu = Visibility.Visible;
        }

        private void On_CloseMenu(object obj)
        {
            Vsb_OpenMenu = Visibility.Visible;
            Vsb_CloseMenu = Visibility.Collapsed;
            Vsb_ExpanderMenu = Visibility.Collapsed;
        }

        private void On_ChangeViewer(object obj)
        {
            if (b_UsrCtrl_View != (UserControl)obj)
            {
                b_UsrCtrl_View = (UserControl)obj;
                var uid = b_UsrCtrl_View.Uid.ToEnum<eVIWER>();
                _ctrl.DoingDataExchage(uid, eDATAEXCHANGE.Model2View);                
                _ctrl.DoingDataExchage(eVIWER.IO, eDATAEXCHANGE.Model2View, eUID4VM.IO_ResetDirectIO);
            }
        }

        frm_User user;
        private void On_PopUpMethod(object obj)
        {
            eUID4VM btn = (eUID4VM)Convert.ToInt32(obj);
            switch (btn)
            {
                case eUID4VM.MAINWIN_Popup_Logout:
                    {                        
                        On_CloseMenu(null);
                        CloseMenuAction();
                        SetLoginUser(null, true);
                        switch (_ctrl.CurrView)
                        {
                            case eVIWER.Monitor: break;
                            default: On_ChangeViewer(b_MenuItem[0].SubItems[0].Screen); break;
                        }
                        break;
                    }
                case eUID4VM.MAINWIN_Popup_Login:
                case eUID4VM.MAINWIN_Popup_Account:
                    {
                        user = new frm_User(_ctrl, btn == eUID4VM.MAINWIN_Popup_Login);
                        user.Topmost = true;
                        user.ShowDialog();
                        break;
                    }
                case eUID4VM.MAINWIN_Popup_Save:
                    {
                        switch (_ctrl.CurrView)
                        {
                            //_log.Write(CmdLogType.prdt, $"Model Data를 저장합니다. [{mainSequence._sysStatus.currMdlFile}]");
                            case eVIWER.None: break;// 모델데이터를 저장 시 추가 코딩필요.                                                                                   
                            default:
                                _Data.Inst.Save();
                                _log.Write(CmdLogType.prdt, $"Config Data를 저장합니다. [{_status.user.grade}/{_status.user.id}]");
                                break;
                        }
                        break;
                    }
                case eUID4VM.MAINWIN_Popup_Minimize:
                    {
                        var win = Window.GetWindow(_mainWin);
                        switch (win.WindowState)
                        {
                            case WindowState.Minimized: win.WindowState = WindowState.Maximized; break;
                            case WindowState.Normal:
                            case WindowState.Maximized: win.WindowState = WindowState.Minimized; break;
                        }
                        break;
                    }
                case eUID4VM.MAINWIN_Popup_Shutdown:
                    {
                        Helper.ShowBlurEffectAllWindow();
                        var rtn = msgBox.ShowDialog($"Application을 종료하시겠습니까?", MsgBox.MsgType.Info, MsgBox.eBTNSTYLE.OK_CANCEL);
                        switch (rtn)
                        {
                            case MsgBox.eBTNTYPE.OK: _Finalize(); break;
                            default: Helper.StopBlurEffectAllWindow(); break;
                        }
                        break;
                    }
                default: break;
            }
        }

        private void On_WinMoveByMouseDown(object obj)
        {
            DragMoveAction();
        }

        private bool CanExecute4ListBox(object arg)
        {
            switch (_status.eqpState)
            {
                case eEQPSATUS.Initing:
                case eEQPSATUS.Stopping:
                case eEQPSATUS.Idle:
                case eEQPSATUS.Run:
                case eEQPSATUS.EMG:
                case eEQPSATUS.Error: return false;
                default: return true;
            }
        }

        private bool CanExecute4Btn(object arg)
        {
            eUID4VM btn = (eUID4VM)Convert.ToInt32(arg);
            switch (_status.eqpState)
            {
                case eEQPSATUS.Initing:
                case eEQPSATUS.Stopping:
                case eEQPSATUS.Idle:
                case eEQPSATUS.Run:
                case eEQPSATUS.EMG:
                case eEQPSATUS.Error:
                    switch (btn)
                    {             
                        case eUID4VM.MAINWIN_CloseMenu:
                        case eUID4VM.MAINWIN_OpenMenu:
                        case eUID4VM.MAINWIN_Popup_Login:
                        case eUID4VM.MAINWIN_Popup_Logout:
                        case eUID4VM.MAINWIN_Popup_Account:
                        case eUID4VM.MAINWIN_Popup_Save:
                        case eUID4VM.MAINWIN_Popup_Shutdown: return false;
                        case eUID4VM.MAINWIN_Popup_Minimize:
                        default: return true;
                    }
                default: return true;
            }            
        }

        private void On_DataExchange(object sender, (eDATAEXCHANGE dir, eUID4VM id)e)
        {
            switch (e.dir)
            {                
                case eDATAEXCHANGE.Model2View:
                    switch (e.id)
                    {
                        case eUID4VM.MAINWIN_ALL:
                            break;
                        case eUID4VM.MAINWIN_EqpState:
                            break;
                        case eUID4VM.MAINWIN_User:
                            {
                                var user = sender as USER;
                                SetLoginUser(user);
                                break;
                            }
                    }
                    break;
                case eDATAEXCHANGE.View2Model:
                    break;                
                default: break;
            }
        }

        public void SetLoginUser(USER user, bool bIsLogOut = false)
        {
            if (false == bIsLogOut)
            {
                _status.User_Set(user);
                b_LoginGrade = user.grade.ToString();
                if (!string.IsNullOrEmpty(user.id))
                {
                    b_LoginID = user.id;
                    _log.Write(CmdLogType.prdt, $"작업자가 로그인 하였습니다. [{user.grade}/{user.id}]");
                }
                else
                {
                    b_LoginID = $"------";
                }                                
            }
            else
            {
                _log.Write(CmdLogType.prdt, $"작업자가 로그아웃 하였습니다. [{_status._UserGrade}/{_status.user.id}]");
                _status.User_Set(null, true);
                b_LoginGrade = b_LoginID = $"------";
            }
        }

        public void On_DevConnection(object sender, (eDEV dev, bool Connection) info)
        {
            var Dev = _DicDevsConn[info.dev];
            Dev.SetConnection(info.Connection);
        }

        public SubItem[] subItem { get; }
        ItemMenu[] menuitem;
        public ItemMenu[] b_MenuItem
        {
            get {
                return menuitem;
            }
            set {
                menuitem = value;
                OnPropertyChanged();
            }
        }

        UserControl userControl;// = new UsCtrl_Dash_Moni();
        public UserControl b_UsrCtrl_View
        {
            get {
                return userControl;
            }
            set {
                userControl = value;
                OnPropertyChanged();
            }

        }

        UserControl usrCtrl_Logs;
        public UserControl b_usrCtrl_Logs
        {
            get {
                return usrCtrl_Logs;
            }
            set {
                usrCtrl_Logs = value;
                OnPropertyChanged();
            }
        }

        string logingrade = $"------";
        public string b_LoginGrade
        {
            get {
                return logingrade;
            }
            set {
                logingrade = value;
                OnPropertyChanged();
            }
        }

        string loginid = $"------";
        public string b_LoginID
        {
            get {
                return loginid;
            }
            set {
                loginid = value;
                OnPropertyChanged();
            }
        }

        string title = string.Empty;
        public string b_title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(); }
        }

        string swVer = string.Empty;
        public string b_SwVer
        {
            get { return swVer; }
            set {
                swVer = value;
                OnPropertyChanged();
            }
        }

        string currTime = string.Empty;
        public string b_CurrTime
        {
            get { return currTime; }
            set {
                currTime = value;
                OnPropertyChanged();
            }
        }


        Visibility openMenuVisibillity = Visibility.Visible;
        public Visibility Vsb_OpenMenu
        {
            get {
                return openMenuVisibillity;
            }
            set {
                openMenuVisibillity = value;
                OnPropertyChanged();
            }
        }

        Visibility closeMenuVisibillity = Visibility.Collapsed;
        public Visibility Vsb_CloseMenu
        {
            get {
                return closeMenuVisibillity;
            }
            set {
                closeMenuVisibillity = value;
                OnPropertyChanged();
            }
        }

        Visibility expanderMenuVisibillity = Visibility.Collapsed;
        public Visibility Vsb_ExpanderMenu
        {
            get {
                return expanderMenuVisibillity;
            }
            set {
                expanderMenuVisibillity = value;
                OnPropertyChanged();
            }
        }

        SolidColorBrush brush_lmp;
        public SolidColorBrush b_brush_lmp
        {
            get {
                return brush_lmp;
            }
            set {
                brush_lmp = value; OnPropertyChanged();
            }
        }
    }
}
