using Source_MFC.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzVehicle
{
    public interface IVehicle
    {
        bool _bConected { get; }
        eVEHICLETYPE eType { get; }
        event EventHandler<bool> Evt_Connection;
        event EventHandler<JCVT_VEC> Evt_UpdateData;
        bool Open();
        void Close();
        void Send(eVEC_CMD nTrg, JCVT_VEC data = null);
    }
}
