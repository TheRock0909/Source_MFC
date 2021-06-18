using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TechfloorUdp
{
    public class AsyncUdpSock : IDisposable
    {
        private UdpClient udpSock = null;
        private DateTime lastRecvTime;
        private IPEndPoint epFrom;
        private bool isServerMode;
        private string heartBeatServer = "S_HB";
        private string heartBeatClient = "C_HB";
        private readonly string token = "\r\n";
        private string recvBuffer = string.Empty;

        public event EventHandler<string> OnRecvStr;
        public event EventHandler<bool> OnChangedConnected;

        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            private set { isConnected = value; }
        }

        /// <summary>
        /// 소켓을 연다. addr이 공백이면 server mode
        /// </summary>
        /// <param name="port"></param>
        /// <param name="clientAddr"></param>
        public void Open(int port, string clientAddr = "0.0.0.0")
        {
            if (clientAddr == "0.0.0.0")
            {
                isServerMode = true;
            }

            if (isServerMode)
            {
                epFrom = new IPEndPoint(IPAddress.Any, port);
                udpSock = new UdpClient(epFrom);
            }
            else
            {
                epFrom = new IPEndPoint(IPAddress.Parse(clientAddr), port);
                udpSock = new UdpClient();
            }
            RecvLoopStart();
            CheckConnected();
        }
        /// <summary>
        /// return 값이 전송 성공을 의미하지 않는다.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<bool> Send(string msg)
        {
            if (udpSock == null || epFrom == null)
            {
                return false;
            }
            if (!msg.Contains(token))
            {
                msg += token;
            }

            var sendBytes = Encoding.UTF8.GetBytes(msg);
            await udpSock.SendAsync(sendBytes, msg.Length, epFrom);
            return true;
        }

        public void Dispose()
        {
            IsConnected = false;
            epFrom = null;
            udpSock?.Close();
            udpSock = null;
        }


        private async void RecvLoopStart()
        {
            try
            {
                while (udpSock != null)
                {
                    if (udpSock?.Available < 1)
                    {
                        await Task.Delay(10);
                        continue;
                    }
                    var recvBuf = await udpSock.ReceiveAsync();

                    lastRecvTime = DateTime.Now;
                    if (!IsConnected)
                    {
                        IsConnected = true;
                        OnChangedConnected?.Invoke(this, IsConnected);
                    }
                    epFrom = recvBuf.RemoteEndPoint;
                    var recvStr = Encoding.Default.GetString(recvBuf.Buffer);
                    recvBuffer += recvStr;
                    var splitIndex = recvBuffer.IndexOf(token);
                    while (splitIndex >= 0)
                    {
                        var splitedStr = recvBuffer.Substring(0, splitIndex + token.Length);
                        splitedStr = splitedStr.Replace(token, string.Empty);
                        recvBuffer = recvBuffer.Remove(0, splitIndex + token.Length);
                        splitIndex = recvBuffer.IndexOf(token);

                        if (splitedStr == heartBeatClient || splitedStr == heartBeatServer)
                        {
                            Send(heartBeatServer);
                            continue;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(splitedStr))
                            {
                                continue;
                            }
                            OnRecvStr?.Invoke(this, splitedStr);
                        }
                    }
                }
            }
            catch (Exception)
            {
                recvBuffer = string.Empty;
            }
        }

        private async void CheckConnected()
        {
            await Task.Run(() =>
            {
                while (udpSock != null)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (!isServerMode && DateTime.Compare(lastRecvTime.AddSeconds(3), DateTime.Now) < 0)
                    {
                        Send(heartBeatClient);                        
                    }

                    if (IsConnected && DateTime.Compare(lastRecvTime.AddSeconds(5), DateTime.Now) < 0)
                    {
                        IsConnected = false;
                        OnChangedConnected?.Invoke(this, IsConnected);
                    }
                }
            });
        }


    }
}
