using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProcessTool
{
    public partial class frmHeaderSelect : Form
    {
        private string[] TestParameters;
        private bool[] chkParameters;
 
        public frmHeaderSelect()
        {
            InitializeComponent();
        }

        public string[] parse_TestParameters
        {
            set
            {
                TestParameters = value;
            }
        }
        public bool[] parse_ChkParameters
        {
            get
            {
                return chkParameters;
            }
        }

        private void frmHeaderSelect_Load(object sender, EventArgs e)
        {
            
            if (TestParameters.Length > 0)
            {
                chkParameters = new bool[TestParameters.Length];
                for (int x=0; x<TestParameters.Length; x++)
                {
                    chkList.Items.Add((x+1).ToString() + " - " + TestParameters[x],true);
                }
            }
        }

        private void chkList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            chkParameters[e.Index]= cState2Bool(e.NewValue);   
        }

        private bool cState2Bool(CheckState state)
        {
            if (state == CheckState.Checked)
            {
                return true;
            }
            else
            {
                return false;   
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnSelection_Click(object sender, EventArgs e)
        {
            if (btnSelection.Text == "Deselect All")
            {
                for (int x = 0; x < chkList.Items.Count; x++)
                {
                    chkList.SetItemChecked(x, false);
                }
                btnSelection.Text = "Select All";
            }
            else
            {
                for (int x = 0; x < chkList.Items.Count; x++)
                {
                    chkList.SetItemChecked(x, true);
                }
                btnSelection.Text = "Deselect All";
            }
        }


    }
}
