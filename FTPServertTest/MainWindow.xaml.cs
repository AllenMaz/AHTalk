using System;
using System.Collections.Generic;
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

namespace FTPServertTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //FTP控制连接端口
        private const int _ftpControlPort = 21;
        //FTP数据连接端口(PORT模式)
        private const int _ftpDataPort = 20;
        private TcpListener _tcpListener;
        
        public MainWindow()
        {
            InitializeComponent();

            

            
        }

        /// <summary>
        /// 启动FTP服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startFTPButton_Click(object sender, RoutedEventArgs e)
        {
            _tcpListener = new TcpListener(IPAddress.Any, _ftpControlPort);
            _tcpListener.Start();

            AddMessage("启动监听：" + _tcpListener.LocalEndpoint.ToString());

            //新建线程，用于监听客户端的请求
            Thread th = new Thread(ListenAndConnect);
            th.IsBackground = true;
            th.Start();


            startFTPButton.IsEnabled = false;
            stopFTPButton.IsEnabled = true;
        }


        private void ListenAndConnect()
        {
            while (true)
            {

                try
                {
                    var client = _tcpListener.AcceptTcpClient();
                    ClientUser user = new ClientUser(client);

                    //为每个新的客户端创建单独的线程处理
                    Thread th = new Thread(ClientHandler);
                    th.IsBackground = true;
                    th.Start(user);

                }
                catch (Exception e)
                {
                    AddMessage("FTP连接异常：" + e.Message);
                }
            }

        }

        private void ClientHandler(object obj)
        {
            var user = (ClientUser)obj;
            while (true)
            {
                var getMsg = user.br.ReadString();
                AddMessage(getMsg);
            }

        }

        private void stopFTPButton_Click(object sender, RoutedEventArgs e)
        {
            Thread.ResetAbort();
            _tcpListener.Stop();
            startFTPButton.IsEnabled = true;
            stopFTPButton.IsEnabled = false;
        }
     
        private delegate void AddMessageEventHandler(string msg);
        private void AddMessage(string msg)
        {
            msg = msg + "\r\n";
            infoListBox.Dispatcher.Invoke((Action)(() => {
                infoListBox.Items.Add(msg);
            }));
        }

       
    }
}
