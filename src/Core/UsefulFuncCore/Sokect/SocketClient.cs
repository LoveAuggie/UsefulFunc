using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Auggie.Lib.Sokect
{
    public delegate void SocketClientRecvDelegate(string msg);

    /// <summary>
    /// Socket客户端
    /// </summary>
    public class SocketClient
    {
        /// <summary>
        /// 连接服务端的socket
        /// </summary>
        private Socket client;

        private string _IP;
        private int _Port;
        private bool isStoped;

        public SocketClientRecvDelegate ReceiveDataAct { get; set; }
        /// <summary>
        /// 创建客户端
        /// </summary>
        /// <param name="ip">连接服务端ip</param>
        /// <param name="port">连接服务端端口</param>
        public SocketClient(string ip, int port)
        {
            _IP = ip;
            _Port = port;
        }

        public void StartConnect()
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(_IP, _Port);

                Thread clientThread = new Thread(new ParameterizedThreadStart(ReceiveData));
                object o = client;
                clientThread.Start(o);
            }
            catch (Exception ex)
            {
                throw new Exception("连接失败：" + ex.Message);
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

                    ReceiveDataAct?.Invoke(value);
                }
                catch (Exception ex)
                {
                    if (client == null || !client.Connected)
                    {
                        return;
                    }
                }
            }
        }

        public bool SendMessage(string msg)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(msg);
                client.Send(data);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void StopConnect()
        {
            try
            {
                isStoped = true;
                client.Close();
            }
            catch (Exception)
            {

            }
        }
    }
}
