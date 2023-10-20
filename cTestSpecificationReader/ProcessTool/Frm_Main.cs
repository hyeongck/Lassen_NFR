using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using cLibrary;
using cTestClasses;


namespace ProcessTool
{
   
    public partial class frm_Main : Form
    {
        //Graphics dc;
        bool ShowMap = false;

        //cAlgorithm Drivers = new cAlgorithm();

        Stopwatch watch = new Stopwatch();
        cTestSpecificationReader.cTestSpec Spec = new cTestSpecificationReader.cTestSpec();
        cTestResultsReader.cTestResultsReader Result = new cTestResultsReader.cTestResultsReader();
        cTestResultsReader.cTestResultsReader Previous_Result = new cTestResultsReader.cTestResultsReader();
        cTestResultsReader.cTestResultsReader Outlier_Result = new cTestResultsReader.cTestResultsReader();

        cTestSpecificationReader.s_SpecFile Test_Spec = new cTestSpecificationReader.s_SpecFile();
        cTestSpecificationReader.s_SpecFile New_Test_Spec = new cTestSpecificationReader.s_SpecFile();
        cTestSpecificationReader.s_SpecFile Outlier_Test_Spec = new cTestSpecificationReader.s_SpecFile();

        cTestResultsReader.s_Results ResultFile = new cTestResultsReader.s_Results();
        cTestResultsReader.s_Results Previous_ResultFile = new cTestResultsReader.s_Results();
        cTestResultsReader.s_Results Outlier_ResultFile = new cTestResultsReader.s_Results();
        cTestResultsReader.s_Results Regen_ResultFile = new cTestResultsReader.s_Results();
        cTestResultsReader.s_ResultsData[] New_ResultData;
        cTestResultsReader.s_Results R3;
        cTestResultsReader.s_Results R4;
        cTestResultsReader.s_Results R5;

        cTestSummary.cTestSummary Summary = new cTestSummary.cTestSummary();

        cMapReader.cWaferMap Map = new cMapReader.cWaferMap();
        cMapReader.cRascoMap RascoMap = new cMapReader.cRascoMap();

        public frm_Main()
        {
            //this.Size = new System.Drawing.Size(600, 300);

            InitializeComponent();
        }
        private void frm_Main_Paint(object sender, PaintEventArgs e)
        {
            Graphics dc = e.Graphics;
            if(ShowMap) Draw_Map(dc);
        }

        private void btnRegen_Click(object sender, EventArgs e)
        {
            StatusStripLabel.Text = ""; 
            btnRegen.Enabled = false;
            if (New_Test_Spec.SerialBin == null)
            {
                
                MessageBox.Show("Missing New Specification / Limit File Data");
                if (ResultFile.RawData == null)
                {
                    txtResult.BackColor = Color.Pink;
                    btnResult.BackColor = Color.Pink;
                }
                txtNewSpec.BackColor = Color.Pink;
                btnNewSpec.BackColor = Color.Pink;
                btnNewSpec.Focus();
                btnRegen.Enabled = true;
                return;
            }
            if (ResultFile.RawData == null)
            {
                MessageBox.Show("Missing Results File Data");
                txtResult.BackColor = Color.Pink;
                btnResult.BackColor = Color.Pink;
                btnResult.Focus();
                btnRegen.Enabled = true; 
                return;
            }


            if (txtOutput.Text == "")
            {
                SaveFileDialog Dialog = new SaveFileDialog();
                Dialog.Filter = "Result File (*.csv)|*.csv";
                Dialog.InitialDirectory = FolderName(txtResult.Text);
                Dialog.AddExtension = true;
                Dialog.FileName = Append_Filename(SafeFileName(txtResult.Text), "Regen");
                if (Dialog.ShowDialog() == DialogResult.OK)
                {
                    lblChkOutput.Visible = false;
                    txtOutput.Text = Dialog.FileName;
                }
            }
            if (txtOutput.Text != "")
            {
                Stopwatch ww = new Stopwatch();
                ww.Start();
                #region "Removed"
                /*
                int Serial_Count = New_Test_Spec.SerialBin.Length;
                int Bin_Count = New_Test_Spec.HW_Bin.Length;

                int[] Bin_Info = new int[Bin_Count];
                int Paramater_Count = New_Test_Spec.SerialBin[0].Max.Length;
                string[] Output = new string[ResultFile.ResultData.Length];
                string NewFile_Path = txtOutput.Text;

                bool[,] New_Serial_Result = new bool[New_Test_Spec.SerialBin.Length, Paramater_Count];
                //bool SW_Bin_Found;
                bool HW_Bin_Found;
                //bool SW_And;
                //bool Serial_Found;
                int TotalTest = 0;
                List<string> FailList = new List<string>();
                int[] FailParamItems = new int[New_Test_Spec.SerialInfo.Length];
                int[,] PassHWBin = new int[Convert.ToInt32(ResultFile.TestHeader.SiteDetails.Testing_sites), New_Test_Spec.HW_Bin.Length];
                string[] KeyParam = new string[New_Test_Spec.SerialInfo.Length];

                int tmp_DataLength = ResultFile.ResultData[1].Data.Length;

                New_ResultData = new cTestResultsReader.s_ResultsData[ResultFile.ResultData.Length];

                System.IO.File.WriteAllText(NewFile_Path, New_Result_Header(ResultFile));
                System.IO.File.AppendAllText(NewFile_Path, "\r\n");
                System.IO.File.AppendAllText(NewFile_Path, New_Result_Data_Headder(ResultFile, null, false, "", 0));

                //string FileSerial = @"e:\serial_chk.txt";
                //string FileBin = @"e:\bin_chk.txt";
                //string tmpstr;

                for (int iParam = 0; iParam < New_Test_Spec.SerialInfo.Length; iParam++)
                {
                    KeyParam[iParam] = (iParam + 1).ToString() + "," + New_Test_Spec.SerialInfo[iParam].TestParameters;
                }

                for (int DataPoint = 0; DataPoint < ResultFile.ResultData.Length; DataPoint++)
                {
                    //tmpstr = "\r\n" + (DataPoint + 1).ToString() + ",";
                    //for (int iArr = 0; iArr < New_Serial_Result.Length; iArr++)
                    //{
                    //    New_Serial_Result[iArr] = true;
                    //}
                    //tmpstr = "";
                    TotalTest++;
                    FailList.Clear();
                    #region "obsolete2"
                    //New_Serial_Result = null;
                    //New_Serial_Result = new bool[New_Test_Spec.SerialBin.Length, Paramater_Count];
                    //for (int iSerial = 0; iSerial < New_Test_Spec.SerialBin.Length; iSerial++)
                    //{
                    //    tmpstr += iSerial.ToString() + ", ";
                    //    for (int iParam = 0; iParam < Paramater_Count; iParam++)
                    //    {
                    //        if (!New_Test_Spec.SerialBin[iSerial].Min[iParam].Min_None)
                    //        {
                    //            if (!New_Test_Spec.SerialBin[iSerial].PassBin)
                    //            {
                    //                if (ResultFile.ResultData[DataPoint].Data[iParam] < New_Test_Spec.SerialBin[iSerial].Min[iParam].Min)
                    //                {
                    //                    New_Serial_Result[iSerial, iParam] = true;
                    //                    break;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (ResultFile.ResultData[DataPoint].Data[iParam] <= New_Test_Spec.SerialBin[iSerial].Min[iParam].Min)
                    //                {
                    //                    New_Serial_Result[iSerial, iParam] = true;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //        if (!New_Test_Spec.SerialBin[iSerial].Max[iParam].Max_None)
                    //        {
                    //            if (!New_Test_Spec.SerialBin[iSerial].PassBin)
                    //            {
                    //                if (ResultFile.ResultData[DataPoint].Data[iParam] > New_Test_Spec.SerialBin[iSerial].Max[iParam].Max)
                    //                {
                    //                    New_Serial_Result[iSerial, iParam] = true;
                    //                    break;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (ResultFile.ResultData[DataPoint].Data[iParam] >= New_Test_Spec.SerialBin[iSerial].Max[iParam].Max)
                    //                {
                    //                    New_Serial_Result[iSerial, iParam] = true;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //        tmpstr += New_Serial_Result[iSerial, iParam].ToString() + ", ";
                    //    }
                    //    tmpstr += "\r\n";
                    //}
                    //System.IO.File.AppendAllText("C:\\Table.txt", tmpstr + "\r\n");

                    //Serial_Found = false;

                    //for (int iSW = 0; iSW < New_Test_Spec.SW_Bin.Length; iSW++)
                    //{

                    //    for (int iParam = 0; iParam < Paramater_Count; iParam++)
                    //    {
                    //        for (int iS_Bin = 0; iS_Bin < New_Test_Spec.SW_Bin[iSW].Bin.Length; iS_Bin++)
                    //        {
                    //            if (iS_Bin == 0)
                    //            {
                    //                Serial_Found = !New_Serial_Result[(New_Test_Spec.SW_Bin[iSW].Bin[iS_Bin] - 1), iParam];
                    //            }
                    //            else
                    //            {
                    //                if (New_Test_Spec.SW_Bin[iSW].BinType == cTestSpecificationReader.e_BinType.AND)
                    //                {
                    //                    Serial_Found = Serial_Found && !New_Serial_Result[(New_Test_Spec.SW_Bin[iSW].Bin[iS_Bin] - 1), iParam];
                    //                }
                    //                else
                    //                {
                    //                    Serial_Found = Serial_Found || !New_Serial_Result[(New_Test_Spec.SW_Bin[iSW].Bin[iS_Bin] - 1), iParam];
                    //                }
                    //            }
                    //        }
                    //        if (!Serial_Found)
                    //        {
                    //            break;
                    //        }
                    //    }
                    //    if (Serial_Found)
                    //    {
                    //        New_ResultData[DataPoint].SBin = New_Test_Spec.SW_Bin[iSW].Bin_Number;
                    //        New_ResultData[DataPoint].SWBinName = New_Test_Spec.SW_Bin[iSW].mode;
                    //        break;
                    //    }
                    //}
                    #endregion

                    #region "Obselete"
                    //#region "Sorting with New Spec"
                    //for (int iSerial = 0; iSerial < Serial_Count; iSerial++)
                    //{
                    //    if (New_Serial_Result[iSerial])
                    //    {
                    //        for (int iParam = 0; iParam < Paramater_Count; iParam++)
                    //        {
                    //            if (!New_Test_Spec.SerialBin[iSerial].Min[iParam].Min_None)
                    //            {
                    //                if (ResultFile.ResultData[DataPoint].Data[iParam] < New_Test_Spec.SerialBin[iSerial].Min[iParam].Min)
                    //                {
                    //                    New_Serial_Result[iSerial] = false;
                    //                    break;
                    //                }
                    //            }
                    //            if (!New_Test_Spec.SerialBin[iSerial].Max[iParam].Max_None)
                    //            {
                    //                if (ResultFile.ResultData[DataPoint].Data[iParam] > New_Test_Spec.SerialBin[iSerial].Max[iParam].Max)
                    //                {
                    //                    New_Serial_Result[iSerial] = false;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //    }
                    //    if (New_Serial_Result[iSerial]) tmpstr += (iSerial + 1).ToString() + "-" + New_Serial_Result[iSerial].ToString() + ",";
                    //}
                    //#endregion
                    ////System.IO.File.AppendAllText(FileSerial, tmpstr);

                    //tmpstr = "\r\n" + (DataPoint + 1).ToString() + ",";
                    //#region "SW Binning"
                    //SW_Bin_Found = false;
                    //for (int iSW = 0; iSW < New_Test_Spec.SW_Bin.Length; iSW++)
                    //{
                    //    SW_And = true;
                    //    if (New_Test_Spec.SW_Bin[iSW].BinType == cTestSpecificationReader.e_BinType.AND)
                    //    {
                    //        foreach (int iSubSerial in New_Test_Spec.SW_Bin[iSW].Bin)
                    //        {

                    //            SW_And = SW_And && New_Serial_Result[iSubSerial - 1];
                    //        }

                    //        if (SW_And)
                    //        {
                    //            New_ResultData[DataPoint].SBin = New_Test_Spec.SW_Bin[iSW].Bin_Number;
                    //            New_ResultData[DataPoint].SWBinName = New_Test_Spec.SW_Bin[iSW].mode;
                    //            SW_Bin_Found = true;
                    //            break;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        foreach (int iSubSerial in New_Test_Spec.SW_Bin[iSW].Bin)
                    //        {
                    //            if (New_Serial_Result[iSubSerial - 1])
                    //            {
                    //                New_ResultData[DataPoint].SBin = New_Test_Spec.SW_Bin[iSW].Bin_Number;
                    //                New_ResultData[DataPoint].SWBinName = New_Test_Spec.SW_Bin[iSW].mode;
                    //                SW_Bin_Found = true;
                    //                break;
                    //            }
                    //        }
                    //    }

                    //    if (SW_Bin_Found)
                    //    {
                    //        tmpstr += (iSW + 1).ToString();
                    //        break;
                    //    }

                    //}
                    //#endregion
                    ////System.IO.File.AppendAllText(FileBin, tmpstr);
                    #endregion
                    #region "New"
                    bool b_SWBinFlag = true;
                    bool b_ValidBinFlag;
                    for (int iSW = 0; iSW < New_Test_Spec.SW_Bin.Length; iSW++)
                    {
                        b_ValidBinFlag = true;
                        for (int iParam = 0; iParam < Paramater_Count; iParam++)
                        {
                            b_SWBinFlag = CheckParamResults(DataPoint, New_Test_Spec.SW_Bin[iSW].Bin[0] - 1, iParam);
                            for (int iS_Bin = 1; iS_Bin < New_Test_Spec.SW_Bin[iSW].Bin.Length; iS_Bin++)
                            {
                                if (New_Test_Spec.SW_Bin[iSW].BinType == cTestSpecificationReader.e_BinType.AND)
                                {
                                    b_SWBinFlag = b_SWBinFlag && CheckParamResults(DataPoint, New_Test_Spec.SW_Bin[iSW].Bin[iS_Bin] - 1, iParam);
                                }
                                else
                                {
                                    b_SWBinFlag = b_SWBinFlag || CheckParamResults(DataPoint, New_Test_Spec.SW_Bin[iSW].Bin[iS_Bin] - 1, iParam);
                                }
                            }
                            if (New_Test_Spec.SW_Bin[iSW].PassBin)
                            {
                                if (!b_SWBinFlag)
                                {
                                    b_ValidBinFlag = false;
                                    FailList.Add(New_Test_Spec.SerialInfo[iParam].TestParameters);
                                }
                            }
                            else
                            {
                                if (!b_SWBinFlag)
                                {
                                    b_ValidBinFlag = false;
                                    continue;
                                }
                            }
                        }
                        if (b_ValidBinFlag)
                        {
                            if (New_Test_Spec.SW_Bin[iSW].PassBin)
                            {
                                New_ResultData[DataPoint].PassFail = 1;
                            }
                            else
                            {
                                New_ResultData[DataPoint].PassFail = 0;
                            }
                            New_ResultData[DataPoint].SBin = New_Test_Spec.SW_Bin[iSW].Bin_Number;
                            New_ResultData[DataPoint].SWBinName = New_Test_Spec.SW_Bin[iSW].mode;
                            break;
                        }
                    }
                    #endregion
                    #region "HW Binning"
                    HW_Bin_Found = false;
                    for (int iHW = 0; iHW < New_Test_Spec.HW_Bin.Length; iHW++)
                    {
                        foreach (int iHWBin in New_Test_Spec.HW_Bin[iHW].Bin)
                        {
                            //if (New_Test_Spec.HW_Bin[iHW].Bin_Number == New_ResultData[DataPoint].SBin)
                            if (iHWBin == New_ResultData[DataPoint].SBin)
                            {
                                //Bin_Info[New_Test_Spec.HW_Bin[iHW].Bin_Number - 1]++;
                                New_ResultData[DataPoint].HBin = New_Test_Spec.HW_Bin[iHW].Bin_Number;
                                PassHWBin[(Convert.ToInt32(ResultFile.ResultData[DataPoint].Site) - 1), iHW]++;
                                HW_Bin_Found = true;
                                break;
                            }
                        }
                    }
                    #endregion


                    Output[DataPoint] = ResultFile.ResultData[DataPoint].ID
                                        + "," + New_ResultData[DataPoint].SBin.ToString()
                                        + "," + New_ResultData[DataPoint].HBin.ToString()
                                        + "," + ResultFile.ResultData[DataPoint].Die_X.ToString()
                                        + "," + ResultFile.ResultData[DataPoint].Die_Y.ToString()
                                        + "," + ResultFile.ResultData[DataPoint].Site
                                        + "," + ResultFile.ResultData[DataPoint].Time.ToString()
                                        + "," + ResultFile.ResultData[DataPoint].TotalTest.ToString()
                                        + "," + ResultFile.ResultData[DataPoint].Lot_ID
                                        + "," + ResultFile.ResultData[DataPoint].Wafer_ID;


                    for (int iData = 0; iData < tmp_DataLength; iData++)
                    {
                        Output[DataPoint] += "," + ResultFile.ResultData[DataPoint].Data[iData].ToString();
                    }
                    Output[DataPoint] += "," + ResultFile.ResultData[DataPoint].PassFail.ToString()
                                        + "," + ResultFile.ResultData[DataPoint].TimeStamp
                                        + "," + ResultFile.ResultData[DataPoint].IndexTime.ToString()
                                        + "," + ResultFile.ResultData[DataPoint].PartSN
                                        + "," + New_ResultData[DataPoint].SWBinName;
                    System.IO.File.AppendAllText(NewFile_Path, Output[DataPoint] + "\r\n");

                    for (int iParam = 0; iParam < New_Test_Spec.SerialInfo.Length; iParam++)
                    {
                        if (FailList.Contains(New_Test_Spec.SerialInfo[iParam].TestParameters))
                        {
                            FailParamItems[iParam]++;
                        }
                    }

                    
                }
                Generate_Summary(Append_Filename(NewFile_Path, "Summary", "txt"), TotalTest, FailParamItems, PassHWBin);
                 */
                #endregion
                Regen_ResultsFileNSummary(New_Test_Spec, ResultFile, txtOutput.Text, false);
                ww.Stop();
                MessageBox.Show(ww.ElapsedMilliseconds.ToString());
                lblChkOutput.Visible = true;
                StatusStripLabel.Text = "Regen File Completed!!";
            }
            
            btnRegen.Enabled = true;
            //System.IO.File.WriteAllLines(@"E:\Work Folder\Strip Handler\Result Test\New_Rslt.csv", Output);
        }
            public void Regen_ResultsFileNSummary(cTestSpecificationReader.s_SpecFile Spec, cTestResultsReader.s_Results ResultInfo, string RegenFilePath, bool b_Regen)
            {
                int Serial_Count = Spec.SerialBin.Length;
                int Bin_Count = Spec.HW_Bin.Length;
                int TotalItems = ResultInfo.ResultData.Length;

                int[] Bin_Info = new int[Bin_Count];
                int Paramater_Count = Spec.SerialBin[0].Max.Length;
                string[] Output = new string[ResultInfo.ResultData.Length];
                string NewFile_Path = RegenFilePath;

                bool[,] New_Serial_Result = new bool[Spec.SerialBin.Length, Paramater_Count];
                bool HW_Bin_Found;

                int TotalTest = 0;
                List<string> FailList = new List<string>();
                int[] FailParamItems = new int[Spec.SerialInfo.Length];
                int[,] PassHWBin = new int[Convert.ToInt32(ResultInfo.TestHeader.SiteDetails.Testing_sites), Spec.HW_Bin.Length];
                //string[] KeyParam = new string[Spec.SerialInfo.Length];

                int tmp_DataLength = ResultInfo.ResultData[1].Data.Length;

                New_ResultData = new cTestResultsReader.s_ResultsData[ResultInfo.ResultData.Length];

                if (!b_Regen)
                {
                    System.IO.File.WriteAllText(NewFile_Path, New_Result_Header(ResultInfo));
                    System.IO.File.AppendAllText(NewFile_Path, "\r\n");
                    System.IO.File.AppendAllText(NewFile_Path, New_Result_Data_Header(ResultInfo, null, false, "", 0));
                }
                else
                {

                }
                //for (int iParam = 0; iParam < Spec.SerialInfo.Length; iParam++)
                //{
                //    KeyParam[iParam] = (iParam + 1).ToString() + "," + Spec.SerialInfo[iParam].TestParameters;
                //}
                ProgressBar.Value = 0;
                ProgressBar.Visible = true;
                
                for (int DataPoint = 0; DataPoint < ResultInfo.ResultData.Length; DataPoint++)
                {

                    TotalTest++;
                    FailList.Clear();
                    
                    #region "New"
                    bool b_SWBinFlag = true;
                    bool b_ValidBinFlag;
                    for (int iSW = 0; iSW < Spec.SW_Bin.Length; iSW++)
                    {
                        b_ValidBinFlag = true;
                        for (int iParam = 0; iParam < Paramater_Count; iParam++)
                        {
                            b_SWBinFlag = CheckParamResults(Spec.SerialBin[Spec.SW_Bin[iSW].Bin[0] - 1], ResultInfo.ResultData[DataPoint], iParam);
                            for (int iS_Bin = 1; iS_Bin < Spec.SW_Bin[iSW].Bin.Length; iS_Bin++)
                            {
                                if (Spec.SW_Bin[iSW].BinType == cTestSpecificationReader.e_BinType.AND)
                                {
                                    b_SWBinFlag = b_SWBinFlag && CheckParamResults(Spec.SerialBin[Spec.SW_Bin[iSW].Bin[iS_Bin] - 1], ResultInfo.ResultData[DataPoint], iParam); //CheckParamResults(DataPoint, Spec.SW_Bin[iSW].Bin[iS_Bin] - 1, iParam);
                                }
                                else
                                {
                                    b_SWBinFlag = b_SWBinFlag || CheckParamResults(Spec.SerialBin[Spec.SW_Bin[iSW].Bin[iS_Bin] - 1], ResultInfo.ResultData[DataPoint], iParam); 
                                }
                            }
                            if (Spec.SW_Bin[iSW].PassBin)
                            {
                                if (!b_SWBinFlag)
                                {
                                    b_ValidBinFlag = false;
                                    FailList.Add(Spec.SerialInfo[iParam].TestParameters);
                                }
                            }
                            else
                            {
                                if (!b_SWBinFlag)
                                {
                                    b_ValidBinFlag = false;
                                    continue;
                                }
                            }
                        }
                        if (b_ValidBinFlag)
                        {
                            if (Spec.SW_Bin[iSW].PassBin)
                            {
                                New_ResultData[DataPoint].PassFail = 1;
                            }
                            else
                            {
                                New_ResultData[DataPoint].PassFail = 0;
                            }
                            New_ResultData[DataPoint].SBin = Spec.SW_Bin[iSW].Bin_Number;
                            New_ResultData[DataPoint].SWBinName = Spec.SW_Bin[iSW].mode;
                            break;
                        }
                    }
                    #endregion
                    #region "HW Binning"
                    HW_Bin_Found = false;
                    for (int iHW = 0; iHW < Spec.HW_Bin.Length; iHW++)
                    {
                        foreach (int iHWBin in Spec.HW_Bin[iHW].Bin)
                        {
                            //if (Spec.HW_Bin[iHW].Bin_Number == New_ResultData[DataPoint].SBin)
                            if (iHWBin == New_ResultData[DataPoint].SBin)
                            {
                                //Bin_Info[Spec.HW_Bin[iHW].Bin_Number - 1]++;
                                New_ResultData[DataPoint].HBin = Spec.HW_Bin[iHW].Bin_Number;
                                New_ResultData[DataPoint].HWBinName = Spec.HW_Bin[iHW].Name;
                                PassHWBin[(Convert.ToInt32(ResultInfo.ResultData[DataPoint].Site) - 1), iHW]++;
                                HW_Bin_Found = true;
                                break;
                            }
                        }
                    }
                    #endregion


                    Output[DataPoint] = ResultInfo.ResultData[DataPoint].ID
                                        + "," + New_ResultData[DataPoint].SBin.ToString()
                                        + "," + New_ResultData[DataPoint].HBin.ToString()
                                        + "," + ResultInfo.ResultData[DataPoint].Die_X.ToString()
                                        + "," + ResultInfo.ResultData[DataPoint].Die_Y.ToString()
                                        + "," + ResultInfo.ResultData[DataPoint].Site
                                        + "," + ResultInfo.ResultData[DataPoint].Time.ToString()
                                        + "," + ResultInfo.ResultData[DataPoint].TotalTest.ToString()
                                        + "," + ResultInfo.ResultData[DataPoint].Lot_ID
                                        + "," + ResultInfo.ResultData[DataPoint].Wafer_ID;


                    for (int iData = 0; iData < tmp_DataLength; iData++)
                    {
                        Output[DataPoint] += "," + ResultInfo.ResultData[DataPoint].Data[iData].ToString();
                    }
                    Output[DataPoint] += "," + ResultInfo.ResultData[DataPoint].PassFail.ToString()
                                        + "," + ResultInfo.ResultData[DataPoint].TimeStamp
                                        + "," + ResultInfo.ResultData[DataPoint].IndexTime.ToString()
                                        + "," + ResultInfo.ResultData[DataPoint].PartSN
                                        + "," + New_ResultData[DataPoint].SWBinName
                                        + "," + New_ResultData[DataPoint].HWBinName;
                    System.IO.File.AppendAllText(NewFile_Path, Output[DataPoint] + "\r\n");

                    for (int iParam = 0; iParam < Spec.SerialInfo.Length; iParam++)
                    {
                        if (FailList.Contains(Spec.SerialInfo[iParam].TestParameters))
                        {
                            FailParamItems[iParam]++;
                        }
                    }
                    Application.DoEvents();
                    ProgressBar.Value = (int)((float)(DataPoint + 1) / (float)TotalItems * 100f);
                    StatusStripLabel.Text = "Processing item " + (DataPoint + 1).ToString() + " of " + TotalItems.ToString() + " - " + ((float)(DataPoint + 1) / (float)TotalItems).ToString("0.00%");
                }
                Generate_Summary(ResultInfo, Append_Filename(NewFile_Path, "Summary", "txt"), TotalTest, FailParamItems, PassHWBin);
            }

        public bool CheckParamResults(cTestSpecificationReader.s_SerialBin SerialBin, cTestResultsReader.s_ResultsData Rslt, int iParam)
        {
            if ((SerialBin.Min[iParam].Min_None) && (SerialBin.Max[iParam].Max_None))
            {
                return (true);
            }
            else if ((SerialBin.Min[iParam].Min_None) && !(SerialBin.Max[iParam].Max_None))
            {
                return (Rslt.Data[iParam] <= SerialBin.Max[iParam].Max);
            }
            else if (!(SerialBin.Min[iParam].Min_None) && (SerialBin.Max[iParam].Max_None))
            {
                return (Rslt.Data[iParam] > SerialBin.Min[iParam].Min);
            }
            else
            {
                return ((Rslt.Data[iParam] <= SerialBin.Max[iParam].Max) && (Rslt.Data[iParam] > SerialBin.Min[iParam].Min));
            }
        }

        public string New_Result_Data_Header(cTestResultsReader.s_Results tmpResult, bool[] ChkParameters, bool IncludeOriginal, string addHeader, int MaxHeader)
        {
            string tmpStr = "";
            string additionalHeader = "";
            int iMax = 1;
            int i;
            if (addHeader.ToUpper() == "MERGE")
            {
                iMax = MaxHeader;
            }
            else if (addHeader.ToUpper() == "DELTA_MERGE")
            {
                iMax = MaxHeader;
            }
            else
            {
                if (IncludeOriginal) iMax = 3;
            }

            for (i = 0; i < iMax; i++)
            {
                #region "Correlation Header"
                tmpStr = "#CF," + tmpResult.TestHeader.Correlation_FileName + ",,,,,,,,,";

                for (int iData = 0; iData < tmpResult.ResultData[1].Data.Length; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += tmpResult.ResultHeader.Correlation_Data[iData] + ",";
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += tmpResult.ResultHeader.Correlation_Data[iData] + ",";
                        }
                    }
                }
                tmpStr += ",,,,";
                #endregion
                if (IncludeOriginal) tmpStr += ",";
            }
            
            tmpStr += "\r\n";

            for (i = 0; i < iMax; i++)
            {
                #region "Test Header"
                tmpStr += "Parameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,";

                if ((iMax == 1) && (addHeader !=""))
                {
                    additionalHeader = addHeader + "_";
                }
                else if ((iMax == 3) && (i == 2) && ((addHeader != "") && (addHeader.ToUpper() != "MERGE")))
                {
                    additionalHeader = addHeader + "_";
                }
                else
                {
                    if ((addHeader.ToUpper() == "DELTA") || (addHeader.ToUpper() == "MERGE"))
                    {
                        additionalHeader = "FILE_" + (i + 1).ToString() + "_";
                    }
                    else
                    {
                        additionalHeader = "";
                    }
                }

                for (int iData = 0; iData < tmpResult.ResultData[1].Data.Length; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += additionalHeader +  tmpResult.ResultHeader.TestParameter_Name[iData] + ",";
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += additionalHeader + tmpResult.ResultHeader.TestParameter_Name[iData] + ",";
                        }
                    }
                }
                tmpStr += "PassFail,TimeStamp,IndexTime,PartSN,SWBinName,HWBinName";
                #endregion
                if (IncludeOriginal) tmpStr += ",";
            }

            tmpStr += "\r\n";
            for (i = 0; i < iMax; i++)
            {
                #region "Test Number"
                tmpStr += "Tests#,,,,,,,,,,";

                for (int iData = 0; iData < tmpResult.ResultData[1].Data.Length; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += tmpResult.ResultHeader.TestNumber[iData] + ",";
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += tmpResult.ResultHeader.TestNumber[iData] + ",";
                        }
                    }

                }
                tmpStr += ",,,,";
                #endregion
                if (IncludeOriginal) tmpStr += ",";
            }

            tmpStr += "\r\n";
            for (i = 0; i < iMax; i++)
            {
                #region "Patterns"
                tmpStr += "Patterns,,,,,,,,,,";

                for (int iData = 0; iData < tmpResult.ResultData[1].Data.Length; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += tmpResult.ResultHeader.Patterns[iData] + ",";
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += tmpResult.ResultHeader.Patterns[iData] + ",";
                        }
                    }
                }
                tmpStr += ",,,,";
                #endregion
                if (IncludeOriginal) tmpStr += ",";
            }

            tmpStr += "\r\n";
            for (i = 0; i < iMax; i++)
            {
                #region "Units"
                tmpStr += "Unit,,,,,,Sec,,,,";

                for (int iData = 0; iData < tmpResult.ResultData[1].Data.Length; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += tmpResult.ResultHeader.Units[iData] + ",";
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += tmpResult.ResultHeader.Units[iData] + ",";
                        }
                    }
                }
                tmpStr += ",,Sec,,";
                #endregion
                if (IncludeOriginal) tmpStr += ",";
            }

            tmpStr += "\r\n";
            for (i = 0; i < iMax; i++)
            {
                #region "High Spec"
                tmpStr += "HighL,,,,,,,,,,";

                for (int iData = 0; iData < tmpResult.ResultData[1].Data.Length; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += tmpResult.ResultHeader.HighL[iData] + ",";
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += tmpResult.ResultHeader.HighL[iData] + ",";
                        }
                    }
                }
                tmpStr += ",,,,";
                #endregion
                if (IncludeOriginal) tmpStr += ",";
            }
            
            tmpStr += "\r\n";

            for (i = 0; i < iMax; i++)
            {
                #region "Low Spec"
                tmpStr += "LowL,,,,,,,,,,";

                for (int iData = 0; iData < tmpResult.ResultData[1].Data.Length; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += tmpResult.ResultHeader.LowL[iData] + ",";
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += tmpResult.ResultHeader.LowL[iData] + ",";
                        }
                    }

                }

                tmpStr += ",,,,";
                #endregion
                if (IncludeOriginal) tmpStr += ",";
            }
            
            tmpStr += "\r\n";
            return tmpStr;
        }
        public string New_Result_Header(cTestResultsReader.s_Results tmpResult)
        {

            StringBuilder SB_Header = new StringBuilder();
            SB_Header.AppendFormat("--- Global Info:,\r\n");
            SB_Header.AppendFormat("Date,{0}\r\n", tmpResult.TestHeader.GlobalInfo.Date);
            SB_Header.AppendFormat("SetupTime,{0}\r\n", tmpResult.TestHeader.GlobalInfo.SetupTime);
            SB_Header.AppendFormat("StartTime,{0}\r\n", tmpResult.TestHeader.GlobalInfo.StartTime);
            SB_Header.AppendFormat("FinishTime,{0}\r\n", tmpResult.TestHeader.GlobalInfo.FinishTime);
            SB_Header.AppendFormat("ProgramName,{0}\r\n", tmpResult.TestHeader.GlobalInfo.ProgramName);
            SB_Header.AppendFormat("ProgramRevision,{0}\r\n", tmpResult.TestHeader.GlobalInfo.ProgramRevision);
            SB_Header.AppendFormat("Lot,{0}\r\n", tmpResult.TestHeader.GlobalInfo.Lot);
            SB_Header.AppendFormat("SubLot,{0}\r\n", tmpResult.TestHeader.GlobalInfo.SubLot);
            SB_Header.AppendFormat("Wafer,{0}\r\n", tmpResult.TestHeader.GlobalInfo.Wafer);
            SB_Header.AppendFormat("WaferOrientation,{0}\r\n", tmpResult.TestHeader.GlobalInfo.WaferOrientation);
            SB_Header.AppendFormat("TesterName,{0}\r\n", tmpResult.TestHeader.GlobalInfo.TesterName);
            SB_Header.AppendFormat("TesterType,{0}\r\n", tmpResult.TestHeader.GlobalInfo.TesterType);
            SB_Header.AppendFormat("Product,{0}\r\n", tmpResult.TestHeader.GlobalInfo.Product);
            SB_Header.AppendFormat("Operator,{0}\r\n", tmpResult.TestHeader.GlobalInfo.Operator);
            SB_Header.AppendFormat("ExecType,{0}\r\n", tmpResult.TestHeader.GlobalInfo.ExecType);
            SB_Header.AppendFormat("ExecRevision,{0}\r\n", tmpResult.TestHeader.GlobalInfo.ExecRevision);
            SB_Header.AppendFormat("RtstCode,{0}\r\n", tmpResult.TestHeader.GlobalInfo.RtstCode);
            SB_Header.AppendFormat("PackageType,{0}\r\n", tmpResult.TestHeader.GlobalInfo.PackageType);
            SB_Header.AppendFormat("Family,{0}\r\n", tmpResult.TestHeader.GlobalInfo.Family);
            SB_Header.AppendFormat("SpecName,{0}\r\n", tmpResult.TestHeader.GlobalInfo.SpecName);
            SB_Header.AppendFormat("SpecVersion,{0}\r\n", tmpResult.TestHeader.GlobalInfo.SpecVersion);
            SB_Header.AppendFormat("FlowID,{0}\r\n", tmpResult.TestHeader.GlobalInfo.FlowID);
            SB_Header.AppendFormat("DesignRevision,{0}\r\n", tmpResult.TestHeader.GlobalInfo.DesignRevision);
            SB_Header.AppendFormat("--- Site details:,{0}\r\n", tmpResult.TestHeader.SiteDetails.HeadNumber);
            SB_Header.AppendFormat("Testing sites,{0}\r\n", tmpResult.TestHeader.SiteDetails.Testing_sites);
            SB_Header.AppendFormat("Handler ID,{0}\r\n", tmpResult.TestHeader.SiteDetails.Handler_ID);
            SB_Header.AppendFormat("Handler type,{0}\r\n", tmpResult.TestHeader.SiteDetails.Handler_type);
            SB_Header.AppendFormat("LoadBoardName,{0}\r\n", tmpResult.TestHeader.SiteDetails.LoadBoardName);
            SB_Header.AppendFormat("--- Options:,\r\n");
            SB_Header.AppendFormat("UnitsMode,{0}\r\n", tmpResult.TestHeader.Options.UnitsMode);
            SB_Header.AppendFormat("--- ConditionName:,{0}\r\n", tmpResult.TestHeader.ConditionName.ConditionName);
            SB_Header.AppendFormat("EMAIL_ADDRESS,{0}\r\n", tmpResult.TestHeader.ConditionName.EMAIL_ADDRESS);
            SB_Header.AppendFormat("Translator,{0}\r\n", tmpResult.TestHeader.ConditionName.Translator);
            SB_Header.AppendFormat("Wafer_Diameter,{0}\r\n", tmpResult.TestHeader.ConditionName.Wafer_Diameter);
            SB_Header.AppendFormat("Facility,{0}\r\n", tmpResult.TestHeader.ConditionName.Facility);
            SB_Header.AppendFormat("HostIpAddress,{0}\r\n", tmpResult.TestHeader.ConditionName.HostIpAddress);
            SB_Header.AppendFormat("Temperature,{0}\r\n", tmpResult.TestHeader.ConditionName.Temperature);
            SB_Header.AppendFormat("PcbLot,{0}\r\n", tmpResult.TestHeader.Misc_Details.PcbLot);
            SB_Header.AppendFormat("AssemblyLot,{0}\r\n", tmpResult.TestHeader.Misc_Details.AssemblyLot);
            SB_Header.AppendFormat("VerificationUnit,{0}\r\n", tmpResult.TestHeader.Misc_Details.VerificationUnit);
            SB_Header.AppendFormat("\r\n");

            //string tmpStr = "";

            //for (int iRow = tmpResult.TestHeader.GlobalInfo.RowNo; iRow < tmpResult.TestHeader.Start_Data_Header_Row - 1; iRow++)
            //{
            //    tmpStr += tmpResult.RawData[iRow] + "\r\n";
            //}
            //return (tmpStr);
            return SB_Header.ToString();
        }

        private void btnDelta_Click(object sender, EventArgs e)
        {
            StatusStripLabel.Text = ""; 
            btnDelta.Enabled = false;
            int FileCount = 2;

            cTestResultsReader.s_Results[] RsltDataFile;
            string[] OutputFileName = new string[FileCount - 1];


            if (ResultFile.RawData == null)
            {
                MessageBox.Show("Missing Results File Data");
                if (Previous_ResultFile.RawData == null)
                {
                    txtPrevResult.BackColor = Color.Pink;
                    btnPrevResult.BackColor = Color.Pink;
                }
                txtResult.BackColor = Color.Pink;
                btnResult.BackColor = Color.Pink;
                btnResult.Focus();
                btnDelta.Enabled = true;
                return;
            }
            if (Previous_ResultFile.RawData == null)
            {
                MessageBox.Show("Missing Previous Results File Data");
                txtPrevResult.BackColor = Color.Pink;
                btnPrevResult.BackColor = Color.Pink;
                btnPrevResult.Focus();
                btnDelta.Enabled = true;
                return;
            }


            if (txtOutput.Text == "")
            {
                SaveFileDialog Dialog = new SaveFileDialog();
                Dialog.Filter = "Result File (*.csv)|*.csv";
                Dialog.InitialDirectory = FolderName(txtPrevResult.Text);
                Dialog.AddExtension = true;
                Dialog.FileName = Append_Filename(SafeFileName(txtPrevResult.Text), "Delta");
                if (Dialog.ShowDialog() == DialogResult.OK)
                {
                    lblChkOutput.Visible = false;
                    txtOutput.Text = Dialog.FileName;
                    
                }
            }
            if (txtOutput.Text != "")
            {
                OutputFileName[0] = txtOutput.Text;
                RsltDataFile = new cTestResultsReader.s_Results[FileCount];
                {
                    RsltDataFile[0] = ResultFile;
                    RsltDataFile[1] = Previous_ResultFile;
                    // add more codes
                    if (R3.RawData != null)
                    {
                        FileCount++;
                        Array.Resize(ref RsltDataFile, FileCount);
                        Array.Resize(ref OutputFileName, FileCount - 1);
                        OutputFileName[FileCount - 2] =ExtractFilePath(txtOutput.Text) + Append_Filename(SafeFileName(txtR3.Text), "Delta") + ".csv";
                        RsltDataFile[FileCount - 1] = R3;
                    }
                    if (R4.RawData != null)
                    {
                        FileCount++;
                        Array.Resize(ref RsltDataFile, FileCount);
                        Array.Resize(ref OutputFileName, FileCount - 1);
                        OutputFileName[FileCount - 2] = ExtractFilePath(txtOutput.Text) + Append_Filename(SafeFileName(txtR4.Text), "Delta") + ".csv";
                        RsltDataFile[FileCount - 1] = R4;
                    }
                    if (R5.RawData != null)
                    {
                        FileCount++;
                        Array.Resize(ref RsltDataFile, FileCount);
                        Array.Resize(ref OutputFileName, FileCount - 1);
                        OutputFileName[FileCount - 2] = ExtractFilePath(txtOutput.Text) + Append_Filename(SafeFileName(txtR5.Text), "Delta") + ".csv";
                        RsltDataFile[FileCount - 1] = R5;
                    }
                }

                int iMaxDataLength = 0;
                for (int iR = 0; iR < FileCount; iR++)
                {
                    if (iMaxDataLength < RsltDataFile[iR].ResultData.Length)
                    {
                        iMaxDataLength = RsltDataFile[iR].ResultData.Length;
                    }
                }

                string[] Output = new string[iMaxDataLength];
                string NewFile_Path = txtOutput.Text;
                bool[] ChkParameters;

                ChkParameters = null;
                if (chkSelectHeader.Checked)
                {
                    frmHeaderSelect fhead = new frmHeaderSelect();
                    fhead.parse_TestParameters = RsltDataFile[0].ResultHeader.TestParameter_Name;
                    DialogResult rslt = fhead.ShowDialog();
                    if (rslt == DialogResult.OK)
                    {
                        ChkParameters = fhead.parse_ChkParameters;
                    }
                }

                for (int iDelta = 1; iDelta < FileCount; iDelta++)
                {
                    NewFile_Path = OutputFileName[iDelta - 1];

                    System.IO.File.WriteAllText(NewFile_Path, New_Result_Header(RsltDataFile[iDelta]));
                    System.IO.File.AppendAllText(NewFile_Path, "\r\n");
                    System.IO.File.AppendAllText(NewFile_Path, New_Result_Data_Header(RsltDataFile[iDelta], ChkParameters, chkIncludeOriginal.Checked, "Delta", 0));

                    int tmp_DataLength = RsltDataFile[iDelta].ResultData[1].Data.Length;


                    if (ResultFile.XY_Info.Valid && Previous_ResultFile.XY_Info.Valid)
                    {
                        #region "Min and Max"
                        int minX = RsltDataFile[0].XY_Info.XY_MinMax.Min_X;
                        int minY = RsltDataFile[0].XY_Info.XY_MinMax.Min_Y;
                        int maxX = RsltDataFile[0].XY_Info.XY_MinMax.Max_X;
                        int maxY = RsltDataFile[0].XY_Info.XY_MinMax.Max_Y;
                        int tmpA_I;
                        int tmpB_I;
                        int Dataout = 0;

                        if (RsltDataFile[iDelta].XY_Info.XY_MinMax.Min_X < minX)
                        {
                            minX = RsltDataFile[iDelta].XY_Info.XY_MinMax.Min_X;
                        }
                        if (RsltDataFile[iDelta].XY_Info.XY_MinMax.Min_Y < minY)
                        {
                            minX = RsltDataFile[iDelta].XY_Info.XY_MinMax.Min_Y;
                        }
                        if (RsltDataFile[iDelta].XY_Info.XY_MinMax.Max_X > maxX)
                        {
                            maxX = RsltDataFile[iDelta].XY_Info.XY_MinMax.Max_X;
                        }
                        if (RsltDataFile[iDelta].XY_Info.XY_MinMax.Max_Y > maxY)
                        {
                            maxY = RsltDataFile[iDelta].XY_Info.XY_MinMax.Max_Y;
                        }
                        #endregion
                        for (int iX = minX; iX < maxX + 1; iX++)
                        {
                            for (int iY = minY; iY < maxY + 1; iY++)
                            {
                                Output[Dataout] = "";
                                tmpA_I = RsltDataFile[0].XY_Info.Match_Position[Parse_PosData(iX), Parse_PosData(iY)];
                                tmpB_I = RsltDataFile[iDelta].XY_Info.Match_Position[Parse_PosData(iX), Parse_PosData(iY)];

                                if (chkIncludeOriginal.Checked)
                                {
                                    Output[Dataout] += generate_OriginData(tmpB_I, tmp_DataLength, ChkParameters, RsltDataFile[iDelta]);
                                    Output[Dataout] += generate_OriginData(tmpA_I, tmp_DataLength, ChkParameters, RsltDataFile[0]);
                                }

                                if (tmpA_I != -1)
                                {
                                    if (tmpB_I != -1)
                                    {
                                        Output[Dataout] += RsltDataFile[iDelta].ResultData[tmpB_I].ID
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].SBin.ToString()
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].HBin.ToString()
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Die_X.ToString()
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Die_Y.ToString()
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Site
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Time.ToString()
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].TotalTest.ToString()
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Lot_ID
                                                    + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Wafer_ID;
                                    }
                                    else
                                    {
                                        Output[Dataout] += RsltDataFile[0].ResultData[tmpA_I].ID
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].SBin.ToString()
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].HBin.ToString()
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].Die_X.ToString()
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].Die_Y.ToString()
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].Site
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].Time.ToString()
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].TotalTest.ToString()
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].Lot_ID
                                                    + "," + RsltDataFile[0].ResultData[tmpA_I].Wafer_ID;
                                    }
                                    for (int iData = 0; iData < tmp_DataLength; iData++)
                                    {
                                        if (tmpB_I != -1)
                                        {
                                            if (ChkParameters == null)
                                            {
                                                Output[Dataout] += "," + (RsltDataFile[iDelta].ResultData[tmpB_I].Data[iData] - RsltDataFile[0].ResultData[tmpA_I].Data[iData]).ToString();
                                            }
                                            else
                                            {
                                                if (ChkParameters[iData])
                                                {
                                                    Output[Dataout] += "," + (RsltDataFile[iDelta].ResultData[tmpB_I].Data[iData] - RsltDataFile[0].ResultData[tmpA_I].Data[iData]).ToString();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ChkParameters == null)
                                            {
                                                Output[Dataout] += "," + (-RsltDataFile[0].ResultData[tmpA_I].Data[iData]);
                                            }
                                            else
                                            {
                                                if (ChkParameters[iData])
                                                {
                                                    Output[Dataout] += "," + (-RsltDataFile[0].ResultData[tmpA_I].Data[iData]);
                                                }
                                            }
                                        }

                                    }
                                    if (tmpB_I != -1)
                                    {
                                        Output[Dataout] += "," + RsltDataFile[iDelta].ResultData[tmpB_I].PassFail.ToString()
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].TimeStamp
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].IndexTime.ToString()
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].PartSN
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].SWBinName
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].HWBinName;
                                    }
                                    else
                                    {
                                        Output[Dataout] += "," + RsltDataFile[0].ResultData[tmpA_I].PassFail.ToString()
                                                            + "," + RsltDataFile[0].ResultData[tmpA_I].TimeStamp
                                                            + "," + RsltDataFile[0].ResultData[tmpA_I].IndexTime.ToString()
                                                            + "," + RsltDataFile[0].ResultData[tmpA_I].PartSN
                                                            + "," + RsltDataFile[0].ResultData[tmpA_I].SWBinName
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpA_I].HWBinName;
                                    }
                                    System.IO.File.AppendAllText(NewFile_Path, Output[Dataout] + "\r\n");
                                }
                                else
                                {
                                    if (tmpB_I != -1)
                                    {
                                        Output[Dataout] += RsltDataFile[iDelta].ResultData[tmpB_I].ID
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].SBin.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].HBin.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Die_X.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Die_Y.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Site
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Time.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].TotalTest.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Lot_ID
                                                + "," + RsltDataFile[iDelta].ResultData[tmpB_I].Wafer_ID;

                                        for (int iData = 0; iData < tmp_DataLength; iData++)
                                        {
                                            if (ChkParameters == null)
                                            {
                                                Output[Dataout] += "," + (RsltDataFile[iDelta].ResultData[tmpB_I].Data[iData]).ToString();
                                            }
                                            else
                                            {
                                                if (ChkParameters[iData])
                                                {
                                                    Output[Dataout] += "," + (RsltDataFile[iDelta].ResultData[tmpB_I].Data[iData]).ToString();
                                                }
                                            }

                                        }
                                        Output[Dataout] += "," + RsltDataFile[iDelta].ResultData[tmpB_I].PassFail.ToString()
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].TimeStamp
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].IndexTime.ToString()
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].PartSN
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].SWBinName
                                                            + "," + RsltDataFile[iDelta].ResultData[tmpB_I].HWBinName;
                                        System.IO.File.AppendAllText(NewFile_Path, Output[Dataout] + "\r\n");
                                        lblChkOutput.Visible = true;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        
                        if ((ResultFile.XY_Info.Valid && !Previous_ResultFile.XY_Info.Valid) || (!ResultFile.XY_Info.Valid && Previous_ResultFile.XY_Info.Valid))
                        {
                            MessageBox.Show("Unable to Identify the X-Y position correctly. Results may be not accurate!", "Warning");
                        }
                        int MaxRowLength = RsltDataFile[0].ResultData.Length;
                        
                        if (RsltDataFile[iDelta].ResultData.Length > MaxRowLength)
                        {
                            MaxRowLength = RsltDataFile[iDelta].ResultData.Length;
                        }

                        for (int DataPoint = 0; DataPoint < MaxRowLength; DataPoint++)
                        {
                            Output[DataPoint] = "";
                            if (chkIncludeOriginal.Checked)
                            {
                                if (RsltDataFile[iDelta].ResultData.Length > DataPoint)
                                {
                                    Output[DataPoint] += generate_OriginData(DataPoint, tmp_DataLength, ChkParameters, RsltDataFile[iDelta]);
                                }
                                else
                                {
                                    Output[DataPoint] += generate_OriginData(-1, tmp_DataLength, ChkParameters, RsltDataFile[iDelta]);
                                }
                                if (RsltDataFile[0].ResultData.Length > DataPoint)
                                {
                                    Output[DataPoint] += generate_OriginData(DataPoint, tmp_DataLength, ChkParameters, RsltDataFile[0]);
                                }
                                else
                                {
                                    Output[DataPoint] += generate_OriginData(-1, tmp_DataLength, ChkParameters, RsltDataFile[0]);
                                }
                            }
                            if (RsltDataFile[iDelta].ResultData.Length > DataPoint)
                            {
                                Output[DataPoint] += RsltDataFile[iDelta].ResultData[DataPoint].ID
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].SBin.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].HBin.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].Die_X.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].Die_Y.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].Site
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].Time.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].TotalTest.ToString()
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].Lot_ID
                                                + "," + RsltDataFile[iDelta].ResultData[DataPoint].Wafer_ID;
                            }
                            else
                            {
                                Output[DataPoint] += RsltDataFile[0].ResultData[DataPoint].ID
                                                + "," + RsltDataFile[0].ResultData[DataPoint].SBin.ToString()
                                                + "," + RsltDataFile[0].ResultData[DataPoint].HBin.ToString()
                                                + "," + RsltDataFile[0].ResultData[DataPoint].Die_X.ToString()
                                                + "," + RsltDataFile[0].ResultData[DataPoint].Die_Y.ToString()
                                                + "," + RsltDataFile[0].ResultData[DataPoint].Site
                                                + "," + RsltDataFile[0].ResultData[DataPoint].Time.ToString()
                                                + "," + RsltDataFile[0].ResultData[DataPoint].TotalTest.ToString()
                                                + "," + RsltDataFile[0].ResultData[DataPoint].Lot_ID
                                                + "," + RsltDataFile[0].ResultData[DataPoint].Wafer_ID;
                            }
                            
                            for (int iData = 0; iData < tmp_DataLength; iData++)
                            {

                                if (ChkParameters == null)
                                {
                                    if ((RsltDataFile[iDelta].ResultData.Length > DataPoint) && (RsltDataFile[0].ResultData.Length > DataPoint))
                                    {
                                        Output[DataPoint] += "," + (RsltDataFile[iDelta].ResultData[DataPoint].Data[iData] - RsltDataFile[0].ResultData[DataPoint].Data[iData]).ToString();
                                    }
                                    else if ((RsltDataFile[iDelta].ResultData.Length > DataPoint) && !(RsltDataFile[0].ResultData.Length > DataPoint))
                                    {
                                        Output[DataPoint] += "," + (RsltDataFile[iDelta].ResultData[DataPoint].Data[iData]).ToString();
                                    }
                                    else if (!(RsltDataFile[iDelta].ResultData.Length > DataPoint) && (RsltDataFile[0].ResultData.Length > DataPoint))
                                    {
                                        Output[DataPoint] += "," + ( - RsltDataFile[0].ResultData[DataPoint].Data[iData]).ToString();
                                    }
                                    
                                }
                                else
                                {
                                    if (ChkParameters[iData])
                                    {
                                        //Output[DataPoint] += "," + (ResultFile.ResultData[DataPoint].Data[iData] - Previous_ResultFile.ResultData[DataPoint].Data[iData]).ToString();
                                        if ((RsltDataFile[iDelta].ResultData.Length > DataPoint) && (RsltDataFile[0].ResultData.Length > DataPoint))
                                        {
                                            Output[DataPoint] += "," + (RsltDataFile[iDelta].ResultData[DataPoint].Data[iData] - RsltDataFile[0].ResultData[DataPoint].Data[iData]).ToString();
                                        }
                                        else if ((RsltDataFile[iDelta].ResultData.Length > DataPoint) && !(RsltDataFile[0].ResultData.Length > DataPoint))
                                        {
                                            Output[DataPoint] += "," + (RsltDataFile[iDelta].ResultData[DataPoint].Data[iData]).ToString();
                                        }
                                        else if (!(RsltDataFile[iDelta].ResultData.Length > DataPoint) && (RsltDataFile[0].ResultData.Length > DataPoint))
                                        {
                                            Output[DataPoint] += "," + (-RsltDataFile[0].ResultData[DataPoint].Data[iData]).ToString();
                                        }
                                    }
                                }
                            }
                            if (RsltDataFile[iDelta].ResultData.Length > DataPoint)
                            {
                                Output[DataPoint] += "," + RsltDataFile[iDelta].ResultData[DataPoint].PassFail.ToString()
                                                        + "," + RsltDataFile[iDelta].ResultData[DataPoint].TimeStamp
                                                        + "," + RsltDataFile[iDelta].ResultData[DataPoint].IndexTime.ToString()
                                                        + "," + RsltDataFile[iDelta].ResultData[DataPoint].PartSN
                                                        + "," + RsltDataFile[iDelta].ResultData[DataPoint].SWBinName
                                                        + "," + RsltDataFile[iDelta].ResultData[DataPoint].HWBinName;
                            }
                            else
                            {
                                Output[DataPoint] += "," + RsltDataFile[0].ResultData[DataPoint].PassFail.ToString()
                                                        + "," + RsltDataFile[0].ResultData[DataPoint].TimeStamp
                                                        + "," + RsltDataFile[0].ResultData[DataPoint].IndexTime.ToString()
                                                        + "," + RsltDataFile[0].ResultData[DataPoint].PartSN
                                                        + "," + RsltDataFile[0].ResultData[DataPoint].SWBinName
                                                        + "," + RsltDataFile[0].ResultData[DataPoint].HWBinName;
                            }
                            System.IO.File.AppendAllText(NewFile_Path, Output[DataPoint] + "\r\n");
                            lblChkOutput.Visible = true;

                        }
                    }
                }
                StatusStripLabel.Text = "Delta File Completed!!";
            }  

            btnDelta.Enabled = true;
        }
        private string generate_OriginData(int iArrData, int tmp_DataLength, bool[] ChkParameters, cTestResultsReader.s_Results DataFile)
        {
            string tmpStr = "";

            if (iArrData != -1)
            {
                tmpStr = DataFile.ResultData[iArrData].ID
                            + "," + DataFile.ResultData[iArrData].SBin.ToString()
                            + "," + DataFile.ResultData[iArrData].HBin.ToString()
                            + "," + DataFile.ResultData[iArrData].Die_X.ToString()
                            + "," + DataFile.ResultData[iArrData].Die_Y.ToString()
                            + "," + DataFile.ResultData[iArrData].Site
                            + "," + DataFile.ResultData[iArrData].Time.ToString()
                            + "," + DataFile.ResultData[iArrData].TotalTest.ToString()
                            + "," + DataFile.ResultData[iArrData].Lot_ID
                            + "," + DataFile.ResultData[iArrData].Wafer_ID;

                for (int iData = 0; iData < tmp_DataLength; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += "," + (DataFile.ResultData[iArrData].Data[iData]);
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += "," + (DataFile.ResultData[iArrData].Data[iData]);
                        }
                    }
                }
                tmpStr += "," + DataFile.ResultData[iArrData].PassFail.ToString()
                                    + "," + DataFile.ResultData[iArrData].TimeStamp
                                    + "," + DataFile.ResultData[iArrData].IndexTime.ToString()
                                    + "," + DataFile.ResultData[iArrData].PartSN
                                    + "," + DataFile.ResultData[iArrData].SWBinName
                                    + "," + DataFile.ResultData[iArrData].HWBinName;
                return (tmpStr + ",");
            }
            else
            {
                tmpStr = "," 
                            + "," 
                            + ","
                            + "," 
                            + "," 
                            + "," 
                            + "," 
                            + "," 
                            + ",";

                for (int iData = 0; iData < tmp_DataLength; iData++)
                {
                    if (ChkParameters == null)
                    {
                        tmpStr += ",";
                    }
                    else
                    {
                        if (ChkParameters[iData])
                        {
                            tmpStr += ",";
                        }
                    }
                }
                tmpStr += "," + "," 
                                    + "," 
                                    + ","
                                    + ",";
                return (tmpStr + ",");
            }
        }
        private int Parse_PosData(int iValue)
        {
            if (iValue < 0)
            {
                return (5000 - iValue);
            }
            else
            {
                return iValue;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            /*Spec.Spec_FileName = @"E:\Work Folder\Strip Handler\Result Test\Test1.csv";
            Spec.Read_File();
            Spec.Processing_Spec();
            Spec.Processing_Serial();
            Outlier_Test_Spec = Spec.parse_Spec;

            Outlier_Result.Result_FileName = @"E:\Work Folder\Strip Handler\Result Test\Results_1_20111123_145443_BBNECCM.csv";
            Outlier_Result.Read_File();
            Outlier_Result.ProcessData();
            Outlier_ResultFile = Outlier_Result.parse_Results;

            int[] Outlier_Param = new int[Outlier_Test_Spec.SerialInfo[0].Outlier_Param];
            int Outlier_Count = 0;

            for (int iSet = 0; iSet < Outlier_Test_Spec.SerialBin[Outlier_Test_Spec.SerialInfo[0].Outlier_Item].Max.Length; iSet++)
            {
                if (Outlier_Test_Spec.SerialBin[Outlier_Test_Spec.SerialInfo[0].Outlier_Item].Max[iSet].Outlier)
                {
                    Outlier_Param[Outlier_Count] = iSet;
                    Outlier_Count++;
                }
            }

            int Outlier_Frequency_Range = 25000000;     //25 MHz
            int Outlier_Step_Range = 5000;              // 5 kHz
            int Outlier_Bin_Range;
            int tmp_Outlier_Bin;
            Outlier_Bin_Range = (Outlier_Frequency_Range / Outlier_Step_Range);

            int[] Outlier_Data = new int[(2*Outlier_Bin_Range) + 2];

            for (int iOut_Count = 0; iOut_Count < Outlier_Count; iOut_Count++)
            {
                for (int iData = 0; iData < Outlier_ResultFile.ResultData.Length; iData++)
                {
                    if ((Outlier_ResultFile.ResultData[iData].Data[Outlier_Param[iOut_Count]] < Outlier_Frequency_Range) && (Outlier_ResultFile.ResultData[iData].Data[Outlier_Param[iOut_Count]] > -Outlier_Frequency_Range))
                    {
                        tmp_Outlier_Bin = (int)((Outlier_ResultFile.ResultData[iData].Data[Outlier_Param[iOut_Count]] / Outlier_Step_Range) + Outlier_Bin_Range);
                    }
                    else if (Outlier_ResultFile.ResultData[iData].Data[Outlier_Param[iOut_Count]] > Outlier_Frequency_Range)
                    {
                        Outlier_Data[(2 * Outlier_Bin_Range)+1]++;
                    }
                    else if (Outlier_ResultFile.ResultData[iData].Data[Outlier_Param[iOut_Count]] < -Outlier_Frequency_Range)
                    {
                        Outlier_Data[0]++;
                    }
                }
            }*/
        }
        private void button6_Click(object sender, EventArgs e)
        {



            RascoMap.Map_Filename = "E:\\Work Folder\\Strip Handler\\Shiva\\shiva_2_temp_85C\\00FA00709_2012041315202900_Regen.xml";
            RascoMap.Read_Map();
            ShowMap = true;
            this.Invalidate();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            //Map.Wafer_FileName = @"E:\4DQ1_G467_07A2_AVI_CLONE.A";
            //Map.ReadFile();
            //Map.Output_FileName = @"E:\MapTest.txt";
            //Map.WriteFile();
            //this.Invalidate();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            //byte aaa= 64;
            //int bbb = 0x64;
            RascoMap = null;
            RascoMap = new cMapReader.cRascoMap();
            //dc = this.CreateGraphics();
            ////dc.VisibleClipBounds.Right = 300;
            //Pen BluePen = new Pen(Color.Black, 2);
            //dc.DrawRectangle(BluePen, 10, 0, 100+(RascoMap.MapData.DeviceInfo.Rows * 3), 100+RascoMap.MapData.DeviceInfo.Columns * 3);

            //dc.Dispose();
            this.Invalidate();
        }
        void Draw_Map(Graphics e)
        {
   //         e = this.CreateGraphics();
   //         System.Drawing.Drawing2D.HatchBrush aHatchBrush = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.
   //Drawing2D.HatchStyle.Plaid, Color.Red, Color.Blue);

   //         e.FillRectangle(aHatchBrush, 500, 10, 100, 100);
            if (RascoMap.MapData.Data != null)
            {
                int scale = 5;
                e = this.CreateGraphics();
                Pen DrawingPen = new Pen(Color.Black, 1);
                SolidBrush brush = new SolidBrush(Color.Blue);
                
                for (int py = 0; py < RascoMap.MapData.DeviceInfo.Rows; py++)
                {
                    for (int px = 0; px < RascoMap.MapData.DeviceInfo.Columns; px++)
                    {

                        FillColor(e,RascoMap.MapData.Data[(RascoMap.MapData.DeviceInfo.Rows - 1) - py,px], scale, brush, px, py);
                        e.DrawRectangle(DrawingPen, 50 + (px * scale), 500 + (py * scale), scale, scale); 
                       
                    }
                }
            }
            if (Map.WaferData.Raw_Data != null)
            {
                float scale = 3f;
                e = this.CreateGraphics();
                Pen DrawingPen = new Pen(Color.Black, 0.2f);
                SolidBrush brush = new SolidBrush(Color.Black);
                e.FillRectangle(brush, 10, 10, (Map.WaferData.XY_Info.Size.X * (scale + 1f)), Map.WaferData.XY_Info.Size.Y * scale); 
                for (int py = 0; py < Map.WaferData.XY_Info.Size.Y; py++)
                {
                    for (int px = 0; px < Map.WaferData.XY_Info.Size.X; px++)
                    {
                       
                        FillColor(e, Map.WaferData.Arr_Data[px,py], scale, brush, px, py);
                        //e.DrawRectangle(DrawingPen, 10 + (px * 4f), 10 + (py * scale), 4f, scale);

                    }
                }
            }
            e.Dispose();
        }
        void FillColor(Graphics e, string Info, float iScale, SolidBrush brush, int X, int Y)
        {

            switch (Info)
            {
                case "P":
                    brush.Color = Color.Green;
                    break;
                case "#":
                    brush.Color = Color.Red;
                    break;
                default:
                    brush.Color = Color.White;
                    break;
            }
            e.FillRectangle(brush, 10 + (X * (iScale + 1f)), 10 + (Y * iScale), 2.5f, (iScale - 1f));
        }
        void FillColor(Graphics e, int Info, int iScale, SolidBrush brush, int X, int Y)
        {
            
            switch (Info)
            {
                case 1:
                    brush.Color = Color.DarkRed;
                    break;
                case 3:
                    brush.Color = Color.Aqua;
                    break;
                case 2:
                    brush.Color = Color.Pink;
                    break;
                case 8:
                    brush.Color = Color.Red;
                    break;
                case 29:
                    brush.Color = Color.Blue;
                    break;
                case 12:
                    brush.Color = Color.Green;
                    break;
                default:
                    brush.Color = Color.White;
                    break;
            }
            e.FillRectangle(brush , 50 + (X * iScale), 500 + (Y * iScale), iScale, iScale); 
        }
        
        private void button9_Click(object sender, EventArgs e)
        {
            frmMap fMap = new frmMap();
            fMap.Parse_Result = ResultFile;
            fMap.Show();

        }
        private void button10_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            txtResult.BackColor = SystemColors.Window;
            btnResult.BackColor = SystemColors.Control;
            dialog.Filter = "Result File (*.csv)|*.csv";
            lblChkResult.Visible = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtResult.Text = dialog.FileName;
                if (Result != null)
                {
                    Result = null;
                    Result = new cTestResultsReader.cTestResultsReader();
                }

                Result.Result_FileName = dialog.FileName;
                Result.bExtract_XY = chk_ExtractXYLoc.Checked;
                if (ResultFile.RawData != null)
                {
                    ResultFile = new cTestResultsReader.s_Results();
                }
                if (Result.Read_File())
                {
                    ResultFile = Result.parse_Results;
                    
                    if (ResultFile.RawData != null)
                    {
                       
                        lblChkResult.Text = "P";
                        lblChkResult.ForeColor = Color.DarkGreen;
                        lblChkResult.Visible = true;
                        
                    }
                    else
                    {
                       
                        lblChkResult.Text = "O";
                        lblChkResult.ForeColor = Color.Red;
                        lblChkResult.Visible = true;
                    }
                }
            }
            else
            {
                txtResult.Text = "";
            }
        }
        private void btnXY_Click(object sender, EventArgs e)
        {
            btnXY.Enabled = false;
            
            if (ResultFile.RawData == null)
            {
                MessageBox.Show("Missing Results File Data");
                btnResult.BackColor = Color.Pink;
                txtResult.BackColor = Color.Pink;
                btnResult.Focus();
                btnXY.Enabled = true;
                return;
            }

            cTestResultsReader.s_Results RFile = new cTestResultsReader.s_Results();
            
            if (Result != null)
            {
                if (txtOutput.Text == "")
                {
                    SaveFileDialog Dialog = new SaveFileDialog();
                    Dialog.Filter = "Result File (*.csv)|*.csv";
                    Dialog.InitialDirectory = FolderName(txtResult.Text);
                    Dialog.AddExtension = true;
                    Dialog.FileName = Append_Filename(SafeFileName(txtResult.Text), "XY");
                    if (Dialog.ShowDialog() == DialogResult.OK)
                    {
                        lblChkOutput.Visible = false;
                        txtOutput.Text = Dialog.FileName;
                    }
                }
                if (txtOutput.Text != "")
                {
                    RFile = Result.parse_Results;

                    string[] Output = new string[RFile.ResultData.Length];
                    string NewFile_Path = txtOutput.Text;

                    System.IO.File.WriteAllText(NewFile_Path, New_Result_Header(RFile));
                    System.IO.File.AppendAllText(NewFile_Path, "\r\n");
                    System.IO.File.AppendAllText(NewFile_Path, New_Result_Data_Header(RFile, null, false, "", 0));

                    int tmp_DataLength = RFile.ResultData[1].Data.Length;

                    for (int DataPoint = 0; DataPoint < RFile.ResultData.Length; DataPoint++)
                    {

                        Output[DataPoint] = RFile.ResultData[DataPoint].ID
                                            + "," + RFile.ResultData[DataPoint].SBin.ToString()
                                            + "," + RFile.ResultData[DataPoint].HBin.ToString()
                                            + "," + Extract_ID_XY(RFile.ResultData[DataPoint].PartSN, 1)
                                            + "," + Extract_ID_XY(RFile.ResultData[DataPoint].PartSN, 2)
                                            + "," + RFile.ResultData[DataPoint].Site
                                            + "," + RFile.ResultData[DataPoint].Time.ToString()
                                            + "," + RFile.ResultData[DataPoint].TotalTest.ToString()
                                            + "," + RFile.ResultData[DataPoint].Lot_ID
                                            + "," + RFile.ResultData[DataPoint].Wafer_ID;


                        for (int iData = 0; iData < tmp_DataLength; iData++)
                        {
                            Output[DataPoint] += "," + (RFile.ResultData[DataPoint].Data[iData]).ToString();
                        }
                        Output[DataPoint] += "," + RFile.ResultData[DataPoint].PassFail.ToString()
                                            + "," + RFile.ResultData[DataPoint].TimeStamp
                                            + "," + RFile.ResultData[DataPoint].IndexTime.ToString()
                                            + "," + RFile.ResultData[DataPoint].PartSN
                                            + "," + RFile.ResultData[DataPoint].SWBinName
                                            + "," + RFile.ResultData[DataPoint].HWBinName;
                        System.IO.File.AppendAllText(NewFile_Path, Output[DataPoint] + "\r\n");
                        lblChkOutput.Visible = true;
                    }
                }
            }
            else
            {
                MessageBox.Show("Missing Results Data File");
            }
            btnXY.Enabled = true;
            
        }
        private string Extract_ID_XY(string inputStr, int Item)
        {
            string[] tmpStr = inputStr.Split('_');
            if (tmpStr.Length == 3)
            {
                return tmpStr[Item];
            }
            else
            {
                return "";
            }
        }
        private void btnOutput_Click(object sender, EventArgs e)
        {
            lblChkOutput.Visible = false;
            txtOutput.BackColor = SystemColors.Window;
            btnOutput.BackColor = SystemColors.Control;
            txtOutput.Text = "";
            SaveFileDialog Dialog = new SaveFileDialog();
            Dialog.Filter = "Result File (*.csv)|*.csv";

            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                txtOutput.Text = Dialog.FileName;
            }
        }
        private string SafeFileName(string FileName)
        {
            string[] tmpStr = FileName.Split('\\');
            return tmpStr[tmpStr.Length - 1];
        }
        private string FolderName(string FileName)
        {
            string tmpStr = FileName.Substring(0, (FileName.Length - SafeFileName(FileName).Length));
            return tmpStr;
        }
        private string Append_Filename(string FileName, string AppendStr)
        {
            string[] tmpStr = FileName.Split('.');
            if (AppendStr != "")
            {
                return (FileName.Substring(0, (FileName.Length - (tmpStr[tmpStr.Length - 1]).Length - 1)) + "_" + AppendStr);
            }
            else
            {
                return FileName;
            }
        }
        private string Append_Filename(string FileName, string AppendStr, string changeType)
        {
            string[] tmpStr = FileName.Split('.');
            if (AppendStr != "")
            {
                return (FileName.Substring(0, (FileName.Length - (tmpStr[tmpStr.Length - 1]).Length - 1)) + "_" + AppendStr + "." + changeType);
            }
            else
            {
                return FileName;
            }
        }

        private void btnPrevResult_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            txtPrevResult.BackColor = SystemColors.Window;
            btnPrevResult.BackColor = SystemColors.Control;
            dialog.Filter = "Result File (*.csv)|*.csv";
            lblChkPrevResult.Visible = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPrevResult.Text = dialog.FileName;
                if (Result != null)
                {
                    Result = null;
                    Result = new cTestResultsReader.cTestResultsReader();
                }
                Result.Result_FileName = dialog.FileName;
                Result.bExtract_XY = chk_ExtractXYLoc.Checked;
                if (Previous_ResultFile.RawData != null)
                {
                    Previous_ResultFile = new cTestResultsReader.s_Results();
                }
                if (Result.Read_File())
                {
                    Previous_ResultFile = Result.parse_Results;
                    if (Previous_ResultFile.RawData != null)
                    {
                        lblChkPrevResult.Text = "P";
                        lblChkPrevResult.ForeColor = Color.DarkGreen;
                        lblChkPrevResult.Visible = true;
                    }
                    else
                    {
                        lblChkPrevResult.Text = "O";
                        lblChkPrevResult.ForeColor = Color.Red;
                        lblChkPrevResult.Visible = true;
                    }
                }
            }
            else
            {
                txtPrevResult.Text = "";
            }
        }
        private void btnNewSpec_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            txtNewSpec.BackColor = SystemColors.Window;
            btnNewSpec.BackColor = SystemColors.Control;
            dialog.Filter = "Specification File (*.csv)|*.csv";
            lblChkNewSpec.Visible = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtNewSpec.Text = dialog.FileName;
                if (Spec != null)
                {
                    Spec = null;
                    Spec = new cTestSpecificationReader.cTestSpec();
                }
                Spec.Spec_FileName = dialog.FileName;
                if (New_Test_Spec.SerialBin != null)
                {
                    New_Test_Spec = new cTestSpecificationReader.s_SpecFile();
                }
                if (Spec.Read_File())
                {
                    New_Test_Spec = Spec.parse_Spec;
                    if (New_Test_Spec.SerialBin != null)
                    {
                        lblChkNewSpec.Text = "P";
                        lblChkNewSpec.ForeColor = Color.DarkGreen;
                        lblChkNewSpec.Visible = true;
                    }
                    else
                    {
                        lblChkNewSpec.Text = "O";
                        lblChkNewSpec.ForeColor = Color.Red;
                        lblChkNewSpec.Visible = true;
                    }

                }
            }
            else
            {
                txtNewSpec.Text = "";
            }

        }
        private void button11_Click(object sender, EventArgs e)
        {
            frmMap fMap = new frmMap();
            fMap.Parse_Result2 = Previous_ResultFile;
            fMap.Show();

        }
        private void button12_Click(object sender, EventArgs e)
        {
            frmMap fMap = new frmMap();
            fMap.Compare_Maps = true;
            fMap.Parse_Result = ResultFile;
            fMap.Parse_Result2 = Previous_ResultFile;
            fMap.Show();

        }
        private void btnRetest_Click(object sender, EventArgs e)
        {
            StatusStripLabel.Text = ""; 
            btnRetest.Enabled = false;
            
            if (ResultFile.RawData == null)
            {
                MessageBox.Show("Missing Results File Data");
                if (Previous_ResultFile.RawData == null)
                {
                    txtPrevResult.BackColor = Color.Pink;
                    btnPrevResult.BackColor = Color.Pink;
                }
                txtResult.BackColor = Color.Pink;
                btnResult.BackColor = Color.Pink;
                btnResult.Focus();
                btnRetest.Enabled = true;

               
                return;
            }
            if (Previous_ResultFile.RawData == null)
            {
                MessageBox.Show("Missing New Merging Results File Data");
                txtPrevResult.BackColor = Color.Pink;
                btnPrevResult.BackColor = Color.Pink;
                btnPrevResult.Focus();
                btnRetest.Enabled = true;
                return;
            }

            if (txtOutput.Text == "")
            {
                SaveFileDialog Dialog = new SaveFileDialog();
                Dialog.Filter = "Result File (*.csv)|*.csv";
                Dialog.InitialDirectory = FolderName(txtResult.Text);
                Dialog.AddExtension = true;
                Dialog.FileName = Append_Filename(SafeFileName(txtResult.Text), "Retest");
                if (Dialog.ShowDialog() == DialogResult.OK)
                {
                    lblChkOutput.Visible = false;
                    txtOutput.Text = Dialog.FileName;
                }
            }
            if (txtOutput.Text != "")
            {
                string[] Output = new string[ResultFile.ResultData.Length];
                string NewFile_Path = txtOutput.Text;
                bool[] ChkParameters;

                ChkParameters = null;
                if (chkSelectHeader.Checked)
                {
                    frmHeaderSelect fhead = new frmHeaderSelect();
                    fhead.parse_TestParameters = ResultFile.ResultHeader.TestParameter_Name;
                    DialogResult rslt = fhead.ShowDialog();
                    if (rslt == DialogResult.OK)
                    {
                        ChkParameters = fhead.parse_ChkParameters;
                    }
                }

                System.IO.File.WriteAllText(NewFile_Path, New_Result_Header(ResultFile));
                System.IO.File.AppendAllText(NewFile_Path, "\r\n");
                System.IO.File.AppendAllText(NewFile_Path, New_Result_Data_Header(ResultFile, ChkParameters, chkIncludeOriginal.Checked, "",0));

                int tmp_DataLength = ResultFile.ResultData[1].Data.Length;


                if (ResultFile.XY_Info.Valid && Previous_ResultFile.XY_Info.Valid)
                {
                    int minX = ResultFile.XY_Info.XY_MinMax.Min_X;
                    int minY = ResultFile.XY_Info.XY_MinMax.Min_Y;
                    int maxX = ResultFile.XY_Info.XY_MinMax.Max_X;
                    int maxY = ResultFile.XY_Info.XY_MinMax.Max_Y;
                    int tmpA_I;
                    int tmpB_I;
                    int Dataout = 0;

                    if (Previous_ResultFile.XY_Info.XY_MinMax.Min_X < minX)
                    {
                        minX = Previous_ResultFile.XY_Info.XY_MinMax.Min_X;
                    }
                    if (Previous_ResultFile.XY_Info.XY_MinMax.Min_Y < minY)
                    {
                        minX = Previous_ResultFile.XY_Info.XY_MinMax.Min_Y;
                    }
                    if (Previous_ResultFile.XY_Info.XY_MinMax.Max_X > maxX)
                    {
                        maxX = Previous_ResultFile.XY_Info.XY_MinMax.Max_X;
                    }
                    if (Previous_ResultFile.XY_Info.XY_MinMax.Max_Y > maxY)
                    {
                        maxY = Previous_ResultFile.XY_Info.XY_MinMax.Max_Y;
                    }
                    for (int iX = minX; iX < maxX + 1; iX++)
                    {
                        for (int iY = minY; iY < maxY + 1; iY++)
                        {
                            Output[Dataout] = "";
                            tmpA_I = Previous_ResultFile.XY_Info.Match_Position[Parse_PosData(iX), Parse_PosData(iY)];
                            tmpB_I = ResultFile.XY_Info.Match_Position[Parse_PosData(iX), Parse_PosData(iY)];

                            if (chkIncludeOriginal.Checked)
                            {
                                Output[Dataout] += generate_OriginData(tmpA_I, tmp_DataLength, ChkParameters, ResultFile);
                                Output[Dataout] += generate_OriginData(tmpB_I, tmp_DataLength, ChkParameters, Previous_ResultFile);
                            }

                            if (tmpB_I != -1)
                            {
                                if (tmpA_I != -1)
                                {
                                    if (Previous_ResultFile.ResultData[tmpA_I].SWBinName.ToUpper().Contains("PASS"))
                                    {
                                        {
                                            Output[Dataout] += Previous_ResultFile.ResultData[tmpA_I].ID
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].SBin.ToString()
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].HBin.ToString()
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].Die_X.ToString()
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].Die_Y.ToString()
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].Site
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].Time.ToString()
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].TotalTest.ToString()
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].Lot_ID
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].Wafer_ID;

                                            for (int iData = 0; iData < tmp_DataLength; iData++)
                                            {
                                                if (ChkParameters == null)
                                                {
                                                    Output[Dataout] += "," + (Previous_ResultFile.ResultData[tmpA_I].Data[iData]).ToString();
                                                }
                                                else
                                                {
                                                    if (ChkParameters[iData])
                                                    {
                                                        Output[Dataout] += "," + (Previous_ResultFile.ResultData[tmpA_I].Data[iData]).ToString();
                                                    }
                                                }
                                            }
                                            Output[Dataout] += "," + Previous_ResultFile.ResultData[tmpA_I].PassFail.ToString()
                                                                + "," + Previous_ResultFile.ResultData[tmpA_I].TimeStamp
                                                                + "," + Previous_ResultFile.ResultData[tmpA_I].IndexTime.ToString()
                                                                + "," + Previous_ResultFile.ResultData[tmpA_I].PartSN
                                                                + "," + Previous_ResultFile.ResultData[tmpA_I].SWBinName
                                                                + "," + Previous_ResultFile.ResultData[tmpA_I].HWBinName;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            Output[Dataout] += ResultFile.ResultData[tmpB_I].ID
                                                        + "," + ResultFile.ResultData[tmpB_I].SBin.ToString()
                                                        + "," + ResultFile.ResultData[tmpB_I].HBin.ToString()
                                                        + "," + ResultFile.ResultData[tmpB_I].Die_X.ToString()
                                                        + "," + ResultFile.ResultData[tmpB_I].Die_Y.ToString()
                                                        + "," + ResultFile.ResultData[tmpB_I].Site
                                                        + "," + ResultFile.ResultData[tmpB_I].Time.ToString()
                                                        + "," + ResultFile.ResultData[tmpB_I].TotalTest.ToString()
                                                        + "," + ResultFile.ResultData[tmpB_I].Lot_ID
                                                        + "," + ResultFile.ResultData[tmpB_I].Wafer_ID;

                                            for (int iData = 0; iData < tmp_DataLength; iData++)
                                            {
                                                if (ChkParameters == null)
                                                {
                                                    Output[Dataout] += "," + (ResultFile.ResultData[tmpB_I].Data[iData]).ToString();
                                                }
                                                else
                                                {
                                                    if (ChkParameters[iData])
                                                    {
                                                        Output[Dataout] += "," + (ResultFile.ResultData[tmpB_I].Data[iData]).ToString();
                                                    }
                                                }
                                            }
                                            Output[Dataout] += "," + ResultFile.ResultData[tmpB_I].PassFail.ToString()
                                                                + "," + ResultFile.ResultData[tmpB_I].TimeStamp
                                                                + "," + ResultFile.ResultData[tmpB_I].IndexTime.ToString()
                                                                + "," + ResultFile.ResultData[tmpB_I].PartSN
                                                                + "," + ResultFile.ResultData[tmpB_I].SWBinName
                                                                + "," + ResultFile.ResultData[tmpB_I].HWBinName;
                                        }
                                    }
                                }
                                
                                System.IO.File.AppendAllText(NewFile_Path, Output[Dataout] + "\r\n");
                            }
                            else
                            {
                                if (tmpA_I != -1)
                                {
                                    Output[Dataout] += Previous_ResultFile.ResultData[tmpA_I].ID
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].SBin.ToString()
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].HBin.ToString()
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].Die_X.ToString()
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].Die_Y.ToString()
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].Site
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].Time.ToString()
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].TotalTest.ToString()
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].Lot_ID
                                                + "," + Previous_ResultFile.ResultData[tmpA_I].Wafer_ID;

                                    for (int iData = 0; iData < tmp_DataLength; iData++)
                                    {
                                        if (ChkParameters == null)
                                        {
                                            Output[Dataout] += "," + (Previous_ResultFile.ResultData[tmpA_I].Data[iData]).ToString();
                                        }
                                        else
                                        {
                                            if (ChkParameters[iData])
                                            {
                                                Output[Dataout] += "," + (Previous_ResultFile.ResultData[tmpA_I].Data[iData]).ToString();
                                            }
                                        }
                                    }
                                    Output[Dataout] += "," + Previous_ResultFile.ResultData[tmpA_I].PassFail.ToString()
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].TimeStamp
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].IndexTime.ToString()
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].PartSN
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].SWBinName
                                                        + "," + Previous_ResultFile.ResultData[tmpA_I].HWBinName;
                                    System.IO.File.AppendAllText(NewFile_Path, Output[Dataout] + "\r\n");
                                    lblChkOutput.Visible = true;
                                }
                            }
                        }
                    }
                }
                else if (ResultFile.ResultData.Length == Previous_ResultFile.ResultData.Length)
                {
                    for (int Dataout = 0; Dataout < tmp_DataLength; Dataout++)
                    {
                        if (!Previous_ResultFile.ResultData[Dataout].SWBinName.ToUpper().Contains("PASS"))
                        {
                            Output[Dataout] = ResultFile.ResultData[Dataout].ID
                                        + "," + ResultFile.ResultData[Dataout].SBin.ToString()
                                        + "," + ResultFile.ResultData[Dataout].HBin.ToString()
                                        + "," + ResultFile.ResultData[Dataout].Die_X.ToString()
                                        + "," + ResultFile.ResultData[Dataout].Die_Y.ToString()
                                        + "," + ResultFile.ResultData[Dataout].Site
                                        + "," + ResultFile.ResultData[Dataout].Time.ToString()
                                        + "," + ResultFile.ResultData[Dataout].TotalTest.ToString()
                                        + "," + ResultFile.ResultData[Dataout].Lot_ID
                                        + "," + ResultFile.ResultData[Dataout].Wafer_ID;

                            for (int iData = 0; iData < tmp_DataLength; iData++)
                            {
                                if (ChkParameters == null)
                                {
                                    Output[Dataout] += "," + (ResultFile.ResultData[Dataout].Data[iData]).ToString();
                                }
                                else
                                {
                                    if (ChkParameters[iData])
                                    {
                                        Output[Dataout] += "," + (ResultFile.ResultData[Dataout].Data[iData]).ToString();
                                    }
                                }
                            }
                            Output[Dataout] += "," + ResultFile.ResultData[Dataout].PassFail.ToString()
                                                + "," + ResultFile.ResultData[Dataout].TimeStamp
                                                + "," + ResultFile.ResultData[Dataout].IndexTime.ToString()
                                                + "," + ResultFile.ResultData[Dataout].PartSN
                                                + "," + ResultFile.ResultData[Dataout].SWBinName
                                                + "," + ResultFile.ResultData[Dataout].HWBinName;
                            System.IO.File.AppendAllText(NewFile_Path, Output[Dataout] + "\r\n");
                        }
                        else
                        {
                            Output[Dataout] = Previous_ResultFile.ResultData[Dataout].ID
                                        + "," + Previous_ResultFile.ResultData[Dataout].SBin.ToString()
                                        + "," + Previous_ResultFile.ResultData[Dataout].HBin.ToString()
                                        + "," + Previous_ResultFile.ResultData[Dataout].Die_X.ToString()
                                        + "," + Previous_ResultFile.ResultData[Dataout].Die_Y.ToString()
                                        + "," + Previous_ResultFile.ResultData[Dataout].Site
                                        + "," + Previous_ResultFile.ResultData[Dataout].Time.ToString()
                                        + "," + Previous_ResultFile.ResultData[Dataout].TotalTest.ToString()
                                        + "," + Previous_ResultFile.ResultData[Dataout].Lot_ID
                                        + "," + Previous_ResultFile.ResultData[Dataout].Wafer_ID;

                            for (int iData = 0; iData < tmp_DataLength; iData++)
                            {
                                if (ChkParameters == null)
                                {
                                    Output[Dataout] += "," + (Previous_ResultFile.ResultData[Dataout].Data[iData]).ToString();
                                }
                                else
                                {
                                    if (ChkParameters[iData])
                                    {
                                        Output[Dataout] += "," + (Previous_ResultFile.ResultData[Dataout].Data[iData]).ToString();
                                    }
                                }
                            }
                            Output[Dataout] += "," + Previous_ResultFile.ResultData[Dataout].PassFail.ToString()
                                                + "," + Previous_ResultFile.ResultData[Dataout].TimeStamp
                                                + "," + Previous_ResultFile.ResultData[Dataout].IndexTime.ToString()
                                                + "," + Previous_ResultFile.ResultData[Dataout].PartSN
                                                + "," + Previous_ResultFile.ResultData[Dataout].SWBinName
                                                + "," + Previous_ResultFile.ResultData[Dataout].HWBinName;
                            System.IO.File.AppendAllText(NewFile_Path, Output[Dataout] + "\r\n");
                            lblChkOutput.Visible = true;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Unable to Merge Files! \r\n No X-Y position data available and Data Information are different", "Error Merging");
                }
                StatusStripLabel.Text = "Retest File Completed!!";
            }
            btnRetest.Enabled = true;
        }

        private void btnRascoXML_Click(object sender, EventArgs e)
        {
            //RascoMap.Map_Filename = "e:\\Work Folder\\Strip Handler\\G85_stripmap_with_comments_V2x.xml";
            txtRascoXML.BackColor = SystemColors.Window;
            btnRascoXML.BackColor = SystemColors.Control;
            lblChkRascoXML.Visible = false;
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Filter = "Rasco Map (*.xml)|*.xml";

            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                txtRascoXML.Text = Dialog.FileName;
                RascoMap.Map_Filename = Dialog.FileName;
                if (RascoMap.MapData.Data != null)
                {
                    RascoMap = null;
                    RascoMap = new cMapReader.cRascoMap();
                }

                if (RascoMap.Read_Map())
                {
                    lblChkRascoXML.Text = "P";
                    lblChkRascoXML.ForeColor = Color.DarkGreen;
                    lblChkRascoXML.Visible = true;
                }
                else
                {
                    lblChkRascoXML.Text = "O";
                    lblChkRascoXML.ForeColor = Color.Red;
                    lblChkRascoXML.Visible = true;
                }
            }
        }
        private void btnGenRascoXML_Click(object sender, EventArgs e)
        {
            string[] MapStartHeaderStr = new string[3];
            string[] MapEndHeaderStr = new string[2];
            string[] MapBinCode;
            string[] MapRowData;

            int tmpLocation;
            int rtnBin;

            string NewMapFileName;

            btnGenRascoXML.Enabled = false;
            if (RascoMap.MapData.Data == null)
            {
                MessageBox.Show("Missing Rasco Map XML Data");
                if (ResultFile.RawData == null)
                {
                    txtResult.BackColor = Color.Pink;
                    btnResult.BackColor = Color.Pink;
                }
                if (New_Test_Spec.SerialBin == null)
                {
                    txtNewSpec.BackColor = Color.Pink;
                    btnNewSpec.BackColor = Color.Pink;
                }
                txtRascoXML.BackColor = Color.Pink;
                btnRascoXML.BackColor = Color.Pink;
                btnRascoXML.Focus();
                btnGenRascoXML.Enabled = true;
                return;
            }
            if (ResultFile.RawData == null)
            {
                MessageBox.Show("Missing Results File Data");
                if (New_Test_Spec.SerialBin == null)
                {
                    txtNewSpec.BackColor = Color.Pink;
                    btnNewSpec.BackColor = Color.Pink;
                }
                txtResult.BackColor = Color.Pink;
                btnResult.BackColor = Color.Pink;
                btnResult.Focus();
                btnGenRascoXML.Enabled = true;
                return;
            }
            if (New_Test_Spec.SerialBin == null)
            {
                
                MessageBox.Show("Missing New Specification / Limit File Data");

                txtNewSpec.BackColor = Color.Pink;
                btnNewSpec.BackColor = Color.Pink;
                btnNewSpec.Focus();
                btnGenRascoXML.Enabled = true;
                return;
            }
            if ((ResultFile.RawData != null) && (RascoMap.MapData.Data != null) && (New_Test_Spec.SerialBin != null))
            {
                SaveFileDialog Dialog = new SaveFileDialog();
                Dialog.Filter = "Rasco Map (*.xml)|*.xml";
                Dialog.InitialDirectory = FolderName(txtRascoXML.Text);
                Dialog.AddExtension = true;

                if (chk_ExtractXYLoc.Checked)
                {
                    string[] tmpStrArr = ResultFile.ResultData[0].PartSN.Split('_');
                    Dialog.FileName = tmpStrArr[0] + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "00_Regen";
                }
                else
                {
                    string trimStr = ResultFile.TestHeader.GlobalInfo.Product;
                    if (ResultFile.TestHeader.GlobalInfo.Lot.Trim() != "")
                    {
                        trimStr += "_" + ResultFile.TestHeader.GlobalInfo.Lot;
                    }
                    if (ResultFile.TestHeader.GlobalInfo.SubLot.Trim() != "")
                    {
                        trimStr += "_" + ResultFile.TestHeader.GlobalInfo.SubLot.Trim();
                    }
                    trimStr += "_";

                    string[] tmpStrArrB = SafeFileName(txtResult.Text.ToUpper()).Trim(trimStr.ToUpper().ToCharArray()).Split('_');
                    Dialog.FileName = tmpStrArrB[0] + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "00_Regen";
                }
                //Dialog.FileName = Append_Filename(SafeFileName(txtRascoXML.Text), "Regen");

                if (Dialog.ShowDialog() == DialogResult.OK)
                {
                    NewMapFileName = Dialog.FileName;
                    MapStartHeaderStr[0] = RascoMap.MapData.RawData[0];
                    MapStartHeaderStr[1] = "<Map xmlns:semi=\"http://www.semi.org\" SubstrateType=\"Strip\" SubstrateId=\"00FA00709\" CarrierType=\"Magazine\" FormatRevision=\"SEMI G85-0703\">";
                    MapStartHeaderStr[2] = "  <Device ProductId=\"SO3000\" LotID=\"shiva_2_temp_85C\" Orientation=\"0\" Rows=\"26\" Columns=\"100\" BinType=\"Decimal\" NullBin=\"255\" OriginLocation=\"2\" HandlerLowerLeftOriginLocation=\"3\">";

                    MapEndHeaderStr[0] = "  </Device>";
                    MapEndHeaderStr[1]= "</Map>";
                    
                    MapBinCode = new string[New_Test_Spec.HW_Bin.Length];

                    for (int iMapBin = 0; iMapBin < New_Test_Spec.HW_Bin.Length; iMapBin++)
                    {
                        MapBinCode[iMapBin] = "    <Bin BinCode=\"" + New_Test_Spec.HW_Bin[iMapBin].Bin_Number.ToString() + "\" BinQuality=\"" + cInfo2PassFail(New_Test_Spec.HW_Bin[iMapBin].Name.Trim()) + "\" />";
                    }

                    MapRowData = new string[ResultFile.XY_Info.XY_MinMax.Max_Y + 2];
                    MapRowData[0] = "\r\n    <Data>";
                    MapRowData[MapRowData.Length-1] = "    </Data>\r\n";

                    for (int iY = 1; iY <= ResultFile.XY_Info.XY_MinMax.Max_Y; iY++)
                    {
                        MapRowData[iY] = "      <Row><![CDATA[";
                        for (int iX = 1; iX <= ResultFile.XY_Info.XY_MinMax.Max_X; iX++)
                        {
                            tmpLocation = ResultFile.XY_Info.Match_Position[iX, iY];
                            if (tmpLocation > -1)
                            {
                                rtnBin = ResultFile.ResultData[tmpLocation].HBin;
                            }
                            else
                            {
                                rtnBin = 255;
                            }
                            if(iX == 1)
                            {
                                MapRowData[iY] += rtnBin.ToString();
                            }
                            else
                            {
                                MapRowData[iY] += " " + rtnBin.ToString();
                            }
                        }
                            MapRowData[iY] += "]]></Row>";
                    }
                    System.IO.File.WriteAllLines(NewMapFileName, MapStartHeaderStr);
                    System.IO.File.AppendAllText(NewMapFileName, string.Join("\r\n", MapBinCode));
                    System.IO.File.AppendAllText(NewMapFileName, string.Join("\r\n", MapRowData));
                    System.IO.File.AppendAllText(NewMapFileName, string.Join("\r\n", MapEndHeaderStr));

                }
            }
            btnGenRascoXML.Enabled = true;
           
        }
        private string cInfo2PassFail(string inputStr)
        {
            if(inputStr.ToUpper().Contains("PASS"))
            {
                return "Pass";
            }
            else if(inputStr.ToUpper().Contains("RETEST"))
            {
                return "Retest";
            }
            else
            {
                return "Fail";
            }
        }
        private string GenerateNewBins()
        {
            string tmpStr ="";
            for (int iBin = 0; iBin < New_Test_Spec.HW_Bin.Length; iBin++)
            {
                tmpStr += "\t\t<Bin BinCode=\"" + New_Test_Spec.HW_Bin[iBin].Bin_Number.ToString() + "\" BinQuality=\"" + CheckBin(New_Test_Spec.HW_Bin[iBin].Name) + "\"/>\t<!-- " + New_Test_Spec.HW_Bin[iBin].Name + " -->\r\n";
            }
            return tmpStr;
        }

        private string CheckBin(string inputStr)
        {
            if (inputStr.ToUpper().Contains("PASS"))
            {
                return "Pass";
            }
            else
            {
                return "Fail";
            }
        }
        private string GenerateRowData()
        {
            string tmpStr = "";
            int count=0;
            for (int iRow = 0; iRow < RascoMap.MapData.DeviceInfo.Rows; iRow++)
            {
                tmpStr += "\t\t\t<Row><![CDATA[";
                //code not complete
                for (int iCol = 0; iCol < RascoMap.MapData.DeviceInfo.Columns; iCol++)
                {
                    if (iCol == 0)
                    {
                        tmpStr += ResultFile.ResultData[count].HBin.ToString();
                    }
                    else
                    {
                        tmpStr += ResultFile.ResultData[count].HBin.ToString() + " ";
                    }
                    count++;
                }
                tmpStr += "]]></Row>\r\n";
            }
            return tmpStr;
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            lblChkOutput.Visible = false;
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            StatusStripLabel.Text = ""; 
            btnMerge.Enabled = false;
            int FileCount = 2;

            cTestResultsReader.s_Results[] RsltDataFile;

            if (ResultFile.RawData == null)
            {
                MessageBox.Show("Missing Results File Data");
                if (Previous_ResultFile.RawData == null)
                {
                    txtPrevResult.BackColor = Color.Pink;
                    btnPrevResult.BackColor = Color.Pink;
                }
                txtResult.BackColor = Color.Pink;
                btnResult.BackColor = Color.Pink;
                btnResult.Focus();
                btnRetest.Enabled = true;


                return;
            }
            if (Previous_ResultFile.RawData == null)
            {
                MessageBox.Show("Missing New Merging Results File 2 Data");
                txtPrevResult.BackColor = Color.Pink;
                btnPrevResult.BackColor = Color.Pink;
                btnPrevResult.Focus();
                btnRetest.Enabled = true;
                return;
            }

            if (txtOutput.Text == "")
            {
                SaveFileDialog Dialog = new SaveFileDialog();
                Dialog.Filter = "Result File (*.csv)|*.csv";
                Dialog.InitialDirectory = FolderName(txtResult.Text);
                Dialog.AddExtension = true;
                Dialog.FileName = Append_Filename(SafeFileName(txtResult.Text), "Merge");
                if (Dialog.ShowDialog() == DialogResult.OK)
                {
                    lblChkOutput.Visible = false;
                    txtOutput.Text = Dialog.FileName;
                }
            }
            if (txtOutput.Text != "")
            {
                RsltDataFile = new cTestResultsReader.s_Results[FileCount];
                {
                    RsltDataFile[0] = ResultFile;
                    RsltDataFile[1] = Previous_ResultFile;
                    // add more codes
                    if (R3.RawData != null)
                    {
                        FileCount++;
                        Array.Resize(ref RsltDataFile, FileCount);
                        RsltDataFile[FileCount - 1] = R3;
                    }
                    if (R4.RawData != null)
                    {
                        FileCount++;
                        Array.Resize(ref RsltDataFile, FileCount);
                        RsltDataFile[FileCount - 1] = R4;
                    }
                    if (R5.RawData != null)
                    {
                        FileCount++;
                        Array.Resize(ref RsltDataFile, FileCount);
                        RsltDataFile[FileCount - 1] = R5;
                    }
                }
                string Output;
                string NewFile_Path = txtOutput.Text;
                bool[] ChkParameters;
                int tmp_i;
                int tmp_DataLength =  RsltDataFile[0].ResultData[1].Data.Length;

                ChkParameters = null;
                if (chkSelectHeader.Checked)
                {
                    frmHeaderSelect fhead = new frmHeaderSelect();
                    fhead.parse_TestParameters = RsltDataFile[0].ResultHeader.TestParameter_Name;
                    DialogResult rslt = fhead.ShowDialog();
                    if (rslt == DialogResult.OK)
                    {
                        ChkParameters = fhead.parse_ChkParameters;
                    }
                }
                System.IO.File.WriteAllText(NewFile_Path, New_Result_Header(ResultFile));
                System.IO.File.AppendAllText(NewFile_Path, "\r\n");
                System.IO.File.AppendAllText(NewFile_Path, New_Result_Data_Header(ResultFile, ChkParameters, true, "Merge", FileCount));

                bool b_valid = true;
                bool b_DataPresent;
                // Check Valid XY position
                for (int valid = 0; valid < FileCount; valid++)
                {
                    if (!RsltDataFile[valid].XY_Info.Valid)
                    {
                        b_valid = false;
                    }
                }
                if (b_valid)
                {
                    #region "Check Min and Max Location"
                    int minX = RsltDataFile[0].XY_Info.XY_MinMax.Min_X;
                    int minY = RsltDataFile[0].XY_Info.XY_MinMax.Min_Y;
                    int maxX = RsltDataFile[0].XY_Info.XY_MinMax.Max_X;
                    int maxY = RsltDataFile[0].XY_Info.XY_MinMax.Max_Y;

                    for (int iRslt = 1; iRslt < FileCount; iRslt++)
                    {
                        if (RsltDataFile[iRslt].XY_Info.XY_MinMax.Min_X < minX)
                        {
                            minX = RsltDataFile[iRslt].XY_Info.XY_MinMax.Min_X;
                        }
                        if (RsltDataFile[iRslt].XY_Info.XY_MinMax.Min_Y < minY)
                        {
                            minX = RsltDataFile[iRslt].XY_Info.XY_MinMax.Min_Y;
                        }
                        if (RsltDataFile[iRslt].XY_Info.XY_MinMax.Max_X > maxX)
                        {
                            maxX = RsltDataFile[iRslt].XY_Info.XY_MinMax.Max_X;
                        }
                        if (RsltDataFile[iRslt].XY_Info.XY_MinMax.Max_Y > maxY)
                        {
                            maxY = RsltDataFile[iRslt].XY_Info.XY_MinMax.Max_Y;
                        }
                    }
                    #endregion
                    for (int iY = minY; iY < maxY + 1; iY++)
                    {
                        for (int iX = minX; iX < maxX + 1; iX++)
                        {
                            Output = "";
                            b_DataPresent = false;
                            for (int iRslt = 0; iRslt < FileCount; iRslt++)
                            {
                                tmp_i = RsltDataFile[iRslt].XY_Info.Match_Position[Parse_PosData(iX), Parse_PosData(iY)];
                                if (tmp_i > -1)
                                {
                                    b_DataPresent = true;
                                }
                            }
                            if (b_DataPresent)
                            {
                                for (int iRslt = 0; iRslt < FileCount; iRslt++)
                                {
                                    tmp_i = RsltDataFile[iRslt].XY_Info.Match_Position[Parse_PosData(iX), Parse_PosData(iY)];
                                    Output += generate_OriginData(tmp_i, tmp_DataLength, ChkParameters, RsltDataFile[iRslt]);
                                }
                            }
                            if (Output != "") System.IO.File.AppendAllText(NewFile_Path, Output + "\r\n");
                        }
                    }
                }
                else
                {
                    // It may not merge correctly
                    int MaxRow = 0;
                    int DataValid = 0;
                    for (int iRslt = 0; iRslt < FileCount; iRslt++)
                    {
                        if (MaxRow < RsltDataFile[iRslt].RawData.Length)
                        {
                            MaxRow = RsltDataFile[iRslt].RawData.Length;
                        }
                    }
                    #region "Select Header"
                    ChkParameters = null;
                    if (chkSelectHeader.Checked)
                    {
                        bool b_chkHeaderError = false;
                        int MaxHeader = RsltDataFile[0].ResultHeader.TestNumber.Length;
                        for (int iRslt = 1; iRslt < FileCount; iRslt++)
                        {
                            if (MaxHeader != RsltDataFile[iRslt].ResultHeader.TestNumber.Length)
                            {
                                b_chkHeaderError = true;
                            }
                        }

                        if (b_chkHeaderError)
                        {
                            MessageBox.Show("Potentially Mergering Error due to inconsistent Headers");
                        }
                    }
                    #endregion
                    for (int iRow = 0; iRow < MaxRow; iRow++)
                    {
                        Output = "";
                        
                        for (int iRslt = 0; iRslt < FileCount; iRslt++)
                        {
                            if(iRow < RsltDataFile[iRslt].ResultData.Length)
                            {
                                DataValid = iRow;
                            }
                            else
                            {
                                DataValid = -1;
                            }
                            Output +=generate_OriginData(DataValid, tmp_DataLength, ChkParameters, RsltDataFile[iRslt]);
                        }

                        if (Output != "") System.IO.File.AppendAllText(NewFile_Path, Output + "\r\n");
                    }
                }
                StatusStripLabel.Text = "File Merging Completed!!";
            }
            btnMerge.Enabled = true;
        }
        private void btnR3_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            txtR3.BackColor = SystemColors.Window;
            btnR3.BackColor = SystemColors.Control;
            dialog.Filter = "Result File (*.csv)|*.csv";
            lblChkR3.Visible = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtR3.Text = dialog.FileName;
                if (Result != null)
                {
                    Result = null;
                    Result = new cTestResultsReader.cTestResultsReader();
                }
                Result.Result_FileName = dialog.FileName;
                Result.bExtract_XY = chk_ExtractXYLoc.Checked;
                R3 = new cTestResultsReader.s_Results();

                if (Result.Read_File())
                {
                    R3 = Result.parse_Results;
                    if (R3.RawData != null)
                    {
                        lblChkR3.Text = "P";
                        lblChkR3.ForeColor = Color.DarkGreen;
                        lblChkR3.Visible = true;
                    }
                    else
                    {
                        lblChkR3.Text = "O";
                        lblChkR3.ForeColor = Color.Red;
                        lblChkR3.Visible = true;
                    }
                }
            }
            else
            {
                txtR3.Text = "";
            }
        }
        private void btnR4_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            txtR4.BackColor = SystemColors.Window;
            btnR4.BackColor = SystemColors.Control;
            dialog.Filter = "Result File (*.csv)|*.csv";
            lblChkR4.Visible = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtR4.Text = dialog.FileName;
                if (Result != null)
                {
                    Result = null;
                    Result = new cTestResultsReader.cTestResultsReader();
                }
                Result.Result_FileName = dialog.FileName;
                Result.bExtract_XY = chk_ExtractXYLoc.Checked;
                R4 = new cTestResultsReader.s_Results();

                if (Result.Read_File())
                {
                    R4 = Result.parse_Results;
                    if (R4.RawData != null)
                    {
                        lblChkR4.Text = "P";
                        lblChkR4.ForeColor = Color.DarkGreen;
                        lblChkR4.Visible = true;
                    }
                    else
                    {
                        lblChkR4.Text = "O";
                        lblChkR4.ForeColor = Color.Red;
                        lblChkR4.Visible = true;
                    }
                }
            }
            else
            {
                txtR4.Text = "";
            }
        }
        private void btnR5_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            txtR5.BackColor = SystemColors.Window;
            btnR5.BackColor = SystemColors.Control;
            dialog.Filter = "Result File (*.csv)|*.csv";
            lblChkR5.Visible = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtR5.Text = dialog.FileName;
                if (Result != null)
                {
                    Result = null;
                    Result = new cTestResultsReader.cTestResultsReader();
                }
                Result.Result_FileName = dialog.FileName;
                Result.bExtract_XY = chk_ExtractXYLoc.Checked;
                R5 = new cTestResultsReader.s_Results();

                if (Result.Read_File())
                {
                    R5 = Result.parse_Results;
                    if (R5.RawData != null)
                    {
                        lblChkR5.Text = "P";
                        lblChkR5.ForeColor = Color.DarkGreen;
                        lblChkR5.Visible = true;
                    }
                    else
                    {
                        lblChkR5.Text = "O";
                        lblChkR5.ForeColor = Color.Red;
                        lblChkR5.Visible = true;
                    }
                }
            }
            else
            {
                txtR5.Text = "";
            }
        }

        #region "Generate Summary"
        private void Generate_Summary(cTestResultsReader.s_Results ResultInfo, string SummaryPath, float TotalUnit, int[] FailParam, int[,] HWBin)
        {
            string[] tmpstr;
            if (SummaryPath != "")
            {
                System.IO.File.WriteAllLines(SummaryPath, Generate_SummaryFileHeader(txtNewSpec.Text));

                //tmpstr = Generate_SummaryHeader(ResultFile);
                tmpstr = Generate_SummaryHeader(ResultInfo);
                foreach (string s in tmpstr)
                {
                    System.IO.File.AppendAllText(SummaryPath, s + "\r\n");
                }
                //System.IO.File.AppendAllText(SummaryPath, Generate_SummaryHWBin(HWBin, ResultFile, TotalUnit));
                System.IO.File.AppendAllText(SummaryPath, Generate_SummaryHWBin(HWBin, ResultInfo, TotalUnit));
                System.IO.File.AppendAllText(SummaryPath, Generate_SummaryTestResult(FailParam, TotalUnit));
            }
        }
        private string[] Generate_SummaryFileHeader(string SpecLimitPath)
        {
            string[] FileHeader = new string[9];
            StringBuilder RawRsltPath = new StringBuilder("Raw Result File (With H/L Limits) Path: ");
            StringBuilder RawRsltChecksum = new StringBuilder("Raw Result File CheckSum: ");
            FileHeader[1] = "--------------------------------------------------------------------------------";
            FileHeader[2] = "\t\t\t\t\t\tGenerated by External Regen";
            RawRsltPath.Append(SpecLimitPath);
            FileHeader[4] = RawRsltPath.ToString();
            RawRsltChecksum.Append("Regen Function");
            FileHeader[5] = RawRsltChecksum.ToString();
            FileHeader[7] = "--------------------------------------------------------------------------------";
            return FileHeader;
        }
        private string[] Generate_SummaryHeader(cTestResultsReader.s_Results ResultInfo)
        {
            string[] tmpArrStr;
            string[] Header = new string[53];
            StringBuilder tmpBuildStr;
            tmpBuildStr = new StringBuilder(("Program Name:").PadRight(28, ' '), 28);
            Header[0] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.ProgramName;
            tmpBuildStr = new StringBuilder(("Program Revision:").PadRight(28, ' '), 28);
            Header[1] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.ProgramRevision;
            tmpBuildStr = new StringBuilder(("Spec Name:").PadRight(28, ' '), 28);
            Header[2] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.SpecName;
            tmpBuildStr = new StringBuilder(("Spec Version:").PadRight(28, ' '), 28);
            Header[3] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.SpecVersion;
            tmpBuildStr = new StringBuilder(("Condition Name:").PadRight(28, ' '), 28);
            Header[4] = tmpBuildStr.ToString() + ResultInfo.TestHeader.ConditionName.ConditionName;
            tmpBuildStr = new StringBuilder(("Correlation Name:").PadRight(28, ' '), 28);
            Header[5] = tmpBuildStr.ToString() + ResultInfo.TestHeader.Correlation_FileName;
            tmpBuildStr = new StringBuilder(("Lot Setup Time:").PadRight(28, ' '), 28);
            Header[6] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.SetupTime;
            tmpBuildStr = new StringBuilder(("Lot Start Time:").PadRight(28, ' '), 28);
            Header[7] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.StartTime;
            tmpBuildStr = new StringBuilder(("Lot Finish Time:").PadRight(28, ' '), 28);
            Header[8] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.FinishTime;
            tmpBuildStr = new StringBuilder(("Device Name:").PadRight(28, ' '), 28);
            Header[9] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.Product;
            tmpBuildStr = new StringBuilder(("Lot Retest:").PadRight(28, ' '), 28);
            Header[10] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("Lot ID:").PadRight(28, ' '), 28);
            Header[11] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.Lot;
            tmpBuildStr = new StringBuilder(("Sublot ID:").PadRight(28, ' '), 28);
            Header[12] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.SubLot;
            tmpBuildStr = new StringBuilder(("Tester Node Name:").PadRight(28, ' '), 28);
            Header[13] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.TesterName;
            tmpBuildStr = new StringBuilder(("Test Head:").PadRight(28, ' '), 28);
            tmpArrStr = ResultInfo.TestHeader.SiteDetails.HeadNumber.Split('#');
            Header[14] = tmpBuildStr.ToString() + CEmpty2Str(tmpArrStr[1].Trim());
            tmpBuildStr = new StringBuilder(("Tester Type:").PadRight(28, ' '), 28);
            Header[15] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.TesterType;
            tmpBuildStr = new StringBuilder(("Serial Number:").PadRight(28, ' '), 28);
            Header[16] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("Operator ID:").PadRight(28, ' '), 28);
            Header[17] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.Operator;
            tmpBuildStr = new StringBuilder(("Clotho Version:").PadRight(28, ' '), 28);
            Header[18] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.ExecRevision;
            tmpBuildStr = new StringBuilder(("FAB ID:").PadRight(28, ' '), 28);
            Header[19] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("Active Flow:").PadRight(28, ' '), 28);
            Header[20] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.PackageType;
            tmpBuildStr = new StringBuilder(("Email Address:").PadRight(28, ' '), 28);
            Header[21] = tmpBuildStr.ToString() + ResultInfo.TestHeader.ConditionName.EMAIL_ADDRESS;
            tmpBuildStr = new StringBuilder(("Translator:").PadRight(28, ' '), 28);
            Header[22] = tmpBuildStr.ToString() + ResultInfo.TestHeader.ConditionName.Translator;
            tmpBuildStr = new StringBuilder(("Facility:").PadRight(28, ' '), 28);
            Header[23] = tmpBuildStr.ToString() + ResultInfo.TestHeader.ConditionName.Facility;
            tmpBuildStr = new StringBuilder(("Host IP Address:").PadRight(28, ' '), 28);
            Header[24] = tmpBuildStr.ToString() + ResultInfo.TestHeader.ConditionName.HostIpAddress;
            tmpBuildStr = new StringBuilder(("Temperature:").PadRight(28, ' '), 28);
            Header[25] = tmpBuildStr.ToString() + ResultInfo.TestHeader.ConditionName.Temperature;
            tmpBuildStr = new StringBuilder(("PcbLot:").PadRight(28, ' '), 28);
            Header[26] = tmpBuildStr.ToString() + ResultInfo.TestHeader.Misc_Details.PcbLot;
            tmpBuildStr = new StringBuilder(("AssemblyLot:").PadRight(28, ' '), 28);
            Header[27] = tmpBuildStr.ToString() + ResultInfo.TestHeader.Misc_Details.AssemblyLot;
            tmpBuildStr = new StringBuilder(("VerificationUnit:").PadRight(28, ' '), 28);
            Header[28] = tmpBuildStr.ToString() + ResultInfo.TestHeader.Misc_Details.VerificationUnit;

            tmpBuildStr = new StringBuilder(("Site Description:").PadRight(28, ' '), 28);
            Header[30] = tmpBuildStr.ToString();
            tmpBuildStr = new StringBuilder(("  Site Count:").PadRight(28, ' '), 28);
            Header[31] = tmpBuildStr.ToString() + CEmpty2Str(ResultInfo.TestHeader.SiteDetails.Testing_sites);
            tmpBuildStr = new StringBuilder(("  Site Numbers:").PadRight(28, ' '), 28);
            Header[32] = tmpBuildStr.ToString() + CEmpty2Str(ResultInfo.TestHeader.SiteDetails.Testing_sites);
            tmpBuildStr = new StringBuilder(("  Prober/Handler ID:").PadRight(28, ' '), 28);
            Header[33] = tmpBuildStr.ToString() + ResultInfo.TestHeader.SiteDetails.Handler_ID;
            tmpBuildStr = new StringBuilder(("  Prober/Handler Type:").PadRight(28, ' '), 28);
            Header[34] = tmpBuildStr.ToString() + ResultInfo.TestHeader.SiteDetails.Handler_type;
            tmpBuildStr = new StringBuilder(("  Active Adapter Board:").PadRight(28, ' '), 28);
            Header[35] = tmpBuildStr.ToString() + ResultInfo.TestHeader.SiteDetails.LoadBoardName;
            
            tmpBuildStr = new StringBuilder(("Wafer Configuration:").PadRight(28, ' '), 28);
            Header[37] = tmpBuildStr.ToString();
            tmpBuildStr = new StringBuilder(("  Wafer Diameter").PadRight(28, ' '), 28);
            Header[38] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("  Die Height:").PadRight(28, ' '), 28);
            Header[39] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("  Die Width:").PadRight(28, ' '), 28);
            Header[40] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("  Wafer Units:").PadRight(28, ' '), 28);
            Header[41] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("  Wafer Flat Orientation:").PadRight(28, ' '), 28);
            Header[42] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.WaferOrientation;
            tmpBuildStr = new StringBuilder(("  XCoord-Center:").PadRight(28, ' '), 28);
            Header[43] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("  YCoord-Center:").PadRight(28, ' '), 28);
            Header[44] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("  PositiveXDir:").PadRight(28, ' '), 28);
            Header[45] = tmpBuildStr.ToString() + "NA";                             // Info?
            tmpBuildStr = new StringBuilder(("  PositiveYDir:").PadRight(28, ' '), 28);
            Header[46] = tmpBuildStr.ToString() + "NA";                             // Info?

            tmpBuildStr = new StringBuilder(("Wafer Information:").PadRight(28, ' '), 28);
            Header[48] = tmpBuildStr.ToString();
            tmpBuildStr = new StringBuilder(("  Wafer ID:").PadRight(28, ' '), 28);
            Header[49] = tmpBuildStr.ToString() + ResultInfo.TestHeader.GlobalInfo.Wafer;

            return Header;
        }
        private string Generate_SummaryHWBin(int[,] HWbin, cTestResultsReader.s_Results ResultInfo, float TotalUnit)
        {
            StringBuilder HWBinSummary = new StringBuilder();
            StringBuilder tmpStrBuilder;
            List<int> SWBinNumber = new List<int>();
            string tmpStr;
            for (int iSite = 0; iSite < HWbin.GetLength(0); iSite++)
            {
                HWBinSummary.AppendFormat("Hardware Bin Summary for {0}, Site{1}:\r\n", ResultInfo.TestHeader.SiteDetails.HeadNumber, (iSite + 1));
                HWBinSummary.Append("  HW Bin No.    P/F     Device Count    Percent     Hardware Bin Name\r\n");
                HWBinSummary.Append(" -----------    ---     ------------    -------     -----------------\r\n");
                for (int iBin = 0; iBin < HWbin.GetLength(1); iBin++)
                {
                    if(HWbin[iSite,iBin] != 0)
                    {
                        if (!SWBinNumber.Contains(HWbin[iSite, iBin]))
                        {
                            SWBinNumber.Add(HWbin[iSite, iBin]);
                            HWBinSummary.Append("     ");
                            tmpStrBuilder = new StringBuilder((New_Test_Spec.HW_Bin[iBin].Bin_Number.ToString().PadRight(12, ' ')), 12);
                            HWBinSummary.Append(tmpStrBuilder.ToString());
                            tmpStrBuilder = new StringBuilder((New_Test_Spec.HW_Bin[iBin].BinState.Substring(0, 1).PadRight(9, ' ')), 9);
                            HWBinSummary.Append(tmpStrBuilder.ToString());

                            tmpStrBuilder = new StringBuilder(HWbin[iSite, iBin].ToString().PadRight(15, ' '), 15);
                            HWBinSummary.Append(tmpStrBuilder.ToString());
                            tmpStrBuilder = new StringBuilder();
                            tmpStrBuilder.AppendFormat("{0:0.##}", (HWbin[iSite, iBin] / TotalUnit * 100f));
                            tmpStr = tmpStrBuilder.ToString();
                            tmpStrBuilder = new StringBuilder(tmpStr.PadRight(10, ' '), 10);
                            HWBinSummary.Append(tmpStrBuilder);
                            HWBinSummary.AppendFormat("{0}\r\n", New_Test_Spec.HW_Bin[iBin].Name);
                        }
                    }
                }
                HWBinSummary.AppendFormat("\r\n");
            }
            HWBinSummary.AppendFormat("\r\n");
            return (HWBinSummary.ToString());
        }
        private string Generate_SummaryTestResult(int[] FailParam, float TotalUnit)
        {
            int TopFailure = 20;
            IComparer myComparer = new myReverserClass();

            StringBuilder tmpStrBuilder;
            StringBuilder tmpParam = new StringBuilder();
            StringBuilder TestParam = new StringBuilder("Test Result Summary:\r\n");
            string[] ParamInfo = new string[New_Test_Spec.SerialInfo.Length];
            //int[] ParamKey = new int[New_Test_Spec.SerialInfo.Length];

            string tmpStr;
            TestParam.Append("  Test No.   Type   Executions    Failures  % Passed   Test Name\r\n");
            TestParam.Append("  --------   ----   ----------    --------  --------   ---------\r\n");
            for (int iParam = 0; iParam < New_Test_Spec.SerialInfo.Length; iParam++)
            {
                //ParamKey[iParam] = iParam;
                tmpParam= new StringBuilder("  ");
                tmpStrBuilder = new StringBuilder((iParam + 1).ToString().PadRight(12, ' '), 12);
                tmpParam.Append(tmpStrBuilder);
                tmpStrBuilder = new StringBuilder(("P").ToString().PadRight(7, ' '), 7);
                tmpParam.Append(tmpStrBuilder);
                tmpStrBuilder = new StringBuilder((TotalUnit).ToString().PadRight(13, ' '), 13);
                tmpParam.Append(tmpStrBuilder);
                tmpStrBuilder = new StringBuilder((FailParam[iParam]).ToString().PadRight(12, ' '), 12);
                tmpParam.Append(tmpStrBuilder);
                tmpStrBuilder = new StringBuilder();
                tmpStrBuilder.AppendFormat("{0:000.00}", (TotalUnit - FailParam[iParam]) / TotalUnit * 100f);
                tmpStr = tmpStrBuilder.ToString();
                tmpStrBuilder = new StringBuilder(tmpStr.PadRight(10, ' '), 10);
                tmpParam.Append(tmpStrBuilder);
                tmpParam.AppendFormat("{0}//\r\n", New_Test_Spec.SerialInfo[iParam].TestParameters);
                ParamInfo[iParam] = tmpParam.ToString();
                TestParam.Append(tmpParam);
            }

            TestParam.Append("\r\n");
            TestParam.Append("\r\n");

            Array.Sort(FailParam, ParamInfo, myComparer);

            if (ParamInfo.Length < TopFailure)
            {
                TopFailure = ParamInfo.Length;
            }

            TestParam.Append("Top " + TopFailure.ToString() + " failure mode:\r\n");
            TestParam.Append("  Test No.   Type   Executions    Failures  % Passed   Test Name\r\n");
            TestParam.Append("  --------   ----   ----------    --------  --------   ---------\r\n");

            for (int iTparam = 0; iTparam < TopFailure; iTparam++)
            {
                TestParam.AppendFormat("{0}", ParamInfo[iTparam]);
            }

            return TestParam.ToString();
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
        #endregion
        private void btnSDIRegen_Click(object sender, EventArgs e)
        {
            /*btnSDIRegen.Enabled = false;
            StatusStripLabel.Text = "";
            if (New_Test_Spec.SerialBin == null)
            {
                MessageBox.Show("Missing New Specification / Limit File Data");
                if (txtTestCondition.Text.Trim() == "")
                {
                    txtTestCondition.BackColor = Color.Pink;
                    btnTestCondition.BackColor = Color.Pink;
                }
                txtNewSpec.BackColor = Color.Pink;
                btnNewSpec.BackColor = Color.Pink;
                btnNewSpec.Focus();
                btnSDIRegen.Enabled = true;
                return;
            }
            if (txtTestCondition.Text.Trim() == "")
            {
                MessageBox.Show("Missing Test Condition File");
                txtTestCondition.BackColor = Color.Pink;
                btnTestCondition.BackColor = Color.Pink;
                btnTestCondition.Focus();
                btnSDIRegen.Enabled = true;
                return;
            }
            FolderBrowserDialog Dialog = new FolderBrowserDialog();
            Dialog.ShowDialog();
            string FolderPath = Dialog.SelectedPath;

            if (FolderPath.Trim() == "")
            {
                btnSDIRegen.Enabled = true;
                return;
            }

            string[] Files = System.IO.Directory.GetFiles(FolderPath, "*.s*");

            bool b_LooseUnit = false;
            //string stry = "X1_Y33_t1_c1";
            string SDI_FilePattern_Loose = "U\\d*_t\\d*_C\\d*";
            string SDI_UnitPattern = "U\\d*";
            string SDI_FilePattern = "X\\d*_Y\\d*_t\\d*_C\\d*";
            string SDI_LocationPattern = "X\\d*_Y\\d*";

            int[] Triggering = new int[1];      // parse triggering
            int iTrigger = 1;
            int tmpTrig;
            int iChn;
            int iMaxChn = 0;
            int Progress;
            int TotalTest = 0;

            string[] tmpStr;
            string OutputRsltFile = "";
            string[] FileExt = new string[1];

            List<string> SDI_List = new List<string>();     //SDI list location to parse
            List<double>[] FreqList;

            Regex rx = new Regex(SDI_FilePattern, RegexOptions.IgnoreCase);
            Regex rx_Loose = new Regex(SDI_FilePattern_Loose, RegexOptions.IgnoreCase);
            Regex rx_loc = new Regex(SDI_LocationPattern, RegexOptions.IgnoreCase);
            Regex rx_Unit = new Regex(SDI_UnitPattern, RegexOptions.IgnoreCase);

            MatchCollection match;
            MatchCollection match_loc;

            if (Files.Length > 0)
            {
                if (txtOutput.Text == "")
                {
                    SaveFileDialog SDialog = new SaveFileDialog();
                    SDialog.Filter = "Result File (*.csv)|*.csv";
                    //SDialog.InitialDirectory = FolderName(txtResult.Text);
                    SDialog.AddExtension = true;
                    //SDialog.FileName = Append_Filename(SafeFileName(txtResult.Text), "Regen");
                    if (SDialog.ShowDialog() == DialogResult.OK)
                    {
                        lblChkOutput.Visible = false;
                        txtOutput.Text = SDialog.FileName;
                        OutputRsltFile = txtOutput.Text;
                    }
                }
                if (txtOutput.Text != "")
                {
                    StatusStripLabel.Text = "Initializing SDI Data";
                    Application.DoEvents();
                    //Check if loose unit SDI or Strip/Wafer Location
                    match = rx_Loose.Matches(Files[0]);
                    if (match.Count == 1)
                    {
                        b_LooseUnit = true;
                    }
                    foreach (string S_FileName in Files)
                    {
                        if (b_LooseUnit)
                        {
                            match = rx_Loose.Matches(S_FileName);
                            if (match.Count == 1)
                            {
                                match_loc = rx_Unit.Matches(S_FileName);
                                if (!SDI_List.Contains(match_loc[0].ToString()))
                                {
                                    SDI_List.Add(match_loc[0].ToString());
                                    TotalTest++;
                                }
                                tmpStr = ExtractFileName(S_FileName).Split('_');
                                tmpTrig = int.Parse(tmpStr[2].Substring(1, tmpStr[2].Length - 1));
                                if (tmpTrig > iTrigger)
                                {
                                    iTrigger = tmpTrig;
                                    Array.Resize(ref Triggering, iTrigger);
                                }
                                if (int.Parse(tmpStr[3].Substring(1, tmpStr[3].Length - 1)) > Triggering[tmpTrig - 1])
                                {
                                    iChn = int.Parse(tmpStr[3].Substring(1, tmpStr[3].Length - 1));
                                    Triggering[tmpTrig - 1] = iChn;
                                    if (iChn > iMaxChn)
                                    {
                                        iMaxChn = iChn;
                                        if (iMaxChn > 1)
                                        {
                                            Array.Resize(ref FileExt, iMaxChn);
                                        }
                                        FileExt[iChn - 1] = ExtractExtFile(S_FileName);
                                    }
                                }
                            }
                        }
                        else
                        {
                            match = rx.Matches(S_FileName);
                            if (match.Count == 1)
                            {
                                match_loc = rx_loc.Matches(S_FileName);
                                if (!SDI_List.Contains(match_loc[0].ToString()))
                                {
                                    SDI_List.Add(match_loc[0].ToString());
                                    TotalTest++;
                                }
                                tmpStr = ExtractFileName(S_FileName).Split('_');
                                tmpTrig = int.Parse(tmpStr[2].Substring(1, tmpStr[2].Length - 1));
                                if (tmpTrig > iTrigger)
                                {
                                    iTrigger = tmpTrig;
                                    Array.Resize(ref Triggering, iTrigger);
                                }
                                if (int.Parse(tmpStr[3].Substring(1, tmpStr[3].Length - 1)) > Triggering[tmpTrig - 1])
                                {
                                    iChn = int.Parse(tmpStr[3].Substring(1, tmpStr[3].Length - 1));
                                    Triggering[tmpTrig - 1] = iChn;
                                    if (iChn > iMaxChn)
                                    {
                                        iMaxChn = iChn;
                                        if (iMaxChn > 1)
                                        {
                                            Array.Resize(ref FileExt, iMaxChn);
                                        }
                                        FileExt[iChn - 1] = ExtractExtFile(S_FileName);
                                    }
                                }
                            }
                        }
                    }

                    #region "Get Frequency List"
                    string tmpFreqList_FileName = SDI_List[0];
                    string[] tmpSDI_FreqData;
                    string[] tmpStrArr;
                    FreqList = new List<double>[iMaxChn];
                    for (int iChannel = 0; iChannel < iMaxChn; iChannel++)
                    {
                        tmpSDI_FreqData = System.IO.File.ReadAllLines(FolderPath + "\\" + tmpFreqList_FileName + "_t1_C" + (iChannel + 1).ToString() + FileExt[iChannel]);
                        for (int iFreqList = 0; iFreqList < tmpSDI_FreqData.Length; iFreqList++)
                        {
                            tmpStrArr = tmpSDI_FreqData[iFreqList].Split('\t');
                            if (iFreqList == 0) FreqList[iChannel] = new List<double>();
                            if (convertStr2Double(tmpStrArr[0]) != 0)
                            {
                                FreqList[iChannel].Add(convertStr2Double(tmpStrArr[0]));
                            }
                        }
                    }
                    #endregion
                    #region "Renerate ResultHeader"
                    int SW_SubBin;
                    Regen_ResultFile.TestHeader.GlobalInfo.Date = DateTime.Now.ToString("yyyy_MM_dd HH:mm:ss");
                    Regen_ResultFile.TestHeader.GlobalInfo.SetupTime = DateTime.Now.ToString("yyyy_MM_dd HH:mm:ss");
                    Regen_ResultFile.TestHeader.GlobalInfo.StartTime = DateTime.Now.ToString("yyyy_MM_dd HH:mm:ss");
                    Regen_ResultFile.TestHeader.GlobalInfo.FinishTime = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.ProgramName = "";        // **
                    Regen_ResultFile.TestHeader.GlobalInfo.ProgramRevision = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.Lot = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.SubLot = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.Wafer = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.WaferOrientation = "NA";
                    Regen_ResultFile.TestHeader.GlobalInfo.TesterName = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.TesterType = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.Product = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.Operator = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.ExecType = "Regen Function";
                    Regen_ResultFile.TestHeader.GlobalInfo.ExecRevision = "v1.0.0";
                    Regen_ResultFile.TestHeader.GlobalInfo.RtstCode = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.PackageType = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.Family = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.SpecName = New_Test_Spec.Header.Title;
                    Regen_ResultFile.TestHeader.GlobalInfo.SpecVersion = New_Test_Spec.Header.Version;
                    Regen_ResultFile.TestHeader.GlobalInfo.FlowID = "";
                    Regen_ResultFile.TestHeader.GlobalInfo.DesignRevision = "";

                    Regen_ResultFile.TestHeader.SiteDetails.HeadNumber = "Head #1";
                    Regen_ResultFile.TestHeader.SiteDetails.Testing_sites = "1";
                    Regen_ResultFile.TestHeader.SiteDetails.Handler_ID = "";
                    Regen_ResultFile.TestHeader.SiteDetails.Handler_type = "";
                    Regen_ResultFile.TestHeader.SiteDetails.LoadBoardName = "";

                    Regen_ResultFile.TestHeader.Options.UnitsMode = "Normalized";

                    Regen_ResultFile.TestHeader.ConditionName.ConditionName = "";
                    Regen_ResultFile.TestHeader.ConditionName.EMAIL_ADDRESS = "";
                    Regen_ResultFile.TestHeader.ConditionName.Translator = "";
                    Regen_ResultFile.TestHeader.ConditionName.Wafer_Diameter = "";
                    Regen_ResultFile.TestHeader.ConditionName.Facility = "";
                    Regen_ResultFile.TestHeader.ConditionName.HostIpAddress = "";
                    Regen_ResultFile.TestHeader.ConditionName.Temperature = "";

                    Regen_ResultFile.TestHeader.Correlation_FileName = "";

                    Regen_ResultFile.ResultHeader.TestNumber = new int[New_Test_Spec.SerialInfo.Length];
                    Regen_ResultFile.ResultHeader.TestParameter_Name = new string[New_Test_Spec.SerialInfo.Length];
                    Regen_ResultFile.ResultHeader.HighL = new string[New_Test_Spec.SerialInfo.Length];
                    Regen_ResultFile.ResultHeader.LowL = new string[New_Test_Spec.SerialInfo.Length];
                    Regen_ResultFile.ResultHeader.Patterns = new string[New_Test_Spec.SerialInfo.Length];
                    Regen_ResultFile.ResultHeader.Units = new string[New_Test_Spec.SerialInfo.Length];

                    Regen_ResultFile.ResultData = new cTestResultsReader.s_ResultsData[SDI_List.Count];

                    for (int iTest = 0; iTest < New_Test_Spec.SerialInfo.Length; iTest++)
                    {
                        Regen_ResultFile.ResultHeader.TestNumber[iTest] = int.Parse(New_Test_Spec.SerialInfo[iTest].TestNumber);
                        Regen_ResultFile.ResultHeader.TestParameter_Name[iTest] = New_Test_Spec.SerialInfo[iTest].TestParameters;
                        Regen_ResultFile.ResultHeader.Patterns[iTest] = "";
                        Regen_ResultFile.ResultHeader.Units[iTest] = "";
                        for (int iSw = 0; iSw < New_Test_Spec.SW_Bin.Length; iSw++)
                        {
                            if (New_Test_Spec.SW_Bin[iSw].PassBin)
                            {
                                for (int iSw_sub = 0; iSw_sub < New_Test_Spec.SW_Bin[iSw].Bin.Length; iSw_sub++)
                                {
                                    SW_SubBin = New_Test_Spec.SW_Bin[iSw].Bin[iSw_sub];
                                    if (New_Test_Spec.SerialBin[SW_SubBin].Max[iTest].Max_None)
                                    {
                                        if (Convert.ToSingle(Regen_ResultFile.ResultHeader.HighL[iTest]) < Convert.ToSingle("3.402823E+38"))
                                        {
                                            Regen_ResultFile.ResultHeader.HighL[iTest] = "3.402823E+38";
                                        }
                                    }
                                    else
                                    {
                                        if (Convert.ToSingle(Regen_ResultFile.ResultHeader.HighL[iTest]) < (New_Test_Spec.SerialBin[SW_SubBin].Max[iTest].Max))
                                        {
                                            Regen_ResultFile.ResultHeader.HighL[iTest] = New_Test_Spec.SerialBin[SW_SubBin].Max[iTest].Max.ToString();
                                        }
                                    }
                                    if (New_Test_Spec.SerialBin[SW_SubBin].Min[iTest].Min_None)
                                    {
                                        if (Convert.ToSingle(Regen_ResultFile.ResultHeader.LowL[iTest]) > Convert.ToSingle("-3.402823E+38"))
                                        {
                                            Regen_ResultFile.ResultHeader.LowL[iTest] = "-3.402823E+38";
                                        }
                                    }
                                    else
                                    {
                                        if (Convert.ToSingle(Regen_ResultFile.ResultHeader.LowL[iTest]) < (New_Test_Spec.SerialBin[SW_SubBin].Min[iTest].Min))
                                        {
                                            Regen_ResultFile.ResultHeader.LowL[iTest] = New_Test_Spec.SerialBin[SW_SubBin].Min[iTest].Min.ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // drivers
                    int X = 0;
                    int Y = 0;
                    int DataPoints = 0;
                    string regen_file;
                    StringBuilder RsltHeader = new StringBuilder();
                    StringBuilder RsltData = new StringBuilder();

                    // Parsing in the Test Condition (Optional)
                    Drivers.TCF_FileName = txtTestCondition.Text;
                    //// Setting the SNP (SDI) files output for S_Parameters
                    Drivers.SNPFile.FileOutput_Enable = false;
                    Drivers.set_RegenFlag = true;
                    Drivers.FBAR.Regen_FreqList = FreqList;
                    //// Initialize the Drivers for FBAR from cAlgorithm
                    Drivers.Initialization();

                    ProgressBar.Value = 0;
                    ProgressBar.Visible = true;
                    foreach (string list in SDI_List)
                    {
                        if (b_LooseUnit)
                        {
                            X = 0;
                            Y = 0;
                        }
                        else
                        {
                            tmpStr = list.Split('_');
                            X = Convert.ToInt32(tmpStr[0].Substring(1, tmpStr[0].Length - 1));
                            Y = Convert.ToInt32(tmpStr[1].Substring(1, tmpStr[1].Length - 1));
                        }

                        RsltData = new StringBuilder("");

                        regen_file = FolderPath + "\\" + list + "_";
                        Drivers.parse_RegenPath = regen_file;
                        Drivers.parse_RegenFileExt = FileExt;
                        Drivers.Run_Tests();
                        Drivers.Get_Results();

                        if (DataPoints == 2)
                        {
                            for (int Rslt = 0; Rslt < Drivers.Results.Length; Rslt++)
                            {

                                if (Drivers.Results[Rslt].Enable)
                                {
                                    if (Drivers.Results[Rslt].b_MultiResult)
                                    {
                                        for (int iRslt = 0; iRslt < Drivers.Results[Rslt].Multi_Results.Length; iRslt++)
                                        {
                                            if (Drivers.Results[Rslt].Multi_Results[iRslt].Enable)
                                            {
                                                RsltHeader.AppendFormat("{0};", Drivers.Results[Rslt].Result_Header + "_"
                                                    + Drivers.Results[Rslt].Multi_Results[iRslt].Result_Header);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        RsltHeader.AppendFormat("{0};", Drivers.Results[Rslt].Result_Header);
                                    }
                                }
                            }
                        }
                        for (int Rslt = 0; Rslt < Drivers.Results.Length; Rslt++)
                        {

                            if (Drivers.Results[Rslt].Enable)
                            {
                                if (Drivers.Results[Rslt].b_MultiResult)
                                {
                                    for (int iRslt = 0; iRslt < Drivers.Results[Rslt].Multi_Results.Length; iRslt++)
                                    {
                                        if (Drivers.Results[Rslt].Multi_Results[iRslt].Enable)
                                        {
                                            RsltData.AppendFormat("{0};", Drivers.Results[Rslt].Multi_Results[iRslt].Result_Data);
                                        }
                                    }
                                }
                                else
                                {
                                    RsltData.AppendFormat("{0};", Drivers.Results[Rslt].Result_Data);
                                }
                            }
                        }
                        Populate_ResultTestData(ref Regen_ResultFile, DataPoints, X, Y, RsltData.ToString().Trim(';'));
                        DataPoints++;
                        Application.DoEvents();
                        Progress = (int)((double)DataPoints / (double)TotalTest * 100f);
                        ProgressBar.Value = Progress;
                        StatusStripLabel.Text = "Processing " + DataPoints.ToString() + " of " + SDI_List.Count.ToString() + "Files.";
                    }

                    if (Check_ResultTestHeader(New_Test_Spec, RsltHeader.ToString().Trim(';')))
                    {

                        System.IO.File.WriteAllText(OutputRsltFile, Generate_ResultHeader(Regen_ResultFile));

                        Regen_ResultsFileNSummary(New_Test_Spec, Regen_ResultFile, OutputRsltFile, true);
                    }
                    #endregion

                    Drivers.Unload();
                    Drivers = null;
                    ProgressBar.Visible = false;
                    StatusStripLabel.Text = "SDi Regen Completed!!";
                }
            }
            btnSDIRegen.Enabled = true;*/
        }
        private string ExtractFileName(string FileName)
        {
            string[] tmpStr = FileName.Split('\\');
            string tmpExt = ExtractExtFile(tmpStr[tmpStr.Length - 1]);
            return tmpStr[tmpStr.Length - 1].Substring(0, tmpStr[tmpStr.Length - 1].Length - tmpExt.Length);
        }
        private string ExtractExtFile(string FileName)
        {
            string[] tmpStr = FileName.Split('.');
            return ("." + tmpStr[tmpStr.Length - 1]);
        }
        private string ExtractFilePath(string FileName)
        {
            string[] Data = FileName.Split('\\');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Data.Length - 1; i++)
            {
                sb.AppendFormat("{0}\\", Data[i]);
            }
            return sb.ToString();
        }
        private string Generate_ResultHeader(cTestResultsReader.s_Results Rslt)
        {
            StringBuilder[] tmpStrBuilder = new StringBuilder[7];
            StringBuilder ResultHeader = new StringBuilder("--- Global Info:,\r\n");
            ResultHeader.AppendFormat("Date,{0}\r\n", Rslt.TestHeader.GlobalInfo.Date);
            ResultHeader.AppendFormat("SetupTime,{0}\r\n", Rslt.TestHeader.GlobalInfo.SetupTime);
            ResultHeader.AppendFormat("StartTime,{0}\r\n", Rslt.TestHeader.GlobalInfo.StartTime);
            ResultHeader.AppendFormat("FinishTime,{0}\r\n", Rslt.TestHeader.GlobalInfo.FinishTime);
            ResultHeader.AppendFormat("ProgramName,{0}\r\n", Rslt.TestHeader.GlobalInfo.ProgramName);
            ResultHeader.AppendFormat("ProgramRevision,{0}\r\n", Rslt.TestHeader.GlobalInfo.ProgramRevision);
            ResultHeader.AppendFormat("Lot,{0}\r\n", Rslt.TestHeader.GlobalInfo.Lot);
            ResultHeader.AppendFormat("SubLot,{0}\r\n", Rslt.TestHeader.GlobalInfo.SubLot);
            ResultHeader.AppendFormat("Wafer,{0}\r\n", Rslt.TestHeader.GlobalInfo.Wafer);
            ResultHeader.AppendFormat("WaferOrientation,{0}\r\n", Rslt.TestHeader.GlobalInfo.WaferOrientation);
            ResultHeader.AppendFormat("TesterName,{0}\r\n", Rslt.TestHeader.GlobalInfo.TesterName);
            ResultHeader.AppendFormat("TesterType,{0}\r\n", Rslt.TestHeader.GlobalInfo.TesterType);
            ResultHeader.AppendFormat("Product,{0}\r\n", Rslt.TestHeader.GlobalInfo.Product);
            ResultHeader.AppendFormat("Operator,{0}\r\n", Rslt.TestHeader.GlobalInfo.Operator);
            ResultHeader.AppendFormat("ExecType,{0}\r\n", Rslt.TestHeader.GlobalInfo.ExecType);
            ResultHeader.AppendFormat("ExecRevision,{0}\r\n", Rslt.TestHeader.GlobalInfo.ExecRevision);
            ResultHeader.AppendFormat("RtstCode,{0}\r\n", Rslt.TestHeader.GlobalInfo.RtstCode);
            ResultHeader.AppendFormat("PackageType,{0}\r\n", Rslt.TestHeader.GlobalInfo.PackageType);
            ResultHeader.AppendFormat("Family,{0}\r\n", Rslt.TestHeader.GlobalInfo.Family);
            ResultHeader.AppendFormat("SpecName,{0}\r\n", Rslt.TestHeader.GlobalInfo.SpecName);
            ResultHeader.AppendFormat("SpecVersion,{0}\r\n", Rslt.TestHeader.GlobalInfo.SpecVersion);
            ResultHeader.AppendFormat("FlowID,{0}\r\n", Rslt.TestHeader.GlobalInfo.FlowID);
            ResultHeader.AppendFormat("DesignRevision,{0}\r\n", Rslt.TestHeader.GlobalInfo.DesignRevision);

            ResultHeader.AppendFormat("--- Site details:,{0}\r\n", Rslt.TestHeader.SiteDetails.HeadNumber);
            ResultHeader.AppendFormat("Testing sites,{0}\r\n", Rslt.TestHeader.SiteDetails.Testing_sites);
            ResultHeader.AppendFormat("Handler ID,{0}\r\n", Rslt.TestHeader.SiteDetails.Handler_ID);
            ResultHeader.AppendFormat("Handler type,{0}\r\n", Rslt.TestHeader.SiteDetails.Handler_type);
            ResultHeader.AppendFormat("LoadBoardName,{0}\r\n", Rslt.TestHeader.SiteDetails.LoadBoardName);

            ResultHeader.AppendFormat("--- Options:\r\n");
            ResultHeader.AppendFormat("UnitsMode,{0}\r\n", Rslt.TestHeader.Options.UnitsMode);
            ResultHeader.AppendFormat("--- ConditionName:,{0}\r\n", Rslt.TestHeader.ConditionName.ConditionName);
            ResultHeader.AppendFormat("EMAIL_ADDRESS,{0}\r\n", Rslt.TestHeader.ConditionName.EMAIL_ADDRESS);
            ResultHeader.AppendFormat("Translator,{0}\r\n", Rslt.TestHeader.ConditionName.Translator);
            ResultHeader.AppendFormat("Wafer_Diameter,{0}\r\n", Rslt.TestHeader.ConditionName.Wafer_Diameter);
            ResultHeader.AppendFormat("Facility,{0}\r\n", Rslt.TestHeader.ConditionName.Facility);
            ResultHeader.AppendFormat("HostIpAddress,{0}\r\n", Rslt.TestHeader.ConditionName.HostIpAddress);
            ResultHeader.AppendFormat("Temperature,{0}\r\n", Rslt.TestHeader.ConditionName.Temperature);
            ResultHeader.Append("\r\n");
            //ResultHeader.AppendFormat("#CF,{0}\r\n", Rslt.TestHeader.Correlation_FileName);

            tmpStrBuilder[0] = new StringBuilder("");
            tmpStrBuilder[0].AppendFormat("#CF,{0},,,,,,,,,", Rslt.TestHeader.Correlation_FileName);
            tmpStrBuilder[1] = new StringBuilder("Parameter,SBIN,HBIN,DIE_X,DIE_Y,SITE,TIME,TOTAL_TESTS,LOT_ID,WAFER_ID,");
            tmpStrBuilder[2] = new StringBuilder("Tests#,,,,,,,,,,");
            tmpStrBuilder[3] = new StringBuilder("Patterns,,,,,,,,,,");
            tmpStrBuilder[4] = new StringBuilder("Unit,,,,,,Sec,,,,");
            tmpStrBuilder[5] = new StringBuilder("HighL,,,,,,,,,,");
            tmpStrBuilder[6] = new StringBuilder("LowL,,,,,,,,,,");

            for (int iParam = 0; iParam < Rslt.ResultHeader.TestNumber.Length; iParam++)
            {
                tmpStrBuilder[0].AppendFormat("{0},", "");
                tmpStrBuilder[1].AppendFormat("{0},", Rslt.ResultHeader.TestParameter_Name[iParam].ToString());
                tmpStrBuilder[2].AppendFormat("{0},", Rslt.ResultHeader.TestNumber[iParam]);
                tmpStrBuilder[3].AppendFormat("{0},", Rslt.ResultHeader.Patterns[iParam]);
                tmpStrBuilder[4].AppendFormat("{0},", Rslt.ResultHeader.Units[iParam]);
                tmpStrBuilder[5].AppendFormat("{0},", Rslt.ResultHeader.HighL[iParam]);
                tmpStrBuilder[6].AppendFormat("{0},", Rslt.ResultHeader.LowL[iParam]);
            }
            tmpStrBuilder[0].Append(",,,,\r\n");
            tmpStrBuilder[1].Append("PassFail,TimeStamp, IndexTime, PartSN, SWBinName,HWBinName\r\n");
            tmpStrBuilder[2].Append(",,,,\r\n");
            tmpStrBuilder[3].Append(",,,,\r\n");
            tmpStrBuilder[4].Append(",,Sec,,\r\n");
            tmpStrBuilder[5].Append(",,,,\r\n");
            tmpStrBuilder[6].Append(",,,,\r\n");

            for (int iB = 0; iB < tmpStrBuilder.Length; iB++)
            {
                ResultHeader.Append(tmpStrBuilder[iB]);
            }

            return ResultHeader.ToString();
        }
        private bool Check_ResultTestHeader(cTestSpecificationReader.s_SpecFile Spec, string testheader)
        {
            string[] Header = testheader.Split(';');
            if (Header.Length != Spec.SerialInfo.Length)
            {
                MessageBox.Show("Mismatch Total Test Parameters!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            else
            {
                for (int iHead = 0; iHead < Header.Length; iHead++)
                {
                    if (Header[iHead] != Spec.SerialInfo[iHead].TestParameters)
                    {
                        MessageBox.Show("Mismatch Test Parameters Found! \r\n\r\n" +
                                            "Result Test Parameter : " + Header[iHead] + "\r\n" +
                                            "Spec Test Parameters : " + Spec.SerialInfo[iHead].TestParameters, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                return true;
            }
        }
        private void Populate_ResultTestData(ref cTestResultsReader.s_Results Rslt, int item, int X_loc, int Y_loc, string ResultData)
        {
            string[] tmpArr = ResultData.Split(';');
            Rslt.ResultData[item].ID = "PID-" + (item + 1).ToString();
            Rslt.ResultData[item].Data = Array.ConvertAll<string, double>(tmpArr, Convert.ToDouble);
            Rslt.ResultData[item].Die_X = X_loc;
            Rslt.ResultData[item].Die_Y = Y_loc;
            Rslt.ResultData[item].Time = 0;
            Rslt.ResultData[item].Site = "1";
            Rslt.ResultData[item].TotalTest = tmpArr.Length;
            Rslt.ResultData[item].Lot_ID = "";
            Rslt.ResultData[item].Wafer_ID = "";
            Rslt.ResultData[item].TimeStamp = DateTime.Now.ToString("yyyy_MM_dd HH:mm:ss");
            Rslt.ResultData[item].IndexTime = 0;
            Rslt.ResultData[item].PartSN = ""; 
        }

        private void btnTestCondition_Click(object sender, EventArgs e)
        {
            txtTestCondition.BackColor = SystemColors.Window;
            btnTestCondition.BackColor = SystemColors.Control;
            txtTestCondition.Text = "";
            lblChkTestCondition.Visible = false;
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Filter = "Test Condition (*.xlsx)|*.xlsx";

            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                txtTestCondition.Text = Dialog.FileName;
            }
        }
        double convertStr2Double(string input)
        {
            double temp = 0;
            try
            {
                temp = double.Parse(input);
            }
            catch
            {
                temp = 0;
            }
            return temp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //WaferMap.frmWaferMap WMap = new WaferMap.frmWaferMap();
            //WMap.Show();
            cWaferMap.frmWaferMap WMap = new cWaferMap.frmWaferMap();
            WMap.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string Filename = "";
            cInputForm.InputForm InForm = new cInputForm.InputForm();
            InForm.parse_InitFolder = @"C:\Avago.ATF.Common\Results";
            InForm.Set_Title = "Open Existing Report File";
            DialogResult Rslt = InForm.ShowDialog();
            if (Rslt == DialogResult.OK)
            {
                Filename = InForm.parse_FileName;
                InForm.Close();
            }
            else
            {
                InForm.Close();
            }
            MessageBox.Show(Filename);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (New_Test_Spec.SerialBin == null)
            {

                MessageBox.Show("Missing New Specification / Limit File Data");
                if (ResultFile.RawData == null)
                {
                    txtResult.BackColor = Color.Pink;
                    btnResult.BackColor = Color.Pink;
                }
                txtNewSpec.BackColor = Color.Pink;
                btnNewSpec.BackColor = Color.Pink;
                btnNewSpec.Focus();
                btnRegen.Enabled = true;
                return;
            }
            if (ResultFile.RawData == null)
            {
                MessageBox.Show("Missing Results File Data");
                txtResult.BackColor = Color.Pink;
                btnResult.BackColor = Color.Pink;
                btnResult.Focus();
                btnRegen.Enabled = true;
                return;
            }
            

            if (txtOutput.Text == "")
            {
                SaveFileDialog Dialog = new SaveFileDialog();
                Dialog.Filter = "Result File (*.csv)|*.csv";
                Dialog.InitialDirectory = FolderName(txtResult.Text);
                Dialog.AddExtension = true;
                Dialog.FileName = Append_Filename(SafeFileName(txtResult.Text), "Regen");
                if (Dialog.ShowDialog() == DialogResult.OK)
                {
                    lblChkOutput.Visible = false;
                    txtOutput.Text = Dialog.FileName;
                }
            }
            
            bool[,] DataPresent = new bool[5000,5000];
            int iData = 0;
            cTestResultsReader.s_Results MergeResult = new cTestResultsReader.s_Results();

            for(int x = 0; x < 5000; x++)
            {
                for(int y=0; y < 5000; y++)
                {
                    if(ResultFile.XY_Info.Match_Position[x,y] != -1)
                    {
                        if(!DataPresent[x,y])
                        {
                            DataPresent[x,y] = true;
                            iData++;
                        }
                    }
                }
            }
            if (Previous_ResultFile.ResultData != null)
            {
                for (int x = 0; x < 5000; x++)
                {
                    for (int y = 0; y < 5000; y++)
                    {
                        if (Previous_ResultFile.XY_Info.Match_Position[x, y] != -1)
                        {
                            if (!DataPresent[x, y])
                            {
                                DataPresent[x, y] = true;
                                iData++;
                            }
                        }
                    }
                }
            }

            MergeResult.ResultData = new cTestResultsReader.s_ResultsData[iData];
            MergeResult.XY_Info.Match_Position = new int[5000, 5000];

            if (Previous_ResultFile.ResultData != null)
            {
                MergeResult.XY_Info.XY_MinMax.Max_X = ParseMaxValue(ResultFile.XY_Info.XY_MinMax.Max_X, Previous_ResultFile.XY_Info.XY_MinMax.Max_X);
                MergeResult.XY_Info.XY_MinMax.Max_Y = ParseMaxValue(ResultFile.XY_Info.XY_MinMax.Max_Y, Previous_ResultFile.XY_Info.XY_MinMax.Max_Y);
                MergeResult.XY_Info.XY_MinMax.Min_X = ParseMinValue(ResultFile.XY_Info.XY_MinMax.Min_X, Previous_ResultFile.XY_Info.XY_MinMax.Min_X);
                MergeResult.XY_Info.XY_MinMax.Min_Y = ParseMinValue(ResultFile.XY_Info.XY_MinMax.Min_Y, Previous_ResultFile.XY_Info.XY_MinMax.Min_Y);
                MergeResult.ResultHeader = Previous_ResultFile.ResultHeader;
                MergeResult.TestHeader = Previous_ResultFile.TestHeader;
            }
            else
            {
                MergeResult.ResultHeader = ResultFile.ResultHeader;
                MergeResult.TestHeader = ResultFile.TestHeader;
                MergeResult.XY_Info.XY_MinMax = ResultFile.XY_Info.XY_MinMax;
            }
            MergeResult.XY_Info.Valid = true;


            for (int x = 0; x < 5000; x++)
            {
                for (int y = 0; y < 5000; y++)
                {
                    MergeResult.XY_Info.Match_Position[x, y] = -1;
                }
            }


            int iDataPos = 0;
            int TmpX =0;
            int TmpY = 0;
            for (int X = MergeResult.XY_Info.XY_MinMax.Min_X; X <= MergeResult.XY_Info.XY_MinMax.Max_X; X++)
            {
                for (int Y = MergeResult.XY_Info.XY_MinMax.Min_Y; Y <= MergeResult.XY_Info.XY_MinMax.Max_Y; Y++)
                {
                    TmpX = Truncate_XY(X);
                    TmpY = Truncate_XY(Y);
                    
                    if (DataPresent[TmpX, TmpY])
                    {
                        if (Previous_ResultFile.ResultData != null)
                        {

                            if (Previous_ResultFile.XY_Info.Match_Position[TmpX, TmpY] != -1)
                            {
                                MergeResult.ResultData[iDataPos] = Previous_ResultFile.ResultData[Previous_ResultFile.XY_Info.Match_Position[TmpX, TmpY]];
                                MergeResult.XY_Info.Match_Position[TmpX, TmpY] = iDataPos;
                                iDataPos++;
                            }
                            else if (ResultFile.XY_Info.Match_Position[TmpX, TmpY] != -1)
                            {
                                MergeResult.ResultData[iDataPos] = ResultFile.ResultData[ResultFile.XY_Info.Match_Position[TmpX, TmpY]];
                                MergeResult.XY_Info.Match_Position[TmpX, TmpY] = iDataPos;
                                iDataPos++;
                            }
                        }
                        else
                        {
                            MergeResult.ResultData[iDataPos] = ResultFile.ResultData[ResultFile.XY_Info.Match_Position[TmpX, TmpY]];
                            MergeResult.XY_Info.Match_Position[TmpX, TmpY] = iDataPos;
                            iDataPos++;
                        }
                    }
                }
            }


            Regen_ResultsFileNSummary(New_Test_Spec, MergeResult, txtOutput.Text, false);


        }

        private int ParseMaxValue(int Value1, int Value2)
        {
            if (Value1 > Value2)
            {
                return Value1;
            }
            else
            {
                return Value2;
            }
        }
        private int ParseMinValue(int Value1, int Value2)
        {
            if (Value1 < Value2)
            {
                return Value1;
            }
            else
            {
                return Value2;
            }
        }
        private int Truncate_XY(int Pos)
        {
            if (Pos < 0)
            {
                return (5000 + Pos);
            }
            return (Pos);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //System.IO.DirectoryInfo DI = new System.IO.DirectoryInfo("C:\\lsc_env");
            //for (int i = 0; i < DI.GetFiles().Length; i++)
            //{
            //    MessageBox.Show(DI.GetFiles()[i].ToString());
            //}
            MessageBox.Show(ExtractFilePath("\\\\test\\me"));
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            //Summary.Summary_Filename = @"E:\Inari Wafer Map\Regen-Delta\4DT7-BASE-T1_W1P358-13D2_R017580_TSK3_FULL_20130106_082030_IP192.168.5.121_D168AUG_DONE_SUMMARY.txt";
            Summary.Summary_Filename = @"C:\Avago.ATF.Common\Results.Backup\2JG5_2_2_FULL_2PASS_20130528_151336_IP192.168.4.42_D5SFANJ_DELTA_SUMMARY.TXT";
            Summary.Read_File();
            Summary.Filtered_Qty = 100;
            Summary.Process_Summary_WithFilter("A");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            
        }

        private void frm_Main_Load(object sender, EventArgs e)
        {

        }
    }

    public class myReverserClass : IComparer
    {

        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        int IComparer.Compare(object x, object y)
        {
            if (Convert.ToSingle(y) > Convert.ToSingle(x))
            {
                return 1;
            }
            else if (Convert.ToSingle(y) < Convert.ToSingle(x))
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

    }

    
}
