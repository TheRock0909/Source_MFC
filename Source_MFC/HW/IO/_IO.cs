using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Source_MFC.HW
{    
    public enum eBOARD
    {
           None
         , Commi
         , EZi
    }
    public class _IO
    {
        public System.Timers.Timer _IOChk = new System.Timers.Timer();
        public eBOARD boardType;
        public Any64 _In;
        public Any64 _Out;
        public Any64 _GetOut;
        private bool connected = false;

        public bool _Connected { set { connected = value; } get { return connected; } }       
        public _IO()
        {

        }

        virtual public bool Open()
        {
            return true;
        }

        virtual public void Close()
        {

        }

       
        virtual public bool GetInput(eINPUT nCh)
        {
            return _In[(int)nCh];
        }

        virtual public bool GetOutput(eOUTPUT nCh)
        {
            return _GetOut[(int)nCh];
        }

        virtual public void SetOutput(eOUTPUT nCh, bool bTrg)
        {

        }
    }
}