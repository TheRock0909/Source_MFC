using Newtonsoft.Json;
using Source_MFC.HW.MobileRobot.LD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Global
{
    public class nPOS_XY
    {
        public int x;
        public int y;
        public nPOS_XY()
        {
            x = 0; y = 0;
        }
    }

    public class nPOS_XYZ
    {
        public int x;
        public int y;
        public int z;
        public nPOS_XYZ()
        {
            x = 0; y = 0; z = 0;
        }
    }

    public class nPOS_XYR
    {
        public int x;
        public int y;
        public int r;
        public nPOS_XYR()
        {
            x = 0; y = 0; r = 0;
        }
    }

    public class dPOS_XY
    {
        public double x;
        public double y;
        public dPOS_XY()
        {
            x = 0; y = 0;
        }
    }

    public class dPOS_XYZ
    {
        public double x;
        public double y;
        public double z;
        public dPOS_XYZ()
        {
            x = 0; y = 0; z = 0;
        }
    }

    public class dPOS_XYR
    {
        public double x;
        public double y;
        public double r;
        public dPOS_XYR()
        {
            x = 0; y = 0; r = 0;
        }
    }

    public class JCVT_VEC
    {
        public string typeName { get; set; }
        public string json { get; set; }
        public static JCVT_VEC Set<T>(T data)
        {
            var obj = new JCVT_VEC();
            obj.typeName = $"{typeof(T).Name}";
            obj.json = JsonConvert.SerializeObject(data, Formatting.Indented);
            return obj;
        }

        public static void Get(string data, out SENDARG arg)
        {
            arg = JsonConvert.DeserializeObject<SENDARG>(data);
        }

        public static void Get(string data, out LD_STATUS arg)
        {
            arg = JsonConvert.DeserializeObject<LD_STATUS>(data);
        }
        public static void Get(string data, out LOG arg)
        {
            arg = JsonConvert.DeserializeObject<LOG>(data);
        }
    }

    public class LOG
    {
        public bool bSend { get; set; }
        public string msg { get; set; }
        public string Get()
        {
            return $"{GetTime()} : {(true == bSend ? "S" : "R")} = {msg}";
        }
        private string GetTime(bool bNeed2MilSec = true)
        {
            DateTime NowTime = DateTime.Now;
            if (bNeed2MilSec) return NowTime.ToString("HH:mm:ss.") + NowTime.Millisecond.ToString("000");
            else return NowTime.ToString("HH_mm_ss");
        }
    }

    public enum eVEHICLETYPE
    {
          LD
        , MIR
    }

    public enum eVEC_CMD
    {
          None = -1
        , Stop
        , Say
        , PauseCancel
        , Go2Goal
        , Go2Point
        , Go2Straight
        , Dock
        , Undock
        , MoveDeltaHeading
        , MoveFront
        , GetDistBetween
        , GetDistFromHere
        , LocalizeAtGoal
    }


    public class SENDARG
    {
        public string goal_1st { get; set; }
        public string goal_2nd { get; set; }
        public nPOS_XYR pos { get; set; }
        public string msg { get; set; }
        public int move { get; set; }
        public int spd { get; set; }
        public int acc { get; set; }
        public int dec { get; set; }
        public SENDARG()
        {
            goal_1st = goal_2nd = msg = string.Empty; pos = new nPOS_XYR();
            move = 0; spd = 30; acc = 10; dec = 10;
        }

        public void CopyFrom(SENDARG arg)
        {
            goal_1st = arg.goal_1st;
            goal_2nd = arg.goal_2nd;
            pos = new nPOS_XYR() { x = arg.pos.x, y = arg.pos.y, r = arg.pos.r } ;
            msg = arg.msg;
            move = arg.move;
            spd = arg.spd;
            acc = arg.acc;
            dec = arg.dec;
        }
    }

    public class _Data
    {
        private static volatile _Data _inst;
        private static object syncRoot = new Object();

        public STATUS status = null;
        public SYS sys = null;

        private _Data()
        {
            status = new STATUS();
            sys = new SYS();
        }

        ~_Data()
        {

        }

        public static _Data Inst
        {
            get 
            {
                if (_inst == null)
                {
                    lock (syncRoot)
                    {
                        if (_inst == null)
                        {
                            _inst = new _Data();                            
                            // 기본 폴더 자동생성
                            var path = string.Format($"Data", Environment.CurrentDirectory);
                            var dtif = new DirectoryInfo(path);
                            if (!dtif.Exists)
                            {
                                dtif.Create();
                            }
                            Trace.Write($"path --------------------------------");

                            bool rtn = false;
                            rtn = _inst.StatusLoad();
                            rtn = _inst.SysLoad();
                        }
                    }
                }
                return _inst;
            }
        }

        public bool StatusLoad()
        {
            bool rtn = false;
            // 설비 상태파일 로딩
            rtn = STATUS.Load(out _inst.status);            
            return rtn;
        }

        public bool SysLoad()
        {
            bool rtn = false;            
            // 설비 데이터 로딩
            rtn = SYS.Load(out _inst.sys);
            return rtn;
        }

        public void StatusSave()
        {
            STATUS.Save(_inst.status);
        }

        public void Save()
        {
            STATUS.Save(_inst.status);
            SYS.Save(_inst.sys);
        }
    }
}
