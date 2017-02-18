using AHTalk.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Utility;

namespace AHTalk
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientInstance clientInstance;
        public MainWindow()
        {
            InitializeComponent();

        }

        /// <summary>
        /// 登录连接到服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            //获取服务器IP
            var serverIP = serverIPTextBox.Text;
            if(string.IsNullOrEmpty(serverIP)){
                MessageBox.Show("请填写正确的服务器地址");
                return;
            }
            try
            {
                var serverIPs = serverIP.Split(':');
                TcpHandler._serverHostName = serverIPs[0]; ToString();
                TcpHandler._serverPort = System.Convert.ToInt32(serverIPs[1]);
            }catch(Exception ex){
                MessageBox.Show("服务器地址格式不正确");
                return;
            }

            var userName = userNameTextBox.Text;
            if(string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("请输入用户名");
                return;
            }

            
            try
            {
                clientInstance = TcpHandler.GetInstance();
                var sendMsg = TcpHelper.PackCommmond(userName, TcpHelper.TalkCommond.Login);
                //发送登录命令
                clientInstance.SendMessage(sendMsg);

                //创建新线程接收服务端消息
                Thread th = new Thread(ReceiveData);
                th.Start();

            }catch(Exception ex){
                MessageBox.Show(ex.Message);
            }
            
        }

        /// <summary>
        /// 接收服务端返回消息
        /// </summary>
        private void ReceiveData()
        {
            while(true)
            {
                try
                {
                    var getMsg = clientInstance.br.ReadString();

                    var unPackMsg = TcpHelper.UnPackCommond(getMsg);
                    var commond = unPackMsg.Item1;
                    switch (commond)
                    {
                        case TcpHelper.TalkCommond.Login:

                            var loginMsg = unPackMsg.Item2.Split(',');
                            if(loginMsg[0].ToLower()=="success")
                            {
                               
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>  
                                {
                                    //登录成功
                                    OpenWindow(new MainScreenWindow(loginMsg[1]));
                                    this.Close();
                                }));
                                return;
                            }
                            else
                            {
                                MessageBox.Show(loginMsg[1]);

                            }
                           

                            break;
                        default:
                            MessageBox.Show("登录失败：" + getMsg);
                            break;
                    }

                }
                catch(Exception e)
                {
                    //MessageBox.Show("接收消息失败:"+e.Message);
                    //关闭连接
                    clientInstance.CloseConnect();
                    return;
                }


            }
        }

        private void OpenWindow(Window window)
        {
            //弹出显示在父窗口中间
            //window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //window.Owner = this;

            //弹出弹出后必须先关闭弹出才能操作父窗口
            //window.ShowDialog();

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = System.Windows.SystemParameters.PrimaryScreenWidth - window.Width;
            window.Top = 20;
            
            window.Show();
        }

        
        private void HideWindow(Window window)
        {
            window.Hide();
        }

        private void MouseKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                loginButton_Click(sender,e);
            }
        }
    }
}
