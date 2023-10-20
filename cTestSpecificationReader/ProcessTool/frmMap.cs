using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ProcessTool
{
    public partial class frmMap : Form
    {
        private cTestResultsReader.s_Results Result;
        private cTestResultsReader.s_Results Result2;

        int scale = 5;

        public frmMap()
        {
            InitializeComponent();
        }

        public cTestResultsReader.s_Results Parse_Result
        {
            set
            {
                Result = value;
            }
            get
            {
                return Result;
            }
        }
        public cTestResultsReader.s_Results Parse_Result2
        {
            set
            {
                Result2 = value;
            }
            get
            {
                return Result2;
            }
        }
        public bool Compare_Maps { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmMap_Paint(object sender, PaintEventArgs e)
        {
            Graphics dc = e.Graphics;
            if (Compare_Maps)
            {
                Draw_Map_Diff(dc);
            }
            else
            {
                if (Result.RawData != null) Draw_Map(dc, Result);
                if (Result2.RawData != null) Draw_Map(dc, Result2);
            } 
        }
        private void frmMap_MouseClick(Object sender, MouseEventArgs e)
        {
        }

        private void Draw_Map(Graphics e, cTestResultsReader.s_Results rslt)
        {
            
            e = this.CreateGraphics();
            Pen DrawingPen = new Pen(Color.Black, 1);
            SolidBrush brush = new SolidBrush(Color.Blue);
            for (int y = rslt.XY_Info.XY_MinMax.Min_Y; y <= rslt.XY_Info.XY_MinMax.Max_Y; y++)
            {
                for (int x = rslt.XY_Info.XY_MinMax.Min_X; x <= rslt.XY_Info.XY_MinMax.Max_X; x++)
                {
                    switch (rslt.ResultData[rslt.XY_Info.Match_Position[x, y]].HBin)
                    {
                        case 1:
                            brush.Color = Color.White;
                            break;
                        case 2:
                            brush.Color = Color.Orange;
                            break;
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                            brush.Color = Color.Red;
                            break;
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                            brush.Color = Color.Green;
                            break;
                        default:
                            brush.Color = Color.Blue;
                            break;
                    }
                    e.FillRectangle(brush, 20 + (x * scale), 20 + (y * scale), scale, scale);
                    e.DrawRectangle(DrawingPen, 20 + (x * scale), 20 + (y * scale), scale, scale); 
                }
            }
        }
        private void Draw_Map_Diff(Graphics e)
        {
            e = this.CreateGraphics();
            Pen DrawingPen = new Pen(Color.Black, 1);
            SolidBrush brush = new SolidBrush(Color.Blue);
            for (int y = Result.XY_Info.XY_MinMax.Min_Y; y <= Result.XY_Info.XY_MinMax.Max_Y; y++)
            {
                for (int x = Result.XY_Info.XY_MinMax.Min_X; x <= Result.XY_Info.XY_MinMax.Max_X; x++)
                {
                    if ((Result.ResultData[Result.XY_Info.Match_Position[x, y]].HBin) != (Result2.ResultData[Result2.XY_Info.Match_Position[x, y]].HBin))
                    {
                        brush.Color = Color.Red;
                    }
                    else
                    {
                        brush.Color = Color.Green;
                    }
                    e.FillRectangle(brush, 20 + (x * scale), 20 + (y * scale), scale, scale);
                    e.DrawRectangle(DrawingPen, 20 + (x * scale), 20 + (y * scale), scale, scale);
                }
            }
        }

        private void frmMap_Load(object sender, EventArgs e)
        {

        }


    }
}
