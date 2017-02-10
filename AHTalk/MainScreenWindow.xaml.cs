using AHTalk.BLL;
using PublicUtility;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Utility;

namespace AHTalk
{
    /// <summary>
    /// MainScreenWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainScreenWindow : Window
    {
        ClientInstance _clientInstance;
        string _userName;
        Thread th1;
        Thread th2;

        public MainScreenWindow(string userName)
        {
            InitializeComponent();
            _userName = userName;
            //this.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/images/bg1.jpg")));
            
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            userNameLabel.Content = _userName;
            _clientInstance = TcpHandler.GetInstance();

            //获取当前登录用户的图标
            var imagePath = "pack://application:,,,/images/" + _userName + ".jpg";
            var iamgeUri = new Uri(imagePath);
            try
            {
                loginUserImage.Source = new BitmapImage(iamgeUri);

            }
            catch
            {
                imagePath = "pack://application:,,,/images/user.jpg";
                iamgeUri = new Uri(imagePath);
                loginUserImage.Source = new BitmapImage(iamgeUri);

            }
            //获取好友列表
            var commond = TcpHelper.PackCommmond(_userName, TcpHelper.TalkCommond.UpdateUserList);
            _clientInstance.SendMessage(commond);

            //新建线程接收服务端数据
            th1 = new Thread(ReceiveData);
            th1.Start();

            //新建线程，用于更新消息提示
            th2 = new Thread(ScanEmptyMsgTip);
            th2.Start();
        }

        private void ScanEmptyMsgTip()
        {
            string updateMsg;
            while(true){
                while (MessageList.resetMessageTip)
                {
                    foreach (var tip in MessageList.GetMessageTipList())
                    {
                        updateMsg = tip.Value.ToString() ;
                        if(tip.Value == 0)
                        {
                            updateMsg = string.Empty;
                        }
                        AddMessageToLabel(tip.Key + "messageTipLabel", updateMsg);

                    }
                    MessageList.resetMessageTip = false;
                }
            }
            
        }

        private void ReceiveData()
        {
            while(true){

                try
                {
                    var getMsg = _clientInstance.br.ReadString();
                    var unPackMsg = TcpHelper.UnPackCommond(getMsg);
                    var commond = unPackMsg.Item1;
                    switch (commond)
                    {
                        case TcpHelper.TalkCommond.Logout:

                            //获取好友列表
                            var sendCommond = TcpHelper.PackCommmond(_userName, TcpHelper.TalkCommond.UpdateUserList);
                            _clientInstance.SendMessage(sendCommond);

                            break;
                        case TcpHelper.TalkCommond.UpdateUserList:
                            try
                            {
                                //获取用户列表
                                var userList = JsonHandler.UnJson<List<TalkUser>>(unPackMsg.Item2);

                                //线程中不能操作UI
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    CleanUserList();
                                    userList = userList.OrderByDescending(v => v.IsOnline).ToList();
                                    foreach (var user in userList)
                                    {

                                        AddUserToList(user);
                                    }
                                }));
                            }catch(Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                           

                            break;
                        case TcpHelper.TalkCommond.Talk:
                            var talkMsgs = unPackMsg.Item2.Split(',');
                            var talkFromUserName = talkMsgs[0];
                            var talkContent = unPackMsg.Item2.Substring(talkMsgs[0].Length+1,unPackMsg.Item2.Length - talkMsgs[0].Length-1);
                            
                            //将消息存入临时列表
                            StoreTalkMessage(talkFromUserName,talkContent);

                            //判断聊天窗口是否打开
                            TalkWindow tw = IsTalkWindowOpen(talkFromUserName);
                            if(tw == null)
                            {
                                //如果聊天窗口没有打开，则将显示消息条数提示
                                var messageCount = MessageList.memory[talkFromUserName].Count();
                                MessageList.SetMessageTipList(talkFromUserName, messageCount);
                                MessageList.resetMessageTip = true;
                                
                            }
                            
                            break;
                        default:
                            MessageBox.Show(getMsg);
                            break;
                    }

                }catch(Exception e)
                {
                    _clientInstance.CloseConnect();
                    return;
                }

            }
        }

        private void CleanUserList()
        {
            friendListBox.Items.Clear();
        }

        /// <summary>
        /// 添加用户到列表
        /// </summary>
        private void AddUserToList(TalkUser user)
        {
            var userName = user.UserName;
            var isOnline = user.IsOnline;


            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Height = 64;
            listBoxItem.Background = isOnline ? Brushes.LightBlue : Brushes.LightGray;
            //双击弹出聊天窗口
            listBoxItem.MouseDoubleClick += StartTalk;
            listBoxItem.Tag = user;

            //listBoxItem.BorderBrush = Brushes.Gray;
            StackPanel sPanel = new StackPanel();
            sPanel.Orientation = Orientation.Horizontal;

            Image image = new Image();
            var imagePath = "pack://application:,,,/images/"+userName+".jpg";
            var iamgeUri = new Uri(imagePath);
            
            try
            {
                image.Source = new BitmapImage(iamgeUri);

            }catch
            {
                imagePath = "pack://application:,,,/images/user.jpg";
                iamgeUri = new Uri(imagePath);
                image.Source = new BitmapImage(iamgeUri);

            }
            image.Height = 64;
            image.Width = 74;

            StackPanel subSPanel = new StackPanel();
            subSPanel.Orientation = Orientation.Vertical;
            subSPanel.Width = 170;

            Label nameLabel = new Label();
            nameLabel.Content = userName;
            nameLabel.FontSize = 18;
            nameLabel.Foreground = isOnline ? (Brush)new BrushConverter().ConvertFrom("#FF2ECDC6") : Brushes.Gray;
            nameLabel.FontWeight = FontWeights.Bold;

            Label onlineStateLabel = new Label();
            onlineStateLabel.Content = isOnline?"在线":"离线";
            onlineStateLabel.FontSize = 14;
            onlineStateLabel.Foreground = isOnline?Brushes.Red:Brushes.Gray;
            nameLabel.FontWeight = FontWeights.Bold;

            subSPanel.Children.Add(nameLabel);
            subSPanel.Children.Add(onlineStateLabel);

            StackPanel rightSPanel = new StackPanel();
            rightSPanel.Orientation = Orientation.Vertical;
            Label messageTipLabel = new Label();
            messageTipLabel.Name = userName + "messageTipLabel";
            messageTipLabel.Content = string.Empty;
            messageTipLabel.FontSize = 16;
            messageTipLabel.Foreground = Brushes.Red;
            messageTipLabel.FontWeight = FontWeights.Bold;
            messageTipLabel.VerticalAlignment = VerticalAlignment.Center;

            rightSPanel.Children.Add(messageTipLabel);

            sPanel.Children.Add(image);
            sPanel.Children.Add(subSPanel);
            sPanel.Children.Add(rightSPanel);

            listBoxItem.Content = sPanel;


            friendListBox.Items.Add(listBoxItem);
        }

        private void StoreTalkMessage(string fromUserName,string msg)
        {
            MessageStore ms = new MessageStore(){Message = msg,SendDate = DateTime.Now};
            if (MessageList.memory.Keys.Any(v => v == fromUserName))
            {
                MessageList.memory[fromUserName].Add(ms);

            }
            else
            {
                MessageList.memory.Add(fromUserName, new List<MessageStore>() { ms });
            }
        }

        /// <summary>
        /// 判断聊天窗口是否打开,如果打开则返回窗口对象，否则返回null
        /// </summary>
        /// <param name="talkToUserName"></param>
        private TalkWindow IsTalkWindowOpen(string talkToUserName)
        {
            TalkWindow openWindow = null;

            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                foreach (Window win in Application.Current.Windows)
                {
                    if (win.Name == talkToUserName + "_TalkWindow")
                    {
                        openWindow = (TalkWindow)win;
                        break;
                    }
                }
            }));

            return openWindow;
        }
        private void StartTalk(object sender, MouseButtonEventArgs e)
        {
            var item = (ListBoxItem)sender;
            var talkToUser = (TalkUser)item.Tag;
            if(!talkToUser.IsOnline)
            {
                MessageBox.Show(talkToUser.UserName+"未登录，不能进行聊天");
                return;
            }

            TalkWindow tw = IsTalkWindowOpen(talkToUser.UserName);
            if(tw == null)
            {
                //打开聊天窗口
                tw = new TalkWindow(_userName, talkToUser.UserName);
                //弹出显示在父窗口中间
                tw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                //tw.Owner = this;
                tw.Name = talkToUser.UserName + "_TalkWindow";
                //弹出弹出后必须先关闭弹出才能操作父窗口
                tw.Show();
            }
            else
            {
                tw.Activate();

            }
           
            

            

        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            th1.Abort();
            th2.Abort();
            if (_clientInstance != null && _clientInstance.client.Connected)
            {
                var sendMsg = TcpHelper.PackCommmond(string.Empty, TcpHelper.TalkCommond.Logout);
                _clientInstance.SendMessage(sendMsg);
                _clientInstance.br.Close();
                _clientInstance.bw.Close();
                _clientInstance.client.Close();
            }
        }


        private delegate void AddMessageToLabelUIEventHandler(string uiName,string msg);
        private void AddMessageToLabelUI(string uiName, string msg)
        {
            var label = FindChild<Label>(this, uiName);
            label.Content = msg;
        }

        private void AddMessageToLabel(string uiName, string msg)
        {
            AddMessageToLabelUIEventHandler dm = AddMessageToLabelUI;
            this.Dispatcher.Invoke(dm, uiName, msg);

        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }


     
}
