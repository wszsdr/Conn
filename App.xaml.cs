using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CnnApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       
    }
    public class FileNodeModel
    {
        public string Name { get; set; }
        public string Type { get; set; } // "File" 或 "Directory"  
        public ObservableCollection<FileNodeModel> Children { get; set; }

        public FileNodeModel()
        {
            Children = new ObservableCollection<FileNodeModel>();
        }
    }
}
