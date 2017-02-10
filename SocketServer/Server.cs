using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTcpServer
{
    /// <summary>
    /// 服务端
    /// </summary>
    class Server
    {
        private static byte[] buff = new byte[1024];
        private const string serverip = "127.0.0.1";
        private const int serverport = 8090;
        static void Main(string[] args)
        {
            IPAddress serverIP = IPAddress.Parse(serverip);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverIPEndPoint = new IPEndPoint(serverIP,serverport);

            serverSocket.Bind(serverIPEndPoint);
            serverSocket.Listen(1);
            Console.WriteLine("服务端启动监听："+serverIPEndPoint.ToString());

            //启动监听客户端的线程
            Thread th = new Thread(ListenClientConnect);
            th.Start(serverSocket);

        }

        /// <summary>
        /// 接受请求
        /// </summary>
        private static void ListenClientConnect(Object socket)
        {
            while (true)
            {
                var serverSocket = (Socket)socket;
                Socket newSocket = serverSocket.Accept();

                //接收数据
                Thread th = new Thread(ReceiveMessage);
                th.Start(newSocket);

                //发送数据
                Thread th1 = new Thread(SendMessage);
                th1.Start(newSocket);

            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="socket"></param>
        private static void ReceiveMessage(Object socket)
        {
            var newSocket = (Socket)socket;

            while(true)
            {
                try
                {
                    if(!newSocket.Connected)
                    {
                        throw new Exception("连接已断开");
                    }
                    if (newSocket.Available > 0)
                    {
                        buff = new byte[1024];
                        newSocket.Receive(buff);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\r\n客户端"+newSocket.RemoteEndPoint+"：" + Encoding.UTF8.GetString(buff).TrimEnd('\0'));
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                   


                }
                catch(Exception e)
                {
                    newSocket.Shutdown(SocketShutdown.Both);
                    newSocket.Close();
                    Console.WriteLine("\r\n接收客户端消息时异常："+e.Message);
                    break;
                }

            }
        }

        private static void SendMessage(Object socket)
        {
            var newSocket = (Socket)socket;

            while (true)
            {
                var input = Console.ReadLine();
                Console.WriteLine("我：" + input);                
                newSocket.Send(Encoding.UTF8.GetBytes(input));

            }
        }
    }
}
