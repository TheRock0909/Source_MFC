using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Source_MFC.HW.MobileRobot.LD
{
    internal class LD_SOCK : IDisposable
    {
        string _ip = null;
        AsyncClintSock sock = null;
        private ConcurrentQueue<string> recvBuf = new ConcurrentQueue<string>();
        private CancellationTokenSource cancelTock;
        public event EventHandler<bool> Evt_Connection;
        public event EventHandler<string> Evt_RecvdData;
        protected byte STX = 0x02, ETX = 0x03, LF = 0x0A, CR = 0x0D;
        public LD_SOCK()
        {

        }

        public void Dispose()
        {
            if (null != cancelTock)
            {
                cancelTock.Cancel();
            }            
            sock?.StopClient();
            sock = null;
        }
        
        public async Task<bool> Conenct(string ip)
        {
            if (sock != null)
            {                
                sock.OnRcvData -= Sock_DataReceived;
                sock.OnChangeConnected -= Sock_Connected;
                sock.StopClient();
                sock = null;
            }
            
            try
            {
                cancelTock = new CancellationTokenSource();
                sock = new AsyncClintSock();
                sock.OnRcvData += Sock_DataReceived;
                sock.OnChangeConnected += Sock_Connected;                
                if (ip == "0.0.0.0")
                {
                    return false;
                }
                await Task.Run(() => sock.ConnectToServer(ip, (ushort)7171));
                ParsRun();
                return true;
            }
            catch
            {
                sock.StopClient();
                sock = null;
                Debug.Assert(false, $"{ip}:7171 Client 소켓연결 실패.");
                return false;
            }
        }

        
        private void Sock_Connected(object sender, ChangeConnectedArgs e)
        {
            Evt_Connection?.Invoke(this, sock.Connected);
        }

        private bool isUploaded = false;
        private void Sock_DataReceived(object sender, byte[] rcvStr)
        {
            while (isUploaded)
            {
                Thread.Sleep(1);
            }
            isUploaded = true;
            var msg = new string(Encoding.Default.GetChars(rcvStr));
            recvBuf.Enqueue(msg);
            isUploaded = false;
        }
        private void Sock_Disconnected(object sender, EventArgs e)
        {
            Evt_Connection?.Invoke(this, false);
        }

        string _recvStr = string.Empty;
        private async Task ParsRun()
        {
            await Task.Run(async () =>
            {
                var tempmsg = string.Empty;
                var tempBuf = new List<byte>();
                while (!cancelTock.IsCancellationRequested)
                {
                    if (recvBuf.IsEmpty)
                    {
                        await Task.Delay(5);
                    }
                    else if (recvBuf.TryDequeue(out string data))
                    {
                        _recvStr += data;
                        _recvStr = _recvStr.Replace("\0", "");
                        string[] splitRcvStr = _recvStr.Split('\n');
                        if (splitRcvStr.Length > 0)
                        {
                            _recvStr = _recvStr.Substring(_recvStr.LastIndexOf('\n') + 1);
                            foreach (var item in splitRcvStr)
                            {
                                if (item.Length < 1 || item.IndexOf('\r') == -1) continue;
                                var result = item.Replace(System.Environment.NewLine, string.Empty);
                                Evt_RecvdData?.Invoke(this, result);
                            }
                        }                           
                    }
                    await Task.Delay(1);
                }
            });
        }

        public void Send(string msg)
        {
            sock.SendMessage(msg);
        }
    }
}
