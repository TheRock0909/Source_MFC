using Source_MFC.Global;
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
                if (true == arg.bStop) return;

                var job = _ctrl._status.Order;
                var seqMode = _Data.Inst.sys.cfg.fac.seqMode;
                var escapeEQP = Get(eSEQLIST.EscapeEQP);
                var move2Dst = Get(eSEQLIST.Move2Dst);
                var pio = Get(eSEQLIST.PIO);      
                
                if ( false == arg.bStop )
                {
                    switch (_ctrl._EQPStatus)
                    {                        
                        case eEQPSATUS.Stopping: arg.bStop = true; break;                        
                        default: break;
                    }
                }

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
                        switch (seqMode)
                        {
                            case eSCENARIOMODE.PC:
                                arg.nStep = 105;
                                escapeEQP.WorkTrg();
                                break;
                            case eSCENARIOMODE.PLC:
                                arg.nStep = 200;
                                break;
                            default: break;
                        }
                        break;
                    case 105:
                        {
                            var chk = escapeEQP.IsDone();
                            if (false == chk.rtn) break;
                            switch (chk.err)
                            {
                                case eERROR.None: arg.nStep = 200; break;
                                default: arg.nStep = 100; arg.StopTrg(); break;
                            }
                            break;
                        }
                    // 목적지까지 이동한다. (PLC모드일 경우 Sequance내에서 LD이동 Skip 후 PuaseCancel을 대기한다.)
                    case 200:
                        if ( false == job.opt.bSkipGo2Dest )
                        {
                            arg.nStep = 205;
                            move2Dst.WorkTrg();
                        }
                        else
                        {
                            arg.nStep = 300;
                        }
                        break;
                    case 205:
                        {
                            var chk = move2Dst.IsDone();
                            if (false == chk.rtn) break;
                            switch (chk.err)
                            {
                                case eERROR.None: arg.nStep = 300; break;
                                default: arg.nStep = 100; arg.StopTrg(); break;
                            }
                            break;
                        }
                    // PIO 확인 후 트래이를 로딩/언로딩한다. (PIO 시퀀스 내부에서 로딩/언로딩 시퀀스를 구동하게됨)
                    case 300:
                        switch (job.type)
                        {                            
                            case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                                if (false == job.opt.bSkipPIO)
                                {
                                    arg.nStep = 305;
                                    pio.WorkTrg();
                                }
                                else
                                {
                                    arg.nStep = 400;
                                }
                                break;
                            default: arg.nStep = 500; break;
                        }                        
                        break;
                    case 305:
                        {
                            var chk = pio.IsDone();
                            if (false == chk.rtn) break;
                            switch (chk.err)
                            {
                                case eERROR.None: arg.nStep = 500; break;
                                default: arg.StopTrg(); break;
                            }
                            break;
                        }

                    // PIO Skip Tansfering.
                    case 400:
                        {
                            if ( false == job.opt.bSkipTransfer)
                            {
                                var transfering = (eJOBTYPE.LOADING == job.type) ? Get(eSEQLIST.Drop) : Get(eSEQLIST.Pick);
                                arg.nStep = 405;
                                transfering.WorkTrg();
                            }
                            else
                            {
                                arg.nStep = 410;
                            }
                            break;
                        }
                    case 405:
                        {
                            var transfering = (eJOBTYPE.LOADING == job.type) ? Get(eSEQLIST.Drop) : Get(eSEQLIST.Pick);
                            var chk = transfering.IsDone();
                            if (false == chk.rtn) break;
                            switch (chk.err)
                            {
                                case eERROR.None: arg.nStep = 410; break;
                                default: arg.StopTrg(); break;
                            }
                            break;
                        }                        
                    case 410: arg.nStep = 500; break;

                    case 500:
                        arg.nStatus = eSTATE.Done;
                        arg.nStep = DEF_CONST.SEQ_MAIN_FINISH;
                        break;
                    case DEF_CONST.SEQ_MAIN_FINISH:
                        arg.nStatus = eSTATE.None;
                        arg.nStep = DEF_CONST.SEQ_INIT; ;
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
