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
using WebSocket4Net;

namespace CnnApp
{
    /// <summary>
    /// Execuwd.xaml 的交互逻辑
    /// </summary>
    public partial class Execuwd : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private string serverUrl = "http://localhost:8080";
        private WebSocket webSocket;
        private string wsurl = "";

        public ObservableCollection<FileNodeModel> FilesLists { get; set; }

        public Execuwd(string ServerIp, string ServerPort)
        {
            InitializeComponent();
            FilesLists = new ObservableCollection<FileNodeModel>();
            serverUrl = ToUrl(ServerIp, ServerPort);
            wsurl = TowsUrl(ServerIp, ServerPort);
            LoadTreeViewAsync();

        }
        private void ExecuteFile(string filePath)
        {
            string uri = wsurl + "/execute";
            webSocket = new WebSocket(uri);

            // 订阅事件  
            webSocket.Opened += (sender, e) =>
            {
                MessageBox.Show("连接已打开");
                var message = new { type = "file", data = filePath };
                webSocket.Send(JsonConvert.SerializeObject(message));
                Dispatcher.Invoke(() =>
                {
                    // 在 UI 线程上更新界面  
                    StopButton.Visibility = Visibility.Visible;
                    StopButton.Click += (sender_, e_) => {
                        webSocket.Send("stop");
                    };
                    this.Closing += (_sender, _e) =>
                    {
                        if (StopButton.Visibility == Visibility.Visible)
                        {
                            StopButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                    };
                });
            };

            webSocket.MessageReceived += (sender, e) =>
            {

                var msg = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(e.Message));
                Dispatcher.Invoke(() =>
                {
                    // 在 UI 线程上更新界面  
                    OutputTextBox.AppendText(msg + Environment.NewLine);
                });
            };

            webSocket.Error += (sender, e) =>
            {
                MessageBox.Show($"Error: {e.Exception.Message}");
            };

            webSocket.Closed += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    // 在 UI 线程上更新界面  
                    StopButton.Visibility = Visibility.Hidden;
                });
                MessageBox.Show("连接已关闭.");
            };

            // 打开连接  
            webSocket.Open();
        }

        private void StopButton_Click1(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private string ToUrl(string ServerIp, string ServerPort)
        {
            return "http://" + ServerIp + ":" + ServerPort;
        }
        private string TowsUrl(string ServerIp, string ServerPort)
        {
            return "ws://" + ServerIp + ":" + ServerPort;
        }
        private async Task LoadTreeViewAsync()
        {
            if (FilesLists.Count > 0)
            {
                FilesLists.RemoveAt(0);
            }
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
        private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

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
                    var currentNodePath = string.Join("\\", pathStack.Reverse()); // 构建当前路径（使用"\\"作为路径分隔符，在Windows上）  

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

        private string getFullPath_ofDir(string fileName, ObservableCollection<FileNodeModel> nodes)
        {
            string fullPath = null;
            Stack<string> pathStack = new Stack<string>(); // 用于构建路径的栈

            void Search(ObservableCollection<FileNodeModel> currentNodes, string currentPath)
            {
                foreach (var node in currentNodes)
                {
                    // 将当前节点名压入栈中  
                    pathStack.Push(node.Name);
                    var currentNodePath = string.Join("\\", pathStack.Reverse()); // 构建当前路径（使用"\\"作为路径分隔符，在Windows上）  

                    if (node.Type.Equals("dir") && node.Name == fileName) // 如果找到文件且文件名匹配  
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

        // 确保在窗口关闭时关闭 WebSocket 连接  
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                webSocket.Close();
            }
        }

        private void DePressButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = FileList.SelectedItem as FileNodeModel;
            if (selectedItem != null && selectedItem.Type.Equals("file"))
            {
                string filePath = getFullPath(selectedItem.Name, FilesLists);
                if (MessageBox.Show($"确认解压文件: {selectedItem.Name}", "确认解压", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    string url = $"{serverUrl}/depress?file={filePath.Replace(Path.DirectorySeparatorChar, '/')}";
                    DePressFile(filePath, url);
                }

            }
        }
        private async void DePressFile(string filePath, string url)
        {
            try
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("解压成功");
                    await LoadTreeViewAsync();
                }
            } catch (Exception e) {
           
                
                    MessageBox.Show(e.Message);
                
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = FileList.SelectedItem as FileNodeModel;
            if (selectedItem != null && selectedItem.Type.Equals("file"))
            {
                string filePath = getFullPath(selectedItem.Name, FilesLists);
                if (MessageBox.Show($"确认执行文件: {selectedItem.Name}", "确认执行", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    ExecuteFile(filePath);
                }

            }
        }

        private void ReMoveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = FileList.SelectedItem as FileNodeModel;
            if (selectedItem != null && selectedItem.Type.Equals("file"))
            {
                string filePath = getFullPath(selectedItem.Name, FilesLists);
                if (MessageBox.Show($"确认删除文件: {selectedItem.Name}", "确认删除", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    string url = $"{serverUrl}/remove?file={filePath.Replace(Path.DirectorySeparatorChar, '/')}";
                    RemoveFile(url, filePath);
                }

            }
            else if (selectedItem != null && selectedItem.Type.Equals("dir"))
            {
                string filePath = getFullPath_ofDir(selectedItem.Name, FilesLists);
                if (MessageBox.Show($"确认删除文件夹: {selectedItem.Name}", "确认删除", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    string url = $"{serverUrl}/remove?file={filePath.Replace(Path.DirectorySeparatorChar, '/')}";
                    RemoveFile(url, filePath);
                }
            }
        }
        private async void RemoveFile(string url, string fileName)
        {
            try
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("删除成功");
                    await LoadTreeViewAsync();
                }
            } catch (Exception e) {
           
                
                    MessageBox.Show(e.Message);
                
            }
        }

    }
}
