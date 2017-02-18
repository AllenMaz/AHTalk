using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace FTPClientTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        NetworkCredential networkCredential;
        private string _userName;
        private string _password;
        private string _ftpUrl;

        private string _currentLocalDirectoryPath;

        public MainWindow()
        {
            InitializeComponent();

            //桌面目录
            _currentLocalDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ShowFtpClientDirectories("", _currentLocalDirectoryPath);
        }

        /// <summary>
        /// 登录FTP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ftpUrl = "ftp://" + serverIPTextBox.Text.ToString();
                _userName = userNameTextBox.Text.ToString();
                _password = passwordTextBox.Text.ToString();
                networkCredential = new NetworkCredential(_userName, _password);
                ShowFtpServerDirectories();
               

            }catch(Exception ex){
                AddMessage("登录异常："+ex.Message);
            }
            



        }

        private void ShowFtpServerDirectories()
        {
            serverListBox.Items.Clear();

            var ftpRequest = CreateFtpWebRequest(_ftpUrl, WebRequestMethods.Ftp.ListDirectoryDetails);
            var ftpResponse = GetFtpWebResponse(ftpRequest);
            var responseStream = ftpResponse.GetResponseStream();

            StreamReader sr = new StreamReader(responseStream);
            var s = sr.ReadToEnd();
            string[] ftpDir = s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            System.Drawing.Icon icon = FileHelp.GetDirectoryIcon();
            foreach(var dir in ftpDir){

                string[] parseDir = ParseMSDosLIST(dir);
                string year = parseDir[0];
                string time = parseDir[1];
                string size = parseDir[2];
                string name = parseDir[3];

                if (size.ToLower().Contains("<dir>"))
                {
                    //文件夹
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;
                    sp.Tag = name;

                    Image img = new Image();
                    img.Source = BitmapToBitmapSource(icon.ToBitmap());

                    Label lb = new Label();
                    lb.FontSize = 16;
                    lb.Content = name;

                    sp.Children.Add(img);
                    sp.Children.Add(lb);
                    serverListBox.Items.Add(sp);
                }
                else
                {
                    System.Drawing.Icon fileIcon = FileHelp.GetFileIcon(name);

                    //文件
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;
                    sp.Tag = name;

                    Image img = new Image();
                    img.Source = BitmapToBitmapSource(fileIcon.ToBitmap());

                    Label lb = new Label();
                    lb.FontSize = 16;
                    lb.Content = name;

                    sp.Children.Add(img);
                    sp.Children.Add(lb);
                    serverListBox.Items.Add(sp);
                }

                //SetServerDirectories(dir);
            }
        }

        /// <summary>
        /// 解析DOS 下LIST返回的列表
        /// 格式：02-23-05  09:24AM                 2245 readme.ESn
        /// 第一段为05-25-04，一个空格后，为第二段08:56AM，一个空格后，
        /// 为    19041660，由于文件长度不一定，预留的位置比较长，因此前面用空格填充了。
        /// </summary>
        /// <returns></returns>
        private string[] ParseMSDosLIST(string dirstr)
        {
            string[] arrDir= new string[4];

            var firstEmptyPostion = dirstr.IndexOf(' ');
            string year = dirstr.Substring(0,firstEmptyPostion);
            dirstr = dirstr.Substring(firstEmptyPostion).Trim();

            firstEmptyPostion = dirstr.IndexOf(' ');
            string time = dirstr.Substring(0,firstEmptyPostion);
            dirstr = dirstr.Substring(firstEmptyPostion).Trim();

            firstEmptyPostion = dirstr.IndexOf(' ');
            string size = dirstr.Substring(0, firstEmptyPostion);
            dirstr = dirstr.Substring(firstEmptyPostion).Trim();

            string name = dirstr;

            arrDir[0] = year;
            arrDir[1] = time;
            arrDir[2] = size;
            arrDir[3] = name;

            return arrDir;
        }

        private bool IsDirectory(string directory)
        {
            var isDirectory = false;
            if(Directory.Exists(directory)){
                isDirectory = true;
            }
            return isDirectory;
        }
        private void ShowFtpClientDirectories(string cmd,string currentDirectory)
        {
            
            System.Drawing.Icon icon = FileHelp.GetDirectoryIcon();

            //清空区域
            localDirectoryList.Items.Clear();
            

            var directories = Directory.GetDirectories(currentDirectory);
            foreach(var directory in directories){
                DirectoryInfo  di = new DirectoryInfo(directory);

                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Tag = di.Name;

                Image img = new Image();
                img.Source = BitmapToBitmapSource(icon.ToBitmap());

                Label lb = new Label();
                lb.FontSize = 16;
                lb.Content = di.Name;

                sp.Children.Add(img);
                sp.Children.Add(lb);
                localDirectoryList.Items.Add(sp);
            }
            var files = Directory.GetFiles(currentDirectory);
            foreach(var file in files){

                System.Drawing.Icon fileIcon = FileHelp.GetFileIcon(file);
                FileInfo fi = new FileInfo(file);
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Tag = fi.Name;

                Image img = new Image();
                img.Source = BitmapToBitmapSource(fileIcon.ToBitmap());

                Label lb = new Label();
                lb.FontSize = 16;
                lb.Content = fi.Name;

                sp.Children.Add(img);
                sp.Children.Add(lb);
                
                localDirectoryList.Items.Add(sp);
            }
            
            
        }

        private BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, bitmap.RawFormat);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        #region 将System.Drawing.Icon转换为BitMapSource
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            BitmapSource result =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //release resource
            DeleteObject(ptr);

            return result;
        }

        #endregion
        /// <summary>
        /// 创建FtpWebRequest对象
        /// </summary>
        /// <param name="uri">资源标识</param>
        /// <param name="requestMethod">要发送到服务器的命令</param>
        /// <returns>FtpWebRequest对象</returns>
        private FtpWebRequest CreateFtpWebRequest(string uri, string requestMethod)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(uri);
            request.Credentials = networkCredential;
            request.KeepAlive = true;
            request.UseBinary = true;
            request.Method = requestMethod;
            return request;
        }

        private FtpWebResponse GetFtpWebResponse(FtpWebRequest request)
        {
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)request.GetResponse();
                AddMessage("StatusCode：" + response.StatusCode);
                AddMessage("StatusDescription：" + response.StatusDescription);
            }
            catch (WebException e)
            {
                var ftpResponse = (FtpWebResponse)e.Response;
                AddMessage("获取FtpWebResponse出现异常："+e.Message);
                AddMessage("StatusCode："+ftpResponse.StatusCode);
                AddMessage("StatusDescription：" + ftpResponse.StatusDescription);

            }
            catch (Exception e)
            {
                AddMessage("获取FtpWebResponse出现异常："+e.Message);
            }
            return response;
        }


        private void SetClientDirectories(string directoryStr)
        {

            localDirectoryList.Dispatcher.Invoke((Action)(() =>
            {
                localDirectoryList.Items.Add(directoryStr);
            }));
        }

        private void SetServerDirectories(string directoryStr)
        {
            
            serverListBox.Dispatcher.Invoke((Action)(() =>
            {
                serverListBox.Items.Add(directoryStr);
            }));
        }

        private void AddMessage(string msg)
        {
            infoListBox.Dispatcher.Invoke((Action)(() => {
                infoListBox.Items.Add(msg);
            }));
        }



        //单击目录时进入下一级目录
        private void LocalDirectoryClick(object sender, SelectionChangedEventArgs e)
        {
            if(localDirectoryList.SelectedItem != null){
                StackPanel sp = (StackPanel)localDirectoryList.SelectedItem;

                var directoryName = sp.Tag.ToString();
                //判断当前选择的是否是文件夹
                var selectItemPath = System.IO.Path.Combine(_currentLocalDirectoryPath, directoryName);
                if(IsDirectory(selectItemPath)){
                    _currentLocalDirectoryPath = selectItemPath;
                    ShowFtpClientDirectories("", _currentLocalDirectoryPath);
                }

                
            }
           
        }

        //返回上级
        private void localBackDirectory_Click(object sender, RoutedEventArgs e)
        {
            //桌面路径
            var deskPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if (_currentLocalDirectoryPath != deskPath)
            {
                var currentDI = new DirectoryInfo(_currentLocalDirectoryPath);
                _currentLocalDirectoryPath = currentDI.Parent.FullName;
                ShowFtpClientDirectories("",_currentLocalDirectoryPath);
            }
        }


        //服务端进入下一级目录
        private void ServerDirectory_Changed(object sender, SelectionChangedEventArgs e)
        {
            if(serverListBox.SelectedItem != null){
                StackPanel sp = (StackPanel)serverListBox.SelectedItem;

                var directoryName = sp.Tag.ToString();
                _ftpUrl = _ftpUrl+"/"+directoryName;
                ShowFtpServerDirectories();
                
            }
           
        }

        //服务端返回上级目录
        private void serverBackDirectory_Click(object sender, RoutedEventArgs e)
        {
            var lastIndex = _ftpUrl.LastIndexOf('/');
            var newFtpUrl = _ftpUrl.Substring(0,lastIndex);
            if (newFtpUrl.ToLower().Contains("ftp://"))
            {
                _ftpUrl = newFtpUrl;
                ShowFtpServerDirectories();

            }
        }

        //上传到ftp
        private void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            var selectItem = localDirectoryList.SelectedItem;
            if(selectItem ==null){
                MessageBox.Show("请选择要上传的文件或文件夹");
                return;
            }

            var selectFileName = ((StackPanel)selectItem).Tag.ToString();

            var selectFilePath =System.IO.Path.Combine(_currentLocalDirectoryPath,selectFileName);
            if(IsDirectory(selectFilePath)){
                //文件夹
                DirectoryInfo di = new DirectoryInfo(selectFilePath);
            }
            else
            {

                FileInfo fileInfo = new FileInfo(selectFilePath);
                string uri = _ftpUrl + "/" + fileInfo.Name;
                FtpWebRequest request = CreateFtpWebRequest(uri, WebRequestMethods.Ftp.UploadFile);
                request.ContentLength = fileInfo.Length;
                int buffLength = 8196;
                byte[] buff = new byte[buffLength];
                FileStream fs = fileInfo.OpenRead();
                try
                {
                    Stream responseStream = request.GetRequestStream();
                    int contentLen = fs.Read(buff, 0, buffLength);
                    while (contentLen != 0)
                    {
                        responseStream.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }
                    responseStream.Close();
                    fs.Close();
                    FtpWebResponse response = GetFtpWebResponse(request);
                    if (response == null)
                    {
                        return;
                    }
                    ShowFtpServerDirectories();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "上传失败");
                }
            }
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
