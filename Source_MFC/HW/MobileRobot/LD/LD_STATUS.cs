using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.HW.MobileRobot.LD
{
    public enum eVECST_CMD_TYPE : int
    {
          NONE = 0
        , ENTER_PASSWORD = 10
        , END_OF_COMMANDS
        , STATEOFCHARGE
        , LOCATION
        , LOCALIZATIONSCORE
        , TEMPERATURE
        , PARKING

        , EXTENDEDSTATUSFORHUMANS = 100
        , STATUS
        , ERROR
        , PAUSETASK

        , COMMANDERRORDESCRIPTION = 200
        , DISTANCEFROMHERE
        , DISTANCEBETWEEN
        , RANGEDEVICEGETCURRENT

        // Use for PCL Mode
        , TRANSGO
        , AFTERSTART
        , KCUNLOADING
        , KCLOADING
        , JRUNLOADING
        , JRLOADING
        , SJUNLOADING
        , SJLOADING

    }
    public enum eVECSTATE : int
    {
          NONE = -1
        , IDLE_PROCESSING
        , STOPPING
        , STOPPED
        , AFTER_GOAL
        , ARRIVED_AT
        , ARRIVED_AT_POINT
        , COMPLETED_DOING_TASK_MOVE
        , COMPLETED_DOING_TASK_DELTAHEADING
        , COMPLETED_DOING_TASK_SETHEADING
        , DOCKINGSTATE
        , GOING_TO
        , GOING_TO_POINT
        , GOING_TO_DOCK_AT
        , DRIVING
        , DRIVING_INTO_DOCK
        , TELEOP_DRIVING
        , MOVING
        , DOING_TASK_DELTAHEADING
        , DOING_TASK_MOVE
        , DOING_TASK_PAUSE
        , DONE_DRIVING
        , DOCKING
        , DOCKED
        , UNDOCKING
        , ROBOT_LOST
        , FAILED_GOING_TO
        , ESTOP_PRESSED
        , ESTOP_RELIEVED
        , CANNOT_FIND_PATH
        , NO_ENTER
        , PAUSING
        , PAUSE_CANCELLED
        , PAUSE_INTERRUPTED
        , PARKING
        , PARKED
    }

    public class LD_STATUS
    {
        public eVECST_CMD_TYPE cmd { get; set; } = eVECST_CMD_TYPE.NONE;
        public LD_STATE state { get; set; } = new LD_STATE(eVECST_CMD_TYPE.NONE, string.Empty);
        public nPOS_XYR pos { get; set; } = new nPOS_XYR();
        public double temp { get; set; } = 0;
        public double local { get; set; } = 0;
        public double batt { get; set; } = 0;
        public string dest { get; set; } = string.Empty;
        public MP_DISTBTW dist { get; set; } = new MP_DISTBTW();

        public LD_STATUS(string rcvMsg)
        {
            double dTemp = 0;
            pos = new nPOS_XYR();
            temp = local = batt = 0;
            dest = string.Empty;
            if (null != rcvMsg)
            {
                string[] splitStr = rcvMsg.Split(':');
                cmd = GetCmd(rcvMsg);
                switch (cmd)
                {
                    case eVECST_CMD_TYPE.EXTENDEDSTATUSFORHUMANS:
                    case eVECST_CMD_TYPE.STATUS:
                    case eVECST_CMD_TYPE.ERROR:
                    case eVECST_CMD_TYPE.PAUSETASK:
                        {
                            var chk = new LD_STATE(cmd, rcvMsg);
                            var st = chk.Get();
                            state.st = st.st;
                            state.dst = st.dst;
                            state.subMsg = st.subMsg;
                            break;
                        }
                    case eVECST_CMD_TYPE.ENTER_PASSWORD:
                        break;
                    case eVECST_CMD_TYPE.END_OF_COMMANDS:
                        break;
                    case eVECST_CMD_TYPE.STATEOFCHARGE:
                        if (true == double.TryParse(splitStr[1], out dTemp))
                        {
                            batt = dTemp;
                        }
                        break;
                    case eVECST_CMD_TYPE.LOCALIZATIONSCORE:
                        if (true == double.TryParse(splitStr[1], out dTemp))
                        {
                            local = dTemp * 100.0f;
                        }
                        break;
                    case eVECST_CMD_TYPE.TEMPERATURE:
                        if (true == double.TryParse(splitStr[1], out dTemp))
                        {
                            temp = dTemp * 100.0f;
                        }
                        break;
                    case eVECST_CMD_TYPE.LOCATION:
                        {
                            var data = splitStr[1].Split(' ');
                            if (true == double.TryParse(data[1], out dTemp))
                            {
                                pos.x = (int)dTemp;
                            }
                            if (true == double.TryParse(data[2], out dTemp))
                            {
                                pos.y = (int)dTemp;
                            }
                            if (true == double.TryParse(data[3], out dTemp))
                            {
                                pos.r = (int)dTemp;
                            }
                            break;
                        }
                    case eVECST_CMD_TYPE.PARKING:
                        state.st = eVECSTATE.PARKING;
                        break;
                    case eVECST_CMD_TYPE.DISTANCEFROMHERE:
                        {
                            var buff1 = new MP_DISTBTW();
                            string result = Ctrls.Remove_line(rcvMsg);
                            result = result.Replace("\"", "");
                            var split = result.Split(' ');
                            var distance = 0;
                            if (int.TryParse(split[1], out distance))
                            {
                                dist.cmd = eSTATE.Done.ToString();
                                dist = new MP_DISTBTW() { goal1 = split[2], goal2 = string.Empty, result = distance };
                            }                            
                            else
                            {
                                dist.cmd = eSTATE.Failed.ToString();
                            }                            
                            break;
                        }
                    case eVECST_CMD_TYPE.DISTANCEBETWEEN:
                        {
                            var buff = new MP_DISTBTW();
                            var rtn = ParsDistance(rcvMsg, ref buff);
                            if (rtn)
                            {
                                dist.cmd = eSTATE.Done.ToString();
                                dist = new MP_DISTBTW() { goal1 = buff.goal1, goal2 = buff.goal2, result = buff.result };
                            }
                            else
                            {
                                dist.cmd = eSTATE.Failed.ToString();
                            }
                            break;
                        }
                    case eVECST_CMD_TYPE.TRANSGO:                    
                        {
                            var buff = new MP_DISTBTW();
                            var rtn = ParsTransGo(rcvMsg, ref buff);
                            if ( true == rtn )
                            {
                                dist = new MP_DISTBTW() { cmd = buff.cmd, goal1 = buff.goal1 };
                            }
                            else
                            {
                                dist.cmd = eSTATE.Failed.ToString();
                            }
                            break;
                        }
                    default: break;
                }
            }
            else
            {
                cmd = eVECST_CMD_TYPE.NONE;
                state = new LD_STATE(eVECST_CMD_TYPE.NONE, null);
            }
        }

        private bool ParsTransGo(string msg, ref MP_DISTBTW dist)
        {
            var rtn = true;
            var result = Ctrls.Remove_line(msg);
            var split = result.Split('_');
            switch (split[3])
            {
                case "UNLOADJRUL":
                case "UNLOADKCUL":
                    dist.cmd = eJOBTYPE.UNLOADING.ToString();
                    dist.goal1 = $"{split[1]}_{split[2]}_U";
                    dist.goal2 = split[3].Contains("UNLOADJRUL") ? ePIOTYPE.JR.ToString() : ePIOTYPE.KCH.ToString();
                    break;
                case "LOADJRLD":
                case "LOADKCLD":
                    dist.cmd = eJOBTYPE.LOADING.ToString();
                    dist.goal1 = $"{split[1]}_{split[2]}_L";
                    dist.goal2 = split[3].Contains("LOADJRLD") ? ePIOTYPE.JR.ToString() : ePIOTYPE.KCH.ToString();
                    break;
                default: rtn = false; break;
            }            
            return rtn;
        }

        private bool ParsDistance(string msg, ref MP_DISTBTW dist)
        {
            bool rtn = true;
            var result = Ctrls.Remove_line(msg);
            result = result.Replace("\"", "");
            var split = result.Split(' ');
            if (0 <= split[0].IndexOf("COMMANDERROR"))
            {
                rtn = false;
            }
            else
            {
                rtn = double.TryParse(split[1], out dist.result);
            }
            if (true == rtn)
            {
                dist.goal1 = split[2]; dist.goal2 = split[3];
            }
            return rtn;
        }

        internal eVECST_CMD_TYPE GetCmd(string rcvMsg)
        {
            eVECST_CMD_TYPE rtn = eVECST_CMD_TYPE.NONE;
            var temp = Ctrls.Remove_line(rcvMsg);
            string[] splitStr = temp.Replace(" ", "_").ToUpper().Split(':');
            foreach (var item in splitStr)
            {
                var cmd = $"{item}";
                rtn = cmd.ToEnum<eVECST_CMD_TYPE>();
                if (eVECST_CMD_TYPE.NONE != rtn) break;
            }

            switch (rtn)
            {
                case eVECST_CMD_TYPE.NONE:                    
                    string[] arclmsgs = temp.Replace(" ", "_").ToUpper().Split('_');
                    if (4 <= arclmsgs.Count())
                    {
                        rtn = arclmsgs[0].ToUpper().ToEnum<eVECST_CMD_TYPE>();
                    }
                    else
                    {
                        rtn = temp.ToUpper().ToEnum<eVECST_CMD_TYPE>();
                    }                    
                    break;
                default: break;
            }
            return rtn;
        }
    }
}
