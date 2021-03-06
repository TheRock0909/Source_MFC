using CrevisFnIoLib;
using Source_MFC;
using Source_MFC.Global;
using Source_MFC.HW;
using Source_MFC.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Source_MFC.HW.IO
{

    class Crevis : _IO
    {
        static public CrevisFnIO _FnIo = new CrevisFnIO();
        static public Int32 _Err = 0;
        static public IntPtr _hSys = new IntPtr();
        static public IntPtr _hDev = new IntPtr();
        MainCtrl _ctrl;
        public Crevis (MainCtrl ctrl)
        {
            _ctrl = ctrl;
            _IOChk.Interval = 5;
            _IOChk.Elapsed += _IOChk_Elapsed;
        }

        private byte[] pInputImage = null;
        private byte[] pOutputImage = null;
        private byte[] pGetOutputImg = null;
        private int val = 0;
        private int InputImageSize = 0;
        private int OutputImageSize = 0;
        private SUBTSKARG _TmrArg = new SUBTSKARG(eTASKLIST.MAX_SUB_SEQ);        
        public event EventHandler<(eDEV dev, bool connection)> Evt_Connected;
        public event EventHandler<(long inputs, long getouts )> Evt_Status;
        override public bool Open()
        {
            _Connected = false;
            try
            {
                _Err = _FnIo.FNIO_LibInitSystem(ref _hSys);
                switch (_Err)
                {
                    case CrevisFnIO.FNIO_ERROR_SUCCESS:
                        if (_IOChk.Enabled == false)
                            _IOChk.Start();
                        break;
                    case CrevisFnIO.FNIO_ERROR_SYSTEM_ALREADY_INIT: return false;
                    case CrevisFnIO.FNIO_ERROR_DEVICE_CONNECT_FAIL:
                    default: goto LBL_ERR;
                }                

                CrevisFnIO.DEVICEINFOMODBUSTCP2 DeviceInfo = new CrevisFnIO.DEVICEINFOMODBUSTCP2();
                DeviceInfo.IpAddress = new byte[4];
                string ipAddress = "192.168.0.22";
                int i = 0;
                string[] words = ipAddress.Split('.');
                foreach (string word in words)
                {
                    DeviceInfo.IpAddress[i] = (byte)(Int32.Parse(word));
                    i++;
                }

                _Err = _FnIo.FNIO_DevOpenDevice(_hSys, ref DeviceInfo, CrevisFnIO.MODBUS_TCP, ref _hDev);
                if (_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                {
                    Console.Write("Failed to open the device.\n ");
                    _FnIo.FNIO_LibFreeSystem(_hSys);
                    goto LBL_ERR;
                }

                // Get Input Image Size & Allocate Memory                
                _Err = _FnIo.FNIO_DevGetParam(_hDev, CrevisFnIO.DEV_INPUT_IMAGE_SIZE, ref val);
                if (_Err != CrevisFnIO.FNIO_ERROR_SUCCESS) goto LBL_ERR;
                InputImageSize = val;
                pInputImage = new byte[InputImageSize];
                // Get Output Image Size & Allocate Memory
                _Err = _FnIo.FNIO_DevGetParam(_hDev, CrevisFnIO.DEV_OUTPUT_IMAGE_SIZE, ref val);
                if (_Err != CrevisFnIO.FNIO_ERROR_SUCCESS) goto LBL_ERR;
                OutputImageSize = val;
                pOutputImage = new byte[OutputImageSize];
                pGetOutputImg = new byte[OutputImageSize];

                // ******************************************************
                // Set the environment variable IO data controls

                // Set the IO data update frequency to maximum speed.                
                val = 0;    //0 ms 
                _Err = _FnIo.FNIO_DevSetParam(_hDev, CrevisFnIO.DEV_UPDATE_FREQUENCY, val);
                if (_Err != CrevisFnIO.FNIO_ERROR_SUCCESS) goto LBL_ERR;

                // Set the response timeout to 1 second. 
                val = 1000; //1 s 
                _Err = _FnIo.FNIO_DevSetParam(_hDev, CrevisFnIO.DEV_RESPONSE_TIMEOUT, val);
                if (_Err != CrevisFnIO.FNIO_ERROR_SUCCESS) goto LBL_ERR;
                // *****************************************************

                // IO data update start                
                _Err = _FnIo.FNIO_DevIoUpdateStart(_hDev, CrevisFnIO.IO_UPDATE_PERIODIC);
                if (_Err != CrevisFnIO.FNIO_ERROR_SUCCESS) goto LBL_ERR;


                _TmrArg.nStep = DEF_CONST.SEQ_INIT;
                _Connected = true;
                Evt_Connected?.Invoke(this, (eDEV.IO, true));
                return true;
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
            }
LBL_ERR:
            _Connected = false;
            Evt_Connected?.Invoke(this, (eDEV.IO, false));
            return false;
        }

        override public void Close()
        {
            _IOChk.Elapsed -= _IOChk_Elapsed;
            _IOChk.Stop();
            _IOChk.Dispose();
            if (true == _Connected)
            {                
                _Err = _FnIo.FNIO_DevCloseDevice(_hDev); //Close Device            
                if (_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                {
                    Debug.Write(false, $"Failed to close the device.");
                    _FnIo.FNIO_LibFreeSystem(_hSys);
                    return;
                }                       
                _Err = _FnIo.FNIO_LibFreeSystem(_hSys); //Free System
            }
        }

        
        private void _IOChk_Elapsed(object sender, ElapsedEventArgs e)
        {
            var arg = _TmrArg;
            if (false == _Connected)
            {                
                _IOChk.Stop(); //2021.05.14 IO 재연결
                Open();
                return;
            }
            else
            {
                if (arg.tSen.IsOver(50))
                {
                    Evt_Status?.Invoke(this, (_In.INT64, _GetOut.INT64));
                }
            }    
            
            switch (arg.nStep)
            {
                case DEF_CONST.SEQ_INIT:
                    arg.nTrg = 0; arg.tDly.nStart = 0; arg.tDly.nDelay = 100;
                    arg.nStep = 5;
                    break;
                case 5:
                    if (false == arg.tDly.IsOver()) break;
                    switch (arg.nStuff)
                    {
                        case 0:                            
                            arg.nStuff++;
                            _Err = _FnIo.FNIO_DevReadOutputImage(_hDev, 0, ref pOutputImage[0], OutputImageSize);
                            SetByteAll(eELEMENT_TYPE.Output, pOutputImage);                            
                            break;                            
                        default: arg.nTrg = 0; arg.nStep = 10; break;
                    }                    
                    break;
                case 10:
                    switch (arg.nTrg)
                    {
                        case 0:
                            arg.nTrg++;
                            if (OutputImageSize > 0)
                            {
                                pOutputImage[0] = _Out.UINT8_0;
                                pOutputImage[1] = _Out.UINT8_1;
                                pOutputImage[2] = _Out.UINT8_2;
                                _Err = _FnIo.FNIO_DevWriteOutputImage(_hDev, 0, ref pOutputImage[0], OutputImageSize);
                                switch (_Err)
                                {
                                    case CrevisFnIO.FNIO_ERROR_SUCCESS: arg.nStep = 15; arg.tDly.Reset(); break;
                                    default: SetDevDispose(); break;
                                }                                
                            }                            
                            break;
                        default: break;
                    }
                    break;
                case 15:
                    if (false == arg.tDly.IsOver()) break;
                    arg.nTrg = 0; arg.nStep = 20;
                    break;
                case 20:
                    switch (arg.nTrg)
                    {
                        case 0:
                            arg.nTrg++;
                            if (OutputImageSize > 0)
                            {
                                _Err = _FnIo.FNIO_DevReadOutputImage(_hDev, 0, ref pGetOutputImg[0], OutputImageSize);
                                switch (_Err)
                                {
                                    case CrevisFnIO.FNIO_ERROR_SUCCESS:
                                        SetByteAll(eELEMENT_TYPE.GetOutput, pGetOutputImg);                                        
                                        arg.nStep = 25; arg.tDly.Reset();
                                        break;
                                    default: SetDevDispose(); break;
                                }
                            }                            
                            break;
                        default: break;
                    }
                    break;
                case 25:
                    if (false == arg.tDly.IsOver()) break;
                    arg.nTrg = 0; arg.nStep = 30;
                    break;
                case 30:
                    switch (arg.nTrg)
                    {
                        case 0:
                            arg.nTrg++;
                            if (InputImageSize > 0)
                            {
                                _Err = _FnIo.FNIO_DevReadInputImage(_hDev, 0, ref pInputImage[0], InputImageSize);
                                switch (_Err)
                                {
                                    case CrevisFnIO.FNIO_ERROR_SUCCESS:
                                        SetByteAll(eELEMENT_TYPE.Input, pInputImage);
                                        arg.nStep = 40; arg.tDly.Reset(); 
                                        break;
                                    default: SetDevDispose(); break;
                                }
                            }                           
                            break;
                        default: break;
                    }
                    break;
                case 40: _TmrArg.nStep = 500; break;
                case 500: _TmrArg.nStep = DEF_CONST.SEQ_INIT; break;
                default: break;
            }
        }

        enum eELEMENT_TYPE { Input, Output, GetOutput }
        private void SetByteAll(eELEMENT_TYPE nType, byte[] arry )
        {
            int idx = 0;
            switch (nType)
            {
                case eELEMENT_TYPE.Input:
                    foreach (byte item in arry)
                    {
                        switch (idx)
                        {
                            case 0: _In.UINT8_0 = item; break;
                            case 1: _In.UINT8_1 = item; break;
                            case 2: _In.UINT8_2 = item; break;
                            case 3: _In.UINT8_3 = item; break;
                            case 4: _In.UINT8_4 = item; break;
                            case 5: _In.UINT8_5 = item; break;
                            case 6: _In.UINT8_6 = item; break;
                            case 7: _In.UINT8_7 = item; break;
                            default: break;
                        }
                        idx++;
                    }
                    break;
                case eELEMENT_TYPE.Output:
                    foreach (byte item in arry)
                    {
                        switch (idx)
                        {
                            case 0: _Out.UINT8_0 = item; break; // 3
                            case 1: _Out.UINT8_1 = item; break;
                            case 2: _Out.UINT8_2 = item; break;
                            case 3: _Out.UINT8_3 = item; break;
                            case 4: _Out.UINT8_4 = item; break;
                            case 5: _Out.UINT8_5 = item; break;
                            case 6: _Out.UINT8_6 = item; break;
                            case 7: _Out.UINT8_7 = item; break;
                            default: break;
                        }                        
                        idx++;
                    }
                    break;
                case eELEMENT_TYPE.GetOutput:
                    foreach (byte item in arry)
                    {
                        switch (idx)
                        {
                            case 0: _GetOut.UINT8_0 = item; break;
                            case 1: _GetOut.UINT8_1 = item; break;
                            case 2: _GetOut.UINT8_2 = item; break;
                            case 3: _GetOut.UINT8_3 = item; break;
                            case 4: _GetOut.UINT8_4 = item; break;
                            case 5: _GetOut.UINT8_5 = item; break;
                            case 6: _GetOut.UINT8_6 = item; break;
                            case 7: _GetOut.UINT8_7 = item; break;
                            default: break;
                        }
                        idx++;
                    }
                    break;
                default: break;
            }
        }
        
        private void SetDevDispose() //2021.05.14 IO 재연결을 위한 Device close
        {
            _Connected = false;
            _Err = _FnIo.FNIO_DevCloseDevice(_hDev);
            _FnIo.FNIO_LibFreeSystem(_hSys);
            _TmrArg.nStep = DEF_CONST.SEQ_INIT;            
            Evt_Connected?.Invoke(this, (eDEV.IO, false));            
        }

        override public void SetOutput(eOUTPUT nCh, bool bTrg)
        {
            if (true == _Connected)
            {
                var src = _ctrl.IO_SrcGet_OUT(nCh);
                src.state = bTrg;
                switch (src.RealID)
                {
                    case -1: break;
                    default:
                        switch (src.SenType)
                        {
                            case eSENTYPE.A: _Out[src.RealID] = bTrg; break;
                            case eSENTYPE.B: _Out[src.RealID] = !bTrg; break;
                        }                        
                        break;
                }                
            }
        }

        public override bool GetOutput(eOUTPUT nCh)
        {
            var src = _ctrl.IO_SrcGet_OUT(nCh);
            switch (src.RealID)
            {
                case -1: return false;
                default: return _GetOut[src.RealID];
            }
        }

        public override bool GetInput(eINPUT nCh)
        {
            var src = _ctrl.IO_SrcGet_IN(nCh);
            switch (src.RealID)
            {
                case -1: break;
                default:
                    var input = _In[src.RealID];
                    switch (src.SenType)
                    {
                        case eSENTYPE.A: return _In[src.RealID];
                        case eSENTYPE.B: return !_In[src.RealID];
                        default: return false;
                    }
            }
            return false;
        }
    }
}