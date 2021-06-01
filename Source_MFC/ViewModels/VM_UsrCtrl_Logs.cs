using MaterialDesignThemes.Wpf;
using Source_MFC.Global;
using Source_MFC.Utils;
using Source_MFC.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Source_MFC.ViewModels
{
    public class VM_UsrCtrl_Logs : Notifier
    {
        MainCtrl _ctrl;
        public ICommand Evt_ListBoxSelChanged { get; set; }        
        public VM_UsrCtrl_Logs(MainCtrl ctrl)
        {
            _ctrl = ctrl;
            Evt_ListBoxSelChanged = new Command(On_LogItemSelChanged);            
            b_LogTypeItems = new[]
            {
                new SubItem("Production", CmdLogType.prdt, PackIconKind.Reproduction, new UsCtrl_LogItem(ctrl, CmdLogType.prdt)),
                new SubItem("Communication", CmdLogType.Comm, PackIconKind.NearFieldCommunication, new UsCtrl_LogItem(ctrl, CmdLogType.Comm)),
                new SubItem("SECSGEM", CmdLogType.Gem, PackIconKind.TableNetwork, new UsCtrl_LogItem(ctrl, CmdLogType.Gem)),
                new SubItem("ERROR", CmdLogType.Err, PackIconKind.Error, new UsCtrl_LogItem(ctrl, CmdLogType.Err)),
                new SubItem("Debug", CmdLogType.Debug, PackIconKind.DebugStepInto, new UsCtrl_LogItem(ctrl, CmdLogType.Debug))
            };            
            On_LogItemSelChanged((int)CmdLogType.prdt);
        }

        private void On_LogItemSelChanged(object obj)
        {
            if (obj != null && (int)obj >= 0)
            {
                if (b_usrCtrl_LogItem != b_LogTypeItems[(int)obj].Screen)
                {
                    b_usrCtrl_LogItem = b_LogTypeItems[(int)obj].Screen;
                }
            }
        }

        SubItem[] logTypeItems;
        public SubItem[] b_LogTypeItems
        {
            get {
                return logTypeItems;
            }
            set {
                logTypeItems = value;
                OnPropertyChanged();
            }
        }

        UserControl userControl;
        public UserControl b_usrCtrl_LogItem
        {
            get {
                return userControl;
            }
            set {
                userControl = value;
                OnPropertyChanged();
            }
        }
    }
}
