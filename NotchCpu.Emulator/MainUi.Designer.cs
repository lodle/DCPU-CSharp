namespace NotchCpu.Emulator
{
    partial class MainUi
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
            System.Windows.Forms.ColumnHeader Reg;
            System.Windows.Forms.ColumnHeader Val;
            this.TextGridView = new System.Windows.Forms.DataGridView();
            this.ButStartToggle = new System.Windows.Forms.Button();
            this.TBLog = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.ButStep = new System.Windows.Forms.Button();
            this.ButReset = new System.Windows.Forms.Button();
            this.CBSpeed = new System.Windows.Forms.ComboBox();
            this.TBSpeed = new System.Windows.Forms.TextBox();
            Reg = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            Val = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.TextGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // Reg
            // 
            Reg.Text = "Reg";
            Reg.Width = 35;
            // 
            // Val
            // 
            Val.Text = "Val";
            // 
            // TextGridView
            // 
            this.TextGridView.AllowUserToAddRows = false;
            this.TextGridView.AllowUserToDeleteRows = false;
            this.TextGridView.AllowUserToResizeColumns = false;
            this.TextGridView.AllowUserToResizeRows = false;
            this.TextGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.TextGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TextGridView.ColumnHeadersVisible = false;
            this.TextGridView.Location = new System.Drawing.Point(119, 13);
            this.TextGridView.MaximumSize = new System.Drawing.Size(485, 290);
            this.TextGridView.Name = "TextGridView";
            this.TextGridView.ReadOnly = true;
            this.TextGridView.RowHeadersVisible = false;
            this.TextGridView.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.TextGridView.Size = new System.Drawing.Size(485, 290);
            this.TextGridView.TabIndex = 0;
            // 
            // ButStartToggle
            // 
            this.ButStartToggle.Location = new System.Drawing.Point(610, 42);
            this.ButStartToggle.Name = "ButStartToggle";
            this.ButStartToggle.Size = new System.Drawing.Size(75, 23);
            this.ButStartToggle.TabIndex = 1;
            this.ButStartToggle.Text = "Start";
            this.ButStartToggle.UseVisualStyleBackColor = true;
            this.ButStartToggle.Click += new System.EventHandler(this.ButStartToggle_Click);
            // 
            // TBLog
            // 
            this.TBLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TBLog.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.TBLog.Location = new System.Drawing.Point(13, 309);
            this.TBLog.Multiline = true;
            this.TBLog.Name = "TBLog";
            this.TBLog.ReadOnly = true;
            this.TBLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TBLog.Size = new System.Drawing.Size(666, 136);
            this.TBLog.TabIndex = 2;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            Reg,
            Val});
            this.listView1.Location = new System.Drawing.Point(13, 13);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(100, 290);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // ButStep
            // 
            this.ButStep.Location = new System.Drawing.Point(611, 72);
            this.ButStep.Name = "ButStep";
            this.ButStep.Size = new System.Drawing.Size(75, 23);
            this.ButStep.TabIndex = 4;
            this.ButStep.Text = "Step";
            this.ButStep.UseVisualStyleBackColor = true;
            this.ButStep.Click += new System.EventHandler(this.ButStep_Click);
            // 
            // ButReset
            // 
            this.ButReset.Location = new System.Drawing.Point(611, 13);
            this.ButReset.Name = "ButReset";
            this.ButReset.Size = new System.Drawing.Size(75, 23);
            this.ButReset.TabIndex = 5;
            this.ButReset.Text = "Reset";
            this.ButReset.UseVisualStyleBackColor = true;
            this.ButReset.Click += new System.EventHandler(this.ButReset_Click);
            // 
            // CBSpeed
            // 
            this.CBSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBSpeed.FormattingEnabled = true;
            this.CBSpeed.Items.AddRange(new object[] {
            "200 khz",
            "100 kHz",
            "50 kHz",
            "10 kHz",
            "1   kHz",
            "500 Hz",
            "10 Hz"});
            this.CBSpeed.Location = new System.Drawing.Point(611, 281);
            this.CBSpeed.Name = "CBSpeed";
            this.CBSpeed.Size = new System.Drawing.Size(68, 21);
            this.CBSpeed.TabIndex = 6;
            this.CBSpeed.SelectedIndexChanged += new System.EventHandler(this.OnSpeedChanged);
            // 
            // TBSpeed
            // 
            this.TBSpeed.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.TBSpeed.Location = new System.Drawing.Point(611, 255);
            this.TBSpeed.Name = "TBSpeed";
            this.TBSpeed.ReadOnly = true;
            this.TBSpeed.Size = new System.Drawing.Size(68, 20);
            this.TBSpeed.TabIndex = 7;
            // 
            // MainUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(691, 454);
            this.Controls.Add(this.TBSpeed);
            this.Controls.Add(this.CBSpeed);
            this.Controls.Add(this.ButReset);
            this.Controls.Add(this.ButStep);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.TBLog);
            this.Controls.Add(this.ButStartToggle);
            this.Controls.Add(this.TextGridView);
            this.Name = "MainUi";
            this.Text = "Notch Cpu Emulator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainUi_Close);
            this.Load += new System.EventHandler(this.MainUi_Load);
            ((System.ComponentModel.ISupportInitialize)(this.TextGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView TextGridView;
        private System.Windows.Forms.Button ButStartToggle;
        private System.Windows.Forms.TextBox TBLog;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button ButStep;
        private System.Windows.Forms.Button ButReset;
        private System.Windows.Forms.ComboBox CBSpeed;
        private System.Windows.Forms.TextBox TBSpeed;
    }
}