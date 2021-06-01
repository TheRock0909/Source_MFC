using Source_MFC.Global;
using Source_MFC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Source_MFC.Views
{
    /// <summary>
    /// UsCtrl_DevCont.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UsCtrl_DevCont : UserControl
    {
        public VM_UsrCtrl_DevCont _VM { get; set; }
        public UsCtrl_DevCont(eDEV dev)
        {
            InitializeComponent();
            _VM = new VM_UsrCtrl_DevCont(dev);
            DataContext = _VM;
        }
    }
}
