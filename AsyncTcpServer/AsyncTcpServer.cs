using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTcpServer
{
    class AsyncTcpServer
    {
        private const string _serverIP = "127.0.0.1";
        private const int _port = 9980;
        static IPAddress _serverIPA = IPAddress.Parse(_serverIP);

        static void Main(string[] args)
        {
            TcpListener tcpListener = new TcpListener(_serverIPA, _port);
            tcpListener.Start();

            Thread th = new Thread(ListenAndConnectEventHandler);
            th.IsBackground = true; //后台线程
            th.Start(tcpListener);


            Console.ReadLine();

        }

        public static void ListenAndConnectEventHandler(object obj)
        {
            Console.WriteLine("BeginAcceptTcpClient之前");

            TcpListener tcpListener = (TcpListener)obj;
            tcpListener.BeginAcceptTcpClient(TcpAcceptAsyncCallBackEventHandler, tcpListener);

            Console.WriteLine("BeginAcceptTcpClient之后");

        }

        public static void TcpAcceptAsyncCallBackEventHandler(IAsyncResult ar)
        {
            Console.WriteLine("进入回调");
            TcpListener tcpListener = (TcpListener)ar.AsyncState;
            var tcpClient = tcpListener.EndAcceptTcpClient(ar);

            //新建线程接收数据
            Thread th = new Thread(ReceiveData);
            th.Start(tcpClient);
        }

        public static void ReceiveData(object obj)
        {
            TcpClient client = (TcpClient)obj;
            BinaryReader br = new BinaryReader(client.GetStream());
            while(true){
                var msg = br.ReadString();
                Console.WriteLine(msg);
            }
        }
    }
}
