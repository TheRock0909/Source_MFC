using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Timers;
using System.Net.NetworkInformation;

namespace Source_MFC.HW.MobileRobot.LD
{
    public class ChangeConnectedArgs : EventArgs
    {
        public bool connected;
    }
    class AsyncClintSock
    {
        public class AsyncObject
        {
            public byte[] Buffer;
            public Socket WorkingSocket;
            public AsyncObject(int bufferSize)
            {
                Buffer = new byte[bufferSize];
            }
        }

        public delegate void RecvStrEvent(object sender, string rcvStr);
        public event RecvStrEvent OnRcvMsg;
        public delegate void RecvDataEvent(object sender, byte[] rcvStr);
        public event RecvDataEvent OnRcvData;
        public event EventHandler<ChangeConnectedArgs> OnChangeConnected;

        private bool g_Connected;
        private Socket m_ClientSocket = null;
        private AsyncCallback m_fnReceiveHandler;
        private AsyncCallback m_fnSendHandler;
        private string HostName;
        private ushort HostPort;

        private System.Timers.Timer tmrConnect = new System.Timers.Timer();

        public AsyncClintSock()
        {
            // 비동기 작업에 사용될 대리자를 초기화합니다.
            m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
            m_fnSendHandler = new AsyncCallback(handleDataSend);
        }


        public bool Connected
        {
            get
            {
                return g_Connected;
            }
        }

        public void ConnectToServer(string hostName, ushort hostPort)
        {
            HostName = hostName;
            HostPort = hostPort;
            // TCP 통신을 위한 소켓을 생성합니다.
            m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            tmrConnect.Enabled = true;
            bool isConnected = false;
            try
            {
                // 연결 시도                
                bool success = m_ClientSocket.BeginConnect(hostName, Convert.ToInt32(hostPort), null, null).AsyncWaitHandle.WaitOne(1000, true);
                m_ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                m_ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

                if (success)
                {
                    // 연결 성공
                    isConnected = true;
                }
            }
            catch
            {
                // 연결 실패 (연결 도중 오류가 발생함)
                isConnected = false;                
            }
            g_Connected = isConnected;

            if (isConnected)
            {
                try
                {
                    // 4096 바이트의 크기를 갖는 바이트 배열을 가진 AsyncObject 클래스 생성
                    AsyncObject ao = new AsyncObject(4096)
                    {
                        // 작업 중인 소켓을 저장하기 위해 sockClient 할당
                        WorkingSocket = m_ClientSocket
                    };

                    // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
                    m_ClientSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
                    OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = true });
                }
                catch (Exception e)
                {
                    OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                    StopClient();
                }
            }
            else
            {
                OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                StopClient();
            }
        }

        public void StopClient()
        {
            // 가차없이 클라이언트 소켓을 닫습니다.
            if (m_ClientSocket != null)
            {
                tmrConnect.Enabled = false;
                if (m_ClientSocket.Connected)
                {
                    m_ClientSocket.Disconnect(false);
                }
                m_ClientSocket.Close();
                g_Connected = false;
                m_ClientSocket.Dispose();
                m_ClientSocket = null;
                //Console.WriteLine($"{HostName}:{HostPort} 클라이언트소켓 삭제");
            }
        }

        public void SendMessage(string message)
        {
            // 추가 정보를 넘기기 위한 변수 선언
            // 크기를 설정하는게 의미가 없습니다.
            // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
            // 최소한의 크기르 배열을 초기화합니다.
            AsyncObject ao = new AsyncObject(1)
            {
                // 문자열을 바이트 배열으로 변환
                //ao.Buffer = Encoding.Unicode.GetBytes(message);
                Buffer = Encoding.UTF8.GetBytes(message),
                WorkingSocket = m_ClientSocket
            };

            // 전송 시작!
            try
            {
                m_ClientSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);
            }
            catch (Exception ex)
            {
                //Logger.Inst.Write(CmdLogType.Err, $"전송 중 오류 발생 : {message} / {ex.Message}");
                //Console.WriteLine($"전송 중 오류 발생!\n메세지: {ex.Message}");
                StopClient();
                g_Connected = false;
                OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
            }
        }


        public void SendData(byte[] dataArray)
        {
            // 추가 정보를 넘기기 위한 변수 선언
            // 크기를 설정하는게 의미가 없습니다.
            // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
            // 최소한의 크기르 배열을 초기화합니다.
            AsyncObject ao = new AsyncObject(1)
            {
                Buffer = dataArray,
                WorkingSocket = m_ClientSocket
            };

            // 전송 시작!
            try
            {
                m_ClientSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"전송 중 오류 발생!\n메세지: {ex.Message}");
                StopClient();
                g_Connected = false;
                OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
            }
        }

        private void handleDataReceive(IAsyncResult ar)
        {
            // 넘겨진 추가 정보를 가져옵니다.
            // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요합니다~!
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 받은 바이트 수 저장할 변수 선언
            Int32 recvBytes;

            try
            {
                // 자료를 수신하고, 수신받은 바이트를 가져옵니다.
                recvBytes = ao.WorkingSocket.EndReceive(ar);
            }
            catch
            {
                // 예외가 발생하면 함수 종료!
                return;
            }

            // 수신받은 자료의 크기가 1 이상일 때에만 자료 처리
            if (recvBytes > 0)
            {
                // 공백 문자들이 많이 발생할 수 있으므로, 받은 바이트 수 만큼 배열을 선언하고 복사한다.
                byte[] msgByte = new byte[recvBytes];
                Array.Copy(ao.Buffer, msgByte, recvBytes);
                //Console.WriteLine($"클라이언트 소켓 수신 : {Encoding.ASCII.GetString(msgByte)}");

                OnRcvData?.Invoke(this, msgByte);
                OnRcvMsg?.Invoke(this, Encoding.ASCII.GetString(msgByte));
            }

            try
            {
                // 자료 처리가 끝났으면~
                // 이제 다시 데이터를 수신받기 위해서 수신 대기를 해야 합니다.
                // Begin~~ 메서드를 이용해 비동기적으로 작업을 대기했다면
                // 반드시 대리자 함수에서 End~~ 메서드를 이용해 비동기 작업이 끝났다고 알려줘야 합니다!
                ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                //Console.WriteLine($"자료 수신 대기 도중 오류 발생! 메세지: {ex.Message}");
                g_Connected = false;
                return;
            }
        }

        private void handleDataSend(IAsyncResult ar)
        {

            // 넘겨진 추가 정보를 가져옵니다.
            AsyncObject ao = (AsyncObject)ar.AsyncState;

            // 보낸 바이트 수를 저장할 변수 선언
            int sentBytes;

            try
            {
                // 자료를 전송하고, 전송한 바이트를 가져옵니다.
                sentBytes = ao.WorkingSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
                //Console.WriteLine($"자료 송신 도중 오류 발생! 메세지: {ex.Message}");
                g_Connected = false;
                return;
            }

            if (sentBytes > 0)
            {
                // 여기도 마찬가지로 보낸 바이트 수 만큼 배열 선언 후 복사한다.
                byte[] msgByte = new byte[sentBytes];
                Array.Copy(ao.Buffer, msgByte, sentBytes);

                //Debug.WriteLine("메세지 보냄: {0}", Encoding.ASCII.GetString(msgByte));
            }
        }

        private void tmrConnect_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (g_Connected)
                {
                    IPEndPoint ip = m_ClientSocket.RemoteEndPoint as IPEndPoint;
                    IPAddress who = ip.Address;
                    int timeout = 1000;
                    string data = "aaaa";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);
                    var reply = new Ping().Send(who, timeout, buffer, new PingOptions(64, true));
                    if (reply.Status != IPStatus.Success)
                    {
                        g_Connected = false;
                        OnChangeConnected?.Invoke(this, new ChangeConnectedArgs() { connected = false });
                    }
                }
            }
            catch
            {

            }

        }
    }
}




