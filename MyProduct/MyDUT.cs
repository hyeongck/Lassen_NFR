using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Avago.ATF.StandardLibrary;
using Ivi.Visa.Interop;
using LibEqmtDriver;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.NIRfsa;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using ni_NoiseFloor;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.SpecAnMX;

namespace MyProduct
{
    public class MyDUT
    {
        public static string CalFilePath;
        public static string LocSetFilePath;
        public static string StartNo_UNIT_ID;         //use during OTP programing for unit_id
        public static string StopNo_UNIT_ID;          //use during OTP programing for unit_id

        public static s_EqmtStatus EqmtStatus;
        public static s_OffBias_AfterTest BiasStatus;
        public s_SNPFile SNPFile;
        public s_SNPDatalog SNPDatalog;
        public s_StopOnFail StopOnFail;
        public static IO_TextFile IO_TxtFile = new IO_TextFile();

        MyUtility myUtility = new MyUtility();
        public static List<string> FailedTests = new List<string>();

        Stopwatch Speedo = new Stopwatch();

        Dictionary<string, string>[] DicTestPA;
        Dictionary<string, string> DicCalInfo;
        Dictionary<string, string> DicWaveForm;
        Dictionary<string, string> DicWaveFormMutate;
        Dictionary<string, string> DicTestLabel;
        Dictionary<string, string>[] DicMipiKey;
        Dictionary<string, string>[] DicPwrBlast;

        List<string> FileContains = new List<string>();

        LibEqmtDriver.SCU.iSwitch EqSwitch;

        string[] SMUSetting;
        LibEqmtDriver.SMU.iPowerSupply[] EqSMU;
        LibEqmtDriver.SMU.PowerSupplyDriver EqSMUDriver;

        LibEqmtDriver.DC_1CH.iDCSupply_1CH[] EqDCSupply;
        LibEqmtDriver.DC_1CH.iDCSupply_1CH EqDC_1CH;
        LibEqmtDriver.DC.iDCSupply EqDC;

        LibEqmtDriver.PS.iPowerSensor EqPwrMeter;

        LibEqmtDriver.SA.iSigAnalyzer EqSA01, EqSA02;
        LibEqmtDriver.SG.iSiggen EqSG01, EqSG02;
        LibEqmtDriver.SG.N5182A_WAVEFORM_MODE ModulationType;

        LibEqmtDriver.MIPI.iMiPiCtrl EqMiPiCtrl;

        LibEqmtDriver.NF_VST.NF_NiPXI_VST EqVST;
        LibEqmtDriver.NF_VST.NF_NI_RFmx EqRFmx; //Seoul

        LibEqmtDriver.TuneableFilter.iTuneFilterDriver EqTuneFilter;

        // Initialize flag
        string PreviousSWMode = "";
        string PreviousMXAMode = "";
        bool MXA_DisplayEnable = false;
        double SGTargetPin = -999;         //Global variable for SG input power
        double CurrentSaAttn = -999;
        double CurrentSa2Attn = -999;
        //Ivan
        static bool FirstDut = true;

        //Result Variable
        #region Result Variable

        int R_ReadMipiReg = -999;

        double R_NF1_Ampl = -999,
            R_NF2_Ampl = -999,
            R_NF1_Freq = -999,
            R_NF2_Freq = -999,
            R_H2_Ampl = -999,
            R_H2_Freq = -999,
            R_Pin = -999,
            R_Pin1 = -999,
            R_Pin2 = -999,
            R_Pout = -999,
            R_Pout1 = -999,
            R_Pout2 = -999,
            R_ITotal = -999,
            R_MIPI = -999,
            R_DCSupply = -999,
            R_Switch = -999,
            R_RFCalStatus = -999;
        #endregion

        #region Misc Variable

        public bool InitInstrStatus = true;

        public int tmpUnit_No;
        public s_Result[] Results;
        public int TestCount;
        public s_TraceData[] MXATrace;
        public s_TraceNo Result_MXATrace;
        public s_TraceData[] PXITrace;
        public s_TraceData[] PXITraceRaw;
        public s_TraceNo Result_PXITrace;
        float dummyData;
        string dummyStrData;

        public int totalDCSupply = 4;        //max DC Supply 1 Channel is 4 (equal 4 channel in tcf)
        int multiRBW_cnt;
        int rbw_counter;
        string rbwParamName = null;
        int NoOfPts = 0;
        double[] RXContactdBm;
        double[] RXContactFreq;
        double[] RXContactGain;//Seoul
        double[] RXPathLoss;//Seoul
        double[] LNAInputLoss;//Seoul
        double[] TXPAOnFreq;//Seoul
        string TXCenterFreq;//Seoul
        double[] Cold_NF;//Seoul
        double[] Cold_NoisePower;//Seoul
        double[] Hot_NF;//Seoul
        double[] Hot_NoisePower;//Seoul
        double[] NFRise;//Seoul
        string calDir;//Seoul
        bool NFCalFlag = true;//Seoul
        Dictionary<double, double> RxGainDic;

        double[][] NF_new;//Seoul
        double[][] NoisePower_new;//Seoul
        double[][] Cold_NF_new;//Seoul
        double[][] Cold_NoisePower_new;//Seoul
        double[][] Hot_NF_new;//Seoul
        double[][] Hot_NoisePower_new;//Seoul

        public string mfgLotID_Path = @"C:\\Avago.ATF.Common\\OTPLogger\\";           //Default path for OTP programming datalogger
        public string mfgLotID;
        public string OTPLogFilePath = null;
        public int otpUnitID;
        public string deviceID;

        //MIPI variables - Custom MIPI Setting
        public string CusMipiKey;
        public string CusMipiRegMap;
        public string CusPMTrigMap;
        public string CusSlaveAddr;
        public string CusMipiPair;
        public string CusMipiSite;
        public string DicMipiTKey;

        //MIPI OTP Variable
        int totalbits = 16;         //default total bit for 2 register address is 16bits (binary)
        int effectiveBits = 16;     //default effective bit for 2 register address is 16bits (binary) - eg. JEDI SN ID only use up to 14bits
        string[] dataHex = null;
        int[] dataDec;
        string[] dataBinary = null;
        string[] biasDataArr = null;
        Int32 dataDec_Conv = 0;
        string dataSizeHex = "0xFFFF";  //default size for 2 byte register (16bit)
        int tmpOutData = 0;
        string appendHex = null;
        string appendBinary = null;
        string effectiveData = null;
        string tmpData = null;

        bool b_lockBit = true;
        int i_lockBit = 0;
        bool b_lockBit2 = true;
        int i_lockBit2 = 0;
        int i_testFlag = 0;
        bool b_testFlag = true;
        int i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
        bool BurnOTP = false;
        int[] efuseCtrlAddress = new int[3];
        string[] tempData = new string[2];

        Int32 tmpOutData_DecConv = 255;     //set to default very high because this variable use to check empty register (byright the value will be '0' if blank register)
        string CM_SITE = "NA";

        //Power Blast variables - PWRBlast sheet
        bool b_PwrBlastTKey = false;
        public double CtrFreqMHz_pwrBlast;
        public double StartPwrLvldBm_pwrBlast;
        public double StopPwrLvldBm_pwrBlast;
        public int StepPwrLvl_pwrBlast;
        public double DwellTmS_pwrBlast;
        public double Transient_mS_pwrBlast;
        public int Transient_Step_pwrBlast;

        //double unit variable
        public int PreviousModID = -1;
        public string ReadValue = "";

        #endregion

        public MyDUT(ref StringBuilder sb)
        {
            Init(ref sb);
        }
        ~MyDUT()
        {
            UnInit();
        }
        public void RunTest(ref ATFReturnResult results)
        {
            Results = new s_Result[DicTestPA.Length];
            MXATrace = new s_TraceData[DicTestPA.Length];
            PXITrace = new s_TraceData[DicTestPA.Length];
            PXITraceRaw = new s_TraceData[DicTestPA.Length];
            FailedTests.Clear(); //Reset failed test

            string StrError = string.Empty;
            long TestTimeFBar, TestTimePA;

            TestCount = 0; //reset to start
            Speedo.Reset();
            Speedo.Start();
            StopOnFail.TestFail = false;
            LibEqmtDriver.MIPI.Lib_Var.b_setNIVIO = true;          //do this once for every unit for NI6570

            foreach (Dictionary<string, string> currTestCond in DicTestPA)
            {
                MXATrace[TestCount].Multi_Trace = new s_TraceNo[1][];
                MXATrace[TestCount].Multi_Trace[0] = new s_TraceNo[2]; //initialize to 2 for 2x MXA trace only

                PXITrace[TestCount].Multi_Trace = new s_TraceNo[10][];  //maximum of 10 RBW trace can be stored
                PXITraceRaw[TestCount].Multi_Trace = new s_TraceNo[10][];  //maximum of 10 RBW trace can be stored

                for (int i = 0; i < PXITrace[TestCount].Multi_Trace.Length; i++)
                {
                    PXITrace[TestCount].Multi_Trace[i] = new s_TraceNo[50]; //initialize to 15 for 15x PXI trace loop only
                    PXITraceRaw[TestCount].Multi_Trace[i] = new s_TraceNo[50]; //initialize to 15 for 15x PXI trace loop only
                }

                if (StopOnFail.Enable != true)      //reset stop on fail flag if enable flag is false
                {
                    StopOnFail.TestFail = false;
                }

                ExecuteTest(currTestCond, ref results);
                TestCount++;
                //ATFResultBuilder.AddResultToDict("aaa", 12, ref StrError);              
            }

            Speedo.Stop();
            TestTimePA = Speedo.ElapsedMilliseconds;

            #region Close RFmx Session after NF Calibration for save cal file -Seoul
            for (int i = 0; i < DicTestPA.Count(); i++)
            {
                string testMode, testParameter;
                DicTestPA[i].TryGetValue("TEST MODE", out testMode);
                DicTestPA[i].TryGetValue("TEST PARAMETER", out testParameter);

                if (testMode.ToUpper() == "CALIBRATION" && testParameter.ToUpper() == "NF_CAL")
                {
                    EqRFmx.CloseSession();
                    MessageBox.Show("The NF calibration is finished.");
                    break;
                }
            }
            #endregion

            #region Power Off SMU and RF Power - for next DUT
            if (BiasStatus.SMU)
            {
                float offVolt = 0.0f;
                float offCurr = 1e-3f;

                if (EqmtStatus.SMU)
                {
                    string[] SetSMU = EqmtStatus.SMU_CH.Split(',');

                    string[] SetSMUSelect = new string[SetSMU.Count()];
                    for (int i = 0; i < SetSMU.Count(); i++)
                    {
                        int smuVChannel = Convert.ToInt16(SetSMU[i]);
                        SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                        EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, offVolt, offCurr);
                    }
                }
            }
            if (EqmtStatus.MXG01)
            {
                EqSG01.SetAmplitude(-110);
                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
            }
            if (EqmtStatus.MXG02)
            {
                EqSG02.SetAmplitude(-110);
                EqSG02.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
            }
            if (EqmtStatus.PM)
            {
                EqPwrMeter.SetOffset(1, 0); //reset power sensor offset to default : 0
            }
            if (EqmtStatus.MIPI)
            {
                EqMiPiCtrl.TurnOff_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT TX
                EqMiPiCtrl.TurnOff_VIO(1);      //mipi pair 1 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board / DUT RX
                EqMiPiCtrl.TurnOff_VIO(2);      //mipi pair 2 - DIO 6, DIO 7 and DIO 8 - For future use
                EqMiPiCtrl.TurnOff_VIO(3);      //mipi pair 3 - DIO 9, DIO 10 and DIO 11 - For future use
            }
            #endregion

            ATFResultBuilder.AddResult(ref results, "PATestTime", "mS", TestTimePA);
            ATFResultBuilder.AddResult(ref results, "TotalTestTime", "mS", TestTimePA);

        }

        private void Init(ref StringBuilder sb)
        {
            #region Load TCF
            ManualResetEvent[] DoneEvents = new ManualResetEvent[5];
            DoneEvents[0] = new ManualResetEvent(false);
            DoneEvents[1] = new ManualResetEvent(false);
            DoneEvents[2] = new ManualResetEvent(false);
            DoneEvents[3] = new ManualResetEvent(false);
            DoneEvents[4] = new ManualResetEvent(false);

            ThreadWithDelegate ThLoadPaTCF = new ThreadWithDelegate(DoneEvents[0]);
            ThLoadPaTCF.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadPaTCF);
            ThreadPool.QueueUserWorkItem(ThLoadPaTCF.ThreadPoolCallback, 0);

            ThreadWithDelegate ThLoadWaveForm = new ThreadWithDelegate(DoneEvents[1]);
            ThLoadWaveForm.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadWafeForm);
            ThreadPool.QueueUserWorkItem(ThLoadWaveForm.ThreadPoolCallback, 0);

            ThreadWithDelegate ThLoadCalTCF = new ThreadWithDelegate(DoneEvents[2]);
            ThLoadCalTCF.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadCalTCF);
            ThreadPool.QueueUserWorkItem(ThLoadCalTCF.ThreadPoolCallback, 0);

            ThreadWithDelegate ThLoadMipiReg = new ThreadWithDelegate(DoneEvents[3]);
            ThLoadMipiReg.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadMipiReg);
            ThreadPool.QueueUserWorkItem(ThLoadMipiReg.ThreadPoolCallback, 0);

            ThreadWithDelegate ThLoadPwrBlast = new ThreadWithDelegate(DoneEvents[4]);
            ThLoadPwrBlast.WorkExternal = new ThreadWithDelegate.DoWorkExternal(ReadPwrBlast);
            ThreadPool.QueueUserWorkItem(ThLoadPwrBlast.ThreadPoolCallback, 0);

            WaitHandle.WaitAll(DoneEvents);

            #endregion

            #region Retrieve Cal Sheet Info

            CalFilePath = Convert.ToString(DicCalInfo[DataFilePath.CalPathRF]);
            LocSetFilePath = Convert.ToString(DicCalInfo[DataFilePath.LocSettingPath]);

            #endregion

            #region Read Local Setting File

            string CalEnable = myUtility.ReadTextFile(LocSetFilePath, LocalSetting.HeaderFilePath, LocalSetting.keyCalEnable);

            //Read & Set DC & SMU biasing status - OFF/ON for every DUT after complete test
            BiasStatus.DC = Convert.ToBoolean(myUtility.ReadTextFile(LocSetFilePath, "OFF_AfterTest", "DC"));
            BiasStatus.SMU = Convert.ToBoolean(myUtility.ReadTextFile(LocSetFilePath, "OFF_AfterTest", "SMU"));

            //Read Stop On Failure status mode - True (program will stop testing if failure happen) , false (proceed per normal)
            StopOnFail.TestFail = false;      //init to default 
            StopOnFail.Enable = Convert.ToBoolean(myUtility.ReadTextFile(LocSetFilePath, "STOP_ON_FAIL", "ENABLE"));

            #endregion

            #region Instrument Init

            InstrInit(LocSetFilePath);

            #endregion

            #region Load Cal

            try
            {
                ATFCrossDomainWrapper.Cal_SwitchInterpolationFlag(true);
                ATFCrossDomainWrapper.Cal_LoadCalData(LocalSetting.CalTag, CalFilePath);
            }
            catch (Exception ex)
            {
                if (DicTestPA[0].ContainsValue("Calibration"))
                {
                    //Do Nothing
                }
                else
                {
                    sb.AppendFormat("Fail to Load 1D Cal Data from {0}: {1}\n", CalFilePath, ex.Message);
                }
            }
            #endregion

            #region RFMX NF Initial Setting -Seoul

            EqRFmx.InitList(DicTestPA.Count());

            for (int i = 0; i < DicTestPA.Count(); i++)
            {
                string parameterName;
                DicTestPA[i].TryGetValue("TEST PARAMETER", out parameterName);

                if (parameterName.ToUpper() == "NF_CAL" || parameterName.ToUpper() == "PXI_NF_COLD" || parameterName.ToUpper() == "PXI_NF_HOT")
                {
                    double nF_BW = Convert.ToDouble(DicTestPA[i]["NF_BW"]);
                    double nF_REFLEVEL = Convert.ToDouble(DicTestPA[i]["NF_REFLEVEL"]);
                    double nF_SWEEPTIME = Convert.ToDouble(DicTestPA[i]["NF_SWEEPTIME"]);
                    int nF_AVERAGE = Convert.ToInt32(DicTestPA[i]["NF_AVERAGE"]);
                    string calSetID = DicTestPA[i]["NF_CALTAG"];

                    double[] dutInputLoss;
                    double[] dutOutputLoss;
                    double[] freqList;

                    NFvariables(DicTestPA[i], out dutInputLoss, out dutOutputLoss, out freqList);
                    EqRFmx.ListConfigureSpecNFColdSource(i, nF_BW, nF_SWEEPTIME, nF_AVERAGE, nF_REFLEVEL, parameterName.ToUpper(), dutInputLoss, dutOutputLoss, freqList, calSetID);
                }
            }
            #endregion

            #region Close VSG/VSA session if this is RF_Calibration case -Seoul

            for (int i = 0; i < DicTestPA.Count(); i++)
            {
                string testMode, testParameter;
                DicTestPA[i].TryGetValue("TEST MODE", out testMode);
                DicTestPA[i].TryGetValue("TEST PARAMETER", out testParameter);

                if (testMode.ToUpper() == "CALIBRATION" && testParameter.ToUpper() == "RF_CAL")
                {
                    EqVST.Close_VST();
                }
            }

            #endregion

        }
        private void UnInit()
        {
            var processes = from p in System.Diagnostics.Process.GetProcessesByName("EXCEL") select p;

            foreach (var process in processes)
            {
                // All those background un-release process will be closed
                if (process.MainWindowTitle == "")
                    process.Kill();
            }

            //InstrUnInit();
        }

        private void InstrInit(string LocSetFilePath)
        {
            #region Tuneable Filter
            string Filtermodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "FILTER");
            string Filteraddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "FILTER");

            switch (Filtermodel.ToUpper())
            {
                case "KNL":
                    EqTuneFilter = new LibEqmtDriver.TuneableFilter.cKnL_D5BT(Filteraddr);
                    EqmtStatus.TuneFilter = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.TuneFilter = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment FILTER Model : " + Filtermodel.ToUpper(), "Pls ignore if Equipment Switch not require.");
                    EqmtStatus.TuneFilter = false;
                    break;

            }
            #endregion

            #region Switch Init
            string SWmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "Switch");
            string SWaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "Switch");

            switch (SWmodel.ToUpper())
            {
                case "3499A":
                    EqSwitch = new LibEqmtDriver.SCU.Agilent3499(SWaddr);
                    EqSwitch.Initialize();
                    EqSwitch.SetPath(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], TCF_Header.ConstSwBand, "INIT"));
                    EqmtStatus.Switch = true;
                    break;
                case "AEM_WOLFER":
                    EqSwitch = new LibEqmtDriver.SCU.AeWofer(8);
                    EqSwitch.Initialize();
                    EqmtStatus.Switch = true;
                    break;
                case "SW_NI6509":
                    EqSwitch = new LibEqmtDriver.SCU.NI6509(SWaddr);
                    EqSwitch.Initialize();
                    EqmtStatus.Switch = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.Switch = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment SWITCH Model : " + SWmodel.ToUpper(), "Pls ignore if Equipment Switch not require.");
                    EqmtStatus.Switch = false;
                    break;
            }
            #endregion

            #region SMU Init

            string SMUmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "SMU");
            string SMUaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "SMU");

            int SMUtotalCH = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "SmuSetting", "TOTAL_SMUCHANNEL"));
            SMUSetting = new string[SMUtotalCH];

            for (int smuCH = 0; smuCH < SMUtotalCH; smuCH++)
            {
                SMUSetting[smuCH] = myUtility.ReadTextFile(LocSetFilePath, "SmuSetting", "SMUV_CH" + smuCH);
            }

            EqSMU = new LibEqmtDriver.SMU.iPowerSupply[1];
            EqSMUDriver = new LibEqmtDriver.SMU.PowerSupplyDriver();

            switch (SMUmodel.ToUpper())
            {
                case "AM1340":
                    EqSMU[0] = new LibEqmtDriver.SMU.Aemulus1340(0);
                    EqSMUDriver.Initialize(EqSMU);
                    EqmtStatus.SMU = true;
                    EqmtStatus.SMU_CH = "0";        //Initialize variable default SMU_CH to SMU Channel0
                    break;
                case "AEPXI":
                    EqSMU[0] = new LibEqmtDriver.SMU.AePXISMU(SMUSetting);
                    EqmtStatus.SMU = true;
                    EqmtStatus.SMU_CH = "0";        //Initialize variable default SMU_CH to SMU Channel0
                    break;
                case "NIPXI":
                    EqSMU[0] = new LibEqmtDriver.SMU.NiPXISMU(SMUSetting);
                    EqmtStatus.SMU = true;
                    EqmtStatus.SMU_CH = "0";        //Initialize variable default SMU_CH to SMU Channel0
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.SMU = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment SMU Model : " + SMUmodel.ToUpper(), "Pls ignore if Equipment SMU not require.");
                    EqmtStatus.SMU = false;
                    break;
            }
            #endregion

            #region Multiple 1CH DC Supply
            //This initilaization will also work with a single 4 Channel Power Supply like N6700B
            //For example N6700B, all address will be same. Software will create 4 instance for each channel

            EqDCSupply = new LibEqmtDriver.DC_1CH.iDCSupply_1CH[totalDCSupply];
            EqmtStatus.DCSupply = new bool[totalDCSupply];

            for (int i = 0; i < totalDCSupply; i++)
            {
                string DCSupplymodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "DCSUPPLY0" + (i + 1));
                string DCSupplyaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "DCSUPPLY0" + (i + 1));

                switch (DCSupplymodel.ToUpper())
                {
                    case "E3633A":
                    case "E3644A":
                        EqDCSupply[i] = new LibEqmtDriver.DC_1CH.E3633A(DCSupplyaddr);
                        EqDCSupply[i].Init();
                        EqmtStatus.DCSupply[i] = true;
                        break;
                    case "N6700B":
                        EqDCSupply[i] = new LibEqmtDriver.DC_1CH.N6700B(DCSupplyaddr);
                        EqDCSupply[i].Init();
                        EqmtStatus.DCSupply[i] = true;
                        break;
                    case "NONE":
                    case "NA":
                        EqmtStatus.DCSupply[i] = false;
                        // Do Nothing , equipment not present
                        break;
                    default:
                        MessageBox.Show("Equipment DC Supply Model(DCSUPPLY0" + (i + 1) + ") : " + DCSupplymodel.ToUpper(), "Pls ignore if Equipment DC not require.");
                        EqmtStatus.DCSupply[i] = false;
                        break;
                }
            }
            #endregion

            #region DC 1-Channel Init
            string DCmodel_1CH = myUtility.ReadTextFile(LocSetFilePath, "Model", "PWRSUPPLY_1CH");
            string DCaddr_1CH = myUtility.ReadTextFile(LocSetFilePath, "Address", "PWRSUPPLY_1CH");

            switch (DCmodel_1CH.ToUpper())
            {
                case "E3633A":
                case "E3644A":
                    EqDC_1CH = new LibEqmtDriver.DC_1CH.E3633A(DCaddr_1CH);
                    EqDC_1CH.Init();
                    EqmtStatus.DC_1CH = true;
                    break;
                case "N6700B":
                    EqDC_1CH = new LibEqmtDriver.DC_1CH.N6700B(DCaddr_1CH);
                    EqDC_1CH.Init();
                    EqmtStatus.DC_1CH = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.DC_1CH = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment DC Supply 1-Channel Model : " + DCmodel_1CH.ToUpper(), "Pls ignore if Equipment DC not require.");
                    EqmtStatus.DC_1CH = false;
                    break;
            }
            #endregion

            #region DC Init

            string DCmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "PWRSUPPLY");
            string DCaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "PWRSUPPLY");

            switch (DCmodel.ToUpper())
            {
                case "N6700B":
                    EqDC = new LibEqmtDriver.DC.N6700B(DCaddr);
                    EqDC.Init();
                    EqmtStatus.DC = true;
                    break;
                case "PS662xA":
                    EqDC = new LibEqmtDriver.DC.PS662xA(DCaddr);
                    EqDC.Init();
                    EqmtStatus.DC = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.DC = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment DC Supply Model : " + DCmodel.ToUpper(), "Pls ignore if Equipment DC not require.");
                    EqmtStatus.DC = false;
                    break;
            }
            #endregion

            #region SA Init
            string SA01model = myUtility.ReadTextFile(LocSetFilePath, "Model", "MXA01");
            string SA01addr = myUtility.ReadTextFile(LocSetFilePath, "Address", "MXA01");
            string SA02model = myUtility.ReadTextFile(LocSetFilePath, "Model", "MXA02");
            string SA02addr = myUtility.ReadTextFile(LocSetFilePath, "Address", "MXA02");
            bool cal_MXA = false;
            bool status = false;

            switch (SA01model.ToUpper())
            {
                case "N9020A":
                    //MXA Alignment Calibration
                    string cnt_str = Interaction.InputBox("Do you want to perform MXA#01 alignment cal?\n" + "If so, please enter \"Yes\".", "MXA#01 ALIGNMENT", "No", 200, 200);
                    switch (cnt_str.ToUpper())
                    {
                        case "NO":
                            break;
                        case "YES":
                            cal_MXA = true;
                            break;
                        case "CANCEL":
                            break;
                        default:
                            break;
                    }

                    EqSA01 = new LibEqmtDriver.SA.N9020A(SA01addr);
                    EqSA01.Preset();
                    if (cal_MXA)
                    {
                        EqSA01.CAL();
                    }
                    EqmtStatus.MXA01 = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.MXA01 = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment MXA Model : " + SA01model.ToUpper(), "Pls ignore if Equipment MXA not required.");
                    EqmtStatus.MXA01 = false;
                    break;
            }

            switch (SA02model.ToUpper())
            {
                case "N9020A":
                    //MXA Alignment Calibration
                    string cnt_str = Interaction.InputBox("Do you want to perform MXA#02 alignment cal?\n" + "If so, please enter \"Yes\".", "MXA#02 ALIGNMENT", "No", 200, 200);
                    switch (cnt_str.ToUpper())
                    {
                        case "NO":
                            break;
                        case "YES":
                            cal_MXA = true;
                            break;
                        case "CANCEL":
                            break;
                        default:
                            break;
                    }

                    EqSA02 = new LibEqmtDriver.SA.N9020A(SA02addr);
                    EqSA02.Preset();
                    if (cal_MXA)
                    {
                        EqSA02.CAL();
                    }
                    EqmtStatus.MXA02 = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.MXA02 = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment MXA02 Model : " + SA02model.ToUpper(), "Pls ignore if Equipment MXA02 not required.");
                    EqmtStatus.MXA02 = false;
                    break;
            }

            if (cal_MXA)
            {
                DelayMs(60000);        //delay to wait for alignment to complete
            }
            else
            {
                DelayMs(1000);
            }

            switch (SA01model.ToUpper())
            {
                case "N9020A":
                    //MXA Display Enable/Disable
                    string cnt_str = Interaction.InputBox("Do you want to enable MXA#01 Display?\n" + "If so, please enter \"Yes\".", "Penang NPI", "No", 200, 200);
                    switch (cnt_str.ToUpper())
                    {
                        case "YES":
                            MXA_DisplayEnable = false;
                            break;
                        case "NO":
                            MXA_DisplayEnable = true;
                            break;
                        default:
                            MXA_DisplayEnable = true;
                            break;
                    }

                    status = EqSA01.OPERATION_COMPLETE();
                    EqSA01.AUTOALIGN_ENABLE(false);
                    EqSA01.Initialize(3);
                    if (MXA_DisplayEnable)
                    {
                        EqSA01.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.OFF);
                    }
                    else
                    {
                        EqSA01.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.ON);
                    }
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.MXA01 = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment MXA Model : " + SA01model.ToUpper(), "Pls ignore if Equipment MXA not required.");
                    EqmtStatus.MXA01 = false;
                    break;
            }

            switch (SA02model.ToUpper())
            {
                case "N9020A":
                    //MXA Display Enable/Disable
                    string cnt_str = Interaction.InputBox("Do you want to enable MXA#02 Display?\n" + "If so, please enter \"Yes\".", "Penang NPI", "No", 200, 200);
                    switch (cnt_str.ToUpper())
                    {
                        case "YES":
                            MXA_DisplayEnable = false;
                            break;
                        case "NO":
                            MXA_DisplayEnable = true;
                            break;
                        default:
                            MXA_DisplayEnable = true;
                            break;
                    }

                    status = EqSA02.OPERATION_COMPLETE();
                    EqSA02.AUTOALIGN_ENABLE(false);
                    EqSA02.Initialize(3);
                    if (MXA_DisplayEnable)
                    {
                        EqSA02.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.OFF);
                    }
                    else
                    {
                        EqSA02.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.ON);
                    }
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.MXA02 = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment MXA02 Model : " + SA02model.ToUpper(), "Pls ignore if Equipment MXA02 not required.");
                    EqmtStatus.MXA02 = false;
                    break;
            }

            #endregion

            #region SG Init
            string SG01model = myUtility.ReadTextFile(LocSetFilePath, "Model", "MXG01");
            string SG01addr = myUtility.ReadTextFile(LocSetFilePath, "Address", "MXG01");
            string SG02model = myUtility.ReadTextFile(LocSetFilePath, "Model", "MXG02");
            string SG02addr = myUtility.ReadTextFile(LocSetFilePath, "Address", "MXG02");

            switch (SG01model.ToUpper())
            {
                case "N5182A":
                    EqSG01 = new LibEqmtDriver.SG.N5182A(SG01addr);
                    EqSG01.Reset();
                    foreach (string key in DicWaveForm.Keys)
                    {
                        EqSG01.MOD_FORMAT_WITH_LOADING_CHECK(key.ToString(), DicWaveForm[key].ToString(), true);
                        DelayMs(500);
                        status = EqSG01.OPERATION_COMPLETE();
                        EqSG01.QueryError_SG(out InitInstrStatus);
                        if (!InitInstrStatus)
                        {
                            MessageBox.Show("Test Program Will Abort .. Please Fixed The Issue", "Equipment MXG01 Model : " + SG01model.ToUpper(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                    }
                    EqSG01.Initialize();
                    EqSG01.SetAmplitude(-110);
                    EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                    EqSG01.EnableModulation(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                    EqmtStatus.MXG01 = true;
                    break;
                case "E8257D":
                    EqSG01 = new LibEqmtDriver.SG.E8257D(SG01addr);
                    EqSG01.Initialize();
                    EqSG01.SetAmplitude(-40);
                    EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                    EqSG01.EnableModulation(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                    EqmtStatus.MXG01 = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.MXG01 = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment MXG01 Model : " + SG01model.ToUpper(), "Pls ignore if Equipment MXA01 not required.");
                    EqmtStatus.MXG01 = false;
                    break;
            }

            switch (SG02model.ToUpper())
            {
                case "N5182A":
                    EqSG02 = new LibEqmtDriver.SG.N5182A(SG02addr);
                    EqSG01.Reset();
                    foreach (string key in DicWaveForm.Keys)
                    {
                        EqSG02.MOD_FORMAT_WITH_LOADING_CHECK(key.ToString(), DicWaveForm[key].ToString(), true);
                        DelayMs(500);
                        status = EqSG02.OPERATION_COMPLETE();
                        EqSG02.QueryError_SG(out InitInstrStatus);
                        if (!InitInstrStatus)
                        {
                            MessageBox.Show("Test Program Will Abort .. Please Fixed The Issue", "Equipment MXG02 Model : " + SG02model.ToUpper(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                    }
                    EqSG02.Initialize();
                    EqSG02.SetAmplitude(-110);
                    EqSG02.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                    EqSG02.EnableModulation(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                    EqmtStatus.MXG02 = true;
                    break;
                case "E8257D":
                    EqSG02 = new LibEqmtDriver.SG.E8257D(SG01addr);
                    EqSG02.Initialize();
                    EqSG02.SetAmplitude(-40);
                    EqSG02.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                    EqSG02.EnableModulation(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                    EqmtStatus.MXG02 = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.MXG02 = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment MXG02 Model : " + SG02model.ToUpper(), "Pls ignore if Equipment MXG02 not required.");
                    EqmtStatus.MXG02 = false;
                    break;
            }
            #endregion

            #region VST Init
            string VSTmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "PXI_VST");
            string VSTaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "PXI_VST");

            switch (VSTmodel.ToUpper())
            {
                case "NI5644R":
                case "PXIE-5644R":
                    EqVST = new LibEqmtDriver.NF_VST.NF_NiPXI_VST(VSTaddr);
                    EqRFmx = new LibEqmtDriver.NF_VST.NF_NI_RFmx(VSTaddr);
                    EqVST.Initialize();
                    EqVST.IQRate = 120e6;

                    foreach (string key in DicWaveForm.Keys)
                    {
                        EqVST.MOD_FORMAT_CHECK(key.ToString(), DicWaveForm[key].ToString(), DicWaveFormMutate[key].ToString(), true);
                    }
                    EqVST.PreConfigureVST();
                    break;
                case "NI5646R":
                case "PXIE-5646R":
                    EqVST = new LibEqmtDriver.NF_VST.NF_NiPXI_VST(VSTaddr);
                    EqRFmx = new LibEqmtDriver.NF_VST.NF_NI_RFmx(VSTaddr);
                    EqVST.Initialize();
                    EqVST.IQRate = 250E6;

                    foreach (string key in DicWaveForm.Keys)
                    {
                        EqVST.MOD_FORMAT_CHECK(key.ToString(), DicWaveForm[key].ToString(), DicWaveFormMutate[key].ToString(), true);
                    }
                    EqVST.PreConfigureVST();
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.PXI_VST = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment PXI VST Model : " + VSTmodel.ToUpper(), "Pls ignore if Equipment PXI_VST not required");
                    EqmtStatus.TuneFilter = false;
                    break;
            }


            #endregion

            #region Power Sensor Init
            string PMmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "PWRMETER");
            string PMaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "PWRMETER");

            switch (PMmodel.ToUpper())
            {
                case "E4416A":
                case "E4417A":
                    EqPwrMeter = new LibEqmtDriver.PS.E4417A(PMaddr);
                    EqPwrMeter.Initialize(1);
                    EqmtStatus.PM = true;
                    break;
                case "NRPZ11":

                    EqPwrMeter = new LibEqmtDriver.PS.RSNRPZ11("USB::0x0aad::0x000c::" + PMaddr);
                    EqPwrMeter.Initialize(1);
                    EqPwrMeter.SetFreq(1, 1500);
                    DelayMs(100);
                    dummyData = EqPwrMeter.MeasPwr(1);
                    EqmtStatus.PM = true;
                    break;
                case "NRP8S":
                    EqPwrMeter = new LibEqmtDriver.PS.RSNRPZ11("USB::0x0aad::0x00e2::" + PMaddr);
                    EqPwrMeter.Initialize(1);
                    EqPwrMeter.SetFreq(1, 1500);
                    DelayMs(200);
                    dummyData = EqPwrMeter.MeasPwr(1);
                    EqmtStatus.PM = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.PM = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment POWERSENSOR Model : " + PMmodel.ToUpper(), "Pls ignore if Equipment Power Sensor not require.");
                    EqmtStatus.PM = false;
                    break;
            }
            #endregion

            #region MIPI Init

            string MIPImodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "MIPI_Card");
            string MIPIaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "MIPI_Card");
            string AemulusPxi_FileName = myUtility.ReadTextFile(LocSetFilePath, "Address", "APXI_FileName");

            #region MIPI Pin Config
            //use for MIPI pin initialization
            string mipiPairCount = "";
            LibEqmtDriver.MIPI.s_MIPI_PAIR[] tmp_mipiPair;
            mipiPairCount = myUtility.ReadTextFile(LocSetFilePath, "MIPI_PIN_CFG", "Mipi_Pair_Count");
            if (mipiPairCount == "")
            {
                // Not define in config file - set to default of 2 mipi pair only
                tmp_mipiPair = new LibEqmtDriver.MIPI.s_MIPI_PAIR[2];

                tmp_mipiPair[0].PAIRNO = 0;
                tmp_mipiPair[0].SCLK = "0";
                tmp_mipiPair[0].SDATA = "1";
                tmp_mipiPair[0].SVIO = "2";

                tmp_mipiPair[1].PAIRNO = 1;
                tmp_mipiPair[1].SCLK = "4";
                tmp_mipiPair[1].SDATA = "5";
                tmp_mipiPair[1].SVIO = "3";

            }
            else
            {
                tmp_mipiPair = new LibEqmtDriver.MIPI.s_MIPI_PAIR[Convert.ToInt32(mipiPairCount)];

                for (int i = 0; i < tmp_mipiPair.Length; i++)
                {
                    tmp_mipiPair[i].PAIRNO = i;
                    tmp_mipiPair[i].SCLK = myUtility.ReadTextFile(LocSetFilePath, "MIPI_PIN_CFG", "SCLK_" + i);
                    tmp_mipiPair[i].SDATA = myUtility.ReadTextFile(LocSetFilePath, "MIPI_PIN_CFG", "SDATA_" + i);
                    tmp_mipiPair[i].SVIO = myUtility.ReadTextFile(LocSetFilePath, "MIPI_PIN_CFG", "SVIO_" + i);
                }
            }

            #endregion

            switch (MIPImodel.ToUpper())
            {
                case "DM280E":
                    try
                    {
                        LibEqmtDriver.MIPI.Lib_Var.ºmyDM280Address = MIPIaddr;
                        LibEqmtDriver.MIPI.Lib_Var.ºDM280_CH0 = 0;
                        LibEqmtDriver.MIPI.Lib_Var.ºDM280_CH1 = 1;
                        LibEqmtDriver.MIPI.Lib_Var.ºSlaveAddress = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Slave_Address"), 16);
                        LibEqmtDriver.MIPI.Lib_Var.ºChannelUsed = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Channel_Used"));
                        //PM Trigger 
                        LibEqmtDriver.MIPI.Lib_Var.ºPMTrig = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "PM_Trig"), 16);
                        LibEqmtDriver.MIPI.Lib_Var.ºPMTrig_Data = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "PM_Trig_Data"), 16);
                        //Read Function
                        string read = myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Read_Function");
                        LibEqmtDriver.MIPI.Lib_Var.ºReadFunction = (read.ToUpper() == "TRUE" ? true : false);

                        //Init
                        EqMiPiCtrl = new LibEqmtDriver.MIPI.Aemulus_DM280e();
                        EqMiPiCtrl.Init(tmp_mipiPair);

                        LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Enable = true;
                        EqmtStatus.MIPI = true;
                    }
                    catch (Exception ex)
                    {
                        LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Enable = false;
                        EqmtStatus.MIPI = false;
                        MessageBox.Show("DM280E MIPI cards not detected, please check!", ex.ToString());
                        return;
                    }
                    break;
                case "DM482E":
                    try
                    {
                        LibEqmtDriver.MIPI.Lib_Var.ºmyDM482Address = MIPIaddr;
                        LibEqmtDriver.MIPI.Lib_Var.ºSlaveAddress = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Slave_Address"), 16);
                        LibEqmtDriver.MIPI.Lib_Var.ºChannelUsed = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Channel_Used"));
                        //PM Trigger 
                        LibEqmtDriver.MIPI.Lib_Var.ºPMTrig = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "PM_Trig"), 16);
                        LibEqmtDriver.MIPI.Lib_Var.ºPMTrig_Data = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "PM_Trig_Data"), 16);
                        //Read Function
                        string read = myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Read_Function");
                        LibEqmtDriver.MIPI.Lib_Var.ºReadFunction = (read.ToUpper() == "TRUE" ? true : false);

                        //Init
                        string AemulusePxi_Path = "C:\\Aemulus\\common\\map_file\\";
                        AemulusePxi_Path += AemulusPxi_FileName;
                        LibEqmtDriver.MIPI.Lib_Var.ºHW_Profile = AemulusePxi_Path;
                        EqMiPiCtrl = new LibEqmtDriver.MIPI.Aemulus_DM482e();
                        EqMiPiCtrl.Init(tmp_mipiPair);

                    }
                    catch (Exception ex)
                    {
                        LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Enable = false;
                        EqmtStatus.MIPI = false;
                        MessageBox.Show("DM482E MIPI cards not detected, please check!", ex.ToString());
                        return;
                    }
                    EqmtStatus.MIPI = true;
                    break;
                case "DM482E_VEC":
                    try
                    {
                        LibEqmtDriver.MIPI.Lib_Var.ºmyDM482Address = MIPIaddr;
                        LibEqmtDriver.MIPI.Lib_Var.ºSlaveAddress = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Slave_Address"), 16);
                        LibEqmtDriver.MIPI.Lib_Var.ºChannelUsed = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Channel_Used"));
                        //PM Trigger 
                        LibEqmtDriver.MIPI.Lib_Var.ºPMTrig = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "PM_Trig"), 16);
                        LibEqmtDriver.MIPI.Lib_Var.ºPMTrig_Data = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "PM_Trig_Data"), 16);
                        //Read Function
                        string read = myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Read_Function");
                        LibEqmtDriver.MIPI.Lib_Var.ºReadFunction = (read.ToUpper() == "TRUE" ? true : false);

                        //Init
                        string AemulusePxi_Path = "C:\\Aemulus\\common\\map_file\\";
                        AemulusePxi_Path += AemulusPxi_FileName;
                        LibEqmtDriver.MIPI.Lib_Var.ºHW_Profile = AemulusePxi_Path;
                        EqMiPiCtrl = new LibEqmtDriver.MIPI.Aemulus_DM482e_Vec();
                        EqMiPiCtrl.Init(tmp_mipiPair);

                    }
                    catch (Exception ex)
                    {
                        LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Enable = false;
                        EqmtStatus.MIPI = false;
                        MessageBox.Show("DM482E MIPI (Vector Config) cards not detected, please check!", ex.ToString());
                        return;
                    }
                    EqmtStatus.MIPI = true;
                    break;
                case "NI6570":
                    try
                    {
                        LibEqmtDriver.MIPI.Lib_Var.ºmyNI6570Address = MIPIaddr;
                        LibEqmtDriver.MIPI.Lib_Var.ºSlaveAddress = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Slave_Address"), 16);
                        LibEqmtDriver.MIPI.Lib_Var.ºChannelUsed = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Channel_Used"));
                        //PM Trigger 
                        LibEqmtDriver.MIPI.Lib_Var.ºPMTrig = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "PM_Trig"), 16);
                        LibEqmtDriver.MIPI.Lib_Var.ºPMTrig_Data = Convert.ToInt32(myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "PM_Trig_Data"), 16);
                        //Read Function
                        string read = myUtility.ReadTextFile(LocSetFilePath, "MIPI_Config", "Read_Function");
                        LibEqmtDriver.MIPI.Lib_Var.ºReadFunction = (read.ToUpper() == "TRUE" ? true : false);

                        //Init
                        EqMiPiCtrl = new LibEqmtDriver.MIPI.NI_PXIe6570(tmp_mipiPair);
                    }
                    catch (Exception ex)
                    {
                        LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Enable = false;
                        EqmtStatus.MIPI = false;
                        MessageBox.Show("NI6570 MIPI cards not detected, please check!", ex.ToString());
                        return;
                    }
                    EqmtStatus.MIPI = true;
                    break;
                case "NONE":
                case "NA":
                    EqmtStatus.PM = false;
                    // Do Nothing , equipment not present
                    break;
                default:
                    MessageBox.Show("Equipment MIPI Model : " + MIPImodel.ToUpper(), "Pls ignore if Equipment MIPI not require.");
                    EqmtStatus.PM = false;
                    break;
            }
            #endregion

        }

        public void InstrUnInit()
        {
            if (EqmtStatus.MXG01)
            {
                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                EqSG01 = null;
            }
            if (EqmtStatus.MXG02)
            {
                EqSG02.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                EqSG02 = null;
            }
            if (EqmtStatus.DC)
            {
                EqDC.Init();
                EqDC = null;
            }
            if (EqmtStatus.DC_1CH)
            {
                EqDC_1CH.Init();
                EqDC_1CH = null;
            }
            for (int i = 0; i < totalDCSupply; i++)
            {
                if (EqmtStatus.DCSupply[i])
                {
                    EqDCSupply[i].Init();
                    EqDCSupply[i] = null;
                }
            }

            if (EqmtStatus.Switch)
            {
                EqSwitch = null;
            }
            if (EqmtStatus.SMU)
            {
                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                EqSMU = null;
                EqSMUDriver = null;
            }

            if (EqmtStatus.MXA01)
            {
                EqSA01 = null;
            }
            if (EqmtStatus.MXA02)
            {
                EqSA02 = null;
            }
            if (EqmtStatus.MIPI)
            {
                EqMiPiCtrl.TurnOff_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT TX
                EqMiPiCtrl.TurnOff_VIO(1);      //mipi pair 1 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board / DUT RX
                EqMiPiCtrl.TurnOff_VIO(2);      //mipi pair 2 - DIO 6, DIO 7 and DIO 8 - For future use
                EqMiPiCtrl.TurnOff_VIO(3);      //mipi pair 3 - DIO 9, DIO 10 and DIO 11 - For future use
                EqMiPiCtrl = null;
            }
            if (EqmtStatus.PXI_VST)
            {
                EqVST = null;
            }
        }

        private void ExecuteTest(Dictionary<string, string> TestPara, ref ATFReturnResult results)
        {
            #region Read TCF Setting
            //Read TCF Setting

            string StrError = string.Empty;

            int ºTestNum = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestNum));       //use as array number for data store
            string ºTestMode = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestMode);
            string ºTestParam = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestParam);
            string ºTestParaName = myUtility.ReadTcfData(TestPara, TCF_Header.ConstParaName);
            string ºTestUsePrev = myUtility.ReadTcfData(TestPara, TCF_Header.ConstUsePrev);

            //Single Freq Condition
            float ºTXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTXFreq));
            float ºRXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRXFreq));
            float ºPout = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout));
            float ºPin = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin));
            string ºTXBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTXBand);
            string ºRXBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRXBand);
            bool ºTunePwr_TX = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX).ToUpper() == "V" ? true : false);

            //Sweep TX1/RX1 Freq Condition
            float ºPout1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout1));
            float ºPin1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin1));
            float ºStartTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartTXFreq1));
            float ºStopTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopTXFreq1));
            float ºStepTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepTXFreq1));
            float ºDwellT1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDwellTime1));
            bool ºTunePwr_TX1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX1).ToUpper() == "V" ? true : false);

            float ºStartRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq1));
            float ºStopRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq1));
            float ºStepRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepRXFreq1));
            float ºRX1SweepT = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX1SweepT));

            string ºTX1Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTX1Band);
            string ºRX1Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX1Band);
            bool ºSetRX1NDiag = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetRX1NDiag).ToUpper() == "V" ? true : false);

            //Sweep TX2/RX2 Freq Condition
            float ºPout2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout2));
            float ºPin2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin2));
            float ºStartTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartTXFreq2));
            float ºStopTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopTXFreq2));
            float ºStepTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepTXFreq2));
            float ºDwellT2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDwellTime2));
            bool ºTunePwr_TX2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX2).ToUpper() == "V" ? true : false);

            float ºStartRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq2));
            float ºStopRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq2));
            float ºStepRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepRXFreq2));
            float ºRX2SweepT = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX2SweepT));

            string ºTX2Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTX2Band);
            string ºRX2Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX2Band);
            bool ºSetRX2NDiag = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetRX2NDiag).ToUpper() == "V" ? true : false);

            //Misc
            string ºPXI_MultiRBW = myUtility.ReadTcfData(TestPara, TCF_Header.PXI_MultiRBW);
            int ºPXI_NoOfSweep = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.PXI_NoOfSweep));
            string ºPoutTolerance = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPoutTolerance);
            string ºPinTolerance = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPinTolerance);
            string ºPowerMode = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPowerMode);
            string ºCalTag = myUtility.ReadTcfData(TestPara, TCF_Header.ConstCalTag);
            string ºSwBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSwBand);
            string ºModulation = myUtility.ReadTcfData(TestPara, TCF_Header.ConstModulation);
            string ºWaveFormName = myUtility.ReadTcfData(TestPara, TCF_Header.ConstWaveformName);
            bool ºSetFullMod = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetFullMod).ToUpper() == "V" ? true : false);

            //Read TCF SMU Setting
            float[] ºSMUVCh;
            ºSMUVCh = new float[9];
            float[] ºSMUILimitCh;
            ºSMUILimitCh = new float[9];

            string ºSMUSetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUSetCh);
            string ºSMUMeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUMeasCh);
            ºSMUVCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh0));
            ºSMUVCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh1));
            ºSMUVCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh2));
            ºSMUVCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh3));
            ºSMUVCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh4));
            ºSMUVCh[5] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh5));
            ºSMUVCh[6] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh6));
            ºSMUVCh[7] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh7));
            ºSMUVCh[8] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh8));

            ºSMUILimitCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh0Limit));
            ºSMUILimitCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh1Limit));
            ºSMUILimitCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh2Limit));
            ºSMUILimitCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh3Limit));
            ºSMUILimitCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh4Limit));
            ºSMUILimitCh[5] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh5Limit));
            ºSMUILimitCh[6] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh6Limit));
            ºSMUILimitCh[7] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh7Limit));
            ºSMUILimitCh[8] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh8Limit));

            //Read TCF DC Setting
            float[] ºDCVCh;
            ºDCVCh = new float[5];
            float[] ºDCILimitCh;
            ºDCILimitCh = new float[5];

            string ºDCSetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCSetCh);
            string ºDCMeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCMeasCh);
            ºDCVCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh1));
            ºDCVCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh2));
            ºDCVCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh3));
            ºDCVCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh4));
            ºDCILimitCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh1Limit));
            ºDCILimitCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh2Limit));
            ºDCILimitCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh3Limit));
            ºDCILimitCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh4Limit));

            //MIPI
            int ºMiPi_RegNo = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegNo));
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg0 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg0);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg1);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg2 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg2);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg3 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg3);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg4 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg4);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg5 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg5);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg6 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg6);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg7 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg7);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg8 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg8);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg9 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg9);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegA = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegA);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegB = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegB);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegC = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegC);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegD = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegD);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegE = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegE);
            LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegF = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegF);

            //Read Set Equipment Flag
            bool ºSetSA1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSA1).ToUpper() == "V" ? true : false);
            bool ºSetSA2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSA2).ToUpper() == "V" ? true : false);
            bool ºSetSG1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSG1).ToUpper() == "V" ? true : false);
            bool ºSetSG2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSG2).ToUpper() == "V" ? true : false);
            bool ºSetSMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSMU).ToUpper() == "V" ? true : false);

            //Read Off State Flag
            bool ºOffSG1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSG1).ToUpper() == "V" ? true : false);
            bool ºOffSG2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSG2).ToUpper() == "V" ? true : false);
            bool ºOffSMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSMU).ToUpper() == "V" ? true : false);
            bool ºOffDC = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffDC).ToUpper() == "V" ? true : false);

            //Read Require test parameter
            bool ºTest_Pin = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin).ToUpper() == "V" ? true : false);
            bool ºTest_Pout = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout).ToUpper() == "V" ? true : false);
            bool ºTest_Pin1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin1).ToUpper() == "V" ? true : false);
            bool ºTest_Pout1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout1).ToUpper() == "V" ? true : false);
            bool ºTest_Pin2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin2).ToUpper() == "V" ? true : false);
            bool ºTest_Pout2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout2).ToUpper() == "V" ? true : false);
            bool ºTest_NF1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_NF1).ToUpper() == "V" ? true : false);
            bool ºTest_NF2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_NF2).ToUpper() == "V" ? true : false);
            bool ºTest_MXATrace = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MXATrace).ToUpper() == "V" ? true : false);
            bool ºTest_MXATraceFreq = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MXATraceFreq).ToUpper() == "V" ? true : false);
            bool ºTest_Harmonic = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Harmonic).ToUpper() == "V" ? true : false);
            bool ºTest_IMD = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_IMD).ToUpper() == "V" ? true : false);
            bool ºTest_MIPI = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MIPI).ToUpper() == "V" ? true : false);
            bool ºTest_SMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_SMU).ToUpper() == "V" ? true : false);
            bool ºTest_DCSupply = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_DCSupply).ToUpper() == "V" ? true : false);
            bool ºTest_Switch = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Switch).ToUpper() == "V" ? true : false);
            bool ºTest_TestTime = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_TestTime).ToUpper() == "V" ? true : false);

            //Read SA & SG setting
            string ºSA1att = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSA1att);
            string ºSA2att = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSA2att);
            double ºSG1MaxPwr = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG1MaxPwr));
            double ºSG2MaxPwr = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG2MaxPwr));
            double ºSG1_DefaultFreq = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG1_DefaultFreq));
            double ºSG2_DefaultFreq = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG2_DefaultFreq));
            double ºPXI_Multiplier_RXIQRate = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMultiplier_RXIQRate));

            //Read Delay Setting
            int ºTrig_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTrig_Delay));
            int ºGeneric_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstGeneric_Delay));
            int ºRdCurr_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRdCurr_Delay));
            int ºRdPwr_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRdPwr_Delay));
            int ºSetup_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetup_Delay));
            int ºStartSync_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartSync_Delay));
            int ºStopSync_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopSync_Delay));
            int ºEstimate_TestTime = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstEstimate_TestTime));

            //Misc Setting
            string ºSearch_Method = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Method);
            //float ºSearch_Value = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Value));
            string ºSearch_Value = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Value);
            bool ºInterpolation = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstInterpolation).ToUpper() == "V" ? true : false);
            bool ºAbs_Value = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstAbs_Value).ToUpper() == "V" ? true : false);
            bool ºSave_MXATrace = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSave_MXATrace).ToUpper() == "V" ? true : false);

            //NF Setting -Seoul
            string ºSwBand_HotNF = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSwitching_Band_HotNF);
            double ºNF_BW = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_BW));
            double ºNF_REFLevel = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_REFLEVEL));
            double ºNF_SweepTime = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_SWEEPTIME));
            int ºNF_Average = Convert.ToInt32(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_AVERAGE));
            string ºNF_CalTag = myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_CalTag);
            double ºNF_SoakTime = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_SoakTime));
            double ºNF_Cal_HL = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_Cal_HL));
            double ºNF_Cal_LL = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_Cal_LL));
            int TestUsePrev_ArrayNo = 0;

            //MIPI Voltage and current setting
            //Read TCF MIPI Voltage and current Setting
            float[] ºMIPI_VSetCh;
            ºMIPI_VSetCh = new float[3];
            float[] ºMIPI_ILimitCh;
            ºMIPI_ILimitCh = new float[3];

            string ºMIPI_SetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPISetCh);
            string ºMIPI_MeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIMeasCh);

            try
            {
                ºMIPI_VSetCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSclk));
                ºMIPI_VSetCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSdata));
                ºMIPI_VSetCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSvio));

                ºMIPI_ILimitCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISclk));
                ºMIPI_ILimitCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISdata));
                ºMIPI_ILimitCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISvio));
            }
            catch (Exception)
            {
                //optional .. will be required to set in TCF if needed
                ºMIPI_VSetCh[0] = 0;
                ºMIPI_VSetCh[1] = 0;
                ºMIPI_VSetCh[2] = 0;

                ºMIPI_ILimitCh[0] = 0;
                ºMIPI_ILimitCh[1] = 0;
                ºMIPI_ILimitCh[2] = 0;
            }
 
            string Tmp1stHeader = null;
            string Tmp2ndHeader = null;
            string[] TmpParamName;
            string TxPAOnScript = "";
            double ºStepTXFreq = 0;

            string[] SetSMU;
            string[] MeasSMU;
            double[] R_SMU_ICh;
            string[] R_SMULabel_ICh;
            R_SMU_ICh = new double[9];
            R_SMULabel_ICh = new string[9];

            string[] SetDC;
            string[] MeasDC;
            double[] R_DC_ICh;
            string[] R_DCLabel_ICh;
            R_DC_ICh = new double[5];
            R_DCLabel_ICh = new string[5];
            string[] SetSMUSelect;

            bool MIPI_Read_Successful = false;

            //temp result storage use for MAX , MIN etc calculation 
            Results[TestCount].Multi_Results = new s_mRslt[15];      //default to 15 , need to check total enum of e_ResultTag
            Results[TestCount].TestNumber = ºTestNum;
            Results[TestCount].Enable = true;

            //MIPI Voltage and current setting
            string[] VSetMIPI;
            string[] IMeasMIPI;
            double[] R_MIPI_ICh;
            string[] R_MIPILabel_ICh;
            R_MIPI_ICh = new double[3];
            R_MIPILabel_ICh = new string[3];

            #endregion

            //Load cal factor
            #region Cal Factor
            double ºLossCouplerPath = 999;
            double ºLossOutputPathRX1 = 999;
            double ºLossOutputPathRX2 = 999;
            double ºLossInputPathSG1 = 999;
            double ºLossInputPathSG2 = 999;

            #endregion

            InitResultVariable();

            //Test Variable
            #region Test Variable
            bool status = false;
            bool pwrSearch = false;
            int Index = 0;
            int tx1_noPoints = 0;
            int rx1_noPoints = 0;
            float tx1_span = 0;
            double rx1_span = 0;
            double rx1_cntrfreq = 0;
            double rx2_span = 0;
            double rx2_cntrfreq = 0;
            double totalInputLoss = 0;      //Input Pathloss + Testboard Loss
            double totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
            double totalRXLoss = 0;     //RX Pathloss + Testboard Loss
            double tolerancePwr = 0;

            //mxa#1 and mxa#2 setting variable
            int rx1_mxa_nopts = 0;
            double rx1_mxa_nopts_step = 0.1;        //step 0.1MHz , example mxa_nopts (601) , every points = 0.1MHz
            int rx2_mxa_nopts = 0;
            double rx2_mxa_nopts_step = 0.1;        //step 0.1MHz , example mxa_nopts (601) , every points = 0.1MHz

            string MkrCalSegmTag = null;
            string CalSegmData = null;
            double tbInputLoss = 0;
            double tbOutputLoss = 0;
            string MXA_Config = null;
            int markerNo = 1;

            int count;
            int txcount;
            int rxcount;
            double tmpInputLoss = 0;
            double tmpCouplerLoss = 0;
            double tmpAveInputLoss = 0;
            double tmpAveCouplerLoss = 0;
            double tmpRxLoss = 0;
            double tmpAveRxLoss = 0;
            double tmpMkrNoiseLoss = 0;
            double tmpAveMkrNoiseLoss = 0;
            double mkrNoiseLoss = 0;
            double AveMkrNoiseLossRX1 = 0;
            double AveMkrNoiseLossRX2 = 0;

            long paramTestTime = 0;
            long syncTest_Delay = 0;
            decimal trigDelay = 0;

            //COMMON case variable
            int resultTag;
            int arrayVal;
            double result;
            bool usePrevRslt = false;
            double prevRslt = 0;
            bool b_mipiTKey = false;

            //VST Variable
            double SG_IQRate = 0;

            #endregion

            #region Misc Setup Variable
            int istep;
            int indexdata = 0;

            double[] tx_freqArray;
            double[] rx_freqArray;
            double[] contactPwr_Array;
            double[] nfAmpl_Array;
            double[] nfAmplFreq_Array;

            //Variable use in VST Measure Function
            int NumberOfRuns = 5;
            double SGPowerLevel = -18;// -18 CDMA dBm //-20 LTE dBm  
            double SAReferenceLevel = -20;
            double SoakTime = 450e-3;
            double SoakFrequency = ºStartTXFreq1 * 1e6;
            double vBW_Hz = 300;
            double RBW_Hz = 1e6;
            bool preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
            int preSoakSweepTemp = 0;
            double stepFreqMHz = 0;
            double tmpRXFreqHz = 0;
            int sweepPts = 0;

            //Variable for NF Result Fetch
            string[] TestUsePrev_Array;
            int NF_TestCount;
            int ColdNF_TestCount;
            int HotNF_TestCount;

            int Nop_NF;
            int Nop_ColdNF;
            int Nop_HotNF;
            int NumberOfRunsNF;
            int NumberOfRunsColdNF;
            int NumberOfRunsHotNF;
            double[] RXPathLoss_NF;
            double[] RXPathLoss_Cold;
            double[] RXPathLoss_Hot;

            s_TraceNo ResultMultiTrace_NF;
            s_TraceNo ResultMultiTrace_ColdNF;
            s_TraceNo ResultMultiTrace_HotNF;
            s_TraceNo ResultMultiTraceDelta;

            Dictionary<double, double> Dic_NF;
            Dictionary<double, double> Dic_ColdNF;
            Dictionary<double, double> Dic_HotNF;

            double MaxNFAmpl;
            double MaxNFFreq;
            double MaxColdNFAmpl;
            double MaxColdNFFreq;
            double MaxHotNFAmpl;
            double MaxHotNFFreq;
            double MaxNFRiseAmpl;
            double MaxNFRiseFreq;
            double CalcData;

            #endregion

            Stopwatch tTime = new Stopwatch();

            tTime.Reset();
            tTime.Start();

       

            #region TEST
            switch (ºTestMode.ToUpper())
            {
                case "MXA_TRACE":
                    switch (ºTestParam.ToUpper())
                    {
                        case "CALC_MXA_TRACE":
                            #region Calculate MXA Trace

                            int mxaNo = 1;
                            int traceNo = 1;
                            double mxaTrace_Ampl = -999;

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], ºCalTag.ToUpper(), ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            if (ºTest_NF1)
                            {
                                mxaTrace_Ampl = -999;
                                mxaNo = 1;
                                Read_MXA_MultiTrace(mxaNo, traceNo, ºTestUsePrev, ºStartRXFreq1, ºStopRXFreq1, ºStepRXFreq1, ºSearch_Method, ºTestParam, out R_NF1_Freq, out R_NF1_Ampl);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref ºLossOutputPathRX1, ref StrError);
                                R_NF1_Ampl = R_NF1_Ampl - ºLossOutputPathRX1 - tbOutputLoss;

                                if (ºTest_MXATrace)
                                {
                                    for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                    {
                                        mxaTrace_Ampl = Result_MXATrace.Ampl[i] - ºLossOutputPathRX1 - tbOutputLoss;    //use same pathloss as previous data (from "Read_MXA_MultiTrace" function)

                                        BuildResults(ref results, "P" + i + "_" + ºTestParaName + "_RX" + ºRX1Band + "_Ampl", "dBm", mxaTrace_Ampl);

                                        if (ºTest_MXATraceFreq)
                                        {
                                            BuildResults(ref results, "P" + i + "_" + ºTestParaName + "_RX" + ºRX1Band + "_Freq", "MHz", Result_MXATrace.FreqMHz[i]);
                                        }
                                    }
                                }
                            }
                            if (ºTest_NF2)
                            {
                                mxaTrace_Ampl = -999;
                                mxaNo = 2;
                                Read_MXA_MultiTrace(mxaNo, traceNo, ºTestUsePrev, ºStartRXFreq2, ºStopRXFreq2, ºStepRXFreq2, ºSearch_Method, ºTestParam, out R_NF2_Freq, out R_NF2_Ampl);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX2CalSegm, R_NF2_Freq, ref ºLossOutputPathRX2, ref StrError);
                                R_NF2_Ampl = R_NF2_Ampl - ºLossOutputPathRX2 - tbOutputLoss;

                                if (ºTest_MXATrace)
                                {
                                    for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                    {
                                        mxaTrace_Ampl = Result_MXATrace.Ampl[i] - ºLossOutputPathRX2 - tbOutputLoss;    //use same pathloss as previous data (from "Read_MXA_MultiTrace" function)

                                        BuildResults(ref results, "P" + i + "_" + ºTestParaName + "_RX" + ºRX2Band + "_Ampl", "dBm", mxaTrace_Ampl);

                                        if (ºTest_MXATraceFreq)
                                        {
                                            BuildResults(ref results, "P" + i + "_" + ºTestParaName + "_RX" + ºRX2Band + "_Freq", "MHz", Result_MXATrace.FreqMHz[i]);
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        case "MERIT_FIGURE":
                            #region Figure of merit calculation

                            double tmpFreqMhz = -999;
                            double tmpAmpl = -999;
                            double count_FOM = 0;
                            double percent_FOM = -999;
                            double totalPts = -999;

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], ºCalTag.ToUpper(), ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            if (ºTest_NF1)
                            {
                                count_FOM = 0;
                                percent_FOM = -999;
                                mxaTrace_Ampl = -999;
                                mxaNo = 1;
                                traceNo = 1;
                                Read_MXA_MultiTrace(mxaNo, traceNo, ºTestUsePrev, ºStartRXFreq1, ºStopRXFreq1, ºStepRXFreq1, ºSearch_Method, ºTestParam, out tmpFreqMhz, out tmpAmpl);
                                totalPts = Result_MXATrace.FreqMHz.Length - 1;

                                for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                {
                                    ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, Result_MXATrace.FreqMHz[i], ref ºLossOutputPathRX1, ref StrError);
                                    mxaTrace_Ampl = Result_MXATrace.Ampl[i] - ºLossOutputPathRX1 - tbOutputLoss;

                                    switch (ºSearch_Method.ToUpper())
                                    {
                                        case "MAX":
                                            if (mxaTrace_Ampl >= Convert.ToSingle(ºSearch_Value))
                                            {
                                                count_FOM++;
                                            }
                                            break;

                                        case "MIN":
                                            if (mxaTrace_Ampl <= Convert.ToSingle(ºSearch_Value))
                                            {
                                                count_FOM++;
                                            }
                                            break;

                                        default:
                                            MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                            break;
                                    }
                                }

                                percent_FOM = Math.Round(((count_FOM / totalPts) * 100), 3);
                                R_NF1_Ampl = percent_FOM;
                                R_NF1_Freq = 1;     //dummy data
                            }

                            if (ºTest_NF2)
                            {
                                count_FOM = 0;
                                percent_FOM = -999;
                                mxaTrace_Ampl = -999;
                                mxaNo = 2;
                                traceNo = 1;
                                Read_MXA_MultiTrace(mxaNo, traceNo, ºTestUsePrev, ºStartRXFreq2, ºStopRXFreq2, ºStepRXFreq2, ºSearch_Method, ºTestParam, out tmpFreqMhz, out tmpAmpl);

                                for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                {
                                    ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX2CalSegm, Result_MXATrace.FreqMHz[i], ref ºLossOutputPathRX2, ref StrError);
                                    mxaTrace_Ampl = Result_MXATrace.Ampl[i] - ºLossOutputPathRX2 - tbOutputLoss;

                                    switch (ºSearch_Method.ToUpper())
                                    {
                                        case "MAX":
                                            if (mxaTrace_Ampl >= Convert.ToSingle(ºSearch_Value))
                                            {
                                                count_FOM++;
                                            }
                                            break;

                                        case "MIN":
                                            if (mxaTrace_Ampl <= Convert.ToSingle(ºSearch_Value))
                                            {
                                                count_FOM++;
                                            }
                                            break;

                                        default:
                                            MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                            break;
                                    }
                                }

                                percent_FOM = (count_FOM / Result_MXATrace.FreqMHz.Length) * 100;
                                R_NF2_Ampl = percent_FOM;
                                R_NF2_Freq = 1;             //dummy data 
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + ºTestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;

                case "PXI_TRACE":
                    switch (ºTestParam.ToUpper())
                    {
                        case "CALC_PXI_TRACE":
                            #region Calculate PXI Trace

                            double pxiTrace_Ampl = -999;
                            tmpAveRxLoss = 0;
                            tmpRxLoss = 0;
                            count = 0;

                            #region decode re-arrange multiple bandwidth (Ascending)
                            int bw_cnt = 0;
                            double[] tmpRBW_Hz = Array.ConvertAll(ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                            double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                            Array.Sort(tmpRBW_Hz);
                            foreach (double key in tmpRBW_Hz)
                            {
                                multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                bw_cnt++;
                            }

                            multiRBW_cnt = multiRBW_Hz.Length;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], ºCalTag.ToUpper(), ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            //Get average pathloss base on start and stop freq with 1MHz step freq
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / 1);
                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                            }
                            tmpAveRxLoss = tmpRxLoss / (count + 1);

                            if (ºTest_NF1)
                            {
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    pxiTrace_Ampl = -999;

                                    Read_PXI_MultiTrace(ºTestUsePrev, ºStartRXFreq1, ºStopRXFreq1, ºStepRXFreq1, ºSearch_Method, ºTestParam, out R_NF1_Freq, out R_NF1_Ampl, rbw_counter, multiRBW_Hz[rbw_counter]);
                                    R_NF1_Ampl = R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss;

                                    if (ºTest_MXATrace)
                                    {
                                        for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                        {
                                            pxiTrace_Ampl = Result_MXATrace.Ampl[i] - tmpAveRxLoss - tbOutputLoss;    //use same pathloss as previous data (from "Read_PXI_MultiTrace" function)

                                            BuildResults(ref results, "P" + i + "_" + ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", pxiTrace_Ampl);

                                            if (ºTest_MXATraceFreq)
                                            {
                                                BuildResults(ref results, "P" + i + "_" + ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", Result_MXATrace.FreqMHz[i]);
                                            }
                                        }
                                    }

                                    if (ºTest_NF1)
                                    {
                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            ºTest_NF1 = false;

                            #endregion
                            break;

                        case "MERIT_FIGURE":
                            #region Figure of merit calculation

                            double tmpFreqMhz = -999;
                            double tmpAmpl = -999;
                            double count_FOM = 0;
                            double percent_FOM = -999;
                            double totalPts = -999;
                            tmpAveRxLoss = 0;
                            tmpRxLoss = 0;
                            count = 0;

                            #region decode re-arrange multiple bandwidth (Ascending)
                            bw_cnt = 0;
                            tmpRBW_Hz = Array.ConvertAll(ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                            multiRBW_Hz = new double[tmpRBW_Hz.Length];

                            Array.Sort(tmpRBW_Hz);
                            foreach (double key in tmpRBW_Hz)
                            {
                                multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                bw_cnt++;
                            }

                            multiRBW_cnt = multiRBW_Hz.Length;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], ºCalTag.ToUpper(), ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            //Get average pathloss base on start and stop freq with 1MHz step freq
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / 1);
                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                            }
                            tmpAveRxLoss = tmpRxLoss / (count + 1);

                            if (ºTest_NF1)
                            {
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    count_FOM = 0;
                                    percent_FOM = -999;
                                    pxiTrace_Ampl = -999;

                                    Read_PXI_MultiTrace(ºTestUsePrev, ºStartRXFreq1, ºStopRXFreq1, ºStepRXFreq1, ºSearch_Method, ºTestParam, out tmpFreqMhz, out tmpAmpl, rbw_counter, multiRBW_Hz[rbw_counter]);
                                    totalPts = Result_MXATrace.FreqMHz.Length - 1;

                                    for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                    {
                                        pxiTrace_Ampl = Result_MXATrace.Ampl[i] - tmpAveRxLoss - tbOutputLoss;

                                        switch (ºSearch_Method.ToUpper())
                                        {
                                            case "MAX":
                                                if (pxiTrace_Ampl >= Convert.ToSingle(ºSearch_Value))
                                                {
                                                    count_FOM++;
                                                }
                                                break;

                                            case "MIN":
                                                if (pxiTrace_Ampl <= Convert.ToSingle(ºSearch_Value))
                                                {
                                                    count_FOM++;
                                                }
                                                break;

                                            default:
                                                MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                break;
                                        }
                                    }

                                    percent_FOM = Math.Round(((count_FOM / totalPts) * 100), 3);
                                    R_NF1_Ampl = percent_FOM;
                                    R_NF1_Freq = 1;     //dummy data

                                    if (ºTest_NF1)
                                    {
                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            ºTest_NF1 = false;

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        case "MAX_MIN":
                            #region MAX MIN calculation
                            tmpFreqMhz = -999;
                            tmpAmpl = -999;
                            indexdata = 0;
                            tmpAveRxLoss = 0;
                            tmpRxLoss = 0;
                            count = 0;

                            #region decode re-arrange multiple bandwidth (Ascending)
                            bw_cnt = 0;
                            tmpRBW_Hz = Array.ConvertAll(ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                            multiRBW_Hz = new double[tmpRBW_Hz.Length];

                            Array.Sort(tmpRBW_Hz);
                            foreach (double key in tmpRBW_Hz)
                            {
                                multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                bw_cnt++;
                            }

                            multiRBW_cnt = multiRBW_Hz.Length;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], ºCalTag.ToUpper(), ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            //Get average pathloss base on start and stop freq with 1MHz step freq
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / 1);
                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                            }
                            tmpAveRxLoss = tmpRxLoss / (count + 1);

                            if (ºTest_NF1)
                            {
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    Read_PXI_MultiTrace(ºTestUsePrev, ºStartRXFreq1, ºStopRXFreq1, ºStepRXFreq1, ºSearch_Method, ºTestParam, out tmpFreqMhz, out tmpAmpl, rbw_counter, multiRBW_Hz[rbw_counter]);

                                    switch (ºSearch_Method.ToUpper())
                                    {
                                        case "MAX":
                                            R_NF1_Ampl = Result_MXATrace.Ampl.Max();
                                            indexdata = Array.IndexOf(Result_MXATrace.Ampl, R_NF1_Ampl);     //return index of max value
                                            R_NF1_Freq = Result_MXATrace.FreqMHz[indexdata];

                                            R_NF1_Ampl = R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss;
                                            break;

                                        case "MIN":
                                            R_NF1_Ampl = Result_MXATrace.Ampl.Min();
                                            indexdata = Array.IndexOf(Result_MXATrace.Ampl, R_NF1_Ampl);     //return index of min value
                                            R_NF1_Freq = Result_MXATrace.FreqMHz[indexdata];

                                            R_NF1_Ampl = R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss;
                                            break;

                                        default:
                                            MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                            break;
                                    }

                                    if (ºTest_NF1)
                                    {
                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            ºTest_NF1 = false;
                            break;
                            #endregion

                        case "TRACE_MERIT_FIGURE":
                            #region Individual Trace Figure of merit calculation

                            tmpFreqMhz = -999;
                            tmpAmpl = -999;
                            count_FOM = 0;
                            percent_FOM = -999;
                            totalPts = -999;
                            tmpAveRxLoss = 0;
                            tmpRxLoss = 0;
                            count = 0;

                            //if excluded soak sweep trace , need to remove the array[0] from PXITrace[testnumber].Multi_Trace[0]
                            bool excludeSoakSweep = false;
                            int traceCount = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(ºTestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    excludeSoakSweep = PXITrace[i].SoakSweep;
                                    traceCount = PXITrace[i].TraceCount;
                                }
                            }

                            #region decode re-arrange multiple bandwidth (Ascending)
                            bw_cnt = 0;
                            tmpRBW_Hz = Array.ConvertAll(ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                            multiRBW_Hz = new double[tmpRBW_Hz.Length];

                            Array.Sort(tmpRBW_Hz);
                            foreach (double key in tmpRBW_Hz)
                            {
                                multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                bw_cnt++;
                            }

                            multiRBW_cnt = multiRBW_Hz.Length;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], ºCalTag.ToUpper(), ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq with 1MHz step freq
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / 1);
                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                            }
                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            #endregion

                            if (ºTest_NF1)
                            {
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    for (int traceNo = 0; traceNo < traceCount; traceNo++)
                                    {
                                        string rbwParamName = null;
                                        string traceName = null;

                                        if ((excludeSoakSweep) && (traceNo == 0))
                                        {
                                            traceName = "Soak";
                                        }
                                        else
                                        {
                                            traceName = (traceNo + 1).ToString();
                                        }

                                        rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz" + "_TR" + traceName;

                                        count_FOM = 0;
                                        percent_FOM = -999;
                                        pxiTrace_Ampl = -999;

                                        Read_PXI_SingleTrace(ºTestUsePrev, traceNo, ºStartRXFreq1, ºStopRXFreq1, ºStepRXFreq1, ºSearch_Method, ºTestParam, rbw_counter, multiRBW_Hz[rbw_counter]);
                                        totalPts = Result_MXATrace.FreqMHz.Length - 1;

                                        for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                        {
                                            pxiTrace_Ampl = Result_MXATrace.Ampl[i] - tmpAveRxLoss - tbOutputLoss;

                                            switch (ºSearch_Method.ToUpper())
                                            {
                                                case "MAX":
                                                    if (pxiTrace_Ampl >= Convert.ToSingle(ºSearch_Value))
                                                    {
                                                        count_FOM++;
                                                    }
                                                    break;

                                                case "MIN":
                                                    if (pxiTrace_Ampl <= Convert.ToSingle(ºSearch_Value))
                                                    {
                                                        count_FOM++;
                                                    }
                                                    break;

                                                default:
                                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                    break;
                                            }
                                        }

                                        percent_FOM = Math.Round(((count_FOM / totalPts) * 100), 3);
                                        R_NF1_Ampl = percent_FOM;
                                        R_NF1_Freq = 1;     //dummy data

                                        if (ºTest_NF1)
                                        {
                                            BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                            BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                        }
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            ºTest_NF1 = false;

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        case "NF_MAX_MIN":
                            #region NF_MAX MIN calculation

                            TestUsePrev_Array = ºTestUsePrev.Split(',');
                            ColdNF_TestCount = 0;
                            HotNF_TestCount = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(TestUsePrev_Array[0]) == PXITrace[i].TestNumber)
                                {
                                    ColdNF_TestCount = i;
                                }

                                if (Convert.ToInt16(TestUsePrev_Array[1]) == PXITrace[i].TestNumber)
                                {
                                    HotNF_TestCount = i;
                                }
                            }

                            Nop_ColdNF = PXITrace[ColdNF_TestCount].Multi_Trace[0][0].NoPoints;
                            Nop_HotNF = PXITrace[HotNF_TestCount].Multi_Trace[0][0].NoPoints;
                            NumberOfRunsColdNF = PXITrace[ColdNF_TestCount].TraceCount;
                            NumberOfRunsHotNF = PXITrace[HotNF_TestCount].TraceCount;
                            RXPathLoss_Cold = new double[Nop_ColdNF];
                            RXPathLoss_Hot = new double[Nop_HotNF];

                            Cold_NF_new = new double[NumberOfRunsColdNF][];
                            Cold_NoisePower_new = new double[NumberOfRunsColdNF][];
                            Hot_NF_new = new double[NumberOfRunsHotNF][];
                            Hot_NoisePower_new = new double[NumberOfRunsHotNF][];

                            ResultMultiTrace_ColdNF = new s_TraceNo();
                            ResultMultiTrace_ColdNF.Ampl = new double[Nop_ColdNF];
                            ResultMultiTrace_ColdNF.FreqMHz = new double[Nop_ColdNF];

                            ResultMultiTrace_HotNF = new s_TraceNo();
                            ResultMultiTrace_HotNF.Ampl = new double[Nop_HotNF];
                            ResultMultiTrace_HotNF.FreqMHz = new double[Nop_HotNF];

                            ResultMultiTraceDelta = new s_TraceNo();
                            ResultMultiTraceDelta.Ampl = new double[Nop_HotNF];
                            ResultMultiTraceDelta.FreqMHz = new double[Nop_HotNF];

                            Dic_ColdNF = new Dictionary<double, double>();
                            Dic_HotNF = new Dictionary<double, double>();

                            MaxColdNFAmpl = 0;
                            MaxColdNFFreq = 0;
                            MaxHotNFAmpl = 0;
                            MaxHotNFFreq = 0;
                            MaxNFRiseAmpl = 0;
                            MaxNFRiseFreq = 0;
                            CalcData = 0;

                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Cold_NF_new[i] = new double[Nop_ColdNF];
                                Cold_NoisePower_new[i] = new double[Nop_ColdNF];
                            }

                            for (int i = 0; i < NumberOfRunsHotNF; i++)
                            {
                                Hot_NF_new[i] = new double[Nop_HotNF];
                                Hot_NoisePower_new[i] = new double[Nop_HotNF];
                            }

                            // Cold NF RX path loss gathering
                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                ºRXFreq = Convert.ToSingle(PXITrace[ColdNF_TestCount].Multi_Trace[0][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                RXPathLoss_Cold[i] = ºLossOutputPathRX1;
                            }

                            // Hot NF RX path loss gathering
                            for (int i = 0; i < Nop_HotNF; i++)
                            {
                                ºRXFreq = Convert.ToSingle(PXITrace[HotNF_TestCount].Multi_Trace[0][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                RXPathLoss_Hot[i] = ºLossOutputPathRX1;
                            }

                            // Cold NF Fetch    
                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                EqRFmx.RetrieveResults_NFColdSource(ColdNF_TestCount, 0, "result::" + "COLD" + ColdNF_TestCount.ToString() + "_" + i);

                                for (int j = 0; j < Nop_ColdNF; j++)
                                {
                                    double Cold_NF_withoutGain = (EqRFmx.dutNoiseFigure[j].ToString().Contains("NaN") || EqRFmx.dutNoiseFigure[j].ToString().Contains("Infinity")) ? 9999 : EqRFmx.dutNoiseFigure[j];

                                    if (Cold_NF_withoutGain == 9999 || PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                    {
                                        Cold_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Cold_NF_new[i][j] = Cold_NF_withoutGain - (PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j]);
                                    }

                                    Cold_NoisePower_new[i][j] = EqRFmx.coldSourcePower[j] - RXPathLoss_Cold[j];

                                }
                            }

                            // Hot NF Fetch
                            for (int i = 0; i < NumberOfRunsHotNF; i++)
                            {
                                for (int j = 0; j < Nop_HotNF; j++)
                                {
                                    EqRFmx.RetrieveResults_NFColdSource(HotNF_TestCount, j, "result::" + "HOT" + HotNF_TestCount + "_" + i.ToString() + "_" + j.ToString());

                                    double Hot_NF_withoutGain = (EqRFmx.dutNoiseFigure[0].ToString().Contains("NaN") || EqRFmx.dutNoiseFigure[0].ToString().Contains("Infinity")) ? 9999 : EqRFmx.dutNoiseFigure[0];

                                    if (Hot_NF_withoutGain == 9999 || PXITrace[HotNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[HotNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                    {
                                        Hot_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Hot_NF_new[i][j] = Hot_NF_withoutGain - (PXITrace[HotNF_TestCount].Multi_Trace[0][i].RxGain[j]);
                                    }

                                    Hot_NoisePower_new[i][j] = EqRFmx.coldSourcePower[0] - RXPathLoss_Hot[j];

                                }
                            }

                            // Store Cold & Hot NF data into PXI Trace
                            StoreNFdata(ColdNF_TestCount, NumberOfRunsColdNF, Nop_ColdNF, Cold_NF_new);
                            StoreNFdata(HotNF_TestCount, NumberOfRunsHotNF, Nop_HotNF, Hot_NF_new);
                            StoreNFRisedata(TestCount, ColdNF_TestCount, HotNF_TestCount, NumberOfRunsColdNF, NumberOfRunsHotNF, Nop_ColdNF, Nop_HotNF, ºTestParaName, ºTestNum);

                            // Save Cold & Hot NF Trace if Save_MXATrace is enabled
                            Save_PXI_NF_TraceRaw(ºTestParaName + "_Cold-NF", ColdNF_TestCount, ºSave_MXATrace, 0, PXITrace[ColdNF_TestCount].Multi_Trace[0][0].RBW_Hz);
                            Save_PXI_NF_TraceRaw(ºTestParaName + "_Hot-NF", HotNF_TestCount, ºSave_MXATrace, 0, PXITrace[HotNF_TestCount].Multi_Trace[0][0].RBW_Hz);
                            Save_PXI_NF_TraceRaw(ºTestParaName + "_NF-Rise", TestCount, ºSave_MXATrace, 0, PXITrace[HotNF_TestCount].Multi_Trace[0][0].RBW_Hz);

                            #region Calculate Result
                            //Calculate the result from the sorted data
                            for (istep = 0; istep < Nop_ColdNF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[ColdNF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                    }
                                }

                            }

                            for (istep = 0; istep < Nop_HotNF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[HotNF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_HotNF.Ampl[istep] = PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_HotNF.FreqMHz[istep] = PXITrace[HotNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_HotNF.Ampl[istep] = PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_HotNF.FreqMHz[istep] = PXITrace[HotNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        CalcData = PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                    }
                                }
                            }

                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                Dic_ColdNF.Add(ResultMultiTrace_ColdNF.FreqMHz[i], ResultMultiTrace_ColdNF.Ampl[i]);
                            }

                            for (int i = 0; i < Nop_HotNF; i++)
                            {
                                Dic_HotNF.Add(ResultMultiTrace_HotNF.FreqMHz[i], ResultMultiTrace_HotNF.Ampl[i]);
                            }

                            int nfCount = 0;
                            foreach (var nfvalue in Dic_HotNF)
                            {
                                try
                                {
                                    if (Dic_HotNF[nfvalue.Key].ToString() == ("9999") || Dic_ColdNF[nfvalue.Key].ToString() == ("9999"))
                                    {
                                        ResultMultiTraceDelta.Ampl[nfCount] = 9999;
                                        ResultMultiTraceDelta.FreqMHz[nfCount] = nfvalue.Key;
                                    }

                                    else
                                    {
                                        ResultMultiTraceDelta.Ampl[nfCount] = Dic_HotNF[nfvalue.Key] - Dic_ColdNF[nfvalue.Key];
                                        ResultMultiTraceDelta.FreqMHz[nfCount] = nfvalue.Key;
                                    }

                                    nfCount++;

                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }
                            }

                            MaxColdNFAmpl = ResultMultiTrace_ColdNF.Ampl.Max();
                            MaxColdNFFreq = ResultMultiTrace_ColdNF.FreqMHz[Array.IndexOf(ResultMultiTrace_ColdNF.Ampl, ResultMultiTrace_ColdNF.Ampl.Max())];

                            MaxHotNFAmpl = ResultMultiTrace_HotNF.Ampl.Max();
                            MaxHotNFFreq = ResultMultiTrace_HotNF.FreqMHz[Array.IndexOf(ResultMultiTrace_HotNF.Ampl, ResultMultiTrace_HotNF.Ampl.Max())];

                            MaxNFRiseAmpl = ResultMultiTraceDelta.Ampl.Max();
                            MaxNFRiseFreq = ResultMultiTraceDelta.FreqMHz[Array.IndexOf(ResultMultiTraceDelta.Ampl, ResultMultiTraceDelta.Ampl.Max())];
                            #endregion

                            if (ºTest_NF1)
                            {
                                for (int i = 0; i < Nop_ColdNF; i++)
                                {
                                    List<double> listColdPower = new List<double>();
                                    for (int j = 0; j < NumberOfRunsColdNF; j++)
                                    {
                                        listColdPower.Add(Cold_NoisePower_new[j][i]);
                                    }

                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-Power" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dBm", listColdPower.Max());
                                }

                                for (int i = 0; i < Nop_HotNF; i++)
                                {
                                    List<double> listHotPower = new List<double>();
                                    for (int j = 0; j < NumberOfRunsHotNF; j++)
                                    {
                                        listHotPower.Add(Hot_NoisePower_new[j][i]);
                                    }

                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTrace_HotNF.FreqMHz[i] + "_Hot-Power" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", listHotPower.Max());
                                }

                                for (int i = 0; i < Nop_ColdNF; i++)
                                {
                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                }

                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Ampl_" + "Cold-Max-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", MaxColdNFAmpl);
                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Freq_" + "Cold-Max-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", MaxColdNFFreq);

                                for (int i = 0; i < Nop_HotNF; i++)
                                {
                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTrace_HotNF.FreqMHz[i] + "_Hot-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", ResultMultiTrace_HotNF.Ampl[i]);
                                }

                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Ampl_" + "Hot-Max-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", MaxHotNFAmpl);
                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Freq_" + "Hot-Max-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", MaxHotNFFreq);

                                for (istep = 0; istep < Nop_HotNF; istep++)
                                {
                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTraceDelta.FreqMHz[istep] + "_NF-Rise" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", ResultMultiTraceDelta.Ampl[istep]);
                                }


                                double[] maxOfmaxNFRiseAmpl = new double[NumberOfRunsHotNF];
                                double[] maxOfmaxNFRiseFreq = new double[NumberOfRunsHotNF];

                                for (istep = 0; istep < NumberOfRunsHotNF; istep++)
                                {
                                    double maxNFRiseAmpl = PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max();
                                    double maxNFRiseFreq = PXITrace[TestCount].Multi_Trace[0][istep].FreqMHz[Array.IndexOf(PXITrace[TestCount].Multi_Trace[0][istep].Ampl, PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max())];

                                    maxOfmaxNFRiseAmpl[istep] = maxNFRiseAmpl;
                                    maxOfmaxNFRiseFreq[istep] = maxNFRiseFreq;

                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Ampl_" + "NF-Max-Rise" + (istep + 1) + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", maxNFRiseAmpl);
                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Freq_" + "NF-Max-Rise" + (istep + 1) + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", maxNFRiseFreq);
                                }

                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Ampl_" + "NF-Max-Rise-ALL" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", maxOfmaxNFRiseAmpl.Max());
                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Freq_" + "NF-Max-Rise-ALL" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", maxOfmaxNFRiseFreq[Array.IndexOf(maxOfmaxNFRiseAmpl, maxOfmaxNFRiseAmpl.Max())]);
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep

                            ºTest_Pin1 = false;
                            ºTest_Pout1 = false;
                            ºTest_SMU = false;
                            ºTest_NF1 = false;

                            break;
                            #endregion

                        case "NF_FETCH":
                            #region NF_COLD_HOT_FETCH calculation

                            TestUsePrev_Array = ºTestUsePrev.Split(',');
                            NF_TestCount = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(TestUsePrev_Array[0]) == PXITrace[i].TestNumber)
                                {
                                    NF_TestCount = i;
                                }
                            }

                            Nop_NF = PXITrace[NF_TestCount].Multi_Trace[0][0].NoPoints;
                            NumberOfRunsNF = PXITrace[NF_TestCount].TraceCount;
                            RXPathLoss_NF = new double[Nop_NF];

                            NF_new = new double[NumberOfRunsNF][];
                            NoisePower_new = new double[NumberOfRunsNF][];

                            ResultMultiTrace_NF = new s_TraceNo();
                            ResultMultiTrace_NF.Ampl = new double[Nop_NF];
                            ResultMultiTrace_NF.FreqMHz = new double[Nop_NF];

                            Dic_NF = new Dictionary<double, double>();

                            MaxNFAmpl = 0;
                            MaxNFFreq = 0;
                            CalcData = 0;

                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            for (int i = 0; i < NumberOfRunsNF; i++)
                            {
                                NF_new[i] = new double[Nop_NF];
                                NoisePower_new[i] = new double[Nop_NF];
                            }

                            // NF RX path loss gathering
                            for (int i = 0; i < Nop_NF; i++)
                            {
                                ºRXFreq = Convert.ToSingle(PXITrace[NF_TestCount].Multi_Trace[0][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                RXPathLoss_NF[i] = ºLossOutputPathRX1;
                            }


                            if (PXITrace[NF_TestCount].Multi_Trace[0][0].MXA_No == "PXI_NF_COLD_Trace")
                            {
                                // Cold NF Fetch    
                                for (int i = 0; i < NumberOfRunsNF; i++)
                                {
                                    EqRFmx.RetrieveResults_NFColdSource(NF_TestCount, 0, "result::" + "COLD" + NF_TestCount.ToString() + "_" + i);

                                    for (int j = 0; j < Nop_NF; j++)
                                    {
                                        double NF_withoutGain = (EqRFmx.dutNoiseFigure[j].ToString().Contains("NaN") || EqRFmx.dutNoiseFigure[j].ToString().Contains("Infinity")) ? 9999 : EqRFmx.dutNoiseFigure[j];

                                        if (NF_withoutGain == 9999 || PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity")||PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                        {
                                            NF_new[i][j] = 9999;
                                        }

                                        else
                                        {
                                            NF_new[i][j] = NF_withoutGain - PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j];
                                        }

                                        NoisePower_new[i][j] = EqRFmx.coldSourcePower[j] - RXPathLoss_NF[j];
                                    }
                                }
                            }

                            else if (PXITrace[NF_TestCount].Multi_Trace[0][0].MXA_No == "PXI_NF_HOT_Trace")
                            {
                                // Hot NF Fetch
                                for (int i = 0; i < NumberOfRunsNF; i++)
                                {
                                    for (int j = 0; j < Nop_NF; j++)
                                    {
                                        EqRFmx.RetrieveResults_NFColdSource(NF_TestCount, j, "result::" + "HOT" + NF_TestCount + "_" + i.ToString() + "_" + j.ToString());

                                        double NF_withoutGain = (EqRFmx.dutNoiseFigure[0].ToString().Contains("NaN") || EqRFmx.dutNoiseFigure[0].ToString().Contains("Infinity")) ? 9999 : EqRFmx.dutNoiseFigure[0];

                                        if (NF_withoutGain == 9999 || PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                        {
                                            NF_new[i][j] = 9999;
                                        }

                                        else
                                        {
                                            NF_new[i][j] = NF_withoutGain - PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j];
                                        }

                                        NoisePower_new[i][j] = EqRFmx.coldSourcePower[0] - RXPathLoss_NF[j];
                                    }
                                }
                            }

                            else { MessageBox.Show("Need to check if Cold NF & Hot NF data acquisition is performed or not"); }

                            // Store Cold & Hot NF data into PXI Trace
                            StoreNFdata(NF_TestCount, NumberOfRunsNF, Nop_NF, NF_new);

                            if (PXITrace[NF_TestCount].Multi_Trace[0][0].MXA_No == "PXI_NF_COLD_Trace")
                            {
                                // Save Cold & Hot NF Trace if Save_MXATrace is enabled
                                Save_PXI_NF_TraceRaw(ºTestParaName + "_Cold-NF", NF_TestCount, ºSave_MXATrace, 0, ºNF_BW * 1e06);
                            }

                            else
                            {
                                // Save Cold & Hot NF Trace if Save_MXATrace is enabled
                                Save_PXI_NF_TraceRaw(ºTestParaName + "_Hot-NF", NF_TestCount, ºSave_MXATrace, 0, ºNF_BW * 1e06);
                            }

                            #region Calculate Result
                            //Calculate the result from the sorted data
                            for (istep = 0; istep < Nop_NF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[NF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_NF.Ampl[istep] = PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_NF.FreqMHz[istep] = PXITrace[NF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_NF.Ampl[istep] = PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_NF.FreqMHz[istep] = PXITrace[NF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        CalcData = PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                    }
                                }

                            }

                            for (int i = 0; i < Nop_NF; i++)
                            {
                                Dic_NF.Add(ResultMultiTrace_NF.FreqMHz[i], ResultMultiTrace_NF.Ampl[i]);
                            }

                            MaxNFAmpl = ResultMultiTrace_NF.Ampl.Max();
                            MaxNFFreq = ResultMultiTrace_NF.FreqMHz[Array.IndexOf(ResultMultiTrace_NF.Ampl, ResultMultiTrace_NF.Ampl.Max())];

                            #endregion

                            if (ºTest_NF1)
                            {
                                if (PXITrace[NF_TestCount].Multi_Trace[0][0].MXA_No == "PXI_NF_COLD_Trace")
                                {
                                    for (int i = 0; i < Nop_NF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsNF; j++)
                                        {
                                            listColdPower.Add(NoisePower_new[j][i]);
                                        }

                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTrace_NF.FreqMHz[i] + "_Cold-Power" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dBm", listColdPower.Max());
                                    }

                                    for (int i = 0; i < Nop_NF; i++)
                                    {
                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTrace_NF.FreqMHz[i] + "_Cold-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", ResultMultiTrace_NF.Ampl[i]);
                                    }

                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Ampl_" + "Cold-Max-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", MaxNFAmpl);
                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Freq_" + "Cold-Max-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", MaxNFFreq);
                                }

                                else
                                {
                                    for (int i = 0; i < Nop_NF; i++)
                                    {
                                        List<double> listHotPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsNF; j++)
                                        {
                                            listHotPower.Add(NoisePower_new[j][i]);
                                        }

                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTrace_NF.FreqMHz[i] + "_Hot-Power" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", listHotPower.Max());
                                    }

                                    for (int i = 0; i < Nop_NF; i++)
                                    {
                                        BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_" + ResultMultiTrace_NF.FreqMHz[i] + "_Hot-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", ResultMultiTrace_NF.Ampl[i]);
                                    }

                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Ampl_" + "Hot-Max-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", MaxNFAmpl);
                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Freq_" + "Hot-Max-NF" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dB", MaxNFFreq);
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep

                            ºTest_Pin1 = false;
                            ºTest_Pout1 = false;
                            ºTest_SMU = false;
                            ºTest_NF1 = false;

                            break;
                            #endregion

                        default:
                            MessageBox.Show("Test Parameter : " + ºTestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;

                case "COMMON":
                    switch (ºTestParam.ToUpper())
                    {
                        case "MAX_MIN":
                            #region MAX MIN calculation
                            //Find result MAX or MIN result from few sets of data - define in 'Use Previous' column - data example 4,6,9,10
                            if (ºTest_Pin)
                            {
                                resultTag = (int)e_ResultTag.PIN;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_Pin = result;
                            }
                            if (ºTest_Pout)
                            {
                                resultTag = (int)e_ResultTag.POUT;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_Pout = result;
                            }
                            if (ºTest_Pin1)
                            {
                                resultTag = (int)e_ResultTag.PIN1;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_Pin1 = result;
                            }
                            if (ºTest_Pout1)
                            {
                                resultTag = (int)e_ResultTag.POUT1;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_Pout1 = result;
                            }
                            if (ºTest_Pin2)
                            {
                                resultTag = (int)e_ResultTag.PIN2;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_Pin2 = result;
                            }
                            if (ºTest_Pout2)
                            {
                                resultTag = (int)e_ResultTag.POUT2;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_Pout2 = result;
                            }
                            if (ºTest_NF1)
                            {
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_NF1_Ampl = result;
                                resultTag = (int)e_ResultTag.NF1_FREQ;
                                R_NF1_Freq = Results[arrayVal].Multi_Results[resultTag].Result_Data;
                            }
                            if (ºTest_NF2)
                            {
                                resultTag = (int)e_ResultTag.NF2_AMPL;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_NF2_Ampl = result;
                                resultTag = (int)e_ResultTag.NF2_FREQ;
                                R_NF2_Freq = Results[arrayVal].Multi_Results[resultTag].Result_Data;
                            }
                            if (ºTest_Harmonic)
                            {
                                resultTag = (int)e_ResultTag.HARMONIC_AMPL;
                                SearchMAXMIN(ºTestParam, ºTestUsePrev, ºSearch_Method, resultTag, out result, out arrayVal);
                                R_H2_Ampl = result;
                            }
                            tTime.Stop();
                            #endregion
                            break;
                        case "AVERAGE":
                            #region AVERAGE calculation
                            //Find result average between few result - define in 'Use Previous' column - data example 4,6,9,10
                            if (ºTest_Pin)
                            {
                                resultTag = (int)e_ResultTag.PIN;
                                R_Pin = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pout)
                            {
                                resultTag = (int)e_ResultTag.POUT;
                                R_Pout = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pin1)
                            {
                                resultTag = (int)e_ResultTag.PIN1;
                                R_Pin1 = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pout1)
                            {
                                resultTag = (int)e_ResultTag.POUT1;
                                R_Pout2 = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pin2)
                            {
                                resultTag = (int)e_ResultTag.PIN2;
                                R_Pin2 = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pout2)
                            {
                                resultTag = (int)e_ResultTag.POUT2;
                                R_Pout2 = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_NF1)
                            {
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                R_NF1_Ampl = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_NF2)
                            {
                                resultTag = (int)e_ResultTag.NF2_AMPL;
                                R_NF2_Ampl = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Harmonic)
                            {
                                resultTag = (int)e_ResultTag.HARMONIC_AMPL;
                                R_H2_Ampl = CalcAverage(ºTestUsePrev, resultTag);
                            }
                            tTime.Stop();
                            #endregion
                            break;
                        case "DELTA":
                            #region MAX MIN calculation
                            //Find result Delta between 2 result - define in 'Use Previous' column - data example 4,10
                            if (ºTest_Pin)
                            {
                                resultTag = (int)e_ResultTag.PIN;
                                R_Pin = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            if (ºTest_Pout)
                            {
                                resultTag = (int)e_ResultTag.POUT;
                                R_Pout = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            if (ºTest_Pin1)
                            {
                                resultTag = (int)e_ResultTag.PIN1;
                                R_Pin1 = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            if (ºTest_Pout1)
                            {
                                resultTag = (int)e_ResultTag.POUT1;
                                R_Pout2 = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            if (ºTest_Pin2)
                            {
                                resultTag = (int)e_ResultTag.PIN2;
                                R_Pin2 = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            if (ºTest_Pout2)
                            {
                                resultTag = (int)e_ResultTag.POUT2;
                                R_Pout2 = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            if (ºTest_NF1)
                            {
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                R_NF1_Ampl = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            if (ºTest_NF2)
                            {
                                resultTag = (int)e_ResultTag.NF2_AMPL;
                                R_NF2_Ampl = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            if (ºTest_Harmonic)
                            {
                                resultTag = (int)e_ResultTag.HARMONIC_AMPL;
                                R_H2_Ampl = CalcDelta(ºTestUsePrev, resultTag, ºAbs_Value);
                            }
                            tTime.Stop();
                            #endregion
                            break;
                        case "SUM":
                            #region Summary calculation
                            //Find result summary between few result - define in 'Use Previous' column - data example 4,6,9,10
                            if (ºTest_Pin)
                            {
                                resultTag = (int)e_ResultTag.PIN;
                                R_Pin = CalcSum(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pout)
                            {
                                resultTag = (int)e_ResultTag.POUT;
                                R_Pout = CalcSum(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pin1)
                            {
                                resultTag = (int)e_ResultTag.PIN1;
                                R_Pin1 = CalcSum(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pout1)
                            {
                                resultTag = (int)e_ResultTag.POUT1;
                                R_Pout2 = CalcSum(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pin2)
                            {
                                resultTag = (int)e_ResultTag.PIN2;
                                R_Pin2 = CalcSum(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Pout2)
                            {
                                resultTag = (int)e_ResultTag.POUT2;
                                R_Pout2 = CalcSum(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_NF1)
                            {
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                R_NF1_Ampl = CalcSum(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_NF2)
                            {
                                resultTag = (int)e_ResultTag.NF2_AMPL;
                                R_NF2_Ampl = CalcSum(ºTestUsePrev, resultTag);
                            }
                            if (ºTest_Harmonic)
                            {
                                resultTag = (int)e_ResultTag.HARMONIC_AMPL;
                                R_H2_Ampl = CalcSum(ºTestUsePrev, resultTag);
                            }
                            tTime.Stop();
                            #endregion
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + ºTestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;

                case "CALIBRATION":
                    switch (ºTestParam.ToUpper())
                    {
                        case "RF_CAL":
                            RF_Calibration(ºTrig_Delay, ºGeneric_Delay, ºRdCurr_Delay, ºRdPwr_Delay, ºSetup_Delay);
                            R_RFCalStatus = 1;
                            tTime.Stop();
                            break;
                        case "NF_CAL":
                            #region NF Calibration
                            if (NFCalFlag)
                            {
                                calDir = @"C:\Avago.ATF.Common\Input\Calibration_NF\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + @"\";

                                if (!Directory.Exists(calDir))
                                {
                                    Directory.CreateDirectory(calDir);
                                }

                                NFCalFlag = false;
                            }

                            NoOfPts = (Convert.ToInt32((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1)) + 1;
                            RXContactFreq = new double[NoOfPts];
                            RXPathLoss = new double[NoOfPts];
                            LNAInputLoss = new double[NoOfPts];

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            int indexStep = 0;
                            ºRXFreq = ºStartRXFreq1;
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1);

                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = Math.Round(ºRXFreq, 3);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                RXPathLoss[i] = ºLossOutputPathRX1;//Seoul

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºRXFreq, ref ºLossCouplerPath, ref StrError);
                                LNAInputLoss[i] = ºLossCouplerPath;//Seoul

                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + ºStepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
                                indexStep = indexStep + Convert.ToInt32(ºStepRXFreq1);
                            }

                            //Switching for NF Testing -Seoul
                            EqSwitch.SetPath(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], TCF_Header.ConstSwitching_Band_HotNF, ºSwBand_HotNF.ToUpper()));
                            PreviousSWMode = ºSwBand_HotNF.ToUpper();
                            DelayMs(ºSetup_Delay);

                            EqVST.ConfigureTriggers();

                            NF_Calibration(TestCount, ºNF_CalTag, RXContactFreq, LNAInputLoss, RXPathLoss, ºNF_Cal_HL, ºNF_Cal_LL, ºNF_BW);

                            EqVST.ReConfigVST();
                            #endregion
                            break;


                        default:
                            MessageBox.Show("Test Parameter : " + ºTestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;

                case "DC":
                    switch (ºTestParam.ToUpper())
                    {
                        case "PS4CH":
                            #region 4-Channel Power Supply Setting
                            //pass to global variable to be use outside this function
                            EqmtStatus.DC_CH = ºDCSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetDC = ºDCSetCh.Split(',');
                            MeasDC = ºDCMeasCh.Split(',');

                            for (int i = 0; i < SetDC.Count(); i++)
                            {
                                int dcVChannel = Convert.ToInt16(SetDC[i]);
                                EqDC.SetVolt((dcVChannel), ºDCVCh[dcVChannel], ºDCILimitCh[dcVChannel]);
                                EqDC.DcOn(dcVChannel);
                            }

                            if (ºTest_DCSupply)
                            {
                                for (int i = 0; i < MeasDC.Count(); i++)
                                {
                                    int dcIChannel = Convert.ToInt16(MeasDC[i]);

                                    if (ºDCILimitCh[dcIChannel] > 0)
                                    {
                                        R_DC_ICh[dcIChannel] = EqDC.MeasI(dcIChannel);
                                        if (R_DC_ICh[dcIChannel] < (ºDCILimitCh[dcIChannel] * 0.1))     //if current measure less than 10%, do 2nd Time to ensure that current are measure correctly
                                        {
                                            DelayMs(ºRdCurr_Delay);
                                            R_DC_ICh[dcIChannel] = EqDC.MeasI(dcIChannel);
                                            R_DC_ICh[dcIChannel] = EqDC.MeasI(dcIChannel);
                                        }
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "DCI_CH" + MeasDC[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_DCLabel_ICh[dcIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        case "PS1CH":
                            #region 1-Channel Power Supply Setting
                            //pass to global variable to be use outside this function
                            EqmtStatus.DC_CH = ºDCSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetDC = ºDCSetCh.Split(',');
                            MeasDC = ºDCMeasCh.Split(',');

                            for (int i = 0; i < 1; i++)
                            {
                                int dcVChannel = Convert.ToInt16(SetDC[i]);
                                EqDC_1CH.SetVolt((dcVChannel), ºDCVCh[dcVChannel], ºDCILimitCh[dcVChannel]);
                                EqDC_1CH.DcOn(dcVChannel);
                            }

                            if (FirstDut)
                            {
                                FirstDut = false;
                                DelayMs(500);
                            }

                            if (ºTest_DCSupply)
                            {
                                for (int i = 0; i < 1; i++)
                                {
                                    int dcIChannel = Convert.ToInt16(MeasDC[i]);

                                    if (ºDCILimitCh[dcIChannel] > 0)
                                    {
                                        R_DC_ICh[dcIChannel] = EqDC_1CH.MeasI(dcIChannel);
                                        if (R_DC_ICh[dcIChannel] < (ºDCILimitCh[dcIChannel] * 0.1))     //if current measure less than 10%, do 2nd Time to ensure that current are measure correctly
                                        {
                                            DelayMs(ºRdCurr_Delay);
                                            R_DC_ICh[dcIChannel] = EqDC_1CH.MeasI(dcIChannel);
                                            //R_DC_ICh[dcIChannel] = EqDC_1CH.MeasI(dcIChannel);
                                        }
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "DCI_CH" + MeasDC[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_DCLabel_ICh[dcIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }
                            #endregion
                            break;

                        case "MULTI_DCSUPPLY":
                            #region Multiple 1-Channel Power Supply Setting
                            //pass to global variable to be use outside this function
                            EqmtStatus.DC_CH = ºDCSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetDC = ºDCSetCh.Split(',');
                            MeasDC = ºDCMeasCh.Split(',');

                            for (int i = 0; i < SetDC.Length; i++)
                            {
                                int dcVChannel = Convert.ToInt16(SetDC[i]);
                                EqDCSupply[i].SetVolt((dcVChannel), ºDCVCh[dcVChannel], ºDCILimitCh[dcVChannel]);
                                EqDCSupply[i].DcOn(dcVChannel);
                            }

                            if (FirstDut)
                            {
                                FirstDut = false;
                                DelayMs(500);
                            }

                            if (ºTest_DCSupply)
                            {
                                for (int i = 0; i < MeasDC.Length; i++)
                                {
                                    int dcIChannel = Convert.ToInt16(MeasDC[i]);

                                    if (ºDCILimitCh[dcIChannel] > 0)
                                    {
                                        R_DC_ICh[dcIChannel] = EqDCSupply[i].MeasI(dcIChannel);
                                        if (R_DC_ICh[dcIChannel] < (ºDCILimitCh[dcIChannel] * 0.1))     //if current measure less than 10%, do 2nd Time to ensure that current are measure correctly
                                        {
                                            DelayMs(ºRdCurr_Delay);
                                            R_DC_ICh[dcIChannel] = EqDCSupply[i].MeasI(dcIChannel);
                                        }
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "DCI_CH" + MeasDC[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_DCLabel_ICh[dcIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }
                            #endregion
                            break;

                        case "SMU":
                            //string[] SetSMUSelect;
                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = ºSMUSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetSMU = ºSMUSetCh.Split(',');
                            MeasSMU = ºSMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                                EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);
                            }

                            EqSMUDriver.DcOn(SetSMUSelect, EqSMU);

                            if (ºTest_SMU)
                            {
                                DelayMs(ºRdCurr_Delay);
                                //float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                float _NPLC = 1;
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (ºSMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + ºTestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }

                    break;
                case "MIPI":
                    switch (ºTestParam.ToUpper())
                    {
                        #region MIPI

                        #region  MIPI - Fixed Mipi Pair and Slave Address (define from config file)
                        case "SETMIPI":

                            //Set MIPI
                            EqMiPiCtrl.TurnOn_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT
                            EqMiPiCtrl.TurnOn_VIO(1);      //mipi pair 0 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board

                            EqMiPiCtrl.SendAndReadMIPICodes(out MIPI_Read_Successful, ºMiPi_RegNo);
                            LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful)
                                R_MIPI = 1;

                            //Measure SMU current
                            MeasSMU = ºSMUMeasCh.Split(',');
                            if (ºTest_SMU)
                            {
                                DelayMs(ºRdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (ºSMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        case "SETMIPI_SMU":
                            //Set SMU
                            //string[] SetSMUSelect;

                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = ºSMUSetCh;

                            SetSMU = ºSMUSetCh.Split(',');
                            MeasSMU = ºSMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                                EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);
                            }

                            EqSMUDriver.DcOn(SetSMUSelect, EqSMU);

                            //Set MIPI
                            EqMiPiCtrl.TurnOn_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT
                            EqMiPiCtrl.TurnOn_VIO(1);      //mipi pair 0 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board
                            EqMiPiCtrl.SendAndReadMIPICodes(out MIPI_Read_Successful, ºMiPi_RegNo);
                            LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful)
                                R_MIPI = 1;

                            if (ºTest_SMU)
                            {
                                DelayMs(ºRdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (ºSMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;
                        #endregion

                        #region MIPI CUSTOM - flexible Mipi Reg, Mipi Pair and Slave Address (define from MIPI spreadsheet)
                        case "SETMIPI_CUSTOM":

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI
                            //EqMiPiCtrl.TurnOn_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT
                            //EqMiPiCtrl.TurnOn_VIO(1);      //mipi pair 0 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board
                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                            LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful)
                                R_MIPI = 1;

                            //Measure SMU current
                            MeasSMU = ºSMUMeasCh.Split(',');
                            if (ºTest_SMU)
                            {
                                DelayMs(ºRdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (ºSMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        case "SETMIPI_CUSTOM_SMU":
                            //Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = ºSMUSetCh;

                            SetSMU = ºSMUSetCh.Split(',');
                            MeasSMU = ºSMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                                EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);
                            }

                            EqSMUDriver.DcOn(SetSMUSelect, EqSMU);

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI
                            //EqMiPiCtrl.TurnOn_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT
                            //EqMiPiCtrl.TurnOn_VIO(1);      //mipi pair 0 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board
                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                            LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful)
                                R_MIPI = 1;

                            if (ºTest_SMU)
                            {
                                DelayMs(ºRdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (ºSMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;
                        #endregion

                        #region READ MIPI CUSTOM - flexible Mipi Reg, Mipi Pair and Slave Address (define from MIPI spreadsheet)

                        case "READMIPI_REG_CUSTOM":

                            if(ºSearch_Method.ToUpper() == "MIPI_CMOS-TX-IPQ_SUB-VER")
                            {

                            }
                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI and Read MIPI
                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            switch (ºSearch_Method.ToUpper())
                            {
                                case "TEMP":
                                case "TEMPERATURE":
                          
                                    #region Set SMU Channel - CMOS Temperature read out
                                    //Read temperature required VBatt to be turn ON
                                    //pass to global variable to be use outside this function
                                    EqmtStatus.SMU_CH = ºSMUSetCh;

                                    SetSMU = ºSMUSetCh.Split(',');
                                    MeasSMU = ºSMUMeasCh.Split(',');

                                    SetSMUSelect = new string[SetSMU.Count()];
                                    for (int i = 0; i < SetSMU.Count(); i++)
                                    {
                                        int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                        SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on required channel only from total of 8 channel available  
                                        EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);
                                    }

                                    EqSMUDriver.DcOn(SetSMUSelect, EqSMU);
                                    DelayMs(ºRdCurr_Delay);
                                    #endregion

                                    dataDec_Conv = 0;
                                    R_MIPI = -999;

                                    EqMiPiCtrl.WriteMIPICodesCustom(CusMipiRegMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    DelayUs(100);       //fixed delay about 100uS before readback the register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out dataDec_Conv, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    dutTempSensor((double)dataDec_Conv, out R_MIPI);

                                    ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    BuildResults(ref results, ºTestParaName, "C", R_MIPI);
                                    break;

                                default:
                                    //EqMiPiCtrl.WriteMIPICodesCustom(CusMipiRegMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    DelayMs(ºRdCurr_Delay);
                                    EqMiPiCtrl.ReadMIPICodesCustom(out R_ReadMipiReg, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));

                                    ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                    break;
                            }

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        case "READ_OTP_CUSTOM":

                            //Init variable
                            dataDec = new int[2];

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI and Read MIPI
                            //example CusMipiRegMap must be in '42:XX 43:XX' where 42 is MSB reg address and XX - Data (don't care) ,  43 is LSB reg address and XX - Data (don't care)
                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(ºRdCurr_Delay);

                            #region Read Back MIPI register
                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    R_ReadMipiReg = -999;   //set to fail value (default)
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 16;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length == effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        R_ReadMipiReg = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                    {
                                        R_MIPI = 1;
                                    }
                                    BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);
                                    BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                    BuildResults(ref results, ºTestParaName + "_MSB", "dec", dataDec[0]);
                                    BuildResults(ref results, ºTestParaName + "_LSB", "dec", dataDec[1]);

                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    R_ReadMipiReg = -999;   //set to fail value (default)
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 14;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                                        //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(0, appendBinary.Length - effectiveBits);                //remove first 2Bits from MSB to make effectiveData = 14 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);                                          //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);
                                    BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                    BuildResults(ref results, ºTestParaName + "_MSB", "dec", dataDec[0]);
                                    BuildResults(ref results, ºTestParaName + "_LSB", "dec", dataDec[1]);

                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        #endregion

                        #region BURN OTP JEDI1 - flexible Mipi Reg, Mipi Pair and Slave Address (define from MIPI spreadsheet)

                        case "BURN_OTP_JEDI":

                            #region Check OTP status and sort data to burn

                            //Initialize to default
                            b_lockBit = true;
                            i_lockBit = 0;
                            i_testFlag = 0;
                            b_testFlag = true;
                            i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                            BurnOTP = false;

                            dataBinary = new string[2];
                            appendBinary = null;
                            dataDec = new int[2];

                            //data to program 
                            dataHex = new string[2];

                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = ºSMUSetCh;

                            SetSMU = ºSMUSetCh.Split(',');
                            MeasSMU = ºSMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                                EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);
                            }

                            EqSMUDriver.DcOn(SetSMUSelect, EqSMU);
                            #endregion

                            #region Decode MIPI Register - Data from Mipi custom spreadsheet

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI and Read MIPI
                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(ºRdCurr_Delay);

                            #endregion

                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    dataSizeHex = "0xFFFF";     //max data size is 16bit
                                    BurnOTP = false;

                                    dataDec_Conv = Convert.ToInt32(mfgLotID);         //convert string to int
                                    if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }
                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    dataSizeHex = "0x3FFF";         //max data size is 14bit 
                                    BurnOTP = false;

                                    //Set the DUT SN ID and Check if file exist , if not exist -> create and write default SN
                                    OTPLogFilePath = mfgLotID_Path + SNPFile.FileOutput_FileName + "_" + mfgLotID + ".txt";
                                    if (tmpUnit_No == 1)
                                    {
                                        if (!Directory.Exists(@mfgLotID_Path))
                                            System.IO.Directory.CreateDirectory(@mfgLotID_Path);

                                        if (!File.Exists(@OTPLogFilePath))
                                        {
                                            // write default SN to file 
                                            try
                                            {
                                                ArrayList LocalTextList = new ArrayList();
                                                LocalTextList.Add("[SN_ID]");
                                                LocalTextList.Add("SN_COUNT = 0");

                                                IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                            }
                                            catch (FileNotFoundException)
                                            {
                                                throw new FileNotFoundException("Cannot Write Existing file!");
                                            }
                                        }
                                    }

                                    //read SN from files
                                    if (File.Exists(@OTPLogFilePath))
                                        tmpData = IO_TxtFile.ReadTextFile(OTPLogFilePath, "SN_ID", "SN_COUNT");

                                    otpUnitID = Convert.ToInt32(tmpData) + 1;       //next SN to burn 
                                    dataDec_Conv = otpUnitID;

                                    if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }
                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    //read test flag register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //read lockbit register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_lockBit, "E5", CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_lockBit = (Convert.ToByte(i_lockBit) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //Normal Unit - All test pass -> Lockbit(E5 = 0) and TestFlag(Bit7 = 0) - both not program
                                    if (FailedTests.Count == 0 && !b_testFlag && !b_lockBit)
                                    {
                                        dataHex[0] = "80";      //Burn bit 7 -> 0x80 >> 1000 0000
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }
                                    break;
                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            #region Decode MIPI Register and Burn OTP

                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                            efuseCtrlAddress = new int[3];
                            tempData = new string[2];

                            for (int i = 0; i < biasDataArr.Length; i++)
                            {
                                //Note : EFuse Control Register
                                //efuse cell_0 (0xE0, mirror address 0x0D)
                                //efuse cell_1 (0xE1, mirror address 0x0E)
                                //efuse cell_2 (0xE2, mirror address 0x21)
                                //efuse cell_3 (0xE3, mirror address 0x40)
                                //efuse cell_4 (0xE4, mirror address 0x41)

                                tempData = biasDataArr[i].Split(':');
                                switch (tempData[0].ToUpper())
                                {
                                    case "0D":
                                    case "E0":              //efuse cell_0 (0xE0)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "0E":
                                    case "E1":              //efuse cell_1 (0xE1)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    case "21":
                                    case "E2":              //efuse cell_2 (0xE2)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 1;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "40":
                                    case "E3":              //efuse cell_3 (0xE3)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 1;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    case "41":
                                    case "E4":              //efuse cell_4  (0xE4)
                                        efuseCtrlAddress[2] = 1;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "E5":                  //unknown Mirror Address - Design document not specified , use efuse cell_5 (0xE5)
                                        efuseCtrlAddress[2] = 1;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    default:
                                        MessageBox.Show("Test Parameter : " + ºTestParam + "(" + tempData[0].ToUpper() + ") - OTP Address not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                        break;
                                }

                                #region Burn OTP Data

                                if (BurnOTP)
                                {
                                    //Burn twice to double confirm the otp programming is done completely
                                    //JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                                    //JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);

                                    //Burn5x to double confirm the otp programming is done completely
                                    for (int cnt = 0; cnt < 5; cnt++)
                                    {
                                        JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                                    }
                                }

                                #endregion
                            }

                            #endregion

                            #region Read Back MIPI register

                            #region Turn off SMU and VIO - to prepare for read back mipi register
                            if (EqmtStatus.SMU)
                            {
                                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                            }

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(ºRdCurr_Delay);

                            //Set MIPI and Read MIPI
                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(ºRdCurr_Delay);
                            #endregion

                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 16;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length == effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        R_ReadMipiReg = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    //ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);
                                    if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                    {
                                        R_MIPI = 1;
                                    }
                                    BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                    BuildResults(ref results, ºTestParaName + "_MSB", "dec", dataDec[0]);
                                    BuildResults(ref results, ºTestParaName + "_LSB", "dec", dataDec[1]);

                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 14;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                                        //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(0, appendBinary.Length - effectiveBits);                //remove first 2Bits from MSB to make effectiveData = 14 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);                                          //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    //ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);
                                    if (R_ReadMipiReg == otpUnitID)
                                    {
                                        R_MIPI = 1;

                                        // write Unit ID data to file
                                        try
                                        {
                                            ArrayList LocalTextList = new ArrayList();
                                            LocalTextList.Add("[SN_ID]");
                                            LocalTextList.Add("SN_COUNT = " + otpUnitID);

                                            IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                        }
                                        catch (FileNotFoundException)
                                        {
                                            throw new FileNotFoundException("Cannot Write Existing file!");
                                        }
                                    }

                                    BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                    BuildResults(ref results, ºTestParaName + "_MSB", "dec", dataDec[0]);
                                    BuildResults(ref results, ºTestParaName + "_LSB", "dec", dataDec[1]);

                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    //set to fail value (default)
                                    R_MIPI = 0;
                                    b_testFlag = false;
                                    i_testFlag = 0;

                                    //read test flag register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    if (BurnOTP)
                                    {
                                        if (b_testFlag && BurnOTP)
                                            R_MIPI = 1;         //Only return '1' if burn OTP Flag and Read Bit 7 = 1
                                        else
                                            R_MIPI = 0;         //Only return '0' if burn OTP Flag and Read Bit 7 = 0 (note - readback Bit 7 failure)
                                    }

                                    //Retest unit and report out accordingly - Note : Not from Fresh Lots , already tested before
                                    if (!BurnOTP)
                                    {
                                        //Retest Unit (2A lot) Test Count pass and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit7 = 1) already program
                                        if (FailedTests.Count == 0 && b_testFlag && !b_lockBit)
                                        {
                                            R_MIPI = 2;
                                        }
                                        //Retest Unit (2A lot) Test Count Fail and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit7 = 1) already program
                                        if (FailedTests.Count > 0 && b_testFlag && !b_lockBit)
                                        {
                                            R_MIPI = 3;
                                        }
                                        //Retest Unit (redo all) - pass SParam - lockBit(E5 = 1) and TestFlag(Bit7 = 1) already program
                                        if (FailedTests.Count == 0 && b_testFlag && b_lockBit)
                                        {
                                            R_MIPI = 4;
                                        }
                                        //Retest Unit (redo all) Test Count Fail and pass SParam - lockBit(E5 = 1) and TestFlag(Bit7 = 1) already program
                                        if (FailedTests.Count > 0 && b_testFlag && b_lockBit)
                                        {
                                            R_MIPI = 5;
                                        }
                                    }

                                    ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);

                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        #endregion

                        #region BURN OTP JEDI2 - flexible Mipi Reg, Mipi Pair and Slave Address (define from MIPI spreadsheet)

                        case "BURN_OTP_JEDI2":

                            #region Check OTP status and sort data to burn

                            //Initialize to default
                            b_lockBit = true;
                            i_lockBit = 0;
                            i_testFlag = 0;
                            b_testFlag = true;
                            i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                            BurnOTP = false;

                            dataBinary = new string[2];
                            appendBinary = null;
                            dataDec = new int[2];

                            //data to program 
                            dataHex = new string[2];

                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = ºSMUSetCh;

                            SetSMU = ºSMUSetCh.Split(',');
                            MeasSMU = ºSMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                                EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);
                            }

                            EqSMUDriver.DcOn(SetSMUSelect, EqSMU);
                            #endregion

                            #region Decode MIPI Register - Data from Mipi custom spreadsheet & Read From DUT register

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);
                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

                            //Set MIPI and Read MIPI
                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(ºRdCurr_Delay);

                            //Read DUT register
                            for (int i = 0; i < biasDataArr.Length; i++)
                            {
                                EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                            }

                            //data use for empty register comparison (byright return data should be '0' for empty register)
                            tmpOutData_DecConv = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal

                            #endregion

                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    dataSizeHex = "0xFFFF";     //max data size is 16bit
                                    BurnOTP = false;

                                    dataDec_Conv = Convert.ToInt32(mfgLotID);         //convert string to int
                                    if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);

                                        if (tmpOutData_DecConv < 1)     //if register data is blank , then only set OTP Burn status to 'TRUE'
                                        {
                                            BurnOTP = true;         //set flag to true for burning otp
                                        }
                                    }
                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    dataSizeHex = "0x7FFF";         //max data size is 15bit 
                                    BurnOTP = false;

                                    //Set the DUT SN ID and Check if file exist , if not exist -> create and write default SN
                                    OTPLogFilePath = mfgLotID_Path + SNPFile.FileOutput_FileName + "_" + mfgLotID + ".txt";
                                    if (tmpUnit_No == 1)
                                    {
                                        if (!Directory.Exists(@mfgLotID_Path))
                                            System.IO.Directory.CreateDirectory(@mfgLotID_Path);

                                        if (!File.Exists(@OTPLogFilePath))
                                        {
                                            //get the 1st running number for unit id from local setting file 
                                            StartNo_UNIT_ID = IO_TxtFile.ReadTextFile(LocSetFilePath, "TESTSITE_UNIT_ID", "START_UNIT_ID");
                                            StopNo_UNIT_ID = IO_TxtFile.ReadTextFile(LocSetFilePath, "TESTSITE_UNIT_ID", "STOP_UNIT_ID");

                                            // write default SN to file 
                                            try
                                            {
                                                ArrayList LocalTextList = new ArrayList();
                                                LocalTextList.Add("[SN_ID]");
                                                LocalTextList.Add("SN_COUNT = " + StartNo_UNIT_ID);     //write default start unit_id (unique starting number for every test site)

                                                IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                            }
                                            catch (FileNotFoundException)
                                            {
                                                throw new FileNotFoundException("Cannot Write Existing file!");
                                            }
                                        }
                                    }

                                    //read SN from files
                                    if (File.Exists(@OTPLogFilePath))
                                        tmpData = IO_TxtFile.ReadTextFile(OTPLogFilePath, "SN_ID", "SN_COUNT");

                                    otpUnitID = Convert.ToInt32(tmpData) + 1;       //next SN to burn 
                                    dataDec_Conv = otpUnitID;

                                    if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);

                                        if (tmpOutData_DecConv < 1)     //if register data is blank , then only set OTP Burn status to 'TRUE'
                                        {
                                            BurnOTP = true;         //set flag to true for burning otp
                                        }
                                    }
                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    //read test flag register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //read lockbit register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_lockBit, "EB", CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_lockBit = (Convert.ToByte(i_lockBit) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //Normal Unit - All test pass -> Lockbit(EB = 0) and TestFlag(Bit0 = 0) - both not program
                                    if (FailedTests.Count == 0 && !b_testFlag && !b_lockBit)
                                    {
                                        dataHex[0] = "1";      //Burn bit 1 -> 0x02 >> 0000 0010
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }
                                    break;

                                case "CM_ID":
                                case "CMID":
                                    //read CM ID register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //read lockbit register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_lockBit, "EB", CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_lockBit = (Convert.ToByte(i_lockBit) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //Normal Unit - All test pass -> Lockbit(EB = 0) and CM_ID (Bit7 = 0) - both not program
                                    if (FailedTests.Count == 0 && !b_testFlag && !b_lockBit)
                                    {
                                        //Have not define how to decode the lot traveller for CM Site defination - Shaz 24/11/2017
                                        switch (CM_SITE.ToUpper())
                                        {
                                            case "ASEKOR":
                                                dataHex[0] = "80";      //Burn bit 7 -> 0x80 >> 1000 0000
                                                BurnOTP = true;        //set flag to true for burning otp
                                                break;
                                            case "AMKOR":
                                                dataHex[0] = "00";      //Burn bit 7 -> 0x00 >> 0000 0000
                                                BurnOTP = true;        //set flag to true for burning otp
                                                break;
                                            default:
                                                MessageBox.Show("Test Parameter : " + ºTestParam + "(" + CM_SITE + ") - CM SITE not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                BurnOTP = false;        //set flag to false for burning otp
                                                break;
                                        }
                                    }
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            #region Decode MIPI Register and Burn OTP

                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                            efuseCtrlAddress = new int[3];
                            tempData = new string[2];

                            for (int i = 0; i < biasDataArr.Length; i++)
                            {
                                //Note : EFuse Control Register
                                //efuse cell_3 (0xE3, mirror address 0x46)
                                //efuse cell_4 (0xE4, mirror address 0x47)
                                //efuse cell_5 (0xE5, mirror address 0x48)

                                tempData = biasDataArr[i].Split(':');
                                switch (tempData[0].ToUpper())
                                {
                                    case "E0":              //efuse cell_0 (0xE0)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "E1":              //efuse cell_1 (0xE1)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    case "E2":              //efuse cell_2 (0xE2)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 1;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "E3":              //efuse cell_3 (0xE3)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 1;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    case "E4":              //efuse cell_4  (0xE4)
                                        efuseCtrlAddress[2] = 1;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "E5":              //efuse cell_5  (0xE5)
                                        efuseCtrlAddress[2] = 1;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    default:
                                        MessageBox.Show("Test Parameter : " + ºTestParam + "(" + tempData[0].ToUpper() + ") - OTP Address not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                        break;
                                }

                                #region Burn OTP Data

                                if (BurnOTP)
                                {
                                    //Burn twice to double confirm the otp programming is done completely
                                    //JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                                    //JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);

                                    //Burn5x to double confirm the otp programming is done completely
                                    for (int cnt = 0; cnt < 5; cnt++)
                                    {
                                        JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                                    }
                                }

                                #endregion
                            }

                            #endregion

                            #region Read Back MIPI register

                            #region Turn off SMU and VIO - to prepare for read back mipi register
                            if (EqmtStatus.SMU)
                            {
                                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                            }

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(ºRdCurr_Delay);

                            //Set MIPI and Read MIPI
                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(ºRdCurr_Delay);
                            #endregion

                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 16;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length == effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        R_ReadMipiReg = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    //ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);
                                    if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                    {
                                        R_MIPI = 1;
                                    }
                                    BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                    BuildResults(ref results, ºTestParaName + "_MSB", "dec", dataDec[0]);
                                    BuildResults(ref results, ºTestParaName + "_LSB", "dec", dataDec[1]);

                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 15;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                                        //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(0, appendBinary.Length - effectiveBits);                //remove bits 7 from MSB to make effectiveData = 15 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);                                          //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    //ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);
                                    if (R_ReadMipiReg == otpUnitID)
                                    {
                                        R_MIPI = 1;

                                        // write Unit ID data to file
                                        try
                                        {
                                            ArrayList LocalTextList = new ArrayList();
                                            LocalTextList.Add("[SN_ID]");
                                            LocalTextList.Add("SN_COUNT = " + otpUnitID);

                                            IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                        }
                                        catch (FileNotFoundException)
                                        {
                                            throw new FileNotFoundException("Cannot Write Existing file!");
                                        }
                                    }

                                    BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                    BuildResults(ref results, ºTestParaName + "_MSB", "dec", dataDec[0]);
                                    BuildResults(ref results, ºTestParaName + "_LSB", "dec", dataDec[1]);

                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    //set to fail value (default)
                                    R_MIPI = 0;
                                    b_testFlag = false;
                                    i_testFlag = 0;

                                    //read test flag register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    if (BurnOTP)
                                    {
                                        if (b_testFlag && BurnOTP)
                                            R_MIPI = 1;         //Only return '1' if burn OTP Flag and Read Bit 1 = 1
                                        else
                                            R_MIPI = 0;         //Only return '0' if burn OTP Flag and Read Bit 1 = 0 (note - readback Bit 1 failure)
                                    }

                                    //Retest unit and report out accordingly - Note : Not from Fresh Lots , already tested before
                                    if (!BurnOTP)
                                    {
                                        //Retest Unit (2A lot) Test Count pass and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit1 = 1) already program
                                        if (FailedTests.Count == 0 && b_testFlag && !b_lockBit)
                                        {
                                            R_MIPI = 2;
                                        }
                                        //Retest Unit (2A lot) Test Count Fail and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit1 = 1) already program
                                        if (FailedTests.Count > 0 && b_testFlag && !b_lockBit)
                                        {
                                            R_MIPI = 3;
                                        }
                                        //Retest Unit (redo all) - pass SParam - lockBit(E5 = 1) and TestFlag(Bit1 = 1) already program
                                        if (FailedTests.Count == 0 && b_testFlag && b_lockBit)
                                        {
                                            R_MIPI = 4;
                                        }
                                        //Retest Unit (redo all) Test Count Fail and pass SParam - lockBit(E5 = 1) and TestFlag(Bit1 = 1) already program
                                        if (FailedTests.Count > 0 && b_testFlag && b_lockBit)
                                        {
                                            R_MIPI = 5;
                                        }
                                    }

                                    ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);

                                    break;

                                case "CM_ID":
                                case "CMID":
                                    //set to fail value (default)
                                    R_MIPI = 0;
                                    b_testFlag = false;
                                    i_testFlag = 0;

                                    //read test flag register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    if (BurnOTP)
                                    {
                                        if (b_testFlag && BurnOTP)
                                            R_MIPI = 1;         //Only return '1' if burn OTP Flag and Read Bit 7 = 1 (note - ASEKr = 1)
                                        else
                                            R_MIPI = 0;         //Only return '0' if burn OTP Flag and Read Bit 7 = 0 (note - AmKor = 0)
                                    }

                                    //Retest unit and report out accordingly - Note : Not from Fresh Lots , already tested before
                                    if (!BurnOTP)
                                    {
                                        //Fail burn otp - set to -999 as default data
                                        R_MIPI = -999;
                                    }

                                    ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);

                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        #endregion

                        case "MIPI_LEAKAGE":
                            #region MIPI Leakage Test
                            #region SMU Biasing
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = ºSMUSetCh;

                            SetSMU = ºSMUSetCh.Split(',');
                            MeasSMU = ºSMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                                EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);
                            }

                            EqSMUDriver.DcOn(SetSMUSelect, EqSMU);
                            #endregion

                            #region MIPI Biasing and MIPI command
                            //to select which Mipi Pin to set and measure - Format in TCF(ºMIPI_SetCh) 0,1 -> means S_CLK & S_DATA to set/measure
                            //Note : 0 -> S_CLK, 1 - S_DATA, 2 - S_VIO
                            VSetMIPI = ºMIPI_SetCh.Split(',');
                            IMeasMIPI = ºMIPI_MeasCh.Split(',');

                            LibEqmtDriver.MIPI.s_MIPI_DCSet[] tmpMipi_DCSet = new LibEqmtDriver.MIPI.s_MIPI_DCSet[VSetMIPI.Length];
                            LibEqmtDriver.MIPI.s_MIPI_DCMeas[] tmpMipi_DCMeas = new LibEqmtDriver.MIPI.s_MIPI_DCMeas[IMeasMIPI.Length];

                            for (int i = 0; i < VSetMIPI.Length; i++)
                            {
                                int mipiCh = Convert.ToInt16(VSetMIPI[i]);
                                tmpMipi_DCSet[i].ChNo = mipiCh;
                                tmpMipi_DCSet[i].VChSet = ºMIPI_VSetCh[mipiCh];
                                tmpMipi_DCSet[i].IChSet = ºMIPI_ILimitCh[mipiCh];
                            }

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            //Set DUT to PDM mode
                            EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                            LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ºReadSuccessful)
                                R_MIPI = 1;

                            //Set MIPI to PPMU Mode (Set Voltage and measure current)
                            EqMiPiCtrl.SetMeasureMIPIcurrent(ºRdCurr_Delay, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16), tmpMipi_DCSet, IMeasMIPI, out tmpMipi_DCMeas);

                            #endregion

                            #region SMU measurement
                            if (ºTest_SMU)
                            {
                                DelayMs(ºRdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (ºSMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region built MIPI Leakage result
                            for (int i = 0; i < tmpMipi_DCMeas.Length; i++)
                            {
                                BuildResults(ref results, ºTestParaName + "_" + tmpMipi_DCMeas[i].MipiPinNames, "A", tmpMipi_DCMeas[i].IChMeas);
                            }
                            #endregion

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + ºTestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;

                        #endregion
                    }
                    break;

                case "MIPI_OTP":                
                    switch (ºTestParam.ToUpper())
                    {
                        #region READ OTP REGISTER with customize bit selection
                        case "READ_OTP_SELECTIVE_BIT":

                            R_ReadMipiReg = -999;   //set to fail value (default)
                            R_MIPI = -999;          //set to fail value (default)
                            tmpOutData_DecConv = -999;  //set to fail value (default)

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);
                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out tmpOutData_DecConv, out dataSizeHex);

                            switch (ºSearch_Method.ToUpper())
                            {
                                case "UNITID":
                                case "UNIT_ID":

                                    R_ReadMipiReg = tmpOutData_DecConv;            
                                    
                                    if (R_ReadMipiReg == PreviousModID)
                                    {
                                        R_ReadMipiReg = -1;
                                        break;
                                    }
                                    else
                                    {
                                        PreviousModID = R_ReadMipiReg;
                                    }

                                    break;
                                  
                                default:
                                    R_ReadMipiReg = tmpOutData_DecConv;
                                    break;
                            }
                            
                            //Build Test Result
                            ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                            //BuildResults(ref results, ºTestParaName + "_STATUS", "NA", R_MIPI);
                            BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;
                        #endregion

                        #region BURN OTP REGISTER with customize bit selection
                        case "BURN_OTP_SELECTIVE_BIT":
                            //R_MIPI FLAG STATUS - REMARK
                            //R_MIPI = -1 > Register Not Blank , did not proceed to burn
                            //R_MIPI = 0 > Register Blank , proceed to burn , Read not same as write data
                            //R_MIPI = 1 > Register Blank , proceed to burn , Read same as write data

                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = ºSMUSetCh;
                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetSMU = ºSMUSetCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                                EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);

                                //Store the SMU Channel Label - to be reuse later during OTP Burn process
                                string tempLabel = "SMUI_CH" + SetSMU[i];
                                foreach (string key in DicTestLabel.Keys)
                                {
                                    if (key == tempLabel)
                                    {
                                        R_SMULabel_ICh[smuVChannel] = DicTestLabel[key].ToString().ToUpper();                                       
                                        break;
                                    }
                                }
                            }

                            EqSMUDriver.DcOn(SetSMUSelect, EqSMU);
                            #endregion

                            #region Initialize variable to default
                            //Initialize to default
                            b_lockBit = true;
                            i_lockBit = -999;
                            i_testFlag = -999;
                            b_testFlag = true;
                            BurnOTP = false;                           
                            dataDec_Conv = -999;
                            dataSizeHex = null;

                            R_ReadMipiReg = -999;   //set to fail value (default)
                            R_MIPI = -999;          //set to fail value (default)
                            tmpOutData_DecConv = -999;

                            appendHex = null;
                            biasDataArr = null;
                            dataHex = null;
                            #endregion

                            #region Read Register and return Data - derive from Mipi custom spreadsheet 
                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);
                            
                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                            dataHex = new string[biasDataArr.Length];

                            ReadValue = ºSearch_Value;

                            if (ºSearch_Method.ToUpper() == "MFG_ID") ReadValue = "E0:FF E1:FF";
                            else if (ºSearch_Method.ToUpper() == "UNIT_ID") ReadValue = "E3:3F E4:FF";
                            else if (ºSearch_Method.ToUpper() == "TEST_FLAG") ReadValue = "E3:80";

                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out tmpOutData_DecConv, out dataSizeHex);
                            #endregion

                  
                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    #region Burn Manufacturing ID
                                    dataDec_Conv = Convert.ToInt32(mfgLotID);         //convert string to int
                                    dataDec_Conv = 2;
                                    if (dataDec_Conv < (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }

                                    //read lock bit register and compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                        R_MIPI = -1;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("VBAT");
                                                if (found)
                                                {
                                                    EqSMUDriver.SetVolt(SMUSetting[i], EqSMU, (float)5.5, (float)0.3);
                                                }
                                            }

                                            burn_OTPReg_viaEffectiveBit(ºTestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                                            }

                                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(ºRdCurr_Delay);
                                            #endregion

                                            //read back register and compare with program data
                                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                            {
                                                R_MIPI = 1;
                                            }
                                            else
                                            {
                                                R_MIPI = 0;
                                            }

                                            #endregion
                                        }
                                    }
                                    #endregion
                                    break;

                                case "UNIT_ID":
                                case "UNITID":
                                    #region Burn Module ID

                                    //Set the DUT SN ID and Check if file exist , if not exist -> create and write default SN
                                    OTPLogFilePath = mfgLotID_Path + SNPFile.FileOutput_FileName + "_" + mfgLotID + ".txt";

                                    //tmpUnit_No = 1;

                                    if (tmpUnit_No == 1)
                                    {
                                        if (!Directory.Exists(@mfgLotID_Path))
                                            System.IO.Directory.CreateDirectory(@mfgLotID_Path);

                                        if (!File.Exists(@OTPLogFilePath))
                                        {
                                            //get the 1st running number for unit id from local setting file 
                                            StartNo_UNIT_ID = IO_TxtFile.ReadTextFile(LocSetFilePath, "TESTSITE_UNIT_ID", "START_UNIT_ID");
                                            StopNo_UNIT_ID = IO_TxtFile.ReadTextFile(LocSetFilePath, "TESTSITE_UNIT_ID", "STOP_UNIT_ID");

                                            // write default SN to file 
                                            try
                                            {
                                                ArrayList LocalTextList = new ArrayList();
                                                LocalTextList.Add("[SN_ID]");
                                                LocalTextList.Add("SN_COUNT = " + StartNo_UNIT_ID);     //write default start unit_id (unique starting number for every test site)

                                                IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                            }
                                            catch (FileNotFoundException)
                                            {
                                                throw new FileNotFoundException("Cannot Write Existing file!");
                                            }
                                        }
                                    }

                                    //read SN from files
                                    if (File.Exists(@OTPLogFilePath))
                                        tmpData = IO_TxtFile.ReadTextFile(OTPLogFilePath, "SN_ID", "SN_COUNT");

                                    otpUnitID = Convert.ToInt32(tmpData) + 1;       //next SN to burn 
                                    dataDec_Conv = otpUnitID;

                                    //dataDec_Conv = 15;
                                    //Set the DUT SN ID and Check if file exist , if not exist -> create and write default SN
                                    //dataDec_Conv = GetNextModuleID(Convert.ToInt32(dataSizeHex, 16), out b_testFlag);

                                    if ((dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16))) && (b_testFlag))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }

                                    //compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ºSearch_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                       // R_MIPI = -1;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                       // PreviousModID = R_ReadMipiReg;

                                        if (R_ReadMipiReg == PreviousModID)
                                        {
                                            R_MIPI = -1;
                                            R_ReadMipiReg = -1;
                                            break;
                                        }
                                        else
                                        {
                                            R_MIPI = 1;
                                            PreviousModID = R_ReadMipiReg;
                                        }

                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("VBAT");
                                                if (found)
                                                {
                                                    EqSMUDriver.SetVolt(SMUSetting[i], EqSMU, (float)5.5, (float)0.3);
                                                }
                                            }

                                            burn_OTPReg_viaEffectiveBit(ºTestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                                            }

                                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(ºRdCurr_Delay);

                                            #endregion

                                            //read back register and compare with program data
                                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if (R_ReadMipiReg == dataDec_Conv)
                                            {
                                                R_MIPI = 1;
                                            }
                                            else
                                            {
                                                R_MIPI = 0;
                                            }
                                            // write Unit ID data to file
                                            try
                                            {
                                                ArrayList LocalTextList = new ArrayList();
                                                LocalTextList.Add("[SN_ID]");
                                                LocalTextList.Add("SN_COUNT = " + otpUnitID);

                                                IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                            }
                                            catch (FileNotFoundException)
                                            {
                                                throw new FileNotFoundException("Cannot Write Existing file!");
                                            }

                                            #endregion
                                        }   
                                    }
                                    #endregion
                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    #region Burn Test Flag
                                    if (FailedTests.Count != 0)
                                    {
                                        BurnOTP = false;
                                        R_MIPI = -1;
                                        R_ReadMipiReg = -999;
                                        break;
                                    }
                                    else
                                    {
                                        //if all pass spec
                                        BurnOTP = true;

                                        //read lock bit register and compare Lockbit register
                                        readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ºSearch_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                        if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                        {
                                            R_MIPI = -1;
                                            R_ReadMipiReg = tmpOutData_DecConv;
                                            break;
                                        }
                                        else
                                        {
                                            //burn OTP register
                                            if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                            {
                                                //Set VBATT only to prepare for otp burn procedure
                                                for (int i = 0; i < SetSMU.Count(); i++)
                                                {
                                                    bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                    if (found)
                                                    {
                                                        EqSMUDriver.SetVolt(SMUSetting[i], EqSMU, (float)5.5, (float)0.1);
                                                    }
                                                }

                                                //Get the program value from Mipi custom spreadsheet 
                                                //example : E3:80 -> E3:1000 0000 (in Binary) - will program bit7 
                                                //regMapValue[0] = E3 , regMapValue[1] = 80
                                                //dataHex will be stored with 80
                                                //split string with blank space as delimiter
                                                biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                                for (int i = 0; i < biasDataArr.Length; i++)
                                                {
                                                    string[] regMapValue = biasDataArr[i].Split(':');
                                                    dataHex[i] = regMapValue[1];
                                                }

                                                burn_OTPReg_viaEffectiveBit(ºTestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                                #region Read Back MIPI register
                                                #region Turn off SMU and VIO - to prepare for read back mipi register
                                                if (EqmtStatus.SMU)
                                                {
                                                    EqSMUDriver.DcOff(SMUSetting, EqSMU);
                                                }

                                                EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                                DelayMs(ºRdCurr_Delay);
                                                #endregion

                                                //read back register and compare with program data
                                                mask_viaEffectiveBit(dataHex, ºSwBand, CusMipiRegMap, out dataDec_Conv);
                                                readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);
                                                if (R_ReadMipiReg == dataDec_Conv)
                                                {
                                                    R_MIPI = 1;
                                                }
                                                else
                                                {
                                                    R_MIPI = 0;
                                                }

                                                #endregion
                                            }
                                        }
                                    }

                                    #endregion
                                    break;

                                case "CM_ID":
                                case "CMID":
                                    #region Burn CM ID
                                    //Check CM base on Device ID scanning
                                    string[] tmpDeviceID = new string[3];
                                    try
                                    {
                                        tmpDeviceID = deviceID.Split('-');
                                    }
                                    catch(Exception ex)
                                    {
                                        throw new Exception("DEVICE ID FORMAT INCORRECT (ENGR-XXXX-Y) : " + deviceID + " -> " + ex.Message);
                                    }

                                    switch (tmpDeviceID[2].ToUpper())
                                    {
                                        case "M":
                                            //Amkor Assembly
                                            R_MIPI = 1;
                                            R_ReadMipiReg = tmpOutData_DecConv;
                                            BurnOTP = false;        //Amkor CM ID = 0 , thus not required to Burn
                                            break;
                                        case "A":
                                            //AseKr Assembly
                                            R_MIPI = 1;
                                            BurnOTP = true;         //ASEKr CM ID = 1 , thus Otp Burn is required
                                            break;
                                        default:
                                            MessageBox.Show("CM Site : " + tmpDeviceID[2].ToUpper() + " (CM SITE not supported at this moment)", "MyDUT", MessageBoxButtons.OK);
                                            R_MIPI = -999;
                                            BurnOTP = false;        //set flag to false for burning otp
                                            break;
                                    }
                                    
                                    #region Check Lockbit and CM ID register and Burn Register
                                    //read lock bit register and compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ºSearch_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                        R_MIPI = -1;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                if (found)
                                                {
                                                    EqSMUDriver.SetVolt(SMUSetting[i], EqSMU, (float)5.5, (float)0.1);
                                                }
                                            }

                                            //Get the program value from Mipi custom spreadsheet 
                                            //example : E3:80 -> E3:1000 0000 (in Binary) - will program bit7 
                                            //regMapValue[0] = E3 , regMapValue[1] = 80
                                            //dataHex will be stored with 80
                                            //split string with blank space as delimiter
                                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                            for (int i = 0; i < biasDataArr.Length; i++)
                                            {
                                                string[] regMapValue = biasDataArr[i].Split(':');
                                                dataHex[i] = regMapValue[1];
                                            }

                                            burn_OTPReg_viaEffectiveBit(ºTestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                                            }

                                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(ºRdCurr_Delay);
                                            #endregion

                                            //read back register and compare with program data
                                            mask_viaEffectiveBit(dataHex, ºSwBand, CusMipiRegMap, out dataDec_Conv);
                                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if ((R_ReadMipiReg == dataDec_Conv) && (BurnOTP))
                                            {
                                                //Good - Return Flag '1'
                                                R_MIPI = 1;
                                            }
                                            else if ((R_ReadMipiReg == 1) && (!BurnOTP))
                                            {
                                                //Bad - Return Flag '-999'
                                                R_MIPI = 0;
                                            }

                                            #endregion
                                        }
                                    }
                                    #endregion

                                    #endregion
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            //Build Test Result
                            ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                            BuildResults(ref results, ºTestParaName + "_STATUS", "NA", R_MIPI);
                            BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + ºTestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;

                        #endregion
                    }
                    break;

                case "MIPI_OTP_BLANTON":

               

                    switch (ºTestParam.ToUpper())
                    {
                 

                        #region READ OTP REGISTER with customize bit selection
                        case "READ_OTP_SELECTIVE_BIT":

                            R_ReadMipiReg = -999;   //set to fail value (default)
                            R_MIPI = -999;          //set to fail value (default)
                            tmpOutData_DecConv = -999;  //set to fail value (default)
                            ////Ivan
                            //tmpOutData = 0;
                            //totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                            //effectiveBits = 14;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                            dataBinary = new string[2];
                            appendBinary = null;
                            dataDec = new int[2];

                            //for (int i = 0; i < biasDataArr.Length; i++)
                            //{
                            //    EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                            //    dataDec[i] = tmpOutData;
                            //    dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                            //    appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                            //}

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            string ReadValue_forRead = CusMipiRegMap;

                            if (ºSearch_Method.ToUpper() == "MFG_ID") ReadValue_forRead = "E0:FF E1:FF";
                            else if (ºSearch_Method.ToUpper() == "UNIT_ID") ReadValue_forRead = "E3:3F E4:FF";
                            else if (ºSearch_Method.ToUpper() == "TEST_FLAG") ReadValue_forRead = "E3:80";

                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue_forRead, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out tmpOutData_DecConv, out dataSizeHex);

                            switch (ºSearch_Method.ToUpper())
                            {
                               // case "UNITID":
                                //case "UNIT_ID":

                                //    R_ReadMipiReg = tmpOutData_DecConv;

                                //    if (R_ReadMipiReg == PreviousModID)
                                //    {
                                //        R_ReadMipiReg = -1;
                                //        break;
                                //    }
                                //    else
                                //    {
                                //        PreviousModID = R_ReadMipiReg;
                                //    }

                                //    break;
                                case "UNIT_ID":
                                    R_ReadMipiReg = -999;   //set to fail value (default)
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 14;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                                        //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(0, appendBinary.Length - effectiveBits);                //remove first 2Bits from MSB to make effectiveData = 14 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);                                          //Convert Binary to Decimal
                                    }

                                    if (R_ReadMipiReg == PreviousModID)
                                    {
                                        R_ReadMipiReg = -1;
                                        break;
                                    }
                                    else
                                    {
                                        PreviousModID = R_ReadMipiReg;
                                    }

                                    break;

                                    //Ivan
                                case "WAFER-ID":
                                    R_ReadMipiReg = -999;   //set to fail value (default)
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 8;         //total bit for 1 register address is 8bits (binary)
                                    effectiveBits = 6;     //prod ID use all 8bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');//Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(6, appendBinary.Length - effectiveBits);                //remove first 2Bits from MSB to make effectiveData = 6 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);  //Convert Binary to Decimal                     
                                    }

                                    break;

                                //Ivan
                                case "WAFER-LOT":
                                    R_ReadMipiReg = -999;   //set to fail value (default)
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 1 register address is 8bits (binary)
                                    effectiveBits = 10;     //prod ID use all 10bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');//Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(0, appendBinary.Length - effectiveBits);                //remove first 2Bits from MSB to make effectiveData = 6 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);  //Convert Binary to Decimal                     
                                    }

                                    break;

                                //Ivan
                                case "PROD_ID":
                                    R_ReadMipiReg = -999;   //set to fail value (default)
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 8;         //total bit for 1 register address is 8bits (binary)
                                    effectiveBits = 8;     //prod ID use all 8bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length == effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        R_ReadMipiReg = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal
                                    }
                                    break;

                                default:
                                    R_ReadMipiReg = tmpOutData_DecConv;
                                    break;
                            }

                            //Build Test Result
                            ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                            //BuildResults(ref results, ºTestParaName + "_STATUS", "NA", R_MIPI);
                            if (ºSearch_Method.Contains("WAFER-ID"))
                            {
                                BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                            }
                            else if (ºSearch_Method.Contains("PROD_ID"))
                             {
                                BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                             }
                            else if (ºSearch_Method.Contains("UNIT_ID"))
                            {
                                BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                appendBinary = dataBinary[0];
                                tmpOutData = Convert.ToInt32(appendBinary);
                                dataBinary[0] = appendBinary.Remove(0, appendBinary.Length - 6);
                                dataDec[0] = Convert.ToInt32(dataBinary[0], 2);  //Convert Binary to Decimal 
                                BuildResults(ref results, ºTestParaName + "_MSB", "dec", dataDec[0]);
                                BuildResults(ref results, ºTestParaName + "_LSB", "dec", dataDec[1]);
                            }
                            else if (ºSearch_Method.Contains("CMOS-TX"))
                            {
                                BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                BuildResults(ref results, ºTestParaName + "-X", "dec", dataDec[0]);
                                BuildResults(ref results, ºTestParaName + "-Y", "dec", dataDec[1]);
                            }
                            else if (ºSearch_Method.Contains("WAFER-LOT"))
                            {
                                BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                appendBinary = dataBinary[0];
                                tmpOutData = Convert.ToInt32(appendBinary);
                                dataBinary[0] = appendBinary.Remove(0, appendBinary.Length - 2);                              
                                dataDec[0] = Convert.ToInt32(dataBinary[0], 2);  //Convert Binary to Decimal 
                                BuildResults(ref results, ºTestParaName + "-MSB", "dec", dataDec[0]);
                                BuildResults(ref results, ºTestParaName + "-LSB", "dec", dataDec[1]);
                            }
                            else
                            {
                                BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                                //Ivan
                                BuildResults(ref results, ºTestParaName + "_MSB", "dec", dataDec[0]);
                                BuildResults(ref results, ºTestParaName + "_LSB", "dec", dataDec[1]);
                            }
                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;
                        #endregion

                        #region BURN OTP REGISTER with customize bit selection
                        case "BURN_OTP_SELECTIVE_BIT":
                            //R_MIPI FLAG STATUS - REMARK
                            //R_MIPI = -1 > Register Not Blank , did not proceed to burn
                            //R_MIPI = 0 > Register Blank , proceed to burn , Read not same as write data
                            //R_MIPI = 1 > Register Blank , proceed to burn , Read same as write data

                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = ºSMUSetCh;
                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetSMU = ºSMUSetCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = SMUSetting[smuVChannel];       //rearrange the SMUSetting base on reqquired channel only from total of 8 channel available  
                                EqSMUDriver.SetVolt(SMUSetting[smuVChannel], EqSMU, ºSMUVCh[smuVChannel], ºSMUILimitCh[smuVChannel]);

                                //Store the SMU Channel Label - to be reuse later during OTP Burn process
                                string tempLabel = "SMUI_CH" + SetSMU[i];
                                foreach (string key in DicTestLabel.Keys)
                                {
                                    if (key == tempLabel)
                                    {
                                        R_SMULabel_ICh[smuVChannel] = DicTestLabel[key].ToString().ToUpper();
                                        break;
                                    }
                                }
                            }

                            EqSMUDriver.DcOn(SetSMUSelect, EqSMU);
                            #endregion

                            #region Initialize variable to default
                            //Initialize to default
                            b_lockBit = true;
                            i_lockBit = -999;
                            b_lockBit2 = true;
                            i_lockBit2 = -999;
                            i_testFlag = -999;
                            b_testFlag = true;
                            BurnOTP = false;
                            dataDec_Conv = -999;
                            dataSizeHex = null;

                            R_ReadMipiReg = -999;   //set to fail value (default)
                            R_MIPI = -999;          //set to fail value (default)
                            tmpOutData_DecConv = -999;

                            appendHex = null;
                            biasDataArr = null;
                            dataHex = null;
                            #endregion

                            #region Read Register and return Data - derive from Mipi custom spreadsheet
                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(ºTestParam, ºSwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                            dataHex = new string[biasDataArr.Length];

                            string ReadValue_forWrite = CusMipiRegMap;

                            if (ºSearch_Method.ToUpper() == "MFG_ID") ReadValue_forWrite = "E0:FF E1:FF";
                            else if (ºSearch_Method.ToUpper() == "UNIT_ID")
                            {
                                ReadValue_forWrite = "E3:3F E4:FF";
                            }
                            else if (ºSearch_Method.ToUpper() == "TEST_FLAG")
                            {
                                ReadValue_forWrite = "E3:80";
                            }

                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue_forWrite, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out tmpOutData_DecConv, out dataSizeHex);
                            #endregion

                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    #region Burn Manufacturing ID

                    
                                    dataDec_Conv = Convert.ToInt32(mfgLotID);         //convert string to int
                                    if (dataDec_Conv < (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }

                                    if (!BurnOTP) R_MIPI = -2;
                                    //read lock bit register and compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, "EB:80", CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);


                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                        R_MIPI = -1;
                                        if (tmpOutData_DecConv != 0) R_MIPI = -3;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("VBAT");
                                                if (found)
                                                {
                                                    EqSMUDriver.SetVolt(SMUSetting[i], EqSMU, (float)5.5, (float)0.3);
                                                }
                                            }

                                            burn_OTPReg_viaEffectiveBit(ºTestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                                            }

                                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(ºRdCurr_Delay);
                                            #endregion

                                            //read back register and compare with program data
                                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue_forWrite, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                            {
                                                R_MIPI = 1;
                                            }
                                            else
                                            {
                                                R_MIPI = 0;
                                            }

                                            #endregion
                                        }
                                    }
                                    #endregion
                                    break;

                                case "UNIT_ID":
                                case "UNITID":
                                    #region Burn Module ID
                                    //Set the DUT SN ID and Check if file exist , if not exist -> create and write default SN
                                    dataDec_Conv = GetNextModuleID(Convert.ToInt32(dataSizeHex, 16), out b_testFlag);

                                    if ((dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16))) && (b_testFlag))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                       
                                    }
                                    if (!BurnOTP) R_MIPI = -2;
                                    //compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, "EB:80", CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                        R_MIPI = -1;
                                        if (tmpOutData_DecConv != 0) R_MIPI = -3;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("VBAT");
                                                if (found)
                                                {
                                                    EqSMUDriver.SetVolt(SMUSetting[i], EqSMU, (float)5.5, (float)0.3);
                                                }
                                            }

                                            burn_OTPReg_viaEffectiveBit(ºTestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                                            }

                                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(ºRdCurr_Delay);

                                            #endregion

                                            //read back register and compare with program data
                                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue_forWrite, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if (R_ReadMipiReg == dataDec_Conv)
                                            {
                                                R_MIPI = 1;
                                            }
                                            else
                                            {
                                                R_MIPI = 0;
                                            }

                                            #endregion
                                        }
                                    }
                                    #endregion
                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    #region Burn Test Flag
                                    //Ivan
                                    b_testFlag = false;
                                    i_testFlag = 0; 
                                   // b_lockBit2 = false;
                                   // i_lockBit2 = -999;

                                    //read test flag register
                                    EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, ReadValue_forWrite, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    if (FailedTests.Count != 0)
                                    {
                                        BurnOTP = false;
                                        //R_MIPI = -1;
                                        //R_ReadMipiReg = -999;
                                        break;
                                    }
                                   
                                    else
                                    {    
                                        BurnOTP = true;

                                        //read lock bit register and compare Lockbit register
                                        readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue_forWrite, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                        if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                        {
                                            R_MIPI = -1;
                                            R_ReadMipiReg = tmpOutData_DecConv;
                                            break;
                                        }                                            
                                        else
                                        {
                                            //burn OTP register
                                            if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                            {
                                                //Set VBATT only to prepare for otp burn procedure
                                                for (int i = 0; i < SetSMU.Count(); i++)
                                                {
                                                    bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                    if (found)
                                                    {
                                                        EqSMUDriver.SetVolt(SMUSetting[i], EqSMU, (float)5.5, (float)0.1);
                                                    }
                                                }

                                                //Get the program value from Mipi custom spreadsheet 
                                                //example : E3:80 -> E3:1000 0000 (in Binary) - will program bit7 
                                                //regMapValue[0] = E3 , regMapValue[1] = 80
                                                //dataHex will be stored with 80
                                                //split string with blank space as delimiter
                                                biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                                for (int i = 0; i < biasDataArr.Length; i++)
                                                {
                                                    string[] regMapValue = biasDataArr[i].Split(':');
                                                    dataHex[i] = regMapValue[1];
                                                }

                                                burn_OTPReg_viaEffectiveBit(ºTestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                                #region Read Back MIPI register
                                                #region Turn off SMU and VIO - to prepare for read back mipi register
                                                if (EqmtStatus.SMU)
                                                {
                                                    EqSMUDriver.DcOff(SMUSetting, EqSMU);
                                                }

                                                EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                                DelayMs(ºRdCurr_Delay);
                                                #endregion

                                                //read back register and compare with program data
                                                mask_viaEffectiveBit(dataHex, ºSwBand, ReadValue_forWrite, out dataDec_Conv);
                                                readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ReadValue_forWrite, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);
                                                if (R_ReadMipiReg == dataDec_Conv)
                                                {
                                                    R_MIPI = 1;
                                                }
                                                else
                                                {
                                                    R_MIPI = 0;
                                                }
                                                #endregion                                              
                                            }
                                        }
                                    }

                                    #endregion
                                    break;

                                case "CM_ID":
                                case "CMID":
                                    #region Burn CM ID
                                    //Check CM base on Device ID scanning
                                    string[] tmpDeviceID = new string[3];
                                    try
                                    {
                                        tmpDeviceID = deviceID.Split('-');
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("DEVICE ID FORMAT INCORRECT (ENGR-XXXX-Y) : " + deviceID + " -> " + ex.Message);
                                    }

                                    switch (tmpDeviceID[2].ToUpper())
                                    {
                                        case "M":
                                            //Amkor Assembly
                                            R_MIPI = 1;
                                            R_ReadMipiReg = tmpOutData_DecConv;
                                            BurnOTP = false;        //Amkor CM ID = 0 , thus not required to Burn
                                            break;
                                        case "A":
                                            //AseKr Assembly
                                            R_MIPI = 1;
                                            BurnOTP = true;         //ASEKr CM ID = 1 , thus Otp Burn is required
                                            break;
                                        default:
                                            MessageBox.Show("CM Site : " + tmpDeviceID[2].ToUpper() + " (CM SITE not supported at this moment)", "MyDUT", MessageBoxButtons.OK);
                                            R_MIPI = -999;
                                            BurnOTP = false;        //set flag to false for burning otp
                                            break;
                                    }

                                    #region Check Lockbit and CM ID register and Burn Register
                                    //read lock bit register and compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, ºSearch_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                        R_MIPI = -1;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                if (found)
                                                {
                                                    EqSMUDriver.SetVolt(SMUSetting[i], EqSMU, (float)5.5, (float)0.1);
                                                }
                                            }

                                            //Get the program value from Mipi custom spreadsheet 
                                            //example : E3:80 -> E3:1000 0000 (in Binary) - will program bit7 
                                            //regMapValue[0] = E3 , regMapValue[1] = 80
                                            //dataHex will be stored with 80
                                            //split string with blank space as delimiter
                                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                            for (int i = 0; i < biasDataArr.Length; i++)
                                            {
                                                string[] regMapValue = biasDataArr[i].Split(':');
                                                dataHex[i] = regMapValue[1];
                                            }

                                            burn_OTPReg_viaEffectiveBit(ºTestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                EqSMUDriver.DcOff(SMUSetting, EqSMU);
                                            }

                                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(ºRdCurr_Delay);
                                            #endregion

                                            //read back register and compare with program data
                                            mask_viaEffectiveBit(dataHex, ºSwBand, CusMipiRegMap, out dataDec_Conv);
                                            readout_OTPReg_viaEffectiveBit(ºRdCurr_Delay, ºSwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if ((R_ReadMipiReg == dataDec_Conv) && (BurnOTP))
                                            {
                                                //Good - Return Flag '1'
                                                R_MIPI = 1;
                                            }
                                            else if ((R_ReadMipiReg == 1) && (!BurnOTP))
                                            {
                                                //Bad - Return Flag '-999'
                                                R_MIPI = 0;
                                            }

                                            #endregion
                                        }
                                    }
                                    #endregion

                                    #endregion
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            //Ivan- To check the RF2 lockbit
                            ºSearch_Value= ("EB:80");                      
                            readout_OTPReg_viaEffectiveBit2(ºRdCurr_Delay, ºSwBand, ºSearch_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit2, out dummyStrData);
                            b_lockBit2 = (i_lockBit2 != 0);    // '0' not program , '1' have program     

                            if (R_MIPI > 0)
                            {
                                if (FailedTests.Count > 0 && !b_testFlag && !b_lockBit2)
                                {
                                    R_MIPI = 0;
                                }

                                if (FailedTests.Count == 0 && b_testFlag && !b_lockBit2)
                                {
                                    R_MIPI = 2;
                                }
                                //Retest Unit (2A lot) Test Count Fail and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit1 = 1) already program
                                if (FailedTests.Count > 0 && b_testFlag && !b_lockBit2)
                                {
                                    R_MIPI = 3;
                                }
                                //Retest Unit (redo all) - pass SParam - lockBit(E5 = 1) and TestFlag(Bit1 = 1) already program
                                if (FailedTests.Count == 0 && b_testFlag && b_lockBit2)
                                {
                                    R_MIPI = 4;
                                }
                                //Retest Unit (redo all) Test Count Fail and pass SParam - lockBit(E5 = 1) and TestFlag(Bit1 = 1) already program
                                if (FailedTests.Count > 0 && b_testFlag && b_lockBit2)
                                {
                                    R_MIPI = 5;
                                }
                            }
                            //Build Test Result
                            ºTest_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                            //BuildResults(ref results, ºTestParaName + "_STATUS", "NA", R_MIPI);
                            //BuildResults(ref results, ºTestParaName, "dec", R_ReadMipiReg);
                            //Ivan
                            BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);

                            EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + ºTestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;

                        #endregion
                    }
                    break;

                case "SWITCH":
                    switch (ºTestParam.ToUpper())
                    {
                        #region SWITCH Setup
                        case "SETSWITCH":
                            R_Switch = 0;
                            if (PreviousSWMode != ºSwBand.ToUpper())
                            {
                                R_Switch = 1;   //Status Switch = 1 , else not switching = 0
                                EqSwitch.SetPath(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], TCF_Header.ConstSwBand, ºSwBand.ToUpper()));
                                PreviousSWMode = ºSwBand.ToUpper();
                                DelayMs(ºSetup_Delay);
                            }

                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            break;

                        #endregion
                    }
                    break;

                case "NF":
                    switch (ºTestParam.ToUpper())
                    {
                        #region NF test function
                        case "NF_CA_NDIAG":
                            // This sweep is a faster sweep , it is a continuous sweep base on SG freq sweep mode
                            #region NF CA NDIAG

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            rx2_span = 0;
                            rx2_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            //DelayMs(ºStartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG , MXA1 and MXA2 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            EqSG01.SetFreq(Convert.ToDouble(ºSG1_DefaultFreq));

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";

                            if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = ºStopTXFreq1 - ºStartTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / ºStepTXFreq1);
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                EqSG01.SET_START_FREQUENCY(ºStartTXFreq1 - (ºStepTXFreq1 / 2));
                                EqSG01.SET_STOP_FREQUENCY(ºStopTXFreq1 + (ºStepTXFreq1 / 2));
                                EqSG01.SET_TRIG_TIMERPERIOD(ºDwellT1);
                                EqSG01.SET_SWEEP_POINT(tx1_noPoints + 2);   //need to add additional 2 points to calculated no of points because of extra point of start_freq and stop_freq for MXG and MXA sync

                                SGTargetPin = ºPin1 - totalInputLoss;
                                EqSG01.SetAmplitude((float)SGTargetPin);
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), ºWaveFormName);
                                EqSG01.SELECT_WAVEFORM(ModulationType);
                                EqSG01.SET_ROUTE_CONN_TOUT(LibEqmtDriver.SG.N5182A_ROUTE_SUBSYS.SweepRun);

                                EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode
                                if (ºSetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(ºSetup_Delay);
                                rx1_span = (ºStopRXFreq1 - ºStartRXFreq1);
                                rx1_cntrfreq = ºStartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                //EqSA01.AUTO_ATTENUATION(true); //Anthony
                                EqSA01.AUTO_ATTENUATION(false); //Anthony
                                if (Convert.ToDouble(ºSA1att) != CurrentSaAttn) //Anthony
                                {
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA1att));
                                    CurrentSaAttn = Convert.ToDouble(ºSA1att);
                                }
                                EqSA01.TRIGGER_SINGLE();
                                EqSA01.TRACE_AVERAGE(1);
                                EqSA01.AVERAGE_OFF();

                                EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                EqSA01.SPAN(rx1_span);
                                EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                //EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                EqSA01.SWEEP_POINTS(rx1_mxa_nopts);

                                if (ºSetRX1NDiag)
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(ºSA1att));
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)ºDwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * ºDwellT1));
                                }
                                else
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(ºRX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                EqSA01.CLEAR_WRITE();

                                status = EqSA01.OPERATION_COMPLETE();

                                #endregion

                                #region MXA 2 setup
                                DelayMs(ºSetup_Delay);
                                rx2_span = (ºStopRXFreq2 - ºStartRXFreq2);
                                rx2_cntrfreq = ºStartRXFreq2 + (rx2_span / 2);
                                rx2_mxa_nopts = (int)((rx2_span / rx2_mxa_nopts_step) + 1);

                                EqSA02.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                //EqSA02.AUTO_ATTENUATION(true); //Anthony
                                EqSA02.AUTO_ATTENUATION(false);
                                if (Convert.ToDouble(ºSA1att) != CurrentSa2Attn) //Anthony
                                {
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA2att));
                                    CurrentSaAttn = Convert.ToDouble(ºSA2att);
                                }
                                EqSA02.TRIGGER_SINGLE();
                                EqSA02.TRACE_AVERAGE(1);
                                EqSA02.AVERAGE_OFF();

                                EqSA02.FREQ_CENT(rx2_cntrfreq.ToString(), "MHz");
                                EqSA02.SPAN(rx2_span);
                                EqSA02.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA02.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA02.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                EqSA02.SWEEP_POINTS(rx2_mxa_nopts);
                                //EqSA02.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);

                                if (ºSetRX2NDiag)
                                {
                                    EqSA02.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(ºSA1att));
                                    EqSA02.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)ºDwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    EqSA02.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    EqSA02.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * ºDwellT1));
                                }
                                else
                                {
                                    EqSA02.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA02.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    EqSA02.SWEEP_TIMES(Convert.ToInt16(ºRX2SweepT));
                                }

                                //Initialize & clear MXA trace
                                EqSA02.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx2_cntrfreq);
                                EqSA02.CLEAR_WRITE();

                                status = EqSA02.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                if (!ºTunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    if (ºTest_Pout1)
                                    {
                                        DelayMs(ºRdPwr_Delay);
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default

                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        //R_Pin = TargetPin + (float)ºLossInputPathSG1;
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }
                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºStartSync_Delay)
                            {
                                syncTest_Delay = (long)ºStartSync_Delay - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            if (pwrSearch)
                            {
                                EqSG01.SINGLE_SWEEP();
                                status = EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(ºTrig_Delay);
                                Capture_MXA1_Trace(1, ºTestNum, ºTestParaName, ºRX1Band, prevRslt, ºSave_MXATrace);
                                Read_MXA1_Trace(1, ºTestNum, out R_NF1_Freq, out R_NF1_Ampl, ºSearch_Method, ºTestParaName);
                                Capture_MXA2_Trace(1, ºTestNum, ºTestParaName, ºRX2Band, prevRslt, ºSave_MXATrace);
                                Read_MXA2_Trace(1, ºTestNum, out R_NF2_Freq, out R_NF2_Ampl, ºSearch_Method, ºTestParaName);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref ºLossOutputPathRX1, ref StrError);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX2CalSegm, R_NF2_Freq, ref ºLossOutputPathRX2, ref StrError);

                                R_NF1_Ampl = R_NF1_Ampl - ºLossOutputPathRX1 - tbOutputLoss;
                                R_NF2_Ampl = R_NF2_Ampl - ºLossOutputPathRX2 - tbOutputLoss;

                                //Save_MXA1Trace(1, ºTestParaName, ºSave_MXATrace);
                                //Save_MXA2Trace(1, ºTestParaName, ºSave_MXATrace);

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF2_Freq = -999;

                                R_NF1_Ampl = 999;
                                R_NF2_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (ºOffSG1)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            EqSA01.CLEAR_WRITE();
                            EqSA02.CLEAR_WRITE();
                            //EqSA01.SET_TRACE_DETECTOR("MAXHOLD");
                            //EqSA02.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(ºStopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "NF_NONCA_NDIAG":
                            // This sweep is a faster sweep , it is a continuous sweep base on SG freq sweep mode
                            #region NF NONCA NDIAG

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            SAReferenceLevel = -20;
                            vBW_Hz = 300;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            //DelayMs(ºStartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset


                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            EqSG01.SetFreq(Convert.ToDouble(ºSG1_DefaultFreq));

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";

                            if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = ºStopTXFreq1 - ºStartTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / ºStepTXFreq1);
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                EqSG01.SET_START_FREQUENCY(ºStartTXFreq1 - (ºStepTXFreq1 / 2));
                                EqSG01.SET_STOP_FREQUENCY(ºStopTXFreq1 + (ºStepTXFreq1 / 2));
                                EqSG01.SET_TRIG_TIMERPERIOD(ºDwellT1);
                                EqSG01.SET_SWEEP_POINT(tx1_noPoints + 2);   //need to add additional 2 points to calculated no of points because of extra point of start_freq and stop_freq for MXG and MXA sync

                                SGTargetPin = ºPin1 - totalInputLoss;
                                EqSG01.SetAmplitude((float)SGTargetPin);
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), ºWaveFormName);
                                EqSG01.SELECT_WAVEFORM(ModulationType);
                                EqSG01.SET_ROUTE_CONN_TOUT(LibEqmtDriver.SG.N5182A_ROUTE_SUBSYS.SweepRun);
                                EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode

                                if (ºSetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(ºSetup_Delay);
                                rx1_span = (ºStopRXFreq1 - ºStartRXFreq1);
                                rx1_cntrfreq = ºStartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);

                                //ANTHONY-ATT
                                EqSA01.AUTO_ATTENUATION(false);
                                if (Convert.ToDouble(ºSA1att) != CurrentSaAttn)
                                {
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA1att));
                                    CurrentSaAttn = Convert.ToDouble(ºSA1att);
                                }

                                //EqSA01.ELEC_ATTEN_ENABLE(true);
                                EqSA01.TRIGGER_SINGLE();
                                EqSA01.TRACE_AVERAGE(1);
                                EqSA01.AVERAGE_OFF();

                                EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                EqSA01.SPAN(rx1_span);
                                EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                EqSA01.SWEEP_POINTS(rx1_mxa_nopts);

                                if (ºSetRX1NDiag)
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    //ANTHONY-ATT
                                    if (Convert.ToDouble(ºSA1att) != CurrentSaAttn) //Anthony
                                    {
                                        EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA1att));
                                        CurrentSaAttn = Convert.ToDouble(ºSA1att);
                                    }
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)ºDwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * ºDwellT1));
                                }
                                else
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    //ANTHONY-ATT
                                    if (myUtility.MXA_Setting.Attenuation != CurrentSaAttn)
                                    {
                                        EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                        CurrentSaAttn = Convert.ToDouble(myUtility.MXA_Setting.Attenuation);
                                    }
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(ºRX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                EqSA01.CLEAR_WRITE();
                                status = EqSA01.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                //Just for maximator Special case // Trick - 39mA  21.06.16
                                EqSMUDriver.SetVolt(SMUSetting[1], EqSMU, ºSMUVCh[1], ºSMUILimitCh[2]);

                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                if (!ºTunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    if (ºTest_Pout1)
                                    {
                                        DelayMs(ºRdPwr_Delay);
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        //R_Pin = TargetPin + (float)ºLossInputPathSG1;
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºStartSync_Delay)
                            {
                                syncTest_Delay = (long)ºStartSync_Delay - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            if (pwrSearch)
                            {
                                EqSG01.SINGLE_SWEEP();
                                status = EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(ºTrig_Delay);
                                Capture_MXA1_Trace(1, ºTestNum, ºTestParaName, ºRX1Band, prevRslt, ºSave_MXATrace);
                                Read_MXA1_Trace(1, ºTestNum, out R_NF1_Freq, out R_NF1_Ampl, ºSearch_Method, ºTestParaName);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref ºLossOutputPathRX1, ref StrError);
                                R_NF1_Ampl = R_NF1_Ampl - ºLossOutputPathRX1 - tbOutputLoss;
                                //Save_MXA1Trace(1, ºTestParaName, ºSave_MXATrace);

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (ºOffSG1)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            EqSA01.CLEAR_WRITE();
                            //EqSA01.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(ºStopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "NF_FIX_NMAX":
                            // This sweep is a slow sweep , will change SG freq and measure NF for every test points
                            // Using Marker Function Noise (measure at dBm/Hz) with External Amp Gain Offset
                            #region NOISE STEP SWEEP NDIAG/NMAX

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            DelayMs(ºStartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], ºCalTag.ToUpper(), ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Calc Average Pathloss, PowerSensor Offset, MXG and MXA1 configuration

                            #region Get Average Pathloss
                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            if (ºStopTXFreq1 == ºStartTXFreq1)
                            {
                                txcount = 1;
                            }
                            else
                            {
                                txcount = Convert.ToInt16(((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1) + 1);
                            }

                            tx_freqArray = new double[txcount];
                            contactPwr_Array = new double[txcount];
                            nfAmpl_Array = new double[txcount];
                            nfAmplFreq_Array = new double[txcount];

                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i < txcount; i++)
                            {
                                tx_freqArray[i] = ºTXFreq;

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;

                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }
                            //Calculate the average pathloss/pathgain
                            tmpAveInputLoss = tmpInputLoss / txcount;
                            tmpAveCouplerLoss = tmpCouplerLoss / txcount;
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //Get average pathloss base on start and stop freq
                            rxcount = Convert.ToInt16(((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1) + 1);
                            rx_freqArray = new double[rxcount];

                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i < rxcount; i++)
                            {
                                rx_freqArray[i] = ºRXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = tmpRxLoss + (float)ºLossOutputPathRX1;
                                ºRXFreq = ºRXFreq + ºStepRXFreq1;
                            }
                            tmpAveRxLoss = tmpRxLoss / rxcount;
                            totalRXLoss = tmpAveRxLoss - tbOutputLoss;
                            #endregion

                            #region config Power Sensor, MXA and MXG
                            //change PowerSensor,  Set Default Power for MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = ºPin1 - totalInputLoss;
                            EqSG01.SetAmplitude((float)SGTargetPin);
                            EqSG01.SetFreq(Convert.ToDouble(ºStartTXFreq1));
                            EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";

                            if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.FIX);
                                EqSG01.SetFreq(Math.Abs(Convert.ToDouble(ºStartTXFreq1)));

                                SGTargetPin = ºPin1 - totalInputLoss;
                                EqSG01.SetAmplitude((float)SGTargetPin);
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), ºWaveFormName);
                                EqSG01.SELECT_WAVEFORM(ModulationType);
                                EqSG01.SINGLE_SWEEP();

                                if (ºSetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(ºSetup_Delay);
                                if (ºSetRX1NDiag)
                                {
                                    //NDIAG - RX Bandwidth base on stepsize
                                    rx1_span = ºStepRXFreq1 * 2;
                                    rx1_cntrfreq = ºStartRXFreq1;
                                    rx1_mxa_nopts = 101;    //fixed no of points
                                }
                                else
                                {
                                    //NMAX - will use full RX Badwidth (StartRX to StopRX)
                                    rx1_span = ºStopRXFreq1 - ºStartRXFreq1;
                                    rx1_cntrfreq = Math.Round(ºStartRXFreq1 + (rx1_span / 2), 3);
                                    rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);
                                }

                                EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_FreeRun);
                                EqSA01.AUTO_ATTENUATION(false);
                                EqSA01.CONTINUOUS_MEASUREMENT_OFF();
                                EqSA01.TRACE_AVERAGE(1);
                                EqSA01.AVERAGE_OFF();

                                EqSA01.SPAN(rx1_span);
                                EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                //EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                EqSA01.SWEEP_POINTS(rx1_mxa_nopts);
                                EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(ºSA1att));
                                EqSA01.SWEEP_TIMES(Convert.ToInt16(ºRX1SweepT));

                                //Initialize & clear MXA trace
                                EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)ºStartRXFreq1);
                                EqSA01.CLEAR_WRITE();

                                status = EqSG01.OPERATION_COMPLETE();
                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                            }
                            #endregion

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                                if (!ºTunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default

                                    if (ºTest_Pout1)
                                    {
                                        DelayMs(ºRdPwr_Delay);
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }

                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                for (int i = 0; i < tx_freqArray.Length; i++)
                                {
                                    if (ºSetRX1NDiag)   //NDIAG - RX Bandwidth base on stepsize else NMAX - will use full RX Badwidth (StartRX to StopRX)
                                    {
                                        EqSA01.FREQ_CENT(rx_freqArray[i].ToString(), "MHz");    //RX Bandwidth base on stepsize
                                        EqSG01.SetFreq(Convert.ToDouble(tx_freqArray[i]));
                                        EqSA01.TRIGGER_IMM();
                                        DelayMs(Convert.ToInt16(ºRX1SweepT));       //Need to set same delay as sweep time before read trace  

                                        status = EqSG01.OPERATION_COMPLETE();
                                        Capture_MXA1_Trace(1, ºTestNum, ºTestParaName, ºRX1Band, prevRslt, false);
                                        Read_MXA1_Trace(1, ºTestNum, out nfAmplFreq_Array[i], out nfAmpl_Array[i], ºSearch_Method, ºTestParaName);
                                        nfAmpl_Array[i] = nfAmpl_Array[i] - totalRXLoss;
                                    }
                                    else
                                    {
                                        EqSG01.SetFreq(Convert.ToDouble(tx_freqArray[i]));
                                        EqSA01.TRIGGER_IMM();
                                        DelayMs(ºTrig_Delay);

                                        status = EqSG01.OPERATION_COMPLETE();
                                        Capture_MXA1_Trace(1, ºTestNum, ºTestParaName, ºRX1Band, prevRslt, ºSave_MXATrace);
                                        Read_MXA1_Trace(1, ºTestNum, out nfAmplFreq_Array[i], out nfAmpl_Array[i], ºSearch_Method, ºTestParaName);
                                        nfAmpl_Array[i] = nfAmpl_Array[i] - totalRXLoss;
                                    }
                                }

                                #region Search result MAX or MIN and Save to Datalog
                                //Find result MAX or MIN result
                                switch (ºSearch_Method.ToUpper())
                                {
                                    case "MAX":
                                        R_NF1_Ampl = nfAmpl_Array.Max();
                                        indexdata = Array.IndexOf(nfAmpl_Array, R_NF1_Ampl);     //return index of max value
                                        R_NF1_Freq = nfAmplFreq_Array[indexdata];
                                        break;

                                    case "MIN":
                                        R_NF1_Ampl = nfAmpl_Array.Min();
                                        indexdata = Array.IndexOf(nfAmpl_Array, R_NF1_Ampl);     //return index of max value
                                        R_NF1_Freq = nfAmplFreq_Array[indexdata];
                                        break;

                                    default:
                                        MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                        break;
                                }

                                //Save all data to datalog 
                                if (ºSetRX1NDiag)           //save trace method is different between NDIAG and NMAX
                                {
                                    if (ºSave_MXATrace)
                                    {
                                        string[] templine = new string[4];
                                        ArrayList LocalTextList = new ArrayList();
                                        ArrayList tmpCalMsg = new ArrayList();

                                        //Calibration File Header
                                        LocalTextList.Add("#MXA1 NF STEP SWEEP DATALOG");
                                        LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                                        LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                                        LocalTextList.Add("#Input TX Power : " + ºPin1 + " dBm");
                                        LocalTextList.Add("#Measure Contact Power : " + Math.Round(R_Pout1, 3) + " dBm");
                                        templine[0] = "#TX_FREQ";
                                        templine[1] = "NOISE_RXFREQ";
                                        templine[2] = "NOISE_AMPL";
                                        LocalTextList.Add(string.Join(",", templine));

                                        // Start looping until complete the freq range
                                        for (istep = 0; istep < tx_freqArray.Length; istep++)
                                        {
                                            //Sorted the calibration result to array
                                            templine[0] = Convert.ToString(tx_freqArray[istep]);
                                            templine[1] = Convert.ToString(nfAmplFreq_Array[istep]);
                                            templine[2] = Convert.ToString(Math.Round(nfAmpl_Array[istep], 3));
                                            LocalTextList.Add(string.Join(",", templine));
                                        }

                                        //Write cal data to csv file
                                        if (!Directory.Exists(SNPFile.FileOutput_Path))
                                        {
                                            Directory.CreateDirectory(SNPFile.FileOutput_Path);
                                        }
                                        //Write cal data to csv file
                                        string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + ºTestParaName + "_Unit" + tmpUnit_No.ToString() + ".csv";
                                        //MessageBox.Show("Path : " + tempPath);
                                        IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
                                    }
                                }
                                #endregion

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (ºOffSG1)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            EqSA01.CLEAR_WRITE();

                            DelayMs(ºStopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "RXPATH_CONTACT":
                            //this function is checking the pathloss/pathgain from antenna port to rx port

                            #region LXI_RXPATH_CONTACT
                            R_NF1_Freq = -99999;
                            R_NF1_Ampl = 99999;

                            NoOfPts = (Convert.ToInt32((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1)) + 1;
                            RXContactdBm = new double[NoOfPts];
                            RXContactFreq = new double[NoOfPts];

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Offset

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1);
                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = ºRXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºRXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = Math.Round(tmpCouplerLoss + (float)ºLossCouplerPath, 3);   //need to use round function because of C# float and double floating point bug/error
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + ºStepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
                            }

                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveCouplerLoss - tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                            totalOutputLoss = tmpAveRxLoss - tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                            //Find actual SG Power Level
                            SGTargetPin = ºPin1 - totalInputLoss;
                            if (SGTargetPin > ºSG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #endregion

                            #region Test RX Path
                            if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                rx1_span = ºStopRXFreq1 - ºStartRXFreq1;
                                rx1_noPoints = Convert.ToInt16(rx1_span / ºStepRXFreq1);
                                rx1_cntrfreq = (ºStartRXFreq1 + ºStopRXFreq1) / 2;
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.FIX);

                                EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                                EqSG01.SetFreq(rx1_cntrfreq);

                                SGTargetPin = ºPin1 - totalInputLoss;
                                EqSG01.SetAmplitude((float)SGTargetPin);
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), ºWaveFormName);
                                EqSG01.SELECT_WAVEFORM(ModulationType);

                                #endregion
                                #region MXA 1 setup

                                DelayMs(ºSetup_Delay);
                                rx1_span = (ºStopRXFreq1 - ºStartRXFreq1);
                                rx1_cntrfreq = ºStartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                EqSA01.AUTO_ATTENUATION(false);

                                if (Convert.ToDouble(ºSA1att) != CurrentSaAttn) //Anthony
                                {
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA1att));
                                    CurrentSaAttn = Convert.ToDouble(ºSA1att);
                                }
                                EqSA01.TRIGGER_SINGLE();
                                EqSA01.TRACE_AVERAGE(1);
                                EqSA01.AVERAGE_OFF();
                                EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");

                                EqSA01.SWEEP_TIMES(Convert.ToInt16(0));

                                EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                EqSA01.SWEEP_POINTS(1);
                                EqSA01.SPAN(0);

                                if (ºSetRX1NDiag)
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(ºSA1att));
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)ºDwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * ºDwellT1));
                                }
                                else
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    if (myUtility.MXA_Setting.Attenuation != CurrentSaAttn) //Anthony
                                    {
                                        EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                        CurrentSaAttn = Convert.ToDouble(myUtility.MXA_Setting.Attenuation);
                                    }

                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_FreeRun);
                                }
                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                            }

                            R_NF1_Freq = rx1_cntrfreq;
                            EqSA01.TRIGGER_SINGLE();    //-14/3 Anthony added
                            EqSA01.TRIGGER_IMM();       //-14/3 Anthony added
                            EqSA01.OPERATION_COMPLETE();//-14/3 Anthony added
                            R_NF1_Ampl = (EqSA01.MEASURE_PEAK_POINT(1) - ºLossOutputPathRX1 - tbInputLoss) - ºPin1;

                            Save_MXA1Trace(1, ºTestParaName, ºSave_MXATrace);

                            #endregion

                            //for test time checking
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }

                            #endregion
                            break;

                        case "NF_STEPSWEEP_NDIAG":
                            // This sweep is a slow sweep , will change SG freq and measure NF for every test points
                            #region NF STEP SWEEP NDIAG

                            int tx_freqPoints;

                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            DelayMs(ºStartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            tx_freqArray = new double[count];
                            rx_freqArray = new double[count];
                            contactPwr_Array = new double[count];
                            nfAmpl_Array = new double[count];
                            ºTXFreq = ºStartTXFreq1;
                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                tx_freqArray[count] = ºTXFreq;
                                rx_freqArray[count] = ºRXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            EqSG01.SetFreq(Convert.ToDouble(ºSG1_DefaultFreq));
                            EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = (ºStopTXFreq1 - ºStartTXFreq1) - ºStepTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / ºStepTXFreq1) + 1;       //need to add additional 1 points to calculated no of points  
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                EqSG01.SET_START_FREQUENCY(ºStartTXFreq1 + (ºStepTXFreq1 / 2));
                                EqSG01.SET_STOP_FREQUENCY(ºStopTXFreq1 - (ºStepTXFreq1 / 2));
                                EqSG01.SET_TRIG_TIMERPERIOD(ºDwellT1);
                                EqSG01.SET_SWEEP_POINT(tx1_noPoints);

                                SGTargetPin = ºPin1 - totalInputLoss;
                                EqSG01.SetAmplitude((float)SGTargetPin);
                                ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), ºWaveFormName);
                                EqSG01.SELECT_WAVEFORM(ModulationType);

                                EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode
                                #endregion

                                #region MXA 1 setup
                                rx1_span = (ºStopRXFreq1 - ºStartRXFreq1);
                                rx1_cntrfreq = ºStartRXFreq1 + (rx1_span / 2);
                                EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                EqSA01.AUTO_ATTENUATION(true);
                                EqSA01.TRIGGER_SINGLE();
                                EqSA01.TRACE_AVERAGE(1);
                                EqSA01.AVERAGE_OFF();

                                EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                EqSA01.SPAN(rx1_span);
                                EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);


                                if (ºSetRX1NDiag)
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(ºSA1att));
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * ºDwellT1));
                                }
                                else
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(ºRX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                EqSA01.CLEAR_WRITE();
                                EqSA01.SET_TRACE_DETECTOR("MAXHOLD");
                                status = EqSA01.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                            if (!ºTunePwr_TX1)
                            {
                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                //if (Math.Abs(ºPout1 - R_Pout1) <= tolerancePwr)
                                //{
                                pwrSearch = true;
                                //}
                            }
                            else
                            {
                                do
                                {
                                    R_Pout1 = EqPwrMeter.MeasPwr(1);
                                    //R_Pin = TargetPin + (float)ºLossInputPathSG1;
                                    R_Pin1 = SGTargetPin + totalInputLoss;

                                    if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                    {
                                        if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                        {
                                            EqSG01.SetAmplitude((float)SGTargetPin);
                                            R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        }

                                        SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                        if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                        {
                                            EqSG01.SetAmplitude((float)SGTargetPin);
                                            DelayMs(ºRdPwr_Delay);
                                        }
                                    }
                                    else if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                    {
                                        SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                        break;
                                    }
                                    else
                                    {
                                        pwrSearch = true;
                                        break;
                                    }

                                    Index++;
                                }
                                while (Index < 10);     // max power search loop
                            }
                            #endregion

                            if (pwrSearch)
                            {
                                EqSG01.SINGLE_SWEEP();
                                status = EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(ºTrig_Delay);
                                R_NF1_Freq = EqSA01.MEASURE_PEAK_FREQ(ºGeneric_Delay);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, (R_NF1_Freq / 1000000), ref ºLossOutputPathRX1, ref StrError);

                                R_NF1_Ampl = EqSA01.MEASURE_PEAK_POINT(ºGeneric_Delay) - ºLossOutputPathRX1 - tbOutputLoss;
                                Save_MXA1Trace(1, ºTestParaName, ºSave_MXATrace);

                                #region Search result MAX or MIN and Save to Datalog
                                //Find result MAX or MIN result
                                switch (ºSearch_Method.ToUpper())
                                {
                                    case "MAX":
                                        //initialize start data 
                                        R_NF1_Ampl = nfAmpl_Array[0];
                                        R_NF1_Freq = tx_freqArray[0];
                                        R_Pout = contactPwr_Array[0];

                                        for (int j = 0; j < tx_freqArray.Length; j++)
                                        {
                                            if (R_NF1_Ampl < nfAmpl_Array[j])
                                            {
                                                R_NF1_Ampl = nfAmpl_Array[j];
                                                R_NF1_Freq = tx_freqArray[j];
                                                R_Pout = contactPwr_Array[j];
                                            }
                                        }
                                        break;
                                    case "MIN":
                                        //initialize start data 
                                        R_NF1_Ampl = nfAmpl_Array[0];
                                        R_NF1_Freq = rx_freqArray[0];
                                        R_Pout = contactPwr_Array[0];

                                        for (int j = 0; j < tx_freqArray.Length; j++)
                                        {
                                            if (R_NF1_Ampl > nfAmpl_Array[j])
                                            {
                                                R_NF1_Ampl = nfAmpl_Array[j];
                                                R_NF1_Freq = rx_freqArray[j];
                                                R_Pout = contactPwr_Array[j];
                                            }
                                        }
                                        break;
                                }

                                //Save all data to datalog 
                                if (ºSave_MXATrace)
                                {
                                    string[] templine = new string[4];
                                    ArrayList LocalTextList = new ArrayList();
                                    ArrayList tmpCalMsg = new ArrayList();

                                    //Calibration File Header
                                    LocalTextList.Add("#MXA1 NF SWEEP DATALOG");
                                    LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                                    LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                                    LocalTextList.Add("#TX Power : " + ºPout1 + " dBm");
                                    templine[0] = "#TX_FREQ";
                                    templine[1] = "RX_FREQ";
                                    templine[2] = "NF_POWER";
                                    templine[3] = "CONTACT";
                                    LocalTextList.Add(string.Join(",", templine));

                                    // Start looping until complete the freq range
                                    for (istep = 0; istep < tx_freqArray.Length; istep++)
                                    {
                                        //Sorted the calibration result to array
                                        templine[0] = Convert.ToString(tx_freqArray[istep]);
                                        templine[1] = Convert.ToString(rx_freqArray[istep]);
                                        templine[2] = Convert.ToString(Math.Round(nfAmpl_Array[istep], 3));
                                        templine[3] = Convert.ToString(Math.Round(contactPwr_Array[istep], 3));
                                        LocalTextList.Add(string.Join(",", templine));
                                    }

                                    //Write cal data to csv file
                                    string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + ºTestParaName + "_Unit" + tmpUnit_No.ToString() + ".csv";
                                    //MessageBox.Show("Path : " + tempPath);
                                    IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
                                }
                                #endregion

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (ºOffSG1)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            EqSA01.CLEAR_WRITE();
                            EqSA01.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(ºStopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "PXI_NF_NONCA_NDIAG":
                            // This is using PXI VST as Sweeper and Analyzer. Will do multiple sweep in one function because using script (Pwr Servo->Soak Sweep->SoakTime->MultiSweep)
                            // Slight different from LXI solution where you define number of sweep in multiple line in TCF

                            #region PXI NF NONCA NDIAG
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure Function
                            NumberOfRuns = 5;
                            SGPowerLevel = -18;// -18 CDMA dBm //-20 LTE dBm  
                            SAReferenceLevel = -20;
                            SoakTime = 450e-3;
                            SoakFrequency = ºStartTXFreq1 * 1e6;
                            vBW_Hz = 300;
                            RBW_Hz = 1e6;
                            preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
                            preSoakSweepTemp = preSoakSweep == true ? 1 : 0; //to indicate if another sweep should be done
                            stepFreqMHz = 0.1;
                            tmpRXFreqHz = ºStartRXFreq1 * 1e6;
                            sweepPts = (Convert.ToInt32((ºStopTXFreq1 - ºStartTXFreq1) / stepFreqMHz)) + 1;
                            //----

                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            if (ºPXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = ºPXI_NoOfSweep;

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Calculate PAPR offset for PXI SG
                            LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE modulationType;
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), ºWaveFormName.ToUpper());
                            int modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            double papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = papr_dB - ºPin1 - totalInputLoss;

                            #region MXA Setup

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            //RBW_Hz = myUtility.MXA_Setting.RBW;

                            #endregion

                            //if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                //generate modulated signal
                                string Script =
                                         "script powerServo\r\n"
                                       + "repeat forever\r\n"
                                       + "generate Signal" + ºWaveFormName + "\r\n"
                                       + "end repeat\r\n"
                                       + "end script";
                                try
                                {
                                    EqVST.rfsgSession.Arb.Scripting.WriteScript(Script);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                EqVST.rfsgSession.RF.Frequency = ºStartTXFreq1 * 1e6;
                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                                //Need to ensure that SG_IQRate re-define , because RX_CONTACT routine has overwritten the initialization data
                                EqVST.Get_s_SignalType(ºModulation, ºWaveFormName, out SG_IQRate);
                                EqVST.rfsgSession.Arb.IQRate = SG_IQRate;

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                                #endregion
                            }

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                if (!ºTunePwr_TX1)
                                {
                                    EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                    EqVST.rfsgSession.Initiate();
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    DelayMs(ºRdPwr_Delay);
                                    R_Pout1 = EqPwrMeter.MeasPwr(1);
                                    R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3);
                                    if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                    {
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin - papr_dB + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                                EqVST.rfsgSession.Initiate();
                                                DelayMs(ºRdPwr_Delay);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                DelayMs(ºRdPwr_Delay);
                                            }

                                            if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                            {
                                                SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            SGPowerLevel = SGTargetPin;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            //total test time for each parameter will include the soak time
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                SoakTime = syncTest_Delay * 1e-3;       //convert to second
                            }
                            else
                            {
                                SoakTime = 0;                //no soak required if power servo longer than expected total test time                                                        
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                #region Measure VST
                                R_NF1_Freq = -888;
                                R_NF1_Ampl = 888;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                EqVST.rfsgSession.Abort();         //stop power servo script

                                Stopwatch timer1 = new Stopwatch();
                                Stopwatch timer2 = new Stopwatch();
                                timer1.Restart();

                                #region Config VST and Measure Noise

                                #region decode and re-arrange multiple bandwidth (Ascending)
                                int bw_cnt = 0;
                                double[] tmpRBW_Hz = Array.ConvertAll(ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                                double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                                Array.Sort(tmpRBW_Hz);
                                foreach (double key in tmpRBW_Hz)
                                {
                                    multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                    bw_cnt++;
                                }

                                multiRBW_cnt = multiRBW_Hz.Length;
                                RBW_Hz = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                                #endregion

                                //if (SoakTime <= 0)
                                if (ºEstimate_TestTime <= 0)    //assume soaktime when 0 or less (set from TCF) does not required soak sweep
                                {
                                    preSoakSweep = false;
                                    preSoakSweepTemp = 0; //to indicate if another sweep should be done
                                }

                                EqVST.ConfigureVSTDuringTest(new LibEqmtDriver.NF_VST.NF_NiPXI_VST.Config(NumberOfRuns + preSoakSweepTemp, ºTX1Band, ºModulation, ºWaveFormName,
                                ºStartTXFreq1 * 1e6, ºStopTXFreq1 * 1e6, ºStepTXFreq1 * 1e6, (ºDwellT1 - 0.03) / 1000, ºStartRXFreq1 * 1e6, ºStopRXFreq1 * 1e6, ºStepRXFreq1 * 1e6,
                                SGPowerLevel, SAReferenceLevel, SoakTime, SoakFrequency, RBW_Hz, vBW_Hz, preSoakSweep, ºPXI_Multiplier_RXIQRate, multiRBW_Hz));

                                LibEqmtDriver.NF_VST.S_MultiRBW_Data[] MultiRBW_RsltMultiTrace = new LibEqmtDriver.NF_VST.S_MultiRBW_Data[multiRBW_cnt];

                                timer1.Stop();
                                timer2.Restart();

                                MultiRBW_RsltMultiTrace = EqVST.Measure_VST(sweepPts);

                                #endregion
                                timer2.Stop();

                                long time1 = timer1.ElapsedMilliseconds;
                                long time2 = timer2.ElapsedMilliseconds;

                                #region Sort and Store Trace Data
                                //Store multi trace from PXI to global array
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    for (int n = 0; n < NumberOfRuns + preSoakSweepTemp; n++)
                                    {
                                        //temp trace array storage use for MAX , MIN etc calculation 
                                        PXITrace[TestCount].Enable = true;
                                        PXITrace[TestCount].SoakSweep = preSoakSweep;
                                        PXITrace[TestCount].TestNumber = ºTestNum;
                                        PXITrace[TestCount].TraceCount = NumberOfRuns + preSoakSweepTemp;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].NoPoints = sweepPts;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].RBW_Hz = MultiRBW_RsltMultiTrace[rbw_counter].RBW_Hz;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Result_Header = ºTestParaName;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].MXA_No = "PXI_Trace";

                                        PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                        PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];

                                        for (istep = 0; istep < sweepPts; istep++)
                                        {
                                            if (istep == 0)
                                                tmpRXFreqHz = ºStartRXFreq1 * 1e6;
                                            else
                                                tmpRXFreqHz = tmpRXFreqHz + (stepFreqMHz * 1e6);

                                            if (usePrevRslt)    //PXI trace result minus out the DUT LNA Gain from previous result
                                            {
                                                PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3) - prevRslt;
                                            }
                                            else
                                            {
                                                PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3);
                                            }

                                            //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3);
                                        }
                                    }

                                    Save_PXI_TraceRaw(ºTestParaName, ºTestUsePrev, ºSave_MXATrace, rbw_counter, multiRBW_Hz[rbw_counter]);
                                }

                                #endregion

                                #region Test Parameter Log

                                //Get average pathloss base on start and stop freq with 1MHz step freq
                                count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / 1);
                                ºRXFreq = ºStartRXFreq1;
                                for (int i = 0; i <= count; i++)
                                {
                                    ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                    tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                    ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                                }
                                tmpAveRxLoss = tmpRxLoss / (count + 1);
                                ºLossOutputPathRX1 = tmpAveRxLoss;

                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    string[] tmpParamName;
                                    string tmp1stHeader = null;
                                    string tmp2ndHeader = null;
                                    tmpParamName = ºTestParaName.Split('_');

                                    for (int i = 0; i < tmpParamName.Length; i++)
                                    {
                                        if (i > 0)
                                            tmp2ndHeader = tmp2ndHeader + "_" + tmpParamName[i];
                                    }

                                    //Sort out test result for all traces and Add test result
                                    for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                    {
                                        R_NF1_Freq = -888;
                                        R_NF1_Ampl = 888;
                                        double tmpNFAmpl = 999;
                                        int tmpIndex = 0;
                                        ºTestParaName = "NF" + (i + 1) + tmp2ndHeader;

                                        switch (ºSearch_Method.ToUpper())
                                        {
                                            case "MAX":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Max();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            case "MIN":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Min();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            default:
                                                MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                break;
                                        }

                                        R_NF1_Ampl = R_NF1_Ampl - ºLossOutputPathRX1 - tbOutputLoss;

                                        if (i == 0)
                                        {
                                            if (ºTest_Pin1)
                                            {
                                                BuildResults(ref results, ºTestParaName + rbwParamName + "_Pin1", "dBm", R_Pin1);
                                            }
                                            if (ºTest_Pout1)
                                            {
                                                BuildResults(ref results, ºTestParaName + rbwParamName + "_Pout1", "dBm", R_Pout1);
                                            }
                                            if (ºTest_NF1)
                                            {
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                            if (ºTest_SMU)
                                            {
                                                MeasSMU = ºSMUMeasCh.Split(',');
                                                for (int j = 0; j < MeasSMU.Count(); j++)
                                                {
                                                    BuildResults(ref results, ºTestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ºTest_NF1)
                                            {
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                        }
                                    }

                                    //Force test flag to false to ensure no repeated test data
                                    //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                    ºTest_Pin1 = false;
                                    ºTest_Pout1 = false;
                                    ºTest_SMU = false;
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                ºTest_NF1 = false;
                                #endregion

                                #endregion
                            }
                            else                                            //if fail power out search , set data to default
                            {
                                #region If Power Servo Fail Routine
                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //if (SoakTime <= 0)
                                if (ºEstimate_TestTime <= 0)    //assume soaktime when 0 or less (set from TCF) does not required soak sweep
                                {
                                    preSoakSweep = false;
                                    preSoakSweepTemp = 0; //to indicate if another sweep should be done
                                }

                                #region measure SMU current - during fail power servo
                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                EqVST.rfsgSession.Abort();         //stop power servo script
                                #endregion

                                #region decode re-arrange multiple bandwidth (Ascending)
                                int bw_cnt = 0;
                                double[] tmpRBW_Hz = Array.ConvertAll(ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                                double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                                Array.Sort(tmpRBW_Hz);
                                foreach (double key in tmpRBW_Hz)
                                {
                                    multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                    bw_cnt++;
                                }

                                multiRBW_cnt = multiRBW_Hz.Length;
                                RBW_Hz = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                                #endregion

                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    //Store multi trace from PXI to global array
                                    for (int n = 0; n < NumberOfRuns + preSoakSweepTemp; n++)
                                    {
                                        //temp trace array storage use for MAX , MIN etc calculation 
                                        PXITrace[TestCount].Enable = true;
                                        PXITrace[TestCount].SoakSweep = preSoakSweep;
                                        PXITrace[TestCount].TestNumber = ºTestNum;
                                        PXITrace[TestCount].TraceCount = NumberOfRuns + preSoakSweepTemp;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].NoPoints = sweepPts;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].RBW_Hz = multiRBW_Hz[rbw_counter];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Result_Header = ºTestParaName;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].MXA_No = "PXI_Trace";

                                        for (istep = 0; istep < sweepPts; istep++)
                                        {
                                            if (istep == 0)
                                                tmpRXFreqHz = ºStartRXFreq1 * 1e6;
                                            else
                                                tmpRXFreqHz = tmpRXFreqHz + (stepFreqMHz * 1e6);

                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = 999;
                                        }
                                    }

                                    #region Test Parameter Log
                                    string[] tmpParamName;
                                    string tmp1stHeader = null;
                                    string tmp2ndHeader = null;
                                    tmpParamName = ºTestParaName.Split('_');
                                    for (int i = 0; i < tmpParamName.Length; i++)
                                    {
                                        if (i > 0)
                                            tmp2ndHeader = tmp2ndHeader + "_" + tmpParamName[i];
                                    }

                                    //Sort out test result for all traces and Add test result
                                    for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                    {
                                        R_NF1_Freq = -888;
                                        R_NF1_Ampl = 888;
                                        double tmpNFAmpl = 999;
                                        int tmpIndex = 0;
                                        ºTestParaName = "NF" + (i + 1) + tmp2ndHeader;

                                        switch (ºSearch_Method.ToUpper())
                                        {
                                            case "MAX":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Max();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            case "MIN":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Min();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            default:
                                                MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                break;
                                        }

                                        if (i == 0)
                                        {
                                            if (ºTest_Pin1)
                                            {
                                                BuildResults(ref results, ºTestParaName + rbwParamName + "_Pin1", "dBm", R_Pin1);
                                            }
                                            if (ºTest_Pout1)
                                            {
                                                BuildResults(ref results, ºTestParaName + rbwParamName + "_Pout1", "dBm", R_Pout1);
                                            }
                                            if (ºTest_NF1)
                                            {
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                            if (ºTest_SMU)
                                            {
                                                MeasSMU = ºSMUMeasCh.Split(',');
                                                for (int j = 0; j < MeasSMU.Count(); j++)
                                                {
                                                    BuildResults(ref results, ºTestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ºTest_NF1)
                                            {
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                        }
                                    }

                                    //Force test flag to false to ensure no repeated test data
                                    //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                    ºTest_Pin1 = false;
                                    ºTest_Pout1 = false;
                                    ºTest_SMU = false;
                                    #endregion
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                ºTest_NF1 = false;
                                #endregion
                            }


                            //for test time checking
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        case "PXI_NF_FIX_NMAX":
                            // This function is similar to PXI_NF_NONCA_NDIAG, but the TX frequency range is fixed while RX band is swept

                            #region PXI NF FIX NMAX
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure Function
                            NumberOfRuns = 5;
                            SGPowerLevel = -18;// -18 CDMA dBm //-20 LTE dBm  
                            SAReferenceLevel = -20;
                            SoakTime = 450e-3;
                            SoakFrequency = ºStartTXFreq1 * 1e6;
                            vBW_Hz = 300;
                            RBW_Hz = 1e6;
                            preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
                            preSoakSweepTemp = preSoakSweep == true ? 1 : 0; //to indicate if another sweep should be done
                            stepFreqMHz = 0.1;
                            tmpRXFreqHz = ºStartRXFreq1 * 1e6;
                            sweepPts = (Convert.ToInt32((ºStopRXFreq1 - ºStartRXFreq1) / stepFreqMHz)) + 1; //Determine sweep points according to RX frequency range
                            //----

                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            if (ºPXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = ºPXI_NoOfSweep;

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Calculate PAPR offset for PXI SG
                            //LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE modulationType;
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), ºWaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = papr_dB - ºPin1 - totalInputLoss;

                            #region MXA Setup

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            //RBW_Hz = myUtility.MXA_Setting.RBW;

                            #endregion

                            //if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                //generate modulated signal
                                string Script =
                                         "script powerServo\r\n"
                                       + "repeat forever\r\n"
                                       + "generate Signal" + ºWaveFormName + "\r\n"
                                       + "end repeat\r\n"
                                       + "end script";
                                try
                                {
                                    EqVST.rfsgSession.Arb.Scripting.WriteScript(Script);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                EqVST.rfsgSession.RF.Frequency = ºStartTXFreq1 * 1e6;
                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                                //Need to ensure that SG_IQRate re-define , because RX_CONTACT routine has overwritten the initialization data
                                EqVST.Get_s_SignalType(ºModulation, ºWaveFormName, out SG_IQRate);
                                EqVST.rfsgSession.Arb.IQRate = SG_IQRate;

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                                #endregion
                            }

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                if (!ºTunePwr_TX1)
                                {
                                    EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                    EqVST.rfsgSession.Initiate();
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    DelayMs(ºRdPwr_Delay);
                                    R_Pout1 = EqPwrMeter.MeasPwr(1);
                                    R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3);
                                    if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                    {
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin - papr_dB + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                                EqVST.rfsgSession.Initiate();
                                                DelayMs(ºRdPwr_Delay);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                DelayMs(ºRdPwr_Delay);
                                            }

                                            if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                            {
                                                SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            SGPowerLevel = SGTargetPin;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            //total test time for each parameter will include the soak time
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                SoakTime = syncTest_Delay * 1e-3;       //convert to second
                            }
                            else
                            {
                                SoakTime = 0;                //no soak required if power servo longer than expected total test time                                                        
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                #region Measure VST
                                R_NF1_Freq = -888;
                                R_NF1_Ampl = 888;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                EqVST.rfsgSession.Abort();         //stop power servo script

                                Stopwatch timer1 = new Stopwatch();
                                Stopwatch timer2 = new Stopwatch();
                                timer1.Restart();

                                int NumberofFixedTXRecords = (int)((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1) + 1; //Get number of records according to TX freq range and step
                                for (int m = 0; m < NumberofFixedTXRecords; m++)
                                {
                                    #region Config VST and Measure Noise

                                    #region decode and re-arrange multiple bandwidth (Ascending)
                                    int bw_cnt = 0;
                                    double[] tmpRBW_Hz = Array.ConvertAll(ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                                    double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                                    Array.Sort(tmpRBW_Hz);
                                    foreach (double key in tmpRBW_Hz)
                                    {
                                        multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                        bw_cnt++;
                                    }

                                    multiRBW_cnt = multiRBW_Hz.Length;
                                    RBW_Hz = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                                    #endregion

                                    //if (SoakTime <= 0)
                                    if (ºEstimate_TestTime <= 0)    //assume soaktime when 0 or less (set from TCF) does not required soak sweep
                                    {
                                        preSoakSweep = false;
                                        preSoakSweepTemp = 0; //to indicate if another sweep should be done
                                    }

                                    double FixedTXFreq = ºStartTXFreq1 + m * ºStepTXFreq1;

                                    EqVST.ConfigureVSTDuringTest_FixedTX(new LibEqmtDriver.NF_VST.NF_NiPXI_VST.Config(NumberOfRuns + preSoakSweepTemp, ºTX1Band, ºModulation, ºWaveFormName,
                                    FixedTXFreq * 1e6, FixedTXFreq * 1e6, ºStepTXFreq1 * 1e6, (ºDwellT1 - 0.03) / 1000, ºStartRXFreq1 * 1e6, ºStopRXFreq1 * 1e6, ºStepRXFreq1 * 1e6,
                                    SGPowerLevel, SAReferenceLevel, SoakTime, SoakFrequency, RBW_Hz, vBW_Hz, preSoakSweep, ºPXI_Multiplier_RXIQRate, multiRBW_Hz));

                                    LibEqmtDriver.NF_VST.S_MultiRBW_Data[] MultiRBW_RsltMultiTrace = new LibEqmtDriver.NF_VST.S_MultiRBW_Data[multiRBW_cnt];

                                    timer1.Stop();
                                    timer2.Restart();

                                    MultiRBW_RsltMultiTrace = EqVST.Measure_VST(sweepPts);

                                    #endregion
                                    timer2.Stop();

                                    long time1 = timer1.ElapsedMilliseconds;
                                    long time2 = timer2.ElapsedMilliseconds;

                                    #region Sort and Store Trace Data
                                    //Store multi trace from PXI to global array
                                    for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                    {
                                        for (int n = 0; n < NumberOfRuns + preSoakSweepTemp; n++)
                                        {
                                            //temp trace array storage use for MAX , MIN etc calculation 
                                            PXITrace[TestCount].Enable = true;
                                            PXITrace[TestCount].SoakSweep = preSoakSweep;
                                            PXITrace[TestCount].TestNumber = ºTestNum;
                                            PXITrace[TestCount].TraceCount = NumberOfRuns + preSoakSweepTemp;
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].NoPoints = sweepPts;
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].RBW_Hz = MultiRBW_RsltMultiTrace[rbw_counter].RBW_Hz;
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].Result_Header = ºTestParaName;
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].MXA_No = "PXI_Trace";

                                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];

                                            for (istep = 0; istep < sweepPts; istep++)
                                            {
                                                if (istep == 0)
                                                    tmpRXFreqHz = ºStartRXFreq1 * 1e6;
                                                else
                                                    tmpRXFreqHz = tmpRXFreqHz + (stepFreqMHz * 1e6);

                                                if (usePrevRslt)    //PXI trace result minus out the DUT LNA Gain from previous result
                                                {
                                                    PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                    PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3) - prevRslt;
                                                }
                                                else
                                                {
                                                    PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                    PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3);
                                                }

                                                //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3);
                                            }
                                        }

                                        Save_PXI_TraceRaw(ºTestParaName + "_FixedTX_" + FixedTXFreq.ToString() + "M", ºTestUsePrev, ºSave_MXATrace, rbw_counter, multiRBW_Hz[rbw_counter]);
                                    }

                                    #endregion

                                    #region Test Parameter Log

                                    //Get average pathloss base on start and stop freq with 1MHz step freq
                                    count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / 1);
                                    ºRXFreq = ºStartRXFreq1;
                                    for (int i = 0; i <= count; i++)
                                    {
                                        ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                        tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                        ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                                    }
                                    tmpAveRxLoss = tmpRxLoss / (count + 1);
                                    ºLossOutputPathRX1 = tmpAveRxLoss;

                                    for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                    {
                                        rbwParamName = null;
                                        rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                        string[] tmpParamName;
                                        string tmp1stHeader = null;
                                        string tmp2ndHeader = null;
                                        tmpParamName = ºTestParaName.Split('_');

                                        for (int i = 0; i < tmpParamName.Length; i++)
                                        {
                                            if (i > 0)
                                                tmp2ndHeader = tmp2ndHeader + "_" + tmpParamName[i];
                                        }

                                        //Sort out test result for all traces and Add test result
                                        for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                        {
                                            R_NF1_Freq = -888;
                                            R_NF1_Ampl = 888;
                                            double tmpNFAmpl = 999;
                                            int tmpIndex = 0;
                                            ºTestParaName = "NF" + (i + 1) + tmp2ndHeader;

                                            switch (ºSearch_Method.ToUpper())
                                            {
                                                case "MAX":
                                                    tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Max();
                                                    tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                    R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                    R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                    break;

                                                case "MIN":
                                                    tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Min();
                                                    tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                    R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                    R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                    break;

                                                default:
                                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                    break;
                                            }

                                            R_NF1_Ampl = R_NF1_Ampl - ºLossOutputPathRX1 - tbOutputLoss;

                                            if (i == 0)
                                            {
                                                if (ºTest_Pin1)
                                                {
                                                    BuildResults(ref results, ºTestParaName + rbwParamName + "_Pin1", "dBm", R_Pin1);
                                                }
                                                if (ºTest_Pout1)
                                                {
                                                    BuildResults(ref results, ºTestParaName + rbwParamName + "_Pout1", "dBm", R_Pout1);
                                                }
                                                if (ºTest_NF1)
                                                {
                                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                                }
                                                if (ºTest_SMU)
                                                {
                                                    MeasSMU = ºSMUMeasCh.Split(',');
                                                    for (int j = 0; j < MeasSMU.Count(); j++)
                                                    {
                                                        BuildResults(ref results, ºTestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (ºTest_NF1)
                                                {
                                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                    BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                                }
                                            }
                                        }

                                        //Force test flag to false to ensure no repeated test data
                                        //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                        ºTest_Pin1 = false;
                                        ºTest_Pout1 = false;
                                        ºTest_SMU = false;
                                    }

                                    //Force test flag to false to ensure no repeated test data
                                    //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                    ºTest_NF1 = false;
                                    #endregion
                                }
                                #endregion
                            }
                            else                                            //if fail power out search , set data to default
                            {
                                #region If Power Servo Fail Routine
                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //if (SoakTime <= 0)
                                if (ºEstimate_TestTime <= 0)    //assume soaktime when 0 or less (set from TCF) does not required soak sweep
                                {
                                    preSoakSweep = false;
                                    preSoakSweepTemp = 0; //to indicate if another sweep should be done
                                }

                                #region measure SMU current - during fail power servo
                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                EqVST.rfsgSession.Abort();         //stop power servo script
                                #endregion

                                #region decode re-arrange multiple bandwidth (Ascending)
                                int bw_cnt = 0;
                                double[] tmpRBW_Hz = Array.ConvertAll(ºPXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                                double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                                Array.Sort(tmpRBW_Hz);
                                foreach (double key in tmpRBW_Hz)
                                {
                                    multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                    bw_cnt++;
                                }

                                multiRBW_cnt = multiRBW_Hz.Length;
                                RBW_Hz = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                                #endregion

                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    //Store multi trace from PXI to global array
                                    for (int n = 0; n < NumberOfRuns + preSoakSweepTemp; n++)
                                    {
                                        //temp trace array storage use for MAX , MIN etc calculation 
                                        PXITrace[TestCount].Enable = true;
                                        PXITrace[TestCount].SoakSweep = preSoakSweep;
                                        PXITrace[TestCount].TestNumber = ºTestNum;
                                        PXITrace[TestCount].TraceCount = NumberOfRuns + preSoakSweepTemp;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].NoPoints = sweepPts;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].RBW_Hz = multiRBW_Hz[rbw_counter];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Result_Header = ºTestParaName;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].MXA_No = "PXI_Trace";

                                        for (istep = 0; istep < sweepPts; istep++)
                                        {
                                            if (istep == 0)
                                                tmpRXFreqHz = ºStartRXFreq1 * 1e6;
                                            else
                                                tmpRXFreqHz = tmpRXFreqHz + (stepFreqMHz * 1e6);

                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = 999;
                                        }
                                    }

                                    #region Test Parameter Log
                                    string[] tmpParamName;
                                    string tmp1stHeader = null;
                                    string tmp2ndHeader = null;
                                    tmpParamName = ºTestParaName.Split('_');
                                    for (int i = 0; i < tmpParamName.Length; i++)
                                    {
                                        if (i > 0)
                                            tmp2ndHeader = tmp2ndHeader + "_" + tmpParamName[i];
                                    }

                                    //Sort out test result for all traces and Add test result
                                    for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                    {
                                        R_NF1_Freq = -888;
                                        R_NF1_Ampl = 888;
                                        double tmpNFAmpl = 999;
                                        int tmpIndex = 0;
                                        ºTestParaName = "NF" + (i + 1) + tmp2ndHeader;

                                        switch (ºSearch_Method.ToUpper())
                                        {
                                            case "MAX":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Max();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            case "MIN":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Min();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            default:
                                                MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                break;
                                        }

                                        if (i == 0)
                                        {
                                            if (ºTest_Pin1)
                                            {
                                                BuildResults(ref results, ºTestParaName + rbwParamName + "_Pin1", "dBm", R_Pin1);
                                            }
                                            if (ºTest_Pout1)
                                            {
                                                BuildResults(ref results, ºTestParaName + rbwParamName + "_Pout1", "dBm", R_Pout1);
                                            }
                                            if (ºTest_NF1)
                                            {
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                            if (ºTest_SMU)
                                            {
                                                MeasSMU = ºSMUMeasCh.Split(',');
                                                for (int j = 0; j < MeasSMU.Count(); j++)
                                                {
                                                    BuildResults(ref results, ºTestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ºTest_NF1)
                                            {
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                        }
                                    }

                                    //Force test flag to false to ensure no repeated test data
                                    //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                    ºTest_Pin1 = false;
                                    ºTest_Pout1 = false;
                                    ºTest_SMU = false;
                                    #endregion
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                ºTest_NF1 = false;
                                #endregion
                            }


                            //for test time checking
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        case "PXI_RXPATH_CONTACT":
                            //this function is checking the pathloss/pathgain from antenna port to rx port

                            #region PXI_RXPATH_CONTACT
                            R_NF1_Freq = -99999;
                            R_NF1_Ampl = 99999;

                            NoOfPts = (Convert.ToInt32((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1)) + 1;
                            RXContactdBm = new double[NoOfPts];
                            RXContactFreq = new double[NoOfPts];

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Offset

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), ºWaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1);
                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = ºRXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºRXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = Math.Round(tmpCouplerLoss + (float)ºLossCouplerPath, 3);   //need to use round function because of C# float and double floating point bug/error
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + ºStepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
                            }

                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveCouplerLoss - tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                            totalOutputLoss = tmpAveRxLoss - tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                            //Find actual SG Power Level
                            SGTargetPin = ºPin1 - (totalInputLoss - papr_dB);
                            if (SGTargetPin > ºSG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #endregion

                            #region Test RX Path
                            EqVST.RXContactCheck(SGTargetPin, ºStartRXFreq1, ºStopRXFreq1, ºStepRXFreq1, SAReferenceLevel, out RXContactdBm);

                            //Sort out test result
                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MAX":
                                    R_NF1_Ampl = RXContactdBm.Max();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactdBm, R_NF1_Ampl)];
                                    break;

                                case "MIN":
                                    R_NF1_Ampl = RXContactdBm.Min();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactdBm, R_NF1_Ampl)];
                                    break;

                                case "AVE":
                                case "AVERAGE":
                                    R_NF1_Ampl = RXContactdBm.Average();
                                    R_NF1_Freq = RXContactFreq[0];          //return default freq i.e Start Freq
                                    break;

                                case "USER":
                                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                                    if ((Convert.ToSingle(ºSearch_Value) >= ºStartRXFreq1) && (Convert.ToSingle(ºSearch_Value) <= ºStopRXFreq1))
                                    {
                                        try
                                        {
                                            R_NF1_Ampl = RXContactdBm[Array.IndexOf(RXContactFreq, Convert.ToSingle(ºSearch_Value))];     //return contact power from same array number(of index number associated with 'USER' Freq)
                                            R_NF1_Freq = Convert.ToSingle(ºSearch_Value);
                                        }
                                        catch       //if ºSearch_Value not in RXContactFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                                        {
                                            R_NF1_Freq = Convert.ToSingle(ºSearch_Value);
                                            R_NF1_Ampl = 99999;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Test Parameter : " + ºTestParam + "(SEARCH METHOD : " + ºSearch_Method + ", USER DEFINE : " + ºSearch_Value + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    }
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            R_NF1_Ampl = (R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss) - ºPin1;      //return DUT only pathgain/loss result while excluding pathloss cal

                            #endregion

                            #region Sort and Store Trace Data
                            //Store RX Contact from PXI to global array
                            //temp trace array storage use for MAX , MIN etc calculation 
                            rbw_counter = 0;

                            PXITrace[TestCount].Enable = true;
                            PXITrace[TestCount].SoakSweep = false;
                            PXITrace[TestCount].TestNumber = ºTestNum;
                            PXITrace[TestCount].TraceCount = 1;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].NoPoints = NoOfPts;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].RBW_Hz = 1e6;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Result_Header = ºTestParaName;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].MXA_No = "PXI_RXCONTACT_Trace";

                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];

                            for (istep = 0; istep < NoOfPts; istep++)
                            {
                                PXITrace[TestCount].Multi_Trace[0][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITrace[TestCount].Multi_Trace[0][0].Ampl[istep] = Math.Round(RXContactdBm[istep], 3);

                                //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep] = Math.Round(RXContactdBm[istep], 3);
                            }

                            Save_PXI_TraceRaw(ºTestParaName, ºTestUsePrev, ºSave_MXATrace, rbw_counter, 1e6);

                            #endregion

                            //for test time checking
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }

                            #endregion
                            break;

                        case "PXI_FIXED_POWERBLAST":
                            //this function is to provide fixed power/freq to stress the unit

                            #region PXI NF FIX POWERBLAST
                            status = false;
                            pwrSearch = false;

                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), ºWaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = ºPin1 - totalInputLoss + papr_dB;

                            //Not use - got some bug when only testing single band (shaz - 14/03/2017)
                            //if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                //generate modulated signal
                                string Script =
                                         "script powerServo\r\n"
                                       + "repeat forever\r\n"
                                       + "generate Signal" + ºWaveFormName + "\r\n"
                                       + "end repeat\r\n"
                                       + "end script";
                                EqVST.rfsgSession.Arb.Scripting.WriteScript(Script);
                                EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                EqVST.rfsgSession.RF.Frequency = ºStartTXFreq1 * 1e6;
                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                                //Need to ensure that SG_IQRate re-define , because RX_CONTACT routine has overwritten the initialization data
                                EqVST.Get_s_SignalType(ºModulation, ºWaveFormName, out SG_IQRate);
                                EqVST.rfsgSession.Arb.IQRate = SG_IQRate;

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                                #endregion
                            }

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                if (!ºTunePwr_TX1)
                                {
                                    EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                    EqVST.rfsgSession.Initiate();
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    DelayMs(ºRdPwr_Delay);
                                    R_Pout1 = EqPwrMeter.MeasPwr(1);
                                    R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3);

                                    if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                    {
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin - papr_dB + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                                EqVST.rfsgSession.Initiate();
                                                DelayMs(ºRdPwr_Delay);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                DelayMs(ºRdPwr_Delay);
                                            }

                                            if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                            {
                                                SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            SGPowerLevel = SGTargetPin;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                //to control DUT soaking time
                                paramTestTime = tTime.ElapsedMilliseconds;
                                if (paramTestTime < (long)ºEstimate_TestTime)
                                {
                                    syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                    DelayMs((int)syncTest_Delay);
                                }

                                EqVST.rfsgSession.Abort();         //stop power servo script
                            }
                            else                                            //if fail power out search , set data to default
                            {
                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                EqVST.rfsgSession.Abort();         //stop power servo script
                            }

                            //for test time checking
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        case "PXI_RAMP_POWERBLAST":
                            //this function is to provide fixed power/freq to stress the unit

                            #region PXI NF POWERBLAST RAMP
                            status = false;
                            pwrSearch = false;
                            R_Pin1 = -999;        //set test flag to default -999
                            ºTest_Pin1 = true;
                            ºTest_Pout1 = false;
                            ºTest_NF1 = false;
                            ºTest_SMU = false;

                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            searchPowerBlastKey(ºTestParam, ºSwBand, out CtrFreqMHz_pwrBlast, out StartPwrLvldBm_pwrBlast, out StopPwrLvldBm_pwrBlast, out StepPwrLvl_pwrBlast, out DwellTmS_pwrBlast, out Transient_mS_pwrBlast, out Transient_Step_pwrBlast, out b_PwrBlastTKey);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration
                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get pathloss base on PowerBlast Center freq
                            ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, CtrFreqMHz_pwrBlast, ref ºLossInputPathSG1, ref StrError);
                            ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, CtrFreqMHz_pwrBlast, ref ºLossCouplerPath, ref StrError);

                            totalInputLoss = Math.Round((float)ºLossInputPathSG1 - tbInputLoss, 3);
                            totalOutputLoss = Math.Abs((float)ºLossCouplerPath - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, Math.Round(totalOutputLoss, 3));
                            SGTargetPin = ºPin1 - totalInputLoss;
                            double startPwrLvl = Math.Round(StartPwrLvldBm_pwrBlast - totalInputLoss, 3);
                            double stopPwrLvl = Math.Round(StopPwrLvldBm_pwrBlast - totalInputLoss, 3);

                            #endregion

                            //Power Ramp routine
                            EqVST.PowerRamp(ºModulation, ºWaveFormName, CtrFreqMHz_pwrBlast * 1e6, Transient_mS_pwrBlast / 1e3, Transient_Step_pwrBlast,
                                                DwellTmS_pwrBlast / 1e3, StepPwrLvl_pwrBlast, startPwrLvl, stopPwrLvl);

                            R_Pin1 = 1;        //set test flag to default 1 indicate complete test only

                            //for test time checking
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        case "PXI_RXPATH_GAIN": //Seoul
                            //this function is checking the pathloss/pathgain from antenna port to rx port

                            #region PXI_RXPATH_GAIN
                            R_NF1_Freq = -99999;
                            R_NF1_Ampl = 99999;

                            NoOfPts = (Convert.ToInt32((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1)) + 1;
                            RXContactdBm = new double[NoOfPts];
                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];//Seoul
                            RXPathLoss = new double[NoOfPts]; //Seoul
                            LNAInputLoss = new double[NoOfPts]; //Seoul

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Offset

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), ºWaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1);
                            ºRXFreq = ºStartRXFreq1;

                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = ºRXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)ºLossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºRXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = Math.Round(tmpCouplerLoss + (float)ºLossCouplerPath, 3);   //need to use round function because of C# float and double floating point bug/error
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + ºStepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
                                RXPathLoss[i] = ºLossOutputPathRX1;//Seoul
                                LNAInputLoss[i] = ºLossCouplerPath;//Seoul
                            }

                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveCouplerLoss - tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                            totalOutputLoss = tmpAveRxLoss - tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                            //Find actual SG Power Level
                            SGTargetPin = ºPin1 - (totalInputLoss - papr_dB);
                            if (SGTargetPin > ºSG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #endregion

                            #region Test RX Path
                            EqVST.RXContactCheck(SGTargetPin, ºStartRXFreq1, ºStopRXFreq1, ºStepRXFreq1, SAReferenceLevel, out RXContactdBm);

                            for (int i = 0; i < RXContactdBm.Length; i++)
                            {
                                RXContactGain[i] = (RXContactdBm[i] - RXPathLoss[i] - tbOutputLoss) - ºPin1; //Seoul for RX Gain trace
                            }

                            //Sort out test result
                            switch (ºSearch_Method.ToUpper())
                            {
                                case "MAX":
                                    R_NF1_Ampl = RXContactGain.Max();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactGain, R_NF1_Ampl)];
                                    break;

                                case "MIN":
                                    R_NF1_Ampl = RXContactGain.Min();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactGain, R_NF1_Ampl)];
                                    break;

                                case "AVE":
                                case "AVERAGE":
                                    R_NF1_Ampl = RXContactGain.Average();
                                    R_NF1_Freq = RXContactFreq[0];          //return default freq i.e Start Freq
                                    break;

                                case "USER":
                                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                                    if ((Convert.ToSingle(ºSearch_Value) >= ºStartRXFreq1) && (Convert.ToSingle(ºSearch_Value) <= ºStopRXFreq1))
                                    {
                                        try
                                        {
                                            R_NF1_Ampl = RXContactGain[Array.IndexOf(RXContactFreq, Convert.ToSingle(ºSearch_Value))];     //return contact power from same array number(of index number associated with 'USER' Freq)
                                            R_NF1_Freq = Convert.ToSingle(ºSearch_Value);
                                        }
                                        catch       //if ºSearch_Value not in RXContactFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                                        {
                                            R_NF1_Freq = Convert.ToSingle(ºSearch_Value);
                                            R_NF1_Ampl = 99999;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Test Parameter : " + ºTestParam + "(SEARCH METHOD : " + ºSearch_Method + ", USER DEFINE : " + ºSearch_Value + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    }
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            #endregion

                            #region Sort and Store Trace Data
                            //Store RX Contact from PXI to global array
                            //temp trace array storage use for MAX , MIN etc calculation 
                            rbw_counter = 0;

                            PXITrace[TestCount].Enable = true;
                            PXITrace[TestCount].SoakSweep = false;
                            PXITrace[TestCount].TestNumber = ºTestNum;
                            PXITrace[TestCount].TraceCount = 1;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].NoPoints = NoOfPts;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].RBW_Hz = 1e6;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Result_Header = ºTestParaName;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].MXA_No = "PXI_RXCONTACT_Trace";

                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];

                            for (istep = 0; istep < NoOfPts; istep++)
                            {
                                PXITrace[TestCount].Multi_Trace[0][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITrace[TestCount].Multi_Trace[0][0].Ampl[istep] = Math.Round(RXContactGain[istep], 3);

                                //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep] = Math.Round(RXContactGain[istep], 3);
                            }

                            Save_PXI_TraceRaw(ºTestParaName, ºTestUsePrev, ºSave_MXATrace, rbw_counter, 1e6);

                            #endregion

                            for (int i = 0; i < NoOfPts; i++)
                            {
                                BuildResults(ref results, ºTestParaName + "_" + RXContactFreq[i] + "_Rx-Gain" , "dB", RXContactGain[i].ToString().Contains("Infinity") ? -999 : RXContactGain[i]);
                            }

                            //for test time checking
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }


                            #endregion
                            break;

                        case "PXI_NF_COLD":

                            #region PXI NF COLD
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure Function

                            #region RxGain & Loss gatherring for NF Measurement

                            if (ºPXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = ºPXI_NoOfSweep;

                            //For Collecting LNA Gain & Loss from previous Data -Seoul
                            NoOfPts = (Convert.ToInt32(Math.Ceiling((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1))) + 1;

                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];
                            RXPathLoss = new double[NoOfPts];
                            LNAInputLoss = new double[NoOfPts];
                            TXPAOnFreq = new double[NoOfPts];
                            RxGainDic = new Dictionary<double, double>();

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            //For Collecting RX Gain trace Number from Previous setting -Seoul
                            TestUsePrev_ArrayNo = 0;
                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(ºTestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    TestUsePrev_ArrayNo = i;
                                }
                            }

                            for (int i = 0; i < PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz.Length; i++)
                            {
                                RxGainDic.Add(PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz[i], PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].Ampl[i]);
                            }

                            ºTXFreq = ºStartTXFreq1;
                            ºRXFreq = ºStartRXFreq1;

                            count = Convert.ToInt16(Math.Ceiling((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1));

                            if ((ºStopTXFreq1 - ºStartTXFreq1) == (ºStopRXFreq1 - ºStartRXFreq1))
                            {
                                ºStepTXFreq = ºStepRXFreq1;
                            }

                            else
                            {
                                ºStepTXFreq = (ºStopTXFreq1 - ºStartTXFreq1) / (NoOfPts - 1);
                            }


                            for (int i = 0; i <= count; i++)
                            {
                                TXPAOnFreq[i] = Math.Round(ºTXFreq, 3);
                                RXContactFreq[i] = Math.Round(ºRXFreq, 3);

                                if (RxGainDic.TryGetValue(ºRXFreq, out RXContactGain[i])) { }
                                else
                                {
                                    MessageBox.Show("Need to check between RxGain & NF Frequency Range");
                                }

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                RXPathLoss[i] = ºLossOutputPathRX1;//Seoul

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºRXFreq, ref ºLossCouplerPath, ref StrError);
                                LNAInputLoss[i] = ºLossCouplerPath;//Seoul

                                ºTXFreq = Convert.ToSingle(Math.Round(ºTXFreq + ºStepTXFreq, 3));
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + ºStepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                                if (ºRXFreq > ºStopRXFreq1)//For Last Freq match
                                {
                                    ºTXFreq = ºStopTXFreq1;
                                    ºRXFreq = ºStopRXFreq1;
                                }
                            }
                            #endregion

                            #region Switching for NF Test
                            //Switching for NF Testing -Seoul
                            EqSwitch.SetPath(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], TCF_Header.ConstSwitching_Band_HotNF, ºSwBand_HotNF.ToUpper()));
                            PreviousSWMode = ºSwBand_HotNF.ToUpper();
                            DelayMs(ºSetup_Delay); //Disabled because this SW is not mechanical SW
                            #endregion

                            #region Cold NF Measurement
                            Cold_NF_new = new double[NumberOfRuns][];
                            Cold_NoisePower_new = new double[NumberOfRuns][];

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Cold_NF_new[i] = new double[NoOfPts];
                                Cold_NoisePower_new[i] = new double[NoOfPts];
                            }

                            EqVST.rfsaSession.Utility.Reset();

                            EqVST.PreConfig_VSTSA();
                            EqVST.ConfigureTriggers();  //Disable Trigger for NF Testing
                            EqVST.rfsaSession.Configuration.Triggers.ReferenceTrigger.Disable(); //Disable Reference Trigger for NF Testing.

                            EqRFmx.specNFColdSource2[TestCount][1].Commit(""); //Configure dummy setting before actual NF measurement

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                EqRFmx.specNFColdSource2[TestCount][0].Initiate("", "COLD" + TestCount.ToString() + "_" + i);
                                EqRFmx.WaitForAcquisitionComplete();
                            }
                            #endregion

                            #region ResetRFSA and Re-configure after NF Measurement

                            EqVST.rfsaSession.Utility.Reset();
                            EqVST.PreConfig_VSTSA();

                            #endregion

                            #region Sort and Store Trace Data
                            //Store multi trace from PXI to global array
                            for (int n = 0; n < NumberOfRuns; n++)
                            {
                                //temp trace array storage use for MAX , MIN etc calculation 
                                PXITrace[TestCount].Enable = true;
                                PXITrace[TestCount].SoakSweep = preSoakSweep;
                                PXITrace[TestCount].TestNumber = ºTestNum;
                                PXITrace[TestCount].TraceCount = NumberOfRuns;
                                PXITrace[TestCount].Multi_Trace[0][n].NoPoints = NoOfPts;
                                PXITrace[TestCount].Multi_Trace[0][n].RBW_Hz = ºNF_BW * 1e06;
                                PXITrace[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Result_Header = ºTestParaName;
                                PXITrace[TestCount].Multi_Trace[0][n].MXA_No = "PXI_NF_COLD_Trace";
                                PXITrace[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                for (istep = 0; istep < NoOfPts; istep++)
                                {
                                    PXITrace[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITrace[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3); //Yoonchun

                                    PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3); //Yoonchun
                                }
                            }
                            #endregion

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            ºTest_Pin1 = false;
                            ºTest_Pout1 = false;
                            ºTest_SMU = false;

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            ºTest_NF1 = false;

                            tTime.Stop();

                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }

                            #endregion
                            break;

                        case "PXI_NF_HOT":
                            // This is using PXI VST as Sweeper and Analyzer. Will do multiple sweep in one function because using script (Pwr Servo->Soak Sweep->SoakTime->MultiSweep)
                            // Slight different from LXI solution where you define number of sweep in multiple line in TCF

                            #region PXI NF HOT
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure Function

                            SGPowerLevel = -18;// -18 CDMA dBm //-20 LTE dBm  
                            SAReferenceLevel = -20;
                            SoakTime = 450e-3;
                            SoakFrequency = ºStartTXFreq1 * 1e6;
                            vBW_Hz = 300;
                            RBW_Hz = 1e6;
                            preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
                            preSoakSweepTemp = preSoakSweep == true ? 1 : 0; //to indicate if another sweep should be done
                            stepFreqMHz = 0.1;
                            tmpRXFreqHz = ºStartRXFreq1 * 1e6;
                            sweepPts = (Convert.ToInt32((ºStopTXFreq1 - ºStartTXFreq1) / stepFreqMHz)) + 1;
                            //----

                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            if (ºPXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = ºPXI_NoOfSweep;

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), ºWaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            ºTXFreq = ºStartTXFreq1;
                            TXCenterFreq = Convert.ToString((ºStartTXFreq1 + ºStopTXFreq1) / 2); //Seoul

                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = papr_dB - ºPin1 - totalInputLoss;

                            //if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                //generate modulated signal
                                string Script =
                                         "script powerServo\r\n"
                                       + "repeat forever\r\n"
                                       + "generate Signal" + ºWaveFormName + "\r\n"
                                       + "end repeat\r\n"
                                       + "end script";
                                TxPAOnScript = Script;

                                try
                                {
                                    EqVST.rfsgSession.Arb.Scripting.WriteScript(Script);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                EqVST.rfsgSession.RF.Frequency = Convert.ToDouble(TXCenterFreq) * 1e6; //Seoul
                                //EqVST.rfsgSession.RF.Frequency = ºStartTXFreq1 * 1e6; // Original
                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                                //Need to ensure that SG_IQRate re-define , because RX_CONTACT routine has overwritten the initialization data
                                EqVST.Get_s_SignalType(ºModulation, ºWaveFormName, out SG_IQRate);
                                EqVST.rfsgSession.Arb.IQRate = SG_IQRate;

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                                #endregion
                            }

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                if (!ºTunePwr_TX1)
                                {
                                    EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                    EqVST.rfsgSession.Initiate();
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    DelayMs(ºRdPwr_Delay);
                                    R_Pout1 = EqPwrMeter.MeasPwr(1);
                                    R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3);
                                    if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                    {
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default

                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin - papr_dB + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                                EqVST.rfsgSession.Initiate();
                                                DelayMs(ºRdPwr_Delay);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                DelayMs(ºRdPwr_Delay);
                                            }

                                            if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                            {
                                                SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            SGPowerLevel = SGTargetPin;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }


                            //Measure SMU current
                            MeasSMU = ºSMUMeasCh.Split(',');
                            if (ºTest_SMU)
                            {
                                DelayMs(ºRdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (ºSMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            //total test time for each parameter will include the soak time
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºNF_SoakTime)
                            {
                                syncTest_Delay = (long)ºNF_SoakTime - paramTestTime;
                                SoakTime = syncTest_Delay * 1e-3;       //convert to second
                            }
                            else
                            {
                                SoakTime = 0;                //no soak required if power servo longer than expected total test time                                                        
                            }

                            DelayMs(Convert.ToInt32(SoakTime * 1e3)); //Delay for Soak Time
                            EqVST.rfsgSession.Abort(); //Abort after power server & soak time
                            #endregion

                            #region RxGain & Loss gatherring for NF Measurement

                            //For Collecting LNA Gain & Loss from previous Data -Seoul
                            NoOfPts = (Convert.ToInt32(Math.Ceiling((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1))) + 1;

                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];
                            RXPathLoss = new double[NoOfPts];
                            LNAInputLoss = new double[NoOfPts];
                            TXPAOnFreq = new double[NoOfPts];
                            RxGainDic = new Dictionary<double, double>();

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            //For Collecting RX Gain trace Number from Previous setting -Seoul
                            TestUsePrev_ArrayNo = 0;
                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(ºTestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    TestUsePrev_ArrayNo = i;
                                }
                            }

                            for (int i = 0; i < PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz.Length; i++)
                            {
                                RxGainDic.Add(PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz[i], PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].Ampl[i]);
                            }

                            ºTXFreq = ºStartTXFreq1;
                            ºRXFreq = ºStartRXFreq1;
                            count = Convert.ToInt16(Math.Ceiling((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1));

                            if ((ºStopTXFreq1 - ºStartTXFreq1) == (ºStopRXFreq1 - ºStartRXFreq1))
                            {
                                ºStepTXFreq = ºStepRXFreq1;
                            }

                            else
                            {
                                ºStepTXFreq = (ºStopTXFreq1 - ºStartTXFreq1) / (NoOfPts - 1);
                            }


                            for (int i = 0; i <= count; i++)
                            {
                                TXPAOnFreq[i] = Math.Round(ºTXFreq, 3);
                                RXContactFreq[i] = Math.Round(ºRXFreq, 3);

                                if (RxGainDic.TryGetValue(ºRXFreq, out RXContactGain[i])) { }
                                else
                                {
                                    MessageBox.Show("Need to check between RxGain & NF Frequency Range");
                                }

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                RXPathLoss[i] = ºLossOutputPathRX1;//Seoul

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºRXFreq, ref ºLossCouplerPath, ref StrError);
                                LNAInputLoss[i] = ºLossCouplerPath;//Seoul

                                ºTXFreq = Convert.ToSingle(Math.Round(ºTXFreq + ºStepTXFreq, 3));
                                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + ºStepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                                if (ºRXFreq > ºStopRXFreq1)//For Last Freq match
                                {
                                    ºTXFreq = ºStopTXFreq1;
                                    ºRXFreq = ºStopRXFreq1;
                                }
                            }
                            #endregion

                            #region Switching for NF Test
                            //Switching for NF Testing -Seoul
                            EqSwitch.SetPath(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], TCF_Header.ConstSwitching_Band_HotNF, ºSwBand_HotNF.ToUpper()));
                            PreviousSWMode = ºSwBand_HotNF.ToUpper();
                            DelayMs(ºSetup_Delay); //Disabled because Ant SW is not mechanical SW
                            #endregion

                            #region Hot NF Measurement
                            Hot_NF_new = new double[NumberOfRuns][];
                            Hot_NoisePower_new = new double[NumberOfRuns][];

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Hot_NF_new[i] = new double[NoOfPts];
                                Hot_NoisePower_new[i] = new double[NoOfPts];
                            }

                            EqVST.rfsaSession.Utility.Reset();

                            EqVST.PreConfig_VSTSA();
                            EqVST.ConfigureTriggers();  //Disable Trigger for NF Testing
                            EqVST.rfsaSession.Configuration.Triggers.ReferenceTrigger.Disable(); //Disable Reference Trigger for NF Testing.

                            EqVST.rfsgSession.Arb.Scripting.WriteScript(TxPAOnScript);
                            EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                            EqVST.Get_s_SignalType(ºModulation, ºWaveFormName, out SG_IQRate);
                            EqVST.rfsgSession.Arb.IQRate = SG_IQRate;
                            EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                            EqRFmx.specNFColdSource2[TestCount][RXContactFreq.Length].Commit(""); //Configure dummy setting before actual NF measurement

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                for (int j = 0; j < RXContactFreq.Length; j++)
                                {
                                    double[] rxFreqPoint = new double[1];
                                    double[] rxGainPoint = new double[1];
                                    rxFreqPoint[0] = RXContactFreq[j];
                                    rxGainPoint[0] = RXContactGain[j];

                                    EqVST.DoneVSGinit.Reset();
                                    EqRFmx.DoneNFCommit.Reset();

                                    ThreadPool.QueueUserWorkItem(EqRFmx.NFcommit2, new LibEqmtDriver.NF_VST.NF_NI_RFmx.Config_RFmx(TestCount, j, rxFreqPoint, rxGainPoint));
                                    ThreadPool.QueueUserWorkItem(EqVST.VSGInitiate, new LibEqmtDriver.NF_VST.NF_NiPXI_VST.Config_SG(TestCount, j, TxPAOnScript, ºModulation, ºWaveFormName, SGTargetPin, TXPAOnFreq[j]));

                                    EqVST.DoneVSGinit.WaitOne();
                                    EqRFmx.DoneNFCommit.WaitOne();

                                    EqRFmx.specNFColdSource2[TestCount][j].Initiate("", "HOT" + TestCount + "_" + i.ToString() + "_" + j.ToString());
                                    EqRFmx.WaitForAcquisitionComplete();
                                }
                                EqVST.rfsgSession.Abort(); //SG Abort after HotPA NF Test
                            }
                            #endregion

                            #region ResetRFSA and Re-configure after NF Measurement
                            EqVST.rfsaSession.Utility.Reset();
                            EqVST.PreConfig_VSTSA();
                            #endregion

                            #region Sort and Store Trace Data
                            //Store multi trace from PXI to global array
                            for (int n = 0; n < NumberOfRuns; n++)
                            {
                                //temp trace array storage use for MAX , MIN etc calculation 
                                PXITrace[TestCount].Enable = true;
                                PXITrace[TestCount].SoakSweep = preSoakSweep;
                                PXITrace[TestCount].TestNumber = ºTestNum;
                                PXITrace[TestCount].TraceCount = NumberOfRuns;
                                PXITrace[TestCount].Multi_Trace[0][n].NoPoints = NoOfPts;
                                PXITrace[TestCount].Multi_Trace[0][n].RBW_Hz = ºNF_BW * 1e06;
                                PXITrace[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Result_Header = ºTestParaName;
                                PXITrace[TestCount].Multi_Trace[0][n].MXA_No = "PXI_NF_HOT_Trace";
                                PXITrace[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                for (istep = 0; istep < NoOfPts; istep++)
                                {
                                    PXITrace[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITrace[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3);

                                    PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3);
                                }
                            }
                            #endregion

                            //Sort out test result for all traces and Add test result
                            for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                            {
                                if (i == 0)
                                {
                                    if (ºTest_Pin1)
                                    {
                                        //BuildResults(ref results, ºTestParaName + rbwParamName + "_Pin1" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dBm", R_Pin1);        //coding bug -  rbwParamName not define 23/02/2018
                                        BuildResults(ref results, ºTestParaName + "_" + ºNF_BW + "MHz_Pin1" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dBm", R_Pin1);
                                    }
                                    if (ºTest_Pout1)
                                    {
                                        //BuildResults(ref results, ºTestParaName + rbwParamName + "_Pout1" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dBm", R_Pout1);      //coding bug -  rbwParamName not define 23/02/2018
                                        BuildResults(ref results, ºTestParaName + "_" + ºNF_BW + "MHz_Pout1" + "_" + ºWaveFormName + "_" + ºPout1 + "dBm", "dBm", R_Pout1);
                                    }
                                    if (ºTest_SMU)
                                    {
                                        MeasSMU = ºSMUMeasCh.Split(',');
                                        for (int j = 0; j < MeasSMU.Count(); j++)
                                        {
                                            //BuildResults(ref results, ºTestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);      //coding bug -  rbwParamName not define 23/02/2018
                                            BuildResults(ref results, ºTestParaName + "_" + ºNF_BW + "MHz_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                        }
                                    }
                                    if (ºTest_NF1)
                                    {
                                    }
                                }
                                else
                                {
                                    if (ºTest_NF1)
                                    {
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            ºTest_Pin1 = false;
                            ºTest_Pout1 = false;
                            ºTest_SMU = false;

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            ºTest_NF1 = false;


                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                BuildResults(ref results, ºTestParaName + "_TestTime" + ºTestNum, "mS", tTime.ElapsedMilliseconds);
                            }
                            #endregion
                            break;

                        default:
                            MessageBox.Show("NF Test Parameter : " + ºTestParam.ToUpper() + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                        #endregion
                    }
                    break;

                case "MKR_NF":
                    switch (ºTestParam.ToUpper())
                    {
                        #region LXI FBAR NOISE TEST - Using MARKER NOISE for normalization
                        //Using Marker Noise function during pathloss calibration to normalize dB/xMHz Noise Floor to dB/Hz
                        //Normalize the result with Calibration Tag -> RFOUT_xxxxx_MKRNoise_xMHz

                        case "NF_CA_NDIAG":
                            // This sweep is a faster sweep , it is a continuous sweep base on SG freq sweep mode
                            #region NF CA NDIAG

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            rx2_span = 0;
                            rx2_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            //DelayMs(ºStartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";
                            #endregion

                            #region PowerSensor Offset, MXG , MXA1 and MXA2 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            #region MXA1 Marker Offset Calculation
                            tmpMkrNoiseLoss = 0;
                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1);
                            ºRXFreq = ºStartRXFreq1;
                            MkrCalSegmTag = myUtility.CalSegm_Setting.RX1CalSegm + "_MKRNoise_" + myUtility.MXA_Setting.RBW / 1e6 + "MHz";
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, MkrCalSegmTag, ºRXFreq, ref mkrNoiseLoss, ref StrError);
                                tmpMkrNoiseLoss = tmpMkrNoiseLoss + mkrNoiseLoss;
                                ºRXFreq = ºRXFreq + ºStepRXFreq1;
                            }
                            AveMkrNoiseLossRX1 = tmpMkrNoiseLoss / (count + 1);
                            #endregion

                            #region MXA2 Marker Offset Calculation
                            tmpMkrNoiseLoss = 0;
                            count = Convert.ToInt16((ºStopRXFreq2 - ºStartRXFreq2) / ºStepRXFreq2);
                            ºRXFreq = ºStartRXFreq2;
                            MkrCalSegmTag = myUtility.CalSegm_Setting.RX2CalSegm + "_MKRNoise_" + myUtility.MXA_Setting.RBW / 1e6 + "MHz";
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, MkrCalSegmTag, ºRXFreq, ref mkrNoiseLoss, ref StrError);
                                tmpMkrNoiseLoss = tmpMkrNoiseLoss + mkrNoiseLoss;
                                ºRXFreq = ºRXFreq + ºStepRXFreq2;
                            }
                            AveMkrNoiseLossRX2 = tmpMkrNoiseLoss / (count + 1);
                            #endregion

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            EqSG01.SetFreq(Convert.ToDouble(ºSG1_DefaultFreq));

                            if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = ºStopTXFreq1 - ºStartTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / ºStepTXFreq1);
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                EqSG01.SET_START_FREQUENCY(ºStartTXFreq1 - (ºStepTXFreq1 / 2));
                                EqSG01.SET_STOP_FREQUENCY(ºStopTXFreq1 + (ºStepTXFreq1 / 2));
                                EqSG01.SET_TRIG_TIMERPERIOD(ºDwellT1);
                                EqSG01.SET_SWEEP_POINT(tx1_noPoints + 2);   //need to add additional 2 points to calculated no of points because of extra point of start_freq and stop_freq for MXG and MXA sync

                                SGTargetPin = ºPin1 - totalInputLoss;
                                EqSG01.SetAmplitude((float)SGTargetPin);
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), ºWaveFormName);
                                EqSG01.SELECT_WAVEFORM(ModulationType);
                                EqSG01.SET_ROUTE_CONN_TOUT(LibEqmtDriver.SG.N5182A_ROUTE_SUBSYS.SweepRun);

                                EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode
                                if (ºSetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(ºSetup_Delay);
                                rx1_span = (ºStopRXFreq1 - ºStartRXFreq1);
                                rx1_cntrfreq = ºStartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                //EqSA01.AUTO_ATTENUATION(true); //Anthony
                                EqSA01.AUTO_ATTENUATION(false); //Anthony
                                if (Convert.ToDouble(ºSA1att) != CurrentSaAttn) //Anthony
                                {
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA1att));
                                    CurrentSaAttn = Convert.ToDouble(ºSA1att);
                                }
                                EqSA01.TRIGGER_SINGLE();
                                EqSA01.TRACE_AVERAGE(1);
                                EqSA01.AVERAGE_OFF();

                                EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                EqSA01.SPAN(rx1_span);
                                EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                //EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                EqSA01.SWEEP_POINTS(rx1_mxa_nopts);

                                if (ºSetRX1NDiag)
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(ºSA1att));
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)ºDwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * ºDwellT1));
                                }
                                else
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(ºRX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                EqSA01.CLEAR_WRITE();

                                status = EqSA01.OPERATION_COMPLETE();

                                #endregion

                                #region MXA 2 setup
                                DelayMs(ºSetup_Delay);
                                rx2_span = (ºStopRXFreq2 - ºStartRXFreq2);
                                rx2_cntrfreq = ºStartRXFreq2 + (rx2_span / 2);
                                rx2_mxa_nopts = (int)((rx2_span / rx2_mxa_nopts_step) + 1);

                                EqSA02.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                //EqSA02.AUTO_ATTENUATION(true); //Anthony
                                EqSA02.AUTO_ATTENUATION(false);
                                if (Convert.ToDouble(ºSA1att) != CurrentSa2Attn) //Anthony
                                {
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA2att));
                                    CurrentSaAttn = Convert.ToDouble(ºSA2att);
                                }
                                EqSA02.TRIGGER_SINGLE();
                                EqSA02.TRACE_AVERAGE(1);
                                EqSA02.AVERAGE_OFF();

                                EqSA02.FREQ_CENT(rx2_cntrfreq.ToString(), "MHz");
                                EqSA02.SPAN(rx2_span);
                                EqSA02.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA02.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA02.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                EqSA02.SWEEP_POINTS(rx2_mxa_nopts);
                                //EqSA02.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);

                                if (ºSetRX2NDiag)
                                {
                                    EqSA02.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(ºSA1att));
                                    EqSA02.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)ºDwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    EqSA02.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    EqSA02.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * ºDwellT1));
                                }
                                else
                                {
                                    EqSA02.CONTINUOUS_MEASUREMENT_ON();
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA02.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    EqSA02.SWEEP_TIMES(Convert.ToInt16(ºRX2SweepT));
                                }

                                //Initialize & clear MXA trace
                                EqSA02.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx2_cntrfreq);
                                EqSA02.CLEAR_WRITE();

                                status = EqSA02.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                if (!ºTunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    if (ºTest_Pout1)
                                    {
                                        DelayMs(ºRdPwr_Delay);
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default

                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        //R_Pin = TargetPin + (float)ºLossInputPathSG1;
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }
                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºStartSync_Delay)
                            {
                                syncTest_Delay = (long)ºStartSync_Delay - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            if (pwrSearch)
                            {
                                EqSG01.SINGLE_SWEEP();
                                status = EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(ºTrig_Delay);
                                Capture_MXA1_Trace(1, ºTestNum, ºTestParaName, ºRX1Band, prevRslt, ºSave_MXATrace, AveMkrNoiseLossRX1);     //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                Read_MXA1_Trace(1, ºTestNum, out R_NF1_Freq, out R_NF1_Ampl, ºSearch_Method, ºTestParaName);
                                Capture_MXA2_Trace(1, ºTestNum, ºTestParaName, ºRX2Band, prevRslt, ºSave_MXATrace, AveMkrNoiseLossRX2);     //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                Read_MXA2_Trace(1, ºTestNum, out R_NF2_Freq, out R_NF2_Ampl, ºSearch_Method, ºTestParaName);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref ºLossOutputPathRX1, ref StrError);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX2CalSegm, R_NF2_Freq, ref ºLossOutputPathRX2, ref StrError);

                                R_NF1_Ampl = R_NF1_Ampl - ºLossOutputPathRX1 - tbOutputLoss;
                                R_NF2_Ampl = R_NF2_Ampl - ºLossOutputPathRX2 - tbOutputLoss;

                                //Save_MXA1Trace(1, ºTestParaName, ºSave_MXATrace);
                                //Save_MXA2Trace(1, ºTestParaName, ºSave_MXATrace);

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF2_Freq = -999;

                                R_NF1_Ampl = 999;
                                R_NF2_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (ºOffSG1)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            EqSA01.CLEAR_WRITE();
                            EqSA02.CLEAR_WRITE();
                            //EqSA01.SET_TRACE_DETECTOR("MAXHOLD");
                            //EqSA02.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(ºStopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "NF_NONCA_NDIAG":
                            // This sweep is a faster sweep , it is a continuous sweep base on SG freq sweep mode
                            #region NF NONCA NDIAG

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            SAReferenceLevel = -20;
                            vBW_Hz = 300;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            //DelayMs(ºStartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1);
                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;
                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            count = Convert.ToInt16((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1);
                            ºRXFreq = ºStartRXFreq1;
                            MkrCalSegmTag = myUtility.CalSegm_Setting.RX1CalSegm + "_MKRNoise_" + myUtility.MXA_Setting.RBW / 1e6 + "MHz";
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, MkrCalSegmTag, ºRXFreq, ref mkrNoiseLoss, ref StrError);
                                tmpMkrNoiseLoss = tmpMkrNoiseLoss + mkrNoiseLoss;
                                ºRXFreq = ºRXFreq + ºStepRXFreq1;
                            }
                            tmpAveMkrNoiseLoss = tmpMkrNoiseLoss / (count + 1);

                            //change PowerSensor, MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            EqSG01.SetFreq(Convert.ToDouble(ºSG1_DefaultFreq));

                            if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = ºStopTXFreq1 - ºStartTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / ºStepTXFreq1);
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                EqSG01.SET_START_FREQUENCY(ºStartTXFreq1 - (ºStepTXFreq1 / 2));
                                EqSG01.SET_STOP_FREQUENCY(ºStopTXFreq1 + (ºStepTXFreq1 / 2));
                                EqSG01.SET_TRIG_TIMERPERIOD(ºDwellT1);
                                EqSG01.SET_SWEEP_POINT(tx1_noPoints + 2);   //need to add additional 2 points to calculated no of points because of extra point of start_freq and stop_freq for MXG and MXA sync

                                SGTargetPin = ºPin1 - totalInputLoss;
                                EqSG01.SetAmplitude((float)SGTargetPin);
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), ºWaveFormName);
                                EqSG01.SELECT_WAVEFORM(ModulationType);
                                EqSG01.SET_ROUTE_CONN_TOUT(LibEqmtDriver.SG.N5182A_ROUTE_SUBSYS.SweepRun);
                                EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode

                                if (ºSetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(ºSetup_Delay);
                                rx1_span = (ºStopRXFreq1 - ºStartRXFreq1);
                                rx1_cntrfreq = ºStartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);

                                //ANTHONY-ATT
                                EqSA01.AUTO_ATTENUATION(false);
                                if (Convert.ToDouble(ºSA1att) != CurrentSaAttn)
                                {
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA1att));
                                    CurrentSaAttn = Convert.ToDouble(ºSA1att);
                                }

                                //EqSA01.ELEC_ATTEN_ENABLE(true);
                                EqSA01.TRIGGER_SINGLE();
                                EqSA01.TRACE_AVERAGE(1);
                                EqSA01.AVERAGE_OFF();

                                EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                EqSA01.SPAN(rx1_span);
                                EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                EqSA01.SWEEP_POINTS(rx1_mxa_nopts);

                                if (ºSetRX1NDiag)
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    //ANTHONY-ATT
                                    if (Convert.ToDouble(ºSA1att) != CurrentSaAttn) //Anthony
                                    {
                                        EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(ºSA1att));
                                        CurrentSaAttn = Convert.ToDouble(ºSA1att);
                                    }
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)ºDwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * ºDwellT1));
                                }
                                else
                                {
                                    EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    //ANTHONY-ATT
                                    if (myUtility.MXA_Setting.Attenuation != CurrentSaAttn)
                                    {
                                        EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                        CurrentSaAttn = Convert.ToDouble(myUtility.MXA_Setting.Attenuation);
                                    }
                                    EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    EqSA01.SWEEP_TIMES(Convert.ToInt16(ºRX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                EqSA01.CLEAR_WRITE();
                                status = EqSA01.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                //Just for maximator Special case // Trick - 39mA  21.06.16
                                //EqSMUDriver.SetVolt(SMUSetting[1], EqSMU, ºSMUVCh[1], ºSMUILimitCh[2]);

                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                if (!ºTunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    if (ºTest_Pout1)
                                    {
                                        DelayMs(ºRdPwr_Delay);
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        //R_Pin = TargetPin + (float)ºLossInputPathSG1;
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºStartSync_Delay)
                            {
                                syncTest_Delay = (long)ºStartSync_Delay - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            if (pwrSearch)
                            {
                                EqSG01.SINGLE_SWEEP();
                                status = EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(ºTrig_Delay);
                                Capture_MXA1_Trace(1, ºTestNum, ºTestParaName, ºRX1Band, prevRslt, ºSave_MXATrace, tmpAveMkrNoiseLoss);     //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                Read_MXA1_Trace(1, ºTestNum, out R_NF1_Freq, out R_NF1_Ampl, ºSearch_Method, ºTestParaName);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref ºLossOutputPathRX1, ref StrError);
                                R_NF1_Ampl = R_NF1_Ampl - ºLossOutputPathRX1 - tbOutputLoss;
                                //Save_MXA1Trace(1, ºTestParaName, ºSave_MXATrace);

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (ºOffSG1)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            EqSA01.CLEAR_WRITE();
                            //EqSA01.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(ºStopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "NF_FIX_NMAX":
                            // This sweep is a slow sweep , will change SG freq and measure NF for every test points
                            // Using Marker Function Noise (measure at dBm/Hz) with External Amp Gain Offset
                            #region NOISE STEP SWEEP NDIAG/NMAX

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(ºPoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(ºTestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(ºTestUsePrev, resultTag), 3);
                            }

                            DelayMs(ºStartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], ºCalTag.ToUpper(), ºSwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", ºSwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";
                            #endregion

                            #region Calc Average Pathloss, PowerSensor Offset, MXG and MXA1 configuration

                            #region Get Average Pathloss
                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            if (ºStopTXFreq1 == ºStartTXFreq1)
                            {
                                txcount = 1;
                            }
                            else
                            {
                                txcount = Convert.ToInt16(((ºStopTXFreq1 - ºStartTXFreq1) / ºStepTXFreq1) + 1);
                            }

                            tx_freqArray = new double[txcount];
                            contactPwr_Array = new double[txcount];
                            nfAmpl_Array = new double[txcount];
                            nfAmplFreq_Array = new double[txcount];

                            ºTXFreq = ºStartTXFreq1;
                            for (int i = 0; i < txcount; i++)
                            {
                                tx_freqArray[i] = ºTXFreq;

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, ºTXFreq, ref ºLossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)ºLossInputPathSG1;

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºTXFreq, ref ºLossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)ºLossCouplerPath;

                                ºTXFreq = ºTXFreq + ºStepTXFreq1;
                            }
                            //Calculate the average pathloss/pathgain
                            tmpAveInputLoss = tmpInputLoss / txcount;
                            tmpAveCouplerLoss = tmpCouplerLoss / txcount;
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //Get average pathloss base on start and stop freq
                            rxcount = Convert.ToInt16(((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1) + 1);
                            rx_freqArray = new double[rxcount];

                            ºRXFreq = ºStartRXFreq1;
                            for (int i = 0; i < rxcount; i++)
                            {
                                rx_freqArray[i] = ºRXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                                tmpRxLoss = tmpRxLoss + (float)ºLossOutputPathRX1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, MkrCalSegmTag, ºRXFreq, ref mkrNoiseLoss, ref StrError);
                                tmpMkrNoiseLoss = tmpMkrNoiseLoss + mkrNoiseLoss;
                                ºRXFreq = ºRXFreq + ºStepRXFreq1;
                            }
                            tmpAveRxLoss = tmpRxLoss / rxcount;
                            totalRXLoss = tmpAveRxLoss - tbOutputLoss;
                            tmpAveMkrNoiseLoss = tmpMkrNoiseLoss / rxcount;
                            #endregion

                            #region config Power Sensor, MXA and MXG
                            //change PowerSensor,  Set Default Power for MXG setting
                            EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = ºPin1 - totalInputLoss;
                            EqSG01.SetAmplitude((float)SGTargetPin);
                            EqSG01.SetFreq(Convert.ToDouble(ºStartTXFreq1));
                            EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                            if (PreviousMXAMode != ºSwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.FIX);
                                EqSG01.SetFreq(Math.Abs(Convert.ToDouble(ºStartTXFreq1)));

                                SGTargetPin = ºPin1 - totalInputLoss;
                                EqSG01.SetAmplitude((float)SGTargetPin);
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), ºWaveFormName);
                                EqSG01.SELECT_WAVEFORM(ModulationType);
                                EqSG01.SINGLE_SWEEP();

                                if (ºSetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(ºSetup_Delay);
                                if (ºSetRX1NDiag)
                                {
                                    //NDIAG - RX Bandwidth base on stepsize
                                    rx1_span = ºStepRXFreq1 * 2;
                                    rx1_cntrfreq = ºStartRXFreq1;
                                    rx1_mxa_nopts = 101;    //fixed no of points
                                }
                                else
                                {
                                    //NMAX - will use full RX Badwidth (StartRX to StopRX)
                                    rx1_span = ºStopRXFreq1 - ºStartRXFreq1;
                                    rx1_cntrfreq = Math.Round(ºStartRXFreq1 + (rx1_span / 2), 3);
                                    rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);
                                }

                                EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_FreeRun);
                                EqSA01.AUTO_ATTENUATION(false);
                                EqSA01.CONTINUOUS_MEASUREMENT_OFF();
                                EqSA01.TRACE_AVERAGE(1);
                                EqSA01.AVERAGE_OFF();

                                EqSA01.SPAN(rx1_span);
                                EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                //EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                EqSA01.SWEEP_POINTS(rx1_mxa_nopts);
                                EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(ºSA1att));
                                EqSA01.SWEEP_TIMES(Convert.ToInt16(ºRX1SweepT));

                                //Initialize & clear MXA trace
                                EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)ºStartRXFreq1);
                                EqSA01.CLEAR_WRITE();

                                status = EqSG01.OPERATION_COMPLETE();
                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = ºSwBand.ToUpper();
                            }
                            #endregion

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                                if (!ºTunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default

                                    if (ºTest_Pout1)
                                    {
                                        DelayMs(ºRdPwr_Delay);
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(ºPout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }

                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(ºPout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < ºSG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                                R_Pout1 = EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (ºPout1 - R_Pout1);

                                            if (SGTargetPin < ºSG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(ºRdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > ºSG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = ºPin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                for (int i = 0; i < tx_freqArray.Length; i++)
                                {
                                    if (ºSetRX1NDiag)   //NDIAG - RX Bandwidth base on stepsize else NMAX - will use full RX Badwidth (StartRX to StopRX)
                                    {
                                        EqSA01.FREQ_CENT(rx_freqArray[i].ToString(), "MHz");    //RX Bandwidth base on stepsize
                                        EqSG01.SetFreq(Convert.ToDouble(tx_freqArray[i]));
                                        EqSA01.TRIGGER_IMM();
                                        DelayMs(Convert.ToInt16(ºRX1SweepT));       //Need to set same delay as sweep time before read trace  

                                        status = EqSG01.OPERATION_COMPLETE();
                                        Capture_MXA1_Trace(1, ºTestNum, ºTestParaName, ºRX1Band, prevRslt, false, tmpAveMkrNoiseLoss);      //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                        Read_MXA1_Trace(1, ºTestNum, out nfAmplFreq_Array[i], out nfAmpl_Array[i], ºSearch_Method, ºTestParaName);
                                        nfAmpl_Array[i] = nfAmpl_Array[i] - totalRXLoss;
                                    }
                                    else
                                    {
                                        EqSG01.SetFreq(Convert.ToDouble(tx_freqArray[i]));
                                        EqSA01.TRIGGER_IMM();
                                        DelayMs(ºTrig_Delay);

                                        status = EqSG01.OPERATION_COMPLETE();
                                        Capture_MXA1_Trace(1, ºTestNum, ºTestParaName, ºRX1Band, prevRslt, ºSave_MXATrace, tmpAveMkrNoiseLoss);     //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                        Read_MXA1_Trace(1, ºTestNum, out nfAmplFreq_Array[i], out nfAmpl_Array[i], ºSearch_Method, ºTestParaName);
                                        nfAmpl_Array[i] = nfAmpl_Array[i] - totalRXLoss;
                                    }
                                }

                                #region Search result MAX or MIN and Save to Datalog
                                //Find result MAX or MIN result
                                switch (ºSearch_Method.ToUpper())
                                {
                                    case "MAX":
                                        R_NF1_Ampl = nfAmpl_Array.Max();
                                        indexdata = Array.IndexOf(nfAmpl_Array, R_NF1_Ampl);     //return index of max value
                                        R_NF1_Freq = nfAmplFreq_Array[indexdata];
                                        break;

                                    case "MIN":
                                        R_NF1_Ampl = nfAmpl_Array.Min();
                                        indexdata = Array.IndexOf(nfAmpl_Array, R_NF1_Ampl);     //return index of max value
                                        R_NF1_Freq = nfAmplFreq_Array[indexdata];
                                        break;

                                    default:
                                        MessageBox.Show("Test Parameter : " + ºTestParam + "(" + ºSearch_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                        break;
                                }

                                //Save all data to datalog 
                                if (ºSetRX1NDiag)           //save trace method is different between NDIAG and NMAX
                                {
                                    if (ºSave_MXATrace)
                                    {
                                        string[] templine = new string[4];
                                        ArrayList LocalTextList = new ArrayList();
                                        ArrayList tmpCalMsg = new ArrayList();

                                        //Calibration File Header
                                        LocalTextList.Add("#MXA1 NF STEP SWEEP DATALOG");
                                        LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                                        LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                                        LocalTextList.Add("#Input TX Power : " + ºPin1 + " dBm");
                                        LocalTextList.Add("#Measure Contact Power : " + Math.Round(R_Pout1, 3) + " dBm");
                                        templine[0] = "#TX_FREQ";
                                        templine[1] = "NOISE_RXFREQ";
                                        templine[2] = "NOISE_AMPL";
                                        LocalTextList.Add(string.Join(",", templine));

                                        // Start looping until complete the freq range
                                        for (istep = 0; istep < tx_freqArray.Length; istep++)
                                        {
                                            //Sorted the calibration result to array
                                            templine[0] = Convert.ToString(tx_freqArray[istep]);
                                            templine[1] = Convert.ToString(nfAmplFreq_Array[istep]);
                                            templine[2] = Convert.ToString(Math.Round(nfAmpl_Array[istep], 3));
                                            LocalTextList.Add(string.Join(",", templine));
                                        }

                                        //Write cal data to csv file
                                        if (!Directory.Exists(SNPFile.FileOutput_Path))
                                        {
                                            Directory.CreateDirectory(SNPFile.FileOutput_Path);
                                        }
                                        //Write cal data to csv file
                                        string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + ºTestParaName + "_Unit" + tmpUnit_No.ToString() + ".csv";
                                        //MessageBox.Show("Path : " + tempPath);
                                        IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
                                    }
                                }
                                #endregion

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                SGTargetPin = ºPin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = ºSMUMeasCh.Split(',');
                                if (ºTest_SMU)
                                {
                                    DelayMs(ºRdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (ºSMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = EqSMUDriver.MeasI(SMUSetting[smuIChannel], EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (ºOffSG1)
                            {
                                EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            EqSA01.CLEAR_WRITE();

                            DelayMs(ºStopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            if (ºTest_TestTime)
                            {
                                ATFResultBuilder.AddResultToDict(ºTestParaName + "_TestTime" + ºTestNum, tTime.ElapsedMilliseconds, ref StrError);
                            }

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)ºEstimate_TestTime)
                            {
                                syncTest_Delay = (long)ºEstimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        default:
                            MessageBox.Show("NF Test Parameter : " + ºTestParam.ToUpper() + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                        #endregion
                    }
                    break;

                default:
                    MessageBox.Show("Test Mode " + ºTestMode + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    break;

            }

            #endregion

            //Add test result
            #region add test result
            if (ºTest_Pin)
            {
                BuildResults(ref results, ºTestParaName + "_Pin", "dBm", R_Pin);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Result_Header = ºTestParaName + "_Pin";
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Result_Data = R_Pin;
            }
            if (ºTest_Pout)
            {
                BuildResults(ref results, ºTestParaName + "_Pout", "dBm", R_Pout);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Result_Header = ºTestParaName + "_Pout";
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Result_Data = R_Pout;
            }
            if (ºTest_Pin1)
            {
                BuildResults(ref results, ºTestParaName + "_Pin1", "dBm", R_Pin1);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Result_Header = ºTestParaName + "_Pin1";
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Result_Data = R_Pin1;
            }
            if (ºTest_Pout1)
            {
                BuildResults(ref results, ºTestParaName + "_Pout1", "dBm", R_Pout1);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Result_Header = ºTestParaName + "_Pout1";
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Result_Data = R_Pout1;
            }
            if (ºTest_Pin2)
            {
                BuildResults(ref results, ºTestParaName + "_Pin2", "dBm", R_Pin2);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Result_Header = ºTestParaName + "_Pin2";
                Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Result_Data = R_Pin2;
            }
            if (ºTest_Pout2)
            {
                BuildResults(ref results, ºTestParaName + "_Pout2", "dBm", R_Pout2);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Result_Header = ºTestParaName + "_Pout2";
                Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Result_Data = R_Pout2;
            }
            if (ºTest_NF1)
            {
                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Ampl", "dBm", R_NF1_Ampl);
                BuildResults(ref results, ºTestParaName + "_RX" + ºRX1Band + "_Freq", "MHz", R_NF1_Freq);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Result_Header = ºTestParaName + "_RX" + ºRX1Band + "_Ampl";
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Result_Data = R_NF1_Ampl;

                Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Result_Header = ºTestParaName + "_RX" + ºRX1Band + "_FREQ";
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Result_Data = R_NF1_Freq;
            }
            if (ºTest_NF2)
            {
                BuildResults(ref results, ºTestParaName + "_RX" + ºRX2Band + "_Ampl", "dBm", R_NF2_Ampl);
                BuildResults(ref results, ºTestParaName + "_RX" + ºRX2Band + "_Freq", "MHz", R_NF2_Freq);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Result_Header = ºTestParaName + "_RX" + ºRX2Band + "_Ampl";
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Result_Data = R_NF2_Ampl;

                Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Result_Header = ºTestParaName + "_RX" + ºRX2Band + "_FREQ";
                Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Result_Data = R_NF2_Freq;
            }
            if (ºTest_Harmonic)
            {
                BuildResults(ref results, ºTestParaName + "_Ampl", "dBm", R_H2_Ampl);
                BuildResults(ref results, ºTestParaName + "_Freq", "MHz", R_H2_Freq);

                //use as temp data storage for calculating MAX, MIN etc of multiple result
                Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Result_Header = ºTestParaName + "_Ampl";
                Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Result_Data = R_H2_Ampl;

                Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Enable = true;
                Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Result_Header = ºTestParaName + "_Freq";
                Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Result_Data = R_H2_Freq;
            }
            if (ºTest_MIPI)
                BuildResults(ref results, ºTestParaName + "_MIPI", "NA", R_MIPI);
            if (ºTest_SMU)
            {
                MeasSMU = ºSMUMeasCh.Split(',');
                for (int i = 0; i < MeasSMU.Count(); i++)
                {
                    BuildResults(ref results, ºTestParaName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[i])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[i])]);
                }
            }
            if (ºTest_DCSupply)
            {
                MeasDC = ºDCMeasCh.Split(',');
                for (int i = 0; i < MeasDC.Count(); i++)
                {
                    BuildResults(ref results, ºTestParaName + "_" + R_DCLabel_ICh[Convert.ToInt16(MeasDC[i])], "A", R_DC_ICh[Convert.ToInt16(MeasDC[i])]);
                }
            }
            if (ºTest_Switch)
                BuildResults(ref results, ºTestParaName + "_Status", "NA", R_Switch);
            if (R_RFCalStatus == 1)
                BuildResults(ref results, ºTestParaName + "_Status", "NA", R_RFCalStatus);
            #endregion
        }

        private double[] CalculatePowerRamp(double minPower, double maxPower, int sampleCount)
        {
            double[] ramp = new double[sampleCount];

            double step = (maxPower - minPower) / (sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                ramp[i] = minPower + i * step;
            }

            return ramp;
        }

        private void InitResultVariable()
        {
            R_NF1_Ampl = -999;
            R_NF2_Ampl = -999;
            R_NF1_Freq = -999;
            R_NF2_Freq = -999;
            R_H2_Ampl = -999;
            R_H2_Freq = -999;
            R_Pin = -999;
            R_Pout = -999;
            R_ITotal = -999;
            R_MIPI = -999;
            R_DCSupply = -999;
            R_Switch = -999;
            R_RFCalStatus = -999;
        }

        private void RF_Calibration(int Trig_Delay, int Generic_Delay, int RdCurr_Delay, int RdPwr_Delay, int Setup_Delay)
        {
            System.Collections.ArrayList tempArray;
            List<string> CalGroup = new List<string>();

            string tempString;
            string FileSetting = Convert.ToString(DicCalInfo[DataFilePath.LocSettingPath]);
            string LocSetFilePath = Convert.ToString(DicCalInfo[DataFilePath.LocSettingPath]);

            string VSTmodel = myUtility.ReadTextFile(LocSetFilePath, "Model", "PXI_VST");
            string VSTaddr = myUtility.ReadTextFile(LocSetFilePath, "Address", "PXI_VST");

            int ArrayCount, CalGroupCount, i = 0, FreqListCountRF = 0, FreqListCountNF = 0;
            bool blnUseSourceCalFactor = false;
            double power = -999;
            tempArray = myUtility.ReadCalProcedure(Convert.ToString(DicCalInfo[DataFilePath.LocSettingPath]));
            ArrayCount = tempArray.Count;
            double startFreq, stopFreq;
            int markerNo;
            string tmpCalHeader;

            for (i = 0; i < ArrayCount; i++)
            {
                if (tempArray[i].ToString().Contains("[Cal"))
                {
                    tempString = tempArray[i].ToString().Replace('[', '.').Replace(']', '.');
                    CalGroup.Add(tempString.Split('.')[1]);
                }
            }
            CalGroupCount = CalGroup.Count();

            myUtility.CalFileGeneration(Convert.ToString(DicCalInfo[DataFilePath.CalPathRF]));

            for (i = 0; i < CalGroupCount; i++)
            {

                if (myUtility.ReadTextFile(FileSetting, CalGroup[i], "Skip").ToUpper() == "FALSE")
                {
                    FileInfo fCalDataFile = new FileInfo(Convert.ToString(DicCalInfo[DataFilePath.CalPathRF]));

                    StreamWriter swCalDataFile = fCalDataFile.AppendText();

                    string tempFreq = string.Empty, tempCalResult = string.Empty, tempMkrCalResult = string.Empty;
                    string Source1_Model = myUtility.ReadTextFile(FileSetting, CalGroup[i], "Source1_Model").ToUpper();
                    string Source2_Model = myUtility.ReadTextFile(FileSetting, CalGroup[i], "Source2_Model").ToUpper();
                    string PowerLevel = myUtility.ReadTextFile(FileSetting, CalGroup[i], "PowerLevel").ToUpper();
                    string Modulation = myUtility.ReadTextFile(FileSetting, CalGroup[i], "Modulation").ToUpper();
                    int Measure_Channel = Convert.ToInt16(myUtility.ReadTextFile(FileSetting, CalGroup[i], "Measure_Channel"));
                    string Target_CalSegment = myUtility.ReadTextFile(FileSetting, CalGroup[i], "Target_CalSegment").ToUpper();
                    string Source_CalFactor = myUtility.ReadTextFile(FileSetting, CalGroup[i], "Source_CalFactor").ToUpper();
                    double CalLimitLow = Convert.ToDouble(myUtility.ReadTextFile(FileSetting, CalGroup[i], "CalLimitLow"));
                    double CalLimitHigh = Convert.ToDouble(myUtility.ReadTextFile(FileSetting, CalGroup[i], "CalLimitHigh"));
                    string CalType = myUtility.ReadTextFile(FileSetting, CalGroup[i], "Type").ToUpper();
                    string CalFreqList = myUtility.ReadTextFile(FileSetting, CalGroup[i], "CalFreqList");
                    string mkrNoise_RBW = myUtility.ReadTextFile(FileSetting, CalGroup[i], "MKRNoise_RBW");
                    string sa_config = myUtility.ReadTextFile(FileSetting, CalGroup[i], "SA_CONFIG").ToUpper();
                    double CalOffset = Convert.ToDouble(myUtility.ReadTextFile(FileSetting, CalGroup[i], "CalOffset"));
                    string switchPath = myUtility.ReadTextFile(FileSetting, CalGroup[i], "SwitchControl").ToUpper();

                    DialogResult calSkip = MessageBox.Show(myUtility.ReadTextFile(FileSetting, CalGroup[i], "MessageBox"), CalType + " (" + Target_CalSegment + ") -> Calibration Data - Press OK to proceed , CANCEL to skip", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (calSkip == DialogResult.Cancel)
                    {
                        CalType = "SKIP_CAL";
                    }

                    switch (switchPath)
                    {
                        case "NONE":
                        case "NA":
                            //Do nothing , switching not enable
                            break;
                        default:
                            EqSwitch.SetPath(switchPath);
                            break;
                    }

                    double[] noiseMKR_RBW;
                    double[] FreqListNF = new double[300], FreqListMKR = new double[300];
                    string[] FreqListRF = new string[300], SourceCalFactor = new string[300];
                    myUtility.LoadCalFreqList(myUtility.ReadTextFile(FileSetting, "FilePath", CalFreqList), ref FreqListRF, ref FreqListCountRF);
                    myUtility.LoadSourceData(Convert.ToString(DicCalInfo[DataFilePath.CalPathRF]), Source_CalFactor, FreqListRF, ref SourceCalFactor, ref blnUseSourceCalFactor, ref swCalDataFile);

                    //variable for display result
                    string[] dispFreq;
                    string[] dispCal;
                    string dispResult = "";
                    int iNewline = 0;
                    bool calStatus = false;
                    bool status = false;
                    bool callimitStatus = true;
                    string tmpMsgTxt = "";
                    double calRslt = -999;
                    string[] tempdataMkr;

                    switch (CalType)
                    {
                        case "RF_LOPWR_NFCAL":
                            #region RF Lo Noise Power Calibration
                            //Calibration using MXA

                            myUtility.Decode_MXA_Setting(sa_config);
                            startFreq = Convert.ToDouble(FreqListRF[0]);
                            stopFreq = Convert.ToDouble(FreqListRF[FreqListCountRF - 1]);
                            markerNo = 1;

                            switch (Measure_Channel)
                            {
                                case 1:
                                    EqSA01.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.ON);
                                    EqSA01.Measure_Setup(LibEqmtDriver.SA.N9020A_MEAS_TYPE.SweptSA);
                                    DelayMs(1500);

                                    EqSA01.SPAN(myUtility.MXA_Setting.Span / 1e6);        //Convert Hz To MHz
                                    EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                    EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                    EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);
                                    EqSA01.SWEEP_TIMES(myUtility.MXA_Setting.SweepT);
                                    EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA01.START_FREQ(startFreq.ToString(), "MHz");
                                    EqSA01.STOP_FREQ(stopFreq.ToString(), "MHz");
                                    EqSA01.TRIGGER_CONTINUOUS();
                                    break;
                                case 2:
                                    EqSA02.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.ON);
                                    EqSA02.Measure_Setup(LibEqmtDriver.SA.N9020A_MEAS_TYPE.SweptSA);
                                    DelayMs(1500);

                                    EqSA02.SPAN(myUtility.MXA_Setting.Span / 1e6);        //Convert Hz To MHz
                                    EqSA02.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                    EqSA02.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                    EqSA02.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);
                                    EqSA02.SWEEP_TIMES(myUtility.MXA_Setting.SweepT);
                                    EqSA02.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA02.START_FREQ(startFreq.ToString(), "MHz");
                                    EqSA02.STOP_FREQ(stopFreq.ToString(), "MHz");
                                    EqSA02.TRIGGER_CONTINUOUS();
                                    break;
                                default:
                                    MessageBox.Show("Wrong MXA Equipment selection : " + Measure_Channel + " , Only MXA 1 or 2 allow!!!");
                                    break;
                            }

                            DelayMs(1000);

                            do
                            {
                                //Initialize Variable
                                tempFreq = string.Empty;
                                tempCalResult = string.Empty;

                                //variable for display result
                                dispFreq = new string[FreqListCountRF];
                                dispCal = new string[FreqListCountRF];
                                dispResult = "";
                                iNewline = 0;
                                calStatus = false;
                                callimitStatus = true;

                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    tempFreq += "," + FreqListRF[iCount];

                                    EqSG01.SetFreq(Convert.ToDouble(FreqListRF[iCount]));
                                    EqSG01.SetAmplitude((float)Convert.ToDouble(PowerLevel));
                                    EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                    DelayMs(200);

                                    switch (Measure_Channel)
                                    {
                                        case 1:
                                            EqSA01.MARKER_TURN_ON_NORMAL_POINT(markerNo, (float)Convert.ToDouble(FreqListRF[iCount]));
                                            DelayMs(myUtility.MXA_Setting.SweepT);
                                            status = EqSA01.OPERATION_COMPLETE();
                                            power = EqSA01.READ_MARKER(markerNo) - Convert.ToDouble(PowerLevel) + CalOffset;
                                            break;
                                        case 2:
                                            EqSA02.MARKER_TURN_ON_NORMAL_POINT(markerNo, (float)Convert.ToDouble(FreqListRF[iCount]));
                                            DelayMs(myUtility.MXA_Setting.SweepT);
                                            status = EqSA02.OPERATION_COMPLETE();
                                            power = EqSA02.READ_MARKER(markerNo) - Convert.ToDouble(PowerLevel) + CalOffset;
                                            break;
                                        default:
                                            break;
                                    }

                                    //compare individual result with cal spec limit & set cal status flag
                                    if ((power < CalLimitLow) || (power > CalLimitHigh))
                                    {
                                        callimitStatus = false;
                                    }

                                    tempCalResult += "," + Math.Round(power, 3);
                                }

                                //Display calibration result 
                                dispFreq = tempFreq.Split(',');
                                dispCal = tempCalResult.Split(',');
                                dispResult = "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "\r\n";
                                iNewline = 0;

                                for (int iCount = 1; iCount < FreqListCountRF + 1; iCount++)
                                {
                                    dispResult += dispFreq[iCount] + "," + dispCal[iCount] + "   ";
                                    iNewline++;

                                    if (iNewline == 4)
                                    {
                                        dispResult += "\r\n";
                                        iNewline = 0;
                                    }
                                }

                                if (callimitStatus)
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data PASS *** " + "\n\r\r Press YES to Save and Continue, NO to Redo Calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "*** Calibration Data PASS ***", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (chkStatus == DialogResult.Yes)
                                    {
                                        calStatus = true;
                                    }
                                }
                                else
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data FAIL *** " + "\r\n Calibration Data Fail Spec -> USL: " + CalLimitHigh + " , LSL: " + CalLimitLow + "\n\r\r Press RETRY to redo Calibration , CANCEL to stop calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "!!! Calibration Data FAIL !!!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

                                    if (chkStatus == DialogResult.Cancel)
                                    {
                                        calStatus = true;
                                    }
                                }

                            } while (!calStatus);

                            EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                            //Write to file if CalLimitStatus is True
                            if (callimitStatus)
                            {
                                swCalDataFile.WriteLine("");
                                swCalDataFile.WriteLine(Target_CalSegment + tempFreq);
                                swCalDataFile.WriteLine(tempCalResult);
                            }

                            break;
                            #endregion

                        case "RF_LOPWR_CAL":
                            #region RF Lo Power Calibration
                            //Calibration using MXA

                            myUtility.Decode_MXA_Setting(sa_config);

                            switch (Measure_Channel)
                            {
                                case 1:
                                    EqSA01.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.ON);
                                    EqSA01.Measure_Setup(LibEqmtDriver.SA.N9020A_MEAS_TYPE.SweptSA);
                                    DelayMs(1500);

                                    EqSA01.SPAN(myUtility.MXA_Setting.Span / 1e6);        //Convert Hz To MHz
                                    EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                    EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                    EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);
                                    EqSA01.SWEEP_TIMES(myUtility.MXA_Setting.SweepT);
                                    EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA01.TRIGGER_CONTINUOUS();
                                    break;
                                case 2:
                                    EqSA02.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.ON);
                                    EqSA02.Measure_Setup(LibEqmtDriver.SA.N9020A_MEAS_TYPE.SweptSA);
                                    DelayMs(1500);

                                    EqSA02.SPAN(myUtility.MXA_Setting.Span / 1e6);        //Convert Hz To MHz
                                    EqSA02.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                    EqSA02.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                    EqSA02.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);
                                    EqSA02.SWEEP_TIMES(myUtility.MXA_Setting.SweepT);
                                    EqSA02.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA02.TRIGGER_CONTINUOUS();
                                    break;
                                default:
                                    MessageBox.Show("Wrong MXA Equipment selection : " + Measure_Channel + " , Only MXA 1 or 2 allow!!!");
                                    break;
                            }

                            DelayMs(1000);

                            do
                            {
                                //Initialize Variable
                                tempFreq = string.Empty;
                                tempCalResult = string.Empty;

                                //variable for display result
                                dispFreq = new string[FreqListCountRF];
                                dispCal = new string[FreqListCountRF];
                                dispResult = "";
                                iNewline = 0;
                                calStatus = false;
                                callimitStatus = true;

                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    tempFreq += "," + FreqListRF[iCount];

                                    EqSG01.SetFreq(Convert.ToDouble(FreqListRF[iCount]));
                                    EqSG01.SetAmplitude((float)Convert.ToDouble(PowerLevel));
                                    EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                    DelayMs(200);

                                    switch (Measure_Channel)
                                    {
                                        case 1:
                                            EqSA01.FREQ_CENT(FreqListRF[iCount].ToString(), "MHz");
                                            DelayMs(500);
                                            power = EqSA01.MEASURE_PEAK_POINT(10) - Convert.ToDouble(PowerLevel) + CalOffset;
                                            break;
                                        case 2:
                                            EqSA02.FREQ_CENT(FreqListRF[iCount].ToString(), "MHz");
                                            DelayMs(500);
                                            power = EqSA02.MEASURE_PEAK_POINT(10) - Convert.ToDouble(PowerLevel) + CalOffset;
                                            break;
                                        default:
                                            break;
                                    }

                                    //compare individual result with cal spec limit & set cal status flag
                                    if ((power < CalLimitLow) || (power > CalLimitHigh))
                                    {
                                        callimitStatus = false;
                                    }
                                    tempCalResult += "," + Math.Round(power, 3);
                                }

                                //Display calibration result 
                                dispFreq = tempFreq.Split(',');
                                dispCal = tempCalResult.Split(',');
                                dispResult = "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "\r\n";
                                iNewline = 0;

                                for (int iCount = 1; iCount < FreqListCountRF + 1; iCount++)
                                {
                                    dispResult += dispFreq[iCount] + "," + dispCal[iCount] + "   ";
                                    iNewline++;

                                    if (iNewline == 4)
                                    {
                                        dispResult += "\r\n";
                                        iNewline = 0;
                                    }
                                }

                                if (callimitStatus)
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data PASS *** " + "\n\r\r Press YES to Save and Continue, NO to Redo Calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "*** Calibration Data PASS ***", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (chkStatus == DialogResult.Yes)
                                    {
                                        calStatus = true;
                                    }
                                }
                                else
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data FAIL *** " + "\r\n Calibration Data Fail Spec -> USL: " + CalLimitHigh + " , LSL: " + CalLimitLow + "\n\r\r Press RETRY to redo Calibration , CANCEL to stop calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "!!! Calibration Data FAIL !!!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

                                    if (chkStatus == DialogResult.Cancel)
                                    {
                                        calStatus = true;
                                    }
                                }

                            } while (!calStatus);

                            EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                            //Write to file if CalLimitStatus is True
                            if (callimitStatus)
                            {
                                swCalDataFile.WriteLine("");
                                swCalDataFile.WriteLine(Target_CalSegment + tempFreq);
                                swCalDataFile.WriteLine(tempCalResult);
                            }

                            break;
                            #endregion

                        case "RF_HIPWR_CAL":
                            #region RF High Power Calibration
                            // calibration using Power Meter
                            tempFreq = string.Empty;
                            tempCalResult = string.Empty;

                            EqPwrMeter.SetOffset(1, CalOffset);

                            do
                            {
                                //Initialize Variable
                                tempFreq = string.Empty;
                                tempCalResult = string.Empty;

                                //variable for display result
                                dispFreq = new string[FreqListCountRF];
                                dispCal = new string[FreqListCountRF];
                                dispResult = "";
                                iNewline = 0;
                                calStatus = false;
                                callimitStatus = true;

                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    tempFreq += "," + FreqListRF[iCount];

                                    EqSG01.SetFreq(Convert.ToDouble(FreqListRF[iCount]));
                                    EqSG01.SetAmplitude((float)Convert.ToDouble(PowerLevel));
                                    EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                    DelayMs(Setup_Delay);

                                    EqPwrMeter.SetFreq(1, Convert.ToDouble(FreqListRF[iCount]));
                                    DelayMs(RdPwr_Delay);
                                    power = EqPwrMeter.MeasPwr(1) - Convert.ToDouble(PowerLevel);

                                    tempCalResult += "," + Math.Round(power, 3);

                                    //compare individual result with cal spec limit & set cal status flag
                                    if (power < CalLimitLow || power > CalLimitHigh)
                                    {
                                        callimitStatus = false;
                                    }
                                }

                                //Display calibration result 
                                dispFreq = tempFreq.Split(',');
                                dispCal = tempCalResult.Split(',');
                                dispResult = "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "\r\n";
                                iNewline = 0;

                                for (int iCount = 1; iCount < FreqListCountRF + 1; iCount++)
                                {
                                    dispResult += dispFreq[iCount] + "," + dispCal[iCount] + "   ";
                                    iNewline++;

                                    if (iNewline == 4)
                                    {
                                        dispResult += "\r\n";
                                        iNewline = 0;
                                    }
                                }

                                if (callimitStatus)
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data PASS *** " + "\n\r\r Press YES to Save and Continue, NO to Redo Calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "*** Calibration Data PASS ***", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (chkStatus == DialogResult.Yes)
                                    {
                                        calStatus = true;
                                    }
                                }
                                else
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data FAIL *** " + "\r\n Calibration Data Fail Spec -> USL: " + CalLimitHigh + " , LSL: " + CalLimitLow + "\n\r\r Press RETRY to redo Calibration , CANCEL to stop calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "!!! Calibration Data FAIL !!!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

                                    if (chkStatus == DialogResult.Cancel)
                                    {
                                        calStatus = true;
                                    }
                                }

                            } while (!calStatus);

                            EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            EqPwrMeter.SetOffset(1, 0); //reset power sensor offset to default : 0

                            //Write to file if CalLimitStatus is True
                            if (callimitStatus)
                            {
                                swCalDataFile.WriteLine("");
                                swCalDataFile.WriteLine(Target_CalSegment + tempFreq);
                                swCalDataFile.WriteLine(tempCalResult);
                            }

                            break;
                            #endregion

                        case "PXI_RF_LOPWR_NFCAL":
                            #region PXI RF Lo Noise Power Cal
                            //Note : using VST-NI5646R
                            //using start,stop and step freq while LXI base using freq list

                            myUtility.Decode_MXA_Setting(sa_config);
                            startFreq = Convert.ToDouble(FreqListRF[0]) * 1e6;
                            stopFreq = Convert.ToDouble(FreqListRF[FreqListCountRF - 1]) * 1e6;
                            double stepFreq = (stopFreq - startFreq) / (FreqListCountRF - 1);
                            float[] tempData = new float[FreqListCountRF];
                            calStatus = false;

                            do
                            {
                                //Initialize Variable
                                tempFreq = string.Empty;
                                tempCalResult = string.Empty;

                                //variable for display result
                                dispFreq = new string[FreqListCountRF];
                                dispCal = new string[FreqListCountRF];
                                dispResult = "";
                                iNewline = 0;
                                calStatus = false;
                                callimitStatus = true;

                                MyCal.MyVSTCal EqVSTCal;
                                EqVSTCal = new MyCal.MyVSTCal(VSTaddr);
                                EqVSTCal.initialize();

                                EqVSTCal.RFSAPreConfigure(myUtility.MXA_Setting.RefLevel);
                                EqVSTCal.RFSGPreConfigure(Convert.ToDouble(PowerLevel));

                                EqVSTCal.VSTConfigure_DuringTest(startFreq, stopFreq, stepFreq, myUtility.MXA_Setting.RBW);
                                tempData = EqVSTCal.measureLowNoiseCal(myUtility.MXA_Setting.VBW);
                                EqVSTCal.closeVST();

                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    tempFreq += "," + FreqListRF[iCount];
                                    calRslt = Math.Round(Convert.ToDouble(tempData[iCount]) - Convert.ToDouble(PowerLevel) + CalOffset, 3);
                                    tempCalResult += "," + calRslt;

                                    //compare individual result with cal spec limit & set cal status flag
                                    if ((calRslt < CalLimitLow) || (calRslt > CalLimitHigh))
                                    {
                                        callimitStatus = false;
                                    }
                                }
                                //Display calibration result 
                                dispFreq = tempFreq.Split(',');
                                dispCal = tempCalResult.Split(',');
                                dispResult = "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "\r\n";
                                iNewline = 0;

                                for (int iCount = 1; iCount < FreqListCountRF + 1; iCount++)
                                {
                                    dispResult += dispFreq[iCount] + "," + dispCal[iCount] + "   ";
                                    iNewline++;

                                    if (iNewline == 4)
                                    {
                                        dispResult += "\r\n";
                                        iNewline = 0;
                                    }
                                }

                                if (callimitStatus)
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data PASS *** " + "\n\r\r Press YES to Save and Continue, NO to Redo Calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "*** Calibration Data PASS ***", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (chkStatus == DialogResult.Yes)
                                    {
                                        calStatus = true;
                                    }
                                }
                                else
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data FAIL *** " + "\r\n Calibration Data Fail Spec -> USL: " + CalLimitHigh + " , LSL: " + CalLimitLow + "\n\r\r Press RETRY to redo Calibration , CANCEL to stop calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "!!! Calibration Data FAIL !!!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

                                    if (chkStatus == DialogResult.Cancel)
                                    {
                                        calStatus = true;
                                    }
                                }

                            } while (!calStatus);

                            //Write to file if CalLimitStatus is True
                            if (callimitStatus)
                            {
                                swCalDataFile.WriteLine("");
                                swCalDataFile.WriteLine(Target_CalSegment + tempFreq);
                                swCalDataFile.WriteLine(tempCalResult);
                            }

                            #endregion
                            break;

                        case "PXI_RF_HIPWR_CAL":
                            #region PXI RF High Power Calibration
                            // calibration using Power Meter and VST NI5646R

                            tempFreq = string.Empty;
                            tempCalResult = string.Empty;
                            bool initVSG = false;

                            EqPwrMeter.SetOffset(1, CalOffset);

                            #region MXG setup
                            MyCal.MyVSTCal EqVSGCal;
                            EqVSGCal = new MyCal.MyVSTCal(VSTaddr);
                            EqVSGCal.initialize();
                            EqVSGCal.RFSGPreConfigure(Convert.ToDouble(PowerLevel));

                            //generate modulated signal
                            string Script =
                                     "script powerServo\r\n"
                                   + "repeat forever\r\n"
                                   + "generate Signal" + "LowCal" + "\r\n"
                                   + "end repeat\r\n"
                                   + "end script";
                            EqVSGCal._rfsgSession.Arb.Scripting.WriteScript(Script);
                            EqVSGCal._rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                            #endregion

                            do
                            {
                                //Initialize Variable
                                tempFreq = string.Empty;
                                tempCalResult = string.Empty;

                                //variable for display result
                                dispFreq = new string[FreqListCountRF];
                                dispCal = new string[FreqListCountRF];
                                dispResult = "";
                                iNewline = 0;
                                calStatus = false;
                                callimitStatus = true;

                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    tempFreq += "," + FreqListRF[iCount];

                                    EqVSGCal._rfsgSession.RF.Frequency = Convert.ToDouble(FreqListRF[iCount]) * 1e6;
                                    EqVSGCal._rfsgSession.RF.PowerLevel = Convert.ToDouble(PowerLevel);

                                    if ((iCount == 0) && (initVSG == false))            //Turn RF ON for 1st time only - continuos mode
                                    {
                                        EqVSGCal._rfsgSession.Initiate();
                                        initVSG = true;
                                    }

                                    DelayMs(Setup_Delay);

                                    EqPwrMeter.SetFreq(1, Convert.ToDouble(FreqListRF[iCount]));
                                    DelayMs(RdPwr_Delay);
                                    power = EqPwrMeter.MeasPwr(1) - Convert.ToDouble(PowerLevel);

                                    tempCalResult += "," + Math.Round(power, 3);

                                    //compare individual result with cal spec limit & set cal status flag
                                    if (power < CalLimitLow || power > CalLimitHigh)
                                    {
                                        callimitStatus = false;
                                    }
                                }

                                //Display calibration result 
                                dispFreq = tempFreq.Split(',');
                                dispCal = tempCalResult.Split(',');
                                dispResult = "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "\r\n";
                                iNewline = 0;

                                for (int iCount = 1; iCount < FreqListCountRF + 1; iCount++)
                                {
                                    dispResult += dispFreq[iCount] + "," + dispCal[iCount] + "   ";
                                    iNewline++;

                                    if (iNewline == 4)
                                    {
                                        dispResult += "\r\n";
                                        iNewline = 0;
                                    }
                                }

                                if (callimitStatus)
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data PASS *** " + "\n\r\r Press YES to Save and Continue, NO to Redo Calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "*** Calibration Data PASS ***", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (chkStatus == DialogResult.Yes)
                                    {
                                        calStatus = true;
                                    }
                                }
                                else
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data FAIL *** " + "\r\n Calibration Data Fail Spec -> USL: " + CalLimitHigh + " , LSL: " + CalLimitLow + "\n\r\r Press RETRY to redo Calibration , CANCEL to stop calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "!!! Calibration Data FAIL !!!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

                                    if (chkStatus == DialogResult.Cancel)
                                    {
                                        calStatus = true;
                                    }
                                }

                            } while (!calStatus);

                            EqVSGCal._rfsgSession.Abort();         //stop power servo script
                            EqVSGCal.closeVST();
                            EqPwrMeter.SetOffset(1, 0); //reset power sensor offset to default : 0

                            //Write to file if CalLimitStatus is True
                            if (callimitStatus)
                            {
                                swCalDataFile.WriteLine("");
                                swCalDataFile.WriteLine(Target_CalSegment + tempFreq);
                                swCalDataFile.WriteLine(tempCalResult);
                            }

                            #endregion
                            break;

                        case "RF_LOPWR_NFCAL_NOISEMKR":
                            #region RF Lo Noise Power Calibration with Noise Marker use for noise floor normalization
                            //Calibration using MXA

                            myUtility.Decode_MXA_Setting(sa_config);
                            startFreq = Convert.ToDouble(FreqListRF[0]);
                            stopFreq = Convert.ToDouble(FreqListRF[FreqListCountRF - 1]);
                            markerNo = 1;
                            tempdataMkr = mkrNoise_RBW.Split(';');

                            #region MXA/MXG Setting
                            switch (Measure_Channel)
                            {
                                case 1:
                                    EqSA01.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.ON);
                                    EqSA01.Measure_Setup(LibEqmtDriver.SA.N9020A_MEAS_TYPE.SweptSA);
                                    DelayMs(1500);

                                    EqSA01.SPAN(myUtility.MXA_Setting.Span / 1e6);        //Convert Hz To MHz
                                    EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                    EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                    EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);
                                    EqSA01.SWEEP_TIMES(myUtility.MXA_Setting.SweepT);
                                    EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                    EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA01.START_FREQ(startFreq.ToString(), "MHz");
                                    EqSA01.STOP_FREQ(stopFreq.ToString(), "MHz");
                                    EqSA01.TRIGGER_CONTINUOUS();
                                    break;
                                case 2:
                                    EqSA02.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.ON);
                                    EqSA02.Measure_Setup(LibEqmtDriver.SA.N9020A_MEAS_TYPE.SweptSA);
                                    DelayMs(1500);

                                    EqSA02.SPAN(myUtility.MXA_Setting.Span / 1e6);        //Convert Hz To MHz
                                    EqSA02.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                    EqSA02.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                    EqSA02.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);
                                    EqSA02.SWEEP_TIMES(myUtility.MXA_Setting.SweepT);
                                    EqSA02.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                    EqSA02.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    EqSA02.START_FREQ(startFreq.ToString(), "MHz");
                                    EqSA02.STOP_FREQ(stopFreq.ToString(), "MHz");
                                    EqSA02.TRIGGER_CONTINUOUS();
                                    break;
                                default:
                                    MessageBox.Show("Wrong MXA Equipment selection : " + Measure_Channel + " , Only MXA 1 or 2 allow!!!");
                                    break;
                            }

                            DelayMs(1000);
                            #endregion

                            #region NOISE MARKER CAL

                            noiseMKR_RBW = new double[tempdataMkr.Length];
                            for (int count = 0; count < tempdataMkr.Length; count++)
                            {
                                //Initialize Variable
                                tempFreq = string.Empty;
                                tempCalResult = string.Empty;
                                FreqListNF = new double[FreqListCountRF];
                                FreqListMKR = new double[FreqListCountRF];

                                //variable for display result
                                dispFreq = new string[FreqListCountRF];
                                dispCal = new string[FreqListCountRF];
                                dispResult = "";
                                iNewline = 0;
                                calStatus = false;
                                callimitStatus = true;
                                tmpCalHeader = "";

                                noiseMKR_RBW[count] = Convert.ToDouble(tempdataMkr[count]);
                                tmpCalHeader = Target_CalSegment + "_MKRNoise_" + noiseMKR_RBW[count] / 1e6 + "MHz";

                                #region Maker Noise Measurement

                                switch (Measure_Channel)
                                {
                                    case 1:
                                        EqSA01.RESOLUTION_BW(noiseMKR_RBW[count]);
                                        EqSA01.MARKER_NOISE(true, markerNo, noiseMKR_RBW[count]);
                                        break;
                                    case 2:
                                        EqSA02.RESOLUTION_BW(noiseMKR_RBW[count]);
                                        EqSA02.MARKER_NOISE(true, markerNo, noiseMKR_RBW[count]);
                                        break;
                                    default:
                                        break;
                                }

                                DelayMs(1000);

                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    tempFreq += "," + FreqListRF[iCount];

                                    switch (Measure_Channel)
                                    {
                                        case 1:
                                            EqSA01.MARKER_TURN_ON_NORMAL_POINT(markerNo, (float)Convert.ToDouble(FreqListRF[iCount]));
                                            DelayMs(myUtility.MXA_Setting.SweepT);
                                            status = EqSA01.OPERATION_COMPLETE();
                                            FreqListMKR[iCount] = Math.Round(EqSA01.READ_MARKER(markerNo), 3);
                                            break;
                                        case 2:
                                            EqSA02.MARKER_TURN_ON_NORMAL_POINT(markerNo, (float)Convert.ToDouble(FreqListRF[iCount]));
                                            DelayMs(myUtility.MXA_Setting.SweepT);
                                            status = EqSA02.OPERATION_COMPLETE();
                                            FreqListMKR[iCount] = Math.Round(EqSA02.READ_MARKER(markerNo), 3);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                #endregion

                                #region Maker Noise Off Measurement
                                switch (Measure_Channel)
                                {
                                    case 1:
                                        EqSA01.MARKER_NOISE(false, markerNo);
                                        break;
                                    case 2:
                                        EqSA02.MARKER_NOISE(false, markerNo);
                                        break;
                                    default:
                                        break;
                                }

                                DelayMs(1000);

                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    switch (Measure_Channel)
                                    {
                                        case 1:
                                            EqSA01.MARKER_TURN_ON_NORMAL_POINT(markerNo, (float)Convert.ToDouble(FreqListRF[iCount]));
                                            DelayMs(myUtility.MXA_Setting.SweepT);
                                            status = EqSA01.OPERATION_COMPLETE();
                                            FreqListNF[iCount] = Math.Round(EqSA01.READ_MARKER(markerNo), 3);
                                            break;
                                        case 2:
                                            EqSA02.MARKER_TURN_ON_NORMAL_POINT(markerNo, (float)Convert.ToDouble(FreqListRF[iCount]));
                                            DelayMs(myUtility.MXA_Setting.SweepT);
                                            status = EqSA02.OPERATION_COMPLETE();
                                            FreqListNF[iCount] = Math.Round(EqSA02.READ_MARKER(markerNo), 3);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                #endregion

                                #region Calc Normalization offset
                                double dB_Hz = 10 * Math.Log10(noiseMKR_RBW[count]);      //convert RBW to dB/Hz
                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    //offset = (Normalize Noise xMHz_RBW to dB/Hz) - Noise Marker at dB/Hz
                                    power = (FreqListNF[iCount] - dB_Hz) - FreqListMKR[iCount];
                                    tempCalResult += "," + Math.Round(power, 3);
                                }

                                //Display calibration result 
                                dispFreq = tempFreq.Split(',');
                                dispCal = tempCalResult.Split(',');
                                dispResult = "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "\r\n";
                                iNewline = 0;

                                for (int iCount = 1; iCount < FreqListCountRF + 1; iCount++)
                                {
                                    dispResult += dispFreq[iCount] + "," + dispCal[iCount] + "   ";
                                    iNewline++;

                                    if (iNewline == 4)
                                    {
                                        dispResult += "\r\n";
                                        iNewline = 0;
                                    }
                                }

                                tmpMsgTxt = "\r\r\n *** MARKER NOISE Calibration Done *** " + "\n\r\r Press OK to Continue";
                                MessageBox.Show(dispResult + tmpMsgTxt, "*** Calibration Data PASS - " + tmpCalHeader + " ***", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                //Write to file if CalLimitStatus is True
                                swCalDataFile.WriteLine("");
                                swCalDataFile.WriteLine(tmpCalHeader + tempFreq);
                                swCalDataFile.WriteLine(tempCalResult);

                                #endregion
                            }

                            #endregion

                            #region RX Pathgain/loss Cal
                            switch (Measure_Channel)
                            {
                                case 1:
                                    EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                    break;
                                case 2:
                                    EqSA02.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                    break;
                                default:
                                    break;
                            }
                            DelayMs(1000);

                            do
                            {
                                //Initialize Variable
                                tempFreq = string.Empty;
                                tempCalResult = string.Empty;

                                //variable for display result
                                dispFreq = new string[FreqListCountRF];
                                dispCal = new string[FreqListCountRF];
                                dispResult = "";
                                iNewline = 0;
                                calStatus = false;
                                callimitStatus = true;

                                for (int iCount = 0; iCount < FreqListCountRF; iCount++)
                                {
                                    tempFreq += "," + FreqListRF[iCount];

                                    EqSG01.SetFreq(Convert.ToDouble(FreqListRF[iCount]));
                                    EqSG01.SetAmplitude((float)Convert.ToDouble(PowerLevel));
                                    EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                    DelayMs(200);

                                    switch (Measure_Channel)
                                    {
                                        case 1:
                                            EqSA01.MARKER_TURN_ON_NORMAL_POINT(markerNo, (float)Convert.ToDouble(FreqListRF[iCount]));
                                            DelayMs(myUtility.MXA_Setting.SweepT);
                                            status = EqSA01.OPERATION_COMPLETE();
                                            power = EqSA01.READ_MARKER(markerNo) - Convert.ToDouble(PowerLevel) + CalOffset;
                                            break;
                                        case 2:
                                            EqSA02.MARKER_TURN_ON_NORMAL_POINT(markerNo, (float)Convert.ToDouble(FreqListRF[iCount]));
                                            DelayMs(myUtility.MXA_Setting.SweepT);
                                            status = EqSA02.OPERATION_COMPLETE();
                                            power = EqSA02.READ_MARKER(markerNo) - Convert.ToDouble(PowerLevel) + CalOffset;
                                            break;
                                        default:
                                            break;
                                    }

                                    //compare individual result with cal spec limit & set cal status flag
                                    if ((power < CalLimitLow) || (power > CalLimitHigh))
                                    {
                                        callimitStatus = false;
                                    }

                                    tempCalResult += "," + Math.Round(power, 3);
                                }

                                //Display calibration result 
                                dispFreq = tempFreq.Split(',');
                                dispCal = tempCalResult.Split(',');
                                dispResult = "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "Freq,CalVal" + "   " + "\r\n";
                                iNewline = 0;

                                for (int iCount = 1; iCount < FreqListCountRF + 1; iCount++)
                                {
                                    dispResult += dispFreq[iCount] + "," + dispCal[iCount] + "   ";
                                    iNewline++;

                                    if (iNewline == 4)
                                    {
                                        dispResult += "\r\n";
                                        iNewline = 0;
                                    }
                                }

                                if (callimitStatus)
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data PASS *** " + "\n\r\r Press YES to Save and Continue, NO to Redo Calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "*** Calibration Data PASS ***", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (chkStatus == DialogResult.Yes)
                                    {
                                        calStatus = true;
                                    }
                                }
                                else
                                {
                                    tmpMsgTxt = "\r\r\n *** Calibration Data FAIL *** " + "\r\n Calibration Data Fail Spec -> USL: " + CalLimitHigh + " , LSL: " + CalLimitLow + "\n\r\r Press RETRY to redo Calibration , CANCEL to stop calibration";
                                    DialogResult chkStatus = MessageBox.Show(dispResult + tmpMsgTxt, "!!! Calibration Data FAIL !!!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

                                    if (chkStatus == DialogResult.Cancel)
                                    {
                                        calStatus = true;
                                    }
                                }

                            } while (!calStatus);

                            EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                            //Write to file if CalLimitStatus is True
                            if (callimitStatus)
                            {
                                swCalDataFile.WriteLine("");
                                swCalDataFile.WriteLine(Target_CalSegment + tempFreq);
                                swCalDataFile.WriteLine(tempCalResult);
                            }

                            #endregion

                            #endregion
                            break;

                        case "SKIP_CAL":
                            //do nothing , skip calibration process
                            break;
                    }

                    swCalDataFile.Close();
                }

            }

            if (MXA_DisplayEnable)
            {
                if (EqmtStatus.MXA01)
                {
                    EqSA01.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.OFF);
                }
                if (EqmtStatus.MXA02)
                {
                    EqSA02.Enable_Display(LibEqmtDriver.SA.N9020A_DISPLAY.OFF);
                }
            }

            MessageBox.Show("The PA calibration is finished.");
            //ATFCrossDomainWrapper.Cal_LoadCalData(LocalSetting.CalTag, Convert.ToString(DicCalInfo[CalPathRF]));
        }

        private void NF_Calibration(int Iteration, string NF_CalTag, double[] Freq, double[] DutInputLoss, double[] DutOutputLoss, double NF_Cal_HL, double NF_Cal_LL, double NF_BW)
        {

            double[] nf_Freq;
            double[] nf_Analyzer;
            double[] nf_ColdSourcePower;
            double maxVal, minVal;
            int maxIndex, minIndex;
            string tmpMsgTxt = "";
            bool calStatus = false;

            StreamWriter resultFile = new StreamWriter(calDir + NF_CalTag + ".csv");
            StringBuilder resultBuilder;
            StringBuilder dispMsgBuilder;

            do
            {
                resultBuilder = new StringBuilder();
                dispMsgBuilder = new StringBuilder();

                EqRFmx.CalibratioSpeNFCouldSource(Iteration, NF_CalTag, Freq, DutInputLoss, DutOutputLoss, NF_BW);

                nf_Freq = EqRFmx.frequencyListOut;
                nf_Analyzer = EqRFmx.analyserNoiseFigure;
                nf_ColdSourcePower = EqRFmx.coldSourcePower;

                maxVal = nf_Analyzer.Max();
                minVal = nf_Analyzer.Min();

                maxIndex = Array.IndexOf(nf_Analyzer, maxVal);
                minIndex = Array.IndexOf(nf_Analyzer, minVal);

                dispMsgBuilder.AppendLine("Analyzer NF Cal Result");
                dispMsgBuilder.AppendLine("Max Value: " + Math.Round(maxVal, 3) + "dB at Freq " + nf_Freq[maxIndex] / 1e6 + "Mhz");
                dispMsgBuilder.AppendLine("Min Value: " + Math.Round(minVal, 3) + "dB at Freq " + nf_Freq[minIndex] / 1e6 + "Mhz");

                resultBuilder.AppendLine("Freq,AnalyzerNoiseFigure,ColdSourcePower");
                for (int i = 0; i < nf_Analyzer.Length; i++)
                {
                    resultBuilder.AppendLine(nf_Freq[i] + "," + nf_Analyzer[i] + "," + nf_ColdSourcePower[i]);
                }

                if ((minVal > NF_Cal_LL) && (maxVal < NF_Cal_HL))
                {
                    tmpMsgTxt = "\r\r\n *** Calibration Data PASS *** " + "\n\r\r Press YES to Save and Continue, NO to Redo Calibration";
                    DialogResult chkStatus = MessageBox.Show(dispMsgBuilder + tmpMsgTxt, "*** Calibration Data PASS ***", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (chkStatus == DialogResult.Yes)
                    {
                        calStatus = true;
                    }
                }

                else
                {
                    tmpMsgTxt = "\r\r\n *** Calibration Data FAIL *** " + "\r\n Calibration Data Fail Spec -> USL: " + NF_Cal_HL + " , LSL: " + NF_Cal_LL + "\n\r\r Press RETRY to redo Calibration , CANCEL to stop calibration";
                    DialogResult chkStatus = MessageBox.Show(dispMsgBuilder + tmpMsgTxt, "!!! Calibration Data FAIL !!!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

                    if (chkStatus == DialogResult.Cancel)
                    {
                        calStatus = true;
                    }
                }

            } while (!calStatus);

            resultFile.Write(resultBuilder);
            resultFile.Close();

        }
        private void ReadPaTCF()
        {
            //myUtility.ReadTCF(ConstPASheetNo, ConstPAIndexColumnNo, ConstPATestParaColumnNo, ref DicTestPA);
            //myUtility.ReadTCF(TCF_Sheet.ConstPASheetNo, TCF_Sheet.ConstPAIndexColumnNo, TCF_Sheet.ConstPATestParaColumnNo, ref DicTestPA, ref DicTestLabel);
            myUtility.ReadTCF(TCF_Sheet.ConstPASheetName, TCF_Sheet.ConstPAIndexColumnNo, TCF_Sheet.ConstPATestParaColumnNo, ref DicTestPA, ref DicTestLabel);
        }
        private void ReadCalTCF()
        {
            //myUtility.ReadCalSheet(TCF_Sheet.ConstCalSheetNo, TCF_Sheet.ConstCalIndexColumnNo, TCF_Sheet.ConstCalParaColumnNo, ref DicCalInfo);
            myUtility.ReadCalSheet(TCF_Sheet.ConstCalSheetName, TCF_Sheet.ConstCalIndexColumnNo, TCF_Sheet.ConstCalParaColumnNo, ref DicCalInfo);
        }
        private void ReadWafeForm()
        {
            //myUtility.ReadWaveformFilePath(TCF_Sheet.ConstKeyWordSheetNo, TCF_Sheet.ConstWaveFormColumnNo, ref DicWaveForm);  //remark and replace by additional dic for mutateWaveform (Shaz - 12/05/2016)
            //myUtility.ReadWaveformFilePath(TCF_Sheet.ConstKeyWordSheetNo, TCF_Sheet.ConstWaveFormColumnNo, ref DicWaveForm, ref DicWaveFormMutate);
            myUtility.ReadWaveformFilePath(TCF_Sheet.ConstKeyWordSheetName, TCF_Sheet.ConstWaveFormColumnNo, ref DicWaveForm, ref DicWaveFormMutate);
        }
        private void ReadMipiReg()
        {
            //myUtility.ReadMipiReg(TCF_Sheet.ConstMipiRegSheetNo, TCF_Sheet.ConstMipiKeyIndexColumnNo, TCF_Sheet.ConstMipiRegColumnNo, ref DicMipiKey);
            myUtility.ReadMipiReg(TCF_Sheet.ConstMipiRegSheetName, TCF_Sheet.ConstMipiKeyIndexColumnNo, TCF_Sheet.ConstMipiRegColumnNo, ref DicMipiKey);
        }
        private void ReadPwrBlast()
        {
            //myUtility.ReadPwrBlast(TCF_Sheet.ConstPwrBlastSheetNo, TCF_Sheet.ConstPwrBlastIndexColumnNo, TCF_Sheet.ConstPwrBlastColumnNo, ref DicPwrBlast);
            myUtility.ReadPwrBlast(TCF_Sheet.ConstPwrBlastSheetName, TCF_Sheet.ConstPwrBlastIndexColumnNo, TCF_Sheet.ConstPwrBlastColumnNo, ref DicPwrBlast);
        }

        public double CalcDelta(string testUsePrev, int rsltTag, bool abs)
        {
            double calcData = -999;
            double data_1 = -999;
            double data_2 = -999;
            string[] resultArray;
            resultArray = testUsePrev.Split(',');

            for (int j = 0; j < Results.Length; j++)
            {
                if (Convert.ToInt16(resultArray[0]) == Results[j].TestNumber)
                {
                    data_1 = Results[j].Multi_Results[rsltTag].Result_Data;
                }
                if (Convert.ToInt16(resultArray[1]) == Results[j].TestNumber)
                {
                    data_2 = Results[j].Multi_Results[rsltTag].Result_Data;
                }
            }

            if (abs)
            {
                calcData = Math.Abs(data_1 - data_2);
            }
            else
            {
                calcData = data_1 - data_2;
            }

            return calcData;
        }
        public double CalcSum(string testUsePrev, int rsltTag)
        {
            double calcData = -999;
            double data_1 = -999;
            string[] resultArray;
            resultArray = testUsePrev.Split(',');

            for (int i = 0; i < resultArray.Length; i++)
            {
                for (int j = 0; j < Results.Length; j++)
                {
                    if (Convert.ToInt16(resultArray[i]) == Results[j].TestNumber)
                    {
                        if (i == 0)     //set start-up data before calculating
                        {
                            calcData = Results[j].Multi_Results[rsltTag].Result_Data;
                        }
                        else
                        {
                            data_1 = Results[j].Multi_Results[rsltTag].Result_Data;
                            calcData = calcData + data_1;       //sum data
                        }
                    }
                }
            }

            return calcData;
        }
        public double CalcAverage(string testUsePrev, int rsltTag)
        {
            double calcData = -999;
            double data_1 = -999;
            string[] resultArray;
            resultArray = testUsePrev.Split(',');

            for (int i = 0; i < resultArray.Length; i++)
            {
                for (int j = 0; j < Results.Length; j++)
                {
                    if (Convert.ToInt16(resultArray[i]) == Results[j].TestNumber)
                    {
                        if (i == 0)     //set start-up data before calculating
                        {
                            calcData = Results[j].Multi_Results[rsltTag].Result_Data;
                        }
                        else
                        {
                            data_1 = Results[j].Multi_Results[rsltTag].Result_Data;
                            calcData = calcData + data_1;       //sum data
                        }
                    }
                }
            }

            calcData = calcData / resultArray.Length;       //calculate average

            return calcData;
        }
        public double ReportRslt(string testUsePrev, int rsltTag)
        {
            //Note : testUsePrev must only be round number , no other character allowed
            double calcData = -999;
            for (int j = 0; j < Results.Length; j++)
            {
                if (Convert.ToInt16(testUsePrev) == Results[j].TestNumber)
                {
                    calcData = Results[j].Multi_Results[rsltTag].Result_Data;
                }
            }
            return calcData;
        }

        public void SearchMAXMIN(string testParam, string testUsePrev, string searchMethod, int rsltTag, out double calcData, out int rsltArrayNo)
        {
            calcData = -999;
            rsltArrayNo = 0;
            string[] resultArray;
            resultArray = testUsePrev.Split(',');

            switch (searchMethod.ToUpper())
            {
                case "MAX":
                    for (int i = 0; i < resultArray.Length; i++)
                    {
                        for (int j = 0; j < Results.Length; j++)
                        {
                            if (Convert.ToInt16(resultArray[i]) == Results[j].TestNumber)
                            {
                                if (i == 0)     //set start-up data before calculating
                                {
                                    calcData = Results[j].Multi_Results[rsltTag].Result_Data;
                                    rsltArrayNo = j;    //pass out the arryno - to be use by NF MAX or MIN search
                                }
                                if (calcData < Results[j].Multi_Results[rsltTag].Result_Data)
                                {
                                    calcData = Results[j].Multi_Results[rsltTag].Result_Data;
                                    rsltArrayNo = j;    //pass out the arryno - to be use by NF MAX or MIN search
                                }
                                break;      //get out of j loop 
                            }
                        }
                    }
                    break;

                case "MIN":
                    for (int i = 0; i < resultArray.Length; i++)
                    {
                        for (int j = 0; j < Results.Length; j++)
                        {
                            if (Convert.ToInt16(resultArray[i]) == Results[j].TestNumber)
                            {
                                if (i == 0)     //set start-up data before calculating
                                {
                                    calcData = Results[j].Multi_Results[rsltTag].Result_Data;
                                    rsltArrayNo = j;    //pass out the arryno - to be use by NF MAX or MIN search
                                }
                                if (calcData > Results[j].Multi_Results[rsltTag].Result_Data)
                                {
                                    calcData = Results[j].Multi_Results[rsltTag].Result_Data;
                                    rsltArrayNo = j;    //pass out the arryno - to be use by NF MAX or MIN search
                                }
                                break;      //get out of j loop
                            }
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Test Parameter : " + testParam + "(" + searchMethod + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    calcData = -999;
                    rsltArrayNo = 0;
                    break;
            }
        }

        public void Capture_MXA1_Trace(int traceNo, int testNum, string testParam, string rxBand, bool saveData)
        {
            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            string resultHeader = testParam + "_RX" + rxBand + "_FREQ";

            //Read MXA Trace and store to temp file
            double startFreqHz = EqSA01.READ_STARTFREQ();
            double stopFreqHz = EqSA01.READ_STOPFREQ();
            int sweepPts = Convert.ToInt16(EqSA01.READ_SWEEP_POINTS());
            stepFreqHz = (stopFreqHz - startFreqHz) / (sweepPts - 1);

            //temp trace array storage use for MAX , MIN etc calculation 
            MXATrace[TestCount].Enable = true;
            MXATrace[TestCount].TestNumber = testNum;
            MXATrace[TestCount].Multi_Trace[0][0].NoPoints = sweepPts;
            MXATrace[TestCount].Multi_Trace[0][0].FreqMHz = new double[sweepPts];
            MXATrace[TestCount].Multi_Trace[0][0].Ampl = new double[sweepPts];
            MXATrace[TestCount].Multi_Trace[0][0].Result_Header = resultHeader;
            MXATrace[TestCount].Multi_Trace[0][0].MXA_No = "MXA1_Trace" + traceNo;

            float[] arrSaTraceData = new float[sweepPts];
            arrSaTraceData = EqSA01.IEEEBlock_READ_MXATrace(traceNo);

            tmpFreqHz = startFreqHz;          //initialize 1st data to startFreq

            for (istep = 0; istep < sweepPts; istep++)
            {
                if (istep > 0)
                {
                    tmpFreqHz = tmpFreqHz + stepFreqHz;
                }

                MXATrace[TestCount].Multi_Trace[0][0].FreqMHz[istep] = tmpFreqHz / 1e6;     //convert to MHz
                MXATrace[TestCount].Multi_Trace[0][0].Ampl[istep] = Math.Round(Convert.ToDouble(arrSaTraceData[istep]), 3);
            }

            if (saveData)
            {
                //Save all data to datalog 
                string[] templine = new string[2];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#MXA1 SWEEP DATALOG");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                templine[0] = "#MXA_FREQ (MHz)";
                templine[1] = "AMPLITUDE (dBm)";
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range
                for (istep = 0; istep < sweepPts; istep++)
                {
                    //Sorted the calibration result to array
                    templine[0] = Convert.ToString(MXATrace[TestCount].Multi_Trace[0][0].FreqMHz[istep]);
                    templine[1] = Convert.ToString(Math.Round(MXATrace[TestCount].Multi_Trace[0][0].Ampl[istep], 3));
                    LocalTextList.Add(string.Join(",", templine));
                }

                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + testParam + "_MXA1_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
            }
        }
        public void Capture_MXA1_Trace(int traceNo, int testNum, string testParam, string rxBand, double prev_Rslt, bool saveData, double mkrNoiseOffset = 0)
        {
            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            string resultHeader = testParam + "_RX" + rxBand + "_FREQ";

            //Read MXA Trace and store to temp file
            double startFreqHz = EqSA01.READ_STARTFREQ();
            double stopFreqHz = EqSA01.READ_STOPFREQ();
            int sweepPts = Convert.ToInt16(EqSA01.READ_SWEEP_POINTS());
            stepFreqHz = (stopFreqHz - startFreqHz) / (sweepPts - 1);

            //temp trace array storage use for MAX , MIN etc calculation 
            MXATrace[TestCount].Enable = true;
            MXATrace[TestCount].TestNumber = testNum;
            MXATrace[TestCount].Multi_Trace[0][0].NoPoints = sweepPts;
            MXATrace[TestCount].Multi_Trace[0][0].FreqMHz = new double[sweepPts];
            MXATrace[TestCount].Multi_Trace[0][0].Ampl = new double[sweepPts];
            MXATrace[TestCount].Multi_Trace[0][0].Result_Header = resultHeader;
            MXATrace[TestCount].Multi_Trace[0][0].MXA_No = "MXA1_Trace" + traceNo;

            float[] arrSaTraceData = new float[sweepPts];
            arrSaTraceData = EqSA01.IEEEBlock_READ_MXATrace(traceNo);

            tmpFreqHz = startFreqHz;          //initialize 1st data to startFreq

            for (istep = 0; istep < sweepPts; istep++)
            {
                if (istep > 0)
                {
                    tmpFreqHz = tmpFreqHz + stepFreqHz;
                }

                MXATrace[TestCount].Multi_Trace[0][0].FreqMHz[istep] = tmpFreqHz / 1e6;     //convert to MHz
                MXATrace[TestCount].Multi_Trace[0][0].Ampl[istep] = Math.Round(Convert.ToDouble(arrSaTraceData[istep]) - prev_Rslt - mkrNoiseOffset, 3);     //prev_Rslt - usually data from DUT with internal LNA gain, other should be 0
            }

            if (saveData)
            {
                //Save all data to datalog 
                string[] templine = new string[2];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#MXA1 SWEEP DATALOG");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                templine[0] = "#MXA_FREQ (MHz)";
                templine[1] = "AMPLITUDE (dBm)";
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range
                for (istep = 0; istep < sweepPts; istep++)
                {
                    //Sorted the calibration result to array
                    templine[0] = Convert.ToString(MXATrace[TestCount].Multi_Trace[0][0].FreqMHz[istep]);
                    templine[1] = Convert.ToString(Math.Round(MXATrace[TestCount].Multi_Trace[0][0].Ampl[istep], 3));       //raw data only from MXA without any prev_Rslt(usually data from DUT with internal LNA gain) embedded
                    LocalTextList.Add(string.Join(",", templine));
                }

                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + testParam + "_MXA1_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
            }
        }
        public void Read_MXA1_Trace(int traceNum, int testNum, out double freqMHz, out double ampl, string searchMethod, string testParam)
        {
            freqMHz = -999;
            ampl = -999;
            int noPoints = 0;
            int traceNo = 0;            //MXA1 array location

            switch (searchMethod.ToUpper())
            {
                case "MAX":
                    for (int i = 0; i < MXATrace.Length; i++)
                    {
                        if (MXATrace[i].TestNumber == testNum)
                        {
                            noPoints = MXATrace[i].Multi_Trace[0][traceNo].NoPoints;

                            for (int j = 0; j < noPoints; j++)
                            {
                                if (j == 0)
                                {
                                    ampl = MXATrace[i].Multi_Trace[0][traceNo].Ampl[0];
                                    freqMHz = MXATrace[i].Multi_Trace[0][traceNo].FreqMHz[0];
                                }
                                if (ampl < MXATrace[i].Multi_Trace[0][traceNo].Ampl[j])
                                {
                                    ampl = MXATrace[i].Multi_Trace[0][traceNo].Ampl[j];
                                    freqMHz = MXATrace[i].Multi_Trace[0][traceNo].FreqMHz[j];
                                }
                            }
                        }
                    }
                    break;

                case "MIN":
                    for (int i = 0; i < MXATrace.Length; i++)
                    {
                        if (MXATrace[i].TestNumber == testNum)
                        {
                            for (int j = 0; j < MXATrace[i].Multi_Trace[0][traceNo].NoPoints; j++)
                            {
                                if (j == 0)
                                {
                                    ampl = MXATrace[i].Multi_Trace[0][traceNo].Ampl[0];
                                    freqMHz = MXATrace[i].Multi_Trace[0][traceNo].FreqMHz[0];
                                }
                                if (ampl > MXATrace[i].Multi_Trace[0][traceNo].Ampl[j])
                                {
                                    ampl = MXATrace[i].Multi_Trace[0][traceNo].Ampl[j];
                                    freqMHz = MXATrace[i].Multi_Trace[0][traceNo].FreqMHz[j];
                                }
                            }
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Test Parameter : " + testParam + "(" + searchMethod + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    ampl = -999;
                    freqMHz = -999;
                    break;
            }
        }
        public void Capture_MXA2_Trace(int traceNo, int testNum, string testParam, string rxBand, bool saveData)
        {
            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            string resultHeader = testParam + "_RX" + rxBand + "_FREQ";

            //Read MXA Trace and store to temp file
            double startFreqHz = EqSA02.READ_STARTFREQ();
            double stopFreqHz = EqSA02.READ_STOPFREQ();
            int sweepPts = Convert.ToInt16(EqSA02.READ_SWEEP_POINTS());
            stepFreqHz = (stopFreqHz - startFreqHz) / (sweepPts - 1);

            //temp trace array storage use for MAX , MIN etc calculation 
            MXATrace[TestCount].Enable = true;
            MXATrace[TestCount].TestNumber = testNum;
            MXATrace[TestCount].Multi_Trace[0][1].NoPoints = sweepPts;
            MXATrace[TestCount].Multi_Trace[0][1].FreqMHz = new double[sweepPts];
            MXATrace[TestCount].Multi_Trace[0][1].Ampl = new double[sweepPts];
            MXATrace[TestCount].Multi_Trace[0][1].Result_Header = resultHeader;
            MXATrace[TestCount].Multi_Trace[0][1].MXA_No = "MXA2_Trace" + traceNo;

            float[] arrSaTraceData = new float[sweepPts];
            arrSaTraceData = EqSA02.IEEEBlock_READ_MXATrace(traceNo);

            tmpFreqHz = startFreqHz;          //initialize 1st data to startFreq

            for (istep = 0; istep < sweepPts; istep++)
            {
                if (istep > 0)
                {
                    tmpFreqHz = tmpFreqHz + stepFreqHz;
                }

                MXATrace[TestCount].Multi_Trace[0][1].FreqMHz[istep] = tmpFreqHz / 1e6;     //convert to MHz
                MXATrace[TestCount].Multi_Trace[0][1].Ampl[istep] = Math.Round(Convert.ToDouble(arrSaTraceData[istep]), 3);

            }

            if (saveData)
            {
                //Save all data to datalog 
                string[] templine = new string[2];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#MXA2 SWEEP DATALOG");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                templine[0] = "#MXA_FREQ (MHz)";
                templine[1] = "AMPLITUDE (dBm)";
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range
                for (istep = 0; istep < sweepPts; istep++)
                {
                    //Sorted the calibration result to array
                    templine[0] = Convert.ToString(MXATrace[TestCount].Multi_Trace[0][1].FreqMHz[istep]);
                    templine[1] = Convert.ToString(Math.Round(MXATrace[TestCount].Multi_Trace[0][1].Ampl[istep], 3));
                    LocalTextList.Add(string.Join(",", templine));
                }

                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + testParam + "_MXA2_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
            }
        }
        public void Capture_MXA2_Trace(int traceNo, int testNum, string testParam, string rxBand, double prev_Rslt, bool saveData, double mkrNoiseOffset = 0)
        {
            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            string resultHeader = testParam + "_RX" + rxBand + "_FREQ";

            //Read MXA Trace and store to temp file
            double startFreqHz = EqSA02.READ_STARTFREQ();
            double stopFreqHz = EqSA02.READ_STOPFREQ();
            int sweepPts = Convert.ToInt16(EqSA02.READ_SWEEP_POINTS());
            stepFreqHz = (stopFreqHz - startFreqHz) / (sweepPts - 1);

            //temp trace array storage use for MAX , MIN etc calculation 
            MXATrace[TestCount].Enable = true;
            MXATrace[TestCount].TestNumber = testNum;
            MXATrace[TestCount].Multi_Trace[0][1].NoPoints = sweepPts;
            MXATrace[TestCount].Multi_Trace[0][1].FreqMHz = new double[sweepPts];
            MXATrace[TestCount].Multi_Trace[0][1].Ampl = new double[sweepPts];
            MXATrace[TestCount].Multi_Trace[0][1].Result_Header = resultHeader;
            MXATrace[TestCount].Multi_Trace[0][1].MXA_No = "MXA2_Trace" + traceNo;

            float[] arrSaTraceData = new float[sweepPts];
            arrSaTraceData = EqSA02.IEEEBlock_READ_MXATrace(traceNo);

            tmpFreqHz = startFreqHz;          //initialize 1st data to startFreq

            for (istep = 0; istep < sweepPts; istep++)
            {
                if (istep > 0)
                {
                    tmpFreqHz = tmpFreqHz + stepFreqHz;
                }

                MXATrace[TestCount].Multi_Trace[0][1].FreqMHz[istep] = tmpFreqHz / 1e6;     //convert to MHz
                MXATrace[TestCount].Multi_Trace[0][1].Ampl[istep] = Math.Round(Convert.ToDouble(arrSaTraceData[istep]) - prev_Rslt - mkrNoiseOffset, 3);     //prev_Rslt - usually data from DUT with internal LNA gain, other should be 0
            }

            if (saveData)
            {
                //Save all data to datalog 
                string[] templine = new string[2];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#MXA2 SWEEP DATALOG");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                templine[0] = "#MXA_FREQ (MHz)";
                templine[1] = "AMPLITUDE (dBm)";
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range
                for (istep = 0; istep < sweepPts; istep++)
                {
                    //Sorted the calibration result to array
                    templine[0] = Convert.ToString(MXATrace[TestCount].Multi_Trace[0][1].FreqMHz[istep]);
                    templine[1] = Convert.ToString(Math.Round(MXATrace[TestCount].Multi_Trace[0][1].Ampl[istep], 3));       //raw data only from MXA without any prev_Rslt(usually data from DUT with internal LNA gain) embedded
                    LocalTextList.Add(string.Join(",", templine));
                }

                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + testParam + "_MXA2_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
            }
        }
        public void Read_MXA2_Trace(int traceNum, int testNum, out double freqMHz, out double ampl, string searchMethod, string testParam)
        {
            freqMHz = -999;
            ampl = -999;
            int noPoints = 0;
            int traceNo = 1;            //MXA2 array location

            switch (searchMethod.ToUpper())
            {
                case "MAX":
                    for (int i = 0; i < MXATrace.Length; i++)
                    {
                        if (MXATrace[i].TestNumber == testNum)
                        {
                            noPoints = MXATrace[i].Multi_Trace[0][traceNo].NoPoints;

                            for (int j = 0; j < noPoints; j++)
                            {
                                if (j == 0)
                                {
                                    ampl = MXATrace[i].Multi_Trace[0][traceNo].Ampl[0];
                                    freqMHz = MXATrace[i].Multi_Trace[0][traceNo].FreqMHz[0];
                                }
                                if (ampl < MXATrace[i].Multi_Trace[0][traceNo].Ampl[j])
                                {
                                    ampl = MXATrace[i].Multi_Trace[0][traceNo].Ampl[j];
                                    freqMHz = MXATrace[i].Multi_Trace[0][traceNo].FreqMHz[j];
                                }
                            }
                        }
                    }
                    break;

                case "MIN":
                    for (int i = 0; i < MXATrace.Length; i++)
                    {
                        if (MXATrace[i].TestNumber == testNum)
                        {
                            for (int j = 0; j < MXATrace[i].Multi_Trace[0][traceNo].NoPoints; j++)
                            {
                                if (j == 0)
                                {
                                    ampl = MXATrace[i].Multi_Trace[0][traceNo].Ampl[0];
                                    freqMHz = MXATrace[i].Multi_Trace[0][traceNo].FreqMHz[0];
                                }
                                if (ampl > MXATrace[i].Multi_Trace[0][traceNo].Ampl[j])
                                {
                                    ampl = MXATrace[i].Multi_Trace[0][traceNo].Ampl[j];
                                    freqMHz = MXATrace[i].Multi_Trace[0][traceNo].FreqMHz[j];
                                }
                            }
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Test Parameter : " + testParam + "(" + searchMethod + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    ampl = -999;
                    freqMHz = -999;
                    break;
            }
        }

        public void Read_MXA_MultiTrace(int MXA_No, int traceNum, string testUsePrev, double startFreqMHz, double stopFreqMHz, double stepFreqMHz, string searchMethod, string testParam, out double calcDataFreq, out double calcData)
        {
            int noPtsUser = 0;
            int[] testNumber;
            string[] resultArray;

            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            noPtsUser = (int)((stopFreqMHz - startFreqMHz) / stepFreqMHz) + 1;
            resultArray = testUsePrev.Split(',');
            s_TraceNo[] sortedMultiTrace = new s_TraceNo[resultArray.Length];
            testNumber = new int[resultArray.Length];

            #region Initialize Array and User Define Freq
            //initialize array & sorted the selected freq points
            for (int i = 0; i < resultArray.Length; i++)
            {
                sortedMultiTrace[i].Ampl = new double[noPtsUser];
                sortedMultiTrace[i].FreqMHz = new double[noPtsUser];

                testNumber[i] = Convert.ToInt16(resultArray[i]);        //example sort "use previous" - 3,4,6 -> array testNumber[i] where i(0) = 3 , i(1) = 4 , i(2) = 6

                tmpFreqHz = startFreqMHz * 1e6;          //initialize 1st data to startFreq
                stepFreqHz = stepFreqMHz * 1e6;
                for (istep = 0; istep < noPtsUser; istep++)
                {
                    if (istep > 0)
                    {
                        tmpFreqHz = tmpFreqHz + stepFreqHz;
                    }
                    sortedMultiTrace[i].FreqMHz[istep] = tmpFreqHz / 1e6;     //convert back to MHz
                    sortedMultiTrace[i].Ampl[istep] = -999999;              //initialize to default
                }
            }
            #endregion

            #region SORTED DATA POINT
            //sort the respective trace data to temp array location
            //really complex sorting , need to sort user define test freq (lower count) to actual of MXA trace test point (higher count) 
            //Example : 65 test freq and compared with 601 points of MXA trace (note:  both user define start & stop freq  must be in range of MXA trace start & stop freq)
            for (int i = 0; i < testNumber.Length; i++)     // "use previous" test number loop -> example sort "use previous" - 3,4,6 -> array testNumber[i] where i(0) = 3 , i(1) = 4 , i(2) = 6
            {
                for (int count = 0; count < MXATrace.Length; count++)      //all MXA trace search loop
                {
                    if (MXATrace[count].TestNumber == testNumber[i])        //select correct trace base on "use previous" test number
                    {
                        if (MXA_No == 1)    //select the correct trace either MXA#1 or MXA#2
                        {
                            for (istep = 0; istep < noPtsUser; istep++)     //sorted user define freq point (lower count) against MXA trace no points (higher count)
                            {
                                for (int j = 0; j < MXATrace[count].Multi_Trace[0][0].NoPoints; j++)
                                {
                                    if (sortedMultiTrace[i].FreqMHz[istep] == MXATrace[count].Multi_Trace[0][0].FreqMHz[j])        //find same freq and store amplitude to temp array
                                    {
                                        sortedMultiTrace[i].Ampl[istep] = MXATrace[count].Multi_Trace[0][0].Ampl[j];
                                    }
                                }
                            }
                        }
                        if (MXA_No == 2)    //select the correct trace either MXA#1 or MXA#2
                        {
                            for (istep = 0; istep < noPtsUser; istep++)     //sorted user define freq point (lower count) against MXA trace no points (higher count)
                            {
                                for (int j = 0; j < MXATrace[count].Multi_Trace[0][1].NoPoints; j++)
                                {
                                    if (sortedMultiTrace[i].FreqMHz[istep] == MXATrace[count].Multi_Trace[0][1].FreqMHz[j])        //find same freq and store amplitude to temp array
                                    {
                                        sortedMultiTrace[i].Ampl[istep] = MXATrace[count].Multi_Trace[0][1].Ampl[j];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Calculate Result
            //Calculate the result from the sorted data
            Result_MXATrace = new s_TraceNo();
            Result_MXATrace.Ampl = new double[noPtsUser];
            Result_MXATrace.FreqMHz = new double[noPtsUser];

            s_TraceNo resultMultiTrace = new s_TraceNo();
            resultMultiTrace.Ampl = new double[noPtsUser];
            resultMultiTrace.FreqMHz = new double[noPtsUser];

            calcData = -999;
            calcDataFreq = -999;
            Result_MXATrace.MXA_No = Convert.ToString(MXA_No);
            Result_MXATrace.NoPoints = noPtsUser;
            Result_MXATrace.Result_Header = testParam;

            switch (searchMethod.ToUpper())
            {
                case "MAX":
                    for (istep = 0; istep < noPtsUser; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                    {
                        for (int i = 0; i < sortedMultiTrace.Length; i++)
                        {
                            if (i == 0)
                            {
                                calcData = sortedMultiTrace[0].Ampl[istep];
                                resultMultiTrace.Ampl[istep] = sortedMultiTrace[i].Ampl[istep];
                                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[i].FreqMHz[istep];
                            }
                            if (calcData < sortedMultiTrace[i].Ampl[istep])
                            {
                                resultMultiTrace.Ampl[istep] = sortedMultiTrace[i].Ampl[istep];
                                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[i].FreqMHz[istep];
                                calcData = sortedMultiTrace[i].Ampl[istep];
                            }
                        }
                    }
                    for (istep = 0; istep < noPtsUser; istep++) //get MAX from the MAX of the multitrace
                    {
                        if (istep == 0)
                        {
                            calcData = resultMultiTrace.Ampl[istep];
                            calcDataFreq = resultMultiTrace.FreqMHz[istep];
                        }
                        if (calcData < resultMultiTrace.Ampl[istep])
                        {
                            calcData = resultMultiTrace.Ampl[istep];
                            calcDataFreq = resultMultiTrace.FreqMHz[istep];
                        }
                    }
                    break;

                case "MIN":
                    for (istep = 0; istep < noPtsUser; istep++)     //get MIN data for every noPtsUser out of multitrace (from "use previous" setting)
                    {
                        for (int i = 0; i < sortedMultiTrace.Length; i++)
                        {
                            if (i == 0)
                            {
                                calcData = sortedMultiTrace[0].Ampl[istep];
                                resultMultiTrace.Ampl[istep] = sortedMultiTrace[i].Ampl[istep];
                                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[i].FreqMHz[istep];
                            }
                            if (calcData > sortedMultiTrace[i].Ampl[istep])
                            {
                                resultMultiTrace.Ampl[istep] = sortedMultiTrace[i].Ampl[istep];
                                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[i].FreqMHz[istep];
                                calcData = sortedMultiTrace[i].Ampl[istep];
                            }
                        }
                    }
                    for (istep = 0; istep < noPtsUser; istep++) //get MIN from the MIN of the multitrace
                    {
                        if (istep == 0)
                        {
                            calcData = resultMultiTrace.Ampl[istep];
                            calcDataFreq = resultMultiTrace.FreqMHz[istep];
                        }
                        if (calcData > resultMultiTrace.Ampl[istep])
                        {
                            calcData = resultMultiTrace.Ampl[istep];
                            calcDataFreq = resultMultiTrace.FreqMHz[istep];
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Test Parameter : " + testParam + "(" + searchMethod + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    break;
            }
            #endregion

            Result_MXATrace = resultMultiTrace;
        }

        public void Save_MXA1Trace(int traceNo, string TestParaName, bool saveData)
        {
            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            if (saveData)
            {
                //Read MXA Trace and store to temp file
                double startFreqHz = EqSA01.READ_STARTFREQ();
                double stopFreqHz = EqSA01.READ_STOPFREQ();
                int sweepPts = Convert.ToInt16(EqSA01.READ_SWEEP_POINTS());
                stepFreqHz = (stopFreqHz - startFreqHz) / (sweepPts - 1);

                double[] freqArray = new double[sweepPts];
                double[] amplitudeArray = new double[sweepPts];
                string[] sort_trace = new string[sweepPts];

                string tmpMxadata = EqSA01.READ_MXATrace(traceNo);
                sort_trace = tmpMxadata.Split(',');

                tmpFreqHz = startFreqHz;          //initialize 1st data to startFreq

                for (istep = 0; istep < sweepPts; istep++)
                {
                    if (istep > 0)
                    {
                        tmpFreqHz = tmpFreqHz + stepFreqHz;
                    }

                    freqArray[istep] = tmpFreqHz / 1e6;       //convert to MHz
                    amplitudeArray[istep] = Convert.ToDouble(sort_trace[istep]);
                }

                //Save all data to datalog 
                string[] templine = new string[2];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#MXA1 SWEEP DATALOG");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                templine[0] = "#MXA_FREQ (MHz)";
                templine[1] = "AMPLITUDE (dBm)";
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range
                for (istep = 0; istep < sweepPts; istep++)
                {
                    //Sorted the calibration result to array
                    templine[0] = Convert.ToString(freqArray[istep]);
                    templine[1] = Convert.ToString(Math.Round(amplitudeArray[istep], 3));
                    LocalTextList.Add(string.Join(",", templine));
                }

                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + TestParaName + "_MXA1_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
            }
        }
        public void Save_MXA2Trace(int traceNo, string TestParaName, bool saveData)
        {
            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            if (saveData)
            {
                //Read MXA Trace and store to temp file
                double startFreqHz = EqSA02.READ_STARTFREQ();
                double stopFreqHz = EqSA02.READ_STOPFREQ();
                int sweepPts = Convert.ToInt16(EqSA02.READ_SWEEP_POINTS());
                stepFreqHz = (stopFreqHz - startFreqHz) / (sweepPts - 1);

                double[] freqArray = new double[sweepPts];
                double[] amplitudeArray = new double[sweepPts];
                string[] sort_trace = new string[sweepPts];

                string tmpMxadata = EqSA02.READ_MXATrace(traceNo);
                sort_trace = tmpMxadata.Split(',');

                tmpFreqHz = startFreqHz;          //initialize 1st data to startFreq

                for (istep = 0; istep < sweepPts; istep++)
                {
                    if (istep > 0)
                    {
                        tmpFreqHz = tmpFreqHz + stepFreqHz;
                    }

                    freqArray[istep] = tmpFreqHz / 1e6;       //convert to MHz
                    amplitudeArray[istep] = Convert.ToDouble(sort_trace[istep]);
                }

                //Save all data to datalog 
                string[] templine = new string[2];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#MXA2 SWEEP DATALOG");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                templine[0] = "#MXA_FREQ (MHz)";
                templine[1] = "AMPLITUDE (dBm)";
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range
                for (istep = 0; istep < sweepPts; istep++)
                {
                    //Sorted the calibration result to array
                    templine[0] = Convert.ToString(freqArray[istep]);
                    templine[1] = Convert.ToString(Math.Round(amplitudeArray[istep], 3));
                    LocalTextList.Add(string.Join(",", templine));
                }

                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + TestParaName + "_MXA2_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
            }
        }

        public void Save_PXI_Trace(string TestParaName, string testUsePrev, bool saveData, int rbw_counter, double rbw_Hz)
        {
            string[] resultArray;
            resultArray = testUsePrev.Split(',');

            int istep = 0;
            int sweepPts = 0;
            int traceNo;
            string rbw_paramName;

            if (saveData)
            {
                rbw_paramName = Math.Abs(rbw_Hz / 1e6).ToString();
                sweepPts = PXITrace[TestCount].Multi_Trace[rbw_counter][0].NoPoints;
                traceNo = PXITrace[TestCount].TraceCount;

                //Save all data to datalog 
                string[] templine = new string[traceNo + 1];
                string[] templine2 = new string[traceNo + 1];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#PXI SWEEP DATALOG");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                LocalTextList.Add("#RBW Hz : " + rbw_Hz.ToString());
                templine[0] = "#VSA_FREQ (MHz)";

                for (int n = 0; n < traceNo; n++)
                {
                    templine[n + 1] = "dBm_RUN_" + (n + 1);
                }
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range 
                for (istep = 0; istep < sweepPts; istep++)
                {
                    for (int n = 0; n < traceNo; n++)
                    {
                        if (n == 0)
                        {
                            templine2[n] = Convert.ToString(PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep]);
                        }
                        templine2[n + 1] = Convert.ToString(PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep]);
                    }
                    LocalTextList.Add(string.Join(",", templine2));
                }



                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + TestParaName + "_" + rbw_paramName + "MHz_PXI_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);

            }

        }
        public void Save_PXI_NF_TraceRaw(string TestParaName, int testUsePrev, bool saveData, int rbw_counter, double rbw_Hz)
        {

            int istep = 0;
            int sweepPts = 0;
            int traceNo;
            string rbw_paramName;

            if (saveData)
            {
                rbw_paramName = Math.Abs(rbw_Hz / 1e6).ToString();

                //NoPoints and TraceCount are similar between PXITrace and PXITRaceRaw .. Only define in PXITrace
                sweepPts = PXITrace[testUsePrev].Multi_Trace[rbw_counter][0].NoPoints;
                traceNo = PXITrace[testUsePrev].TraceCount;

                //Save all data to datalog 
                string[] templine = new string[traceNo + 1];
                string[] templine2 = new string[traceNo + 1];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#PXI SWEEP DATALOG - RAW Data");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                LocalTextList.Add("#Measured Bandwidth Hz : " + rbw_Hz.ToString());
                templine[0] = "#VSA_FREQ (MHz)";

                for (int n = 0; n < traceNo; n++)
                {
                    templine[n + 1] = "dB_RUN_" + (n + 1);
                }
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range 
                for (istep = 0; istep < sweepPts; istep++)
                {
                    for (int n = 0; n < traceNo; n++)
                    {
                        if (n == 0)
                        {
                            templine2[n] = Convert.ToString(PXITraceRaw[testUsePrev].Multi_Trace[rbw_counter][n].FreqMHz[istep]);
                        }
                        templine2[n + 1] = Convert.ToString(PXITraceRaw[testUsePrev].Multi_Trace[rbw_counter][n].Ampl[istep]);
                    }
                    LocalTextList.Add(string.Join(",", templine2));
                }

                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + TestParaName + "_" + rbw_paramName + "MHz_PXI_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);

            }

        }

        public void Save_PXI_TraceRaw(string TestParaName, string testUsePrev, bool saveData, int rbw_counter, double rbw_Hz)
        {
            string[] resultArray;
            resultArray = testUsePrev.Split(',');

            int istep = 0;
            int sweepPts = 0;
            int traceNo;
            string rbw_paramName;

            if (saveData)
            {
                rbw_paramName = Math.Abs(rbw_Hz / 1e6).ToString();

                //NoPoints and TraceCount are similar between PXITrace and PXITRaceRaw .. Only define in PXITrace
                sweepPts = PXITrace[TestCount].Multi_Trace[rbw_counter][0].NoPoints;
                traceNo = PXITrace[TestCount].TraceCount;

                //Save all data to datalog 
                string[] templine = new string[traceNo + 1];
                string[] templine2 = new string[traceNo + 1];
                ArrayList LocalTextList = new ArrayList();
                ArrayList tmpCalMsg = new ArrayList();

                //Calibration File Header
                LocalTextList.Add("#PXI SWEEP DATALOG - RAW Data");
                LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                LocalTextList.Add("#RBW Hz : " + rbw_Hz.ToString());
                templine[0] = "#VSA_FREQ (MHz)";

                for (int n = 0; n < traceNo; n++)
                {
                    templine[n + 1] = "dBm_RUN_" + (n + 1);
                }
                LocalTextList.Add(string.Join(",", templine));

                // Start looping until complete the freq range 
                for (istep = 0; istep < sweepPts; istep++)
                {
                    for (int n = 0; n < traceNo; n++)
                    {
                        if (n == 0)
                        {
                            templine2[n] = Convert.ToString(PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep]);
                        }
                        templine2[n + 1] = Convert.ToString(PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep]);
                    }
                    LocalTextList.Add(string.Join(",", templine2));
                }

                //Write cal data to csv file
                if (!Directory.Exists(SNPFile.FileOutput_Path))
                {
                    Directory.CreateDirectory(SNPFile.FileOutput_Path);
                }
                string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + TestParaName + "_" + rbw_paramName + "MHz_PXI_Unit" + tmpUnit_No.ToString() + ".csv";
                IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);

            }

        }

        public void Read_PXI_MultiTrace(string testUsePrev, double startFreqMHz, double stopFreqMHz, double stepFreqMHz, string searchMethod, string testParam, out double calcDataFreq, out double calcData, int rbw_counter, double rbw_Hz)
        {
            int noPtsUser = 0;
            int[] testNumber;
            int traceCount = 0;
            int startTraceNo = 0;
            int testUsePrev_ArrayNo = 0;

            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            bool excludeSoakSweep = true;

            noPtsUser = (int)((stopFreqMHz - startFreqMHz) / Math.Round(stepFreqMHz, 3)) + 1;

            //if excluded soak sweep trace , need to remove the array[0] from PXITrace[testnumber].Multi_Trace[0]
            for (int i = 0; i < PXITrace.Length; i++)
            {
                if (Convert.ToInt16(testUsePrev) == PXITrace[i].TestNumber)
                {
                    testUsePrev_ArrayNo = i;
                    excludeSoakSweep = PXITrace[testUsePrev_ArrayNo].SoakSweep;
                }
            }

            if (excludeSoakSweep)
            {
                traceCount = PXITrace[testUsePrev_ArrayNo].TraceCount - 1;
                startTraceNo = 1;
            }
            else
            {
                traceCount = PXITrace[testUsePrev_ArrayNo].TraceCount;
                startTraceNo = 0;
            }

            #region Initialize Array and User Define Freq
            //initialize array & sorted the selected freq points
            s_TraceNo[] sortedMultiTrace = new s_TraceNo[traceCount];
            testNumber = new int[traceCount];

            for (int i = 0; i < traceCount; i++)
            {
                sortedMultiTrace[i].Ampl = new double[noPtsUser];
                sortedMultiTrace[i].FreqMHz = new double[noPtsUser];

                testNumber[i] = startTraceNo;           //example sort "use previous" - 3,4,6 -> array testNumber[i] where i(0) = 3 , i(1) = 4 , i(2) = 6

                tmpFreqHz = startFreqMHz * 1e6;         //initialize 1st data to startFreq
                stepFreqHz = stepFreqMHz * 1e6;
                for (istep = 0; istep < noPtsUser; istep++)
                {
                    if (istep > 0)
                    {
                        tmpFreqHz = tmpFreqHz + stepFreqHz;
                    }
                    sortedMultiTrace[i].FreqMHz[istep] = Math.Round((tmpFreqHz / 1e6), 3);    //convert back to MHz
                    sortedMultiTrace[i].Ampl[istep] = 99999;              //initialize to default
                }

                startTraceNo++;
            }
            #endregion

            #region SORTED DATA POINT
            //sort the respective trace data to temp array location
            //really complex sorting , need to sort user define test freq (lower count) to actual of MXA trace test point (higher count) 
            //Example : 65 test freq and compared with 601 points of MXA trace (note:  both user define start & stop freq  must be in range of MXA trace start & stop freq)
            for (int i = 0; i < testNumber.Length; i++)     // "use previous" test number loop -> example sort "use previous" - 3,4,6 -> array testNumber[i] where i(0) = 3 , i(1) = 4 , i(2) = 6
            {
                for (istep = 0; istep < noPtsUser; istep++)     //sorted user define freq point (lower count) against MXA trace no points (higher count)
                {
                    if (rbw_Hz == PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[i]].RBW_Hz)     //check stored RBW is same as pass in RBW , if different will not proceed
                    {
                        for (int j = 0; j < PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[i]].NoPoints; j++)
                        {
                            if (sortedMultiTrace[i].FreqMHz[istep] == PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[i]].FreqMHz[j])        //find same freq and store amplitude to temp array
                            {
                                sortedMultiTrace[i].Ampl[istep] = PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[i]].Ampl[j];
                            }
                        }
                    }
                }
            }
            #endregion

            #region Calculate Result
            //Calculate the result from the sorted data
            Result_MXATrace = new s_TraceNo();
            Result_MXATrace.Ampl = new double[noPtsUser];
            Result_MXATrace.FreqMHz = new double[noPtsUser];

            s_TraceNo resultMultiTrace = new s_TraceNo();
            resultMultiTrace.Ampl = new double[noPtsUser];
            resultMultiTrace.FreqMHz = new double[noPtsUser];

            calcData = 999;
            calcDataFreq = -999;
            Result_MXATrace.MXA_No = "PXI";
            Result_MXATrace.NoPoints = noPtsUser;
            Result_MXATrace.Result_Header = testParam;

            switch (searchMethod.ToUpper())
            {
                case "MAX":
                    for (istep = 0; istep < noPtsUser; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                    {
                        for (int i = 0; i < sortedMultiTrace.Length; i++)
                        {
                            if (i == 0)
                            {
                                calcData = sortedMultiTrace[0].Ampl[istep];
                                resultMultiTrace.Ampl[istep] = sortedMultiTrace[i].Ampl[istep];
                                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[i].FreqMHz[istep];
                            }
                            if (calcData < sortedMultiTrace[i].Ampl[istep])
                            {
                                resultMultiTrace.Ampl[istep] = sortedMultiTrace[i].Ampl[istep];
                                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[i].FreqMHz[istep];
                                calcData = sortedMultiTrace[i].Ampl[istep];
                            }
                        }
                    }
                    for (istep = 0; istep < noPtsUser; istep++) //get MAX from the MAX of the multitrace
                    {
                        if (istep == 0)
                        {
                            calcData = resultMultiTrace.Ampl[istep];
                            calcDataFreq = resultMultiTrace.FreqMHz[istep];
                        }
                        if (calcData < resultMultiTrace.Ampl[istep])
                        {
                            calcData = resultMultiTrace.Ampl[istep];
                            calcDataFreq = resultMultiTrace.FreqMHz[istep];
                        }
                    }
                    break;

                case "MIN":
                    for (istep = 0; istep < noPtsUser; istep++)     //get MIN data for every noPtsUser out of multitrace (from "use previous" setting)
                    {
                        for (int i = 0; i < sortedMultiTrace.Length; i++)
                        {
                            if (i == 0)
                            {
                                calcData = sortedMultiTrace[0].Ampl[istep];
                                resultMultiTrace.Ampl[istep] = sortedMultiTrace[i].Ampl[istep];
                                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[i].FreqMHz[istep];
                            }
                            if (calcData > sortedMultiTrace[i].Ampl[istep])
                            {
                                resultMultiTrace.Ampl[istep] = sortedMultiTrace[i].Ampl[istep];
                                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[i].FreqMHz[istep];
                                calcData = sortedMultiTrace[i].Ampl[istep];
                            }
                        }
                    }
                    for (istep = 0; istep < noPtsUser; istep++) //get MIN from the MIN of the multitrace
                    {
                        if (istep == 0)
                        {
                            calcData = resultMultiTrace.Ampl[istep];
                            calcDataFreq = resultMultiTrace.FreqMHz[istep];
                        }
                        if (calcData > resultMultiTrace.Ampl[istep])
                        {
                            calcData = resultMultiTrace.Ampl[istep];
                            calcDataFreq = resultMultiTrace.FreqMHz[istep];
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Test Parameter : " + testParam + "(" + searchMethod + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    break;
            }
            #endregion

            Result_MXATrace = resultMultiTrace;

        }
        public void Read_PXI_SingleTrace(string testUsePrev, int treaceNo, double startFreqMHz, double stopFreqMHz, double stepFreqMHz, string searchMethod, string testParam, int rbw_counter, double rbw_Hz)
        {
            int noPtsUser = 0;
            int[] testNumber;
            int traceCount = 0;
            int startTraceNo = 0;
            int testUsePrev_ArrayNo = 0;

            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            bool excludeSoakSweep = true;

            noPtsUser = (int)((stopFreqMHz - startFreqMHz) / Math.Round(stepFreqMHz, 3)) + 1;

            //if excluded soak sweep trace , need to remove the array[0] from PXITrace[testnumber].Multi_Trace[0]
            for (int i = 0; i < PXITrace.Length; i++)
            {
                if (Convert.ToInt16(testUsePrev) == PXITrace[i].TestNumber)
                {
                    testUsePrev_ArrayNo = i;
                    excludeSoakSweep = PXITrace[testUsePrev_ArrayNo].SoakSweep;
                    traceCount = PXITrace[testUsePrev_ArrayNo].TraceCount;
                }
            }

            #region Initialize Array and User Define Freq
            //initialize array & sorted the selected freq points
            s_TraceNo[] sortedMultiTrace = new s_TraceNo[traceCount];
            testNumber = new int[traceCount];

            for (int i = 0; i < traceCount; i++)
            {
                sortedMultiTrace[i].Ampl = new double[noPtsUser];
                sortedMultiTrace[i].FreqMHz = new double[noPtsUser];

                testNumber[i] = startTraceNo;           //example sort "use previous" - 3,4,6 -> array testNumber[i] where i(0) = 3 , i(1) = 4 , i(2) = 6

                tmpFreqHz = startFreqMHz * 1e6;         //initialize 1st data to startFreq
                stepFreqHz = stepFreqMHz * 1e6;
                for (istep = 0; istep < noPtsUser; istep++)
                {
                    if (istep > 0)
                    {
                        tmpFreqHz = tmpFreqHz + stepFreqHz;
                    }
                    sortedMultiTrace[i].FreqMHz[istep] = Math.Round((tmpFreqHz / 1e6), 3);    //convert back to MHz
                    sortedMultiTrace[i].Ampl[istep] = 99999;              //initialize to default
                }

                startTraceNo++;
            }
            #endregion

            #region SORTED DATA POINT
            //sort the respective trace data to temp array location
            //really complex sorting , need to sort user define test freq (lower count) to actual of MXA trace test point (higher count) 
            //Example : 65 test freq and compared with 601 points of MXA trace (note:  both user define start & stop freq  must be in range of MXA trace start & stop freq)
            for (istep = 0; istep < noPtsUser; istep++)     //sorted user define freq point (lower count) against MXA trace no points (higher count)
            {
                if (rbw_Hz == PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].RBW_Hz)     //check stored RBW is same as pass in RBW , if different will not proceed
                {
                    for (int j = 0; j < PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].NoPoints; j++)
                    {
                        if (sortedMultiTrace[treaceNo].FreqMHz[istep] == PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].FreqMHz[j])        //find same freq and store amplitude to temp array
                        {
                            sortedMultiTrace[treaceNo].Ampl[istep] = PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].Ampl[j];
                        }
                    }
                }
            }
            #endregion

            #region Calculate Result
            //Calculate the result from the sorted data
            Result_MXATrace = new s_TraceNo();
            Result_MXATrace.Ampl = new double[noPtsUser];
            Result_MXATrace.FreqMHz = new double[noPtsUser];

            s_TraceNo resultMultiTrace = new s_TraceNo();
            resultMultiTrace.Ampl = new double[noPtsUser];
            resultMultiTrace.FreqMHz = new double[noPtsUser];

            Result_MXATrace.MXA_No = "PXI";
            Result_MXATrace.NoPoints = noPtsUser;
            Result_MXATrace.Result_Header = testParam;

            for (istep = 0; istep < noPtsUser; istep++)
            {
                resultMultiTrace.Ampl[istep] = sortedMultiTrace[treaceNo].Ampl[istep];
                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[treaceNo].FreqMHz[istep];
            }
            #endregion

            Result_MXATrace = resultMultiTrace;
        }
        public void Read_PXI_SingleTrace_Interpolate(string testUsePrev, int treaceNo, double startFreqMHz, double stopFreqMHz, double stepFreqMHz, string searchMethod, string testParam, int rbw_counter, double rbw_Hz)
        {
            //routine to read the trace array and interpolate if SaveTrace data points not equal to SearchData
            int noPtsUser = 0;
            int[] testNumber;
            int traceCount = 0;
            int startTraceNo = 0;
            int testUsePrev_ArrayNo = 0;

            int istep;
            double tmpFreqHz = 0;
            double stepFreqHz = 0;

            bool excludeSoakSweep = true;

            noPtsUser = (int)((stopFreqMHz - startFreqMHz) / Math.Round(stepFreqMHz, 3)) + 1;

            //if excluded soak sweep trace , need to remove the array[0] from PXITrace[testnumber].Multi_Trace[0]
            for (int i = 0; i < PXITrace.Length; i++)
            {
                if (Convert.ToInt16(testUsePrev) == PXITrace[i].TestNumber)
                {
                    testUsePrev_ArrayNo = i;
                    excludeSoakSweep = PXITrace[testUsePrev_ArrayNo].SoakSweep;
                    traceCount = PXITrace[testUsePrev_ArrayNo].TraceCount;
                }
            }

            #region Initialize Array and User Define Freq
            //initialize array & sorted the selected freq points
            s_TraceNo[] sortedMultiTrace = new s_TraceNo[traceCount];
            testNumber = new int[traceCount];

            for (int i = 0; i < traceCount; i++)
            {
                sortedMultiTrace[i].Ampl = new double[noPtsUser];
                sortedMultiTrace[i].FreqMHz = new double[noPtsUser];

                testNumber[i] = startTraceNo;           //example sort "use previous" - 3,4,6 -> array testNumber[i] where i(0) = 3 , i(1) = 4 , i(2) = 6

                tmpFreqHz = startFreqMHz * 1e6;         //initialize 1st data to startFreq
                stepFreqHz = stepFreqMHz * 1e6;
                for (istep = 0; istep < noPtsUser; istep++)
                {
                    if (istep > 0)
                    {
                        tmpFreqHz = tmpFreqHz + stepFreqHz;
                    }
                    sortedMultiTrace[i].FreqMHz[istep] = Math.Round((tmpFreqHz / 1e6), 3);    //convert back to MHz
                    sortedMultiTrace[i].Ampl[istep] = 99999;              //initialize to default
                }

                startTraceNo++;
            }
            #endregion

            #region SORTED DATA POINT
            //sort the respective trace data to temp array location
            //really complex sorting , need to sort user define test freq (lower count) to actual of MXA trace test point (higher count) 
            //Example : 65 test freq and compared with 601 points of MXA trace (note:  both user define start & stop freq  must be in range of MXA trace start & stop freq)

            if (rbw_Hz == PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].RBW_Hz)     //check stored RBW is same as pass in RBW , if different will not proceed
            {
                if (PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].NoPoints < noPtsUser)
                {
                    //will do interpolate if MXA Trace point is smaller than user define test points
                    for (istep = 0; istep < noPtsUser; istep++)     //sorted user define freq point (lower count) against MXA trace no points (higher count)
                    {
                        for (int j = 0; j < PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].NoPoints; j++)
                        {
                            if (sortedMultiTrace[treaceNo].FreqMHz[istep] == PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].FreqMHz[j])        //find same freq and store amplitude to temp array
                            {
                                sortedMultiTrace[treaceNo].Ampl[istep] = PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].Ampl[j];
                                break;
                            }
                            else        //interpolation 
                            {
                                if (j < PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].NoPoints - 1)   //to ensure that no array overflow
                                {
                                    if ((sortedMultiTrace[treaceNo].FreqMHz[istep] > PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].FreqMHz[j]) && (sortedMultiTrace[treaceNo].FreqMHz[istep] < PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].FreqMHz[j + 1]))
                                    {
                                        double g = sortedMultiTrace[treaceNo].FreqMHz[istep];
                                        double g1 = PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].FreqMHz[j];
                                        double g2 = PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].FreqMHz[j + 1];

                                        double d1 = PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].Ampl[j];
                                        double d2 = PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].Ampl[j + 1];

                                        //linear interpolation formula
                                        double d = d1 + (((g - g1) / (g2 - g1)) * (d2 - d1));
                                        sortedMultiTrace[treaceNo].Ampl[istep] = Math.Round(d, 3);
                                        break;
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    for (istep = 0; istep < noPtsUser; istep++)     //sorted user define freq point (lower count) against MXA trace no points (higher count)
                    {
                        for (int j = 0; j < PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].NoPoints; j++)
                        {
                            if (sortedMultiTrace[treaceNo].FreqMHz[istep] == PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].FreqMHz[j])        //find same freq and store amplitude to temp array
                            {
                                sortedMultiTrace[treaceNo].Ampl[istep] = PXITrace[testUsePrev_ArrayNo].Multi_Trace[rbw_counter][testNumber[treaceNo]].Ampl[j];
                                break;
                            }
                        }
                    }
                }
            }

            #endregion

            #region Calculate Result
            //Calculate the result from the sorted data
            Result_MXATrace = new s_TraceNo();
            Result_MXATrace.Ampl = new double[noPtsUser];
            Result_MXATrace.FreqMHz = new double[noPtsUser];

            s_TraceNo resultMultiTrace = new s_TraceNo();
            resultMultiTrace.Ampl = new double[noPtsUser];
            resultMultiTrace.FreqMHz = new double[noPtsUser];

            Result_MXATrace.MXA_No = "PXI";
            Result_MXATrace.NoPoints = noPtsUser;
            Result_MXATrace.Result_Header = testParam;

            for (istep = 0; istep < noPtsUser; istep++)
            {
                resultMultiTrace.Ampl[istep] = sortedMultiTrace[treaceNo].Ampl[istep];
                resultMultiTrace.FreqMHz[istep] = sortedMultiTrace[treaceNo].FreqMHz[istep];
            }
            #endregion

            Result_MXATrace.Ampl = resultMultiTrace.Ampl;
            Result_MXATrace.FreqMHz = resultMultiTrace.FreqMHz;
        }

        // Delay routine to avoid using Thread.Sleep()
        public void DelayMs(int mSec)
        {
            LibEqmtDriver.Utility.HiPerfTimer timer = new LibEqmtDriver.Utility.HiPerfTimer();
            timer.wait(mSec);
        }
        public void DelayUs(int uSec)
        {
            LibEqmtDriver.Utility.HiPerfTimer timer = new LibEqmtDriver.Utility.HiPerfTimer();
            timer.wait_us(uSec);
        }

        #region Small Function for OTP & MIPI
        public void JediOTPBurn(string efuseCtlReg_hex, int[] efuseDataByteNum, string data_hex, string cusMipiPair, string cusSlaveAddr, bool invertData = false)
        {
            //EFuse Control Register Definition (0xC0) - efuseCtlReg_hex
            //Bit7[program] Bit6[NA] Bit5[a2] Bit4[a1] Bit3[a0] Bit2[b2] Bit1[b1] Bit0[b0] - burnDataDec
            //a[2:0]:	Address of six 8-bit eFuse cells - efuseDataByteNum[x]
            //b[2:0]:	Bit address of the 8-bit eFuse cells - data_hex

            int programMode = 1;

            try
            {
                int data_dec = Convert.ToInt32(data_hex, 16);

                if (data_dec > 255)
                {
                    MessageBox.Show("Error: Cannot burn decimal values greater than 255", "BurnOTP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // burn the data - Bit by Bit
                for (int bit = 0; bit < 8; bit++)
                {
                    int bitVal = (int)Math.Pow(2, bit);

                    if ((bitVal & data_dec) == (invertData ? 0 : bitVal))
                    {
                        int burnDataDec = (programMode << 7) + (efuseDataByteNum[2] << 5) + (efuseDataByteNum[1] << 4) + (efuseDataByteNum[0] << 3) + bit;

                        // Convert integer as a hex in a string variable
                        string hexValue = burnDataDec.ToString("X");
                        EqMiPiCtrl.WriteOTPRegister(efuseCtlReg_hex, hexValue, Convert.ToInt16(cusMipiPair), Convert.ToInt32((cusSlaveAddr), 16));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "OTP Burn Error");
            }
        }
        public void Sort_MSBnLSB(Int32 decData, out string ID_MSB, out string ID_LSB)
        {
            int OtpExpectedValue1 = 52;
            int OtpExpectedValue2 = 1;
            ID_MSB = "0x00";            //set to default
            ID_LSB = "0x01";            //set to default
            int math1, math1a, math2;

            if (true)
            {
                math1 = Convert.ToInt32(decData / 256);
                if ((math1 * 256) > decData)
                {
                    math1 = math1 - 1;
                }
                math1a = math1 * 256;
                math2 = decData - math1a;
                OtpExpectedValue2 = math1;
                OtpExpectedValue1 = math2;

                ID_MSB = decToHex(OtpExpectedValue2);
                ID_LSB = decToHex(OtpExpectedValue1);
            }
        }
        public string decToHex(int number)
        {
            int number1a, number1b, number1c;
            string hex1, hex2;
            string number2 = "0x00";

            number1a = Convert.ToInt32(number / 16);

            if ((number1a * 16) > number)
            {
                number1a = number1a - 1;
            }
            number1b = number1a * 16;
            number1c = number - number1b;

            hex1 = decToHex2(number1a);
            hex2 = decToHex2(number1c);

            number2 = "0x" + hex1 + hex2;

            return number2;
        }
        public string decToHex2(int number)
        {
            string hexa = "0";
            switch (number)
            {
                case 0:
                    hexa = "0";
                    break;
                case 1:
                    hexa = "1";
                    break;
                case 2:
                    hexa = "2";
                    break;
                case 3:
                    hexa = "3";
                    break;
                case 4:
                    hexa = "4";
                    break;
                case 5:
                    hexa = "5";
                    break;
                case 6:
                    hexa = "6";
                    break;
                case 7:
                    hexa = "7";
                    break;
                case 8:
                    hexa = "8";
                    break;
                case 9:
                    hexa = "9";
                    break;
                case 10:
                    hexa = "A";
                    break;
                case 11:
                    hexa = "B";
                    break;
                case 12:
                    hexa = "C";
                    break;
                case 13:
                    hexa = "D";
                    break;
                case 14:
                    hexa = "E";
                    break;
                case 15:
                    hexa = "F";
                    break;

                default:
                    throw new Exception("Can't convert number to Hex: " + number);
            }

            return hexa;

        }
        public void dutTempSensor(double data_dec, out double dutTempC)
        {
            //init to default 
            dutTempC = -999;
            double tempCalc = -999;
            double minTempC = -20;
            double maxTempC = 130;

            //Note : Temp Sensor range from -20C to 130C
            //0x00 -> -20C , 0xFF -> 130C , so for 0x00 to 0xFF -> temperature range = 130C - (-20C) = 150C
            //temp change per point = 150C/255 = 0.588C
            //equation to convert data_dec from register to tempC
            double dutTempRangeC = maxTempC - minTempC;
            tempCalc = (data_dec * (dutTempRangeC / 255)) + minTempC;
            dutTempC = Math.Round(tempCalc, 3);
        }
        public void searchMIPIKey(string testParam, string searchKey, out string CusMipiRegMap, out string CusPMTrigMap, out string CusSlaveAddr, out string CusMipiPair, out string CusMipiSite, out bool b_mipiTKey)
        {
            //initialize variable - reset to default
            b_mipiTKey = false;
            CusMipiRegMap = null;
            CusPMTrigMap = null;
            CusSlaveAddr = null;
            CusMipiPair = null;
            CusMipiSite = null;

            //Data from Mipi custom spreadsheet 
            foreach (Dictionary<string, string> currMipiReg in DicMipiKey)
            {
                currMipiReg.TryGetValue("MIPI KEY", out DicMipiTKey);

                if (searchKey.ToUpper() == DicMipiTKey)
                {
                    currMipiReg.TryGetValue("REGMAP", out CusMipiRegMap);
                    currMipiReg.TryGetValue("TRIG", out CusPMTrigMap);
                    currMipiReg.TryGetValue("SLAVEADDR", out CusSlaveAddr);
                    currMipiReg.TryGetValue("MIPI_PAIR", out CusMipiPair);
                    currMipiReg.TryGetValue("MIPI_SITE", out CusMipiSite);
                    b_mipiTKey = true;          //change flag if match
                }
            }

            if (!b_mipiTKey)        //if cannot find , show error
                MessageBox.Show("Failed to find MIPI KEY (" + searchKey.ToUpper() + ") in MIPI sheet \n\n", testParam.ToUpper(), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void readout_OTPReg_viaEffectiveBit(int delay_mSec, string SwBand, string CusMipiRegMap, string CusPMTrigMap, string CusSlaveAddr, string CusMipiPair, string CusMipiSite, out int decDataOut, out string dataSizeHex)
        {
            biasDataArr = null;
            dataSizeHex = null;
            decDataOut = 0;

            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

            //Init variable & sorted effective bit data
            appendBinary = null;
            dataDec = new int[biasDataArr.Length];
            dataBinary = new string[biasDataArr.Length];

            //Note : effective bit are selected if any of the bit is set to '1' in CusMipiRegMap data column (in hex format >> register:effective bits) => 0x42:0x03
            //example CusMipiRegMap must be in '42:03 43:FF' where 0x43 is LSB reg address and 0xFF (1111 1111) all 8 bits are to be effectively read
            //0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read

            //example after MIPI read => reg 0x43 = data read 0xCB (11001011) &  reg 0x42 = data read 0x2E (00101110)
            //Effective bit decode example => for reg 0x42 since all 8bits will be use , effectiveBitData(0xCB) = 11001011
            //while reg 0x43 only bit0 and bit1 will be taken (*note: shown in bracket) , effectiveBitData(0x2E) = 001011 (10)
            //reported data => 10 11001011 => convert to dec = 715

            //Set MIPI and Read MIPI
            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
            DelayMs(delay_mSec);

            for (int i = 0; i < biasDataArr.Length; i++)
            {
                EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                dataDec[i] = tmpOutData;
                dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
            }

            //sorted out MIPI data base effective bit selection and publish result
            for (int i = 0; i < biasDataArr.Length; i++)
            {
                //sort out the effective bit - register address
                //Example : 0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read
                int tempReg_Dec = 0;
                string[] tempReg_Hex = new string[2];
                tempReg_Hex = biasDataArr[i].Split(':');

                try
                {
                    tempReg_Dec = int.Parse(tempReg_Hex[1], System.Globalization.NumberStyles.HexNumber);       //convert Effective BIT for given register address from HEX to Decimal
                    dataSizeHex = dataSizeHex + tempReg_Hex[1];
                }
                catch (Exception)
                {
                    MessageBox.Show("!!! WRONG SELECTIVE BIT FORMAT !!!\n" +
                        "DATA MUST BE IN HEX FORMAT (" + SwBand + " : " + biasDataArr[i] + ")\n" +
                        "PLS CHECK & FIXED IN MIPI WORKSHEET");
                }

                string tempReg_Binary = Convert.ToString(tempReg_Dec, 2).PadLeft(8, '0');                       //Convert DEC to 8 Bit Binary

                //sort out the effective data base of effective bit of a given register address
                //Example : 0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read
                //example after MIPI read => reg 0x42 = data read 0x2E (00101110)
                //reg 0x43 only bit0 and bit1 will be taken (*note: shown in bracket) , effectiveBitData(0x2E) = 001011 (10)
                char[] selectiveBitReg_Binary = new char[8];
                char[] selectiveData_Binary = new char[8];

                //stored in charArray format in Binary form
                selectiveBitReg_Binary = tempReg_Binary.ToCharArray();
                selectiveData_Binary = dataBinary[i].ToCharArray();

                for (int j = 0; j < selectiveBitReg_Binary.Length; j++)
                {
                    if (selectiveBitReg_Binary[j] == '1')
                    {
                        appendBinary = appendBinary + selectiveData_Binary[j];   //construct and concatenations binary data bit by bit
                    }
                }
            }

            decDataOut = Convert.ToInt32(appendBinary, 2);            //Convert Binary to Decimal
        }

        public void readout_OTPReg_viaEffectiveBit2(int delay_mSec, string SwBand, string CusMipiRegMap, string CusPMTrigMap, string CusSlaveAddr, string CusMipiPair, string CusMipiSite, out int decDataOut, out string dataSizeHex)
        {
            biasDataArr = null;
            dataSizeHex = null;
            decDataOut = 0;

            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

            //Init variable & sorted effective bit data
            appendBinary = null;
            dataDec = new int[biasDataArr.Length];
            dataBinary = new string[biasDataArr.Length];

            //Note : effective bit are selected if any of the bit is set to '1' in CusMipiRegMap data column (in hex format >> register:effective bits) => 0x42:0x03
            //example CusMipiRegMap must be in '42:03 43:FF' where 0x43 is LSB reg address and 0xFF (1111 1111) all 8 bits are to be effectively read
            //0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read

            //example after MIPI read => reg 0x43 = data read 0xCB (11001011) &  reg 0x42 = data read 0x2E (00101110)
            //Effective bit decode example => for reg 0x42 since all 8bits will be use , effectiveBitData(0xCB) = 11001011
            //while reg 0x43 only bit0 and bit1 will be taken (*note: shown in bracket) , effectiveBitData(0x2E) = 001011 (10)
            //reported data => 10 11001011 => convert to dec = 715

            //Set MIPI and Read MIPI
            EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
            DelayMs(delay_mSec);

            for (int i = 0; i < biasDataArr.Length; i++)
            {
                EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                dataDec[i] = tmpOutData;
                dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
            }

            //sorted out MIPI data base effective bit selection and publish result
            for (int i = 0; i < biasDataArr.Length; i++)
            {
                //sort out the effective bit - register address
                //Example : 0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read
                int tempReg_Dec = 0;
                string[] tempReg_Hex = new string[2];
                tempReg_Hex = biasDataArr[i].Split(':');

                try
                {
                    tempReg_Dec = int.Parse(tempReg_Hex[1], System.Globalization.NumberStyles.HexNumber);       //convert Effective BIT for given register address from HEX to Decimal
                    dataSizeHex = dataSizeHex + tempReg_Hex[1];
                }
                catch (Exception)
                {
                    MessageBox.Show("!!! WRONG SELECTIVE BIT FORMAT !!!\n" +
                        "DATA MUST BE IN HEX FORMAT (" + SwBand + " : " + biasDataArr[i] + ")\n" +
                        "PLS CHECK & FIXED IN MIPI WORKSHEET");
                }

                string tempReg_Binary = Convert.ToString(tempReg_Dec, 2).PadLeft(8, '0');                       //Convert DEC to 8 Bit Binary

                //sort out the effective data base of effective bit of a given register address
                //Example : 0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read
                //example after MIPI read => reg 0x42 = data read 0x2E (00101110)
                //reg 0x43 only bit0 and bit1 will be taken (*note: shown in bracket) , effectiveBitData(0x2E) = 001011 (10)
                char[] selectiveBitReg_Binary = new char[8];
                char[] selectiveData_Binary = new char[8];

                //stored in charArray format in Binary form
                selectiveBitReg_Binary = tempReg_Binary.ToCharArray();
                selectiveData_Binary = dataBinary[i].ToCharArray();

                for (int j = 0; j < selectiveBitReg_Binary.Length; j++)
                {
                    if (selectiveBitReg_Binary[j] == '1')
                    {
                        appendBinary = appendBinary + selectiveData_Binary[j];   //construct and concatenations binary data bit by bit
                    }
                }
            }

            decDataOut = Convert.ToInt32(appendBinary, 2);            //Convert Binary to Decimal
        }

        public void burn_OTPReg_viaEffectiveBit(string testParam, string CusMipiRegMap, string CusMipiPair, string CusSlaveAddr, string[] dataHex)
        {
            #region Decode MIPI Register and Burn OTP
            biasDataArr = null;

            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
            int[] efuseCtrlAddress = new int[4];
            string[] tempData = new string[2];

            for (int i = 0; i < biasDataArr.Length; i++)
            {
                //Note : EFuse Control Register
                //efuse cell_0 (0xE0, mirror address 0x0D)
                //efuse cell_1 (0xE1, mirror address 0x0E)
                //efuse cell_2 (0xE2, mirror address 0x21)
                //efuse cell_3 (0xE3, mirror address 0x40)
                //efuse cell_4 (0xE4, mirror address 0x41)

                tempData = biasDataArr[i].Split(':');
                switch (tempData[0].ToUpper())
                {
                    case "E0":
                        efuseCtrlAddress[3] = 0;
                        efuseCtrlAddress[2] = 0;
                        efuseCtrlAddress[1] = 0;
                        efuseCtrlAddress[0] = 0;
                        break;
                    case "E1":
                        efuseCtrlAddress[3] = 0;
                        efuseCtrlAddress[2] = 0;
                        efuseCtrlAddress[1] = 0;
                        efuseCtrlAddress[0] = 1;
                        break;
                    case "E2":
                        efuseCtrlAddress[3] = 0;
                        efuseCtrlAddress[2] = 0;
                        efuseCtrlAddress[1] = 1;
                        efuseCtrlAddress[0] = 0;
                        break;
                    case "E3":
                        efuseCtrlAddress[3] = 0;
                        efuseCtrlAddress[2] = 0;
                        efuseCtrlAddress[1] = 1;
                        efuseCtrlAddress[0] = 1;
                        break;
                    case "E4":
                        efuseCtrlAddress[3] = 0;
                        efuseCtrlAddress[2] = 1;
                        efuseCtrlAddress[1] = 0;
                        efuseCtrlAddress[0] = 0;
                        break;
                    case "E5":
                        efuseCtrlAddress[3] = 0;
                        efuseCtrlAddress[2] = 1;
                        efuseCtrlAddress[1] = 0;
                        efuseCtrlAddress[0] = 1;
                        break;
                    case "E6":
                        efuseCtrlAddress[3] = 0;
                        efuseCtrlAddress[2] = 1;
                        efuseCtrlAddress[1] = 1;
                        efuseCtrlAddress[0] = 0;
                        break;
                    case "E7":
                        efuseCtrlAddress[3] = 0;
                        efuseCtrlAddress[2] = 1;
                        efuseCtrlAddress[1] = 1;
                        efuseCtrlAddress[0] = 1;
                        break;
                    default:
                        MessageBox.Show("Test Parameter : " + testParam + "(" + tempData[0].ToUpper() + ") - OTP Address not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                        break;
                }

                #region Burn OTP Data

                if (BurnOTP)
                {
                    //Burn3x to double confirm the otp programming is done completely
                    for (int cnt = 0; cnt < 3; cnt++)
                    {
                        JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                    }
                }

                #endregion
            }

            #endregion
        }
        public void mask_viaEffectiveBit(string[] dataHex, string SwBand, string CusMipiRegMap, out int decDataOut)
        {
            biasDataArr = null;
            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

            //Init variable & sorted effective bit data
            dataSizeHex = null;
            decDataOut = 0;
            appendBinary = null;
            dataDec = new int[biasDataArr.Length];
            dataBinary = new string[biasDataArr.Length];

            //Note : effective bit are selected if any of the bit is set to '1' in CusMipiRegMap data column (in hex format >> register:effective bits) => 0x42:0x03
            //example CusMipiRegMap must be in '42:03 43:FF' where 0x43 is LSB reg address and 0xFF (1111 1111) all 8 bits are to be effectively read
            //0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read

            //example after MIPI read => reg 0x43 = data read 0xCB (11001011) &  reg 0x42 = data read 0x2E (00101110)
            //Effective bit decode example => for reg 0x42 since all 8bits will be use , effectiveBitData(0xCB) = 11001011
            //while reg 0x43 only bit0 and bit1 will be taken (*note: shown in bracket) , effectiveBitData(0x2E) = 001011 (10)
            //reported data => 10 11001011 => convert to dec = 715

            for (int i = 0; i < biasDataArr.Length; i++)
            {
                tmpOutData = int.Parse(dataHex[i], System.Globalization.NumberStyles.HexNumber);
                dataDec[i] = tmpOutData;
                dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
            }

            //sorted out MIPI data base effective bit selection and publish result
            for (int i = 0; i < biasDataArr.Length; i++)
            {
                //sort out the effective bit - register address
                //Example : 0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read
                int tempReg_Dec = 0;
                string[] tempReg_Hex = new string[2];
                tempReg_Hex = biasDataArr[i].Split(':');

                try
                {
                    tempReg_Dec = int.Parse(tempReg_Hex[1], System.Globalization.NumberStyles.HexNumber);       //convert Effective BIT for given register address from HEX to Decimal
                    dataSizeHex = dataSizeHex + tempReg_Hex[1];
                }
                catch (Exception)
                {
                    MessageBox.Show("!!! WRONG SELECTIVE BIT FORMAT !!!\n" +
                        "DATA MUST BE IN HEX FORMAT (" + SwBand + " : " + biasDataArr[i] + ")\n" +
                        "PLS CHECK & FIXED IN MIPI WORKSHEET");
                }

                string tempReg_Binary = Convert.ToString(tempReg_Dec, 2).PadLeft(8, '0');                       //Convert DEC to 8 Bit Binary

                //sort out the effective data base of effective bit of a given register address
                //Example : 0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read
                //example after MIPI read => reg 0x42 = data read 0x2E (00101110)
                //reg 0x43 only bit0 and bit1 will be taken (*note: shown in bracket) , effectiveBitData(0x2E) = 001011 (10)
                char[] selectiveBitReg_Binary = new char[8];
                char[] selectiveData_Binary = new char[8];

                //stored in charArray format in Binary form
                selectiveBitReg_Binary = tempReg_Binary.ToCharArray();
                selectiveData_Binary = dataBinary[i].ToCharArray();

                for (int j = 0; j < selectiveBitReg_Binary.Length; j++)
                {
                    if (selectiveBitReg_Binary[j] == '1')
                    {
                        appendBinary = appendBinary + selectiveData_Binary[j];   //construct and concatenations binary data bit by bit
                    }
                }
            }

            decDataOut = Convert.ToInt32(appendBinary, 2);            //Convert Binary to Decimal
        }

        #endregion

        public void BuildResults(ref ATFReturnResult results, string paraName, string unit, double value)
        {
            ATFResultBuilder.AddResult(ref results, paraName, unit, value);
            if (!ResultBuilder.CheckPass(paraName, value)) FailedTests.Add(paraName);
        }

        //Get the pathloss data
        private void GetCalData_Array(out double[] rtnPathloss, out double[] rtnPathlossFreq, string calTag, string calSegm, double startFreq, double stopFreq, double stepFreq, string searchMethod, double optSearchValue = 1710)
        {
            double searchFreq = -999;
            double lossOutput = 999;
            string strError = null;
            double[] tmpPathloss;
            double[] tmpPathlossFreq;

            //Get pathloss base on start and stop freq
            int count = Convert.ToInt16((stopFreq - startFreq) / stepFreq) + 1;
            searchFreq = Math.Round(startFreq, 3);          //need to use round function because of C# float and double floating point bug/error

            //initialize array
            tmpPathloss = new double[count];
            tmpPathlossFreq = new double[count];
            rtnPathloss = new double[count];
            rtnPathlossFreq = new double[count];

            for (int i = 0; i < count; i++)
            {
                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(calTag, calSegm, searchFreq, ref lossOutput, ref strError);
                tmpPathloss[i] = lossOutput;
                tmpPathlossFreq[i] = searchFreq;
                searchFreq = Math.Round(searchFreq + stepFreq, 3);      //need to use round function because of C# float and double floating point bug/error
            }

            //Sort out test result
            switch (searchMethod.ToUpper())
            {
                case "ALL":
                case "RANGE":
                    rtnPathloss = tmpPathloss;
                    rtnPathlossFreq = tmpPathlossFreq;
                    break;

                case "MAX":
                    rtnPathloss = new double[1];
                    rtnPathlossFreq = new double[1];
                    rtnPathloss[0] = tmpPathloss.Max();
                    rtnPathlossFreq[0] = tmpPathlossFreq[Array.IndexOf(tmpPathloss, rtnPathloss[0])];
                    break;

                case "MIN":
                    rtnPathloss = new double[1];
                    rtnPathlossFreq = new double[1];
                    rtnPathloss[0] = tmpPathloss.Min();
                    rtnPathlossFreq[0] = tmpPathlossFreq[Array.IndexOf(tmpPathloss, rtnPathloss[0])];
                    break;

                case "AVE":
                case "AVERAGE":
                    rtnPathloss = new double[1];
                    rtnPathlossFreq = new double[1];
                    rtnPathloss[0] = tmpPathloss.Average();
                    rtnPathlossFreq[0] = tmpPathlossFreq[0];          //return default freq i.e Start Freq
                    break;

                case "USER":
                    rtnPathloss = new double[1];
                    rtnPathlossFreq = new double[1];

                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                    if ((optSearchValue >= startFreq) && (optSearchValue <= stopFreq))
                    {
                        try
                        {
                            rtnPathloss[0] = tmpPathloss[Array.IndexOf(tmpPathlossFreq, optSearchValue)];     //return contact power from same array number(of index number associated with 'USER' Freq)
                            rtnPathlossFreq[0] = optSearchValue;
                        }
                        catch       //if ºSearch_Value not in tmpPathlossFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                        {
                            rtnPathloss[0] = 99999;
                            rtnPathlossFreq[0] = optSearchValue;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Function: GetCalData_Array" + "(SEARCH METHOD : " + searchMethod + ", USER DEFINE : " + optSearchValue + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    }
                    break;

                default:
                    MessageBox.Show("Function: GetCalData_Array" + "(SEARCH METHOD : " + searchMethod + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    break;
            }
        }
        private void GetCalData(out double rtnPathloss, out double rtnPathlossFreq, string calTag, string calSegm, double startFreq, double stopFreq, double stepFreq, string searchMethod, double optSearchValue = 1710)
        {
            double searchFreq = -999;
            double lossOutput = 999;
            string strError = null;
            double[] tmpPathloss;
            double[] tmpPathlossFreq;

            //Get pathloss base on start and stop freq
            int count = Convert.ToInt16((stopFreq - startFreq) / stepFreq) + 1;
            searchFreq = Math.Round(startFreq, 3);         //need to use round function because of C# float and double floating point bug/error

            //initialize array
            tmpPathloss = new double[count];
            tmpPathlossFreq = new double[count];
            rtnPathloss = 999;
            rtnPathlossFreq = -999;

            for (int i = 0; i < count; i++)
            {
                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(calTag, calSegm, searchFreq, ref lossOutput, ref strError);
                tmpPathloss[i] = lossOutput;
                tmpPathlossFreq[i] = searchFreq;
                searchFreq = Math.Round(searchFreq + stepFreq, 3);      //need to use round function because of C# float and double floating point bug/error
            }

            //Sort out test result
            switch (searchMethod.ToUpper())
            {
                case "MAX":
                    rtnPathloss = tmpPathloss.Max();
                    rtnPathlossFreq = tmpPathlossFreq[Array.IndexOf(tmpPathloss, rtnPathloss)];
                    break;

                case "MIN":
                    rtnPathloss = tmpPathloss.Min();
                    rtnPathlossFreq = tmpPathlossFreq[Array.IndexOf(tmpPathloss, rtnPathloss)];
                    break;

                case "AVE":
                case "AVERAGE":
                    rtnPathloss = tmpPathloss.Average();
                    rtnPathlossFreq = tmpPathlossFreq[0];          //return default freq i.e Start Freq
                    break;

                case "USER":
                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                    if ((optSearchValue >= startFreq) && (optSearchValue <= stopFreq))
                    {
                        try
                        {
                            rtnPathloss = tmpPathloss[Array.IndexOf(tmpPathlossFreq, optSearchValue)];     //return contact power from same array number(of index number associated with 'USER' Freq)
                            rtnPathlossFreq = optSearchValue;
                        }
                        catch       //if ºSearch_Value not in tmpPathlossFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                        {
                            rtnPathloss = 999;
                            rtnPathlossFreq = optSearchValue;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Function: GetCalData" + "(SEARCH METHOD : " + searchMethod + ", USER DEFINE : " + optSearchValue + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    }
                    break;

                default:
                    MessageBox.Show("Function: GetCalData" + "(SEARCH METHOD : " + searchMethod + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    break;
            }
        }

        //Get Power Blast Setting
        public void searchPowerBlastKey(string testParam, string searchKey, out double cntrFreqMhz, out double startPwrlvl, out double stopPwrlvl, out int stepPwrlvl,
                                        out double dwellTimeMs, out double transientMs, out int transientStep, out bool b_pwrBlastTKey)
        {
            string tmpStingOut = null;

            //initialize variable - reset to default
            b_pwrBlastTKey = false;
            cntrFreqMhz = -999;
            startPwrlvl = -999;
            stopPwrlvl = -999;
            stepPwrlvl = -999;
            dwellTimeMs = -999;
            transientMs = -999;
            transientStep = -999;

            //Data from Mipi custom spreadsheet 
            foreach (Dictionary<string, string> currPwrBlast in DicPwrBlast)
            {
                currPwrBlast.TryGetValue("TEST SELECTION", out DicMipiTKey);

                if (searchKey.ToUpper() == DicMipiTKey)
                {
                    currPwrBlast.TryGetValue("CENTERFREQ_MHZ", out tmpStingOut);
                    cntrFreqMhz = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("START_PWRLVL", out tmpStingOut);
                    startPwrlvl = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("STOP_PWRLVL", out tmpStingOut);
                    stopPwrlvl = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("STEP_PWRLVL", out tmpStingOut);
                    stepPwrlvl = Convert.ToInt32(tmpStingOut);

                    currPwrBlast.TryGetValue("DWELLT_MS", out tmpStingOut);
                    dwellTimeMs = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("TRANSIENT_MS", out tmpStingOut);
                    transientMs = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("TRANSIENT_STEP", out tmpStingOut);
                    transientStep = Convert.ToInt32(tmpStingOut);

                    b_pwrBlastTKey = true;          //change flag if match
                }
            }

            if (!b_pwrBlastTKey)        //if cannot find , show error
                MessageBox.Show("Failed to find Power Blast Test Selection KEY (" + searchKey.ToUpper() + ") in PWRBlast sheet \n\n", testParam.ToUpper(), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void NFvariables(Dictionary<string, string> TestPara, out double[] LNAInputLoss, out double[] RXPathLoss, out double[] RXContactFreq)
        {
            string StrError = string.Empty;

            float ºRXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRXFreq));
            float ºStartRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq1));
            float ºStopRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq1));
            float ºStepRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepRXFreq1));
            string ºSwBand_HotNF = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSwitching_Band_HotNF);

            //Load cal factor           
            double ºLossCouplerPath = 999;
            double ºLossOutputPathRX1 = 999;

            NoOfPts = (Convert.ToInt32(Math.Ceiling((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1))) + 1;

            RXContactFreq = new double[NoOfPts];
            RXPathLoss = new double[NoOfPts];
            LNAInputLoss = new double[NoOfPts];

            #region Decode Calibration Path and Segment Data
            string CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", ºSwBand_HotNF.ToUpper());
            myUtility.Decode_CalSegm_Setting(CalSegmData);
            #endregion

            ºRXFreq = ºStartRXFreq1;

            int count = Convert.ToInt16(Math.Ceiling((ºStopRXFreq1 - ºStartRXFreq1) / ºStepRXFreq1));

            for (int i = 0; i <= count; i++)
            {
                RXContactFreq[i] = Math.Round(ºRXFreq, 3);

                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, ºRXFreq, ref ºLossOutputPathRX1, ref StrError);
                RXPathLoss[i] = ºLossOutputPathRX1;//Seoul

                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, ºRXFreq, ref ºLossCouplerPath, ref StrError);
                LNAInputLoss[i] = ºLossCouplerPath;//Seoul

                ºRXFreq = Convert.ToSingle(Math.Round(ºRXFreq + ºStepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                if (ºRXFreq > ºStopRXFreq1)//For Last Freq match
                {
                    ºRXFreq = ºStopRXFreq1;
                }
            }
        }

        public void StoreNFdata(int TestCount, int NumberOfRuns, int NoOfPts, double[][] NFdata)
        {
            for (int n = 0; n < NumberOfRuns; n++)
            {
                for (int istep = 0; istep < NoOfPts; istep++)
                {
                    PXITrace[TestCount].Multi_Trace[0][n].Ampl[istep] = Math.Round(NFdata[n][istep], 3);
                    PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl[istep] = Math.Round(NFdata[n][istep], 3);
                }
            }
        }

        public void StoreNFRisedata(int NFRiseTestCount, int ColdNFTestCount, int HotNFTestCount, int ColdNFNumberOfRuns, int HotNFNumberOfRuns, int ColdNFNoOfPts, int HotNFNoOfPts, string TestParaName, int TestNum)
        {
            for (int n = 0; n < HotNFNumberOfRuns; n++)
            {
                Dictionary<double, double> dic_ColdNF = new Dictionary<double, double>();
                Dictionary<double, double> dic_HotNF = new Dictionary<double, double>();

                //temp trace array storage use for MAX , MIN etc calculation 
                PXITrace[TestCount].Enable = true;
                //PXITrace[TestCount].SoakSweep = preSoakSweep;
                PXITrace[TestCount].TestNumber = TestNum;
                PXITrace[TestCount].TraceCount = HotNFNumberOfRuns;
                PXITrace[TestCount].Multi_Trace[0][n].NoPoints = HotNFNoOfPts;
                PXITrace[TestCount].Multi_Trace[0][n].RBW_Hz = PXITrace[HotNFTestCount].Multi_Trace[0][n].RBW_Hz;
                PXITrace[TestCount].Multi_Trace[0][n].FreqMHz = new double[HotNFNoOfPts];
                PXITrace[TestCount].Multi_Trace[0][n].Ampl = new double[HotNFNoOfPts];
                PXITrace[TestCount].Multi_Trace[0][n].Result_Header = TestParaName;
                PXITrace[TestCount].Multi_Trace[0][n].MXA_No = "PXI_NF_RISE_Trace";

                PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz = new double[HotNFNoOfPts];
                PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl = new double[HotNFNoOfPts];

                for (int istep = 0; istep < ColdNFNoOfPts; istep++)
                {
                    dic_ColdNF.Add(PXITrace[ColdNFTestCount].Multi_Trace[0][0].FreqMHz[istep], PXITrace[ColdNFTestCount].Multi_Trace[0][0].Ampl[istep]); //1st sweep of cold NF data is used for NF Rise calculation                    
                }

                for (int istep = 0; istep < HotNFNoOfPts; istep++)
                {
                    dic_HotNF.Add(PXITrace[HotNFTestCount].Multi_Trace[0][n].FreqMHz[istep], PXITrace[HotNFTestCount].Multi_Trace[0][n].Ampl[istep]);
                }

                for (int istep = 0; istep < HotNFNoOfPts; istep++)
                {
                    double nfFreq = PXITrace[HotNFTestCount].Multi_Trace[0][n].FreqMHz[istep];

                    PXITrace[TestCount].Multi_Trace[0][n].FreqMHz[istep] = PXITrace[HotNFTestCount].Multi_Trace[0][n].FreqMHz[istep];
                    PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz[istep] = PXITrace[HotNFTestCount].Multi_Trace[0][n].FreqMHz[istep];

                    if (dic_HotNF[nfFreq].ToString() == ("9999") || dic_ColdNF[nfFreq].ToString() == ("9999"))
                    {
                        PXITrace[TestCount].Multi_Trace[0][n].Ampl[istep] = 9999;
                        PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl[istep] = 9999;
                    }

                    else
                    {
                        PXITrace[TestCount].Multi_Trace[0][n].Ampl[istep] = dic_HotNF[nfFreq] - dic_ColdNF[nfFreq];
                        PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl[istep] = dic_HotNF[nfFreq] - dic_ColdNF[nfFreq];
                    }
                }
            }

        }

        public static int GetNextModuleID(int maxModuleID, out bool status)
        {
            status = true;      //default set to true
            int siteNumber = 0;
            string lotId = "999999";
            int moduleId = 0;
            string[] Id;
            string ModuleDir = @"C:\Avago.ATF.Common\ModuleID\";
            string moduleIDLogFile = "";
            char[] separator = new char[] { '-' };
            int site1InitID = 0, site2InitID = 10000, site3InitID = 20000;
            string warning = "";

            string testerId = "999999";

#if (!DEBUG)
    testerId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_ID, "");
    lotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
#else
            testerId = "DUMMY-02";      // Need to enable this during debug mode
            lotId = "PT0000000000-E";
            MessageBox.Show("Program Running in Debug Mode Compilation - For Lab Usage only", "!!! WARNING !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif

    try
    {
        testerId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_TESTER_ID, "");
        lotId = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_LOT_ID, "");
    }
    catch
    {

    }
          if (testerId == "") testerId = "DUMMY-01";
          if (lotId.Contains("\t")) lotId = lotId.Replace("\t", "");


            ////For debug purpose
          //MessageBox.Show("TesterId: " + testerId + "@ LotId: " + lotId);

            Id = testerId.Split(separator);

            try
            {
                siteNumber = Convert.ToInt32(Id[1]);
            }
            catch (Exception e)
            {
                MessageBox.Show("Invalid Tester_ID (" + testerId + ") was entered", "!!! WARNING !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                status = false;
                return 0;
            }

            moduleIDLogFile = string.Format("{0}{1}.txt", ModuleDir, lotId);

            if (File.Exists(moduleIDLogFile))
            {
                moduleId = Convert.ToInt32(System.IO.File.ReadAllText(moduleIDLogFile)) + 1;
            }
            else
            {
                if (siteNumber == 1) { moduleId = site1InitID + 1; }
                else if (siteNumber == 2) { moduleId = site2InitID + 1; }
                else if (siteNumber == 3) { moduleId = site3InitID + 1; }
                else { moduleId = 0; }
            }

            //To prevent duplicate module ID in case test sites are down and only single test site is down.
            if (siteNumber == 1)
            {
                if (moduleId > site2InitID)
                {
                    warning = string.Format("Module ID for Site{0} exceeded {1}!\nIf test is continue, there may be duplicated module ID",
                        siteNumber, site2InitID);
                    status = false;
                    MessageBox.Show(warning, "!!! WARNING !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (siteNumber == 2)
            {
                if (moduleId > site3InitID)
                {
                    warning = string.Format("Module ID for Site{0} exceeded {1}!\nIf test is continue, there may be duplicated module ID",
                        siteNumber, site3InitID);
                    status = false;
                    MessageBox.Show(warning, "!!! WARNING !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (siteNumber == 3)
            {
                if (moduleId > maxModuleID)
                {
                    warning = string.Format("Module ID for Site{0} exceeded {1}!\nQuit test, Module ID not supported",
                        siteNumber, maxModuleID);
                    status = false;
                    MessageBox.Show(warning, "!!! WARNING !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (!Directory.Exists(ModuleDir)) { Directory.CreateDirectory(ModuleDir); }

            try
            {
                System.IO.File.WriteAllText(moduleIDLogFile, moduleId.ToString());
            }
            catch (Exception e)
            {
                status = false;
                MessageBox.Show(e.Message);
            }

            return moduleId;
        }
    }
}