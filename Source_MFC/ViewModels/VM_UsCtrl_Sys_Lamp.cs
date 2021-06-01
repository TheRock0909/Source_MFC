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
    class VM_UsCtrl_Sys_Lamp : Notifier
    {
        MainCtrl _ctrl;
        public ICommand Evt_lstbox_SelectedItemChanged { get; set; }
        public ICommand Evt_rdo_Changed { get; set; }
        int twrLmp_SelectedItem = 0;
        LAMPINFO _lmp ;
        public VM_UsCtrl_Sys_Lamp(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _lmp = _Data.Inst.sys.lmp;
            _ctrl.Evt_Sys_Lamp_DataExchange += On_DataExchange;
            Evt_lstbox_SelectedItemChanged = new Command(ExecuteMethod, CanExecuteMethod);
            Evt_rdo_Changed = new Command(On_RdoChanged);
            b_SubItems = new[]
            {
                  new SubItem($" {eEQPSATUS.Init}", MaterialDesignThemes.Wpf.PackIconKind.LockReset)
                , new SubItem($" {eEQPSATUS.Initing}", MaterialDesignThemes.Wpf.PackIconKind.ShoeRunning)
                , new SubItem($" {eEQPSATUS.Stop}", MaterialDesignThemes.Wpf.PackIconKind.Stopwatch)
                , new SubItem($" {eEQPSATUS.Stopping}", MaterialDesignThemes.Wpf.PackIconKind.StopCircle)
                , new SubItem($" {eEQPSATUS.Idle}", MaterialDesignThemes.Wpf.PackIconKind.Peace)
                , new SubItem($" {eEQPSATUS.Run}", MaterialDesignThemes.Wpf.PackIconKind.RunFast)
                , new SubItem($" {eEQPSATUS.Error}", MaterialDesignThemes.Wpf.PackIconKind.Error)
                , new SubItem($" {eEQPSATUS.EMG}", MaterialDesignThemes.Wpf.PackIconKind.CarEmergencyBrake)
            };                            
        }

        private void On_RdoChanged(object obj)
        {
            var uid = (eUID4VM)(Convert.ToInt32(obj));
            _ctrl.DoingDataExchage(eVIWER.TowerLamp, eDATAEXCHANGE.View2Model, uid, (eEQPSATUS)twrLmp_SelectedItem);
        }

        private void On_DataExchange(object sender, (eEQPSATUS state, LAMPINFO data) e)
        {            
            var lmp = e.data.lst.SingleOrDefault(l => l.status == (eEQPSATUS)twrLmp_SelectedItem);
            if ( null != lmp )
            {
                switch (lmp.Green)
                {
                    case TWRLAMP.OFF: b_Green_Off = true; break;
                    case TWRLAMP.ON: b_Green_On = true; break;
                    case TWRLAMP.BLINK: b_Green_Blink = true; break;                    
                }

                switch (lmp.Yellow)
                {
                    case TWRLAMP.OFF: b_Yellow_Off = true; break;
                    case TWRLAMP.ON: b_Yellow_On = true; break;
                    case TWRLAMP.BLINK: b_Yellow_Blink = true; break;
                }

                switch (lmp.Red)
                {
                    case TWRLAMP.OFF: b_Red_Off = true; break;
                    case TWRLAMP.ON: b_Red_On = true; break;
                    case TWRLAMP.BLINK: b_Red_Blink = true; break;
                }

                if ( lmp.Buzzer )
                {
                    b_Buzzer_On = true;
                }
                else
                {
                    b_Buzzer_Off = true;
                }

                b_BlinkTime = e.data.blinkTime;
            }
        }

        private bool CanExecuteMethod(object arg)
        {
            switch (_ctrl.CurrView)
            {
                case eVIWER.TowerLamp: return true;
                default: return false;
            }
        }

        private void ExecuteMethod(object obj)
        {
            if ((int)obj > -1)
            {
                twrLmp_SelectedItem = (int)obj;
                _ctrl.DoingDataExchage(eVIWER.TowerLamp, eDATAEXCHANGE.Model2View);
            }
        }

        SubItem[] subItems;
        public SubItem[] b_SubItems
        {
            get {
                return subItems;
            }
            set {
                subItems = value;
                OnPropertyChanged();
            }
        }

        bool green_Off = false;
        public bool b_Green_Off
        {
            get {
                return green_Off;
            }
            set {
                green_Off = value;
                OnPropertyChanged();
            }
        }

        bool green_On = false;
        public bool b_Green_On
        {
            get {
                return green_On;
            }
            set {
                green_On = value;
                OnPropertyChanged();
            }
        }

        bool green_Blink = false;
        public bool b_Green_Blink
        {
            get {
                return green_Blink;
            }
            set {
                green_Blink = value;
                OnPropertyChanged();
            }
        }

        bool yellow_Off = false;
        public bool b_Yellow_Off
        {
            get {
                return yellow_Off;
            }
            set {
                yellow_Off = value;
                OnPropertyChanged();
            }
        }

        bool yellow_On = false;
        public bool b_Yellow_On
        {
            get {
                return yellow_On;
            }
            set {
                yellow_On = value;
                OnPropertyChanged();
            }
        }

        bool yellow_Blink = false;
        public bool b_Yellow_Blink
        {
            get {
                return yellow_Blink;
            }
            set {
                yellow_Blink = value;
                OnPropertyChanged();
            }
        }

        bool red_Off = false;
        public bool b_Red_Off
        {
            get {
                return red_Off;
            }
            set {
                red_Off = value;
                OnPropertyChanged();
            }
        }

        bool red_On = false;
        public bool b_Red_On
        {
            get {
                return red_On;
            }
            set {
                red_On = value;
                OnPropertyChanged();
            }
        }

        bool red_Blink = false;
        public bool b_Red_Blink
        {
            get {
                return red_Blink;
            }
            set {
                red_Blink = value;
                OnPropertyChanged();
            }
        }

        bool buzzer_Off = false;
        public bool b_Buzzer_Off
        {
            get {
                return buzzer_Off;
            }
            set {
                buzzer_Off = value;
                OnPropertyChanged();
            }
        }

        bool buzzer_On = false;
        public bool b_Buzzer_On
        {
            get {
                return buzzer_On;
            }
            set {
                buzzer_On = value;
                OnPropertyChanged();
            }
        }
        
        public int b_BlinkTime
        {
            get {
                return _lmp.blinkTime;
            }
            set {
                _lmp.blinkTime = value;
                OnPropertyChanged();
            }
        }
    }
}
