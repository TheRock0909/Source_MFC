using EzVehicle;
using Source_MFC.Global;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Source_MFC.HW.MobileRobot.LD
{
    public class LD : IVehicle
    {
        private class TIMEARG
        {
            public long nStart = 0;
            public long nDelay = 0;
            public long nCurr = 0;

            private void SetCheckTime(ref long nTime)
            {
                nTime = DateTime.Now.Ticks;
            }

            private bool GetCheckTimeOver(long nStartTm, long nDelay, ref long CurrTm)
            {
                bool rtn = false; long nCurrTime = 0; double dCurr = 0.0f;
                nCurrTime = System.DateTime.Now.Ticks;
                dCurr = (nCurrTime - nStartTm) / 10000.0f;
                rtn = (nDelay < dCurr) ? true : false;
                CurrTm = (long)dCurr;
                return rtn;
            }

            private void ChkCurrTime(ref long CurrTm)
            {
                long nCurrTime = 0; double dCurr = 0.0f;
                nCurrTime = System.DateTime.Now.Ticks;
                dCurr = (nCurrTime - this.nStart) / 10000.0f;
                CurrTm = (long)dCurr;
                this.nStart = 0;
            }

            public void Reset()
            {
                this.nStart = 0;
            }

            public bool IsOver(long Delay)
            {
                bool rtn = false;
                switch (this.nStart)
                {
                    case 0:
                        SetCheckTime(ref this.nStart);
                        this.nDelay = Delay;
                        break;
                    default:
                        rtn = GetCheckTimeOver(this.nStart, this.nDelay, ref this.nCurr);
                        if (true == rtn)
                        {
                            Reset();
                            return true;
                        }
                        break;
                }
                return rtn;
            }
        }

        bool bConnection = false, bInit = false;
        public bool _bConected => bConnection && bInit;
        public eVEHICLETYPE eType { get { return eVEHICLETYPE.LD; } }
        string _ip = null;
        LD_SOCK ldSock;
        public event EventHandler<bool> Evt_Connection;
        public event EventHandler<JCVT_VEC> Evt_UpdateData;
        private System.Timers.Timer _StatusChk = new System.Timers.Timer();
        public LD_STATUS _CurrStatus =>_curr;
        LD_STATUS _beforeSt = new LD_STATUS(null), _curr = new LD_STATUS(null);
        TIMEARG _tReqStaus = new TIMEARG();
        public bool bRobotPaused { get; set; } = false;
        public LD(string ip)
        {
            _ip = ip;
            ldSock = new LD_SOCK();
            bConnection = false;
            ldSock.Evt_Connection += On_Connection;
            ldSock.Evt_RecvdData += On_RecvdData;
            _StatusChk.Interval = 50;
            _StatusChk.Enabled = false;
            _StatusChk.Elapsed += _Status_Elapsed;
            _beforeSt.state.st = eVECSTATE.ROBOT_LOST;
            _tReqStaus.Reset();
        }
        public bool  Open()
        {
            ldSock.Conenct(_ip);
            _StatusChk.Enabled = true;
            return true;
        }

        public void Close()
        {
            bInit = false;
            _StatusChk.Enabled = false;
            ldSock.Dispose();
        }


        private void On_Connection(object sender, bool connection)
        {
            bConnection = connection;
            Evt_Connection?.Invoke(this, connection);
        }

        private void On_RecvdData(object sender, string data)
        {
            var temp = new LD_STATUS(data);                  
            switch (temp.cmd)
            {
                case eVECST_CMD_TYPE.EXTENDEDSTATUSFORHUMANS:
                case eVECST_CMD_TYPE.STATUS:
                case eVECST_CMD_TYPE.ERROR:
                case eVECST_CMD_TYPE.PAUSETASK:
                    _curr.state.st = temp.state.st;
                    _curr.state.dst = temp.state.dst;
                    _curr.state.subMsg = temp.state.subMsg;
                    break;
                case eVECST_CMD_TYPE.STATEOFCHARGE: _curr.batt = temp.batt; break;
                case eVECST_CMD_TYPE.LOCALIZATIONSCORE: _curr.local = temp.local; break;
                case eVECST_CMD_TYPE.TEMPERATURE: _curr.temp = temp.temp; break;
                case eVECST_CMD_TYPE.LOCATION: _curr.pos = new nPOS_XYR() { x = temp.pos.x, y = temp.pos.y, r = temp.pos.r }; break;
                case eVECST_CMD_TYPE.PARKING: _curr.state.st = temp.state.st; break;
                case eVECST_CMD_TYPE.DISTANCEBETWEEN:
                case eVECST_CMD_TYPE.DISTANCEFROMHERE:
                    {
                        var rlt = temp.dist.cmd.ToEnum<eSTATE>();
                        _curr.dist.cmd = rlt.ToString();
                        _curr.dist.goal1 = temp.dist.goal1;
                        _curr.dist.goal2 = temp.dist.goal2;
                        _curr.dist.result = temp.dist.result;
                        break;
                    }
                case eVECST_CMD_TYPE.TRANSGO:
                    {
                        _curr.cmd = temp.cmd;
                        _curr.dist.cmd = temp.dist.cmd;
                        _curr.dist.goal1 = temp.dist.goal1;
                        break;
                    }
                default: break;
            }

            switch (temp.state.st)
            {
               
                case eVECSTATE.PAUSING: bRobotPaused = true; break;
                case eVECSTATE.PAUSE_CANCELLED: bRobotPaused = false; break;
                case eVECSTATE.PAUSE_INTERRUPTED:                    
                default: bRobotPaused = false; break;
            }

            switch (temp.cmd)
            {
                case eVECST_CMD_TYPE.NONE: Logger.Inst.Write(CmdLogType.Comm, $"LD-R(Parsing Failed) : {data}", CommLogType.VEHICLE); break;
                case eVECST_CMD_TYPE.ENTER_PASSWORD:
                    SendQuery("adept", false);
                    bInit = true;
                    break;
                case eVECST_CMD_TYPE.END_OF_COMMANDS: break;
                default:
                    var rcv = JCVT_VEC.Set(_curr);
                    Evt_UpdateData?.Invoke(this, rcv);
                    if (_beforeSt.state.st != _curr.state.st || _beforeSt.state.dst != _curr.state.dst)
                    {
                        _beforeSt.state.st = _curr.state.st;
                        _beforeSt.state.dst = _curr.state.dst;
                        Logger.Inst.Write(CmdLogType.Comm, $"LD-R : {data}", CommLogType.VEHICLE);
                    }
                    break;
            }
        }

        private void _Status_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (true == _bConected)
            {
                if (_tReqStaus.IsOver(1 * 1000))
                {
                    SendQuery("status");
                }
            }
        }

        public void Send(eVEC_CMD cmd, JCVT_VEC data = null)
        {
            bool bSend = true;
            string cmdmsg = string.Empty;
            switch (data.typeName)
            {
                case "SENDARG":
                    {
                        if (null != data)
                        {
                            SENDARG arg;
                            JCVT_VEC.Get(data.json, out arg);
                            switch (cmd)
                            {
                                case eVEC_CMD.Say: cmdmsg = $"say {arg.msg}"; break;
                                case eVEC_CMD.LocalizeAtGoal: cmdmsg = $"dotask localizeatgoal {arg.goal_1st}"; break;
                                case eVEC_CMD.Go2Goal:
                                case eVEC_CMD.Go2Point:
                                case eVEC_CMD.Go2Straight:
                                    {
                                        ChangeToIdleMode();
                                        switch (cmd)
                                        {
                                            case eVEC_CMD.Go2Goal: cmdmsg = $"goto {arg.goal_1st}"; break;
                                            case eVEC_CMD.Go2Point: cmdmsg = $"gotopoint {arg.pos.x} {arg.pos.y}"; break;
                                            case eVEC_CMD.Go2Straight: cmdmsg = $"doTask gotostraight {arg.goal_1st}"; break;
                                        }
                                        break;
                                    }
                                case eVEC_CMD.MoveDeltaHeading:
                                case eVEC_CMD.MoveFront:
                                    {
                                        if (0 >= arg.acc) arg.acc = 10;
                                        if (0 >= arg.dec) arg.acc = 10;
                                        switch (cmd)
                                        {

                                            case eVEC_CMD.MoveDeltaHeading:
                                                if (30 >= arg.dec)
                                                {
                                                    arg.acc = 30;
                                                }
                                                cmdmsg = $"dotask deltaheading {arg.move} {arg.spd} {arg.acc} {arg.dec}";
                                                break;
                                            case eVEC_CMD.MoveFront:
                                                if (50 >= arg.dec)
                                                {
                                                    arg.acc = 50;
                                                }
                                                cmdmsg = $"dotask move {arg.move} {arg.spd} {arg.acc} {arg.dec} 10";
                                                break;
                                        }
                                        break;
                                    }
                                case eVEC_CMD.GetDistBetween:
                                    _curr.dist.cmd = eSTATE.Checking.ToString();
                                    cmdmsg = $"distancebetween {arg.goal_1st} {arg.goal_2nd}";
                                    break;
                                case eVEC_CMD.GetDistFromHere:
                                    _curr.dist.cmd = eSTATE.Checking.ToString();
                                    cmdmsg = $"distanceFromHere {arg.goal_1st}";
                                    break;
                                case eVEC_CMD.SendMassage: ArclMsg(arg.msg); break;
                                default: bSend = false; break;
                            }
                        }
                        else
                        {
                            switch (cmd)
                            {
                                case eVEC_CMD.Stop: cmdmsg = $"stop"; break;
                                case eVEC_CMD.PauseCancel: cmdmsg = $"pausetaskcancel"; break;
                                case eVEC_CMD.Dock: cmdmsg = $"dock"; break;
                                case eVEC_CMD.Undock: cmdmsg = $"undock"; break;
                                default: bSend = false; break;
                            }
                        }

                        if (true == bSend)
                        {
                            SendQuery(cmdmsg);
                        }
                        break;
                    }
                default: break;
            }
        }

        private void ChangeToIdleMode()
        {
            SendQuery("go", false);
        }

        private void SendQuery(string msg, bool bLog = true)
        {
            try
            {
                if (true == bConnection)
                {
                    ldSock.Send($"{msg}\r\n");
                    if (!msg.Contains("status") && true == bLog)
                    {
                        Logger.Inst.Write(CmdLogType.Comm, $"LD-S : {msg}", CommLogType.VEHICLE);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, $"{e.ToString()}");
            }
        }

        public void ArclMsg(string msg, bool bWriteLog = true)
        {
            SendQuery($"arclsendtext {msg}", bWriteLog);
        }
    }
}
