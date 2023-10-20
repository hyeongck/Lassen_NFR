namespace cWaferMap
{
    partial class frmWaferMap
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
            this.btnClose = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StripStatus_Location = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripStatus_Color = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripStatus_Tmp = new System.Windows.Forms.ToolStripStatusLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_BS50 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_BS75 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_BS100 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_BS125 = new System.Windows.Forms.ToolStripMenuItem();
            this.factorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Factor3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Factor5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Factor7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Factor9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Factor11 = new System.Windows.Forms.ToolStripMenuItem();
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_SelectDieInformation = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_Grid = new System.Windows.Forms.ToolStripMenuItem();
            this.group_SelectInformation = new System.Windows.Forms.GroupBox();
            this.txtSelect_Color = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSelect_LocY = new System.Windows.Forms.Label();
            this.lblSelect_LocX = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.group_SelectInformation.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(644, 629);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 32);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StripStatus_Location,
            this.StripStatus_Color,
            this.StripStatus_Tmp});
            this.statusStrip1.Location = new System.Drawing.Point(0, 670);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(752, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StripStatus_Location
            // 
            this.StripStatus_Location.AutoSize = false;
            this.StripStatus_Location.Name = "StripStatus_Location";
            this.StripStatus_Location.Size = new System.Drawing.Size(100, 17);
            this.StripStatus_Location.Text = "0,0";
            this.StripStatus_Location.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StripStatus_Color
            // 
            this.StripStatus_Color.AutoSize = false;
            this.StripStatus_Color.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.StripStatus_Color.Name = "StripStatus_Color";
            this.StripStatus_Color.Size = new System.Drawing.Size(50, 17);
            // 
            // StripStatus_Tmp
            // 
            this.StripStatus_Tmp.Name = "StripStatus_Tmp";
            this.StripStatus_Tmp.Size = new System.Drawing.Size(23, 17);
            this.StripStatus_Tmp.Text = "0,0";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(658, 590);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(82, 33);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem,
            this.zoomToolStripMenuItem,
            this.showToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(752, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.closeToolStripMenuItem.Text = "&Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // zoomToolStripMenuItem
            // 
            this.zoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableToolStripMenuItem,
            this.boxSizeToolStripMenuItem,
            this.factorToolStripMenuItem});
            this.zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
            this.zoomToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.zoomToolStripMenuItem.Text = "Zoom";
            // 
            // enableToolStripMenuItem
            // 
            this.enableToolStripMenuItem.Name = "enableToolStripMenuItem";
            this.enableToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.enableToolStripMenuItem.Text = "Enable";
            this.enableToolStripMenuItem.Click += new System.EventHandler(this.enableToolStripMenuItem_Click);
            // 
            // boxSizeToolStripMenuItem
            // 
            this.boxSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_BS50,
            this.toolStripMenuItem_BS75,
            this.toolStripMenuItem_BS100,
            this.toolStripMenuItem_BS125});
            this.boxSizeToolStripMenuItem.Name = "boxSizeToolStripMenuItem";
            this.boxSizeToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.boxSizeToolStripMenuItem.Text = "Box Size";
            // 
            // toolStripMenuItem_BS50
            // 
            this.toolStripMenuItem_BS50.Checked = true;
            this.toolStripMenuItem_BS50.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem_BS50.Name = "toolStripMenuItem_BS50";
            this.toolStripMenuItem_BS50.Size = new System.Drawing.Size(92, 22);
            this.toolStripMenuItem_BS50.Text = "50";
            this.toolStripMenuItem_BS50.Click += new System.EventHandler(this.toolStripMenuItem_BS50_Click);
            // 
            // toolStripMenuItem_BS75
            // 
            this.toolStripMenuItem_BS75.Name = "toolStripMenuItem_BS75";
            this.toolStripMenuItem_BS75.Size = new System.Drawing.Size(92, 22);
            this.toolStripMenuItem_BS75.Text = "75";
            this.toolStripMenuItem_BS75.Click += new System.EventHandler(this.toolStripMenuItem_BS75_Click);
            // 
            // toolStripMenuItem_BS100
            // 
            this.toolStripMenuItem_BS100.Name = "toolStripMenuItem_BS100";
            this.toolStripMenuItem_BS100.Size = new System.Drawing.Size(92, 22);
            this.toolStripMenuItem_BS100.Text = "100";
            this.toolStripMenuItem_BS100.Click += new System.EventHandler(this.toolStripMenuItem_BS100_Click);
            // 
            // toolStripMenuItem_BS125
            // 
            this.toolStripMenuItem_BS125.Name = "toolStripMenuItem_BS125";
            this.toolStripMenuItem_BS125.Size = new System.Drawing.Size(92, 22);
            this.toolStripMenuItem_BS125.Text = "125";
            this.toolStripMenuItem_BS125.Click += new System.EventHandler(this.toolStripMenuItem_BS125_Click);
            // 
            // factorToolStripMenuItem
            // 
            this.factorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Factor3,
            this.toolStripMenuItem_Factor5,
            this.toolStripMenuItem_Factor7,
            this.toolStripMenuItem_Factor9,
            this.toolStripMenuItem_Factor11});
            this.factorToolStripMenuItem.Name = "factorToolStripMenuItem";
            this.factorToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.factorToolStripMenuItem.Text = "Factor";
            // 
            // toolStripMenuItem_Factor3
            // 
            this.toolStripMenuItem_Factor3.Name = "toolStripMenuItem_Factor3";
            this.toolStripMenuItem_Factor3.Size = new System.Drawing.Size(86, 22);
            this.toolStripMenuItem_Factor3.Text = "3";
            this.toolStripMenuItem_Factor3.Click += new System.EventHandler(this.toolStripMenuItem_Factor3_Click);
            // 
            // toolStripMenuItem_Factor5
            // 
            this.toolStripMenuItem_Factor5.Checked = true;
            this.toolStripMenuItem_Factor5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem_Factor5.Name = "toolStripMenuItem_Factor5";
            this.toolStripMenuItem_Factor5.Size = new System.Drawing.Size(86, 22);
            this.toolStripMenuItem_Factor5.Text = "5";
            this.toolStripMenuItem_Factor5.Click += new System.EventHandler(this.toolStripMenuItem_Factor5_Click);
            // 
            // toolStripMenuItem_Factor7
            // 
            this.toolStripMenuItem_Factor7.Name = "toolStripMenuItem_Factor7";
            this.toolStripMenuItem_Factor7.Size = new System.Drawing.Size(86, 22);
            this.toolStripMenuItem_Factor7.Text = "7";
            this.toolStripMenuItem_Factor7.Click += new System.EventHandler(this.toolStripMenuItem_Factor7_Click);
            // 
            // toolStripMenuItem_Factor9
            // 
            this.toolStripMenuItem_Factor9.Name = "toolStripMenuItem_Factor9";
            this.toolStripMenuItem_Factor9.Size = new System.Drawing.Size(86, 22);
            this.toolStripMenuItem_Factor9.Text = "9";
            this.toolStripMenuItem_Factor9.Click += new System.EventHandler(this.toolStripMenuItem_Factor9_Click);
            // 
            // toolStripMenuItem_Factor11
            // 
            this.toolStripMenuItem_Factor11.Name = "toolStripMenuItem_Factor11";
            this.toolStripMenuItem_Factor11.Size = new System.Drawing.Size(86, 22);
            this.toolStripMenuItem_Factor11.Text = "11";
            this.toolStripMenuItem_Factor11.Click += new System.EventHandler(this.toolStripMenuItem_Factor11_Click);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_SelectDieInformation,
            this.ToolStripMenuItem_Grid});
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.showToolStripMenuItem.Text = "Show";
            // 
            // ToolStripMenuItem_SelectDieInformation
            // 
            this.ToolStripMenuItem_SelectDieInformation.Checked = true;
            this.ToolStripMenuItem_SelectDieInformation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolStripMenuItem_SelectDieInformation.Name = "ToolStripMenuItem_SelectDieInformation";
            this.ToolStripMenuItem_SelectDieInformation.Size = new System.Drawing.Size(148, 22);
            this.ToolStripMenuItem_SelectDieInformation.Text = "Die Information";
            this.ToolStripMenuItem_SelectDieInformation.Click += new System.EventHandler(this.ToolStripMenuItem_SelectDieInformation_Click);
            // 
            // ToolStripMenuItem_Grid
            // 
            this.ToolStripMenuItem_Grid.Checked = true;
            this.ToolStripMenuItem_Grid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolStripMenuItem_Grid.Name = "ToolStripMenuItem_Grid";
            this.ToolStripMenuItem_Grid.Size = new System.Drawing.Size(148, 22);
            this.ToolStripMenuItem_Grid.Text = "Grid";
            this.ToolStripMenuItem_Grid.Click += new System.EventHandler(this.ToolStripMenuItem_Grid_Click);
            // 
            // group_SelectInformation
            // 
            this.group_SelectInformation.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.group_SelectInformation.Controls.Add(this.txtSelect_Color);
            this.group_SelectInformation.Controls.Add(this.label4);
            this.group_SelectInformation.Controls.Add(this.lblSelect_LocY);
            this.group_SelectInformation.Controls.Add(this.lblSelect_LocX);
            this.group_SelectInformation.Controls.Add(this.label3);
            this.group_SelectInformation.Controls.Add(this.label2);
            this.group_SelectInformation.Controls.Add(this.label1);
            this.group_SelectInformation.Location = new System.Drawing.Point(631, 27);
            this.group_SelectInformation.Name = "group_SelectInformation";
            this.group_SelectInformation.Size = new System.Drawing.Size(115, 139);
            this.group_SelectInformation.TabIndex = 5;
            this.group_SelectInformation.TabStop = false;
            this.group_SelectInformation.Text = "Die Information";
            // 
            // txtSelect_Color
            // 
            this.txtSelect_Color.Location = new System.Drawing.Point(69, 92);
            this.txtSelect_Color.Name = "txtSelect_Color";
            this.txtSelect_Color.Size = new System.Drawing.Size(40, 20);
            this.txtSelect_Color.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Bin Color";
            // 
            // lblSelect_LocY
            // 
            this.lblSelect_LocY.AutoSize = true;
            this.lblSelect_LocY.Location = new System.Drawing.Point(53, 70);
            this.lblSelect_LocY.Name = "lblSelect_LocY";
            this.lblSelect_LocY.Size = new System.Drawing.Size(16, 13);
            this.lblSelect_LocY.TabIndex = 4;
            this.lblSelect_LocY.Text = "-1";
            // 
            // lblSelect_LocX
            // 
            this.lblSelect_LocX.AutoSize = true;
            this.lblSelect_LocX.Location = new System.Drawing.Point(53, 52);
            this.lblSelect_LocX.Name = "lblSelect_LocX";
            this.lblSelect_LocX.Size = new System.Drawing.Size(19, 13);
            this.lblSelect_LocX.TabIndex = 3;
            this.lblSelect_LocX.Text = "-1:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Y :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "X :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Position";
            // 
            // frmWaferMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 692);
            this.ControlBox = false;
            this.Controls.Add(this.group_SelectInformation);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmWaferMap";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "frmWaferMap";
            this.Load += new System.EventHandler(this.frmWaferMap_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.frmWaferMap_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.frmWaferMap_MouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmWaferMap_MouseMove);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.group_SelectInformation.ResumeLayout(false);
            this.group_SelectInformation.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StripStatus_Location;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boxSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_BS50;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_BS75;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_BS100;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_BS125;
        private System.Windows.Forms.ToolStripMenuItem factorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Factor3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Factor5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Factor7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Factor9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Factor11;
        private System.Windows.Forms.GroupBox group_SelectInformation;
        private System.Windows.Forms.Label lblSelect_LocY;
        private System.Windows.Forms.Label lblSelect_LocX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSelect_Color;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripStatusLabel StripStatus_Color;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_SelectDieInformation;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_Grid;
        private System.Windows.Forms.ToolStripStatusLabel StripStatus_Tmp;
    }
}