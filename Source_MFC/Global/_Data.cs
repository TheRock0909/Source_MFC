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

    public class _Data
    {
        private static volatile _Data _inst;
        private static object syncRoot = new Object();

        public STATUS status = null;
        public SYS sys = null;

        public bool bMakeBak = false;
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
                            Trace
                                .Write($"path --------------------------------");

                            bool rtn = false;
                            rtn = _inst.StatusLoad();
                            if(rtn == false)
                            {
                                foreach (eJsonName idx in Enum.GetValues(typeof(eJsonName)))
                                {
                                    STATUS.LoadParam(_inst.status, eBackUpType.Bak, idx);
                                    STATUS.SaveParam(_inst.status, eBackUpType.Default, idx);
                                    STATUS.LoadParam(_inst.status, eBackUpType.Default, idx);
                                }
                            }
                            rtn = _inst.SysLoad();
                            if(rtn == false)
                            {
                                foreach (eJsonName idx in Enum.GetValues(typeof(eJsonName)))
                                {
                                    SYS.LoadParam(_inst.sys, eBackUpType.Bak, idx);
                                    SYS.SaveParam(_inst.sys, eBackUpType.Default, idx);
                                    SYS.LoadParam(_inst.sys, eBackUpType.Default, idx);
                                }
                            }
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

            // UI SAVE 버튼 누를 시 백업파일 경로로 같이 저장
            foreach (eJsonName idx in Enum.GetValues(typeof(eJsonName)))
            {
                STATUS.SaveParam(_inst.status, eBackUpType.Bak, idx);
            }
        }
    }
}
