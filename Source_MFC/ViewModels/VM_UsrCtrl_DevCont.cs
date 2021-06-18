using MaterialDesignThemes.Wpf;
using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Source_MFC.ViewModels
{
    public class VM_UsrCtrl_DevCont : Notifier
    {
        DispatcherTimer _tmrUpdate;
        PackIconKind icon = PackIconKind.CastOff;
        SolidColorBrush msgforegroud;
        SolidColorBrush devNameforegroud;
        public VM_UsrCtrl_DevCont(eDEV dev)
        {
            _Initialize(dev);
        }

        ~VM_UsrCtrl_DevCont()
        {
            _tmrUpdate.Tick -= Tmr_Tick;
            _tmrUpdate.Stop();
        }

        private void _Initialize(eDEV dev)
        {
            DevType = dev;
            _tmrUpdate = new DispatcherTimer();
            _tmrUpdate.Interval = TimeSpan.FromMilliseconds(500);    //시간간격 설정
            _tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가     
            _tmrUpdate.Start();
            SetConnection(false);
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            if (false == _Data.Inst.status.bLoaded) return;
            b_DevName = DevType.ToString();
            b_IconCont = icon;
            b_IconForeGround = msgforegroud;
            b_devNameforegroud = devNameforegroud;
        }

        public void SetConnection(bool cont)
        {
            icon = (true == cont) ? PackIconKind.CastConnected : PackIconKind.CastOff;
            msgforegroud = (true == cont) ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.SlateGray);
            devNameforegroud = (true == cont) ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.SlateGray);
        }

        eDEV DevType;
        string _Device = string.Empty;
        public string b_DevName
        {
            get { return _Device; }
            set { this.MutateVerbose(ref _Device, value, RaisePropertyChanged()); }
        }

        PackIconKind _Icon = PackIconKind.CastOff;
        public PackIconKind b_IconCont
        {
            get { return _Icon; }
            set { this.MutateVerbose(ref _Icon, value, RaisePropertyChanged()); }
        }

        SolidColorBrush _msgforegroud;
        public SolidColorBrush b_IconForeGround
        {
            get { return _msgforegroud; }
            set { this.MutateVerbose(ref _msgforegroud, value, RaisePropertyChanged()); }
        }

        SolidColorBrush _DevNameforegroud;
        public SolidColorBrush b_devNameforegroud
        {
            get { return _DevNameforegroud; }
            set { this.MutateVerbose(ref _DevNameforegroud, value, RaisePropertyChanged()); }
        }
    }
}
