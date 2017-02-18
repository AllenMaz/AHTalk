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
    /// 组播/多播
    /// 组播地址范围：224.0.0.1 - 239.255.255.254
    /// </summary>
    public class MulitCast
    {
        static UdpClient _client;
        private const int _port = 5000;
        //组播地址
        static IPAddress _ipa = IPAddress.Parse("224.0.0.1");
        static IPEndPoint _endport = new IPEndPoint(_ipa,_port);

        public static void Test()
        {
            _client = new UdpClient(_port);
            //客户端加入广播组
            _client.Ttl = 50;
            _client.JoinMulticastGroup(_ipa);

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
                    var input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {

                        byte[] sendByte = Encoding.Default.GetBytes(input);
                        _client.Send(sendByte, sendByte.Length, _endport);
                        //_client.BeginSend(sendByte, sendByte.Length, _endport, SendAsyncCallBack,_client);
                    }

                }catch(Exception e){
                    Console.WriteLine("消息发送失败："+e.Message);
                }
                
            }
        }

        private static void SendAsyncCallBack(IAsyncResult ar){

            var client = (UdpClient)ar.AsyncState;
            client.EndSend(ar);
            Console.WriteLine("消息发送完毕");
        }

        private static void ReceiveData()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, _port);

            while(true){
                try
                {
                    var getByte = _client.Receive(ref endpoint);
                    Console.WriteLine("接收消息:" + Encoding.Default.GetString(getByte));
                    

                }catch(Exception e){
                    Console.WriteLine("消息接收失败："+e.Message);
                }
            }
        }

        private static void ReceiveAsyncCallBack(IAsyncResult ar)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, _port);

            var client = (UdpClient)ar.AsyncState;
            var getByte = client.EndReceive(ar, ref endpoint);
            Console.WriteLine("接收消息:" + Encoding.Default.GetString(getByte));
        }
           
    }
}
