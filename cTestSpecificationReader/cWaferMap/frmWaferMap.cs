using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cWaferMap
{
    public struct s_XY
    {
        public int X;
        public int Y;
    }
    public partial class frmWaferMap : Form
    {
        private cGraphics Graphics;
        private cMapping Mapping;

        private bool bdraw;
        private bool bUpdate;
        private int iX, iY;
        private float XFac, YFac;
        private int XSize, YSize;
        private s_XY Current_MousePosition;
        private s_XY Previous_MousePosition;
        private s_XY Temp_MousePosition;
        private s_XY Lock_MousePosition;
        private int Height_Offset = 23;
        private bool bZoom;
        private bool bMouseLock;
        private bool bDieInfo;

        public frmWaferMap()
        {
            Graphics = new cGraphics();
            Mapping = new cMapping();
            InitializeComponent();
            bdraw = true;
            bUpdate = false;
            Current_MousePosition.X = -1;
            Current_MousePosition.Y = -1;
            Previous_MousePosition.X = -1;
            Previous_MousePosition.Y = -1;
            Temp_MousePosition.X = -1;
            Temp_MousePosition.Y = -1;
            Lock_MousePosition.X = -1;
            Lock_MousePosition.Y = -1;

            iX = 0;
            iY = 0;
            XSize = 800;
            YSize = 800;
            Graphics.parse_height_offset = Height_Offset;
            bDieInfo = true;
        }

        private void frmWaferMap_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            if (Screen.PrimaryScreen.WorkingArea.Width >= 920)
            {
                this.Width = 950;
            }
            if (Screen.PrimaryScreen.WorkingArea.Height >= 920)
            {
                this.Height = 890;
            }
            
            Graphics.CreateDoubleBuffer(this.CreateGraphics(), XSize, YSize);
            LoadData();
        }
        private void frmWaferMap_Paint(object sender, PaintEventArgs e)
        {
            
            
            if (bdraw)
            {
                bdraw = false;
                Graphics.Draw(Graphics.g);
            }

            Graphics.Render(e.Graphics);
            Graphics.DrawBox(e.Graphics, Current_MousePosition);
            
            if (bUpdate)
            {
                Graphics.Update(Graphics.g, iX, iY);
                Graphics.UpdateGraphic(e.Graphics, iX, iY);
                bUpdate = false;
                bdraw = true;
            }
            if (bZoom)
            {
                Graphics.Zoom(e.Graphics, Current_MousePosition);
            }
            if (bMouseLock)
            {
                Graphics.Lock(e.Graphics, Lock_MousePosition);
                txtSelect_Color.BackColor = Graphics.Get_BinColor(Lock_MousePosition);
            }
        }
        private void frmWaferMap_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string sCTmp = "";
            if ((e.X > 10 && e.X < XSize + 10) && (e.Y > 10 + Height_Offset && e.Y < YSize + 10 + Height_Offset))
            {
                sCTmp = Math.Floor((e.X - 10) / XFac).ToString() + ", " + Math.Floor((e.Y - 10 - Height_Offset) / YFac).ToString();
                Current_MousePosition.X = (int)Math.Floor((e.X - 10) / XFac);
                Current_MousePosition.Y = (int)Math.Floor((e.Y - 10 - Height_Offset) / YFac);
                if (Temp_MousePosition.X != Current_MousePosition.X)
                {
                    Previous_MousePosition = Temp_MousePosition;
                    Temp_MousePosition.X = Current_MousePosition.X;
                }

                if (Temp_MousePosition.Y != Current_MousePosition.Y)
                {
                    Previous_MousePosition = Temp_MousePosition;
                    Temp_MousePosition.Y = Current_MousePosition.Y;
                }
                StripStatus_Color.BackColor = Graphics.Get_BinColor(Current_MousePosition.X, Current_MousePosition.Y);
            }
            else
            {
                Current_MousePosition.X = -1;
                Current_MousePosition.Y = -1;
            }
            StripStatus_Location.Text = sCTmp;

            Invalidate();
        }
        private void frmWaferMap_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            s_XY tmpLoc;
            if (bDieInfo)
            {
                if ((e.X > 10 && e.X < XSize + 10) && (e.Y > 10 + Height_Offset && e.Y < YSize + 10 + Height_Offset))
                {
                    Lock_MousePosition.X = (int)Math.Floor((e.X - 10) / XFac);
                    Lock_MousePosition.Y = (int)Math.Floor((e.Y - 10 - Height_Offset) / YFac);
                    tmpLoc = Mapping.Get_ProbePosition(Lock_MousePosition);
                    lblSelect_LocX.Text = Lock_MousePosition.X.ToString();
                    lblSelect_LocY.Text = Lock_MousePosition.Y.ToString();
                    bMouseLock = true;
                    bdraw = true;
                    StripStatus_Tmp.Text = tmpLoc.X.ToString() + ", " + tmpLoc.Y.ToString();
                    Invalidate();
                }
            }
        }
        public void LoadData()
        {
            int Xact_Start, Xact_End, Yact_Start, Yact_End;

            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Filter = "Wafer File|*.wst";
            Dialog.InitialDirectory = @"E:\rx\uTest_Database\Wafer_Setting";
            Dialog.ShowDialog();
            Xact_Start = 999;
            Xact_End = 0;
            Yact_Start = 999;
            Yact_End = 0;
            string Filename = Dialog.FileName;
            if (Filename != "")
            {
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
                            if (y > Yact_End) Yact_End = y;
                        }
                        data++;
                    }
                }
                Xact_End++;
                Yact_End++;
                int[,] info_upd = new int[Xact_End - Xact_Start, Yact_End - Yact_Start];
                for (int x = Xact_Start; x < Xact_End; x++)
                {
                    for (int y = Yact_Start; y < Yact_End; y++)
                    {
                        info_upd[x - Xact_Start, y - Yact_Start] = info[x, y];
                    }
                }
                Graphics.Parse_Data = info_upd;
                Mapping.Parse_FirstTestPos = Graphics.parse_FirstTestPos;
                Mapping.WaferMap_Data = Graphics.WaferMap_Data;
                XFac = (float)XSize / (float)(Xact_End - Xact_Start);
                YFac = (float)YSize / (float)(Yact_End - Yact_Start);
            }
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
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!enableToolStripMenuItem.Checked)
            {
                enableToolStripMenuItem.Checked = true;
                bZoom = true;
            }
            else
            {
                enableToolStripMenuItem.Checked = false;
                bZoom = false;  
            }
            bdraw = true;
            Invalidate();
        }

        private void toolStripMenuItem_BS50_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_BS50.Checked)
            {
                Uncheck_BoxSizeMenuItem();
                toolStripMenuItem_BS50.Checked = true;
                Graphics.parse_ZoomBoxSize = 50;
                bdraw = true;
            }
        }
        private void toolStripMenuItem_BS75_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_BS75.Checked)
            {
                Uncheck_BoxSizeMenuItem();
                toolStripMenuItem_BS75.Checked = true;
                Graphics.parse_ZoomBoxSize = 75;
                bdraw = true;
            }
        }
        private void toolStripMenuItem_BS100_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_BS100.Checked)
            {
                Uncheck_BoxSizeMenuItem();
                toolStripMenuItem_BS100.Checked = true;
                Graphics.parse_ZoomBoxSize = 100;
                bdraw = true;
            }
        }
        private void toolStripMenuItem_BS125_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_BS125.Checked)
            {
                Uncheck_BoxSizeMenuItem();
                toolStripMenuItem_BS125.Checked = true;
                Graphics.parse_ZoomBoxSize = 125;
                bdraw = true;
            }
        }
        public void Uncheck_BoxSizeMenuItem()
        {
            toolStripMenuItem_BS50.Checked = false;
            toolStripMenuItem_BS75.Checked = false;
            toolStripMenuItem_BS100.Checked = false;
            toolStripMenuItem_BS125.Checked = false;
        }

        private void toolStripMenuItem_Factor3_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_Factor3.Checked)
            {
                Uncheck_FactorMenuItem();
                toolStripMenuItem_Factor3.Checked = true;
                Graphics.parse_ZoomFactor = 3;
            }
        }
        private void toolStripMenuItem_Factor5_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_Factor5.Checked)
            {
                Uncheck_FactorMenuItem();
                toolStripMenuItem_Factor5.Checked = true;
                Graphics.parse_ZoomFactor = 5;
            }
        }
        private void toolStripMenuItem_Factor7_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_Factor7.Checked)
            {
                Uncheck_FactorMenuItem();
                toolStripMenuItem_Factor7.Checked = true;
                Graphics.parse_ZoomFactor = 7;
            }
        }
        private void toolStripMenuItem_Factor9_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_Factor9.Checked)
            {
                Uncheck_FactorMenuItem();
                toolStripMenuItem_Factor9.Checked = true;
                Graphics.parse_ZoomFactor = 9;
            }
        }
        private void toolStripMenuItem_Factor11_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItem_Factor11.Checked)
            {
                Uncheck_FactorMenuItem();
                toolStripMenuItem_Factor11.Checked = true;
                Graphics.parse_ZoomFactor = 11;
            }
        }
        public void Uncheck_FactorMenuItem()
        {
            toolStripMenuItem_Factor3.Checked = false;
            toolStripMenuItem_Factor5.Checked = false;
            toolStripMenuItem_Factor7.Checked = false;
            toolStripMenuItem_Factor9.Checked = false;
            toolStripMenuItem_Factor11.Checked = false;
        }

        private void ToolStripMenuItem_SelectDieInformation_Click(object sender, EventArgs e)
        {
            if (!ToolStripMenuItem_SelectDieInformation.Checked)
            {
                ToolStripMenuItem_SelectDieInformation.Checked = true;
                group_SelectInformation.Visible = true;
                bMouseLock = false;
                bDieInfo = true;
            }
            else
            {
                ToolStripMenuItem_SelectDieInformation.Checked = false;
                group_SelectInformation.Visible = false;
                bMouseLock = false;
                bDieInfo = false;
            }
        }

        private void ToolStripMenuItem_Grid_Click(object sender, EventArgs e)
        {
            if (!ToolStripMenuItem_Grid.Checked)
            {
                ToolStripMenuItem_Grid.Checked = true;
                Graphics.parse_GridSetting = true;
            }
            else
            {
                ToolStripMenuItem_Grid.Checked = false;
                Graphics.parse_GridSetting = false;
            }
            bdraw = true;
            Invalidate();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
