namespace tu_SET
{
    partial class Main
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.ctlConnectionSettings1 = new PollingLibraries.LibPorts.CtlConnectionSettings();
            this.ctlMeters1 = new Drivers.LibMeter.ctlMeters();
            this.btnTest = new System.Windows.Forms.Button();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.ctlSelectDriver1 = new tu_set.CtlSelectDriver();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // ctlConnectionSettings1
            // 
            this.ctlConnectionSettings1.AccessibleRole = System.Windows.Forms.AccessibleRole.ScrollBar;
            this.ctlConnectionSettings1.COM = null;
            this.ctlConnectionSettings1.GSM = false;
            this.ctlConnectionSettings1.IP = null;
            this.ctlConnectionSettings1.IsTCPSelected = false;
            this.ctlConnectionSettings1.Location = new System.Drawing.Point(12, 12);
            this.ctlConnectionSettings1.Name = "ctlConnectionSettings1";
            this.ctlConnectionSettings1.Size = new System.Drawing.Size(292, 400);
            this.ctlConnectionSettings1.TabIndex = 2;
            this.ctlConnectionSettings1.TCPPort = "0";
            // 
            // ctlMeters1
            // 
            this.ctlMeters1.AddressMeter = ((uint)(1u));
            this.ctlMeters1.AddressParam = 1;
            this.ctlMeters1.ChannelParam = 1;
            this.ctlMeters1.EnableAuxilary = true;
            this.ctlMeters1.EnableCurrent = true;
            this.ctlMeters1.Enabled = false;
            this.ctlMeters1.EnableDaily = true;
            this.ctlMeters1.EnableHalfs = true;
            this.ctlMeters1.EnableMonthly = true;
            this.ctlMeters1.Location = new System.Drawing.Point(310, 93);
            this.ctlMeters1.Name = "ctlMeters1";
            this.ctlMeters1.PasswordMeter = "000000";
            this.ctlMeters1.Size = new System.Drawing.Size(930, 268);
            this.ctlMeters1.TabIndex = 1;
            this.ctlMeters1.Load += new System.EventHandler(this.ctlMeters1_Load);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(1122, 367);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(105, 34);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "TEST";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxLogo.Image = global::tu_set.Properties.Resources.pi_logo_2;
            this.pictureBoxLogo.InitialImage = null;
            this.pictureBoxLogo.Location = new System.Drawing.Point(1141, 12);
            this.pictureBoxLogo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(86, 76);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabIndex = 51;
            this.pictureBoxLogo.TabStop = false;
            this.pictureBoxLogo.Click += new System.EventHandler(this.pictureBoxLogo_Click);
            // 
            // ctlSelectDriver1
            // 
            this.ctlSelectDriver1.Location = new System.Drawing.Point(310, 12);
            this.ctlSelectDriver1.Name = "ctlSelectDriver1";
            this.ctlSelectDriver1.PredefinedDriverName = "SET";
            this.ctlSelectDriver1.Size = new System.Drawing.Size(362, 75);
            this.ctlSelectDriver1.TabIndex = 4;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1244, 420);
            this.Controls.Add(this.pictureBoxLogo);
            this.Controls.Add(this.ctlSelectDriver1);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.ctlConnectionSettings1);
            this.Controls.Add(this.ctlMeters1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "Тестовая утилита SET4TM";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Drivers.LibMeter.ctlMeters ctlMeters1;
        private PollingLibraries.LibPorts.CtlConnectionSettings ctlConnectionSettings1;
        private System.Windows.Forms.Button btnTest;
        private tu_set.CtlSelectDriver ctlSelectDriver1;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
    }
}

