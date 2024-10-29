using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

namespace CnnApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileInfo> SelectedFiles { get; set; }
        private string ServerIp;
        private string ServerPort = "8080";
        public int stastic_code = 0;
        public MainWindow(string ServerIp, string ServerPort = "8080")
        {
            if (ServerIp.Equals(""))
            {
                stastic_code = -1;
            }
            this.ServerIp = ServerIp;
            this.ServerPort = ServerPort;
            InitializeComponent();
        }
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            Uploadwd uploadwd = new Uploadwd(ServerIp, ServerPort);
            uploadwd.ShowDialog();
        }
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Downloadwd downloadwd = new Downloadwd(ServerIp, ServerPort);
            downloadwd.ShowDialog();
        }

        private void ExecuButton_Click(object sender, RoutedEventArgs e)
        {
            Execuwd execuwd = new Execuwd(ServerIp, ServerPort);
            execuwd.ShowDialog();
        }
    }
}
