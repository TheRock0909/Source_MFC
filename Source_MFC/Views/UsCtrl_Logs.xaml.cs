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
    /// UsCtrl_Logs.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UsCtrl_Logs : UserControl
    {
        MainCtrl _ctrl;                
        public UsCtrl_Logs(MainCtrl ctrl)
        {
            InitializeComponent();
            _ctrl = ctrl;
            var vm = new VM_UsrCtrl_Logs(_ctrl);
            this.DataContext = vm;
        }
    }
}
