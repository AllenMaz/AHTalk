using PublicUtility;
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
using Utility;

namespace TcpServerTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<User> _loginUserList = new List<User>();
        TcpListener _tcpListener;
        private const int _serverPort = 8090;
        IPAddress _serverIPAddress = Dns.GetHostAddresses(Dns.GetHostName())
            .Where(v => v.AddressFamily == AddressFamily.InterNetwork).First();
        
        
        public MainWindow()
        {
            InitializeComponent();
            stopListenButton.IsEnabled = false;
            _serverIPAddress = IPAddress.Parse("127.0.0.1");

            UpdateUserListShow();
        }



        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startListenButton_Click(object sender, RoutedEventArgs e)
        {


            _tcpListener = new TcpListener(_serverIPAddress, _serverPort);
            _tcpListener.Start();
            AddMessage("TCP服务端已启动监听：" + _serverIPAddress + ":" + _serverPort);

            //创建一个线程监听客户端
            Thread th = new Thread(ListenClientConnect);
            th.IsBackground = true;
            th.Start();

            startListenButton.IsEnabled = false;
            stopListenButton.IsEnabled = true;
        }

        /// <summary>
        /// 监听客户端
        /// </summary>
        private void ListenClientConnect()
        {
            while (true)
            {
                try
                {
                    TcpClient newClient = _tcpListener.AcceptTcpClient();

                    
                    //每个客户端都创建一个新的线程处理
                    Thread th = new Thread(ClientHandler);
                    th.IsBackground = true;
                    th.Start(newClient);


                }
                catch (Exception e)
                {
                    break;
                }
            }
        }

        private void ClientHandler(object client)
        {
            var currentClient = (TcpClient)client;
            var currentUser = new User(currentClient);
            _loginUserList.Add(currentUser);

            while (true)
            {
                string getMsg = string.Empty;
                try
                {
                    getMsg = currentUser._binaryReader.ReadString();

                }catch(Exception e)
                {
                    AddMessage("客户端"+currentUser.UserName+"连接异常,已终止该用户连接。"+e.Message);
                    //连接异常时退出登录
                    RemoveUser(currentUser);
                    break;
                }

                var unPackMsg = TcpHelper.UnPackCommond(getMsg);


                var commond = unPackMsg.Item1;
                switch(commond)
                {
                    case TcpHelper.TalkCommond.Login:
                        //用户登录
                        var userName = unPackMsg.Item2;
                        var login = Login(currentUser,userName);

                        var sendMsg = TcpHelper.PackCommmond(login.Item2, TcpHelper.TalkCommond.Login);
                        SendMessageToClient(currentUser, sendMsg);
                        var isLogin = login.Item1;
                        if (isLogin)
                        {
                            //登录成功
                            //用户登录
                            AddMessage("用户" + userName + "已登录");
                            SetTotalUserCount(_loginUserList.Count());
                            currentUser.UserName = userName;
                        }
                        else
                        {
                            RemoveUser(currentUser);
                            //断开连接
                            currentUser.Close();
                            return;
                        }

                        break;
                    case TcpHelper.TalkCommond.Logout:
                        sendMsg = TcpHelper.PackCommmond(string.Empty, TcpHelper.TalkCommond.Logout);
                        SendMessageToAllClient(sendMsg);
                        RemoveUser(currentUser);
                        return;

                    case TcpHelper.TalkCommond.Talk:
                        userName = currentUser.UserName;
                        var talkMsg = unPackMsg.Item2.Split(',');
                        var talktoUserName = talkMsg[0];
                        var talkContent = unPackMsg.Item2.Substring(talkMsg[0].Length+1,unPackMsg.Item2.Length-talkMsg[0].Length-1);
                        
                        //服务端转发消息到相应客户端
                        var talktoUser = _loginUserList.Where(v => v.UserName == talktoUserName).First();

                        talkContent = TcpHelper.PackCommmond(userName + "," + talkContent, TcpHelper.TalkCommond.Talk);
                        SendMessageToClient(talktoUser,talkContent);

                        break;
                    case TcpHelper.TalkCommond.UpdateUserList:

                        userName = unPackMsg.Item2;
                        //更新用户列表(今后可过滤好友用户)
                        var firendList = GetFirendTalkUserList(userName);

                        var jsonFirendList = JsonHandler.ToJson(firendList);
                        sendMsg = TcpHelper.PackCommmond(jsonFirendList, TcpHelper.TalkCommond.UpdateUserList);
                        SendMessageToClient(currentUser, sendMsg);
                        
                        //将当期用户登录通知每位已上线的好友
                        var onlineFriends = firendList.Where(v=>v.IsOnline).Select(v=>v.UserName).ToList();
                        var onlineUsers = _loginUserList.Where(v => onlineFriends.Contains(v.UserName)).ToList();
                        foreach (var ur in onlineUsers)
                        {
                            //获取好友列表
                            firendList = GetFirendTalkUserList(ur.UserName);
                            jsonFirendList = JsonHandler.ToJson(firendList);
                            sendMsg = TcpHelper.PackCommmond(jsonFirendList, TcpHelper.TalkCommond.UpdateUserList);
                            SendMessageToClient(ur,sendMsg);
                        }

                        break;
                    default:
                        AddMessage("未定义命令："+commond);
                        break;
                }


                
            }


        }

        

        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns></returns>
        private List<TalkUser> GetFirendTalkUserList(string loginUserName)
        {
            List<TalkUser> users = new List<TalkUser>();
            //更新用户列表(今后可过滤好友用户)
            var firendList = UserList.Users.Where(v => v != loginUserName).ToList();
            //在线状态
            foreach(var name in firendList)
            {
                TalkUser user = new TalkUser();
                user.UserName = name;
                user.IsOnline = _loginUserList.Any(v => v.UserName == name);
                users.Add(user);
            }

            return users;

        }

        private Tuple<bool,string> Login(User user,string userName)
        {
            Tuple<bool, string> rt;
            string returnMsg = "success,"+userName;

            if (UserList.Users.All(v => v != userName))
            {
                returnMsg = "false,用户：" + userName + "不存在";
                rt = Tuple.Create(false, returnMsg);
            }
            else
            {
                if (_loginUserList.Any(v => v.UserName == userName))
                {
                    returnMsg = "false,用户：" + userName + "已登录";
                    rt = Tuple.Create(false, returnMsg);
                }
                else
                {
                    rt = Tuple.Create(true, returnMsg);
                }

            }

            return rt;
        }

        /// <summary>
        /// 发送消息到客户端
        /// </summary>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        private void SendMessageToClient(User user,string msg)
        {
            try
            {
                user._binaryWrite.Write(msg);
                user._binaryWrite.Flush();

            }catch(Exception e)
            {
                AddMessage("消息发送失败:"+e.Message);
            }
        }

        /// <summary>
        /// 发送消息到所有客户端
        /// </summary>
        /// <param name="msg"></param>
        private void SendMessageToAllClient(string msg)
        {
            foreach(var user in _loginUserList)
            {
                SendMessageToClient(user,msg);
            }
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopListenButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = _loginUserList.Count - 1; i >= 0;i-- )
            {
                RemoveUser(_loginUserList[i]);
            }
            _tcpListener.Stop();
            startListenButton.IsEnabled = true;
            stopListenButton.IsEnabled = false;
            AddMessage("TCP服务器已停止监听！");
        }

        private void RemoveUser(User user)
        {
            _loginUserList.Remove(user);
            user.Close();
            AddMessage("用户：" + user.UserName + "退出登录");
            SetTotalUserCount(_loginUserList.Count());

        }

        private delegate void AddMessageToTextBlockEventHandler(string msg);
        private void AddMessageToTextBlock(string msg)
        {
            serverTextBlock.Text += DateTime.Now+"："+msg+"\r\n";
        }

        private void AddMessage(string msg)
        {
            AddMessageToTextBlockEventHandler dm = AddMessageToTextBlock;
            serverTextBlock.Dispatcher.Invoke(dm, msg);

        }

        private delegate void AddMessageToLabelEventHandler(int num);
        private void SetTotalUserCountToLable(int num)
        {
            totalUserLable.Content = num.ToString();
        }

        private void SetTotalUserCount(int num)
        {

            AddMessageToLabelEventHandler dm = SetTotalUserCountToLable;
            totalUserLable.Dispatcher.Invoke(dm, num);

        }

        private void addUserButton_Click(object sender, RoutedEventArgs e)
        {
            var user = userTextBox.Text;
            if(!string.IsNullOrEmpty(user)){
                if(UserList.Users.Contains(user)){
                    MessageBox.Show("用户："+user+"已存在");
                }
                else
                {
                    userTextBox.Text = string.Empty;
                    UserList.Users.Add(user);
                    UpdateUserListShow();

                }
            }
            else
            {
                MessageBox.Show("用户名不能为空");
            }
        }

        private void UpdateUserListShow()
        {
            userListBox.Dispatcher.Invoke((Action)(() => {
                userListBox.Items.Clear();

                foreach(var u in UserList.Users){
                    userListBox.Items.Add(u);
                }
            }));
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

        public void Close()
        {

            _binaryWrite.Close();
            _binaryReader.Close();
            _tcpClient.Close();
        }
    }
}
