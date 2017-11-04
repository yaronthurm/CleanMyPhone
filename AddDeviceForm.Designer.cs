namespace CleanMyPhone
{
    partial class AddDeviceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddDeviceForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.numericPort = new System.Windows.Forms.NumericUpDown();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.panelRestOfSettings = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.chkEnableDelete = new System.Windows.Forms.CheckBox();
            this.txtDestinationFolder = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSourceFolder = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numericHighThreshold = new System.Windows.Forms.NumericUpDown();
            this.numericLowThreshold = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.numericIdleTime = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtDeviceID = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.lblSaveStatus = new System.Windows.Forms.Label();
            this.panelBasicSettings = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).BeginInit();
            this.panelRestOfSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericHighThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLowThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericIdleTime)).BeginInit();
            this.panelBasicSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(349, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Turn on the WebDav Server on your device and enter the following data";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Current IP address of the device";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(183, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Port";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Username";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(180, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Password";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(3, 26);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(159, 20);
            this.txtIP.TabIndex = 5;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(3, 73);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(157, 20);
            this.txtUsername.TabIndex = 6;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(180, 73);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(157, 20);
            this.txtPassword.TabIndex = 7;
            // 
            // numericPort
            // 
            this.numericPort.Location = new System.Drawing.Point(183, 26);
            this.numericPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericPort.Name = "numericPort";
            this.numericPort.Size = new System.Drawing.Size(154, 20);
            this.numericPort.TabIndex = 8;
            this.numericPort.Value = new decimal(new int[] {
            8080,
            0,
            0,
            0});
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(15, 137);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(110, 23);
            this.btnConnect.TabIndex = 9;
            this.btnConnect.Text = "Connect to device";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Location = new System.Drawing.Point(131, 142);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(22, 13);
            this.lblConnectionStatus.TabIndex = 10;
            this.lblConnectionStatus.Text = "NA";
            this.lblConnectionStatus.Visible = false;
            // 
            // panelRestOfSettings
            // 
            this.panelRestOfSettings.Controls.Add(this.lblSaveStatus);
            this.panelRestOfSettings.Controls.Add(this.txtDeviceID);
            this.panelRestOfSettings.Controls.Add(this.label11);
            this.panelRestOfSettings.Controls.Add(this.btnSave);
            this.panelRestOfSettings.Controls.Add(this.numericIdleTime);
            this.panelRestOfSettings.Controls.Add(this.label10);
            this.panelRestOfSettings.Controls.Add(this.numericLowThreshold);
            this.panelRestOfSettings.Controls.Add(this.label9);
            this.panelRestOfSettings.Controls.Add(this.numericHighThreshold);
            this.panelRestOfSettings.Controls.Add(this.label8);
            this.panelRestOfSettings.Controls.Add(this.chkEnableDelete);
            this.panelRestOfSettings.Controls.Add(this.txtDestinationFolder);
            this.panelRestOfSettings.Controls.Add(this.label7);
            this.panelRestOfSettings.Controls.Add(this.txtSourceFolder);
            this.panelRestOfSettings.Controls.Add(this.label6);
            this.panelRestOfSettings.Location = new System.Drawing.Point(15, 166);
            this.panelRestOfSettings.Name = "panelRestOfSettings";
            this.panelRestOfSettings.Size = new System.Drawing.Size(345, 347);
            this.panelRestOfSettings.TabIndex = 18;
            this.panelRestOfSettings.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 168);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(204, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "Enter High threshod [MB] (10,000-50,000)";
            // 
            // chkEnableDelete
            // 
            this.chkEnableDelete.AutoSize = true;
            this.chkEnableDelete.Location = new System.Drawing.Point(3, 144);
            this.chkEnableDelete.Name = "chkEnableDelete";
            this.chkEnableDelete.Size = new System.Drawing.Size(101, 17);
            this.chkEnableDelete.TabIndex = 22;
            this.chkEnableDelete.Text = "Enable Deleting";
            this.chkEnableDelete.UseVisualStyleBackColor = true;
            // 
            // txtDestinationFolder
            // 
            this.txtDestinationFolder.Location = new System.Drawing.Point(3, 117);
            this.txtDestinationFolder.Name = "txtDestinationFolder";
            this.txtDestinationFolder.Size = new System.Drawing.Size(328, 20);
            this.txtDestinationFolder.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 97);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Enter destination folder";
            // 
            // txtSourceFolder
            // 
            this.txtSourceFolder.Location = new System.Drawing.Point(3, 70);
            this.txtSourceFolder.Name = "txtSourceFolder";
            this.txtSourceFolder.Size = new System.Drawing.Size(328, 20);
            this.txtSourceFolder.TabIndex = 19;
            this.txtSourceFolder.Text = "/DCIM/Camera";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(141, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Source Folder on the device";
            // 
            // numericHighThreshold
            // 
            this.numericHighThreshold.Location = new System.Drawing.Point(3, 188);
            this.numericHighThreshold.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericHighThreshold.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericHighThreshold.Name = "numericHighThreshold";
            this.numericHighThreshold.Size = new System.Drawing.Size(125, 20);
            this.numericHighThreshold.TabIndex = 24;
            this.numericHighThreshold.Value = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            // 
            // numericLowThreshold
            // 
            this.numericLowThreshold.Location = new System.Drawing.Point(3, 235);
            this.numericLowThreshold.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericLowThreshold.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericLowThreshold.Name = "numericLowThreshold";
            this.numericLowThreshold.Size = new System.Drawing.Size(125, 20);
            this.numericLowThreshold.TabIndex = 26;
            this.numericLowThreshold.Value = new decimal(new int[] {
            19000,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 215);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(204, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Enter Low threshold [MB] (10,000-50,000)";
            // 
            // numericIdleTime
            // 
            this.numericIdleTime.Location = new System.Drawing.Point(3, 282);
            this.numericIdleTime.Maximum = new decimal(new int[] {
            1800,
            0,
            0,
            0});
            this.numericIdleTime.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numericIdleTime.Name = "numericIdleTime";
            this.numericIdleTime.Size = new System.Drawing.Size(125, 20);
            this.numericIdleTime.TabIndex = 28;
            this.numericIdleTime.Value = new decimal(new int[] {
            180,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 262);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(182, 13);
            this.label10.TabIndex = 27;
            this.label10.Text = "Enter Idle time in seconds (180-1800)";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(3, 316);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 23);
            this.btnSave.TabIndex = 19;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtDeviceID
            // 
            this.txtDeviceID.Location = new System.Drawing.Point(3, 25);
            this.txtDeviceID.Name = "txtDeviceID";
            this.txtDeviceID.Size = new System.Drawing.Size(328, 20);
            this.txtDeviceID.TabIndex = 30;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(104, 13);
            this.label11.TabIndex = 29;
            this.label11.Text = "Chose the device ID";
            // 
            // lblSaveStatus
            // 
            this.lblSaveStatus.AutoSize = true;
            this.lblSaveStatus.Location = new System.Drawing.Point(122, 321);
            this.lblSaveStatus.Name = "lblSaveStatus";
            this.lblSaveStatus.Size = new System.Drawing.Size(22, 13);
            this.lblSaveStatus.TabIndex = 19;
            this.lblSaveStatus.Text = "NA";
            this.lblSaveStatus.Visible = false;
            // 
            // panelBasicSettings
            // 
            this.panelBasicSettings.Controls.Add(this.txtIP);
            this.panelBasicSettings.Controls.Add(this.label2);
            this.panelBasicSettings.Controls.Add(this.label3);
            this.panelBasicSettings.Controls.Add(this.label4);
            this.panelBasicSettings.Controls.Add(this.numericPort);
            this.panelBasicSettings.Controls.Add(this.label5);
            this.panelBasicSettings.Controls.Add(this.txtPassword);
            this.panelBasicSettings.Controls.Add(this.txtUsername);
            this.panelBasicSettings.Location = new System.Drawing.Point(15, 27);
            this.panelBasicSettings.Name = "panelBasicSettings";
            this.panelBasicSettings.Size = new System.Drawing.Size(345, 106);
            this.panelBasicSettings.TabIndex = 19;
            // 
            // AddDeviceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 525);
            this.Controls.Add(this.panelBasicSettings);
            this.Controls.Add(this.panelRestOfSettings);
            this.Controls.Add(this.lblConnectionStatus);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AddDeviceForm";
            this.Text = "AddDeviceForm";
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).EndInit();
            this.panelRestOfSettings.ResumeLayout(false);
            this.panelRestOfSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericHighThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLowThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericIdleTime)).EndInit();
            this.panelBasicSettings.ResumeLayout(false);
            this.panelBasicSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.NumericUpDown numericPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Panel panelRestOfSettings;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkEnableDelete;
        private System.Windows.Forms.TextBox txtDestinationFolder;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtSourceFolder;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericHighThreshold;
        private System.Windows.Forms.NumericUpDown numericLowThreshold;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericIdleTime;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtDeviceID;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblSaveStatus;
        private System.Windows.Forms.Panel panelBasicSettings;
    }
}