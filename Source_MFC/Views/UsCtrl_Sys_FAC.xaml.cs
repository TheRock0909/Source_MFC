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
    /// UsCtrl_Sys_FAC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UsCtrl_Sys_FAC : UserControl
    {
        public UsCtrl_Sys_FAC(MainCtrl ctrl)
        {
            InitializeComponent();
            this.Uid = eVIWER.FAC.ToString();
            VM_UsCtrl_Sys_FAC vm = new VM_UsCtrl_Sys_FAC(ctrl);
            this.DataContext = vm;
        }
    }
}
