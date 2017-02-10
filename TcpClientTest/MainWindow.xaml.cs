using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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
using Utility;

namespace TcpClientTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //private const string _serverHostName = "server.natappfree.cc";
        //private const int _serverPort = 43834;
        private const string _serverHostName = "localhost";
        private const int _serverPort = 8090;

        private TcpClient _tcpClient;
        private BinaryWriter _bw;
        private BinaryReader _br;

        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void joinButton_Click(object sender, RoutedEventArgs e)
        {
            var clientName = nameTextBox.Text;
            if (string.IsNullOrEmpty(clientName))
            {
                MessageBox.Show("请输入用户名");
                return;
            }

            try
            {
                _tcpClient = new TcpClient(_serverHostName, _serverPort);
                EnableJoinButton(false);
                _bw = new BinaryWriter(_tcpClient.GetStream());
                _br = new BinaryReader(_tcpClient.GetStream());

                var sendMsg = TcpHelper.PackCommmond(clientName, TcpHelper.TalkCommond.Login);
                //发送登录命令
                SendMessage(sendMsg);
            }
            catch(Exception ep)
            {
                EnableJoinButton(true);
                ShowMessage("连接服务器失败："+ep.Message);
            }
            
            
        }

        private void EnableJoinButton(bool enable)
        {
            joinButton.IsEnabled = enable;
        }

        private void SendMessage(string msg)
        {
            try
            {
                _bw.Write(msg);
                _bw.Flush();

            }catch(Exception e)
            {
                ShowMessage("发送消息失败:"+e.Message);
            }
        }

        private delegate void AddMessageToTextBlockEventHandler(string msg);
        private void AddMessageToTextBlock(string msg)
        {

            showMsgTextBlock.Text += DateTime.Now+"："+msg+"\r\n";
        }

        private void ShowMessage(string msg)
        {
            AddMessageToTextBlockEventHandler dm = AddMessageToTextBlock;
            showMsgTextBlock.Dispatcher.Invoke(dm, msg);

        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(_tcpClient != null)
            {
                var sendMsg = TcpHelper.PackCommmond(string.Empty, TcpHelper.TalkCommond.Logout);
                SendMessage(sendMsg);
                _br.Close();
                _bw.Close();
                _tcpClient.Close();
            }
        }

       

    }

}
