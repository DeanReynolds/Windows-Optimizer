namespace Windows_Optimizer {
    partial class MainWindow {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.btnStart = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dcrewLabel = new System.Windows.Forms.LinkLabel();
            this.freethyLabel = new System.Windows.Forms.LinkLabel();
            this.tornixLabel = new System.Windows.Forms.LinkLabel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.freethyIcon = new System.Windows.Forms.PictureBox();
            this.dcrewIcon = new System.Windows.Forms.PictureBox();
            this.btnUncheck = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.freethyIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dcrewIcon)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(315, 86);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(72, 29);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Fix All";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "warn");
            this.imageList1.Images.SetKeyName(1, "check");
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(67, 147);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(282, 34);
            this.panel1.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(205, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "v1.1-alpha.0";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.DimGray;
            this.label2.Location = new System.Drawing.Point(0, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(196, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Uncheck an item to ignore/not fix it";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.DimGray;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hover over an item to see what it does";
            // 
            // dcrewLabel
            // 
            this.dcrewLabel.AutoSize = true;
            this.dcrewLabel.BackColor = System.Drawing.Color.Transparent;
            this.dcrewLabel.Location = new System.Drawing.Point(92, 1);
            this.dcrewLabel.Name = "dcrewLabel";
            this.dcrewLabel.Size = new System.Drawing.Size(40, 15);
            this.dcrewLabel.TabIndex = 9;
            this.dcrewLabel.TabStop = true;
            this.dcrewLabel.Text = "Dcrew";
            this.dcrewLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.dcrewLabel_LinkClicked);
            // 
            // freethyLabel
            // 
            this.freethyLabel.AutoSize = true;
            this.freethyLabel.BackColor = System.Drawing.Color.Transparent;
            this.freethyLabel.Location = new System.Drawing.Point(92, 18);
            this.freethyLabel.Name = "freethyLabel";
            this.freethyLabel.Size = new System.Drawing.Size(54, 15);
            this.freethyLabel.TabIndex = 8;
            this.freethyLabel.TabStop = true;
            this.freethyLabel.Text = "FR33THY";
            this.freethyLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.freethyLabel_LinkClicked);
            // 
            // tornixLabel
            // 
            this.tornixLabel.AutoSize = true;
            this.tornixLabel.BackColor = System.Drawing.Color.Transparent;
            this.tornixLabel.Location = new System.Drawing.Point(28, 18);
            this.tornixLabel.Name = "tornixLabel";
            this.tornixLabel.Size = new System.Drawing.Size(40, 15);
            this.tornixLabel.TabIndex = 3;
            this.tornixLabel.TabStop = true;
            this.tornixLabel.Text = "TorniX";
            this.tornixLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.tornixLabel_LinkClicked);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(12, 16);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(16, 16);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 7;
            this.pictureBox3.TabStop = false;
            // 
            // freethyIcon
            // 
            this.freethyIcon.Image = ((System.Drawing.Image)(resources.GetObject("freethyIcon.Image")));
            this.freethyIcon.Location = new System.Drawing.Point(74, 17);
            this.freethyIcon.Name = "freethyIcon";
            this.freethyIcon.Size = new System.Drawing.Size(16, 16);
            this.freethyIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.freethyIcon.TabIndex = 5;
            this.freethyIcon.TabStop = false;
            // 
            // dcrewIcon
            // 
            this.dcrewIcon.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.dcrewIcon.Image = ((System.Drawing.Image)(resources.GetObject("dcrewIcon.Image")));
            this.dcrewIcon.Location = new System.Drawing.Point(74, 0);
            this.dcrewIcon.Name = "dcrewIcon";
            this.dcrewIcon.Size = new System.Drawing.Size(16, 16);
            this.dcrewIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.dcrewIcon.TabIndex = 3;
            this.dcrewIcon.TabStop = false;
            // 
            // btnUncheck
            // 
            this.btnUncheck.Location = new System.Drawing.Point(393, 86);
            this.btnUncheck.Name = "btnUncheck";
            this.btnUncheck.Size = new System.Drawing.Size(86, 29);
            this.btnUncheck.TabIndex = 3;
            this.btnUncheck.Text = "Uncheck All";
            this.btnUncheck.UseVisualStyleBackColor = true;
            this.btnUncheck.Click += new System.EventHandler(this.btnUncheck_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.freethyLabel);
            this.panel2.Controls.Add(this.dcrewLabel);
            this.panel2.Controls.Add(this.dcrewIcon);
            this.panel2.Controls.Add(this.freethyIcon);
            this.panel2.Controls.Add(this.pictureBox3);
            this.panel2.Controls.Add(this.tornixLabel);
            this.panel2.Location = new System.Drawing.Point(490, 338);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(152, 36);
            this.panel2.TabIndex = 10;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 375);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btnUncheck);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Windows Optimizer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.freethyIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dcrewIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Button btnStart;
        private ImageList imageList1;
        private Panel panel1;
        private Label label2;
        private Label label1;
        private PictureBox dcrewIcon;
        private PictureBox freethyIcon;
        private PictureBox pictureBox3;
        private LinkLabel dcrewLabel;
        private LinkLabel freethyLabel;
        private LinkLabel tornixLabel;
        private Label label3;
        private Button btnUncheck;
        private Panel panel2;
    }
}