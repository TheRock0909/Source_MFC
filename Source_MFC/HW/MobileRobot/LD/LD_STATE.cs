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
    public class LD_STATE
    {
        public eVECST_CMD_TYPE cmd { get; }
        public eVECSTATE st { get; set; }
        public string dst { get; set; }
        public string subMsg { get; set; }
        public string msg { get; }
        

        public LD_STATE(eVECST_CMD_TYPE Cmd, string rcvMsg)
        {
            cmd = Cmd; msg = rcvMsg;
            st = eVECSTATE.NONE;
            dst = string.Empty;
            subMsg = string.Empty;
        }

        public LD_STATE Get()
        {
            try
            {
                switch (cmd)
                {
                    case eVECST_CMD_TYPE.EXTENDEDSTATUSFORHUMANS:
                        {
                            var chk = EXTENDEDSTATUSFORHUMANS();
                            st = chk.st;
                            dst = chk.dst;
                            subMsg = chk.subStr;
                            break;
                        }
                    case eVECST_CMD_TYPE.STATUS:
                        {
                            var chk = STATUS();
                            st = chk.st;
                            dst = chk.dst;
                            break;
                        }
                    case eVECST_CMD_TYPE.ERROR:
                    case eVECST_CMD_TYPE.PAUSETASK:
                        {
                            st = ERROR_PAUSETASK();
                            break;
                        }
                    case eVECST_CMD_TYPE.DISTANCEBETWEEN:                        
                        break;
                    default: break;
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, $"{e.ToString()}");
            }
            return this;
        }

        private enum eSTATE_TYPE
        {
              NONE
            , IDLE
            , GOING     // 4 EXTENDEDSTATUSFORHUMANS
            , FALIED
            , ESTOP
            , DRIVING
            , MOVING
            , PARKING

            , STOPPING // 4 STATUS
            , STOPPED
            , ARRIVED
            , TELEOP
            , DOCKINGSTATE
            , DOING
            , COMPLETED            
            , PARKED

            , CANNOT // 4 ERROR
            , NO

            , PAUSING // 4 PAUSETASK
            , PAUSE
        }        

        private (eVECSTATE st, string dst, string subStr) EXTENDEDSTATUSFORHUMANS()
        {
            var rtn = eVECSTATE.NONE; string destination = string.Empty; string sub = string.Empty;
            try
            {                
                string[] splitWord = msg.ToUpper().Split(':');
                string[] splitStr = splitWord[1].ToUpper().Split('|');
                string[] splitState = splitStr[0].Split(' ');

                for (int i = 0; i < splitStr.Count(); i++)
                {
                    if (i == 0) continue;
                    sub += $"{splitWord[i]}";
                }

                switch (splitState.Count())
                {
                    case 1: rtn = splitState[0].ToEnum<eVECSTATE>(); break;
                    case 2:
                        {
                            var chkstr = splitState[1].Replace(" ", "_");
                            rtn = chkstr.ToEnum<eVECSTATE>();
                            break;
                        }
                    default:
                        if (2 < splitState.Count())
                        {
                            var type = splitState[1].ToEnum<eSTATE_TYPE>();
                            switch (type)
                            {
                                case eSTATE_TYPE.GOING:
                                    switch (splitState.Count())
                                    {
                                        case 3:
                                            rtn = $"{splitState[0]}_{splitState[1]}".ToEnum<eVECSTATE>();
                                            destination = splitState[2];
                                            break;
                                        default:
                                            switch (splitState[2])
                                            {
                                                case "POINT":
                                                    rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}".ToEnum<eVECSTATE>();
                                                    destination = $"{splitState[3]},{splitState[4]}";
                                                    break;
                                                case "DOCK":
                                                    rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}_{splitState[3]}".ToEnum<eVECSTATE>();
                                                    destination = $"{splitState[4]}";
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case eSTATE_TYPE.FALIED:
                                    switch (splitState.Count())
                                    {
                                        case 4:
                                            rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}".ToEnum<eVECSTATE>();
                                            destination = $"{splitState[3]}";
                                            break;
                                        case 5:
                                            rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}_{splitState[3]}".ToEnum<eVECSTATE>();
                                            destination = $"{splitState[4]}";
                                            break;
                                    }
                                    break;
                                case eSTATE_TYPE.IDLE: rtn = eVECSTATE.IDLE_PROCESSING; break;
                                case eSTATE_TYPE.ESTOP: rtn = $"{splitState[1]}_{splitState[2]}".ToEnum<eVECSTATE>(); ; break;
                                case eSTATE_TYPE.DRIVING: rtn = eVECSTATE.DRIVING_INTO_DOCK; break;
                                case eSTATE_TYPE.MOVING:
                                    rtn = eVECSTATE.MOVING;
                                    destination = splitState[1];
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, $"{e.ToString()}");
            }

            if (rtn == eVECSTATE.NONE)
            {
                rtn = eVECSTATE.NONE;
            }
            return (rtn, destination, sub);
        }

        internal (eVECSTATE st, string dst) STATUS()
        {
            var rtn = eVECSTATE.NONE; string destination = null;
            string[] splitWord = msg.ToUpper().Split(':');
            string[] splitState = splitWord[1].Split(' ');
            var type = splitState[1].Replace("\r", string.Empty).ToEnum<eSTATE_TYPE>();
            try
            {               
                switch (type)
                {
                    case eSTATE_TYPE.STOPPING:
                    case eSTATE_TYPE.STOPPED:
                    case eSTATE_TYPE.PARKING:
                    case eSTATE_TYPE.PARKED: rtn = splitState[1].Replace("\r", string.Empty).ToEnum<eVECSTATE>(); break;
                    case eSTATE_TYPE.TELEOP: rtn = eVECSTATE.TELEOP_DRIVING; break;
                    case eSTATE_TYPE.ESTOP: rtn = (2 >= splitState.Count()) ? eVECSTATE.ESTOP_PRESSED : eVECSTATE.ESTOP_RELIEVED; break;
                    case eSTATE_TYPE.GOING:
                    case eSTATE_TYPE.ARRIVED:
                        switch (splitState.Count())
                        {
                            case 3:
                                rtn = $"{splitState[0]}_{splitState[1]}".ToEnum<eVECSTATE>();
                                destination = splitState[2];
                                break;
                            default:
                                switch (splitState[2])
                                {
                                    case "POINT":
                                        rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}".ToEnum<eVECSTATE>();
                                        destination = $"{splitState[3]},{splitState[4]}";
                                        break;
                                    default: break;
                                }
                                break;
                        }
                        break;
                    case eSTATE_TYPE.DOING:
                        rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}".ToEnum<eVECSTATE>();
                        switch (rtn)
                        {
                            case eVECSTATE.DOING_TASK_DELTAHEADING:
                                destination = $"{splitState[3]},{splitState[4]},{splitState[5]},{splitState[6]}";
                                break;
                            case eVECSTATE.DOING_TASK_MOVE:
                                destination = $"{splitState[3]},{splitState[4]},{splitState[5]},{splitState[6]}_{splitState[5]}";
                                break;
                            case eVECSTATE.DOING_TASK_PAUSE:
                                break;
                            default: break;
                        }
                        break;
                    case eSTATE_TYPE.COMPLETED:
                        rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}_{splitState[3]}".ToEnum<eVECSTATE>();
                        switch (rtn)
                        {
                            case eVECSTATE.COMPLETED_DOING_TASK_DELTAHEADING:
                                destination = $"{splitState[4]},{splitState[5]},{splitState[6]},{splitState[7]}";
                                break;
                            case eVECSTATE.COMPLETED_DOING_TASK_MOVE:
                                destination = $"{splitState[4]},{splitState[5]},{splitState[6]},{splitState[7]},{splitState[8]}";
                                break;
                            case eVECSTATE.COMPLETED_DOING_TASK_SETHEADING: destination = $"{splitState[4]}"; break;
                            default: break;
                        }
                        break;
                    case eSTATE_TYPE.DOCKINGSTATE:
                        {
                            string[] splitdock = splitWord[2].Split(' ');
                            switch (splitdock[0])
                            {
                                case "DOCKED":
                                case "DOCKING":
                                case "UNDOCKING":
                                    rtn = splitdock[0].ToEnum<eVECSTATE>();
                                    break;
                                default: break;
                            }
                            break;
                        }
                    default: break;
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, $"{e.ToString()}");
            }
            if ( rtn == eVECSTATE.NONE )
            {
                rtn = eVECSTATE.NONE;
            }
            return (rtn, destination);
        }

        internal eVECSTATE ERROR_PAUSETASK()
        {
            var rtn = eVECSTATE.NONE;
            try
            {
                string[] splitWord = msg.ToUpper().Split(':');
                string[] splitState = splitWord[1].Split(' ');
                var type = splitState[0].ToEnum<eSTATE_TYPE>();
                switch (type)
                {
                    case eSTATE_TYPE.FALIED: rtn = eVECSTATE.FAILED_GOING_TO; break;
                    case eSTATE_TYPE.CANNOT: rtn = eVECSTATE.CANNOT_FIND_PATH; break;
                    case eSTATE_TYPE.NO: rtn = eVECSTATE.NO_ENTER; break;
                    case eSTATE_TYPE.PAUSING: rtn = eVECSTATE.PAUSING; break;
                    case eSTATE_TYPE.PAUSE:
                        rtn = $"{splitState[0]}_{splitState[1]}".ToEnum<eVECSTATE>();
                        break;
                    default: break;
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, $"{e.ToString()}");
            }            
            return rtn;
        }
    }
}
