using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTcpClient
{
    class AsyncTcpClient
    {
        private const string _serverIP = "127.0.0.1";
        private const int _port = 9980;
        static IPAddress _serverIPA = IPAddress.Parse(_serverIP);
        static BinaryWriter _bw;

        static void Main(string[] args)
        {
            Console.WriteLine("BeginConnect之前");

            TcpClient tcpClient = new TcpClient();
            tcpClient.BeginConnect(_serverIPA, _port, TcpClientAsyncCallBackEventHandler,tcpClient);

            Console.WriteLine("BeginConnect之后");
            Console.ReadLine();
        }

        private static void TcpClientAsyncCallBackEventHandler(IAsyncResult ar)
        {
            Console.WriteLine("进入回调......");
            TcpClient tcpClient = (TcpClient)ar.AsyncState;
            tcpClient.EndConnect(ar);

            
            _bw = new BinaryWriter(tcpClient.GetStream());
            var msg ="你好服务器，我是客户端";
            //同步发送消息
            //_bw.Write("你好服务器，我是客户端");
            //_bw.Flush();
            SendMessageEventHandler sm = SendMessage;
            sm.BeginInvoke(msg, SendMessageAsyncCallBack, sm);
            Console.WriteLine("消息发送完毕");
        }


        private static void SendMessageAsyncCallBack(IAsyncResult ar)
        {
            Console.WriteLine("发送完消息后回调");
            var sm = (SendMessageEventHandler)ar.AsyncState;
            sm.EndInvoke(ar);
            Console.WriteLine("发送完消息后结束回调");
        }

        private delegate void SendMessageEventHandler(string msg);
        private static void SendMessage(string msg)
        {
            try
            {
                _bw.Write(msg);
                _bw.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("发送消息异常："+e.Message);
            }
        }
    }
}
