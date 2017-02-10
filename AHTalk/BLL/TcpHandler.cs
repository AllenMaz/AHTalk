using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AHTalk.BLL
{
    /// <summary>
    /// 系统配置文件
    /// 单例模式
    /// </summary>
    public class TcpHandler
    {
        private const string _serverHostName = "server.natappfree.cc";
        private const int _serverPort = 33201;
        //private const string _serverHostName = "localhost";
        //private const int _serverPort = 8090;

        // 定义一个静态变量来保存类的实例
        private static ClientInstance _clientInstance;
       
        // 定义一个标识确保线程同步
        private static readonly object locker = new object();

        // 定义私有构造函数，使外界不能创建该类实例
        private TcpHandler()
        {
            
        }

        /// <summary>
        /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
        /// </summary>
        /// <returns></returns>
        public static ClientInstance GetInstance()
        {
            if (_clientInstance != null && !_clientInstance.client.Connected) _clientInstance = null;
            // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
            // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
            // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
            // 双重锁定只需要一句判断就可以了
            if (_clientInstance == null)
            {
                lock (locker)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (_clientInstance == null )
                    {
                        try
                        {
                            //创建TCP连接
                            var tcpClient = new TcpClient(_serverHostName, _serverPort);
                            var bw = new BinaryWriter(tcpClient.GetStream());
                            var br = new BinaryReader(tcpClient.GetStream());

                            _clientInstance = new ClientInstance();
                            _clientInstance.client = tcpClient;
                            _clientInstance.bw = bw;
                            _clientInstance.br = br;
                        }
                        catch(Exception e)
                        {
                            throw new Exception("连接服务器失败："+e.Message);
                        }
                        
                        
                    }
                }
            }
            return _clientInstance;
        }



    }

    public class ClientInstance
    {
        public TcpClient client;
        public BinaryReader br;
        public BinaryWriter bw;

        
        //发送消息
        public void SendMessage(string msg)
        {
            try
            {
                bw.Write(msg);
                bw.Flush();

            }
            catch (Exception e)
            {
                throw new Exception("发送消息失败:" + e.Message);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void CloseConnect()
        {
            if (client != null)
            {
                br.Close();
                bw.Close();
                client.Close();
                
            }
        }
    }
}
