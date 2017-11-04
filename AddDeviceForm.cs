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
    public partial class AddDeviceForm : Form
    {
        public AddDeviceForm()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            this.lblConnectionStatus.Visible = true;
            this.lblConnectionStatus.Text = "Trying to connect";
            this.btnConnect.Enabled = false;
            await Task.Delay(100);
            var webDav = new WebDavFileManager(txtIP.Text, (int)numericPort.Value, txtUsername.Text, txtPassword.Text);
            var isConnected = await webDav.ExistAsync("/");
            this.lblConnectionStatus.Text = isConnected ? "Connected" : "Failed to connect";
            this.panelRestOfSettings.Visible = isConnected;
            this.panelBasicSettings.Enabled = !isConnected;
            this.btnConnect.Enabled = !isConnected;
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            this.btnSave.Enabled = false;
            this.panelRestOfSettings.Enabled = false;
            try {
                await DeviceSetup.SetupDeviceAsync(
                    GetAppFolder(),
                    this.txtDeviceID.Text,
                    this.txtIP.Text,
                    (int)this.numericPort.Value,
                    this.txtUsername.Text,
                    this.txtPassword.Text,
                    this.txtSourceFolder.Text,
                    this.txtDestinationFolder.Text,
                    (int)this.numericHighThreshold.Value,
                    (int)this.numericLowThreshold.Value,
                    (int)this.numericIdleTime.Value,
                    this.chkEnableDelete.Checked
                    );
                this.lblSaveStatus.Text = "Saved";
                this.lblSaveStatus.Visible = true;
                await Task.Delay(1000);
                this.Close();
            }
            catch
            {
                this.lblSaveStatus.Text = "Failed saving";
                this.lblSaveStatus.Visible = true;
            }
            this.btnSave.Enabled = true;
            this.panelRestOfSettings.Enabled = true;
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
    }
}
