using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cTestResultsReader
{
    public struct s_TestHeader
    {
        public s_GlobalInfo GlobalInfo;
        public s_SiteDetails SiteDetails;
        public s_Options Options;
        public s_ConditionName ConditionName;
        public s_MiscDetails Misc_Details;
        public string Correlation_FileName;
        public int Start_Data_Header_Row;
        public int Revision;
        public bool b_XY;
    }
    public struct s_MiscDetails
    {
        public string PcbLot;
        public string AssemblyLot;
        public string VerificationUnit;
    }
    public struct s_GlobalInfo
    {
        public int RowNo;
        public string Date;
        public string SetupTime;
        public string StartTime;
        public string FinishTime;
        public string ProgramName;
        public string ProgramRevision;
        public string Lot;
        public string SubLot;
        public string Wafer;
        public string WaferOrientation;
        public string TesterName;
        public string TesterType;
        public string Product;
        public string Operator;
        public string ExecType;
        public string ExecRevision;
        public string RtstCode;
        public string PackageType;
        public string Family;
        public string SpecName;
        public string SpecVersion;
        public string FlowID;
        public string DesignRevision;
    }
    public struct s_SiteDetails
    {
        public int RowNo;
        public string HeadNumber;
        public string Testing_sites;
        public string Handler_ID;
        public string Handler_type;
        public string LoadBoardName;
    }
    public struct s_Options
    {
        public int RowNo;
        public string UnitsMode;
    }
    public struct s_ConditionName
    {
        public int RowNo;
        public string ConditionName;
        public string EMAIL_ADDRESS;
        public string Translator;
        public string Wafer_Diameter;
        public string Facility;
        public string HostIpAddress;
        public string Temperature;
    }
    public struct s_RegenInfo
    {
        public string RegenSpec;
        public string RegenSpecVersion;
    }

    public struct s_ResultHeader
    {
        public string[] TestParameter_Name;
        public int[] TestNumber;
        public string[] Patterns;
        public string[] Units;
        public string[] HighL;
        public string[] LowL;
        public string[] Correlation_Data;
    }
    public struct s_ResultsData
    {
        public string ID;
        public int SBin;
        public int HBin;
        public int Die_X;
        public int Die_Y;
        public string Site;
        public double Time;
        public int TotalTest;
        public string Lot_ID;
        public string Wafer_ID;
        public int PassFail;
        public string TimeStamp;
        public double IndexTime;
        public string PartSN;
        public string SWBinName;
        public string HWBinName;
        public double[] Data;
        public bool ValidData;
    }
    
    public struct s_PositionXY
    {
        public int Max_X;
        public int Min_X;
        public int Max_Y;
        public int Min_Y;
    }
    public struct s_PositionInfo
    {
        public bool Valid;
        public s_PositionXY XY_MinMax;
        public int[,] Match_Position;
    }
    public struct s_Results
    {
        public string[] RawData;
        public s_TestHeader TestHeader;
        public s_ResultHeader ResultHeader;
        public s_ResultsData[] ResultData;
        public s_PositionInfo XY_Info;
        public bool b_XY;
    }
    public class cTestResultsReader
    {
        public string Result_FileName;
        public int MaxX = 5000;
        public int MaxY = 5000;

        public bool bExtract_XY;

        public s_RegenInfo Regen_Info;

        private s_TestHeader TestHeader = new s_TestHeader();
        private s_ResultHeader ResultHeader;
        private s_ResultsData[] ResultData;

        private s_PositionXY XY_Info = new s_PositionXY();

        private string[] Data;
        private int StartResults_Column = 11;
        private int EndResult_Column = 5;
        private int ResultHeader_Row = 5;
        private int ResultStartHeader_Row;

        private int i_InvalidCount = 0;

        int nSite;

        private int[,] Match_Position;

        public cTestResultsReader()
        {
            Match_Position = new int[MaxX, MaxY];
            Init_MatchPosition();
            XY_Info.Max_X = -99999;
            XY_Info.Min_X = 99999;
            XY_Info.Max_Y = -99999;
            XY_Info.Min_Y = 99999;
            nSite = 1;
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(true);
        }
        private void Init_MatchPosition()
        {
            for (int x = 0; x < MaxX; x++)
            {
                for (int y = 0; y < MaxY; y++)
                {
                    Match_Position[x, y] = -1;
                }
            }
        }

        public bool Read_File()
        {
            if ((Result_FileName != null) && (Result_FileName != ""))
            {
                Data = System.IO.File.ReadAllLines(Result_FileName);
                i_InvalidCount = Get_InvalidDataCount();
                ProcessData();

                return true;
            }
            else return false;
        }
        public void ProcessData()
        {
            string[] tmpStr;
            bool b_TestHeader;
            bool b_ResultHeader;
            //bool b_ResultData;
            int Results_StartLine;

            b_TestHeader = false;
            b_ResultHeader = false;
            //b_ResultData = false;

            for (int iRow = 0; iRow < Data.Length; iRow++)
            {
                if (b_TestHeader)
                {
                    Process_TestHeader(iRow);
                    b_TestHeader = false;
                }
                if (b_ResultHeader)
                {
                    Process_Result(iRow);
                    b_ResultHeader = false;
                    break;
                }
                //if (b_ResultData)
                //{

                //    b_ResultData = false;
                //}
                tmpStr = Data[iRow].Split(',');
                if (tmpStr[0].ToUpper() == "--- GLOBAL INFO:")
                {
                    b_TestHeader = true;
                }
                if (tmpStr[0].ToUpper() == "#CF")
                {
                    ResultStartHeader_Row = iRow;
                    TestHeader.Start_Data_Header_Row = ResultStartHeader_Row;
                    b_ResultHeader = true;
                }
                //if (tmpStr[0].ToUpper() == "LOWL")
                //{
                //    b_ResultData = true;
                //    Results_StartLine = iRow + 1;
                //}
            }
            if(Convert.ToInt32(TestHeader.SiteDetails.Testing_sites) != nSite)
            {
                TestHeader.SiteDetails.Testing_sites = nSite.ToString();
            }

        }
        string ReturnStr(int Row, int Item)
        {
            string[] tmpStr = Data[Row].Split(',');
            if (tmpStr[Item] != null)
            {
                return (tmpStr[Item]);
            }
            return ("");
        }
        private void Process_TestHeader(int Row)
        {
            TestHeader.Revision = 0;

            TestHeader.GlobalInfo = new s_GlobalInfo();
            TestHeader.SiteDetails = new s_SiteDetails();
            TestHeader.Options = new s_Options();
            TestHeader.ConditionName = new s_ConditionName();

            TestHeader.GlobalInfo.RowNo = Row - 1;
            TestHeader.GlobalInfo.Date = ReturnStr(Row, 1);
            TestHeader.GlobalInfo.SetupTime = ReturnStr(Row + 1, 1);
            TestHeader.GlobalInfo.StartTime = ReturnStr(Row + 2, 1);
            TestHeader.GlobalInfo.FinishTime = ReturnStr(Row + 3, 1);
            TestHeader.GlobalInfo.ProgramName = ReturnStr(Row + 4, 1);
            TestHeader.GlobalInfo.ProgramRevision = ReturnStr(Row + 5, 1);
            TestHeader.GlobalInfo.Lot = ReturnStr(Row + 6, 1);
            TestHeader.GlobalInfo.SubLot = ReturnStr(Row + 7, 1);
            TestHeader.GlobalInfo.Wafer = ReturnStr(Row + 8, 1);
            TestHeader.GlobalInfo.WaferOrientation = ReturnStr(Row + 9, 1);
            TestHeader.GlobalInfo.TesterName = ReturnStr(Row + 10, 1);
            TestHeader.GlobalInfo.TesterType = ReturnStr(Row + 11, 1);
            TestHeader.GlobalInfo.Product = ReturnStr(Row + 12, 1);
            TestHeader.GlobalInfo.Operator = ReturnStr(Row + 13, 1);
            TestHeader.GlobalInfo.ExecType = ReturnStr(Row + 14, 1);
            TestHeader.GlobalInfo.ExecRevision = ReturnStr(Row + 15, 1);
            TestHeader.GlobalInfo.RtstCode = ReturnStr(Row + 16, 1);
            TestHeader.GlobalInfo.PackageType = ReturnStr(Row + 17, 1);
            TestHeader.GlobalInfo.Family = ReturnStr(Row + 18, 1);
            TestHeader.GlobalInfo.SpecName = ReturnStr(Row + 19, 1);
            TestHeader.GlobalInfo.SpecVersion = ReturnStr(Row + 20, 1);
            TestHeader.GlobalInfo.FlowID = ReturnStr(Row + 21, 1);
            TestHeader.GlobalInfo.DesignRevision = ReturnStr(Row + 22, 1);

            TestHeader.SiteDetails.RowNo = Row + 23 - 1;
            TestHeader.SiteDetails.HeadNumber = ReturnStr(Row + 23, 1);
            TestHeader.SiteDetails.Testing_sites = CEmpty2Str( ReturnStr(Row + 24, 1));
            TestHeader.SiteDetails.Handler_ID = ReturnStr(Row + 25, 1);
            TestHeader.SiteDetails.Handler_type = ReturnStr(Row + 26, 1);
            TestHeader.SiteDetails.LoadBoardName = ReturnStr(Row + 27, 1);

            TestHeader.Options.RowNo = Row + 28 - 1;
            TestHeader.Options.UnitsMode = ReturnStr(Row + 29, 1);

            TestHeader.ConditionName.RowNo = Row + 30 - 1;
            TestHeader.ConditionName.ConditionName = ReturnStr(Row + 30, 1);
            TestHeader.ConditionName.EMAIL_ADDRESS = ReturnStr(Row + 31, 1);
            TestHeader.ConditionName.Translator = ReturnStr(Row + 32, 1);
            TestHeader.ConditionName.Wafer_Diameter = ReturnStr(Row + 33, 1);
            TestHeader.ConditionName.Facility = ReturnStr(Row + 34, 1);
            TestHeader.ConditionName.HostIpAddress = ReturnStr(Row + 35, 1);
            TestHeader.ConditionName.Temperature = ReturnStr(Row + 36, 1);

            if (ReturnStr(Row + 37, 0).Trim().ToUpper() == "PcbLot".ToUpper())
            {
                TestHeader.Revision = 1;
                TestHeader.Misc_Details.PcbLot = ReturnStr(Row + 37, 1);
                TestHeader.Misc_Details.AssemblyLot = ReturnStr(Row + 38, 1);
                TestHeader.Misc_Details.VerificationUnit = ReturnStr(Row + 39, 1);
                EndResult_Column = 6;
            }
            else
            {
                TestHeader.Misc_Details.PcbLot = "";
                TestHeader.Misc_Details.AssemblyLot = "";
                TestHeader.Misc_Details.VerificationUnit = "";
            }

            if (TestHeader.SiteDetails.Handler_ID.ToUpper().Contains("WAFER") || TestHeader.SiteDetails.Handler_ID.ToUpper().Contains("STRIP") ||
                TestHeader.SiteDetails.Handler_type.ToUpper().Contains("WAFER") || TestHeader.SiteDetails.Handler_type.ToUpper().Contains("STRIP"))
            {
                TestHeader.b_XY = true;
            }
        }
        private void Process_Result(int Row)
        {
            string[] tmpStr;
            string[] tmpArray;
            //int TotalData = Data.Length - Row - ResultHeader_Row - i_InvalidCount - 1;
            int TotalParameter;
            int iCnt = 0;
            int Loc_X, Loc_Y;

            bool[,] ValidData = new bool[MaxX, MaxY];

            tmpStr = Data[Row].Split(',');
            TotalParameter = tmpStr.Length - (StartResults_Column - 1) - EndResult_Column;
            
            //ResultData = new s_ResultsData[TotalData];
            //ResultData = new s_ResultsData[1];
            TestHeader.Correlation_FileName = ReturnStr(Row - 1, 1);
            ResultHeader.Correlation_Data = new string[TotalParameter];
            tmpStr = Data[Row - 1].Split(',');
            Array.Copy(tmpStr, (StartResults_Column - 1), ResultHeader.Correlation_Data, 0, TotalParameter);
            
            tmpStr = Data[Row].Split(',');
            
            ResultHeader.TestParameter_Name = new string[TotalParameter];
            ResultHeader.TestNumber = new int[TotalParameter];
            ResultHeader.Patterns = new string[TotalParameter];
            ResultHeader.Units = new string[TotalParameter];
            ResultHeader.HighL = new string[TotalParameter];
            ResultHeader.LowL = new string[TotalParameter];
            tmpArray = new string[TotalParameter];

            Array.Copy(tmpStr, (StartResults_Column - 1), ResultHeader.TestParameter_Name, 0, TotalParameter);
            tmpStr = Data[Row + 1].Split(',');
            Array.Copy(tmpStr, (StartResults_Column - 1), tmpArray, 0, TotalParameter);
            ResultHeader.TestNumber = Array.ConvertAll<string, int>(tmpArray, Convert.ToInt32);
            tmpStr = Data[Row + 2].Split(',');
            Array.Copy(tmpStr, (StartResults_Column - 1), ResultHeader.Patterns, 0, TotalParameter);
            tmpStr = Data[Row + 3].Split(',');
            Array.Copy(tmpStr, (StartResults_Column - 1), ResultHeader.Units, 0, TotalParameter);
            tmpStr = Data[Row + 4].Split(',');
            Array.Copy(tmpStr, (StartResults_Column - 1), ResultHeader.HighL, 0, TotalParameter);
            tmpStr = Data[Row + 5].Split(',');
            Array.Copy(tmpStr, (StartResults_Column - 1), ResultHeader.LowL, 0, TotalParameter);

            if (TestHeader.b_XY)
            {
                int TotalDataCount = 0;

                for (int iRow = Row + ResultHeader_Row + 1; iRow < Data.Length; iRow++)
                {
                    string[] infodata = Data[iRow].Split(',');
                    if (infodata[0] == "" || infodata[0] == null) break;
                    Loc_X = int.Parse(infodata[3]);
                    Loc_Y = int.Parse(infodata[4]);

                    if (!((Loc_X == -99999) || (Loc_Y == -99999)))
                    {
                        if (!ValidData[mPos_X(Loc_X), mPos_Y(Loc_Y)])
                        {
                            ValidData[mPos_X(Loc_X), mPos_Y(Loc_Y)] = true;
                            Match_Position[mPos_X(Loc_X), mPos_Y(Loc_Y)] = TotalDataCount;
                            TotalDataCount++;
                        }
                    }
                }
                ResultData = new s_ResultsData[TotalDataCount];
                for (int iRow = Row + ResultHeader_Row + 1; iRow < Data.Length; iRow++)
                {
                    //if (iCnt >= 1)
                    //{
                    //    Array.Resize(ref ResultData, iCnt + 1);
                    //}
                    tmpStr = Data[iRow].Split(',');
                    if (tmpStr[0] == "" || tmpStr[0] == null) break;
                    Loc_X = int.Parse(tmpStr[3]);
                    Loc_Y = int.Parse(tmpStr[4]);

                    if (!((Loc_X == -99999) || (Loc_Y == -99999)))
                    {
                        iCnt = Match_Position[mPos_X(Loc_X), mPos_Y(Loc_Y)];
                        ResultData[iCnt].Data = new double[TotalParameter];
                        ResultData[iCnt].ID = tmpStr[0];
                        ResultData[iCnt].SBin = int.Parse(tmpStr[1]);
                        ResultData[iCnt].HBin = int.Parse(tmpStr[2]);

                        if (bExtract_XY)
                        {
                            ResultData[iCnt].Die_X = Extract_ID_XY(tmpStr[(StartResults_Column - 1) + TotalParameter + 3], 1, tmpStr[3]);
                            ResultData[iCnt].Die_Y = Extract_ID_XY(tmpStr[(StartResults_Column - 1) + TotalParameter + 3], 2, tmpStr[4]);
                        }
                        else
                        {
                            ResultData[iCnt].Die_X = int.Parse(tmpStr[3]);
                            ResultData[iCnt].Die_Y = int.Parse(tmpStr[4]);
                        }

                        //Match_Position[mPos_X(ResultData[iCnt].Die_X), mPos_Y(ResultData[iCnt].Die_Y)] = iCnt;
                        Process_XY(ResultData[iCnt].Die_X, ResultData[iCnt].Die_Y);
                        ResultData[iCnt].Site = tmpStr[5];
                        if (Convert.ToInt32(tmpStr[5]) > nSite)
                        {
                            nSite = Convert.ToInt32(tmpStr[5]);
                        }
                        ResultData[iCnt].Time = double.Parse(tmpStr[6]);
                        ResultData[iCnt].TotalTest = int.Parse(tmpStr[7]);
                        ResultData[iCnt].Lot_ID = tmpStr[8];
                        ResultData[iCnt].Wafer_ID = tmpStr[9];
                        ResultData[iCnt].PassFail = int.Parse(tmpStr[(StartResults_Column - 1) + TotalParameter]);
                        ResultData[iCnt].TimeStamp = tmpStr[(StartResults_Column - 1) + TotalParameter + 1];
                        ResultData[iCnt].IndexTime = double.Parse(tmpStr[(StartResults_Column - 1) + TotalParameter + 2]);
                        ResultData[iCnt].PartSN = tmpStr[(StartResults_Column - 1) + TotalParameter + 3];
                        ResultData[iCnt].SWBinName = tmpStr[(StartResults_Column - 1) + TotalParameter + 4];
                        if (TestHeader.Revision == 1)
                        {
                            ResultData[iCnt].HWBinName = tmpStr[(StartResults_Column - 1) + TotalParameter + 5];
                        }
                        else
                        {
                            ResultData[iCnt].HWBinName = "";
                        }
                        Array.Copy(tmpStr, StartResults_Column - 1, tmpArray, 0, TotalParameter);
                        ResultData[iCnt].Data = Array.ConvertAll<string, double>(tmpArray, Convert.ToDouble);
                        ResultData[iCnt].ValidData = true;
                        //iCnt++;
                    }

                }
            }
            else
            {
                ResultData = new s_ResultsData[Data.Length - (Row + ResultHeader_Row + 1)];
                for (int iRow = Row + ResultHeader_Row + 1; iRow < Data.Length; iRow++)
                {
                    tmpStr = Data[iRow].Split(',');
                    ResultData[iCnt].Data = new double[TotalParameter];
                    ResultData[iCnt].ID = tmpStr[0];
                    ResultData[iCnt].SBin = int.Parse(tmpStr[1]);
                    ResultData[iCnt].HBin = int.Parse(tmpStr[2]);
                    ResultData[iCnt].Die_X = int.Parse(tmpStr[3]);
                    ResultData[iCnt].Die_Y = int.Parse(tmpStr[4]);
                    ResultData[iCnt].Site = tmpStr[5];
                    ResultData[iCnt].Time = double.Parse(tmpStr[6]);
                    ResultData[iCnt].TotalTest = int.Parse(tmpStr[7]);
                    ResultData[iCnt].Lot_ID = tmpStr[8];
                    ResultData[iCnt].Wafer_ID = tmpStr[9];
                    ResultData[iCnt].PassFail = int.Parse(tmpStr[(StartResults_Column - 1) + TotalParameter]);
                    ResultData[iCnt].TimeStamp = tmpStr[(StartResults_Column - 1) + TotalParameter + 1];
                    ResultData[iCnt].IndexTime = double.Parse(tmpStr[(StartResults_Column - 1) + TotalParameter + 2]);
                    ResultData[iCnt].PartSN = tmpStr[(StartResults_Column - 1) + TotalParameter + 3];
                    ResultData[iCnt].SWBinName = tmpStr[(StartResults_Column - 1) + TotalParameter + 4];
                    if (TestHeader.Revision == 1)
                    {
                        ResultData[iCnt].HWBinName = tmpStr[(StartResults_Column - 1) + TotalParameter + 5];
                    }
                    else
                    {
                        ResultData[iCnt].HWBinName = "";
                    }
                    Array.Copy(tmpStr, StartResults_Column - 1, tmpArray, 0, TotalParameter);
                    ResultData[iCnt].Data = Array.ConvertAll<string, double>(tmpArray, Convert.ToDouble);
                    ResultData[iCnt].ValidData = true;
                    iCnt++;
                }
            }
            
            
        }
        private int Get_InvalidDataCount()
        {
            int i_Count = 0;
            string[] tmpStr;
            bool b_StartCheck = false;
            for (int iRow = 0; iRow < Data.Length; iRow++)
            {
                tmpStr = Data[iRow].Split(',');
                if (b_StartCheck)
                {
                    if ((tmpStr[3].Trim() != "") && (tmpStr[4].Trim() != ""))
                    {
                        if ((int.Parse(tmpStr[3]) == -99999) || (int.Parse(tmpStr[4]) == -99999))
                        {
                            i_Count++;
                        }
                    }
                }
                if (tmpStr[0].Trim().ToUpper() == "LOWL")
                {
                    b_StartCheck = true;
                }
            }
            return i_Count;
        }
        private int Extract_ID_XY(string inputStr, int Item, string Default)
        {
            string[] tmpStr = inputStr.Trim().Split('_');
            if (tmpStr.Length == 3)
            {
                return Convert.ToInt32(tmpStr[Item].Trim('\"'));
            }
            else
            {
                return Convert.ToInt32(Default);
            }
        }
        public int mPos_X(int X)
        {
            if (X < 0)
            {
                return (MaxX + X);
            }
            return (X);
        }
        public int mPos_Y(int Y)
        {
            if (Y < 0)
            {
                return (MaxY + Y);
            }
            return (Y);
        }

        public void Process_XY(int X, int Y)
        {
            if (X > XY_Info.Max_X) XY_Info.Max_X = X;
            if (X < XY_Info.Min_X) XY_Info.Min_X = X;
            if (Y > XY_Info.Max_Y) XY_Info.Max_Y = Y;
            if (Y < XY_Info.Min_Y) XY_Info.Min_Y = Y;
        }

        public int[,] parse_Match_Position
        {
            get
            {
                return (Match_Position);
            }
        }
        public s_PositionXY parse_XY_Info
        {
            get
            {
                return (XY_Info);
            }
        }
        public s_Results parse_Results
        {
            get
            {
                s_Results tmp = new s_Results();
                tmp.RawData = Data;
                tmp.TestHeader = TestHeader;
                tmp.ResultHeader = ResultHeader;
                tmp.ResultData = ResultData;
                tmp.XY_Info.XY_MinMax = parse_XY_Info;
                if ((tmp.XY_Info.XY_MinMax.Max_X == tmp.XY_Info.XY_MinMax.Min_X) && (tmp.XY_Info.XY_MinMax.Max_Y == tmp.XY_Info.XY_MinMax.Min_Y))
                {
                    tmp.XY_Info.Valid = false;
                }
                else
                {
                    tmp.XY_Info.Valid = true;
                }
                tmp.XY_Info.Match_Position = new int[MaxX, MaxY];
                tmp.XY_Info.Match_Position = parse_Match_Position;
                tmp.b_XY = TestHeader.b_XY;
                return tmp;
            }
        }
        public string[] generate_NewHeader()
        {
            string[] tmpStr = new string[5];
            tmpStr[0] = "--- Regen:,";
            tmpStr[1] = "RegenSpecName," + Regen_Info.RegenSpec;
            tmpStr[2] = "RegenSpecVersion," + Regen_Info.RegenSpecVersion;
            tmpStr[3] = "RegenDate," + DateTime.Now.ToString("yyyy_MM_dd");
            tmpStr[4] = "RegenTime," + DateTime.Now.ToString("H:mm:ss");
            return (tmpStr);
        }
        string CEmpty2Str(string inputStr)
        {
            if ((inputStr == "") || (inputStr == null))
            {
                return "1";
            }
            else
            {
                return inputStr;
            }
        }
    }
}
