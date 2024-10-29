using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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


namespace CnnApp
{
    /// <summary>
    /// Downloadwd.xaml 的交互逻辑
    /// </summary>
    public partial class Downloadwd : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private string serverUrl = "http://localhost:8080";

        public ObservableCollection<FileNodeModel> FilesLists { get; set; }

        public Downloadwd(string ServerIp, string ServerPort)
        {
            InitializeComponent();
            FilesLists = new ObservableCollection<FileNodeModel>();
            serverUrl = ToUrl(ServerIp, ServerPort);
            LoadTreeViewAsync();

        }
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = FileList.SelectedItem as FileNodeModel;
            if (selectedItem != null && selectedItem.Type.Equals("file"))
            {
                string filePath = getFullPath(selectedItem.Name, FilesLists);
                if (MessageBox.Show($"确认下载文件: {selectedItem.Name}", "确认下载", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    string downloadUrl = $"{serverUrl}/download?file={filePath.Replace(Path.DirectorySeparatorChar, '/')}";
                    DownloadFile(downloadUrl, filePath);
                }

            }
        }
        private async void DownloadFile(string url, string fileName)
        {
            try
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = Path.GetFileName(fileName),
                        DefaultExt = Path.GetExtension(fileName),
                        Filter = "All files (*.*)|*.*"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        await File.WriteAllBytesAsync(saveFileDialog.FileName, fileBytes);
                        MessageBox.Show("文件下载成功");
                    }
                }
            } catch (Exception ex) {
           
                    MessageBox.Show(ex.Message);
                return;
            }
        }


        private async Task LoadTreeViewAsync()
        {
            try
            {
                var response = await client.GetAsync($"{serverUrl}/files");
                response.EnsureSuccessStatusCode();


                string json = await response.Content.ReadAsStringAsync();
                var items = JsonConvert.DeserializeObject<FileNodeModel>(json);
                FilesLists.Add(items);
                DataContext = this;

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading file list: {ex.Message}");
            }
        }


        private string ToDnUrl()
        {
            return serverUrl + "/download";
        }
        private string ToUrl(string ServerIp, string ServerPort)
        {
            return "http://" + ServerIp + ":" + ServerPort;
        }


        private string getFullPath(string fileName, ObservableCollection<FileNodeModel> nodes)
        {
            string fullPath = null;
            Stack<string> pathStack = new Stack<string>(); // 用于构建路径的栈

            void Search(ObservableCollection<FileNodeModel> currentNodes, string currentPath)
            {
                foreach (var node in currentNodes)
                {
                    // 将当前节点名压入栈中  
                    pathStack.Push(node.Name);
                    var currentNodePath = string.Join("/", pathStack.Reverse()); // 构建当前路径（使用"\\"作为路径分隔符，在Windows上）  

                    if (node.Type.Equals("file") && node.Name == fileName) // 如果找到文件且文件名匹配  
                    {
                        fullPath = currentNodePath; // 设置完整路径  
                        return; // 找到文件，退出搜索  
                    }

                    // 如果节点有子节点，则递归搜索  
                    if (node.Children.Count > 0)
                    {
                        Search(node.Children, currentNodePath);
                    }

                    // 回溯：将当前节点名从栈中弹出  
                    pathStack.Pop();
                }
            }
            // 从根节点开始搜索  
            Search(nodes, string.Empty); // 根节点路径为空  

            return fullPath; // 返回找到的完整路径或null（如果未找到文件）  

        }

        private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = FileList.SelectedItem as FileNodeModel;
            if (selectedItem != null && selectedItem.Type.Equals("file"))
            {
                string filePath = getFullPath(selectedItem.Name, FilesLists);
                if (MessageBox.Show($"确认下载文件: {selectedItem.Name}", "确认下载", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    string downloadUrl = $"{serverUrl}/download?file={filePath.Replace(Path.DirectorySeparatorChar, '/')}";
                    DownloadFile(downloadUrl, filePath);
                }

            }
        }
    }
    
}
