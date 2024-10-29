using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace CnnApp
{
    /// <summary>
    /// Connect.xaml 的交互逻辑
    /// </summary>
    public partial class Connect : Window
    {
        public Connect()
        {
            InitializeComponent();
        }
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                try
                {
                    var response = await client.GetAsync($"http://{IpBox.Text}:8080/connect", HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    MessageBox.Show("连接成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            MainWindow mainWindow = null;
            if (IpBox.Text.LastIndexOf(':').Equals(-1))
            {
                mainWindow = new MainWindow(IpBox.Text);
            }
            else
            {
                string port = IpBox.Text.Substring(IpBox.Text.LastIndexOf(':') + 1);
                mainWindow = new MainWindow(IpBox.Text, port);
            }
            if (mainWindow.stastic_code.Equals(0))
            {
                mainWindow.Show();
                this.Close();
            }
            else
            {
                mainWindow.Close();
            }

        }
    }
}
