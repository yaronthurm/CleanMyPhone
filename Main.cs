using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CleanMyPhone
{
    public partial class Main : Form
    {
        private string _appFolder;
        private Dictionary<string, CleanerSettings> _configs;
        private List<SingleDevicePhoneCleaner> _cleaners = new List<SingleDevicePhoneCleaner>();
        private Dictionary<SingleDevicePhoneCleaner, List<string>> _logs = new Dictionary<SingleDevicePhoneCleaner, List<string>>();
        private string _selectedDeviceID;
        private bool _autoScroll = true;

        public Main()
        {
            InitializeComponent();

            _notifyIcon.Icon = this.Icon;
            _appFolder = GetAppFolder();
            _configs = CleanerSettings.GetAllConfigs(_appFolder);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            foreach (var config in _configs)
            {
                this.comboBox1.Items.Add(config.Key);
                var cleaner = new SingleDevicePhoneCleaner(config.Key, config.Value);
                _logs[cleaner] = new List<string>();
                cleaner.NewLogLineAdded += HandleNewLogLine;
                cleaner.RunInBackground();
                _cleaners.Add(cleaner);
            }
            if (_configs.Keys.Count > 0)
                this.comboBox1.SelectedIndex = 0;
        }

        private void HandleNewLogLine(SingleDevicePhoneCleaner sender, string line)
        {
            var list = _logs[sender];
            list.Add(line);
            if (list.Count > 1000)
            {
                var trailingLines = list.Skip(500).ToList();
                list.Clear();
                list.AddRange(trailingLines);
            }

            if (this.InvokeRequired)
                this.BeginInvoke((Action)UpdateRollingLogBasedOnSelectedDevice);
        }


        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            this.Visible = !this.Visible;
            this.ShowInTaskbar = !this.ShowInTaskbar;
        }

        private static string GetAppFolder()
        {
            var pathToAppFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CleanMyPhone");
            if (!Directory.Exists(pathToAppFolder))
                Directory.CreateDirectory(pathToAppFolder);
            return pathToAppFolder;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.flowLayoutPanel1.Controls.Clear();
            _selectedDeviceID = this.comboBox1.SelectedItem.ToString();
            var selectedDeviceSettings = _configs[_selectedDeviceID];

            foreach (var prop in typeof(CleanerSettings).GetProperties())
            {
                var name = prop.Name;
                var value = prop.GetValue(selectedDeviceSettings).ToString();
                var label = new Label() { Text = name, Margin = new Padding(0), Height = 15, BackColor = Color.LightBlue, Width = this.flowLayoutPanel1.Width };
                var txt = new TextBox() { Text = value, Margin = new Padding(0, 0, 0, 10), Width = this.flowLayoutPanel1.Width};
                this.flowLayoutPanel1.Controls.AddRange(new Control[] { label, txt });
            }

            UpdateRollingLogBasedOnSelectedDevice();
        }

        private void UpdateRollingLogBasedOnSelectedDevice()
        {
            var selectedDeviceCleaner = _cleaners.First(x => x._deviceID == _selectedDeviceID);
            if (_autoScroll) { 
                this.textBox1.Lines = _logs[selectedDeviceCleaner].ToArray();
                var indexOf = this.textBox1.Lines.Any()?this.textBox1.Text.IndexOf(this.textBox1.Lines.Last()): 0;
                this.textBox1.SelectionStart = indexOf;
                this.textBox1.SelectionLength = 0;
                this.textBox1.ScrollToCaret();
            }
        }        


        private void flowLayoutPanel1_Resize(object sender, EventArgs e)
        {
            foreach (Control control in this.flowLayoutPanel1.Controls){
                control.Width = this.flowLayoutPanel1.Width;
            }
        }

        private async void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Text = "Stopping...";
            foreach (var cleaner in _cleaners)
                cleaner.Cancel();

            // This will cause the app to remain until all code has executed
            e.Cancel = true;

            // This will cause the code to wait while still being responsive
            await Task.Run(() =>
            {
                foreach (var cleaner in _cleaners)
                    cleaner.WaitForIdle();
            });

            // Now we can exit the app
            this.Text = "All done";
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _autoScroll = false;
        }

        private async void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            await Task.Delay(1500);
            _autoScroll = true;
        }
    }
}
