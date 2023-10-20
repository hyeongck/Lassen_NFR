using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cInputForm
{
    public partial class InputForm : Form
    {
        OpenFileDialog Dialog = new OpenFileDialog();
        public InputForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        public string Set_Title
        {
            set
            {
                this.Text = value;
            }
        }
        public string parse_FileName
        {
            get
            {
                return textBox1.Text;
            }
        }
        public string parse_InitFolder
        {
            set
            {
                Dialog.InitialDirectory = value;
            }
        }
        private void cInputForm_Load(object sender, EventArgs e)
        {
            //Dialog.InitialDirectory = "C:\\Avago.ATF.Common\\Results";
            Dialog.Filter = "Report File|*.csv";
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult rslt = Dialog.ShowDialog();
            if(rslt == DialogResult.OK)
            {
                textBox1.Text = Dialog.FileName;
            }
        }
    }
}
