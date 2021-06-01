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
    /// frm_InputBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class frm_InputBox : Window
    {
        public frm_InputBox()
        {
            InitializeComponent();
            VM_Frm_InputBox vm = new VM_Frm_InputBox();
            this.DataContext = vm;

            if (vm.CloseAction == null)
                vm.CloseAction = new Action(() => this.Close());
            if (vm.DragMoveAction == null)
                vm.DragMoveAction = new Action(() => DragMove());
        }
    }
}
