using KeyPad;
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
    class VM_UsCtrl_Sys_FAC : Notifier
    {
        MainCtrl _ctrl;
        FAC _fac;
        public IEnumerable<eEQPTYPE> eEqpType { get; set; }        
        public IEnumerable<eCUSTOMER> eCustomer { get; set; }
        public IEnumerable<eSCENARIOMODE> eSeqMode { get; set; }
        public IEnumerable<eLANGUAGE> eLanguage { get; set; }
        public ICommand Evt_DataExchange4FAC { get; set; }
        public VM_UsCtrl_Sys_FAC(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _fac = _Data.Inst.sys.cfg.fac;
            eEqpType = Enum.GetValues(typeof(eEQPTYPE)).Cast<eEQPTYPE>();
            eLanguage = Enum.GetValues(typeof(eLANGUAGE)).Cast<eLANGUAGE>();
            eCustomer = Enum.GetValues(typeof(eCUSTOMER)).Cast<eCUSTOMER>();
            eSeqMode = Enum.GetValues(typeof(eSCENARIOMODE)).Cast<eSCENARIOMODE>();
            _ctrl.Evt_Sys_FAC_Item_DataExchange += On_DataExchange;
            Evt_DataExchange4FAC = new Command(On_DataExchange4FAC);
        }

        private void On_DataExchange4FAC(object obj)
        {
            On_DataExchange(obj, (eDATAEXCHANGE.View2Model, null));
        }

        private void On_DataExchange(object sender, (eDATAEXCHANGE dir, FAC data) e)
        {
            switch (e.dir)
            {                
                case eDATAEXCHANGE.Model2View:
                    b_EQPType = e.data.eqpType;
                    b_EQPName = e.data.eqpName;
                    b_SeqMode = e.data.seqMode;
                    b_Customer = e.data.customer;
                    b_MpIP = e.data.mplusIP;
                    b_MpPort = $"{e.data.mplusPort}";
                    b_VecIP = e.data.VecIP;
                    break;
                case eDATAEXCHANGE.View2Model:
                    {
                        var uid = (eUID4VM)Convert.ToInt32(sender);
                        var datatype = eDATATYPE.NONE;
                        var strCurr = string.Empty;
                        switch (uid)
                        {
                            case eUID4VM.FAC_EQPType:   case eUID4VM.FAC_EQPName:   case eUID4VM.FAC_Customer:
                            case eUID4VM.FAC_SeqMode:   case eUID4VM.FAC_Language:  case eUID4VM.FAC_MPlusIP:
                            case eUID4VM.FAC_MPlusPort: case eUID4VM.FAC_VehicleIP:
                                {                                    
                                    switch (uid)
                                    {
                                        case eUID4VM.FAC_EQPType: case eUID4VM.FAC_SeqMode: case eUID4VM.FAC_Language: case eUID4VM.FAC_Customer: break;
                                        case eUID4VM.FAC_EQPName: case eUID4VM.FAC_MPlusIP: case eUID4VM.FAC_VehicleIP: datatype = eDATATYPE._str; break;
                                        case eUID4VM.FAC_MPlusPort: datatype = eDATATYPE._int; break;                                        
                                        default: break;
                                    }

                                    switch (datatype)
                                    {
                                        case eDATATYPE._str:
                                            {
                                                VirtualKeyboard keyboardWindow = new VirtualKeyboard(strCurr);
                                                if (keyboardWindow.ShowDialog() == true)
                                                {
                                                    var chk = _ctrl.DoingDataExchage(eVIWER.FAC, eDATAEXCHANGE.View2Model, uid, keyboardWindow.Result);                                                   
                                                    if (true == chk)
                                                    {
                                                        On_DataExchange(null, (eDATAEXCHANGE.Model2View, _fac));
                                                    }
                                                }
                                                break;
                                            }
                                        case eDATATYPE._bool:
                                            {

                                                break;
                                            }
                                        case eDATATYPE._int:
                                        case eDATATYPE._float:
                                        case eDATATYPE._double:
                                            {
                                                Keypad keypadWindow = new Keypad(strCurr);
                                                if (keypadWindow.ShowDialog() == true)
                                                {
                                                    var chk = _ctrl.DoingDataExchage(eVIWER.FAC, eDATAEXCHANGE.View2Model, uid, keypadWindow.Result);
                                                    if (true == chk)
                                                    {
                                                        On_DataExchange(null, (eDATAEXCHANGE.Model2View, _fac));
                                                    }
                                                }
                                                break;
                                            }
                                        default: break;
                                    }
                                    break;
                                }
                            default: break;
                        }
                        break;
                    }
                default:
                    break;
            }
            OnPropertyChanged();
        }

        public eEQPTYPE b_EQPType
        {
            get { return _fac.eqpType; }
            set { _fac.eqpType = value; OnPropertyChanged("b_EQPType"); }
        }

        public string b_EQPName
        {
            get { return _fac.eqpName; }
            set { _fac.eqpName = value; OnPropertyChanged("b_EQPName"); }
        }

        public eCUSTOMER b_Customer
        {
            get { return _fac.customer; }
            set { _fac.customer = value; OnPropertyChanged("b_Customer"); }
        }

        public eSCENARIOMODE b_SeqMode
        {
            get { return _fac.seqMode; }
            set { _fac.seqMode = value; OnPropertyChanged("b_SeqMode"); }
        }

        public eLANGUAGE b_eLanguage
        {
            get { return _fac.language; }
            set { _fac.language = value; OnPropertyChanged("b_eLanguage"); }
        }

        public string b_MpIP
        {
            get { return _fac.mplusIP; }
            set { _fac.mplusIP = value; OnPropertyChanged("b_MpIP"); }
        }

        string mpPort = string.Empty;
        public string b_MpPort
        {
            get { return mpPort; }
            set { mpPort = value; OnPropertyChanged("b_MpPort"); }
        }

        public string b_VecIP
        {
            get { return _fac.VecIP; }
            set { _fac.VecIP = value; OnPropertyChanged("b_VecIP"); }
        }
    }
}
