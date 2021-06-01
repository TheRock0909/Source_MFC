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
    /// UsCtrl_Dash_Moni.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UsCtrl_Dash_Moni : UserControl
    {
        public UsCtrl_Dash_Moni(MainCtrl ctrl)
        {
            InitializeComponent();
            this.Uid = eVIWER.Monitor.ToString();
            VM_UsCtrl_Dash_Moni vm = new VM_UsCtrl_Dash_Moni(ctrl);
            this.DataContext = vm;
        }
    }
}
