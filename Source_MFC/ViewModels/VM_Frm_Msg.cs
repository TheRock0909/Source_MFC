using MaterialDesignThemes.Wpf;
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
using System.Windows.Threading;
using static Source_MFC.Utils.MsgBox;

namespace Source_MFC.ViewModels
{
    class VM_Frm_Msg : Notifier
    {
        public ICommand CmdWindowLoaded { get; set; }
        public ICommand Evt_BtnClicked { get; set; }
        public ICommand Evt_CmdBarMouseDown { get; set; }
        public eBTNSTYLE btnStyle { get; set; }
        public Action CloseAction { get; set; }
        public Action DragMoveAction { get; set; }

        MsgBox msgBox = MsgBox.Inst;
        MsgType type;
        DispatcherTimer tmrUpdate, tmrTskProc;
        DateTime createTime;
        
        public VM_Frm_Msg()
        {
            CmdWindowLoaded = new Command(WindowLoaded);
            Evt_BtnClicked = new Command(On_BtnClicked);
            Evt_CmdBarMouseDown = new Command(On_BarMouseDown);
            msgBox.Evt_MsgBoxDataInit += On_MsgBoxDataInit;
        }

        private void WindowLoaded(object obj)
        {
            string[] split = b_Msg.Split('\r');
            b_WinHgt = (split.Length * 120) + 80;            
        }

        private void On_BtnClicked(object obj)
        {
            msgBox.btnRlt = (eBTNTYPE)Convert.ToInt32(obj);
            b_Msg = string.Empty;
            tmrUpdate.Stop();
            SetTskProc();
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

        private void On_MsgBoxDataInit(object sender, MSGBOXDATA e)
        {
            b_Msg = e.message;
            type = e.msgType;
            btnStyle = e.btnStyle;
            BtnSet();
        }

        bool bTogle = false;
        private void Tmr_Tick(object sender, EventArgs e)
        {
            var time = DateTime.Now - createTime;
            b_CurrTime = time.ToString(@"hh\:mm\:ss");
            switch (type)
            {
                case MsgType.Info:
                    b_MsgBackGround = new SolidColorBrush(Colors.White);
                    b_MsgForeGround = new SolidColorBrush(Colors.Black);
                    break;
                case MsgType.Warn:
                    b_MsgBackGround = new SolidColorBrush(Colors.Orange);
                    b_MsgForeGround = new SolidColorBrush(Colors.WhiteSmoke);
                    break;
                case MsgType.Error:
                    b_MsgBackGround = bTogle ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Yellow);
                    b_MsgForeGround = bTogle ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Red);
                    break;
            }

            var state = _Data.Inst.status.eqpState;
            switch (type)
            {
                case MsgType.Error:
                    switch (state)
                    {
                        case eEQPSATUS.Error: case eEQPSATUS.EMG: break;
                        default: msgBox.btnRlt = eBTNTYPE.OK; CloseAction(); break;
                    }
                    break;
                default: break;
            }
            bTogle ^= true;
        }

        private void BtnSet()
        {
            switch (type)
            {
                case MsgType.Info: icon_kind = PackIconKind.Information; break;
                case MsgType.Warn: icon_kind = PackIconKind.Warning; break;
                case MsgType.Error: icon_kind = PackIconKind.ErrorOutline; break;
            }

            switch (btnStyle)
            {
                case eBTNSTYLE.OK:
                    b_Vsb_BtnOK = Visibility.Visible;
                    b_Col_BtnOK = 1;
                    b_ColSpan_BtnOK = 2;
                    b_Row_BtnOK = 0;
                    break;
                case eBTNSTYLE.OK_CANCEL:
                    b_Vsb_BtnOK = Visibility.Visible;
                    b_Vsb_BtnCancel = Visibility.Visible;

                    b_Col_BtnOK = 0;
                    b_ColSpan_BtnOK = 2;
                    b_Row_BtnOK = 0;

                    b_Col_BtnCancel = 2;
                    b_ColSpan_BtnCancel = 2;
                    b_Row_BtnCancel = 0;
                    break;
                case eBTNSTYLE.OK_CANCEL_RETRY:
                    b_Vsb_BtnOK = Visibility.Visible;
                    b_Vsb_BtnCancel = Visibility.Visible;
                    b_Vsb_BtnRetry = Visibility.Visible;

                    b_Col_BtnOK = 0;
                    b_ColSpan_BtnOK = 2;
                    b_Row_BtnOK = 0;

                    b_Col_BtnCancel = 1;
                    b_ColSpan_BtnCancel = 2;
                    b_Row_BtnCancel = 0;

                    b_Col_BtnRetry = 2;
                    b_ColSpan_BtnRetry = 2;
                    b_Row_BtnRetry = 0;
                    break;
                case eBTNSTYLE.OK_CANCEL_RETRY_IGNORE:
                    b_Vsb_BtnOK = Visibility.Visible;
                    b_Vsb_BtnCancel = Visibility.Visible;
                    b_Vsb_BtnRetry = Visibility.Visible;
                    b_Vsb_BtnIgnore = Visibility.Visible;

                    b_Col_BtnOK = 0;
                    b_Row_BtnOK = 0;

                    b_Col_BtnCancel = 1;
                    b_Row_BtnCancel = 0;

                    b_Col_BtnRetry = 2;
                    b_Row_BtnRetry = 0;

                    b_Col_BtnIgnore = 3;
                    b_Row_BtnIgnore = 0;
                    break;
            }
            createTime = DateTime.Now;
            tmrUpdate = new DispatcherTimer();
            tmrUpdate.Interval = TimeSpan.FromMilliseconds(10);    //시간간격 설정
            tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가    
            tmrUpdate.Start();

            tmrTskProc = new DispatcherTimer();
            tmrUpdate.Interval = TimeSpan.FromMilliseconds(0.1);    //시간간격 설정

            SetTskProc(true);
        }

        public void SetTskProc(bool bEnb = false)
        {
            if (true == bEnb)
            {
                tmrTskProc.Start();
            }
            else
            {
                tmrTskProc.Stop();
            }
        }

        int win_Hgt = 200;
        public int b_WinHgt
        {
            get {
                return win_Hgt;
            }
            set {
                win_Hgt = value;
                OnPropertyChanged();
            }
        }

        string message = "test message";
        public string b_Msg
        {
            get {
                return message;
            }
            set {
                message = value;
                OnPropertyChanged();
            }
        }

        int col_BtnOK = 0, colSpan_BtnOK = 1, row_BtnOK = 0;
        Visibility vsb_BtnOK = Visibility.Hidden;
        public int b_Col_BtnOK
        {
            get { return col_BtnOK; }
            set { col_BtnOK = value; OnPropertyChanged(); }
        }
        public int b_ColSpan_BtnOK
        {
            get { return colSpan_BtnOK; }
            set { colSpan_BtnOK = value; OnPropertyChanged(); }
        }
        public int b_Row_BtnOK
        {
            get { return row_BtnOK; }
            set { row_BtnOK = value; OnPropertyChanged(); }
        }
        public Visibility b_Vsb_BtnOK
        {
            get { return vsb_BtnOK; }
            set {
                vsb_BtnOK = value; OnPropertyChanged();
            }
        }

        int col_BtnCancel = 0, colSpan_BtnCancel = 1, row_BtnCancel = 0;
        Visibility vsb_BtnCancel = Visibility.Hidden;
        public int b_Col_BtnCancel
        {
            get { return col_BtnCancel; }
            set { col_BtnCancel = value; OnPropertyChanged(); }
        }
        public int b_ColSpan_BtnCancel
        {
            get { return colSpan_BtnCancel; }
            set { colSpan_BtnCancel = value; OnPropertyChanged(); }
        }
        public int b_Row_BtnCancel
        {
            get { return row_BtnCancel; }
            set { row_BtnCancel = value; OnPropertyChanged(); }
        }
        public Visibility b_Vsb_BtnCancel
        {
            get { return vsb_BtnCancel; }
            set {
                vsb_BtnCancel = value; OnPropertyChanged();
            }
        }

        int col_BtnRetry = 0, colSpan_BtnRetry = 1, row_BtnRetry = 0;
        Visibility vsb_BtnRetry = Visibility.Hidden;
        public int b_Col_BtnRetry
        {
            get { return col_BtnRetry; }
            set { col_BtnRetry = value; OnPropertyChanged(); }
        }
        public int b_ColSpan_BtnRetry
        {
            get { return colSpan_BtnRetry; }
            set { colSpan_BtnRetry = value; OnPropertyChanged(); }
        }
        public int b_Row_BtnRetry
        {
            get { return row_BtnRetry; }
            set { row_BtnRetry = value; OnPropertyChanged(); }
        }
        public Visibility b_Vsb_BtnRetry
        {
            get { return vsb_BtnRetry; }
            set {
                vsb_BtnRetry = value; OnPropertyChanged();
            }
        }

        int col_BtnIgnore = 0, colSpan_BtnIgnore = 1, row_BtnIgnore = 0;
        Visibility vsb_BtnIgnore = Visibility.Hidden;
        public int b_Col_BtnIgnore
        {
            get { return col_BtnIgnore; }
            set { col_BtnIgnore = value; OnPropertyChanged(); }
        }
        public int b_ColSpan_BtnIgnore
        {
            get { return colSpan_BtnIgnore; }
            set { colSpan_BtnIgnore = value; OnPropertyChanged(); }
        }
        public int b_Row_BtnIgnore
        {
            get { return row_BtnIgnore; }
            set { row_BtnIgnore = value; OnPropertyChanged(); }
        }
        public Visibility b_Vsb_BtnIgnore
        {
            get { return vsb_BtnIgnore; }
            set {
                vsb_BtnIgnore = value; OnPropertyChanged();
            }
        }

        PackIconKind icon_kind;
        public PackIconKind b_Icon_Kind
        {
            get { return icon_kind; }
            set { icon_kind = value; OnPropertyChanged(); }
        }

        string currenttime = string.Empty;
        public string b_CurrTime
        {
            get { return currenttime;
            }
            set { currenttime = value; OnPropertyChanged();
            }
        }

        SolidColorBrush msgbackgroud;
        public SolidColorBrush b_MsgBackGround
        {
            get { return msgbackgroud;
            }
            set { msgbackgroud = value; OnPropertyChanged();
            }
        }

        SolidColorBrush msgforegroud;
        public SolidColorBrush b_MsgForeGround
        {
            get {
                return msgforegroud;
            }
            set {
                msgforegroud = value;
                OnPropertyChanged();
            }
        }
    }
}
