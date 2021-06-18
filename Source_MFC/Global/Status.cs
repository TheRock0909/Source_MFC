using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Source_MFC.HW.MobileRobot.LD;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Source_MFC.Utils.MsgBox;

namespace Source_MFC.Global
{
    public class TIMEARG
    {
        public long nStart = 0;
        public long nDelay = 0;
        public long nCurr = 0;

        private void SetTime(ref long nTime)
        {
            nTime = DateTime.Now.Ticks;
        }

        private bool ChkTimeOver(long nStartTm, long nDelay, ref long CurrTm)
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

        public void Check()
        {
            switch (this.nStart)
            {
                case 0: SetTime(ref this.nStart); break;
                default: ChkCurrTime(ref this.nCurr); break;
            }
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
                    SetTime(ref this.nStart);
                    this.nDelay = Delay;
                    break;
                default:
                    rtn = ChkTimeOver(this.nStart, this.nDelay, ref this.nCurr);
                    if (true == rtn)
                    {
                        this.nStart = 0;
                        return true;
                    }
                    break;
            }
            return rtn;
        }

        public bool IsOver()
        {
            bool rtn = false;
            switch (this.nStart)
            {
                case 0: SetTime(ref this.nStart); break;
                default:
                    rtn = ChkTimeOver(this.nStart, this.nDelay, ref this.nCurr);
                    if (true == rtn)
                    {
                        this.nStart = 0;
                        return true;
                    }
                    break;
            }
            return rtn;
        }
    }

    public class TASKARG
    {
        public eSEQLIST nID = 0;
        public eSTATE nStatus = 0;
        public int nStep = 0;
        public eERROR nErr = 0;
        public bool bStop = false;
        public int nTrg = 0;
        public int nStuff = 0;
        public double result = 0;
        public eTASKLIST errSubTsk = eTASKLIST.MAX_SUB_SEQ;        
        public TIMEARG tWrk = new TIMEARG();
        public TIMEARG tSen = new TIMEARG();
        public TIMEARG tDly = new TIMEARG();

        public TASKARG(eSEQLIST ID)
        {
            this.nID = ID;
        }

        public TASKARG()
        {
        }

        public eSEQLIST GetID()
        {
            return this.nID;
        }

        public void Init()
        {
            this.nStatus = eSTATE.None;
            this.nStep = DEF_CONST.SEQ_FINISH;
            this.nErr = eERROR.None;
            this.bStop = false;
            this.nTrg = 0;
            this.nStuff = 0;
            this.tWrk.nStart = 0;
            this.errSubTsk = eTASKLIST.MAX_SUB_SEQ;
            ResetTime();
        }

        public void ResetTime()
        {
            this.tSen.Reset();
            this.tDly.Reset();
            this.tWrk.Reset();
        }

        public void WorkTrg()
        {
            this.nStatus = eSTATE.Working;
            this.nStep = DEF_CONST.SEQ_INIT;
        }

        public void StopTrg()
        {
            this.nStatus = eSTATE.Stopping;
        }

        public void SetErr(eERROR err, eTASKLIST tsk = eTASKLIST.MAX_SUB_SEQ)
        {
            switch (this.nErr)
            {
                case eERROR.None:
                    this.bStop = true;
                    this.nStatus = eSTATE.Stopping;
                    this.nErr = err;
                    this.errSubTsk = tsk;
                    break;
                default: break;
            }
        }

        public void Reset()
        {
            this.bStop = false;
            switch (this.nStatus)
            {
                case eSTATE.Stopped:
                case eSTATE.Error:
                    this.nStatus = eSTATE.Working;
                    break;
                default: break;
            }
            this.nErr = eERROR.None;
            ResetTime();
        }
    }

    public class SUBTSKARG
    {
        public eTASKLIST nID = 0;
        public eSTATE nStatus = 0;
        public int nStep = 0;
        public eERROR nErr = 0;
        public bool bStop = false;
        public bool bTrg = false;
        public string errCmt = string.Empty;
        public int Idx = 0;
        public int nTrg = 0;
        public int nStuff = 0;
        public double result = 0.0;
        public string rcp = string.Empty;
        public dPOS_XY pos = new dPOS_XY();
        public nPOS_XY node = new nPOS_XY();
        public TIMEARG tWrk = new TIMEARG();
        public TIMEARG tSen = new TIMEARG();
        public TIMEARG tDly = new TIMEARG();

        public SUBTSKARG(eTASKLIST ID)
        {
            this.nID = ID;
        }

        public eTASKLIST GetID()
        {
            return this.nID;
        }

        public void Init()
        {
            this.nStatus = eSTATE.None;
            this.nStep = DEF_CONST.SEQ_FINISH;
            this.nErr = eERROR.None;
            this.bStop = false;
            this.nTrg = 0;
            this.nStuff = 0;
            this.errCmt = this.rcp = string.Empty;
            this.tWrk.Reset();
            ResetTactTime();
        }

        public void WorkTrg()
        {
            this.bStop = false;
            this.nErr = eERROR.None;
            this.Idx = 0;
            this.nStuff = 0;
            this.nStatus = eSTATE.WorkTrg;
            this.nStep = DEF_CONST.SEQ_INIT;
            this.tWrk.Reset();
        }

        public void StopTrg()
        {
            this.bStop = true;
            this.nStatus = eSTATE.Stopped;
        }

        public void ResetTactTime()
        {
            this.tSen.Reset();
            this.tDly.Reset();
        }

        public void Resume()
        {
            if (true == this.bStop || eSTATE.Stopped == this.nStatus)
            {
                this.nErr = eERROR.None;
                this.nStatus = eSTATE.Working;
                ResetTactTime();
                this.bStop = false;
                this.bTrg = false;
                this.errCmt = string.Empty;
                this.tWrk.Reset();
            }
        }

        public void SetErr(eERROR err, string cmt = "")
        {
            switch (this.nErr)
            {
                case eERROR.None:
                    Logger.Inst.Write(CmdLogType.Err, $"Sub-{nID.ToString()} = ERR : {err}, STEP : {nStep}");
                    this.bStop = true;
                    this.nStatus = eSTATE.Error;
                    this.nErr = err;
                    this.errCmt = cmt;
                    this.bTrg = false;
                    break;
                default: break;
            }
        }
    }

    public class CONNECSION
    {
        public eCOMSTATUS mPlus { get; set; } = eCOMSTATUS.DISCONNECTED;
        public eCOMSTATUS io { get; set; } = eCOMSTATUS.DISCONNECTED;
        public eCOMSTATUS Vec { get; set; } = eCOMSTATUS.DISCONNECTED;

        public void Init()
        {
            mPlus = eCOMSTATUS.DISCONNECTED;
            io = eCOMSTATUS.DISCONNECTED;
            Vec = eCOMSTATUS.DISCONNECTED;
        }
    }

    public class LogMsg
    {
        public CmdLogType logUsr;
        public string msg;
    }

    public class MSGBOXDATA
    {
        public string message = string.Empty;
        public MsgType msgType = MsgType.Info;
        public eBTNSTYLE btnStyle = eBTNSTYLE.OK;
    }

    public class INPUTBOXDATA
    {
        public PackIconKind packIcon;
        public string title = string.Empty;
    }

    public class VEHICLESTATE
    {
        public int soc { get; set; } = 0;
        public int temp { get; set; } = 0;
        public int localize { get; set; } = 0;
        public eVECSTATE state { get; set; } = eVECSTATE.NONE;
        public nPOS_XYR pos { get; set; } = new nPOS_XYR() ;        
        public eROBOTST JobState { get; set; } = eROBOTST.NotAssigned;
        public string dst { get; set; }
        public string msg { get; set; }
        public string subMsg { get; set; }        
        public VEHICLESTATE()
        {
            soc = 0;
            state = eVECSTATE.NONE;
            pos = new nPOS_XYR();
        }

        public void Job_Reset()
        {
            JobState = eROBOTST.NotAssigned;
        }        
    }

    public class JOBOPT
    {
        public bool bSkipGo2Dest { get; set; }
        public bool bSkipPIO { get; set; }     
        public bool bSkipTransfer { get; set; }
        
        public JOBOPT()
        {
            bSkipGo2Dest = false;
            bSkipPIO = false;
            bSkipTransfer = false;            
        }
    }

    public class JOB
    {
        public eJOBTYPE type { get; set; } = eJOBTYPE.NONE;
        public string cmdID { get; set; } = string.Empty;
        public GOALITEM goal { get; set; } = new GOALITEM();
        public JOBOPT opt { get; set; } = new JOBOPT();
        public eJOBST state { get; set; } = eJOBST.None;
        public string materialID { get; set; } = string.Empty;
        public int partidx { get; set; } = 0;
        public string size { get; set; } = string.Empty;
        public bool bContinueWrk { get; set; } = false;
        public ePIOTYPE pio { get; set; } = ePIOTYPE.NOUSE;

        public void Init()
        {
            this.type = eJOBTYPE.NONE;
            this.cmdID = string.Empty;
            this.goal = new GOALITEM();
            this.opt = new JOBOPT();
            this.state = eJOBST.None;
            this.materialID = string.Empty;
            this.partidx = 0;
            this.bContinueWrk = false;
            this.size = string.Empty;
            this.pio = ePIOTYPE.NOUSE;
        }

        public void Set(MP_JOB Order)
        {
            var job = Order.Job;
            var goal = Order.Job.goal;

            this.type = job.type;
            this.cmdID = job.cmdID;
            this.goal = new GOALITEM()
            {
                type = goal.type, mcType = goal.mcType, line = goal.line
              , hostName = goal.hostName, name = goal.name, label = goal.label
              , pos = new nPOS_XYR() { x = goal.pos.x, y = goal.pos.y, r = goal.pos.r }
              , escape = new nPOS_XYR() { x = goal.escape.x, y = goal.escape.y, r = goal.escape.r }
            } ;
            this.opt = new JOBOPT();
            this.state = eJOBST.None;
            this.materialID = job.materialID;
            this.partidx = job.partidx;
            this.bContinueWrk = job.bContinueWrk;
            this.size = job.size;
            this.pio = job.pio;
        }

        public void Set(JOB job)
        {
            this.type = job.type;
            this.cmdID = job.cmdID;
            this.goal = new GOALITEM()
            {
                type = job.goal.type, mcType = job.goal.mcType, line = job.goal.line
              , hostName = job.goal.hostName, name = job.goal.name, label = job.goal.label
              , pos = new nPOS_XYR() { x = job.goal.pos.x, y = job.goal.pos.y, r = job.goal.pos.r }
              , escape = new nPOS_XYR() { x = job.goal.escape.x, y = job.goal.escape.y, r = job.goal.escape.r }
            };
            this.opt = new JOBOPT();
            this.state = eJOBST.None;
            this.materialID = job.materialID;
            this.partidx = job.partidx;
            this.bContinueWrk = job.bContinueWrk;
            this.size = job.size;
            this.pio = job.pio;
        }
    }

    public class MP_JOB
    {
        public string rcvmsg = string.Empty;
        public JOB Job = new JOB();
        public static MP_JOB Parse(string input, GOALINFO goalinfo)
        {
            string[] words = input.ToUpper().Split(';');
            MP_JOB data = new MP_JOB();
            data.Job = new JOB();
            data.rcvmsg = input;
            switch (words[0])
            {
                case "SRC": data.Job.type = eJOBTYPE.UNLOADING; break; // 설비기준임.                    
                case "DST": data.Job.type = eJOBTYPE.LOADING; break; // 설비기준임.                    
                case "STANDBY": data.Job.type = eJOBTYPE.STANDBY; break;
                case "CHARGE": data.Job.type = eJOBTYPE.CAHRGE; break;
            }
            switch (data.Job.type)
            {                
                case eJOBTYPE.LOADING: case eJOBTYPE.UNLOADING:
                    {
                        data.Job.cmdID = words[1]; //(GOAL_TYPE)Enum.Parse(typeof(GOAL_TYPE), words[1]);
                        data.Job.goal = goalinfo.Get(eJOBTYPE.UNLOADING == data.Job.type ? eGOALTYPE.Pickup : eGOALTYPE.Dropoff, words[2], eSRCHGOALBY.Host);
                        data.Job.materialID = words[3];
                        string strIdx = words[4].Substring(0, 1);
                        data.Job.partidx = Convert.ToInt32(strIdx);
                        data.Job.size = words[4].Substring(2, 1);
                        switch (words[5].ToUpper())
                        {
                            case "TRUE": data.Job.bContinueWrk = true; break;
                            case "FALSE": data.Job.bContinueWrk = false; break;
                        }
                        data.Job.pio = words[6].ToEnum<ePIOTYPE>();
                        break;
                    }
                case eJOBTYPE.STANDBY: case eJOBTYPE.CAHRGE:
                    data.Job.goal = goalinfo.Get(eJOBTYPE.CAHRGE == data.Job.type ? eGOALTYPE.Charge : eGOALTYPE.Standby, words[2], eSRCHGOALBY.Map);
                    break;
                default: break;
            }
            return data;
        }

    }

    public class MP_ERR
    {
        public eERRST state { get; set; }
        public eERROR code { get; set; }

        public MP_ERR Get()
        {
            return this;
        }

        public string GetMsg()
        {
            return $"{eCMD4MPLUS.ERROR};{(int)this.state};{(int)this.code};";
        }

        public void Set(eERROR err)
        {
            this.code = err;
            this.state = eERRST.Occur;
        }

        public void Clear(eERROR err)
        {
            this.code = err;
            this.state = eERRST.Clear;
        }

        public void Reset()
        {
            this.code = eERROR.None;
            this.state = eERRST.Clear;
        }
    }    

    public class MP_REMOTE
    {
        public string rcvmsg;
        public eREMOTE_MODE mode;
        public eREMOTE_MODE_REPLY reply;
        public static MP_REMOTE Parse(string input)
        {
            MP_REMOTE data = new MP_REMOTE();
            string[] words = input.ToUpper().Split(';');
            data.rcvmsg = input;
            data.mode = words[1].ToEnum<eREMOTE_MODE>();
            data.reply = eREMOTE_MODE_REPLY.NONE;
            return data;
        }

        public void SetReply()
        {
            switch (mode)
            {                
                case eREMOTE_MODE.RESUME: reply = eREMOTE_MODE_REPLY.RESUMED; break;
                case eREMOTE_MODE.PAUSE: reply = eREMOTE_MODE_REPLY.PAUSED; break;
                case eREMOTE_MODE.ABORT: reply = eREMOTE_MODE_REPLY.ABORTED; break;                    
                case eREMOTE_MODE.CANCEL: reply = eREMOTE_MODE_REPLY.CANCELED; break;
                default: break;
            }
        }

        public string GetMsg()
        {
            return $"{eCMD4MPLUS.REMOTE};{this.reply};";
        }

    }


    public class MP_STATUS
    {
        public string rcvmsg;
        public nPOS_XYR pos { get; set; }
        public eROBOTST state { get; set; }
        public eROBOTMODE mode { get; set; }
        public double DnBatt { get; set; }
        public double UpBatt { get; set; }
        public MP_ERR Err { get; set; }

        public double UpVolt { get; set; }
        public double UpCurr { get; set; }
        public long cost { get; set; }
        public double LdTemp { get; set; }

        public static MP_STATUS Parse(string input)
        {
            MP_STATUS data = new MP_STATUS();
            string[] words = input.ToUpper().Split(';');
            if (0 <= words[0].IndexOf("RESP"))
            {
                data.rcvmsg = input;
                data.pos = new nPOS_XYR() { x = Convert.ToInt32(words[2]), y = Convert.ToInt32(words[3]), r = Convert.ToInt32(words[4]) };
                data.state = words[5].ToEnum<eROBOTST>();
                data.mode = words[6].ToEnum<eROBOTMODE>();
                data.DnBatt = Convert.ToDouble(words[7]);
                data.UpBatt = Convert.ToDouble(words[8]);
                data.UpVolt = Convert.ToDouble(words[9]);
                data.UpCurr = Convert.ToDouble(words[10]);
                data.LdTemp = Convert.ToDouble(words[11]);
            }
            else
            {
                data.rcvmsg = input;
                data.pos = new nPOS_XYR() { x = Convert.ToInt32(words[2]), y = Convert.ToInt32(words[3]), r = Convert.ToInt32(words[4]) };
                data.state = words[5].ToEnum<eROBOTST>();
                data.mode = words[6].ToEnum<eROBOTMODE>();
                data.DnBatt = Convert.ToDouble(words[7]);
                data.UpBatt = Convert.ToDouble(words[8]);
                data.UpVolt = Convert.ToDouble(words[9]);
                data.UpCurr = Convert.ToDouble(words[10]);
                data.LdTemp = Convert.ToDouble(words[11]);
            }
            return data;
        }

        public static MP_STATUS Get(STATUS status, eERROR err = eERROR.None)
        {
            var mode = eROBOTMODE.MANUAL;
            switch (status.eqpState)
            {
                case eEQPSATUS.Stop:
                case eEQPSATUS.Stopping: mode = eROBOTMODE.MANUAL; break;
                case eEQPSATUS.Idle:
                case eEQPSATUS.Run: mode = eROBOTMODE.AUTO; break;
                case eEQPSATUS.Error:
                case eEQPSATUS.EMG: mode = eROBOTMODE.ERROR; break;
                default: mode = eROBOTMODE.PM; break;
            }
            var rtn = new MP_STATUS()
            {
                 pos = new nPOS_XYR() { x = status.vecState.pos.x, y = status.vecState.pos.y, r = status.vecState.pos.r }
               , mode = mode
               , DnBatt = status.vecState.soc
               , state = status.vecState.JobState
               , LdTemp = status.vecState.temp
               , UpBatt = 0, UpVolt = 0, UpCurr = 0, cost = 0
               , Err = new MP_ERR() { code = status.err4mp.code, state = status.err4mp.state }
            };

            switch (err)
            {
                case eERROR.None: rtn.Err.Reset(); break;
                case eERROR.Clear: rtn.Err.Clear(err); break;
                default: rtn.Err.Set(err); break;
            }
            return rtn;
        }

        public string GetMsg()
        {
            var rtn = string.Empty;
            rtn = $"{eCMD4MPLUS.STATUS};";
            rtn += $"{this.pos.x};";
            rtn += $"{this.pos.y};";
            rtn += $"{this.pos.r};";
            rtn += $"{this.state};";
            rtn += $"{this.mode};";
            rtn += $"{this.DnBatt};";
            rtn += $"{this.UpBatt};"; // UR Soc
            rtn += $"{this.UpVolt};"; // UR Voltage
            rtn += $"{this.UpCurr};"; // UR Curr
            rtn += $"{this.cost};"; // cost
            rtn += $"0.0;"; // elgen temp
            rtn += $"0.0;"; // ur controller temp
            rtn += $"0.0;"; // elgen box temp
            rtn += $"{this.LdTemp};"; // down temp
            return rtn;
        }
    }

    public class MP_JOBST
    {
        public string rcvmsg;
        public string cmd;
        public string cmdID { get; set; }
        public eJOBST state { get; set; }
        public static MP_JOBST Parse(string input)
        {
            //RESP;JOB;OPER_015843306_1;ASSIGN;
            MP_JOBST data = new MP_JOBST();
            string[] words = input.ToUpper().Split(';');
            if (0 <= words[0].IndexOf("RESP"))
            {
                data.rcvmsg = input;
                data.cmd = words[1];
                data.cmdID = words[2];
                data.state = words[3].ToEnum<eJOBST>();
            }
            else
            {
                data.rcvmsg = input;
                data.cmd = words[0];
                data.cmdID = words[1];
                data.state = words[2].ToEnum<eJOBST>();
            }
            return data;
        }

        public void Set(STATUS status)
        {
            this.cmdID = status.Order.cmdID;
            this.state = status.Order.state;       
        }

        public string GetMsg(JOB jobSt)
        {
            return (null != jobSt.cmdID) ? $"{eCMD4MPLUS.JOB};{jobSt.cmdID};{jobSt.state};" : $"{eCMD4MPLUS.JOB};NULL;{jobSt.state};";
        }

        public string GetMsg()
        {
            return $"{eCMD4MPLUS.JOB};{cmdID};{state};";
        }
    }

    public class MP_DISTBTW
    {
        public string rcvmsg;
        public string cmd;
        public string goal1;
        public string goal2;
        public double result;

        public static MP_DISTBTW Parse(string input)
        {
            MP_DISTBTW data = new MP_DISTBTW();
            string[] words = input.ToUpper().Split(';');
            data.rcvmsg = input;
            data.cmd = words[0];
            data.goal1 = words[1];
            data.goal2 = words[2];
            data.result = 0;
            return data;
        }

        public void Set(STATUS status, double cost)
        {
            this.goal1 = status.mnlOrdr4mp.goal.hostName;
            this.goal2 = status.mnlOrdr4mp.goal.name;
            this.result = cost;            
        }

        public string GetMsg()
        {
            return $"{eCMD4MPLUS.DISTANCEBTW};{this.goal1};{this.goal2};{this.result};";
        }
    }

    public class MNL_CARRIRE_CHANGE
    {
        public eMNL_INST cmd;
        public int nPartID;
        public string nSize;
        public string CarrierID;

        public static MNL_CARRIRE_CHANGE Parse(string input)
        {
            MNL_CARRIRE_CHANGE data = new MNL_CARRIRE_CHANGE();
            string[] words = input.ToUpper().Split(';');
            //RESP;MANUAL_MAG_INST;1/A;CARR_1234;
            //RESP;MANUAL_MAG_UNNST;1;
            if (0 <= words[0].IndexOf("RESP"))
            {
                if (0 <= words[1].LastIndexOf("MANUAL_MAG_INST")) data.cmd = eMNL_INST.INSTALL;
                else if (0 <= words[1].LastIndexOf("MANUAL_MAG_UNNST")) data.cmd = eMNL_INST.UNINSTALL;
                else data.cmd = eMNL_INST.NONE;
                switch (data.cmd)
                {
                    case eMNL_INST.INSTALL:
                        data.nPartID = Convert.ToInt32(words[2].Substring(0, 1));
                        data.nSize = words[2].Substring(1, 1);
                        data.CarrierID = words[3];
                        break;
                    case eMNL_INST.UNINSTALL: data.nPartID = Convert.ToInt32(words[2]); break;
                }
            }
            else
            {
                if (0 <= words[0].LastIndexOf("MANUAL_MAG_INST")) data.cmd = eMNL_INST.INSTALL;
                else if (0 <= words[0].LastIndexOf("MANUAL_MAG_UNNST")) data.cmd = eMNL_INST.UNINSTALL;
                else data.cmd = eMNL_INST.NONE;
                switch (data.cmd)
                {
                    case eMNL_INST.INSTALL:
                        data.nPartID = Convert.ToInt32(words[1].Substring(0, 1));
                        data.nSize = words[1].Substring(1, 1);
                        data.CarrierID = words[2];
                        break;
                    case eMNL_INST.UNINSTALL: data.nPartID = Convert.ToInt32(words[1]); break;
                }
            }
            return data;
        }

        public void Set(STATUS status, eMNL_INST cmd)
        {
            this.cmd = cmd;
            this.nPartID = status.mnlOrdr4mp.partidx;
            this.nSize = status.mnlOrdr4mp.size;
            this.CarrierID = status.mnlOrdr4mp.materialID;
        }

        public string GetMsg()
        {
            var rtn = string.Empty;
            switch (this.cmd)
            {
                case eMNL_INST.INSTALL: rtn = $"{eCMD4MPLUS.MANUAL_MAG_INST};{this.nPartID};{this.nSize};{this.CarrierID};"; break;
                case eMNL_INST.UNINSTALL: rtn = $"{eCMD4MPLUS.MANUAL_MAG_UNINST};{this.nPartID};"; break;
            }
            return rtn;
        }
    }

    public class MP_GOAL
    {
        public eJOBTYPE cmd { get; set; } = eJOBTYPE.NONE;
        public List<GOALITEM> lstGoals { get; set; } = new List<GOALITEM>();
        public static MP_GOAL Parse(string input)
        {
            MP_GOAL data = new MP_GOAL();
            string[] words = input.ToUpper().Split(';');
            int cnt = Convert.ToInt32(words[1]);            
            switch (words[0])
            {
                case "GOAL_LD": data.cmd = eJOBTYPE.LOADING; break;
                case "GOAL_UL": data.cmd = eJOBTYPE.UNLOADING; break;
            }
            data.lstGoals.Clear();
            for (int i = 0; i < cnt; i++)
            {
                var goal = new GOALITEM()
                {
                      name = words[2 + i]
                    , hostName = $"{words[2 + i]}"
                    , label = $"{words[2 + i]}"
                    , line = eLINE._23
                    , mcType = ePIOTYPE.NOUSE
                    , type = eJOBTYPE.LOADING == data.cmd ? eGOALTYPE.Dropoff : eGOALTYPE.Pickup
                };
                data.lstGoals.Add(goal);
            }            
            return data;
        }

        public string GetMsg(eJOBTYPE type)
        {
            var rtn = string.Empty;
            switch (type)
            {                
                case eJOBTYPE.LOADING: rtn = $"GOAL_LD;"; break;
                case eJOBTYPE.UNLOADING: rtn = $"GOAL_UL;"; break;                
                default: break;
            }
            return rtn;
        }

        public string GetMsg()
        {
            var rtn = string.Empty;
            switch (cmd)
            {
                case eJOBTYPE.LOADING: rtn = $"GOAL_LD;"; break;
                case eJOBTYPE.UNLOADING: rtn = $"GOAL_UL;"; break;
                default: break;
            }
            return rtn;
        }
    }


    public class QUE4MP
    {
        public eCMD4MPLUS cmd;
        public string msg;
        // Received massage form mplus
        public MP_JOB order;
        public MP_REMOTE remote;

        // send massage to mplus
        public MP_STATUS _status;
        public MP_JOBST _jobState;
        public MP_DISTBTW _distbtw;
        public MP_ERR _err;
        public MNL_CARRIRE_CHANGE _trayChg;
        public MP_GOAL _askGoal;

        public QUE4MP()
        {
            cmd = eCMD4MPLUS.NONE;
            msg = string.Empty;            
        }

        public eCMD4MPLUS GetCmd(string input)
        {
            eCMD4MPLUS rtn = eCMD4MPLUS.NONE;
            char tok = ';';
            var split = input.Split(tok);
            rtn = split[0].ToEnum<eCMD4MPLUS>();
            return rtn;
        }

        public void SetJob(string input, GOALINFO goals, ref JOB job)
        {
            this.order = MP_JOB.Parse(input, goals);
            job.Set(this.order);
        }

        public void SetStatus(STATUS status)
        {
            _status = MP_STATUS.Get(status);
        }

        public void SetErr(STATUS status, eERROR err)
        {
            _status = MP_STATUS.Get(status, err);
            _err = new MP_ERR() { code = _status.Err.code, state = _status.Err.state };            
        }

        public void SetJobState(STATUS status)
        {
            _jobState = new MP_JOBST();
            _jobState.Set(status);
        }        
    }

    public class STATUS
    {
        public bool bDebug { get; set; } = false;
        public bool bSimul { get; set; } = false;
        public string swVer { get; set; } = string.Empty;
        public eEQPSATUS eqpState { get; set; } = eEQPSATUS.Init;
        public USER user { get; set; } = new USER();
        public bool bManual { get; set; } = false;
        public bool bLoaded { get; set; } = false;
        public string prcssMem { get; set; } = string.Empty;
        public TASKARG DevTsksCycle { get; set; } = new TASKARG(eSEQLIST.MAX_SEQ);
        public TASKARG HdrTskCycle { get; set; } = new TASKARG(eSEQLIST.MAX_SEQ);
        public bool bIsManual { get; set; } = false;
        public CONNECSION devsCont { get; set; } = new CONNECSION();
        public VEHICLESTATE vecState { get; set; } = new VEHICLESTATE();
        public JOB Order { get; set; } = new JOB();
        public JOB mnlOrdr4mp { get; set; } = new JOB(); // M+에게 통신하기 위한 데이터버퍼
        public string lastDest { get; set; } = string.Empty;
        public MP_ERR err4mp { get; set; } = new MP_ERR();  
        public double dResult { get; set; } = 0;

        public STATUS()
        {

        }

        public static bool Save(STATUS st)
        {
            try
            {
                string path = $"Data\\Status.json";
                var saveData = JsonConvert.SerializeObject(st);
                File.WriteAllText(path, saveData);
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
                return false;
            }
            return true;
        }

        public static bool Load(out STATUS st)
        {
            try
            {
                string path = $"Data\\Status.json";
                var loadData = File.ReadAllText(path);
                var temp = JsonConvert.DeserializeObject<STATUS>(loadData);
                st = temp;
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
                st = new STATUS();
                return false;
            }
            return true;
        }

        public void User_Set(USER id, bool IsLogOut = false)
        {
            if (false == IsLogOut)
            {
                user.id = id.id;
                user.grade = id.grade;
            }
            else
            {
                user.id = string.Empty;
                user.grade = eOPRGRADE.Operator;
            }
        }

        public eOPRGRADE _UserGrade => user.grade;
    }
}
