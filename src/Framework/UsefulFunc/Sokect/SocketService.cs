using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auggie.Lib.Sokect
{
    public delegate void ReceiveClientData(Socket client, string msg);
    public delegate void SocketLog(string msg);
    /// <summary>
    /// Socket服务端
    /// </summary>
    public class SocketService
    {
        #region Private parameter
        private string _IP;
        private int _Port;
        private Socket socketServer = null;
        private Thread acceptClientThread = null;
        private bool isStoped = false;
        #endregion

        #region Public parameter
        /// <summary>
        /// 接收消息用委托
        /// </summary>
        public ReceiveClientData ReceiveDataAct { get; set; }
        /// <summary>
        /// Debug日志输出委托
        /// </summary>
        public SocketLog LogAct { get; set; }
        #endregion

        /// <summary>
        /// 创建服务端
        /// </summary>
        /// <param name="ip">监听IP</param>
        /// <param name="port">client</param>
        public SocketService(string ip, int port)
        {
            _IP = ip;
            _Port = port;
        }

        public void Start()
        {
            try
            {
                //创建终结点
                IPAddress ip = IPAddress.Parse(_IP);
                IPEndPoint ipe = new IPEndPoint(ip, _Port);
                //创建Socket并开始监听
                //创建一个Socket对象，如果用UDP协议，则要用SocketType.Dgram类型的套接字
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
                socketServer.Bind(ipe);    //绑定EndPoint对象
                socketServer.Listen(0);    //开始监听
                //为新建立的连接创建新的Socket
                acceptClientThread = new Thread(new ThreadStart(AcceptClient));
                acceptClientThread.Start();
                log(string.Format("SocketServer 启动成功 => {0}:{1}", _IP, _Port));
            }
            catch (Exception exp)
            {
                log("创建socket失败：" + exp.Message);
            }
        }

        private void AcceptClient()
        {
            try
            {
                while (!isStoped)
                {
                    Socket client = socketServer.Accept();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(ReceiveData));
                    object o = client;
                    clientThread.Start(o);
                }
            }
            catch (Exception exp)
            {
                log("客户端连接错误：" + exp.Message);
            }
        }

        private void ReceiveData(object obj)
        {
            while (!isStoped)
            {
                Socket client = (Socket)obj;
                try
                {
                    byte[] recvBytes = new byte[4096];
                    int bytes;
                    bytes = client.Receive(recvBytes, recvBytes.Length, 0); //从客户端接受消息

                    string value = Encoding.UTF8.GetString(recvBytes, 0, bytes).Trim();
                    if (string.IsNullOrEmpty(value))
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }
                    log($"收到消息=>{value}");

                    ReceiveDataAct?.Invoke(client, value);
                }
                catch (Exception ex)
                {
                    if (client == null || !client.Connected)
                    {
                        log($"连接断开！");
                        return;
                    }
                }
            }
        }

        public bool SendMessageToClient(Socket client, string message)
        {
            try
            {
                byte[] b = Encoding.UTF8.GetBytes(message);
                return client.Send(b, b.Length, 0) > 0;
            }
            catch (Exception ex)
            {
                log("推送消息失败：" + ex.Message);
                return false;
            }
        }

        private void log(string msg)
        {
            if (LogAct != null)
                LogAct.Invoke(msg);
            else
            {
                var cMsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + msg;
                Console.WriteLine(cMsg);
             }
        }

        public void Stop()
        {
            isStoped = true;
            this.socketServer.Close();
        }
    }
}
