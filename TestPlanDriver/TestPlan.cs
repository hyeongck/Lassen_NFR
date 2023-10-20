
#region COMMNET and Copyright SECTION (NO MANUAL TOUCH!)
// This is AUTOMATIC generated template Test Plan (.cs) file for ATF (Clotho) of WSD, AvagoTech: V2.2.1.0
// Any Questions or Comments, Please Contact: YU HAN, yu.han@avagotech.com
// NOTE 1: Test Plan template .cs has 'FIXED' Sections which should NEVER be Manually Touched 
// NOTE 2: Starting from V2.2.0, Clotho follows new Package style test plan management:
//       (a) Requires valid integer Version defined for TestPlan, TestLimit, and ExcelUI
//               For TestPlan.cs, refer to header item 'TestPlanVersion=1'
//               For TestLiimit.csv, refer to row #7 'SpecVersion,1'
//               For ExcelUI.xlsx, refer to sheet #1, row #1 'VER	1'
//       Note TestPlanTemplateGenerator generated items holds default version as '1'
//       (b) About ExcelUI file and TestLimit file:
//               Always load from same parent folder as Test Plan .cs, @ root level
//       (c) About Correlation File:
//               When Development mode, loaded from  C:\Avago.ATF.Common.x64\CorrelationFiles\Development\
//               When Production mode, loaded from package folder within C:\Avago.ATF.Common.x64\CorrelationFiles\Production\
#endregion COMMNET and Copyright SECTION

#region Test Plan Properties Section (NO MANUAL TOUCH)
////<TestPlanVersion>TestPlanVersion=1<TestPlanVersion/>
////<ExcelBuddyConfig>BuddyExcel = ENGR-8089-AP2-NS_PXI_TCF_Rev01.xlsx;ExcelDisplay = 1<ExcelBuddyConfig/>
////<xTestLimitBuddyConfig>BuddyTestLimit = ENGR-8089-AP2-NS_PXI_TSF_Rev07.csv<TestLimitBuddyConfig/>
////<xCorrelationBuddyConfig>BuddyCorrelaton = ENGR-8089-AP2-NS_PXI_CORR_Rev04.csv<CorrelationBuddyConfig/>
#endregion Test Plan Properties Section

#region Test Plan Hardware Configuration Section
#endregion Test Plan Hardware Configuration Section


#region Test Plan Parameters Section
////<TestParameter>Name="SimHW";Type="IntType";Unit=""<TestParameter/>
#endregion Test Plan Parameters Section


#region Singel Value Parameters Section
////<SingelValueParameter>Name="SimHW";Value="1";Type="IntType";Unit=""<SingelValueParameter/>
#endregion Singel Value Parameters Section


#region Test Plan Sweep Control Section (NO MANUAL TOUCH!)
#endregion Test Plan Sweep Control Section


#region 'FIXED' Reference Section (NO MANUAL TOUCH!)
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Ivi.Visa.Interop;

using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
#endregion 'FIXED' Reference Section


#region Custom Reference Section
//////////////////////////////////////////////////////////////////////////////////
// ----------- ONLY provide your Custom Reference 'Usings' here --------------- //
using MyProduct;
using System.Net;

using clsfrmInputUI;
using ProductionControl_x86;
using TCPHandlerProtocol;

// ----------- END of Custom Reference 'Usings' --------------- //
//////////////////////////////////////////////////////////////////////////////////
#endregion Custom Reference Section


public class LASSEN_NFRISE_REV001 : MarshalByRefObject, IATFTest
{
    MyDUT myDUT;

    #region  SNP (Datalog) variable
    IPHostEntry ipEntry = null;
    DateTime DT = new DateTime();

    bool InitSNP;

    string
    tPVersion = "",
    ProductTag = "",
    lotId = "",
    SublotId = "",
    WaferId = "",
    OpId = "",
    HandlerSN = "",
    newPath = "",
    FileName = "",
    TesterHostName = "",
    TesterIP = "",
    activeDir = @"C:\\Avago.ATF.Common\\DataLog\\";

    //Temp string for current Lot and SubLot ID - to solve Inari issue when using Tally Generator without unload testplan
    //This will cause the datalog for current lot been copied to previous lot folder
    string previous_LotSubLotID = "",
        current_LotSubLotID = "",
        tempWaferId = "",
        tempOpId = "",
        tempHandlerSN = "";

    #endregion

    //GUI ENTRY Variable flag
    bool GUI_Enable = false;

    string MFG_LotID = "123456";
    string InitProTag = "";
    bool FirstTest;
    bool programLoadSuccess = true;

    public ExistechHandler HandlerA;
    int PickUpHeadNo = 111;
    double PlungerForce = 111;
    double WorkingStroke = 111;
    long PlungerHeadNo = 111;
    Boolean Handler_Info = true;
    Boolean HandlerStatus = true;
    public string DoATFInit(string args)
    {
      //  Debugger.Break();

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Enter DoATFInit: {0}\nDo Minimum HW Init:\n{1}\n", args, ATFInitializer.DoMinimumHWInit());


        #region Custom Init Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom Init Coding here --------------- //

        myDUT = new MyDUT(ref sb);
        myDUT.tmpUnit_No = 0;
        myDUT.mfgLotID = MFG_LotID;
        myDUT.deviceID = "XXXX-1234-Y";

        #region Handler Init

        if (Handler_Info)
        {
            string testerid = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_ID, "");
            string Handler_Address = "1";

            if (testerid.Contains("-01"))
            {
                Handler_Address = "1";
            }
            else
            {
                Handler_Address = "2";
            }

            var ping = new System.Net.NetworkInformation.Ping();
            HandlerStatus = true;

            switch (Handler_Address)
            {
                case "1":
                    var result1 = ping.Send("192.168.0.101");
                    if (result1.Status != System.Net.NetworkInformation.IPStatus.Success)
                    {
                        MessageBox.Show("Handler IP Address [192.168.0.101] ping timeout. Please check LAN connection or Local Setting File at \n" + MyDUT.LocSetFilePath + "\n"
                            + "[HANDLER_INFO]\nENABLE = true\nEXISTECH_ADDRESS = 1  <== 192.168.0.101 correctly set in Handler?");
                        HandlerStatus = false;
                    }
                    break;
                case "2":
                    var result2 = ping.Send("192.168.0.102");
                    if (result2.Status != System.Net.NetworkInformation.IPStatus.Success)
                    {
                        MessageBox.Show("Handler IP Address [192.168.0.102] ping timeout. Please check LAN connection or Local Setting File at \n" + MyDUT.LocSetFilePath + "\n"
                            + "[HANDLER_INFO]\nENABLE = true\nEXISTECH_ADDRESS = 2  <== 192.168.0.102 correctly set in Handler?");
                        HandlerStatus = false;
                    }
                    break;
                case "3":
                    var result3 = ping.Send("192.168.0.103");
                    if (result3.Status != System.Net.NetworkInformation.IPStatus.Success)
                    {
                        MessageBox.Show("Handler IP Address [192.168.0.103] ping timeout. Please check LAN connection or Local Setting File at \n" + MyDUT.LocSetFilePath + "\n"
                            + "[HANDLER_INFO]\nENABLE = true\nEXISTECH_ADDRESS = 3  <== 192.168.0.103 correctly set in Handler?");
                        HandlerStatus = false;
                    }
                    break;
                case "4":
                    var result4 = ping.Send("192.168.0.104");
                    if (result4.Status != System.Net.NetworkInformation.IPStatus.Success)
                    {
                        MessageBox.Show("Handler IP Address [192.168.0.104] ping timeout. Please check LAN connection or Local Setting File at \n" + MyDUT.LocSetFilePath + "\n"
                            + "[HANDLER_INFO]\nENABLE = true\nEXISTECH_ADDRESS = 4  <== 192.168.0.104 correctly set in Handler?");
                        HandlerStatus = false;
                    }
                    break;
            }

            if (HandlerStatus)
            {
                HandlerA = new ExistechHandler(Convert.ToInt32(Handler_Address));
                HandlerA.Connect();
                if (!HandlerA.Connected)
                {
                    MessageBox.Show("Handler FW is not ready for PickUpHead Information readback\nPUH info will not available. \n" +
                        "Test still able to proceed.");
                    HandlerStatus = false;
                }
                else
                    HandlerA.StartOfLot();
            }
        }

        #endregion
        //force Clotho to exit if Instrument Init detect failure
        if (!myDUT.InitInstrStatus)
        {
            return TestPlanRunConstants.RunFailureFlag;
        }

        //ChoonChin - Lock product tag field right after init button is pressed
        InitProTag = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "");
        Clotho.EnableClothoTextBoxes(false);
        FirstTest = true;

        //Check boolean status of GUI ENTRY
        GUI_Enable = true;

        FrmDataInput formInput;
        if (GUI_Enable == true)
        {
            #region New InputUI
            formInput = new FrmDataInput();

            //string AssemblyID_ = " ";

            DialogResult rslt = formInput.ShowDialog();

            if (rslt == DialogResult.OK)
            {
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_OP_ID, formInput.OperatorID + "\t");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_LOT_ID, formInput.LotID + "\t");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_SUB_LOT_ID, formInput.SublotID + "\t");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_DIB_ID, formInput.LoadBoardID + "\t");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_CONTACTOR_ID, formInput.ContactorID + "\t");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_HANDLER_SN, formInput.HandlerID);
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PCB_ID, "NA");
                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_WAFER_ID, "NA");
                myDUT.mfgLotID = formInput.MfgLotID;
                myDUT.deviceID = formInput.DeviceID;
            }

            #region Lock ClothoUI
            if (!formInput.AdminLevel)
            {
                Thread t1 = new Thread(new ThreadStart(LockClothoInputUI));
                t1.Start();
            }
            #endregion

            #endregion
        }


        // ----------- END of Custom Init Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom Init Coding Section

        return sb.ToString();
    }


    public string DoATFUnInit(string args)
    {
        Debugger.Break();

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Enter DoATFUnInit: {0}\n", args);


        #region Custom UnInit Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom UnInit Coding here --------------- //

        myDUT.InstrUnInit();

        if (HandlerStatus)
        {
            HandlerA.EndOfLot();
            HandlerA.Disconnect();
        }

        // ----------- END of Custom UnInit Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom UnInit Coding Section

        return sb.ToString();
    }


    public string DoATFLot(string args)
    {
        Debugger.Break();

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Enter DoATFLot: {0}\n", args);


        #region Custom CloseLot Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom CloseLot Coding here --------------- //




        // ----------- END of Custom CloseLot Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom CloseLot Coding Section

        return sb.ToString();
    }


    public ATFReturnResult DoATFTest(string args)
    {
        //Debugger.Break();

        string err = "";
        StringBuilder sb = new StringBuilder();
        ATFReturnResult result = new ATFReturnResult();
        HandlerLotInfo hli = new HandlerLotInfo();

        // ----------- Example for Argument Parsing --------------- //
        Dictionary<string, string> dict = new Dictionary<string, string>();
        if (!ArgParser.parseArgString(args, ref dict))
        {
            err = "Invalid Argument String" + args;
            MessageBox.Show(err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new ATFReturnResult(err);
        }


        int simHW;
        try
        {
            simHW = ArgParser.getIntItem(ArgParser.TagSimMode, dict);
        }
        catch (Exception ex)
        {
            err = ex.Message;
            MessageBox.Show(err, "Exit Test Plan Run", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new ATFReturnResult(err);
        }
        // ----------- END of Argument Parsing Example --------------- //


        #region Custom Test Coding Section
        //////////////////////////////////////////////////////////////////////////////////
        // ----------- ONLY provide your Custom Test Coding here --------------- //
        // Example for build TestPlan Result (Single Site)

        if (FirstTest == true)
        {
            //string[] ResultFileName = ATFCrossDomainWrapper.GetClothoCurrentResultFileFullPath().Split('_');

            //if (GUI_Enable == true)
            //{
            //    if (ResultFileName[0] != InitProTag)
            //    {
            //        programLoadSuccess = false;
            //        MessageBox.Show("Product Tag accidentally changed to: " + ResultFileName[0] + "\nPlease re-load program!");
            //        err = "Product Tag accidentally changed to: " + ResultFileName[0];
            //        return new ATFReturnResult(err); ;
            //    }
            //}
        }

        if (!programLoadSuccess)
        {
            MessageBox.Show("Program was not loaded successfully.\nPlease resolve errors and reload program.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            err = "Program was not loaded successfully.\nPlease resolve errors and reload program";
            return new ATFReturnResult(err);
        }

        #region Retrieve lot ID# (for Datalog)
        //Retrieve lot ID#
        tPVersion = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TP_VER, "");
        ProductTag = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TAG, "").ToUpper();
        lotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "").ToUpper();
        SublotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_SUB_LOT_ID, "").ToUpper();
        WaferId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_WAFER_ID, "");
        OpId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_OP_ID, "");
        HandlerSN = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_HANDLER_SN, "");
        TesterHostName = System.Net.Dns.GetHostName();
        ipEntry = System.Net.Dns.GetHostEntry(TesterHostName);
        //TesterIP = ipEntry.AddressList[0].ToString().Replace(".", ""); //Always default to the 1st network card. This is because for Result FileName , clotho always take the 1st nework id return by system
        TesterIP = NetworkHelper.GetStaticIPAddress().Replace(".", "");      //Use Clotho method , original code has issue with IPv6 - 12/03/2015 Shaz

        if (myDUT.tmpUnit_No == 0)      //do this for the 1st unit only
        {
            DT = DateTime.Now;

            if (ProductTag != "" && lotId != "")
            {
                //// SnP file Dir generation            
                newPath = System.IO.Path.Combine(activeDir, ProductTag + "_" + lotId + "_" + SublotId + "_" + TesterIP + "\\");
                //System.IO.Directory.CreateDirectory(newPath);
                //FileName = System.IO.Path.Combine(activeDir, ProductTag + "_" + lotId + "_" + SublotId + "_" + TesterIP + "\\" + lotId + ".txt");
            }
            else
            {
                string tempname = "DebugMode_" + DT.ToString("yyyyMMdd" + "_" + "HHmmss");
                newPath = System.IO.Path.Combine(activeDir, tempname + "\\");
                //System.IO.Directory.CreateDirectory(newPath);
                ProductTag = "Debug";
                //FileName = System.IO.Path.Combine(activeDir, tempname + "\\" + "DebugMode" + ".txt");
            }

            //Parse information to LibFbar
            myDUT.SNPFile.FileOutput_Path = newPath;
            myDUT.SNPFile.FileOutput_FileName = ProductTag;
            InitSNP = true;

            // Added variable to solve issue with datalog when Inari operator using 
            //Tally Generator to close lot instead of unload test plan
            //WaferId,OpId and HandlerSN are null when 2nd Lot started - make assumption that this 3 param are similar 1st Lot
            tempWaferId = WaferId;
            tempOpId = OpId;
            tempHandlerSN = HandlerSN;
            previous_LotSubLotID = current_LotSubLotID;
        }
        #endregion

#if (!DEBUG)
    myDUT.tmpUnit_No = Convert.ToInt32(ATFCrossDomainWrapper.GetClothoCurrentSN());
#else
        myDUT.tmpUnit_No++;      // Need to enable this during debug mode
#endif

        ATFResultBuilder.Reset();
        FirstTest = false;
        if (HandlerStatus)
        {
            hli = HandlerA.LastTestedInTheLotQuery();

            PickUpHeadNo = hli.PickUpHeadNo;
            PlungerForce = Convert.ToDouble(hli.PlungerForce);
            WorkingStroke = Convert.ToDouble(hli.WorkingStroke);
            PlungerHeadNo = hli.PlungerHeadNo;
        }

        ATFResultBuilder.AddResult(ref result, "PickUpHeadNo", "No", PickUpHeadNo);
        ATFResultBuilder.AddResult(ref result, "PlungerForce", "kgf", PlungerForce);
        ATFResultBuilder.AddResult(ref result, "WorkingStroke", "um", WorkingStroke);
        ATFResultBuilder.AddResult(ref result, "TestSitePosition", "No", PlungerHeadNo);

        ATFResultBuilder.AddResult(ref result, "MFG_LOTID", "NA", Convert.ToDouble(myDUT.mfgLotID));
        myDUT.RunTest(ref result);

        // ----------- END of Custom Test Coding --------------- //
        //////////////////////////////////////////////////////////////////////////////////
        #endregion Custom Test Coding Section

        //ATFReturnResult result = new ATFReturnResult();
        //ATFResultBuilder.AddResult(ref result, "PARAM", "X", 0.01);
        return result;
    }

    private void LockClothoInputUI()
    {
        Clotho.EnableClothoTextBoxes(false);
        Thread.Sleep(5);
        Clotho.EnableClothoTextBoxes(false);
        Thread.Sleep(10);
        Clotho.EnableClothoTextBoxes(false);
        Thread.Sleep(15);
        Clotho.EnableClothoTextBoxes(false);
        Thread.Sleep(20);
        Clotho.EnableClothoTextBoxes(false);
    }

}
