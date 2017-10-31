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
        private Dictionary<string, CleanerSettings> _configs;
        private List<SingleDevicePhoneCleaner> _cleaners = new List<SingleDevicePhoneCleaner>();
        private Dictionary<SingleDevicePhoneCleaner, List<string>> _logs = new Dictionary<SingleDevicePhoneCleaner, List<string>>();
        private string _selectedDeviceID;
        private bool _autoScroll = true;

        public Main()
        {
            InitializeComponent();

            _notifyIcon.Icon = this.Icon;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _configs = CleanerSettings.GetAllConfigs(GetAppFolder());
            foreach (var config in _configs)
            {
                this.cmbDevices.Items.Add(config.Key);
                var cleaner = new SingleDevicePhoneCleaner(config.Key, config.Value);
                _logs[cleaner] = new List<string>();
                cleaner.NewLogLineAdded += HandleNewLogLine;
                cleaner.RunInBackground();
                _cleaners.Add(cleaner);
            }
            if (_configs.Keys.Count > 0)
                this.cmbDevices.SelectedIndex = 0;
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


        private static string GetAppFolder()
        {
            var pathToAppFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CleanMyPhone");
            if (!Directory.Exists(pathToAppFolder))
                Directory.CreateDirectory(pathToAppFolder);
            return pathToAppFolder;
        }

        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedDeviceID = this.cmbDevices.SelectedItem.ToString();

            var selectedDeviceSettings = _configs[_selectedDeviceID];
            this.panelSettings.Controls.Clear();
            foreach (var prop in typeof(CleanerSettings).GetProperties())
            {
                var name = prop.Name;
                var value = prop.GetValue(selectedDeviceSettings).ToString();
                var label = new Label() { Text = name, Margin = new Padding(0), Height = 15, BackColor = Color.LightBlue, Width = this.panelSettings.Width - 30 };

                Control valueCtrl;
                if (prop.PropertyType == typeof(int))
                {
                    var numeric = new NumericUpDown() { Minimum = int.MinValue, Maximum = int.MaxValue };
                    numeric.ValueChanged += (s1, e1) =>
                    {
                        (s1 as NumericUpDown).Text = (s1 as NumericUpDown).Value.ToString();
                        EnableDisableSaveChangesButton();
                    };
                    valueCtrl = numeric;
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    var checkBox = new CheckBox() { Checked = bool.Parse(value), Text = value };
                    checkBox.CheckedChanged += (s1, e1) =>
                    {
                        (s1 as CheckBox).Text = (s1 as CheckBox).Checked.ToString();
                        EnableDisableSaveChangesButton();
                    };
                    valueCtrl = checkBox;
                }
                else
                    valueCtrl = new TextBox();

                valueCtrl.Margin = new Padding(0, 0, 0, 8);
                valueCtrl.Width = this.panelSettings.Width - 30;
                valueCtrl.Tag = value;
                valueCtrl.Text = value;
                valueCtrl.TextChanged += (s1, e1) => EnableDisableSaveChangesButton();
                this.panelSettings.Controls.AddRange(new Control[] { label, valueCtrl });
            }

            UpdateRollingLogBasedOnSelectedDevice();
        }

        private void EnableDisableSaveChangesButton()
        {
            var selectedDeviceSettings = _configs[_selectedDeviceID];
            this.btnSaveChanges.Enabled = this.panelSettings.Controls.OfType<Control>()
                .Where(x => x.Tag != null)
                .Any(x =>  x.Tag.ToString() != x.Text.Trim());
        }

        private void UpdateRollingLogBasedOnSelectedDevice()
        {
            var selectedDeviceCleaner = _cleaners.First(x => x._deviceID == _selectedDeviceID);
            if (_autoScroll) { 
                this.txtRollingLog.Lines = _logs[selectedDeviceCleaner].ToArray();
                var indexOf = this.txtRollingLog.Lines.Any()?this.txtRollingLog.Text.IndexOf(this.txtRollingLog.Lines.Last()): 0;
                this.txtRollingLog.SelectionStart = indexOf;
                this.txtRollingLog.SelectionLength = 0;
                this.txtRollingLog.ScrollToCaret();
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

        private void txtRollingLog_MouseDown(object sender, MouseEventArgs e)
        {
            _autoScroll = false;
        }

        private async void txtRollingLog_MouseUp(object sender, MouseEventArgs e)
        {
            await Task.Delay(1500);
            _autoScroll = true;
        }

        

        private void Main_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.ShowInTaskbar = false;
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {

        }
    }
}
