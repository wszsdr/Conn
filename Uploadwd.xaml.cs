using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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
using System.Threading;

namespace CnnApp
{
    /// <summary>
    /// Uploadwd.xaml 的交互逻辑
    /// </summary>
    public partial class Uploadwd : Window
    {
        public ObservableCollection<FileInfo> SelectedFiles { get; set; }
        private string ServerIp;
        private string ServerPort = "8080";
        public Uploadwd(string ServerIp, string ServerPort = "8080")
        {
            InitializeComponent();
            this.ServerIp = ServerIp;
            this.ServerPort = ServerPort;
            SelectedFiles = new ObservableCollection<FileInfo>();
            FileListBox.ItemsSource = SelectedFiles;
        }
        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            UploadButton.IsEnabled = false;
            if (MessageBox.Show($"确认上传文件", "确认上传", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                using (var client = new HttpClient())
                using (var content = new MultipartFormDataContent())
                {
                    foreach (var file in SelectedFiles)
                    {
                        var streamContent = new StreamContent(new FileStream(file.ToString(), FileMode.Open, FileAccess.Read));
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        content.Add(streamContent, "files", file.Name);
                    }
                    client.Timeout = TimeSpan.FromHours(10);
                    try
                    {
                        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                        {
                            CancelButton.Visibility = Visibility.Visible;
                            CancelButton.Click += (se, rg) => { cancellationTokenSource.Cancel(); };
                            var response = await client.PostAsync(ToUpUrl(ServerIp, ServerPort), content, cancellationTokenSource.Token);
                            response.EnsureSuccessStatusCode();
                            UploadButton.IsEnabled = true;
                            CancelButton.Visibility = Visibility.Hidden;
                            MessageBox.Show("文件上传成功!");
                        }
                    }catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        CancelButton.Visibility = Visibility.Hidden;
                        UploadButton.IsEnabled = true;
                        return;
                    }

                }
            }
        }

        private void SelectFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {

                    SelectedFiles.Add(new FileInfo(file));
                }
            }
        }
        private string ToUpUrl(string ServerIp, string ServerPort)
        {
            return "http://" + ServerIp + ":" + ServerPort + "/upload";
        }
    }
}
