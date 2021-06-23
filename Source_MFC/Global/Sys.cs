using Newtonsoft.Json;
using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Global
{
    public class USER
    {
        public eOPRGRADE grade { get; set; }
        public string id { get; set; }
        public string password { get; set; }
        public string msg { get; set; }
        public USER()
        {
            grade = eOPRGRADE.Operator;
            id = password = msg = string.Empty;
        }
    }

    public class USERINFO
    {
        public List<USER> src = new List<USER>();
        public USERINFO()
        {

        }

        public USER LogIn(string id, string password)
        {
            USER user = new USER();
            if (string.Empty != id && string.Empty != password)
            {
                var rtn = src.Any(s => s.id == id && s.password == password);
                if (true == rtn)
                {
                    user = src.Single(s => s.id == id && s.password == password);
                    user.msg = string.Empty;
                }
                else
                {
                    if (src.Any(s => s.id == id))
                    {
                        user.msg = $"The password is wrong.";
                    }
                    else
                    {
                        user.msg = $"{id} is does not exist.";
                    }
                }
            }
            else
            {
                user.msg = $"Please fill in the id and password\r\n[ID:{id}, Passworrd:{password}]\r\nFinally The Rock has come back to Home!!!!";
            }
            return user;
        }

        public bool Add(USER info)
        {
            if (info.id == "Username" && info.password == "Password")
            {
                info.id = string.Empty;
                info.password = string.Empty;
            }
            bool rtn = false;
            if (string.Empty != info.password)
            {
                if (false == src.Where(s => s.id == info.id).Any())
                {
                    rtn = true;
                    src.Add(new USER() { id = info.id, grade = info.grade, password = info.password, msg = string.Empty });
                    info.msg = $"{info.id} Add complete";
                }
                else
                {
                    info.msg = $"{info.id} is already exist.";
                }
            }
            else
            {
                info.msg = $"Please Insert the password.";
            }
            return rtn;
        }

        public bool Remove(USER info)
        {
            bool rtn = false;
            if (false == src.Where(s => s.id == info.id).Any())
            {
                rtn = src.Remove(info);
            }
            else
            {
                info.msg = $"{info.id} is not found.";
            }
            return rtn;
        }

        public static bool Save(USERINFO st)
        {
            try
            {
                string path = $"Data\\User.json";
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

        public static bool Load(out USERINFO st)
        {
            try
            {
                string path = $"Data\\User.json";
                var loadData = File.ReadAllText(path);
                var temp = JsonConvert.DeserializeObject<USERINFO>(loadData);
                st = temp;
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
                st = new USERINFO();
                return false;
            }
            return true;
        }

        ~USERINFO()
        {
            src.Clear();
            src = null;
        }
    }

    public class IOSRC
    {
        public eIOTYPE Type { get; set; }
        public eSENTYPE SenType { get; set; }
        public string name4Enum { get; set; }
        public int eID { get; set; }        //  도면상 ID        
        public int RealID { get; set; }     // 실제 물리적으로 구성된 ID                
        public string Label { get; set; }
        public bool state { get; set; }
        public bool getOutput { get; set; }
        public IOSRC()
        {
            Type = eIOTYPE.INPUT;
            SenType = eSENTYPE.A;
            eID = 0;
            RealID = 0;
            Label = name4Enum = string.Empty;
            state = getOutput = false;
        }

    }
    public class IOINFO
    {
        private int nMaxPortNo = 0 ;
        public int _MaxPort { get { return nMaxPortNo; } set { nMaxPortNo = value; } }
        public long _CurrInputState { get; set; } = 0;
        public long _CurrGetOutputState { get; set; } = 0;
        private bool bDirectIO = false;
        public bool _bDirectIO { get { return bDirectIO; } set { bDirectIO = value; } }
        public List<IOSRC> lst;
        public IOINFO()
        {
            _bDirectIO = false;
            lst = new List<IOSRC>();
            lst.Clear();
        }

        public bool Add(IOSRC info)
        {
            bool rtn = false;
            if (false == lst.Where(s => s.eID == info.eID && s.Type == info.Type).Any())
            {
                rtn = true;
                lst.Add(new IOSRC() { eID = info.eID, Type = info.Type, RealID = info.RealID, SenType = info.SenType, Label = info.Label });
            }
            return rtn;
        }

        public (int chNo, int bitNo) GetInfo(int id)
        {
            return ((int)id / nMaxPortNo, (int)id % nMaxPortNo);
        }

        public IOSRC Get(eINPUT id)
        {
            var src = lst.SingleOrDefault(s => s.name4Enum == id.ToString() && s.Type == eIOTYPE.INPUT);
            if (null != src)
            {
                return src;
            }
            else
            {
                return new IOSRC();
            }
        }

        public IOSRC Get(string label)
        {
            var src = lst.SingleOrDefault(s => s.Label == label && s.Type == eIOTYPE.INPUT);
            if (null != src)
            {
                return src;
            }
            else
            {
                return new IOSRC();
            }
        }

        public IOSRC Get(eOUTPUT id)
        {
            var src = lst.SingleOrDefault(s => s.name4Enum == id.ToString() && s.Type == eIOTYPE.OUTPUT);
            if (null != src)
            {
                return src;
            }
            else
            {
                return new IOSRC();
            }
        }

        public void WriteStateAll(long inputs, long getOutputs)
        {
            int idx = 0;
            Any64 utemp = new Any64();
            utemp.INT64 = inputs;            
            var lst_in = this.lst.Where(s => s.Type == eIOTYPE.INPUT).ToList();
            foreach (var item in lst_in)
            {
                var src = lst_in.SingleOrDefault(i => i.RealID == idx);
                if ( null != src )
                {
                    src.state = utemp[idx];
                }
            }
            this._CurrInputState = utemp.INT64;

            idx = 0;
            utemp = new Any64();
            utemp.INT64 = getOutputs;
            var lst_out = this.lst.Where(s => s.Type == eIOTYPE.OUTPUT).ToList();
            foreach (var item in lst_out)
            {
                var src = lst_out.SingleOrDefault(i => i.RealID == idx);
                if (null != src)
                {
                    src.getOutput = utemp[idx];
                }
            }
            this._CurrGetOutputState = utemp.INT64;
        }

        public long WriteOutputsAll()
        {
            int idx = 0;
            var utemp = new Any64();
            var lst_out = this.lst.Where(s => s.Type == eIOTYPE.OUTPUT).ToList();
            foreach (var item in lst_out)
            {
                var src = lst_out.SingleOrDefault(i => i.RealID == idx);
                utemp[idx] = src.state;
            }
            return utemp.INT64;
        }

        public static bool Save( IOINFO st, eEQPTYPE type, eLANGUAGE lan = eLANGUAGE.kor)
        {
            try
            {
                RemoveVirtualIO(st);
                string path = $"Data\\IO_{type}_{lan}.json";
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

        public static bool Load(out IOINFO st, eEQPTYPE type, eLANGUAGE lan = eLANGUAGE.kor)
        {
            bool rtn = true;
            try
            {
                string path = $"Data\\IO_{type}_{lan}.json";
                var loadData = File.ReadAllText(path);                
                var temp = JsonConvert.DeserializeObject<IOINFO>(loadData);               
                st = temp;
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
                st = new IOINFO();
                for (int i = 0; i < 16; i++)
                {
                    st.Add(new IOSRC() { Type = eIOTYPE.INPUT, eID = i, SenType = eSENTYPE.A, name4Enum = string.Empty, Label = $"INPUT-#{i:00}", state = false, RealID = i });
                    st.Add(new IOSRC() { Type = eIOTYPE.OUTPUT, eID = i, SenType = eSENTYPE.A, name4Enum = string.Empty, Label = $"OUTPUT-#{i:00}", state = false, RealID = i });
                }
                Save(st, type, lan);
                rtn = false;
            }
            MakeVirtualIO(st);
            return rtn;
        }

        private static void MakeVirtualIO(IOINFO info)
        {
            var inAry = new int[4] { (int)eINPUT.VTA_PIO_Valid, (int)eINPUT.VTA_PIO_Ready, (int)eINPUT.VTA_PIO_Completed, (int)eINPUT.VTA_PIO_MC_ERROR };
            foreach (eINPUT item in inAry)
            {
                if (false == info.lst.Any(s => s.name4Enum == item.ToString() && s.Type == eIOTYPE.INPUT))
                {
                    info.lst.Add(new IOSRC() { Type = eIOTYPE.INPUT, eID = (int)item, name4Enum = $"{item}", RealID = (int)item, Label = Ctrls.Remove_(item.ToString()) });
                }
            }

            var outAry = new int[2] { (int)eOUTPUT.VTA_PIO_Ready, (int)eOUTPUT.VTA_PIO_Completed };
            foreach (eOUTPUT item in outAry)
            {
                if (false == info.lst.Any(s => s.name4Enum == item.ToString() && s.Type == eIOTYPE.OUTPUT))
                {
                    info.lst.Add(new IOSRC() { Type = eIOTYPE.OUTPUT, eID = (int)item, name4Enum = $"{item}", RealID = (int)item, Label = Ctrls.Remove_(item.ToString()) });
                }
            }
        }

        private static void RemoveVirtualIO(IOINFO info)
        {
            var inAry = new int[4] { (int)eINPUT.VTA_PIO_Valid, (int)eINPUT.VTA_PIO_Ready, (int)eINPUT.VTA_PIO_Completed, (int)eINPUT.VTA_PIO_MC_ERROR };
            foreach (eINPUT item in inAry)
            {
                var src = info.lst.SingleOrDefault(s => s.name4Enum == item.ToString() && s.Type == eIOTYPE.INPUT);
                if ( null != src )
                {
                    info.lst.Remove(src);
                }
            }

            var outAry = new int[2] { (int)eOUTPUT.VTA_PIO_Ready, (int)eOUTPUT.VTA_PIO_Completed };
            foreach (eOUTPUT item in outAry)
            {
                var src = info.lst.SingleOrDefault(s => s.name4Enum == item.ToString() && s.Type == eIOTYPE.OUTPUT);
                if (null != src)
                {
                    info.lst.Remove(src);
                }
            }
        }
    }

    public class LAMP
    {
        public eEQPSATUS status = eEQPSATUS.Init;
        public TWRLAMP Green { get; set; }
        public TWRLAMP Red { get; set; }
        public TWRLAMP Yellow { get; set; }
        public bool Buzzer { get; set; }

        public LAMP()
        {
            Green = TWRLAMP.OFF;
            Red = TWRLAMP.OFF;
            Yellow = TWRLAMP.OFF;
            Buzzer = false;
        }
    }

    public class LAMPINFO
    {
        public int blinkTime;
        public List<LAMP> lst;
        public LAMPINFO()
        {
            lst = new List<LAMP>();
            lst.Clear();
        }

        public LAMP GetLmp(eEQPSATUS st)
        {
            return (0 < lst.Count) ? lst.Single(s => s.status == st) : new LAMP();
        }

        public static bool Save(LAMPINFO st)
        {
            try
            {
                string path = $"Data\\Lamp.json";
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

        public static bool Load(out LAMPINFO st)
        {
            try
            {
                string path = $"Data\\Lamp.json";
                var loadData = File.ReadAllText(path);
                var temp = JsonConvert.DeserializeObject<LAMPINFO>(loadData);
                st = temp;
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
                st = new LAMPINFO();
                st.lst.Clear();
                foreach (eEQPSATUS col in Enum.GetValues(typeof(eEQPSATUS)))
                {
                    var lmp = new LAMP() { status = col };
                    st.lst.Add(lmp);
                }
                st.blinkTime = 500;
                Save(st);
                return false;
            }
            return true;
        }
    }

    public sealed class GOALITEM
    {
        public eGOALTYPE type { get; set; }
        public string name { get; set; }
        public eLINE line { get; set; }
        public string hostName { get; set; }
        public string label { get; set; }
        public ePIOTYPE mcType { get; set; }
        public nPOS_XYR pos { get; set; }
        public nPOS_XYR escape { get; set; }
        public GOALITEM()
        {
            type = eGOALTYPE.Standby;
            name = string.Empty;
            line = eLINE.None;
            hostName = label = string.Empty;
            mcType = ePIOTYPE.NOUSE;
            pos = new nPOS_XYR();
            escape = new nPOS_XYR();
        }
    }

    public class GOALINFO
    {
        public List<GOALITEM> lst;
        DateTime Date = DateTime.Now;
        
        public GOALINFO()
        {
            lst = new List<GOALITEM>();
            lst.Clear();
        }

        public GOALITEM Get(eGOALTYPE Type, string name, eSRCHGOALBY srchBy = eSRCHGOALBY.Map)
        {
            switch (srchBy)
            {
                case eSRCHGOALBY.Map:
                    if (true == lst.Any(g => g.type == Type && g.name == name))
                    {
                        var goal = lst.SingleOrDefault(g => g.type == Type && g.name == name);
                        return goal;
                    }
                    else
                    {
                        return null;
                    }
                case eSRCHGOALBY.Host:
                    if (true == lst.Any(g => g.type == Type && g.hostName == name))
                    {
                        var goal = lst.SingleOrDefault(g => g.type == Type && g.label == name);
                        return goal;
                    }
                    else
                    {
                        return null;
                    }
                case eSRCHGOALBY.Label:
                    if (true == lst.Any(g => g.type == Type && g.label == name))
                    {
                        var goal = lst.SingleOrDefault(g => g.type == Type && g.label == name);
                        return goal;
                    }
                    else
                    {
                        return null;
                    }
                default: return null;
            }
        }

        public bool Add(GOALITEM item)
        {
            bool rtn = false;
            if (false == lst.Any(g => g.type == item.type && g.name == item.name))
            {
                var goal = lst.SingleOrDefault(g => g.type == item.type && g.name == item.name);
                if (null == goal)
                {
                    lst.Add(item);
                    rtn = true;
                }
            }
            return rtn;
        }

        public bool Remove(GOALITEM item)
        {
            bool rtn = false;
            if (true == lst.Any(g => g.type == item.type && g.name == item.name))
            {
                rtn = true;
                var fndlst = lst.Where(g => g.type == item.type && g.name == item.name).ToList();
                foreach (var g in fndlst)
                {
                    var chk = lst.Remove(g);
                }
            }
            return rtn;
        }

        public List<GOALITEM> GetList(eGOALTYPE type)
        {
            var fndLst = lst.Where(s => s.type == type).ToList();
            return (0 < fndLst.Count) ? fndLst : new List<GOALITEM>();
        }

        public static bool Save(GOALINFO st)
        {
            try
            {
                string path = $"Data\\Goal.json";
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

        public static bool Load(out GOALINFO st)
        {
            try
            {
                string path = $"Data\\Goal.json";
                var loadData = File.ReadAllText(path);
                var temp = JsonConvert.DeserializeObject<GOALINFO>(loadData);
                st = temp;
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");


                st = new GOALINFO();
                foreach (eGOALTYPE item in Enum.GetValues(typeof(eGOALTYPE)))
                {                    
                    foreach (eLINE line in Enum.GetValues(typeof(eLINE)))
                    {
                        if (eLINE.None == line) continue;
                        st.Add(new GOALITEM() { type = item, line = line, name = $"{item}_{line}", label = $"{item}_{line}", hostName = string.Empty });
                    }                    
                }
                Save(st);
                return false;
            }
            if(st == null)
            {
                return false;
            }
            else return true;
        }
    }

    public class PIO
    {
        public int nInterfaceTimeout { get; set; } // 설비와 도킹 후 PIO 시작 대기시간 (스메마신호 On 확인)
        public int nDockSenChkTime { get; set; } // 도킹 후 정위치 센서 확인시간
        public int nFeedTimeOut_Start { get; set; } // 피딩시작 후 입구센서에 트래이 감지 타임아웃
        public int nFeedTimeOut_Work { get; set; } // 피딩시작 후 작업완료 타임아웃
        public int nFeedTimeOut_End { get; set; } // 작업종료 후 PIO 종료 (스메마신호 Off 확인)
        public int nSenDelay { get; set; } // 센서 확인 시 딜레이
        public int nCommTimeout { get; set; } // 통신 타임아웃(LD, Mplus)
        public int nConvSpd_Normal { get; set; }
        public int nConvSpd_Slow { get; set; }
        public PIO()
        {
            nInterfaceTimeout = 0;
            nDockSenChkTime = 0;
            nFeedTimeOut_Start = 0;
            nFeedTimeOut_Work = 0;
            nFeedTimeOut_End = 0;
            nSenDelay = 0;
            nCommTimeout = 0;
            nConvSpd_Normal = 0;
            nConvSpd_Slow = 0;
        }
    }


    public class FAC
    {
        public eEQPTYPE eqpType { get; set; }
        public string eqpName { get; set; }
        public eCUSTOMER customer { get; set; }
        public eSCENARIOMODE seqMode { get; set; }
        public eLANGUAGE language { get; set; }
        public string mplusIP { get; set; }
        public int mplusPort { get; set; }
        public string VecIP { get; set; }        

        public FAC()
        {
            eqpType = eEQPTYPE.NONE;
            eqpName = string.Empty;
            customer = eCUSTOMER.NONE;
            seqMode = eSCENARIOMODE.PC;
            language = eLANGUAGE.kor;
            mplusIP = string.Empty;
            mplusPort = 0;
            VecIP = string.Empty;            
        }
    }

    public class CFG
    {
        public PIO pio;
        public FAC fac;


        public CFG()
        {
            pio = new PIO();
            fac = new FAC();
        }

        public static bool Save(CFG st)
        {
            try
            {
                string path = $"Data\\Cfg.json";
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

        public static bool Load(out CFG st)
        {
            try
            {
                string path = $"Data\\Cfg.json";
                var loadData = File.ReadAllText(path);
                var temp = JsonConvert.DeserializeObject<CFG>(loadData);
                st = temp;
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
                st = new CFG();
                return false;
            }
            if(st == null)
            {
                return false;
            }
            else return true;
        }
    }

    public class SYS
    {
        public CFG cfg;
        public USERINFO user;
        public IOINFO io;
        public GOALINFO goal;
        public LAMPINFO lmp; 
        
        public SYS()
        {
            cfg = new CFG();
            user = new USERINFO();
            io = new IOINFO();
            goal = new GOALINFO();
            lmp = new LAMPINFO();

        }

        public static bool Save(SYS st)
        {
            try
            {
                bool rtn = false;
                rtn = CFG.Save(st.cfg);
                rtn = USERINFO.Save(st.user);
                rtn = IOINFO.Save(st.io, st.cfg.fac.eqpType, st.cfg.fac.language);
                rtn = GOALINFO.Save(st.goal);
                rtn = LAMPINFO.Save(st.lmp);
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
                return false;
            }
            return true;
        }

        public static bool Load(out SYS st)
        {
            st = new SYS();
            try
            {                
                bool rtn = false;
                rtn = CFG.Load(out st.cfg);
                rtn = USERINFO.Load(out st.user);
                rtn = IOINFO.Load(out st.io, st.cfg.fac.eqpType, st.cfg.fac.language );
                rtn = GOALINFO.Load(out st.goal);
                rtn = LAMPINFO.Load(out st.lmp);

                if(rtn == true)
                {
                    foreach (eJsonName idx in Enum.GetValues(typeof(eJsonName)))
                    {
                        SaveParam(st, eBackUpType.Bak, idx);
                        SaveParam(st, eBackUpType.DateBak, idx);
                    }
                }
                
            }
            catch (Exception e)
            {
                Debug.Write(false, $"{e.ToString()}");
                return false;
            }
            return true;
        }

        public static void SaveParam(SYS st ,eBackUpType type, eJsonName name)
        {
            string pathBak = BackUpLogger.Inst.RoutejsonData(type, name);
            switch (name)
            {
                case eJsonName.Cfg:
                    var savecfg = JsonConvert.SerializeObject(st.cfg);
                    File.WriteAllText(pathBak, savecfg);
                    break;
                case eJsonName.Goal:
                    var savegoal = JsonConvert.SerializeObject(st.goal);
                    File.WriteAllText(pathBak, savegoal);
                    break;
                case eJsonName.Status:
                    break;
                case eJsonName.IO:
                    var saveio = JsonConvert.SerializeObject(st.io);
                    File.WriteAllText(pathBak, saveio);
                    break;
                default:
                    break;
            }
        }

        public static void LoadParam(SYS st, eBackUpType type, eJsonName name)
        {
            string pathBak = BackUpLogger.Inst.RoutejsonData(type, name);
            var loadData = File.ReadAllText(pathBak);
            switch (name)
            {
                case eJsonName.Cfg:
                    var tempcfg = JsonConvert.DeserializeObject<CFG>(loadData);
                    st.cfg = tempcfg;
                    break;
                case eJsonName.Goal:
                    var tempgoal = JsonConvert.DeserializeObject<GOALINFO>(loadData);
                    st.goal = tempgoal;
                    break;
                case eJsonName.Status:
                    break;
                case eJsonName.IO:
                    var tempio = JsonConvert.DeserializeObject<IOINFO>(loadData);
                    st.io = tempio;
                    break;
                default:
                    break;
            }
        }
    }

}
