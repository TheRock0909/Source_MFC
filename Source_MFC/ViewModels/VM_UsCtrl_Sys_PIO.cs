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
                        break;
                    }
                case eDATAEXCHANGE.View2Model:
                    {                        
                        var uid = (eUID4VM)Convert.ToInt32(sender);
                        var datatype = eDATATYPE.NONE;
                        var strCurr = string.Empty;
                        switch (uid)
                        {
                            case eUID4VM.PIO_0:  case eUID4VM.PIO_1:  case eUID4VM.PIO_2: 
                            case eUID4VM.PIO_3:  case eUID4VM.PIO_4:                                
                                datatype = eDATATYPE._int;
                                switch (uid)
                                {
                                    case eUID4VM.PIO_0: strCurr = $"{b_InterfaceTimeout}"; break;
                                    case eUID4VM.PIO_1: strCurr = $"{b_DockSenChkTime}"; break;
                                    case eUID4VM.PIO_2: strCurr = $"{b_FeedTimeOut_Start}"; break;
                                    case eUID4VM.PIO_3: strCurr = $"{b_FeedTimeOut_Work}"; break;
                                    case eUID4VM.PIO_4: strCurr = $"{b_FeedTimeOut_End}"; break;
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
                InterfaceTimeout = value;
                OnPropertyChanged("b_InterfaceTimeout");
            }
        }

        string DockSenChkTime;
        public string b_DockSenChkTime
        {
            get { return DockSenChkTime; }
            set {
                DockSenChkTime = value;
                OnPropertyChanged("b_DockSenChkTime");
            }
        }

        string FeedTimeOut_Start;
        public string b_FeedTimeOut_Start
        {
            get { return FeedTimeOut_Start; }
            set {
                FeedTimeOut_Start = value;
                OnPropertyChanged("b_FeedTimeOut_Start");
            }
        }

        string FeedTimeOut_Work;
        public string b_FeedTimeOut_Work
        {
            get { return FeedTimeOut_Work; }
            set {
                FeedTimeOut_Work = value;
                OnPropertyChanged("b_FeedTimeOut_Work");
            }
        }

        string FeedTimeOut_End;
        public string b_FeedTimeOut_End
        {
            get { return FeedTimeOut_End; }
            set {
                FeedTimeOut_End = value;
                OnPropertyChanged("b_FeedTimeOut_End");
            }
        }

    }
}
