using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WaferMap
{
    public partial class frmWaferMap : Form
    {
        private cGraphics Graphics;
        private cDrawObj DrawObj;
        bool bdraw;
        private bool bUpdate;
        int iX, iY;

        public frmWaferMap()
        {
            Graphics = new cGraphics();
            DrawObj = new cDrawObj();
            bdraw = true;
            InitializeComponent();
            iX = 0;
            iY = 0;
            bUpdate = false;
        }

        private void frmWaferMap_Load(object sender, EventArgs e)
        {
            if (Screen.PrimaryScreen.WorkingArea.Width >= 920)
            {
                this.Width = 920;
            }
            if (Screen.PrimaryScreen.WorkingArea.Height >= 920)
            {
                this.Height = 920;
            }
            LoadData();
            Graphics.CreateDoubleBuffer(this.CreateGraphics(), 920, 920);
        }
        private void FrmWaferMap_Paint(object sender, PaintEventArgs e)
        {
            //Graphics dc = e.Graphics;
            //Draw_Map(dc);
            if (bdraw)
            {
                Graphics.g.FillRectangle(new SolidBrush(SystemColors.Window), 0, 0, 920, 920);
                DrawObj.Draw(Graphics.g);
                
            }
            if (bUpdate)
            {
                DrawObj.Update(Graphics.g, iX, iY);
                bUpdate = false;
            }
            Graphics.Render(e.Graphics);
            bdraw = false;
        }
        private void FrmWaferMap_Activate(Object sender, EventArgs e)
        {
            bdraw = true;
            Invalidate();
        }
        private void frmWaferMap_Deactivate(Object sender, EventArgs e)
        {
            bdraw = false;
        }
        private void FrmWaferMap_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string sTmp="";
            if (e.X >= 10 && e.X <= 810)
            {
                sTmp = e.X.ToString() + ", ";
            }
            if (e.Y >= 10 && e.Y <= 810)
            {
                sTmp += e.Y.ToString();
            }
            StripStatus_Location.Text = sTmp;
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            iX++;
            iY++;
            bUpdate = true;
            Invalidate();
            //int[,] info = new int[400, 400];
            //OpenFileDialog Dialog = new OpenFileDialog();
            //Dialog.Filter = "Wafer File|*.wst";
            //Dialog.InitialDirectory = @"E:\rx\uTest_Database\Wafer_Setting";
            //Dialog.ShowDialog();

            //string Filename = Dialog.FileName;
            //string[] WaferData = System.IO.File.ReadAllLines(Filename);
            //int XMax = int.Parse(WaferData[10]) + 1;
            //int YMax = int.Parse(WaferData[11]) + 1;
            //int data = 12;
            //for (int x = 0; x < XMax; x++)
            //{
            //    for (int y = 0; y < YMax; y++)
            //    {
            //        info[x, y] = int.Parse(WaferData[data]);
            //        data++;
            //    }
            //}
            //DrawObj.Parse_Data = info;
            //Invalidate();


        }
        public void LoadData()
        {
            int Xact_Start, Xact_End, Yact_Start, Yact_End;
            int itmp;
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Filter = "Wafer File|*.wst";
            Dialog.InitialDirectory = @"E:\rx\uTest_Database\Wafer_Setting";
            Dialog.ShowDialog();
            Xact_Start = 999;
            Xact_End = 0;
            Yact_Start=999;
            Yact_End=0;
            string Filename = Dialog.FileName;
            string[] WaferData = System.IO.File.ReadAllLines(Filename);
            int XMax = int.Parse(WaferData[10]) + 1;
            int YMax = int.Parse(WaferData[11]) + 1;
            int data = 12;
            int[,] info = new int[XMax, YMax];
            for (int x = 0; x < XMax; x++)
            {
                for (int y = 0; y < YMax; y++)
                {
                    info[x, y] = int.Parse(WaferData[data]);

                    if (info[x, y] != 17)
                    {
                        if (x > Xact_End) Xact_End = x;
                        if (x < Xact_Start) Xact_Start = x;
                        if (y < Yact_Start) Yact_Start = y;
                        if (y > Xact_End) Yact_End = y;
                    }
                    data++;
                }
            }

            int[,] info_upd = new int[Xact_End - Xact_Start, Yact_End - Yact_Start];
            for (int x = Xact_Start; x < Xact_End; x++)
            {
                for (int y = Yact_Start; y < Yact_End; y++)
                {
                    info_upd[x - Xact_Start, y - Yact_Start] = info[x, y];
                    data++;
                }
            }
            DrawObj.Parse_Data = info_upd;
        }
    }
}
