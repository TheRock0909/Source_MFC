using Source_MFC.Global;
using Source_MFC.Utils;
using Source_MFC.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Source_MFC.ViewModels
{
    public class VM_UsrCtrl_LogItem 
    {        
        MainCtrl _ctrl;
        private CmdLogType logItemType;                
        public VM_UsrCtrl_LogItem(MainCtrl ctrl, CmdLogType logType)
        {            
            _ctrl = ctrl;
            logItemType = logType;
            b_ReceData = new IClassTextBoxAppend();
            _ctrl.Evt_WriteLog += On_WriteLog;            
        }

        private void On_WriteLog(object sender, WriteLogArgs e)
        {
            if ( logItemType == e.type )
            {
                b_ReceData.Append($"{e.time.ToString("HH:mm:ss.fff")} : {e.msg}");                
            }
        }

        public ITextBoxAppend b_ReceData { get; set; }
    }
}
