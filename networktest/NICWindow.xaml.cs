using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace networktest
{
    /// <summary>
    /// NICWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NICWindow : Window
    {
        public NICWindow()
        {
            InitializeComponent();
        }

        private void NIC_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> infos = new List<string>();
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            infos.Add("网络适配器个数："+adapters.Count());

            int i = 0;
            foreach(var adapter in adapters)
            {
                i++;
                infos.Add("\r\n----------------第个"+i+"适配器-----------------");
                infos.Add("描述："+adapter.Description);
                infos.Add("名称：" + adapter.Name);
                infos.Add("类型：" + adapter.NetworkInterfaceType);
                infos.Add("速度：" + adapter.Speed);
                infos.Add("MAC地址：" + adapter.GetPhysicalAddress());

                infos.Add("DNS服务器IP地址：");
                var ipProperties = adapter.GetIPProperties();
                var dnsAddress = ipProperties.DnsAddresses;
                foreach(var address in dnsAddress)
                {
                    infos.Add(address.ToString());
                }
            }

            foreach(var info in infos)
            {
                nicTextBlock1.Text += info+"\r\n";
            }
        }
    }
}
