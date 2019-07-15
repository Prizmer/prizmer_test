namespace tu_set
{
    partial class CtlSelectDriver
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gbDriver = new System.Windows.Forms.GroupBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.cbDriverName = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.gbDriver.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDriver
            // 
            this.gbDriver.Controls.Add(this.btnApply);
            this.gbDriver.Controls.Add(this.cbDriverName);
            this.gbDriver.Location = new System.Drawing.Point(3, 3);
            this.gbDriver.Name = "gbDriver";
            this.gbDriver.Size = new System.Drawing.Size(350, 66);
            this.gbDriver.TabIndex = 0;
            this.gbDriver.TabStop = false;
            this.gbDriver.Text = "(1) Драйвер";
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(236, 19);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(107, 34);
            this.btnApply.TabIndex = 1;
            this.btnApply.Text = "Применить";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // cbDriverName
            // 
            this.cbDriverName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDriverName.FormattingEnabled = true;
            this.cbDriverName.Location = new System.Drawing.Point(6, 25);
            this.cbDriverName.Name = "cbDriverName";
            this.cbDriverName.Size = new System.Drawing.Size(210, 24);
            this.cbDriverName.TabIndex = 0;
            this.toolTip1.SetToolTip(this.cbDriverName, "Выберите драйвер из списка и нажмите применить");
            // 
            // CtlSelectDriver
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbDriver);
            this.Name = "CtlSelectDriver";
            this.Size = new System.Drawing.Size(358, 75);
            this.Load += new System.EventHandler(this.CtlSelectDriver_Load);
            this.gbDriver.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbDriver;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.ComboBox cbDriverName;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
