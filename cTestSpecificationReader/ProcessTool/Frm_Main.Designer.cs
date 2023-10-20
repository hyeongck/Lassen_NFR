namespace ProcessTool
{
    partial class frm_Main
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
            this.btnRegen = new System.Windows.Forms.Button();
            this.btnDelta = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.LblResult = new System.Windows.Forms.Label();
            this.btnResult = new System.Windows.Forms.Button();
            this.lblChkResult = new System.Windows.Forms.Label();
            this.btnXY = new System.Windows.Forms.Button();
            this.btnOutput = new System.Windows.Forms.Button();
            this.lblOutput = new System.Windows.Forms.Label();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkIncludeOriginal = new System.Windows.Forms.CheckBox();
            this.chkSelectHeader = new System.Windows.Forms.CheckBox();
            this.chk_ExtractXYLoc = new System.Windows.Forms.CheckBox();
            this.lblChkPrevResult = new System.Windows.Forms.Label();
            this.btnPrevResult = new System.Windows.Forms.Button();
            this.lblPrevResultFile = new System.Windows.Forms.Label();
            this.txtPrevResult = new System.Windows.Forms.TextBox();
            this.lblChkNewSpec = new System.Windows.Forms.Label();
            this.btnNewSpec = new System.Windows.Forms.Button();
            this.lblNewSpec = new System.Windows.Forms.Label();
            this.txtNewSpec = new System.Windows.Forms.TextBox();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.btnRetest = new System.Windows.Forms.Button();
            this.lblChkOutput = new System.Windows.Forms.Label();
            this.lblChkRascoXML = new System.Windows.Forms.Label();
            this.btnRascoXML = new System.Windows.Forms.Button();
            this.lblRascoXML = new System.Windows.Forms.Label();
            this.txtRascoXML = new System.Windows.Forms.TextBox();
            this.btnGenRascoXML = new System.Windows.Forms.Button();
            this.btnMerge = new System.Windows.Forms.Button();
            this.lblChkR3 = new System.Windows.Forms.Label();
            this.btnR3 = new System.Windows.Forms.Button();
            this.lblR3 = new System.Windows.Forms.Label();
            this.txtR3 = new System.Windows.Forms.TextBox();
            this.lblChkR4 = new System.Windows.Forms.Label();
            this.btnR4 = new System.Windows.Forms.Button();
            this.lblR4 = new System.Windows.Forms.Label();
            this.txtR4 = new System.Windows.Forms.TextBox();
            this.lblChkR5 = new System.Windows.Forms.Label();
            this.btnR5 = new System.Windows.Forms.Button();
            this.lblR5 = new System.Windows.Forms.Label();
            this.txtR5 = new System.Windows.Forms.TextBox();
            this.btnSDIRegen = new System.Windows.Forms.Button();
            this.lblChkTestCondition = new System.Windows.Forms.Label();
            this.btnTestCondition = new System.Windows.Forms.Button();
            this.lblTestCondition = new System.Windows.Forms.Label();
            this.txtTestCondition = new System.Windows.Forms.TextBox();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRegen
            // 
            this.btnRegen.Location = new System.Drawing.Point(713, 341);
            this.btnRegen.Name = "btnRegen";
            this.btnRegen.Size = new System.Drawing.Size(120, 52);
            this.btnRegen.TabIndex = 2;
            this.btnRegen.Text = "Regen";
            this.btnRegen.UseVisualStyleBackColor = true;
            this.btnRegen.Click += new System.EventHandler(this.btnRegen_Click);
            // 
            // btnDelta
            // 
            this.btnDelta.Location = new System.Drawing.Point(582, 341);
            this.btnDelta.Name = "btnDelta";
            this.btnDelta.Size = new System.Drawing.Size(120, 52);
            this.btnDelta.TabIndex = 3;
            this.btnDelta.Text = "Delta";
            this.btnDelta.UseVisualStyleBackColor = true;
            this.btnDelta.Click += new System.EventHandler(this.btnDelta_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(739, 512);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(96, 29);
            this.button5.TabIndex = 4;
            this.button5.Text = "Outlier";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(434, 617);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(117, 39);
            this.button6.TabIndex = 5;
            this.button6.Text = "XML Map";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(434, 538);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(116, 73);
            this.button7.TabIndex = 6;
            this.button7.Text = ".A Map";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(556, 539);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(114, 72);
            this.button8.TabIndex = 7;
            this.button8.Text = "Clear Map";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(841, 496);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(96, 37);
            this.button9.TabIndex = 8;
            this.button9.Text = "Result Map 1";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(181, 48);
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(559, 20);
            this.txtResult.TabIndex = 9;
            // 
            // LblResult
            // 
            this.LblResult.AutoSize = true;
            this.LblResult.Location = new System.Drawing.Point(20, 51);
            this.LblResult.Name = "LblResult";
            this.LblResult.Size = new System.Drawing.Size(142, 13);
            this.LblResult.TabIndex = 10;
            this.LblResult.Text = "Result Filename (reference) :";
            // 
            // btnResult
            // 
            this.btnResult.Location = new System.Drawing.Point(746, 45);
            this.btnResult.Name = "btnResult";
            this.btnResult.Size = new System.Drawing.Size(33, 25);
            this.btnResult.TabIndex = 11;
            this.btnResult.Text = "...";
            this.btnResult.UseVisualStyleBackColor = true;
            this.btnResult.Click += new System.EventHandler(this.button10_Click);
            // 
            // lblChkResult
            // 
            this.lblChkResult.AutoSize = true;
            this.lblChkResult.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkResult.ForeColor = System.Drawing.Color.Green;
            this.lblChkResult.Location = new System.Drawing.Point(785, 49);
            this.lblChkResult.Name = "lblChkResult";
            this.lblChkResult.Size = new System.Drawing.Size(26, 21);
            this.lblChkResult.TabIndex = 12;
            this.lblChkResult.Text = "P";
            this.lblChkResult.Visible = false;
            // 
            // btnXY
            // 
            this.btnXY.Location = new System.Drawing.Point(320, 341);
            this.btnXY.Name = "btnXY";
            this.btnXY.Size = new System.Drawing.Size(120, 52);
            this.btnXY.TabIndex = 17;
            this.btnXY.Text = "Tabulate XY location to New Result File";
            this.btnXY.UseVisualStyleBackColor = true;
            this.btnXY.Click += new System.EventHandler(this.btnXY_Click);
            // 
            // btnOutput
            // 
            this.btnOutput.Location = new System.Drawing.Point(746, 212);
            this.btnOutput.Name = "btnOutput";
            this.btnOutput.Size = new System.Drawing.Size(33, 25);
            this.btnOutput.TabIndex = 20;
            this.btnOutput.Text = "...";
            this.btnOutput.UseVisualStyleBackColor = true;
            this.btnOutput.Click += new System.EventHandler(this.btnOutput_Click);
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(20, 218);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(90, 13);
            this.lblOutput.TabIndex = 19;
            this.lblOutput.Text = "Output Filename :";
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(181, 215);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(559, 20);
            this.txtOutput.TabIndex = 18;
            this.txtOutput.TextChanged += new System.EventHandler(this.txtOutput_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkIncludeOriginal);
            this.groupBox1.Controls.Add(this.chkSelectHeader);
            this.groupBox1.Controls.Add(this.chk_ExtractXYLoc);
            this.groupBox1.Location = new System.Drawing.Point(818, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(177, 120);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Option";
            // 
            // chkIncludeOriginal
            // 
            this.chkIncludeOriginal.AutoSize = true;
            this.chkIncludeOriginal.Location = new System.Drawing.Point(23, 71);
            this.chkIncludeOriginal.Name = "chkIncludeOriginal";
            this.chkIncludeOriginal.Size = new System.Drawing.Size(125, 17);
            this.chkIncludeOriginal.TabIndex = 2;
            this.chkIncludeOriginal.Text = "Include Original Data";
            this.chkIncludeOriginal.UseVisualStyleBackColor = true;
            // 
            // chkSelectHeader
            // 
            this.chkSelectHeader.AutoSize = true;
            this.chkSelectHeader.Location = new System.Drawing.Point(23, 48);
            this.chkSelectHeader.Name = "chkSelectHeader";
            this.chkSelectHeader.Size = new System.Drawing.Size(94, 17);
            this.chkSelectHeader.TabIndex = 1;
            this.chkSelectHeader.Text = "Select Header";
            this.chkSelectHeader.UseVisualStyleBackColor = true;
            // 
            // chk_ExtractXYLoc
            // 
            this.chk_ExtractXYLoc.AutoSize = true;
            this.chk_ExtractXYLoc.Location = new System.Drawing.Point(23, 25);
            this.chk_ExtractXYLoc.Name = "chk_ExtractXYLoc";
            this.chk_ExtractXYLoc.Size = new System.Drawing.Size(116, 17);
            this.chk_ExtractXYLoc.TabIndex = 0;
            this.chk_ExtractXYLoc.Text = "Extract XY location";
            this.chk_ExtractXYLoc.UseVisualStyleBackColor = true;
            // 
            // lblChkPrevResult
            // 
            this.lblChkPrevResult.AutoSize = true;
            this.lblChkPrevResult.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkPrevResult.ForeColor = System.Drawing.Color.Green;
            this.lblChkPrevResult.Location = new System.Drawing.Point(785, 77);
            this.lblChkPrevResult.Name = "lblChkPrevResult";
            this.lblChkPrevResult.Size = new System.Drawing.Size(26, 21);
            this.lblChkPrevResult.TabIndex = 25;
            this.lblChkPrevResult.Text = "P";
            this.lblChkPrevResult.Visible = false;
            // 
            // btnPrevResult
            // 
            this.btnPrevResult.Location = new System.Drawing.Point(746, 73);
            this.btnPrevResult.Name = "btnPrevResult";
            this.btnPrevResult.Size = new System.Drawing.Size(33, 25);
            this.btnPrevResult.TabIndex = 24;
            this.btnPrevResult.Text = "...";
            this.btnPrevResult.UseVisualStyleBackColor = true;
            this.btnPrevResult.Click += new System.EventHandler(this.btnPrevResult_Click);
            // 
            // lblPrevResultFile
            // 
            this.lblPrevResultFile.Location = new System.Drawing.Point(20, 79);
            this.lblPrevResultFile.Name = "lblPrevResultFile";
            this.lblPrevResultFile.Size = new System.Drawing.Size(132, 13);
            this.lblPrevResultFile.TabIndex = 23;
            this.lblPrevResultFile.Text = "Result Filename 2 :";
            // 
            // txtPrevResult
            // 
            this.txtPrevResult.Location = new System.Drawing.Point(181, 76);
            this.txtPrevResult.Name = "txtPrevResult";
            this.txtPrevResult.Size = new System.Drawing.Size(559, 20);
            this.txtPrevResult.TabIndex = 22;
            // 
            // lblChkNewSpec
            // 
            this.lblChkNewSpec.AutoSize = true;
            this.lblChkNewSpec.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkNewSpec.ForeColor = System.Drawing.Color.Green;
            this.lblChkNewSpec.Location = new System.Drawing.Point(785, 20);
            this.lblChkNewSpec.Name = "lblChkNewSpec";
            this.lblChkNewSpec.Size = new System.Drawing.Size(26, 21);
            this.lblChkNewSpec.TabIndex = 29;
            this.lblChkNewSpec.Text = "P";
            this.lblChkNewSpec.Visible = false;
            // 
            // btnNewSpec
            // 
            this.btnNewSpec.Location = new System.Drawing.Point(746, 16);
            this.btnNewSpec.Name = "btnNewSpec";
            this.btnNewSpec.Size = new System.Drawing.Size(33, 25);
            this.btnNewSpec.TabIndex = 28;
            this.btnNewSpec.Text = "...";
            this.btnNewSpec.UseVisualStyleBackColor = true;
            this.btnNewSpec.Click += new System.EventHandler(this.btnNewSpec_Click);
            // 
            // lblNewSpec
            // 
            this.lblNewSpec.AutoSize = true;
            this.lblNewSpec.Location = new System.Drawing.Point(20, 22);
            this.lblNewSpec.Name = "lblNewSpec";
            this.lblNewSpec.Size = new System.Drawing.Size(118, 13);
            this.lblNewSpec.TabIndex = 27;
            this.lblNewSpec.Text = "New Specification File :";
            // 
            // txtNewSpec
            // 
            this.txtNewSpec.Location = new System.Drawing.Point(181, 19);
            this.txtNewSpec.Name = "txtNewSpec";
            this.txtNewSpec.Size = new System.Drawing.Size(559, 20);
            this.txtNewSpec.TabIndex = 26;
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(841, 539);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(96, 37);
            this.button11.TabIndex = 30;
            this.button11.Text = "Result Map 2";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(841, 582);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(96, 37);
            this.button12.TabIndex = 31;
            this.button12.Text = "Compare Map";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // btnRetest
            // 
            this.btnRetest.Location = new System.Drawing.Point(844, 341);
            this.btnRetest.Name = "btnRetest";
            this.btnRetest.Size = new System.Drawing.Size(120, 52);
            this.btnRetest.TabIndex = 32;
            this.btnRetest.Text = "Retest Files";
            this.btnRetest.UseVisualStyleBackColor = true;
            this.btnRetest.Click += new System.EventHandler(this.btnRetest_Click);
            // 
            // lblChkOutput
            // 
            this.lblChkOutput.AutoSize = true;
            this.lblChkOutput.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkOutput.ForeColor = System.Drawing.Color.Green;
            this.lblChkOutput.Location = new System.Drawing.Point(785, 216);
            this.lblChkOutput.Name = "lblChkOutput";
            this.lblChkOutput.Size = new System.Drawing.Size(26, 21);
            this.lblChkOutput.TabIndex = 33;
            this.lblChkOutput.Text = "P";
            this.lblChkOutput.Visible = false;
            // 
            // lblChkRascoXML
            // 
            this.lblChkRascoXML.AutoSize = true;
            this.lblChkRascoXML.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkRascoXML.ForeColor = System.Drawing.Color.Green;
            this.lblChkRascoXML.Location = new System.Drawing.Point(785, 260);
            this.lblChkRascoXML.Name = "lblChkRascoXML";
            this.lblChkRascoXML.Size = new System.Drawing.Size(26, 21);
            this.lblChkRascoXML.TabIndex = 37;
            this.lblChkRascoXML.Text = "P";
            this.lblChkRascoXML.Visible = false;
            // 
            // btnRascoXML
            // 
            this.btnRascoXML.Location = new System.Drawing.Point(746, 256);
            this.btnRascoXML.Name = "btnRascoXML";
            this.btnRascoXML.Size = new System.Drawing.Size(33, 25);
            this.btnRascoXML.TabIndex = 36;
            this.btnRascoXML.Text = "...";
            this.btnRascoXML.UseVisualStyleBackColor = true;
            this.btnRascoXML.Click += new System.EventHandler(this.btnRascoXML_Click);
            // 
            // lblRascoXML
            // 
            this.lblRascoXML.AutoSize = true;
            this.lblRascoXML.Location = new System.Drawing.Point(20, 262);
            this.lblRascoXML.Name = "lblRascoXML";
            this.lblRascoXML.Size = new System.Drawing.Size(132, 13);
            this.lblRascoXML.TabIndex = 35;
            this.lblRascoXML.Text = "RascoMap XML Filename:";
            // 
            // txtRascoXML
            // 
            this.txtRascoXML.Location = new System.Drawing.Point(181, 259);
            this.txtRascoXML.Name = "txtRascoXML";
            this.txtRascoXML.Size = new System.Drawing.Size(559, 20);
            this.txtRascoXML.TabIndex = 34;
            // 
            // btnGenRascoXML
            // 
            this.btnGenRascoXML.Location = new System.Drawing.Point(189, 341);
            this.btnGenRascoXML.Name = "btnGenRascoXML";
            this.btnGenRascoXML.Size = new System.Drawing.Size(120, 52);
            this.btnGenRascoXML.TabIndex = 38;
            this.btnGenRascoXML.Text = "Generate Rasco Map XML";
            this.btnGenRascoXML.UseVisualStyleBackColor = true;
            this.btnGenRascoXML.Click += new System.EventHandler(this.btnGenRascoXML_Click);
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(451, 341);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(120, 52);
            this.btnMerge.TabIndex = 39;
            this.btnMerge.Text = "Merge Files";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
            // 
            // lblChkR3
            // 
            this.lblChkR3.AutoSize = true;
            this.lblChkR3.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkR3.ForeColor = System.Drawing.Color.Green;
            this.lblChkR3.Location = new System.Drawing.Point(785, 107);
            this.lblChkR3.Name = "lblChkR3";
            this.lblChkR3.Size = new System.Drawing.Size(26, 21);
            this.lblChkR3.TabIndex = 43;
            this.lblChkR3.Text = "P";
            this.lblChkR3.Visible = false;
            // 
            // btnR3
            // 
            this.btnR3.Location = new System.Drawing.Point(746, 103);
            this.btnR3.Name = "btnR3";
            this.btnR3.Size = new System.Drawing.Size(33, 25);
            this.btnR3.TabIndex = 42;
            this.btnR3.Text = "...";
            this.btnR3.UseVisualStyleBackColor = true;
            this.btnR3.Click += new System.EventHandler(this.btnR3_Click);
            // 
            // lblR3
            // 
            this.lblR3.Location = new System.Drawing.Point(20, 109);
            this.lblR3.Name = "lblR3";
            this.lblR3.Size = new System.Drawing.Size(132, 13);
            this.lblR3.TabIndex = 41;
            this.lblR3.Text = "Result Filename 3 :";
            // 
            // txtR3
            // 
            this.txtR3.Location = new System.Drawing.Point(181, 106);
            this.txtR3.Name = "txtR3";
            this.txtR3.Size = new System.Drawing.Size(559, 20);
            this.txtR3.TabIndex = 40;
            // 
            // lblChkR4
            // 
            this.lblChkR4.AutoSize = true;
            this.lblChkR4.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkR4.ForeColor = System.Drawing.Color.Green;
            this.lblChkR4.Location = new System.Drawing.Point(785, 138);
            this.lblChkR4.Name = "lblChkR4";
            this.lblChkR4.Size = new System.Drawing.Size(26, 21);
            this.lblChkR4.TabIndex = 47;
            this.lblChkR4.Text = "P";
            this.lblChkR4.Visible = false;
            // 
            // btnR4
            // 
            this.btnR4.Location = new System.Drawing.Point(746, 134);
            this.btnR4.Name = "btnR4";
            this.btnR4.Size = new System.Drawing.Size(33, 25);
            this.btnR4.TabIndex = 46;
            this.btnR4.Text = "...";
            this.btnR4.UseVisualStyleBackColor = true;
            this.btnR4.Click += new System.EventHandler(this.btnR4_Click);
            // 
            // lblR4
            // 
            this.lblR4.Location = new System.Drawing.Point(20, 140);
            this.lblR4.Name = "lblR4";
            this.lblR4.Size = new System.Drawing.Size(132, 13);
            this.lblR4.TabIndex = 45;
            this.lblR4.Text = "Result Filename 4 :";
            // 
            // txtR4
            // 
            this.txtR4.Location = new System.Drawing.Point(181, 137);
            this.txtR4.Name = "txtR4";
            this.txtR4.Size = new System.Drawing.Size(559, 20);
            this.txtR4.TabIndex = 44;
            // 
            // lblChkR5
            // 
            this.lblChkR5.AutoSize = true;
            this.lblChkR5.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkR5.ForeColor = System.Drawing.Color.Green;
            this.lblChkR5.Location = new System.Drawing.Point(785, 168);
            this.lblChkR5.Name = "lblChkR5";
            this.lblChkR5.Size = new System.Drawing.Size(26, 21);
            this.lblChkR5.TabIndex = 51;
            this.lblChkR5.Text = "P";
            this.lblChkR5.Visible = false;
            // 
            // btnR5
            // 
            this.btnR5.Location = new System.Drawing.Point(746, 164);
            this.btnR5.Name = "btnR5";
            this.btnR5.Size = new System.Drawing.Size(33, 25);
            this.btnR5.TabIndex = 50;
            this.btnR5.Text = "...";
            this.btnR5.UseVisualStyleBackColor = true;
            this.btnR5.Click += new System.EventHandler(this.btnR5_Click);
            // 
            // lblR5
            // 
            this.lblR5.Location = new System.Drawing.Point(20, 170);
            this.lblR5.Name = "lblR5";
            this.lblR5.Size = new System.Drawing.Size(132, 13);
            this.lblR5.TabIndex = 49;
            this.lblR5.Text = "Result Filename 5 :";
            // 
            // txtR5
            // 
            this.txtR5.Location = new System.Drawing.Point(181, 167);
            this.txtR5.Name = "txtR5";
            this.txtR5.Size = new System.Drawing.Size(559, 20);
            this.txtR5.TabIndex = 48;
            // 
            // btnSDIRegen
            // 
            this.btnSDIRegen.Location = new System.Drawing.Point(58, 341);
            this.btnSDIRegen.Name = "btnSDIRegen";
            this.btnSDIRegen.Size = new System.Drawing.Size(120, 52);
            this.btnSDIRegen.TabIndex = 52;
            this.btnSDIRegen.Text = "Regen from SDI";
            this.btnSDIRegen.UseVisualStyleBackColor = true;
            this.btnSDIRegen.Click += new System.EventHandler(this.btnSDIRegen_Click);
            // 
            // lblChkTestCondition
            // 
            this.lblChkTestCondition.AutoSize = true;
            this.lblChkTestCondition.Font = new System.Drawing.Font("Wingdings 2", 15F, System.Drawing.FontStyle.Bold);
            this.lblChkTestCondition.ForeColor = System.Drawing.Color.Green;
            this.lblChkTestCondition.Location = new System.Drawing.Point(785, 286);
            this.lblChkTestCondition.Name = "lblChkTestCondition";
            this.lblChkTestCondition.Size = new System.Drawing.Size(26, 21);
            this.lblChkTestCondition.TabIndex = 56;
            this.lblChkTestCondition.Text = "P";
            this.lblChkTestCondition.Visible = false;
            // 
            // btnTestCondition
            // 
            this.btnTestCondition.Location = new System.Drawing.Point(746, 282);
            this.btnTestCondition.Name = "btnTestCondition";
            this.btnTestCondition.Size = new System.Drawing.Size(33, 25);
            this.btnTestCondition.TabIndex = 55;
            this.btnTestCondition.Text = "...";
            this.btnTestCondition.UseVisualStyleBackColor = true;
            this.btnTestCondition.Click += new System.EventHandler(this.btnTestCondition_Click);
            // 
            // lblTestCondition
            // 
            this.lblTestCondition.AutoSize = true;
            this.lblTestCondition.Location = new System.Drawing.Point(20, 288);
            this.lblTestCondition.Name = "lblTestCondition";
            this.lblTestCondition.Size = new System.Drawing.Size(123, 13);
            this.lblTestCondition.TabIndex = 54;
            this.lblTestCondition.Text = "Test Condition Filename:";
            // 
            // txtTestCondition
            // 
            this.txtTestCondition.Location = new System.Drawing.Point(181, 285);
            this.txtTestCondition.Name = "txtTestCondition";
            this.txtTestCondition.Size = new System.Drawing.Size(559, 20);
            this.txtTestCondition.TabIndex = 53;
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusStripLabel,
            this.ProgressBar});
            this.StatusStrip.Location = new System.Drawing.Point(0, 407);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1027, 22);
            this.StatusStrip.TabIndex = 57;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // StatusStripLabel
            // 
            this.StatusStripLabel.AutoSize = false;
            this.StatusStripLabel.Name = "StatusStripLabel";
            this.StatusStripLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Always;
            this.StatusStripLabel.Size = new System.Drawing.Size(300, 17);
            this.StatusStripLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(100, 16);
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressBar.Value = 50;
            this.ProgressBar.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(841, 630);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 25);
            this.button1.TabIndex = 58;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(674, 620);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 49);
            this.button2.TabIndex = 59;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(144, 558);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(111, 53);
            this.button3.TabIndex = 60;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(58, 508);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(91, 44);
            this.button4.TabIndex = 61;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(606, 465);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(116, 54);
            this.button10.TabIndex = 62;
            this.button10.Text = "Test Summary";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click_1);
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(277, 479);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(129, 96);
            this.button13.TabIndex = 63;
            this.button13.Text = "button13";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // frm_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1027, 429);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.lblChkTestCondition);
            this.Controls.Add(this.btnTestCondition);
            this.Controls.Add(this.lblTestCondition);
            this.Controls.Add(this.txtTestCondition);
            this.Controls.Add(this.btnSDIRegen);
            this.Controls.Add(this.lblChkR5);
            this.Controls.Add(this.btnR5);
            this.Controls.Add(this.lblR5);
            this.Controls.Add(this.txtR5);
            this.Controls.Add(this.lblChkR4);
            this.Controls.Add(this.btnR4);
            this.Controls.Add(this.lblR4);
            this.Controls.Add(this.txtR4);
            this.Controls.Add(this.lblChkR3);
            this.Controls.Add(this.btnR3);
            this.Controls.Add(this.lblR3);
            this.Controls.Add(this.txtR3);
            this.Controls.Add(this.btnMerge);
            this.Controls.Add(this.btnGenRascoXML);
            this.Controls.Add(this.lblChkRascoXML);
            this.Controls.Add(this.btnRascoXML);
            this.Controls.Add(this.lblRascoXML);
            this.Controls.Add(this.txtRascoXML);
            this.Controls.Add(this.lblChkOutput);
            this.Controls.Add(this.btnRetest);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.lblChkNewSpec);
            this.Controls.Add(this.btnNewSpec);
            this.Controls.Add(this.lblNewSpec);
            this.Controls.Add(this.txtNewSpec);
            this.Controls.Add(this.lblChkPrevResult);
            this.Controls.Add(this.btnPrevResult);
            this.Controls.Add(this.lblPrevResultFile);
            this.Controls.Add(this.txtPrevResult);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOutput);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnXY);
            this.Controls.Add(this.lblChkResult);
            this.Controls.Add(this.btnResult);
            this.Controls.Add(this.LblResult);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.btnDelta);
            this.Controls.Add(this.btnRegen);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frm_Main";
            this.Text = "New Report Tools";
            this.Load += new System.EventHandler(this.frm_Main_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.frm_Main_Paint);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRegen;
        private System.Windows.Forms.Button btnDelta;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Label LblResult;
        private System.Windows.Forms.Button btnResult;
        private System.Windows.Forms.Label lblChkResult;
        private System.Windows.Forms.Button btnXY;
        private System.Windows.Forms.Button btnOutput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chk_ExtractXYLoc;
        private System.Windows.Forms.Label lblChkPrevResult;
        private System.Windows.Forms.Button btnPrevResult;
        private System.Windows.Forms.Label lblPrevResultFile;
        private System.Windows.Forms.TextBox txtPrevResult;
        private System.Windows.Forms.Label lblChkNewSpec;
        private System.Windows.Forms.Button btnNewSpec;
        private System.Windows.Forms.Label lblNewSpec;
        private System.Windows.Forms.TextBox txtNewSpec;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.CheckBox chkSelectHeader;
        private System.Windows.Forms.CheckBox chkIncludeOriginal;
        private System.Windows.Forms.Button btnRetest;
        private System.Windows.Forms.Label lblChkOutput;
        private System.Windows.Forms.Label lblChkRascoXML;
        private System.Windows.Forms.Button btnRascoXML;
        private System.Windows.Forms.Label lblRascoXML;
        private System.Windows.Forms.TextBox txtRascoXML;
        private System.Windows.Forms.Button btnGenRascoXML;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.Label lblChkR3;
        private System.Windows.Forms.Button btnR3;
        private System.Windows.Forms.Label lblR3;
        private System.Windows.Forms.TextBox txtR3;
        private System.Windows.Forms.Label lblChkR4;
        private System.Windows.Forms.Button btnR4;
        private System.Windows.Forms.Label lblR4;
        private System.Windows.Forms.TextBox txtR4;
        private System.Windows.Forms.Label lblChkR5;
        private System.Windows.Forms.Button btnR5;
        private System.Windows.Forms.Label lblR5;
        private System.Windows.Forms.TextBox txtR5;
        private System.Windows.Forms.Button btnSDIRegen;
        private System.Windows.Forms.Label lblChkTestCondition;
        private System.Windows.Forms.Button btnTestCondition;
        private System.Windows.Forms.Label lblTestCondition;
        private System.Windows.Forms.TextBox txtTestCondition;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusStripLabel;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button13;
    }
}

