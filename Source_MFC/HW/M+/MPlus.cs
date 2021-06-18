using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechfloorUdp;

namespace Source_MFC.HW.M_
{
    public class MPlus
    {
        private AsyncUdpSock udp;
         private bool connected = false;
        public bool _Connected { set { connected = value; } get => connected; }
        public event EventHandler<bool> Evt_Connection;
        public event EventHandler<(string msg, bool bManual)> Evt_RecvData;
        public MPlus()
        {
            udp = new AsyncUdpSock();
            udp.OnChangedConnected += On_Connection;
            udp.OnRecvStr += On_RcvdMsg;
        }

        private void On_Connection(object sender, bool bConnection)
        {
            Evt_Connection?.Invoke(this, bConnection);
        }

        private void On_RcvdMsg(object sender, string msg)
        {
            Evt_RecvData?.Invoke(this, (msg,false));
        }

        public void Open(string ip, int port)
        {
            udp.Open(port, ip);
        }

        public void Close()
        {
            udp.OnChangedConnected -= On_Connection;
            udp.OnRecvStr -= On_RcvdMsg;
            udp.Dispose();
        }

        public async Task<bool> Send(string msg)
        {
            var rtn = await udp.Send(msg);
            Logger.Inst.Write(CmdLogType.Gem, msg);
            return rtn;
        }
    }
}
