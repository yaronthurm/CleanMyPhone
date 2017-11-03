namespace CleanMyPhone
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this._notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmbDevices = new System.Windows.Forms.ComboBox();
            this.panelSettings = new System.Windows.Forms.FlowLayoutPanel();
            this.txtRollingLog = new System.Windows.Forms.TextBox();
            this.btnSaveChanges = new System.Windows.Forms.Button();
            this.btnAddDevice = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _notifyIcon
            // 
            this._notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this._notifyIcon.Text = "CleanMyPhone";
            this._notifyIcon.Visible = true;
            this._notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // cmbDevices
            // 
            this.cmbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDevices.FormattingEnabled = true;
            this.cmbDevices.Location = new System.Drawing.Point(12, 12);
            this.cmbDevices.Name = "cmbDevices";
            this.cmbDevices.Size = new System.Drawing.Size(347, 21);
            this.cmbDevices.TabIndex = 0;
            this.cmbDevices.SelectedIndexChanged += new System.EventHandler(this.cmbDevices_SelectedIndexChanged);
            // 
            // panelSettings
            // 
            this.panelSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelSettings.AutoScroll = true;
            this.panelSettings.Location = new System.Drawing.Point(12, 53);
            this.panelSettings.Name = "panelSettings";
            this.panelSettings.Size = new System.Drawing.Size(347, 338);
            this.panelSettings.TabIndex = 10;
            // 
            // txtRollingLog
            // 
            this.txtRollingLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRollingLog.Location = new System.Drawing.Point(413, 53);
            this.txtRollingLog.Multiline = true;
            this.txtRollingLog.Name = "txtRollingLog";
            this.txtRollingLog.ReadOnly = true;
            this.txtRollingLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRollingLog.Size = new System.Drawing.Size(348, 338);
            this.txtRollingLog.TabIndex = 11;
            this.txtRollingLog.WordWrap = false;
            // 
            // btnSaveChanges
            // 
            this.btnSaveChanges.Enabled = false;
            this.btnSaveChanges.Location = new System.Drawing.Point(12, 416);
            this.btnSaveChanges.Name = "btnSaveChanges";
            this.btnSaveChanges.Size = new System.Drawing.Size(90, 23);
            this.btnSaveChanges.TabIndex = 12;
            this.btnSaveChanges.Text = "Save changes";
            this.btnSaveChanges.UseVisualStyleBackColor = true;
            this.btnSaveChanges.Click += new System.EventHandler(this.btnSaveChanges_Click);
            // 
            // btnAddDevice
            // 
            this.btnAddDevice.Location = new System.Drawing.Point(377, 12);
            this.btnAddDevice.Name = "btnAddDevice";
            this.btnAddDevice.Size = new System.Drawing.Size(75, 23);
            this.btnAddDevice.TabIndex = 14;
            this.btnAddDevice.Text = "Add device";
            this.btnAddDevice.UseVisualStyleBackColor = true;
            this.btnAddDevice.Click += new System.EventHandler(this.btnAddDevice_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.btnAddDevice);
            this.Controls.Add(this.btnSaveChanges);
            this.Controls.Add(this.txtRollingLog);
            this.Controls.Add(this.panelSettings);
            this.Controls.Add(this.cmbDevices);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private System.Windows.Forms.ComboBox cmbDevices;
        private System.Windows.Forms.FlowLayoutPanel panelSettings;
        private System.Windows.Forms.TextBox txtRollingLog;
        private System.Windows.Forms.Button btnSaveChanges;
        private System.Windows.Forms.Button btnAddDevice;
    }
}