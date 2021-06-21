using Source_MFC.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Utils
{

    public class BackUpLogger
    {

        private static volatile BackUpLogger instance;
        private static object syncRoot = new object();

        public static BackUpLogger Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new BackUpLogger();
                    }
                }
                return instance;
            }
        }
        private BackUpLogger()
        {

        }

        string _BackFodler = string.Format($"Data\\BackUpData", Environment.CurrentDirectory);

        private void MakeBackDir()
        {
            DateTime Date = DateTime.Now;
            string strY, strM, strD;
            DirectoryInfo dtif = new DirectoryInfo(_BackFodler);
            strY = string.Format("{0:yyyy}", Date);
            strM = string.Format("{0:MM}", Date);
            strD = string.Format("{0:dd}", Date);

            dtif = new DirectoryInfo($"{_BackFodler}\\{strY}");
            if (!dtif.Exists)
            {
                dtif.Create();
            }
            dtif = new DirectoryInfo($"{_BackFodler}\\{strY}\\{strM}");
            if (!dtif.Exists)
            {
                dtif.Create();
            }
            dtif = new DirectoryInfo($"{_BackFodler}\\{strY}\\{strM}\\{strD}");
            if (!dtif.Exists)
            {
                dtif.Create();
            }
        }

        private string BackUpDataPath(eBackUpType type)
        {
            string rtn = string.Empty;
            DateTime Date = DateTime.Now;
            string strY, strM, strD;
            DirectoryInfo dtif = new DirectoryInfo(_BackFodler);
            strY = string.Format("{0:yyyy}", Date);
            strM = string.Format("{0:MM}", Date);
            strD = string.Format("{0:dd}", Date);

            switch (type)
            {
                case eBackUpType.Default:
                    rtn = "Data";
                    break;
                case eBackUpType.Bak:
                    rtn = $"{_BackFodler}";
                    break;
                case eBackUpType.DateBak:
                    rtn = $"{_BackFodler}\\{strY}\\{strM}\\{strD}";
                    break;
                default:
                    break;
            }
            return rtn;
        }

        public string RoutejsonData(eBackUpType type, eJsonName name)
        {
            MakeBackDir();
            string FullPathBak = string.Empty;
            switch (type)
            {
                case eBackUpType.Default:
                    string pathdefault = BackUpDataPath(eBackUpType.Default);
                    if (name == eJsonName.IO)
                    {
                        FullPathBak = $"{pathdefault}\\{name}_{_Data.Inst.sys.cfg.fac.eqpType}_{_Data.Inst.sys.cfg.fac.language}.json";
                    }
                    else
                    {
                        FullPathBak = $"{pathdefault}\\{name}.json";
                    }
                    break;
                case eBackUpType.Bak:
                    string pathbak = BackUpDataPath(eBackUpType.Bak);
                    if(name == eJsonName.IO)
                    {
                        FullPathBak = $"{pathbak}\\{name}_{_Data.Inst.sys.cfg.fac.eqpType}_{_Data.Inst.sys.cfg.fac.language}_bak.json";
                    }
                    else
                    {
                        FullPathBak = $"{pathbak}\\{name}_bak.json";
                    }
                    break;
                case eBackUpType.DateBak:
                    string pathdatebak = BackUpDataPath(eBackUpType.DateBak);
                    if (name == eJsonName.IO)
                    {
                        FullPathBak = $"{pathdatebak}\\{name}_{_Data.Inst.sys.cfg.fac.eqpType}_{_Data.Inst.sys.cfg.fac.language}.json";
                    }
                    else
                    {
                        FullPathBak = $"{pathdatebak}\\{name}.json";
                    }
                    break;
                default:
                    break;
            }
            return FullPathBak;
        }

    }
}
