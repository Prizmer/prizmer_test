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
            this.ctlConnectionSettings1 = new PollingLibraries.LibPorts.CtlConnectionSettings();
            this.ctlMeters1 = new Drivers.LibMeter.ctlMeters();
            this.SuspendLayout();
            // 
            // ctlConnectionSettings1
            // 
            this.ctlConnectionSettings1.AccessibleRole = System.Windows.Forms.AccessibleRole.ScrollBar;
            this.ctlConnectionSettings1.COM = null;
            this.ctlConnectionSettings1.GSM = false;
            this.ctlConnectionSettings1.IP = null;
            this.ctlConnectionSettings1.IsTCPSelected = false;
            this.ctlConnectionSettings1.Location = new System.Drawing.Point(10, 12);
            this.ctlConnectionSettings1.Name = "ctlConnectionSettings1";
            this.ctlConnectionSettings1.Size = new System.Drawing.Size(292, 396);
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
            this.ctlMeters1.Location = new System.Drawing.Point(308, 12);
            this.ctlMeters1.Name = "ctlMeters1";
            this.ctlMeters1.PasswordMeter = "000000";
            this.ctlMeters1.Size = new System.Drawing.Size(930, 268);
            this.ctlMeters1.TabIndex = 1;
            this.ctlMeters1.Load += new System.EventHandler(this.ctlMeters1_Load);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1239, 416);
            this.Controls.Add(this.ctlConnectionSettings1);
            this.Controls.Add(this.ctlMeters1);
            this.Name = "Main";
            this.Text = "Тестовая утилита SET4TM";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private Drivers.LibMeter.ctlMeters ctlMeters1;
        private PollingLibraries.LibPorts.CtlConnectionSettings ctlConnectionSettings1;
    }
}

