﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CleanMyPhone
{
    public partial class Main : Form
    {
        private Dictionary<string, CleanerSettings> _settingsByDeviceID;
        private List<SingleDevicePhoneCleaner> _cleaners = new List<SingleDevicePhoneCleaner>();
        private Dictionary<SingleDevicePhoneCleaner, List<string>> _logs = new Dictionary<SingleDevicePhoneCleaner, List<string>>();
        private string _selectedDeviceID;

        public Main()
        {
            InitializeComponent();

            _notifyIcon.Icon = this.Icon;
        }

        private async void Main_Load(object sender, EventArgs e)
        {
            _settingsByDeviceID = CleanerSettings.GetAllConfigs(GetAppFolder());
            foreach (var settings in _settingsByDeviceID)
            {
                await AddOrUpdateCleaner(settings.Key, settings.Value);
            }
            if (_settingsByDeviceID.Keys.Count > 0)
                this.cmbDevices.SelectedIndex = 0;
        }

        private async Task AddOrUpdateCleaner(string deviceID, CleanerSettings settings)
        {
            // Add to combo box
            if (!this.cmbDevices.Items.OfType<string>().Any(x => x == deviceID))
                this.cmbDevices.Items.Add(deviceID);

            // Remove from memory if exists
            var existingCleaner = _cleaners.FirstOrDefault(x => x.DeviceID == deviceID);
            if (existingCleaner != null)
            {
                existingCleaner.Cancel();
                await existingCleaner.WaitForIdleAsync();
                _cleaners.Remove(existingCleaner);
                _logs.Remove(existingCleaner);
            }

            var newCleaner = new SingleDevicePhoneCleaner(deviceID, settings);
            _cleaners.Add(newCleaner);
            _logs[newCleaner] = new List<string>();
            newCleaner.NewLogLineAdded += HandleNewLogLine;
            newCleaner.RunInBackground();
        }

        private void HandleNewLogLine(SingleDevicePhoneCleaner sender, string line)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<SingleDevicePhoneCleaner, string>)HandleNewLogLine, sender, line);
                return;
            }

            var list = _logs[sender];
            list.Add(line);
            if (list.Count > 1000)
            {
                var trailingLines = list.Skip(500).ToList();
                list.Clear();
                list.AddRange(trailingLines);
            }
            UpdateRollingLogBasedOnSelectedDevice();
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

            var selectedDeviceSettings = _settingsByDeviceID[_selectedDeviceID];
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
                        prop.SetValue(selectedDeviceSettings, (int)(s1 as NumericUpDown).Value);
                        EnableDisableSaveChangesButton();
                    };
                    valueCtrl = numeric;
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    var checkBox = new CheckBox() { Checked = bool.Parse(value)};
                    checkBox.CheckedChanged += (s1, e1) =>
                    {
                        (s1 as CheckBox).Text = (s1 as CheckBox).Checked.ToString();
                        prop.SetValue(selectedDeviceSettings, (s1 as CheckBox).Checked);
                        EnableDisableSaveChangesButton();
                    };
                    valueCtrl = checkBox;
                }
                else {
                    var txtBox = new TextBox();
                    txtBox.TextChanged += (s1, e1) =>
                    {
                        prop.SetValue(selectedDeviceSettings, (s1 as TextBox).Text);
                        EnableDisableSaveChangesButton();
                    };
                    valueCtrl = txtBox;
                }

                valueCtrl.Margin = new Padding(0, 0, 0, 8);
                valueCtrl.Width = this.panelSettings.Width - 30;
                valueCtrl.Tag = value;
                valueCtrl.Text = value;
                
                this.panelSettings.Controls.AddRange(new Control[] { label, valueCtrl });
                EnableDisableSaveChangesButton();
            }

            UpdateRollingLogBasedOnSelectedDevice();
        }

        private void EnableDisableSaveChangesButton()
        {
            var selectedDeviceSettings = _settingsByDeviceID[_selectedDeviceID];
            this.btnSaveChanges.Enabled = this.panelSettings.Controls.OfType<Control>()
                .Where(x => x.Tag != null)
                .Any(x =>  x.Tag.ToString() != x.Text.Trim());
        }

        private void UpdateRollingLogBasedOnSelectedDevice()
        {
            var selectedDeviceCleaner = _cleaners.First(x => x.DeviceID == _selectedDeviceID);
            AppendTextToTextBox(this.txtRollingLog, _logs[selectedDeviceCleaner], this.chkAutoScroll.Checked);
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

        private async void btnSaveChanges_Click(object sender, EventArgs e)
        {
            _settingsByDeviceID[_selectedDeviceID].Save();
            foreach (Control ctrl in this.panelSettings.Controls)
                if (ctrl.Tag != null)
                    ctrl.Tag = ctrl.Text;
            EnableDisableSaveChangesButton();
            await AddOrUpdateCleaner(_selectedDeviceID, _settingsByDeviceID[_selectedDeviceID]);
        }



        private const int SB_HOR = 0x0;
        private const int SB_VERT = 0x1;
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int SB_THUMBPOSITION = 0x4;
        private const int SB_BOTTOM = 0x7;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        [DllImport("user32.dll")]
        private static extern bool PostMessageA(IntPtr hWnd, int nBar, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        private static void AppendTextToTextBox(TextBox textbox, List<string> lines, bool autoscroll)
        {
            int savedVpos = GetScrollPos(textbox.Handle, SB_VERT);
            int savedHpos = GetScrollPos(textbox.Handle, SB_HOR);
            int savedSelectionStart = textbox.SelectionStart;
            int savedSelectionLenght = textbox.SelectionLength;
            textbox.Lines = lines.ToArray();
            textbox.SelectionStart = savedSelectionStart;
            textbox.SelectionLength = savedSelectionLenght;
            if (autoscroll)
            {
                PostMessageA(textbox.Handle, WM_VSCROLL, SB_BOTTOM, 0);
            }
            else
            {
                SetScrollPos(textbox.Handle, SB_VERT, savedVpos, true);
                SetScrollPos(textbox.Handle, SB_HOR, savedHpos, true);
                PostMessageA(textbox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
                PostMessageA(textbox.Handle, WM_HSCROLL, SB_THUMBPOSITION + 0x10000 * savedHpos, 0);
            }
        }
    }
}
