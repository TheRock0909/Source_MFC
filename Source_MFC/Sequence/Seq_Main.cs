﻿using Source_MFC.Global;
using Source_MFC.Sequence.MainTasks;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Sequence
{
    public class Seq_Main : _SEQBASE
    {
        public Seq_Main(MainCtrl main)
        {
            _ctrl = main;
            arg = new TASKARG(eSEQLIST.Main);
        }

        override public void Run()
        {
            try
            {
                var job = _ctrl._status.Order;
                switch (arg.nStep)
                {
                    case DEF_CONST.SEQ_INIT:
                        arg.nStep = 10;
                        arg.nStatus = eSTATE.Working;
                        break;
                    case 10:
                        switch (job.state)
                        {
                            case eJOBST.None: break;
                            case eJOBST.Assign:
                                arg.nStep = 100;
                                break;
                            case eJOBST.Enroute:
                                arg.nStep = 200;
                                break;
                            case eJOBST.Arrived:
                            case eJOBST.Transferring:
                            case eJOBST.TransStart:
                            case eJOBST.CarrierChanged:
                                arg.nStep = 300;
                                break;
                            case eJOBST.TransComplete:
                            case eJOBST.UserStopped:
                            default: break;
                        }
                        break;
                    // 처음 Job을 받았을 경우 현재 위치를 확인해서 회피구동을 해야하는지 확인한다.
                    case 100:                        

                        break;
                    case 500:
                        arg.nStatus = eSTATE.Done;
                        arg.nStep = DEF_CONST.SEQ_MAIN_FINISH;
                        break;
                    case DEF_CONST.SEQ_MAIN_FINISH:
                        arg.nStep = DEF_CONST.SEQ_FINISH;
                        break;
                    case DEF_CONST.SEQ_FINISH: break;
                }
            }
            catch (Exception e)
            {
                Logger.Inst.Write(CmdLogType.Debug, $"Exception : {arg.GetID().ToString()}, {arg.nStep}\r\n{e.ToString()}\r\n");
            }
        }
    }
}
