using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cTestSpecificationReader
{
    public enum e_BinType
    {
        OR = 0,
        AND
    }
    public enum e_SBL_mode
    {
        More=0,
        Less,
        More_Or_Equal,
        Less_Or_Equal,
        Equal
    }
    
    #region "Structure"
    public struct s_Header
    {
        public string TestMode;
        public string Title;
        public string Date;
        public string Author;
        public string Description;
        public string Version;
    }
    public struct s_ControlParameters
    {
        public int TotalBinYieldAlarmLimit;
        public int StopAfterBinLimitFail;
        public int StopAfterAnyParaLimitFail;
        public int StopRequiredUnitsCount;
        public int ContinuousUnitsPassAlarmLimit;
        public int ContinuousUnitsFailAlarmLimit;
        public int MoveToQABinUnitsCount;
        public int MoveToQAHwBinNum;
    }
    public struct s_HW_Bin
    {
        public int Bin_Number;
        public string Name;
        public int[] Bin;
        public string BinState;
    }
    public struct s_SW_Bin
    {
        public int Bin_Number;
        public string Name;
        public string mode;
        public e_BinType BinType;
        public bool PassBin;
        public int[] Bin;
    }
    public struct s_SerialInfo
    {
        public string TestNumber;
        public string TestParameters;
        public bool ColumnDisplayFlag;
        public bool ChartDisplayFlag;
        public double FailThreshold;
        public bool Outlier_Param;
        public bool Pat_Param;
        public bool PSP_Param;
        //public int Outlier_Item;
    }
    public struct s_SerialBin
    {
        public s_Min[] Min;
        public s_Max[] Max;
        public bool PassBin;
    }
    
    public struct s_Min
    {
        public bool Pat;
        public bool Outlier;
        public bool Min_None;
        public double Min;
        public bool PSP;
    }
    public struct s_Max
    {
        public bool Pat;
        public bool Outlier;
        public bool Max_None;
        public double Max;
        public bool PSP;
    }
    public struct s_SpecFile
    {
        public cTestSpecificationReader.s_Header Header;
        public cTestSpecificationReader.s_ControlParameters ControlParameters;
        public cTestSpecificationReader.s_HW_Bin[] HW_Bin;
        public cTestSpecificationReader.s_SW_Bin[] SW_Bin;
        public cTestSpecificationReader.s_SerialInfo[] SerialInfo;
        public cTestSpecificationReader.s_SerialBin[] SerialBin;
        public cTestSpecificationReader.s_SBL[] SBL_Bin;
        public cTestSpecificationReader.s_AMap[] AMap_Bin;
        public bool SBL_Enable;
        public int PAT_Item;
        public int Outlier_Item;
    }
    public struct s_AMap
    {
        public string InkCode;
        public string MapCode;
        public int[] Bin;
    }
    public struct s_SBL
    {
        public double TriggerLevel;
        public e_SBL_mode Mode;
        public int[] Bin;
    }
    #endregion
    public class cTestSpec
    {
        public string Spec_FileName;

        private s_Header Spec_Header = new s_Header();
        private s_ControlParameters Spec_ControlParameters = new s_ControlParameters();
        private s_HW_Bin[] Spec_HW_Bin = new s_HW_Bin[1];
        private s_SW_Bin[] Spec_SW_Bin = new s_SW_Bin[1];
        private s_SerialInfo[] Spec_SerialInfo;
        private s_SerialBin[] Spec_SerialBin;
        private s_SBL[] Spec_SBL_bin = new s_SBL[1];
        private s_AMap[] Spec_AMap_bin = new s_AMap[1];
        private int Spec_Outlier_Item;
        private int Serial_Start;
        private int Serial_Param;
        private int Serial_Set;
        private bool Spec_SBL_Enable = false;
        private int Spec_PAT_Item;
        private int Spec_PSP_Item;
        private string[] Lines;

        public bool Read_File()
        {
            if ((Spec_FileName != null) && (Spec_FileName != ""))
            {
                Spec_Outlier_Item = 0;
                Lines = System.IO.File.ReadAllLines(Spec_FileName);
                Processing_Spec();
                Processing_Serial();
                return true;
            }
            else return false;
        }
        public void Processing_Spec()
        {
            bool b_Header = false;
            bool b_ControlParameters = false;
            bool b_HW_Bin = false;
            bool b_SW_Bin = false;
            bool b_Serial = false;
            bool b_SBL = false;
            bool b_AMap = false;
            string[] tmpStr;
            int Count = 0;
            int iBin;
            int iSet = 0;
            int iTmp;
            int iSBL_Bin;
            int iAMap_Bin;
            int iHWBin;
            int try_Integer;
            bool[] CheckPass_SWBin = new bool[1];
       
            for (int line = 0; line < Lines.Length; line++)
            {
                #region "Header"
                if (b_Header)
                {
                    Spec_Header.TestMode = parse_Str(Lines[line]);
                    Spec_Header.Title = parse_Str(Lines[line + 1]);
                    Spec_Header.Date = parse_Str(Lines[line + 2]);
                    Spec_Header.Author = parse_Str(Lines[line + 3]);
                    Spec_Header.Description = parse_Str(Lines[line + 4]);
                    Spec_Header.Version = parse_Str(Lines[line + 5]);
                    b_Header = false;
                }
                #endregion
                #region "Control Parameters"
                if (b_ControlParameters)
                {
                    Spec_ControlParameters.TotalBinYieldAlarmLimit = int.Parse(parse_Str(Lines[line]));
                    Spec_ControlParameters.StopAfterBinLimitFail = int.Parse(parse_Str(Lines[line + 1]));
                    Spec_ControlParameters.StopAfterAnyParaLimitFail = int.Parse(parse_Str(Lines[line + 2]));
                    Spec_ControlParameters.StopRequiredUnitsCount = int.Parse(parse_Str(Lines[line + 3]));
                    Spec_ControlParameters.ContinuousUnitsPassAlarmLimit = int.Parse(parse_Str(Lines[line + 4]));
                    Spec_ControlParameters.ContinuousUnitsFailAlarmLimit = int.Parse(parse_Str(Lines[line + 5]));
                    Spec_ControlParameters.MoveToQABinUnitsCount = int.Parse(parse_Str(Lines[line + 6]));
                    Spec_ControlParameters.MoveToQAHwBinNum = int.Parse(parse_Str(Lines[line + 7]));
                    b_ControlParameters = false;
                }
                #endregion
                #region "HW_Bin"
                if (b_HW_Bin)
                {
                    if (parse_Str(Lines[line], 0).ToUpper() == "#END")
                    {
                        b_HW_Bin = false;
                    }
                    else
                    {
                        tmpStr = Lines[line].Split(',');
                        if (Count > 0)
                        {
                            Array.Resize(ref Spec_HW_Bin, Count + 1);
                        }
                        //iHWBin = 0;
                        {
                            Spec_HW_Bin[Count].Bin_Number = int.Parse(tmpStr[0]);
                            Spec_HW_Bin[Count].Name = tmpStr[1];
                            Spec_HW_Bin[Count].BinState = cStr2State(tmpStr[1]);
                            //Spec_HW_Bin[Count].Bin[iHWBin] = int.Parse(tmpStr[2]);
                            Spec_HW_Bin[Count].Bin = new int[1];
                            iHWBin = 1;
                            for (int iHWB = 0; iHWB < tmpStr.Length - 2; iHWB++)
                            {
                                if ((tmpStr[iHWB + 2].Trim() != null) && (tmpStr[iHWB + 2].Trim() != ""))
                                {
                                    if (iHWBin > 1)
                                    {
                                        Array.Resize(ref Spec_HW_Bin[Count].Bin, iHWBin);
                                    }
                                    Spec_HW_Bin[Count].Bin[iHWB] = int.Parse(tmpStr[iHWB + 2]); 
                                    iHWBin++;
                                }
                            }
                            Count++;
                        }
                    }
                }
                #endregion
                #region "SW_Bin"
                if (b_SW_Bin)
                {
                    if (parse_Str(Lines[line], 0).ToUpper() == "#END")
                    {
                        b_SW_Bin = false;
                    }
                    else
                    {
                        tmpStr = Lines[line].Split(',');
                        if (Count > 0)
                        {
                            Array.Resize(ref Spec_SW_Bin, Count + 1);
                            Array.Resize(ref CheckPass_SWBin, Count + 1);
                        }
                        
                        {
                            //iBin = tmpStr.Length - 3;
                            iBin = 1;
                            for (int iCol=3; iCol < tmpStr.Length; iCol++)
                            {
                                if (tmpStr[iCol].Trim() == "")
                                {
                                    iBin = iCol - 3;
                                    break;
                                }
                            }
                            Spec_SW_Bin[Count].Bin_Number = int.Parse(tmpStr[0]);
                            Spec_SW_Bin[Count].mode = tmpStr[1];
                            if (Spec_SW_Bin[Count].mode.ToUpper().Contains("PASS"))
                            {
                                Spec_SW_Bin[Count].PassBin = true;
                                CheckPass_SWBin[Count] = true;
                            }
                            Spec_SW_Bin[Count].BinType = (e_BinType)Enum.Parse(typeof(e_BinType), tmpStr[2]);
                            Spec_SW_Bin[Count].Bin = new int[iBin];
                            for(int bin = 0; bin < iBin; bin++)
                            {
                                Spec_SW_Bin[Count].Bin[bin] = int.Parse(tmpStr[bin + 3]);
                            }
                            Count++;
                        }
                    }
                }
                #endregion
                #region "Serial"
                if(b_Serial)
                {
                    if (parse_Str(Lines[line], 0).ToUpper() == "#END")
                    {
                        Serial_Param = Count;
                        b_Serial = false;
                        tmpStr = Lines[Serial_Start].Split(',');
                        foreach(string s in tmpStr)
                        {
                            if (s.Trim() != "")
                            {
                                iSet = int.Parse(s);
                            }
                        }
                        Serial_Set = iSet;
                        //break;
                    }
                    else
                    {
                        Count++;
                    }
                }
                #endregion
                #region "SBL"
                if (b_SBL)
                {
                    if (parse_Str(Lines[line], 0).ToUpper() == "#END")
                    {
                        b_SBL = false;
                    }
                    else
                    {
                        Spec_SBL_Enable = true;
                        tmpStr = Lines[line].Split(',');
                        if (Count > 0)
                        {
                            Array.Resize(ref Spec_SBL_bin, Count + 1);
                        }
                        if (int.TryParse(tmpStr[0], out iTmp))
                        {
                            iSBL_Bin = 0;
                            for (int iSBL = 3; iSBL < tmpStr.Length; iSBL++)
                            {
                                if (tmpStr[iSBL].Trim() != "" && tmpStr[iSBL].Trim() != null)
                                {
                                    iSBL_Bin++;
                                }
                            }
                            
                            Spec_SBL_bin[Count].Bin = new int[iSBL_Bin];
                            Spec_SBL_bin[Count].TriggerLevel = Convert.ToDouble(tmpStr[1]);
                            switch (tmpStr[2].Trim())
                            {
                                case ">":
                                    Spec_SBL_bin[Count].Mode = e_SBL_mode.More;
                                    break;
                                case "<":
                                    Spec_SBL_bin[Count].Mode = e_SBL_mode.Less;
                                    break;
                                case ">=":
                                    Spec_SBL_bin[Count].Mode = e_SBL_mode.More_Or_Equal;
                                    break;
                                case "<=":
                                    Spec_SBL_bin[Count].Mode = e_SBL_mode.Less_Or_Equal;
                                    break;
                                case "=":
                                    Spec_SBL_bin[Count].Mode = e_SBL_mode.Equal;
                                    break;

                            }
                            iSBL_Bin = 0;
                            for (int iSBL = 3; iSBL < tmpStr.Length; iSBL++)
                            {
                                if (tmpStr[iSBL].Trim() != "")
                                {
                                    Spec_SBL_bin[Count].Bin[iSBL_Bin] = int.Parse(tmpStr[iSBL]);
                                    iSBL_Bin++;
                                }
                            }
                            Count++;
                        }

                    }
                }
                #endregion
                #region "AMap"
                if (b_AMap)
                {
                    if (parse_Str(Lines[line], 0).ToUpper() == "#END")
                    {
                        b_AMap = false;
                    }
                    else
                    {
                        tmpStr = Lines[line].Split(',');
                        if (Count > 0)
                        {
                            Array.Resize(ref Spec_AMap_bin, Count + 1);
                        }
                        if (int.TryParse(tmpStr[0], out iTmp))
                        {
                            iAMap_Bin = 0;
                            for (int iAMap = 3; iAMap < tmpStr.Length; iAMap++)
                            {
                                if (tmpStr[iAMap].Trim() != "" && tmpStr[iAMap].Trim() != null)
                                {
                                    iAMap_Bin++;
                                }
                            }

                            Spec_AMap_bin[Count].Bin = new int[iAMap_Bin];
                            Spec_AMap_bin[Count].InkCode = tmpStr[1];
                            Spec_AMap_bin[Count].MapCode = tmpStr[2];

                            iAMap_Bin = 0;
                            for (int iAMap = 3; iAMap < tmpStr.Length; iAMap++)
                            {
                                if (tmpStr[iAMap].Trim() != "")
                                {
                                    Spec_AMap_bin[Count].Bin[iAMap_Bin] = int.Parse(tmpStr[iAMap]);
                                    iAMap_Bin++;
                                }
                            }
                            Count++;
                        }
                    }
                }
                #endregion

                switch (parse_Str(Lines[line],0).ToUpper())
                {
                    case "#HEADER":
                        b_Header = true;
                        break;
                    case "#CONTROL_PARAMETERS":
                        b_ControlParameters = true;
                        break;
                    case "#HWBIN_DEFINITION":
                        b_HW_Bin = true;
                        Count = 0;
                        break;
                    case "#SWBIN_DEFINITION":
                        b_SW_Bin = true;
                        Count = 0;
                        break;
                    case "#SERIAL_DEFINITION":
                        b_Serial = true;
                        Serial_Start = line + 1;
                        Count = 0;
                        break;
                    case "#SBL_DEFINITION":
                        b_SBL = true;
                        Count = 0;
                        break;
                    case "#AMAP_DEFINITION":
                        b_AMap = true;
                        Count = 0;
                        break;
                }
            }

            //re-spec the sw bin

            s_SW_Bin[] tmpBin = new s_SW_Bin[Spec_SW_Bin.Length];
            tmpBin = Spec_SW_Bin;
            Spec_SW_Bin = new s_SW_Bin[tmpBin.Length];
            int iCnt = 0;
            for (int iBA = 0; iBA < Spec_SW_Bin.Length; iBA++)
            {
                if (CheckPass_SWBin[iBA])
                {
                    Spec_SW_Bin[iCnt] = tmpBin[iBA];
                    iCnt++;
                }
            }
            for (int iBA = 0; iBA < Spec_SW_Bin.Length; iBA++)
            {
                if (!CheckPass_SWBin[iBA])
                {
                    Spec_SW_Bin[iCnt] = tmpBin[iBA];
                    iCnt++;
                }
            }
        }
        public void Processing_Serial()
        {
            string[] tmpStr;
            int Count = 0;
            //bool tmpBool;
            
            Spec_SerialInfo = new s_SerialInfo[Serial_Param - 2];
            Spec_SerialBin = new s_SerialBin[Serial_Set];
            for (int iSet = 0; iSet < Serial_Set; iSet++)
            {
                Spec_SerialBin[iSet].Min = new s_Min[Serial_Param -2];
                Spec_SerialBin[iSet].Max = new s_Max[Serial_Param -2];
            }
            bool binFound;
            for (int iSSet = 0; iSSet < Serial_Set; iSSet++)
            {
                binFound = false;
                for (int iBin = 0; iBin < Spec_SW_Bin.Length; iBin++)
                {
                    foreach (int ib in Spec_SW_Bin[iBin].Bin)
                    {
                        if (ib == (iSSet + 1))
                        {
                            if (Spec_SW_Bin[iBin].mode.ToUpper().Contains("PASS"))
                            {
                                binFound = true;
                            }
                            break;
                        }
                    }
                    if (binFound)
                    {
                        Spec_SerialBin[iSSet].PassBin = true;
                        break;
                    }
                }
            }

            for (int line = Serial_Start + 2; line < (Serial_Start + Serial_Param); line++)
            {
                tmpStr = Lines[line].Split(',');
                Spec_SerialInfo[Count].TestNumber = tmpStr[0];
                Spec_SerialInfo[Count].TestParameters = tmpStr[1];
                Spec_SerialInfo[Count].ColumnDisplayFlag = CStr2Bool(tmpStr[2]);
                Spec_SerialInfo[Count].ChartDisplayFlag = CStr2Bool(tmpStr[3]);
                Spec_SerialInfo[Count].FailThreshold = Convert.ToDouble(tmpStr[4]);
                for (int iSets = 0; iSets < Serial_Set; iSets++)
                {
                    if (tmpStr[5 + (iSets * 2)].ToUpper() == "NONE")
                    {
                        Spec_SerialBin[iSets].Min[Count].Min_None = true;
                    }
                    else if (tmpStr[5 + (iSets * 2)].ToUpper() == "OUTLIER")
                    {
                        Spec_SerialBin[iSets].Min[Count].Outlier = true;
                    }
                    else if (tmpStr[5 + (iSets * 2)].ToUpper() == "PAT")
                    {
                        Spec_SerialBin[iSets].Min[Count].Pat = true;
                    }
                    else if (tmpStr[5 + (iSets * 2)].ToUpper() == "PSP")
                    {
                        Spec_SerialBin[iSets].Min[Count].PSP = true;
                        Spec_SerialBin[iSets].Min[Count].Min = 0;
                    }
                    else
                    {
                        Spec_SerialBin[iSets].Min[Count].Min = Convert.ToDouble(tmpStr[5 + (iSets * 2)]);
                    }

                    if (tmpStr[6 + (iSets * 2)].ToUpper() == "NONE")
                    {
                        Spec_SerialBin[iSets].Max[Count].Max_None = true;
                    }
                    else if (tmpStr[6 + (iSets * 2)].ToUpper() == "OUTLIER")
                    {
                        Spec_SerialBin[iSets].Max[Count].Outlier = true;
                    }
                    else if (tmpStr[6 + (iSets * 2)].ToUpper() == "PAT")
                    {
                        Spec_SerialBin[iSets].Max[Count].Pat = true;
                    }
                    else if (tmpStr[6 + (iSets * 2)].ToUpper() == "PSP")
                    {
                        //Spec_SerialBin[iSets].Max[Count].PSP = true;
                    }
                    else
                    {
                        Spec_SerialBin[iSets].Max[Count].Max = Convert.ToDouble(tmpStr[6 + (iSets * 2)]);
                    }

                    if ((tmpStr[5 + (iSets * 2)].ToUpper() == "PAT") && (tmpStr[6 + (iSets * 2)].ToUpper() == "PAT"))
                    {
                        if (!Spec_SerialInfo[Count].Pat_Param)
                        {
                            Spec_SerialInfo[Count].Pat_Param = true;
                            Spec_PAT_Item++;
                        }
                    }

                    if ((tmpStr[5 + (iSets * 2)].ToUpper() == "PSP") || (tmpStr[6 + (iSets * 2)].ToUpper() == "PSP"))
                    {
                        if (!Spec_SerialInfo[Count].PSP_Param)
                        {
                            Spec_SerialInfo[Count].PSP_Param = true;
                            Spec_PSP_Item++;
                        }
                    }
                    if ((tmpStr[5 + (iSets * 2)].ToUpper() == "OUTLIER") || (tmpStr[6 + (iSets * 2)].ToUpper() == "OUTLIER"))
                    {
                        //if (Spec_SerialInfo[0].Outlier_Param >= 1)
                        //{
                        //    Array.Resize(ref Spec_Outlier_Item, (Spec_SerialInfo[0].Outlier_Param + 1));
                        //}
                        //Spec_Outlier_Item[Spec_SerialInfo[0].Outlier_Param] = Count;
                        if (!Spec_SerialInfo[Count].Outlier_Param)
                        {
                            Spec_SerialInfo[Count].Outlier_Param = true;
                            Spec_Outlier_Item++;
                        }
                        //Spec_SerialInfo[0].Outlier_Item = Count;
                    }
                }
                Count++;
            }
        }
        public string parse_Str(string inputStr)
        {
            string[] tmpStr;
            tmpStr = inputStr.Split(',');
            return (tmpStr[1]);
        }
        public string parse_Str(string inputStr, int Position)
        {
            string[] tmpStr;
            tmpStr = inputStr.Split(',');
            return (tmpStr[Position]);
        }
        public s_SpecFile parse_Spec
        {
            get
            {
                s_SpecFile tmp = new s_SpecFile();
                tmp.Header = Spec_Header;
                tmp.ControlParameters = Spec_ControlParameters;
                tmp.HW_Bin = Spec_HW_Bin;
                tmp.SW_Bin = Spec_SW_Bin;
                tmp.SerialInfo = Spec_SerialInfo;
                tmp.SerialBin = Spec_SerialBin;
                tmp.SBL_Bin = Spec_SBL_bin;
                tmp.AMap_Bin = Spec_AMap_bin;
                tmp.Outlier_Item = Spec_Outlier_Item;
                tmp.PAT_Item = Spec_PAT_Item;
                tmp.SBL_Enable = Spec_SBL_Enable;
                return (tmp);
            }
        }
        public string cStr2State(string inputStr)
        {
            if (inputStr.ToUpper().Contains("PASS"))
            {
                return ("PASS");
            }
            else
            {
                return ("FAIL");
            }
        }
        public bool CStr2Bool(string Input)
        {
            if (Input.Trim() == "1" || Input.ToUpper().Trim() == "YES" || Input.ToUpper().Trim() == "ON" || Input.ToUpper().Trim() == "V")
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
    }
}
