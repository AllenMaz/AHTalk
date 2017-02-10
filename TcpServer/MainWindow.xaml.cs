using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TcpServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<User> _user = new List<User>();
        TcpListener _tcpListener;
        private const int _serverPort = 8090;
        IPAddress _serverIPAddress = Dns.GetHostAddresses(Dns.GetHostName())
            .Where(v=>v.AddressFamily == AddressFamily.InterNetwork).First();

        public MainWindow()
        {
            InitializeComponent();
            stopListenButton.IsEnabled = false;
        }


       
        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startListenButton_Click(object sender, RoutedEventArgs e)
        {
            

            _tcpListener = new TcpListener(_serverIPAddress,_serverPort);
            _tcpListener.Start();
            AddMessage("\r\nTCP服务端已启动监听："+_serverIPAddress+":"+_serverPort);
            
            //创建一个线程监听客户端
            Thread th = new Thread(ListenClientConnect);
            th.Start();

            startListenButton.IsEnabled = false;
            stopListenButton.IsEnabled = true;
        }

        /// <summary>
        /// 监听客户端
        /// </summary>
        private void ListenClientConnect()
        {
            while(true)
            {
                try
                {
                    TcpClient newClient = _tcpListener.AcceptTcpClient();

                    var user = new User(newClient);
                    _user.Add(user);
                    //每个客户端都创建一个新的线程处理
                    Thread th = new Thread(ClientHandler);
                    th.Start(user);


                }catch(Exception e)
                {
                    break;
                }
            }
        }

        private void ClientHandler(object user)
        {
            var newUser = (User)user;
            AddMessage("\r\n用户" + newUser._tcpClient.Client.RemoteEndPoint + "已登录");
            AddMessage("当前用户："+_user.Count());

            while(true)
            {
                var getMsg = newUser._binaryReader.ReadString();
                AddMessage("\r\n"+getMsg.TrimEnd('\0'));
            }


        }

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopListenButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var user in _user)
            {
                RemoveUser(user);
            }
            _tcpListener.Stop();
            startListenButton.IsEnabled = true;
            stopListenButton.IsEnabled = false;
            AddMessage("\r\nTCP服务器已停止监听！");
        }

        private void RemoveUser(User user)
        {
            _user.Remove(user);
            user.Close();
            AddMessage("\r\n用户："+user.UserName+"退出登录");
            AddMessage("当前用户：" + _user.Count());

        }

        private delegate void AddMessageToTextBlockEventHandler(string msg);
        private void AddMessageToTextBlock(string msg)
        {
            serverTextBlock.Text += msg; 
        }

        private void AddMessage(string msg)
        {
            AddMessageToTextBlockEventHandler dm = AddMessageToTextBlock;
            serverTextBlock.Dispatcher.Invoke(dm,msg);

        }
    }


    /// <summary>
    /// 客户端用户类
    /// </summary>
    public class User
    {
        public TcpClient _tcpClient;
        public BinaryReader _binaryReader;
        public BinaryWriter _binaryWrite;
        public string UserName;

        public User(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _binaryReader = new BinaryReader(_tcpClient.GetStream());
            _binaryWrite = new BinaryWriter(_tcpClient.GetStream());
        }

        public void Close(){

            _binaryWrite.Close();
            _binaryReader.Close();
            _tcpClient.Close();
        }
    }
}
