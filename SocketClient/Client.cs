using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTcpClient
{
    /// <summary>
    /// 客户端
    /// </summary>
    class Client
    {
        private static byte[] buff = new byte[1024];
        private const string serverip = "110.80.31.162";
        private const int serverport = 43933;

        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            IPAddress serverIPAddress = IPAddress.Parse(serverip);
            IPEndPoint serverIPEndPoint = new IPEndPoint(Dns.GetHostEntry("server.natappfree.cc").AddressList[0], serverport);

            try
            {
                socket.Connect(serverIPEndPoint);
                Console.WriteLine("服务器连接成功。");
                socket.Send(Encoding.UTF8.GetBytes("客户端已上线"));

            }catch(Exception e){
                Console.WriteLine("服务器连接失败："+e.Message);

            }

           
            //接收消息
            Thread th = new Thread(ReceiveMessage);
            th.Start(socket);

            //发送数据
            Thread th1 = new Thread(SendMessage);
            th1.Start(socket);

        }

        private static void ReceiveMessage(object socket)
        {
            while(true)
            {

                var newSocket = (Socket)socket;
                try
                {
                    if (!newSocket.Connected)
                    {
                        throw new Exception("连接已断开");
                    }
                    if(newSocket.Available >0)
                    {
                        buff = new byte[1024];
                        ((Socket)socket).Receive(buff);
                        var getMessage = Encoding.UTF8.GetString(buff);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("服务端" + newSocket.RemoteEndPoint + "：" + getMessage.TrimEnd('\0'));
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    


                }
                catch (Exception e)
                {
                    newSocket.Shutdown(SocketShutdown.Both);
                    newSocket.Close();
                    Console.WriteLine(e.Message);
                    break;
                }
            }
            
        }

        private static void SendMessage(object socket)
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
