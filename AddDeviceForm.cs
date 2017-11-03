using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            this.lblConnectionStatus.Text = await webDav.ExistAsync("/") ? "Connected" : "Failed to connect";
            this.btnConnect.Enabled = true;
        }
    }
}
