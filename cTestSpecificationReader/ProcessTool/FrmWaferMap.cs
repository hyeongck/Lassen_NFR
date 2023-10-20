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
    public partial class FrmWaferMap : Form
    {
        public FrmWaferMap()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FrmWaferMap_Load(object sender, EventArgs e)
        {
            if (Screen.PrimaryScreen.WorkingArea.Width >= 920)
            {
                this.Width = 920;
            }
            if (Screen.PrimaryScreen.WorkingArea.Height >= 920)
            {
                this.Height = 920;
            }
            
        }
        private void FrmWaferMap_Paint(object sender, PaintEventArgs e)
        {
            Graphics dc = e.Graphics;
            Draw_Map(dc);
        }
        private void Draw_Map(Graphics e)
        {
            //e = this.CreateGraphics();
            //Pen DrawingPen = new Pen(Color.Black, 1);
            //e.DrawRectangle(DrawingPen, 10, 10, 800, 800);
            //SolidBrush brush = new SolidBrush(Color.Blue);
            //for (int x = 0; x < 400; x++)
            //{
            //    for (int y = 0; y < 400; y++)
            //    {
            //        //e.DrawRectangle(DrawingPen, 10+(x * 2), 10+(y * 2), 2, 2);
            //        e.FillRectangle(brush, 10 + (x * 2), 10 + (y * 2), 2, 2);
            //    }
            //}
            

        }
    }
}
