using KeyPad;
using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Source_MFC.ViewModels
{
    public class VM_UsCtrl_Sys_PIO : Notifier
    {
        MainCtrl _ctrl;
        PIO _PIO;
        public ICommand Evt_MouseDown4PIO { get; set; }        
        public VM_UsCtrl_Sys_PIO(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _PIO = _Data.Inst.sys.cfg.pio;
            _ctrl.Evt_Sys_PIO_Item_DataExchange += On_DataExchange;
            Evt_MouseDown4PIO = new Command(On_MouseDown4PIO);
        }

        ~VM_UsCtrl_Sys_PIO()
        {
            _ctrl.Evt_Sys_PIO_Item_DataExchange -= On_DataExchange;
        }

        private void On_MouseDown4PIO(object obj)
        {
            On_DataExchange(obj, (eDATAEXCHANGE.View2Model, null));
        }

        public void On_DataExchange(object sender, (eDATAEXCHANGE dir, PIO data)e)
        {
            switch (e.dir)
            {                
                case eDATAEXCHANGE.Model2View:
                    {
                        b_InterfaceTimeout = $"{e.data.nInterfaceTimeout}";
                        b_DockSenChkTime = $"{e.data.nDockSenChkTime}";
                        b_FeedTimeOut_Start = $"{e.data.nFeedTimeOut_Start}";
                        b_FeedTimeOut_Work = $"{e.data.nFeedTimeOut_Work}";
                        b_FeedTimeOut_End = $"{e.data.nFeedTimeOut_End}";
                        b_SensorDelay = $"{e.data.nSenDelay}";
                        b_CommTimeout = $"{e.data.nCommTimeout}";
                        b_ConvSpd_Normal = $"{e.data.nConvSpd_Normal}";
                        b_ConvSpd_Slow = $"{e.data.nConvSpd_Slow}";
                        break;
                    }
                case eDATAEXCHANGE.View2Model:
                    {                        
                        var uid = (eUID4VM)Convert.ToInt32(sender);
                        var datatype = eDATATYPE.NONE;
                        var strCurr = string.Empty;
                        switch (uid)
                        {
                            case eUID4VM.PIO_0:     case eUID4VM.PIO_1:  case eUID4VM.PIO_2: 
                            case eUID4VM.PIO_3:     case eUID4VM.PIO_4:
                            case eUID4VM.SENDELAY:  case eUID4VM.COMM_TIMEOUT:
                                datatype = eDATATYPE._int;
                                switch (uid)
                                {
                                    case eUID4VM.PIO_0: strCurr = $"{b_InterfaceTimeout}"; break;
                                    case eUID4VM.PIO_1: strCurr = $"{b_DockSenChkTime}"; break;
                                    case eUID4VM.PIO_2: strCurr = $"{b_FeedTimeOut_Start}"; break;
                                    case eUID4VM.PIO_3: strCurr = $"{b_FeedTimeOut_Work}"; break;
                                    case eUID4VM.PIO_4: strCurr = $"{b_FeedTimeOut_End}"; break;
                                    case eUID4VM.SENDELAY: strCurr = $"{b_SensorDelay}"; break;
                                    case eUID4VM.COMM_TIMEOUT: strCurr = $"{b_CommTimeout}"; break;
                                    case eUID4VM.CONVSPD_NORMAL: strCurr = $"{b_ConvSpd_Normal}"; break;
                                    case eUID4VM.CONVSPD_SLOW: strCurr = $"{b_ConvSpd_Slow}"; break;
                                    default: break;
                                }
                                break;
                            default: break;
                        }

                        switch (datatype)
                        {
                            case eDATATYPE._str:
                                {
                                    VirtualKeyboard keyboardWindow = new VirtualKeyboard(strCurr);
                                    if (keyboardWindow.ShowDialog() == true)
                                    {
                                        
                                    }
                                    break;
                                }
                            case eDATATYPE._bool:
                                {
                                    
                                    break;
                                }
                            case eDATATYPE._int: case eDATATYPE._float: case eDATATYPE._double:
                                {
                                    Keypad keypadWindow = new Keypad(strCurr);
                                    if (keypadWindow.ShowDialog() == true)
                                    {
                                        var chk = _ctrl.DoingDataExchage(eVIWER.PIO, eDATAEXCHANGE.View2Model, uid, keypadWindow.Result);
                                        if ( true == chk )
                                        {
                                            On_DataExchange(null, (eDATAEXCHANGE.Model2View, _PIO));
                                        }                                        
                                    }
                                    break;
                                }
                            default: break;
                        }
                        break;
                    }
                case eDATAEXCHANGE.StatusUpdate:
                    break;
            }            
            OnPropertyChanged();
        }

        string InterfaceTimeout;
        public string b_InterfaceTimeout
        {
            get { return InterfaceTimeout; }
            set {                
                this.MutateVerbose(ref InterfaceTimeout, value, RaisePropertyChanged());
            }
        }

        string DockSenChkTime;
        public string b_DockSenChkTime
        {
            get { return DockSenChkTime; }
            set {
                this.MutateVerbose(ref DockSenChkTime, value, RaisePropertyChanged());
            }
        }

        string FeedTimeOut_Start;
        public string b_FeedTimeOut_Start
        {
            get { return FeedTimeOut_Start; }
            set {
                this.MutateVerbose(ref FeedTimeOut_Start, value, RaisePropertyChanged());
            }
        }

        string FeedTimeOut_Work;
        public string b_FeedTimeOut_Work
        {
            get { return FeedTimeOut_Work; }
            set {
                this.MutateVerbose(ref FeedTimeOut_Work, value, RaisePropertyChanged());
            }
        }

        string FeedTimeOut_End;
        public string b_FeedTimeOut_End
        {
            get { return FeedTimeOut_End; }
            set {
                this.MutateVerbose(ref FeedTimeOut_End, value, RaisePropertyChanged());
            }
        }

        string SensorDelay;
        public string b_SensorDelay
        {
            get { return SensorDelay; }
            set {
                this.MutateVerbose(ref SensorDelay, value, RaisePropertyChanged());
            }
        }

        string commTimeout;
        public string b_CommTimeout
        {
            get { return commTimeout; }
            set {
                this.MutateVerbose(ref commTimeout, value, RaisePropertyChanged());
            }
        }


        string convSpd_Normal;
        public string b_ConvSpd_Normal
        {
            get { return convSpd_Normal; }
            set {
                this.MutateVerbose(ref convSpd_Normal, value, RaisePropertyChanged());
            }
        }

        string convSpd_Slow;
        public string b_ConvSpd_Slow
        {
            get { return convSpd_Slow; }
            set {
                this.MutateVerbose(ref convSpd_Slow, value, RaisePropertyChanged());
            }
        }

    }
}
