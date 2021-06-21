using Source_MFC.Global;
using Source_MFC.Sequence.SubTasks;
using Source_MFC.Utils;
using Source_MFC.HW.MobileRobot.LD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Tasks
{
    public class Tsk_VecCmd : _TSKBASE
    {
        public eVEC_CMD currCmd { get; set; } = eVEC_CMD.None;
        public SENDARG _sendArg = new SENDARG();
        private VEHICLESTATE vecState;
        public Tsk_VecCmd(MainCtrl main)
        {
            _ctrl = main;
            vecState = _ctrl._status.vecState;
            arg = new SUBTSKARG(eTASKLIST.MPLUSCOMM);
        }
        
        override public void Run()
        {
            try
            {
                int nDelay = _Data.Inst.sys.cfg.pio.nSenDelay;
                int nTimeout = _Data.Inst.sys.cfg.pio.nInterfaceTimeout;
                switch (arg.nStep)
                {
                    case DEF_CONST.SEQ_INIT:
                        arg.nStep = 10;
                        ChkTactTime();
                        arg.nStatus = eSTATE.Working;
                        break;
                    case 10:
                        switch (currCmd)
                        {                            
                            // Task 구동 전 비클 정지 후 Task를 시작한다.
                            case eVEC_CMD.Go2Goal:
                            case eVEC_CMD.Go2Point:
                            case eVEC_CMD.Go2Straight:
                            case eVEC_CMD.MoveDeltaHeading:
                            case eVEC_CMD.MoveFront:
                            case eVEC_CMD.GetDistBetween:
                            case eVEC_CMD.GetDistFromHere:
                            case eVEC_CMD.LocalizeAtGoal:
                                arg.nStep = 15;
                                StopRobot();
                                ResetTime();                                
                                break;
                            default: arg.nStep = 100; break;
                        }                        
                        break;
                    case 15:
                        if ( arg.tSen.IsOver(nTimeout) )
                        {
                            SetErr(eERROR.VEC_UNAVAILAABLE);
                        }
                        else
                        {
                            switch (vecState.state)
                            {
                                case eVECSTATE.STOPPED:
                                    if ( arg.tDly.IsOver(nDelay) )
                                    {
                                        arg.nStep = 100;
                                    }
                                    break;
                                default: arg.tDly.Reset(); break;
                            }
                        }
                        break;
                    case 100:
                        SendCmd();
                        switch (currCmd)
                        {
                            // 완료확인이 필요없는 Tasks
                            case eVEC_CMD.Stop: case eVEC_CMD.Say:
                            case eVEC_CMD.PauseCancel: case eVEC_CMD.Undock:
                            case eVEC_CMD.LocalizeAtGoal:
                                arg.nStep = 500;
                                break;
                            // 완료확인이 필요한 Tasks
                            case eVEC_CMD.Dock:
                            case eVEC_CMD.Go2Goal:
                            case eVEC_CMD.Go2Point:
                            case eVEC_CMD.Go2Straight:
                            case eVEC_CMD.MoveDeltaHeading:
                            case eVEC_CMD.MoveFront:
                            case eVEC_CMD.GetDistBetween:
                            case eVEC_CMD.GetDistFromHere:
                                arg.nStep = 105;
                                ResetTime();
                                break;
                            default: arg.nStep = 500; break;
                        }
                        break;
                    case 105:
                        if (arg.tSen.IsOver(nTimeout))
                        {
                            SetErr(eERROR.VEC_COMMTIMEOUT);
                        }
                        else
                        {
                            switch (currCmd)
                            {
                                // 완료확인이 필요한 Tasks
                                case eVEC_CMD.Dock:
                                case eVEC_CMD.Go2Goal:
                                case eVEC_CMD.Go2Point:
                                case eVEC_CMD.Go2Straight:
                                    switch (vecState.state)
                                    {
                                        case eVECSTATE.GOING_TO:
                                        case eVECSTATE.GOING_TO_POINT:
                                        case eVECSTATE.GOING_TO_DOCK_AT:
                                            if (arg.tDly.IsOver(nDelay))
                                            {
                                                arg.nStep = 120;
                                            }                                            
                                            break;
                                        default: break;
                                    }
                                    break;
                                case eVEC_CMD.MoveDeltaHeading:
                                case eVEC_CMD.MoveFront:
                                    switch (vecState.state)
                                    {
                                        case eVECSTATE.DOING_TASK_DELTAHEADING:
                                        case eVECSTATE.DOING_TASK_MOVE:
                                            if (arg.tDly.IsOver(nDelay))
                                            {
                                                ResetTime();
                                                arg.nStep = 110;
                                            }
                                            break;                                       
                                        default: break;
                                    }
                                    break;
                                case eVEC_CMD.GetDistBetween:
                                case eVEC_CMD.GetDistFromHere:
                                    switch (arg.nStatus)
                                    {                                        
                                        case eSTATE.Checked: arg.nStep = 120; break;
                                        case eSTATE.Failed:
                                            SetErr(eERROR.VEC_UNAVAILAABLE);
                                            arg.nStep = 120;
                                            break;                                        
                                        default: break;
                                    }
                                    break;
                                default: arg.tDly.Reset(); break;
                            }
                        }
                        break;
                    case 110:
                        if (arg.tSen.IsOver(nTimeout))
                        {
                            SetErr(eERROR.VEC_COMMTIMEOUT);
                        }
                        else
                        {
                            switch (currCmd)
                            {
                                case eVEC_CMD.MoveDeltaHeading:
                                case eVEC_CMD.MoveFront:
                                    switch (vecState.state)
                                    {
                                        case eVECSTATE.COMPLETED_DOING_TASK_MOVE:
                                        case eVECSTATE.COMPLETED_DOING_TASK_DELTAHEADING:
                                            if (arg.tDly.IsOver(nDelay))
                                            {
                                                arg.nStep = 120;
                                            }
                                            break;
                                        default: arg.tDly.Reset(); break;
                                    }
                                    break;
                                default: break;
                            }
                        }
                        break;
                    case 120:
                        arg.nStep = 500;
                        break;
                    case 500:
                        currCmd = eVEC_CMD.None;
                        arg.nStatus = eSTATE.Done;
                        arg.nStep = DEF_CONST.SEQ_MAIN_FINISH;
                        break;
                    case DEF_CONST.SEQ_MAIN_FINISH:
                        ChkTactTime();
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

        public void WrkStart(eVEC_CMD cmd, SENDARG data)
        {
            currCmd = cmd;
            _sendArg.CopyFrom(data);
            Init();
            WorkTrg();
        }

        private void SendCmd()
        {
            arg.nStatus = eSTATE.Checking;
            _ctrl.VEC_SendCmd(currCmd, _sendArg);
        }

        private void StopRobot()
        {
            _ctrl.VEC_SendCmd(eVEC_CMD.Stop, null);
        }

    }
}
