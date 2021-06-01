using Source_MFC.Utils;
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
    /// UsCtrl_LogItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UsCtrl_LogItem : UserControl
    {
        public UsCtrl_LogItem(MainCtrl ctrl, CmdLogType logType)
        {
            InitializeComponent();
            var vm = new VM_UsrCtrl_LogItem(ctrl, logType);
            this.DataContext = vm;            
        }
    }
}
