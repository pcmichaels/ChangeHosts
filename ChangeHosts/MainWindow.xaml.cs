using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ChangeHosts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _URIFilter;

        public string URIFilter
        {
            get { return _URIFilter; }
            set
            {
                if (value != _URIFilter)
                {
                    _URIFilter = value;
                    NotifyPropertyChanged();
                    UpdateHostsList();
                }
            }
        }

        private List<string> _hostSelections = new List<string>();
        public List<string> HostSelections
        {
            get { return _hostSelections; }
            set
            {
                _hostSelections = value;
                NotifyPropertyChanged();
            }
        }

        private int _previousSelectedIdx = -1;
        private int _hostSelectedIdx;
        public int HostSelectedIdx
        {
            get { return _hostSelectedIdx; }
            set
            {
                if (_hostSelectedIdx != _previousSelectedIdx || _previousSelectedIdx == -1)
                {
                    // Update previous value
                    _previousSelectedIdx = _hostSelectedIdx;

                    // Now update the selected index
                    _hostSelectedIdx = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            SetDefaults();
        }

        private void SetDefaults()
        {
            URIFilter = "www.myuri.com";
        }

        private async void LoadHosts_Click(object sender, RoutedEventArgs e)
        {
            await LoadHostFile();
        }

        private async Task LoadHostFile()
        {
            using (StreamReader sr = File.OpenText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts")))
            {
                string hosts = await sr.ReadToEndAsync();
                string[] entries = hosts.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                HostSelections = entries.ToList();

                int idxSel = entries.Select((str, idx) => new { str, idx }).FirstOrDefault(e => !e.str.StartsWith("#")).idx;
                HostSelectedIdx = idxSel;
            }
        }

        private async void SetActiveHost_Click(object sender, RoutedEventArgs e)
        {
            await ChangeSelecteHost();
        }

        private async Task ChangeSelecteHost()
        {
            if (HostSelections[HostSelectedIdx].StartsWith("#"))
            {
                HostSelections[HostSelectedIdx] = HostSelections[HostSelectedIdx].TrimStart('#');
                HostSelections[_previousSelectedIdx] = $"#{HostSelections[_previousSelectedIdx]}";

                WriteHostsFile();
                await LoadHostFile();
            }
            else
            {
                MessageBox.Show("The selected line is already selected");
            }
        }

        private void WriteHostsFile()
        {
            string updatedHosts = string.Join(Environment.NewLine, HostSelections);

            File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), 
                "drivers/etc/hosts"), updatedHosts);
        }

        private void UpdateHostsList()
        {
            
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String info = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
