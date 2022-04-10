using System;
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
        private Dictionary<string, ICleanerSettings> _settingsByDeviceID;
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
            _settingsByDeviceID = CleanerSettingsFactory.GetAllConfigs(GetAppFolder());
            foreach (var settings in _settingsByDeviceID)
            {
                await AddOrUpdateCleaner(settings.Key, settings.Value);
            }
            if (this.cmbDevices.Items.Count > 0)
                this.cmbDevices.SelectedIndex = 0;
        }

        private async Task AddOrUpdateCleaner(string deviceID, ICleanerSettings settings)
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
            if (selectedDeviceSettings is CleanerSettingsV1)
                RenderSettingsV1(selectedDeviceSettings as CleanerSettingsV1);
            if (selectedDeviceSettings is CleanerSettingsV2)
                RenderSettingsV2(selectedDeviceSettings as CleanerSettingsV2);

            UpdateRollingLogBasedOnSelectedDevice();
        }

        private void RenderSettingsV1(CleanerSettingsV1 selectedDeviceSettings )
        {
            this.panelSettings.Controls.Clear();
            foreach (var prop in typeof(CleanerSettingsV1).GetProperties())
            {
                var name = prop.Name;
                var value = prop.GetValue(selectedDeviceSettings).ToString();
                var label = new Label() { Text = name, Margin = new Padding(0), Height = 15, BackColor = Color.LightBlue, Width = this.panelSettings.Width - 30 };

                Control valueCtrl = null;
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
                    var checkBox = new CheckBox() { Checked = bool.Parse(value) };
                    checkBox.CheckedChanged += (s1, e1) =>
                    {
                        (s1 as CheckBox).Text = (s1 as CheckBox).Checked.ToString();
                        prop.SetValue(selectedDeviceSettings, (s1 as CheckBox).Checked);
                        EnableDisableSaveChangesButton();
                    };
                    valueCtrl = checkBox;
                }
                else if (prop.PropertyType == typeof(string))
                {
                    var txtBox = new TextBox();
                    txtBox.TextChanged += (s1, e1) =>
                    {
                        prop.SetValue(selectedDeviceSettings, (s1 as TextBox).Text);
                        EnableDisableSaveChangesButton();
                    };
                    valueCtrl = txtBox;
                }

                if (valueCtrl != null)
                {
                    valueCtrl.Margin = new Padding(0, 0, 0, 8);
                    valueCtrl.Width = this.panelSettings.Width - 30;
                    valueCtrl.Tag = value;
                    valueCtrl.Text = value;

                    this.panelSettings.Controls.AddRange(new Control[] { label, valueCtrl });
                }
                EnableDisableSaveChangesButton();
            }
        }

        private void RenderSettingsV2(CleanerSettingsV2 selectedDeviceSettings)
        {
            this.panelSettings.Controls.Clear();
            var txtBox = new TextBox();
            txtBox.Multiline = true;
            txtBox.Margin = new Padding(0, 0, 0, 8);
            txtBox.Width = this.panelSettings.Width - 6;
            txtBox.Height = this.panelSettings.Height - 12;
            txtBox.Tag = txtBox.Text = selectedDeviceSettings.ToText();
            txtBox.TextChanged += (s1, e1) => EnableDisableSaveChangesButton();
            txtBox.WordWrap = false;            
            this.panelSettings.Controls.AddRange(new Control[] { txtBox });            
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
            AppendTextToTextBox(this.txtRollingLog, _logs[selectedDeviceCleaner].ToArray());
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
            var selectedSettings = _settingsByDeviceID[_selectedDeviceID];
            if (selectedSettings is CleanerSettingsV2)
            {
                var txt = this.panelSettings.Controls.Cast<Control>().OfType<TextBox>().FirstOrDefault()?.Text;
                try
                {
                    (selectedSettings as CleanerSettingsV2).UpdateFromText(txt);
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid input", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                } 
            }

            selectedSettings.Save();
            foreach (Control ctrl in this.panelSettings.Controls)
                if (ctrl.Tag != null)
                    ctrl.Tag = ctrl.Text;
            EnableDisableSaveChangesButton();
            await AddOrUpdateCleaner(_selectedDeviceID, _settingsByDeviceID[_selectedDeviceID]);
        }
  
        private async void btnAddDevice_Click(object sender, EventArgs e)
        {
            var f = new AddDeviceForm();
            f.StartPosition = FormStartPosition.CenterParent;
            f.ShowDialog();

            // Reload new items that were added (if any)
            var freshSettings = CleanerSettingsFactory.GetAllConfigs(GetAppFolder());
            if (freshSettings.Count == 1) // This is the first item added
                _selectedDeviceID = freshSettings.First().Key;
            var addedDevices = freshSettings.Where(x => !_settingsByDeviceID.ContainsKey(x.Key));
            foreach (var addedDeviceSettings in addedDevices)
            {
                _settingsByDeviceID.Add(addedDeviceSettings.Key, addedDeviceSettings.Value);
                await AddOrUpdateCleaner(addedDeviceSettings.Key, addedDeviceSettings.Value);                
            }

            if (this.cmbDevices.Items.Count == 1)// This is the first item added
                this.cmbDevices.SelectedIndex = 0; // Ensure it is selected
        }


        private const int SB_HORZ = 0x0;
        private const int SB_VERT = 0x1;
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int SB_THUMBPOSITION = 0x4;
        private const int SB_BOTTOM = 0x7;
        private const int SB_OFFSET = 13;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        [DllImport("user32.dll")]
        private static extern bool PostMessageA(IntPtr hWnd, int nBar, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        // Constants for extern calls to various scrollbar functions
        [DllImport("user32.dll")]
        static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

        private static void AppendTextToTextBox(TextBox textbox, string[] lines)
        {
            // Win32 magic to keep the textbox scrolling to the newest append to the textbox unless
            // the user has moved the scrollbox up
            int sbOffset = (int)((textbox.ClientSize.Height - SystemInformation.HorizontalScrollBarHeight) / (textbox.Font.Height));
            int savedVpos = GetScrollPos(textbox.Handle, SB_VERT);
            int savedHpos = GetScrollPos(textbox.Handle, SB_HORZ);

            int VSmin, VSmax;
            GetScrollRange(textbox.Handle, SB_VERT, out VSmin, out VSmax);

            bool bottomFlag = false;
            if (savedVpos >= (VSmax - sbOffset - 3))
                bottomFlag = true;

            int savedSelectionStart = textbox.SelectionStart;
            int savedSelectionLenght = textbox.SelectionLength;
            textbox.Lines = lines.ToArray();
            textbox.SelectionStart = savedSelectionStart;
            textbox.SelectionLength = savedSelectionLenght;

            if (bottomFlag)
            {
                GetScrollRange(textbox.Handle, SB_VERT, out VSmin, out VSmax);
                savedVpos = VSmax - sbOffset;
            }

            // Resume horizontal scroll
            SetScrollPos(textbox.Handle, SB_HORZ, savedHpos, true);
            PostMessageA(textbox.Handle, WM_HSCROLL, SB_THUMBPOSITION + 0x10000 * savedHpos, 0);
            
            // Resume/Set vertical scroll
            SetScrollPos(textbox.Handle, SB_VERT, savedVpos, true);
            PostMessageA(textbox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * savedVpos, 0);
        }
    }
}
