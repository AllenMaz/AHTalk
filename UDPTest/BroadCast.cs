using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPTest
{
    /// <summary>
    /// 广播
    /// </summary>
    public class BroadCast
    {
        private static UdpClient _client;
        private static int port = 5000;

        public static void Test()
        {
            //本机IP
            _client = new UdpClient(port);
            Console.WriteLine("当前IP:Port为" + _client.Client.LocalEndPoint.ToString());


            Thread th = new Thread(SendData);
            th.Start();

            Thread th1 = new Thread(ReceiveData);
            th1.IsBackground = true;
            th1.Start();

        }

        private static void SendData()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("发送消息（to" + _client.Client.LocalEndPoint.ToString() + "）");

                    var input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {

                        var sendByte = Encoding.Default.GetBytes(input);

                        //发送到广播地址
                        IPEndPoint broadcastEndPort = new IPEndPoint(IPAddress.Broadcast, port);
                        var sendByteNum = _client.Send(sendByte, sendByte.Length, broadcastEndPort);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("发送数据：Num:" + sendByteNum + "    Content:" + input);
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("发送消息失败：" + e.Message);
                }


            }

        }
        private static void ReceiveData()
        {
            IPEndPoint remoteEndPort = new IPEndPoint(IPAddress.Any,port);
            while (true)
            {
                try
                {


                    byte[] receiveByte = _client.Receive(ref remoteEndPort);
                    var receiveData = Encoding.Default.GetString(receiveByte);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("接收数据：" + receiveData);
                    Console.ForegroundColor = ConsoleColor.White;

                }
                catch (Exception e)
                {
                    Console.WriteLine("接收消息失败：" + e.Message);

                }
            }
        }
    }
}
