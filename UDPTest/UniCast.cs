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
    /// 单播
    /// </summary>
    public class UniCast
    {
        private static UdpClient _client;
        private static string _remoteIp;
        private static int _remotePort;
        static IPAddress _remoteIPA;
        static IPEndPoint _remoteEndPort;


        public static void Test()
        {
            //本机IP
            _client = new UdpClient(_remotePort);
            Console.WriteLine("当前IP:Port为" + _client.Client.LocalEndPoint.ToString());

            Console.WriteLine("请输入远程IP:端口号");
            bool rightInput = false;
            while (!rightInput)
            {
                var input = Console.ReadLine();
                try
                {
                    var ipandport = input.Split(':');
                    _remoteIp = ipandport[0].ToString();
                    _remotePort = System.Convert.ToInt32(ipandport[1]);
                    _remoteIPA = IPAddress.Parse(_remoteIp);
                    _remoteEndPort = new IPEndPoint(_remoteIPA, _remotePort);
                    rightInput = true;
                }
                catch
                {
                    Console.WriteLine("输入格式错误，请重新输入");
                    rightInput = false;
                }
            }

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
                    Console.WriteLine("发送消息（to" + _remoteEndPort.ToString() + "）");

                    var input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {

                        var sendByte = Encoding.Default.GetBytes(input);

                        //发送到特定目标主机
                        var sendByteNum = _client.Send(sendByte, sendByte.Length, _remoteEndPort);
                        

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(_client.Client.LocalEndPoint.ToString() + "：Num:" + sendByteNum + "    Content:" + input);
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
            while (true)
            {
                try
                {

                    byte[] receiveByte = _client.Receive(ref _remoteEndPort);

                    var receiveData = Encoding.Default.GetString(receiveByte);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(_remoteEndPort.ToString() + "：" + receiveData);
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
