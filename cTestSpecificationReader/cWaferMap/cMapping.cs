using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cWaferMap
{
    struct s_MappingInfo
    {
        public s_XY Probe_Position;
        public bool Test;
        public bool Probe_Status;
    }
    
    struct s_ProbeInfo
    {
        public int Count;
        public s_XY[] Probe;
        public bool[,] Probe_Setting;
        public int X_width;
        public int Y_hieght;
    }
    public enum e_ProbeMovement
    {
        LeftRight = 0,
        UpDown
    }
    public class cMapping
    {
        private int XMax;
        private int YMax;
        private s_MappingInfo[,] Map;
        private s_WaferMap[,] WaferMap;
        private s_XY Ref_Position;
        private s_XY FirstTestPos;
        private s_ProbeInfo Probe = new s_ProbeInfo();
        private e_ProbeMovement ProbeMovement;

        public cMapping()
        {
            Ref_Position.X = 0;
            Ref_Position.Y = 0;
            ProbeMovement = e_ProbeMovement.LeftRight;
            Set_ProbeShape();
        }
        public void Set_ProbeShape()
        {
            Probe.Probe = new s_XY[2];
            Probe.Probe[0].X = 0;
            Probe.Probe[0].Y = 0;
            Probe.Probe[1].X = 1;
            Probe.Probe[1].Y = 1;
            Probe.Probe_Setting = new bool[2,2];
            Probe.Probe_Setting[0,0] = true;
            Probe.Probe_Setting[1,1] = true;
            Probe.Count = Probe.Probe.Length;
            Probe.X_width = 0;
            Probe.Y_hieght = 0;
            for(int i = 0; i < Probe.Count; i++)
            {
                if (Probe.X_width < (Probe.Probe[i].X + 1))
                {
                    Probe.X_width = (Probe.Probe[i].X + 1);
                }
                if (Probe.Y_hieght < (Probe.Probe[i].Y + 1))
                {
                    Probe.Y_hieght = (Probe.Probe[i].Y + 1);
                }
            }
        }
        public s_WaferMap[,] WaferMap_Data
        {
            set
            {
                WaferMap = value;
                XMax = value.GetUpperBound(0) + 1;
                YMax = value.GetUpperBound(1) + 1;
                Map = new s_MappingInfo[XMax, YMax];
                Init_Mapping();
            }
        }
        public s_XY Parse_FirstTestPos
        {
            set
            {
                FirstTestPos = value;
            }
        }
        public int[,] Parse_Data
        {
            set
            {
                XMax = value.GetUpperBound(0) + 1;
                YMax = value.GetUpperBound(1) + 1;
                Map = new s_MappingInfo[XMax, YMax];
                Init_Mapping();
            }
        }
        public s_XY parse_reference_point
        {
            set
            {
                Ref_Position = value;
            }
        }
        public s_XY Get_ProbePosition(s_XY pos)
        {
            return Map[pos.X, pos.Y].Probe_Position;
        }
        public s_XY Get_ProbePosition(int X, int Y)
        {
            return Map[X, Y].Probe_Position;
        }
        public int X_moveSet(int X, int Y)
        {
            int move = 0;
            bool found = false;
            int step = 0;
            if (X > 0)
            {
                step = 1;
            }
            else
            {
                step = -1;
            }
            do
            {
                if (Probe.Probe_Setting[X - move, Y])
                {
                    found = true;
                }
                else
                {
                    move += step;
                }
            } while (!found);
            return move;
        }
        public void Init_Mapping()
        {
            int X_off, Y_off, X_Set, Y_Set, move_X;
            for (int iY = 0; iY < YMax; iY++)
            {
                for (int iX = 0; iX < XMax; iX++)
                {
                    if (WaferMap[iX, iY].DieType == e_DieType.GOOD)
                    {
                        if (ProbeMovement == e_ProbeMovement.LeftRight)
                        {
                            X_off = (iX - FirstTestPos.X) % Probe.X_width;
                            Y_off = (iY) % Probe.Y_hieght;
                        }
                        else
                        {
                            X_off = (iX) % Probe.X_width;
                            Y_off = (iY - FirstTestPos.Y) % Probe.Y_hieght;
                        }

                        X_Set = iX - X_off;
                        Y_Set = iY - Y_off;
                        move_X = X_moveSet(X_off, Y_off);
                        X_Set = X_Set + move_X;
                        Map[iX, iY].Probe_Position.X = X_Set;
                        Map[iX, iY].Probe_Position.Y = Y_Set;
                        //if (WaferMap[iX, iY].DieType == e_DieType.GOOD)
                        //{
                            Map[iX, iY].Test = true;
                        //}
                    }
                    else
                    {
                        Map[iX, iY].Probe_Position.X = -999;
                        Map[iX, iY].Probe_Position.Y = -999;
                    }
                }
            }
            //bool SkipSetting = false;
            //for (int iY = 0; iY < YMax; iY++)
            //{
            //    for (int iX = 0; iX < XMax; iX++)
            //    {
            //        for (int iP = 0; iP < Probe.Count; iP++)
            //        {
            //            if (iP == 0)
            //            {
            //                SkipSetting = false;
            //                if (Map[iX + Probe.Probe[iP].X, iY + Probe.Probe[iP].Y].Probe_Status)
            //                {
            //                    SkipSetting = true;
            //                }
            //            }
            //            if ((iX + Probe.Probe[iP].X < XMax) && (iY + Probe.Probe[iP].Y < YMax))
            //            {

            //                if ((!Map[iX + Probe.Probe[iP].X, iY + Probe.Probe[iP].Y].Probe_Status) && !SkipSetting)
            //                {
            //                    Map[iX + Probe.Probe[iP].X, iY + Probe.Probe[iP].Y].Probe_Position.X = iX - Ref_Position.X;
            //                    Map[iX + Probe.Probe[iP].X, iY + Probe.Probe[iP].Y].Probe_Position.Y = iY - Ref_Position.Y;
            //                    Map[iX + Probe.Probe[iP].X, iY + Probe.Probe[iP].Y].Probe_Status = true;
            //                    if (WaferMap[iX + Probe.Probe[iP].X, iY + Probe.Probe[iP].Y].DieType == e_DieType.GOOD)
            //                    {
            //                        if (iP == 0)
            //                        {
            //                            Map[iX + Probe.Probe[iP].X, iY + Probe.Probe[iP].Y].Test = true;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }
        }
}
