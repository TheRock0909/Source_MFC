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
using System.Windows.Shapes;

namespace Source_MFC.Views
{
    /// <summary>
    /// frm_Noti.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class frm_Noti : Window
    { 
        public frm_Noti(string msg)
        {
            InitializeComponent();
            VM_Frm_Noti vm = new VM_Frm_Noti(msg);
            this.DataContext = vm;

            if (vm.CloseAction == null)
                vm.CloseAction = new Action(() => this.Close());
        }
    }
}
