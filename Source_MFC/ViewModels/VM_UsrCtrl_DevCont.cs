using MaterialDesignThemes.Wpf;
using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Source_MFC.ViewModels
{
    public class VM_UsrCtrl_DevCont : Notifier
    {
        public VM_UsrCtrl_DevCont(eDEV dev)
        {
            _Initialize(dev);
        }

        ~VM_UsrCtrl_DevCont()
        {
         
        }

        private void _Initialize(eDEV dev)
        {
            _DevType = dev;
            b_DevName = _DevType.ToString();
            SetConnection(false);
        }

        

        public void SetConnection(bool cont)
        {
            b_IconCont = (true == cont) ? PackIconKind.CastConnected : PackIconKind.CastOff;
            b_IconForeGround = (true == cont) ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.SlateGray);
            b_devNameforegroud = (true == cont) ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.SlateGray);
        }

        eDEV _DevType;
        string device = string.Empty;
        public string b_DevName
        {
            get {
                return device;
            }
            set {
                device = value;
                OnPropertyChanged();
            }
        }

        PackIconKind icon = PackIconKind.CastOff;
        public PackIconKind b_IconCont
        {
            get {
                return icon;
            }
            set {
                icon = value;
                OnPropertyChanged();
            }
        }

        SolidColorBrush msgforegroud;
        public SolidColorBrush b_IconForeGround
        {
            get {
                return msgforegroud;
            }
            set {
                msgforegroud = value;
                OnPropertyChanged();
            }
        }

        SolidColorBrush devNameforegroud;
        public SolidColorBrush b_devNameforegroud
        {
            get {
                return devNameforegroud;
            }
            set {
                devNameforegroud = value;
                OnPropertyChanged();
            }
        }
    }
}
