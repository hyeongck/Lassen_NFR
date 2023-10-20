using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace cWaferMap
{
    public struct s_WaferMap
    {
        public int Raw_Info;
        public e_DieZone DieZone;
        public bool DieProbe;
        public e_DieType DieType;
        public Color BinColor;
        public int BinTag;
        public int X_Reference;
        public int Y_Reference;
    }
    public struct s_MapColor
    {
        public Color Valid_Good;
        public Color Valid_Ugly;
        public Color Valid_NoDie;
        public Color Valid_Reference;
        public Color No_Probe;
    }
    public enum e_DieZone
    {
        RED,
        YELLOW,
        GREEN,
        OTHER
    }
    public enum e_DieType
    {
        GOOD,
        UGLY,
        NODIE,
        REFERENCE
    }
    public class cGraphics
    {
        private const int iZone_Red = 1; // &H1
        private const int iZone_Yellow = 2; // &H2
        private const int iZone_Green = 3; // &H3
        private const int iZone_Other = 0; // &H0

        private const int iProbe_Valid = 4; // &H4

        private const int iType_Good = 0; // &H0
        private const int iType_Ugly = 8; // &H8
        private const int iType_NoDie = 16; // &H16
        private const int iType_Reference = 56; // &H38


        private int Zoom_Factor = 5;
        private int Zoom_BoxSize = 50;

        private Graphics graphics;
        private Bitmap memoryBitmap;

        private int width;
        private int height;

        private int height_offset = 24;

        private s_WaferMap[,] WaferMap;

        private bool bGrid;

        private int XMax;
        private int YMax;
        private float XFac;
        private float YFac;

        private s_MapColor MapColor;
        private s_XY FirstTestPos;

        
        public cGraphics()
        {
            width = 0;
            height = 0;
            Init_MapColor();
            FirstTestPos.X = 9999;
            FirstTestPos.Y = 9999;
            bGrid = true;
        }
        public s_XY parse_FirstTestPos
        {
            get
            {
                return FirstTestPos;
            }
        }
        public bool parse_GridSetting
        {
            set
            {
                bGrid = value;
            }
        }
        public s_WaferMap[,] WaferMap_Data
        {
            get
            {
                return WaferMap;
            }
        }
        public int parse_ZoomBoxSize
        {
            set
            {
                Zoom_BoxSize = value;
            }
        }
        public int parse_ZoomFactor
        {
            set
            {
                Zoom_Factor = value;
            }
        }
        public void Init_MapColor()
        {
            MapColor.Valid_Good = Color.LightSkyBlue;
            MapColor.Valid_Ugly = Color.White;
            MapColor.Valid_NoDie = Color.Yellow;
            MapColor.Valid_Reference = Color.Red;
            MapColor.No_Probe = Color.DarkGray;
        }
        public void Init_Mapping(int[,] Data)
        {
            for (int ix = 0; ix < XMax; ix++)
            {
                for (int iy = 0; iy < YMax; iy++)
                {
                    WaferMap[ix, iy].Raw_Info = Data[ix, iy];
                    WaferMap[ix, iy].DieZone = Translate_DieZone(Data[ix, iy]);
                    WaferMap[ix, iy].DieType = Translate_DieType(Data[ix, iy]);
                    WaferMap[ix, iy].DieProbe = Translate_DieProbe(Data[ix, iy]);
                    WaferMap[ix, iy].BinTag = -1;
                    WaferMap[ix,iy].BinColor = Translate_MapColor(ix, iy);
                    if ((WaferMap[ix, iy].DieProbe) && (WaferMap[ix, iy].DieType == e_DieType.GOOD))
                    {
                        if (iy < FirstTestPos.Y)
                        {
                            FirstTestPos.Y = iy;
                        }
                        if (ix < FirstTestPos.X)
                        {
                            FirstTestPos.X = ix;
                        }
                    }
                }
            }
        }
        public int parse_height_offset
        {
            set
            {
                height_offset = value;
            }
        }
        public bool Translate_DieProbe(int info)
        {
            int rslt = info & 4;
            switch (rslt)
            {
                case iProbe_Valid:
                    return true;
                    break;
                default:
                    return false;
                    break;
            }
        }
        public e_DieZone Translate_DieZone(int info)
        {
            int rslt = info & 3;
            switch (rslt)
            {
                case iZone_Red:
                    return e_DieZone.RED;
                    break;
                case iZone_Yellow:
                    return e_DieZone.YELLOW;
                    break;
                case iZone_Green:
                    return e_DieZone.GREEN;
                    break;
                case iZone_Other:
                    return e_DieZone.OTHER;
                    break;
                default:
                    return e_DieZone.OTHER;
                    break;
            }
        }
        public e_DieType Translate_DieType(int info)
        {
            int rslt = info & 56;
            switch (rslt)
            {
                case iType_Good:
                    return e_DieType.GOOD;
                    break;
                case iType_Ugly:
                    return e_DieType.UGLY;
                    break;
                case iType_NoDie:
                    return e_DieType.NODIE;
                    break;
                case iType_Reference:
                    return e_DieType.REFERENCE;
                    break;
                default:
                    return e_DieType.NODIE;
                    break;
            }
        }
        public Color Translate_MapColor(int X, int Y)
        {
            Color rslt;
            if (WaferMap[X, Y].DieProbe)
            {
                switch (WaferMap[X, Y].DieType)
                {
                    case e_DieType.GOOD:
                        rslt = MapColor.Valid_Good;
                        break;
                    case e_DieType.UGLY:
                        rslt = MapColor.Valid_Ugly;
                        break;
                    case e_DieType.NODIE:
                        rslt = MapColor.Valid_NoDie;
                        break;
                    case e_DieType.REFERENCE:
                        rslt = MapColor.Valid_Reference;
                        break;
                    default:
                        rslt = MapColor.Valid_NoDie;
                        break;
                }
            }
            else
            {
                rslt = MapColor.No_Probe;
            }
            return rslt;
        }
        public int[,] Parse_Data
        {
            set
            {
                XMax = value.GetUpperBound(0) + 1;
                YMax = value.GetUpperBound(1) + 1;
                WaferMap = new s_WaferMap[XMax, YMax];
                Init_Mapping(value);
        
                //BinData = value;
                XFac = (float)height / (float)XMax;
                YFac = (float)width / (float)YMax;
                //binColor = new Color[XMax, YMax];
            }
        }
        public Color Get_BinColor(int X, int Y)
        {
            return WaferMap[X, Y].BinColor;
        }
        public Color Get_BinColor(s_XY pos)
        {
            return WaferMap[pos.X, pos.Y].BinColor;
        }
        public bool CreateDoubleBuffer(Graphics g, int set_width, int set_height)
        {

            if (memoryBitmap != null)
            {
                memoryBitmap.Dispose();
                memoryBitmap = null;
            }

            if (graphics != null)
            {
                graphics.Dispose();
                graphics = null;
            }

            if (set_width == 0 || set_height == 0)
                return false;


            if ((set_width != this.width) || (set_height != this.height))
            {
                this.width = set_width;
                this.height = set_height;

                memoryBitmap = new Bitmap(set_width + 20, set_height + 20 + height_offset);
                graphics = Graphics.FromImage(memoryBitmap);
            }

            return true;
        }
        
        public void Zoom(Graphics g, int X, int Y)
        {
            int ZoomFactor = (Zoom_Factor - 1) / 2;
            float ZoomBoxSize = (float)Zoom_BoxSize / Zoom_Factor;
            int iX, iY;
            iX=0;
            iY=0;
            Brush brush = new SolidBrush(Color.LightGray);
            Pen DrawPen = new Pen(Color.Gray);
            if ((X != -1) && (Y != -1))
            {
                g.FillRectangle(brush, 10, 10 + height_offset, Zoom_BoxSize, Zoom_BoxSize);
                
                for (int pX = X - ZoomFactor; pX <= X + ZoomFactor; pX++)
                {
                    for(int pY = Y - ZoomFactor; pY <= Y + ZoomFactor; pY++)
                    {
                        //if (iX == 0)
                        //{
                        //    g.DrawLine(DrawPen, 0, (iY * ZoomBoxSize), Zoom_BoxSize, (iY * ZoomBoxSize));
                        //}
                        if ((pX < 0) || (pX >= XMax))
                        {
                            brush = new SolidBrush(Color.Black);
                        }
                        else
                        {
                            if ((pY < 0) || (pY >= YMax))
                            {
                                brush = new SolidBrush(Color.Black);
                            }
                            else
                            {
                                brush = new SolidBrush(WaferMap[pX,pY].BinColor);
                            }
                        }
                        g.FillRectangle(brush, 11f + (iX * ZoomBoxSize), 11f + height_offset + (iY * ZoomBoxSize), ZoomBoxSize -1f, ZoomBoxSize-1f);
                        iY++;
                    }
                    //g.DrawLine(DrawPen, 0, (iY * ZoomBoxSize), Zoom_BoxSize, (iY * ZoomBoxSize));
                    iX++;
                    iY = 0;
                }
                g.DrawRectangle(new Pen(Color.Black), 10, 10 + height_offset, Zoom_BoxSize, Zoom_BoxSize);
                g.DrawRectangle(new Pen(Color.Red, 2), 10 + (ZoomFactor * ZoomBoxSize), 10 + height_offset + (ZoomFactor * ZoomBoxSize), ZoomBoxSize, ZoomBoxSize);
            }
        }
        public void Zoom(Graphics g, s_XY pos)
        {
            int ZoomFactor = (Zoom_Factor - 1) / 2;
            float ZoomBoxSize = (float)Zoom_BoxSize / Zoom_Factor;
            int iX, iY;
            iX = 0;
            iY = 0;
            Brush brush = new SolidBrush(Color.LightGray);
            Pen DrawPen = new Pen(Color.Gray);
            if ((pos.X != -1) && (pos.Y != -1))
            {
                g.FillRectangle(brush, 10, 10 + height_offset, Zoom_BoxSize, Zoom_BoxSize);

                for (int pX = pos.X - ZoomFactor; pX <= pos.X + ZoomFactor; pX++)
                {
                    for (int pY = pos.Y - ZoomFactor; pY <= pos.Y + ZoomFactor; pY++)
                    {
                        //if (iX == 0)
                        //{
                        //    g.DrawLine(DrawPen, 0, (iY * ZoomBoxSize), Zoom_BoxSize, (iY * ZoomBoxSize));
                        //}
                        if ((pX < 0) || (pX >= XMax))
                        {
                            brush = new SolidBrush(Color.Black);
                        }
                        else
                        {
                            if ((pY < 0) || (pY >= YMax))
                            {
                                brush = new SolidBrush(Color.Black);
                            }
                            else
                            {
                                brush = new SolidBrush(WaferMap[pX, pY].BinColor);
                            }
                        }
                        g.FillRectangle(brush, 11f + (iX * ZoomBoxSize), 11f + height_offset + (iY * ZoomBoxSize), ZoomBoxSize - 1f, ZoomBoxSize - 1f);
                        iY++;
                    }
                    //g.DrawLine(DrawPen, 0, (iY * ZoomBoxSize), Zoom_BoxSize, (iY * ZoomBoxSize));
                    iX++;
                    iY = 0;
                }
                g.DrawRectangle(new Pen(Color.Black), 10, 10 + height_offset, Zoom_BoxSize, Zoom_BoxSize);
                g.DrawRectangle(new Pen(Color.Red, 2), 10 + (ZoomFactor * ZoomBoxSize), 10 + height_offset + (ZoomFactor * ZoomBoxSize), ZoomBoxSize, ZoomBoxSize);
            }
        }
        public void Lock(Graphics g, s_XY pos)
        {
            g.DrawRectangle(new Pen(Color.Red, 1), 10 + (pos.X * XFac), 10 + height_offset + (pos.Y * YFac), XFac, YFac);
        }
        public void Lock(Graphics g, int X, int Y)
        {
            g.DrawRectangle(new Pen(Color.Red, 1), 10 + (X * XFac), 10 + height_offset + (Y * YFac), XFac, YFac);
        }
        public void Update(Graphics g, int X, int Y)
        {
            if (WaferMap[X, Y].DieType == e_DieType.GOOD)
            {
                WaferMap[X, Y].BinColor = Color.Blue;
            }
        }
        public void UpdateGraphic(Graphics g, int X, int Y)
        {
            SolidBrush brush = new SolidBrush(WaferMap[X, Y].BinColor);
            g.FillRectangle(brush, 10 + (X * XFac), 10 + height_offset + (Y * YFac), XFac, YFac);
        }
        public void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(SystemColors.Window), 0, height_offset, width + 20, height + 20);
            Pen DrawingPen = new Pen(Color.LightGray, 1);
            g.DrawRectangle(DrawingPen, 10, 10 + height_offset, width, height);
            
            SolidBrush brush;
            for (int x = 0; x < XMax; x++)
            {
                for (int y = 0; y < YMax; y++)
                {
                    brush = new SolidBrush(WaferMap[x,y].BinColor);
                    g.FillRectangle(brush, 10 + (x * XFac), 10 + height_offset + (y * YFac), XFac, YFac);
                    if (bGrid)
                    {
                        if (x == XMax - 1)
                        {
                            if (y % 10 == 0)
                            {
                                g.DrawLine(DrawingPen, 10, 10 + height_offset + (y * YFac), 10 + width, 10 + height_offset + (y * YFac));
                            }
                        }
                    }
                }
                if (bGrid)
                {
                    if (x % 10 == 0)
                    {
                        g.DrawLine(DrawingPen, 10 + (x * XFac), 10 + height_offset, 10 + (x * XFac), 10 + height_offset + height);
                    }
                }
            }
        }
        public void DrawFillBox(Graphics g, int x, int y)
        {
            if ((x != -1) && (y != -1))
            {
                for (int pX = x - 2; pX < (x + 2); pX++)
                {
                    for (int pY = y - 2; pY < (y + 2); pY++)
                    {
                        //SolidBrush brush = new SolidBrush(binColor[pX, pY]);
                        SolidBrush brush = new SolidBrush(WaferMap[pX, pY].BinColor);
                        g.FillRectangle(brush, 10 + (pX * XFac), 10 + height_offset + (pY * YFac), XFac, YFac);
                    }
                }
            }
        }
        public void DrawFillBox(Graphics g, int x, int y, int Gap)
        {
            int low_x, low_y, high_x, high_y;
            Pen DrawingPen = new Pen(Color.LightGray, 1);
            if (x - Gap > 0)
            {
                low_x = x - Gap;
            }
            else
            {
                low_x = 0;
            }
            if (y - Gap > 0)
            {
                low_y = y - Gap;
            }
            else
            {
                low_y = 0;
            }
            if (x + Gap > XMax)
            {
                high_x = XMax;
            }
            else
            {
                high_x = x + Gap;
            }
            if (y + Gap > YMax)
            {
                high_y = YMax;
            }
            else
            {
                high_y = y + Gap;
            }
            if ((x != -1) && (y != -1))
            {

                for (int pX = low_x; pX < high_x; pX++)
                {
                    for (int pY = low_y; pY < high_y; pY++)
                    {
                        //SolidBrush brush = new SolidBrush(binColor[pX, pY]);
                        SolidBrush brush = new SolidBrush(WaferMap[pX, pY].BinColor);
                        g.FillRectangle(brush, 10 + (pX * XFac), 10 + height_offset + (pY * YFac), XFac, YFac);
                        if (bGrid)
                        {
                            if (pX == high_x - 1)
                            {
                                if (pY % 10 == 0)
                                {
                                    g.DrawLine(DrawingPen, 10 + (low_x * XFac), 10 + height_offset + (pY * YFac), 10 + (high_x * XFac), 10 + height_offset + (pY * YFac));
                                }
                            }
                        }
                    }
                    if (bGrid)
                    {
                        if (pX % 10 == 0)
                        {
                            g.DrawLine(DrawingPen, 10 + (pX * XFac), 10 + height_offset + (low_y * YFac), 10 + (pX * XFac), 10 + height_offset + (high_y * YFac));
                        }
                    }
                }
            }
        }

        public void DrawBox(Graphics g, int x, int y)
        {
            if ((x != -1) && (y != -1))
            {
                //lastColor = memoryBitmap.GetPixel((int)(11 + (x * XFac)), (int)(11 + (y * YFac)));
                Pen DrawingPen = new Pen(Color.Black, 1);
                g.DrawRectangle(DrawingPen, 10f + (x * XFac), 10f + height_offset + (y * YFac), XFac, YFac);
            }
        }
        public void DrawBox(Graphics g, s_XY pos)
        {
            if ((pos.X != -1) && (pos.Y != -1))
            {
                Pen DrawingPen = new Pen(Color.Black, 1);
                g.DrawRectangle(DrawingPen, 10f + (pos.X * XFac), 10f + height_offset + (pos.Y * YFac), XFac, YFac);
            }
        }
       
        public void Render(Graphics g)
        {
            if (memoryBitmap != null)
                g.DrawImage(memoryBitmap, new Rectangle(0, 0, width + 20, height + 20 + height_offset), 0, 0, width + 20, height + 20 + height_offset, GraphicsUnit.Pixel);
        }
        public Graphics g
        {
            get
            {
                return graphics;
            }
        }
    }
}
