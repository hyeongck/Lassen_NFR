using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cMapReader
{
    public enum e_FlatLocation
    {
        Bottom = 0,
        Left,
        Right,
        Top
    }
    public struct s_WaferInfo
    {
        public string RevisionNumber;
        public string LotNumber;
        public string WaferNumber;
        public DateTime DateProbe;
        public s_Wafer_XY XY_Info;
        public string Status;
        public s_AddInfo[] AddInfo;
        public string FLAT_Location;
        public string Raw_Data;
        public string[,] Arr_Data;  // Need to re-declare
    }
    public struct s_AddInfo
    {
        public string Char_ASCII;
        public int Count;
        public string Code;
    }
    public struct s_Wafer_XY
    {
        public s_XY Size;
        public s_XY ReferencePoint;
        public s_XY Step_microns;
    }
    public struct s_XY
    {
        public int X;
        public int Y;
    }
    public class cWaferMap
    {
        public s_WaferInfo WaferData = new s_WaferInfo();
        
        public string Wafer_FileName { get; set; }
        public string Output_FileName { get; set; }
        public void ReadFile()
        {
            string[] DataStr;
            DataStr = System.IO.File.ReadAllLines(Wafer_FileName,Encoding.ASCII);

            WaferData.AddInfo = new s_AddInfo[5];
            WaferData.Raw_Data = DataStr[0];
            WaferData.RevisionNumber = DataStr[0].Substring(0, 2);
            WaferData.LotNumber = DataStr[0].Substring(12, 10);
            WaferData.WaferNumber = DataStr[0].Substring(22, 8);
            WaferData.DateProbe = cStr2Date(DataStr[0].Substring(42, 6));
            WaferData.XY_Info.Size.X = Convert.ToInt32(DataStr[0].Substring(48, 3));
            WaferData.XY_Info.Size.Y = Convert.ToInt32(DataStr[0].Substring(51, 3));
            WaferData.XY_Info.ReferencePoint.X = Convert.ToInt32(DataStr[0].Substring(54, 3));
            WaferData.XY_Info.ReferencePoint.Y = Convert.ToInt32(DataStr[0].Substring(57, 3));
            WaferData.XY_Info.Step_microns.X = Convert.ToInt32(DataStr[0].Substring(60, 5));
            WaferData.XY_Info.Step_microns.Y = Convert.ToInt32(DataStr[0].Substring(65, 5));
            WaferData.Status = DataStr[0].Substring(70, 6);
            for (int iAdd = 0; iAdd < 5; iAdd++)
            {
                WaferData.AddInfo[iAdd].Char_ASCII = DataStr[0].Substring(76 + (iAdd * 16), 1);
                WaferData.AddInfo[iAdd].Count = cStr2Int(DataStr[0].Substring(77 + (iAdd * 16), 5));
                WaferData.AddInfo[iAdd].Code = DataStr[0].Substring(82 + (iAdd * 16), 10);
            }
            WaferData.FLAT_Location = DataStr[0].Substring(207, 1);

            int iDataCalc = WaferData.XY_Info.Size.X * WaferData.XY_Info.Size.Y;
            int iDataAct = (DataStr[0].Trim().Length - 244);

            if (iDataAct == iDataCalc)
            {
                WaferData.Arr_Data = new string[WaferData.XY_Info.Size.X, WaferData.XY_Info.Size.Y];
                int X = 0;
                int Y = 0;
                for (int iDat = 0; iDat < (DataStr[0].Length - 244); iDat++)
                {
                    WaferData.Arr_Data[X, Y] = DataStr[0].Substring((244 + iDat), 1);
                    X++;
                    if (X == (WaferData.XY_Info.Size.X))
                    {
                        X = 0;
                        Y++;
                    }
                }
            }
            else
            {
                MessageBox.Show("Mismatch Wafer Information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void WriteFile()
        {
            int TotalLine;
            string[] OutputData;
            TotalLine = 27 + WaferData.XY_Info.Size.Y;
            OutputData = new string[TotalLine];
            OutputData[0] = "Revision Number," + WaferData.RevisionNumber;
            OutputData[1] = "Lot Number," + WaferData.LotNumber;
            OutputData[2] = "Wafer Number," + WaferData.WaferNumber;
            OutputData[3] = "Date of Probing," + WaferData.DateProbe.ToShortDateString();
            OutputData[4] = "Size of X Dimension," + WaferData.XY_Info.Size.X.ToString();
            OutputData[5] = "Size of Y Dimension," + WaferData.XY_Info.Size.Y.ToString();
            OutputData[6] = "X Coordinate - Alignment Ref Point," + WaferData.XY_Info.ReferencePoint.X.ToString();
            OutputData[7] = "Y Coordinate - Alignment Ref Point," + WaferData.XY_Info.ReferencePoint.Y.ToString();
            OutputData[8] = "X dimension for Step n Repeat (microns)," + WaferData.XY_Info.Step_microns.X.ToString();
            OutputData[9] = "Y dimension for Step n Repeat (microns)," + WaferData.XY_Info.Step_microns.Y.ToString();
            OutputData[10] = "Status," + WaferData.Status;
            for (int iCat = 0; iCat < 5; iCat++)
            {
                OutputData[11 + (3 * iCat)] = "ASCII char for sort category " + (iCat + 1).ToString() + "," + WaferData.AddInfo[iCat].Char_ASCII;
                OutputData[12 + (3 * iCat)] = "Count Number of good for sort category " + (iCat + 1).ToString() + "," + WaferData.AddInfo[iCat].Count.ToString();
                OutputData[13 + (3 * iCat)] = "Code for sort category " + (iCat + 1).ToString() + "," + WaferData.AddInfo[iCat].Code;
            }
            OutputData[26] = "Flat Location," + WaferData.FLAT_Location;
            for (int iData = 0; iData < WaferData.XY_Info.Size.Y; iData++)
            {
                for(int iArr = 0; iArr < WaferData.XY_Info.Size.X; iArr++)
                {
                    OutputData[27 + iData] += WaferData.Arr_Data[iArr, iData] + ",";
                }
            }
            System.IO.File.WriteAllLines(Output_FileName, OutputData);
        }
        public void ReadFile(string FileName)
        {
            Wafer_FileName = FileName;
            ReadFile();
        }

        DateTime cStr2Date(string inputStr)
        {
            string tmpStr;
            tmpStr = inputStr.Substring(2, 2) + "-"
                     + inputStr.Substring(4, 2) + "-"
                     + inputStr.Substring(0, 2);
            return Convert.ToDateTime(tmpStr);
        }
        int cStr2Int(string inputStr)
        {
            if (inputStr.Trim() != "")
            {
                return Convert.ToInt32(inputStr);
            }
            else
            {
                return 0;
            }
        }
    }
}
