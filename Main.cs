using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CleanMyPhone
{
    public partial class Main : Form
    {
        private string _appFolder;
        private Dictionary<string, CleanerSettings> _configs;

        public Main()
        {
            InitializeComponent();

            this.notifyIcon1.Icon = this.Icon;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.notifyIcon1.ShowBalloonTip(3000);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _appFolder = GetAppFolder();
            _configs = CleanerSettings.GetAllConfigs(_appFolder);

            foreach (var config in _configs)            
                this.comboBox1.Items.Add(config.Key);
            if (_configs.Keys.Count > 0)
                this.comboBox1.SelectedIndex = 0;
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
            var selectedDeviceID = this.comboBox1.SelectedItem.ToString();
            var selectedDeviceSettings = _configs[selectedDeviceID];

            foreach (var prop in typeof(CleanerSettings).GetProperties())
            {
                var name = prop.Name;
                var value = prop.GetValue(selectedDeviceSettings).ToString();
                var label = new Label() { Text = name, Margin = new Padding(0), Height = 15, BackColor = Color.LightBlue, Width = this.flowLayoutPanel1.Width };
                var txt = new TextBox() { Text = value, Margin = new Padding(0, 0, 0, 10), Width = this.flowLayoutPanel1.Width};
                this.flowLayoutPanel1.Controls.AddRange(new Control[] { label, txt });
            }
           
        }

        private void flowLayoutPanel1_Resize(object sender, EventArgs e)
        {
            foreach (Control control in this.flowLayoutPanel1.Controls){
                control.Width = this.flowLayoutPanel1.Width;
            }
        }
    }
}
