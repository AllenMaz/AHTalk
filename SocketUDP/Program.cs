using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketUDP
{
    class Program
    {
        private const string selfip = "127.0.0.1";
        private static int selfport;

        static IPEndPoint serverIPE;
        static IPEndPoint selfIPE;

        static void Main(string[] args)
        {
            //随机生成端口号
            Random rdPort = new Random();
            selfport = rdPort.Next(8000, 9000);
            selfIPE = new IPEndPoint(IPAddress.Parse(selfip), selfport);
            Console.WriteLine("\r\n当前客户端为：" + selfIPE.ToString());

            Thread th1 = new Thread(ReceiveMessage);
            th1.Start();
            Thread th = new Thread(SendMessage);
            th.Start();

            
        }

        private static void SendMessage()
        {
            //输入要通讯的客户端ip:端口号
            Console.WriteLine("\r\n输入要通讯的客户端ip:端口号");
            var serverip = Console.ReadLine();
            if (!string.IsNullOrEmpty(serverip))
            {
                var ip = serverip.Split(':')[0].ToString();
                var port = System.Convert.ToInt32(serverip.Split(':')[1]);
                serverIPE = new IPEndPoint(IPAddress.Parse(ip), port);
            }

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            Console.WriteLine("\r\n发送消息：");
            while(true)
            {
                var input = Console.ReadLine();

                socket.SendTo(Encoding.UTF8.GetBytes("\r\n" + input), serverIPE);

            }
        }

        private static void ReceiveMessage()
        {
            
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //IP地址和端口号均由系统自动分配
            IPEndPoint ipeRemote = new IPEndPoint(IPAddress.Any, 0);
            var endpoint = (EndPoint)ipeRemote;
            socket.Bind(selfIPE);
            while(true)
            {
                byte[] buff = new byte[1024];
               
                socket.ReceiveFrom(buff,ref endpoint);
                Console.WriteLine(Encoding.UTF8.GetString(buff).TrimEnd('\0'));
            }
        }
    }
}
