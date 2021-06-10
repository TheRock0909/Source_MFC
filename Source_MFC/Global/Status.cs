using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
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
        public EzVehicle.Ctrl.eSTATE state { get; set; } = EzVehicle.Ctrl.eSTATE.NONE;
        public nPOS_XYR pos { get; set; } = new nPOS_XYR() ;
        public RobotState JobState { get; set; } = RobotState.NotAssigned;
        public VEHICLESTATE()
        {
            soc = 0;
            state = EzVehicle.Ctrl.eSTATE.NONE;
            pos = new nPOS_XYR();
        }

        public void Job_Reset()
        {
            JobState = RobotState.NotAssigned;
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
        public CommandState state { get; set; } = CommandState.None;
        public string materialID { get; set; } = string.Empty;
        public int partidx { get; set; } = 0;
        public bool bContinueWrk { get; set; } = false;

        public void Init()
        {
            this.type = eJOBTYPE.NONE;
            this.cmdID = string.Empty;
            this.goal = new GOALITEM();
            this.opt = new JOBOPT();
            this.state = CommandState.None;
            this.materialID = string.Empty;
            this.partidx = 0;
            this.bContinueWrk = false;
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
        public string lastDest { get; set; } = string.Empty;
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
