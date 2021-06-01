using MaterialDesignThemes.Wpf;
using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.ViewModels
{
    class VM_UsrCtrl_MenuItem : Notifier
    {
        ItemMenu _itemMenu;
        public VM_UsrCtrl_MenuItem(ItemMenu itemMenu)
        {
            _Initialize(itemMenu);
        }

        private void _Initialize(ItemMenu itemMenu)
        {
            _itemMenu = itemMenu;
            b_Header = itemMenu.Header;
            b_SubItems = itemMenu.SubItems;
            b_IconItem = itemMenu.Icon;
        }

        ePAGE header = ePAGE.DashBoard;
        public ePAGE b_Header
        {
            get {
                return header;
            }
            set {
                header = value;
                OnPropertyChanged();
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

        PackIconKind icon = PackIconKind.Monitor;
        public PackIconKind b_IconItem
        {
            get {
                return icon;
            }
            set {
                icon = value;
                OnPropertyChanged();
            }
        }
    }
}
