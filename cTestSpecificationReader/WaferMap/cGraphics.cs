using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace WaferMap
{
    public class cGraphics
    {
        private Graphics graphics;
        private Bitmap memoryBitmap;
        private int width;
        private int height;

        public cGraphics()
        {
            width = 0;
            height = 0;
        }
        
        public bool CreateDoubleBuffer(Graphics g, int width, int height)
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

            if (width == 0 || height == 0)
                return false;


            if ((width != this.width) || (height != this.height))
            {
                this.width = width;
                this.height = height;

                memoryBitmap = new Bitmap(width, height);
                graphics = Graphics.FromImage(memoryBitmap);
            }

            return true;
        }
        public void Render(Graphics g)
        {
            if (memoryBitmap != null)
                g.DrawImage(memoryBitmap, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
        }
        public bool CanDoubleBuffer()
        {
            return graphics != null;
        }
        public Graphics g
        {
            get
            {
                return graphics;
            }
        }
    }
    public class cDrawObj
    {
        //private Rectangle boundingRect;
        private int[,] BinData;
        private int XMax;
        private int YMax;
        private float XFac;
        private float YFac;

        public cDrawObj()
		{
			//boundingRect	= new Rectangle(10,10,800,800);	
		}
        public int[,] Parse_Data
        {
            set
            {
                BinData = value;
                XMax = BinData.GetUpperBound(0);
                YMax = BinData.GetUpperBound(1);
                XFac = 800f / (float)XMax;
                YFac = 800f / (float)YMax;
            }
        }
        public void Update(Graphics g, int X, int Y)
        {
            SolidBrush brush = new SolidBrush(Color.Blue);
            g.FillRectangle(brush, 10 + (X * XFac), 10 + (Y * YFac), XFac, YFac);
        }
        public void Draw(Graphics g)
        {
            Pen DrawingPen = new Pen(Color.Gray, 1);
            g.DrawRectangle(DrawingPen, 10, 10, 800, 800);
            SolidBrush brush;
            for (int x = 0; x < XMax; x++)
            {
                for (int y = 0; y < YMax; y++)
                {

                    if (BinData == null)
                    {
                        brush = new SolidBrush(Color.LightGray);
                    }
                    else
                    {
                        if (BinData[x, y] == 15)
                        {
                            brush = new SolidBrush(Color.White);
                        }
                        else if (BinData[x, y] == 7)
                        {
                            brush = new SolidBrush(Color.LightSkyBlue);
                        }
                        else if (BinData[x, y] == 18)
                        {
                            brush = new SolidBrush(Color.Green);
                        }
                        else if (BinData[x, y] == 23)
                        {
                            brush = new SolidBrush(Color.Yellow);
                        }
                        else
                        {
                            brush = new SolidBrush(Color.DarkGray);
                        }
                    }
                    g.FillRectangle(brush, 10 + (x * XFac), 10 + (y * YFac), XFac, YFac);
                    //if (BinData[x, y] != 17) g.DrawRectangle(DrawingPen, 10 + (x * XFac), 10 + (y * YFac), XFac, YFac);
                }
            }
            
        }
    }
}
