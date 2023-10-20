using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.SpecAnMX;
using System.Diagnostics;
using System.Threading;

namespace LibEqmtDriver.NF_VST
{
    public class NF_NI_RFmx
    {
        //RFmx Variables
        public static RFmxInstrMX instrSession;
        public List<RFmxSpecAnMX> specNFColdSource;
        public List<RFmxSpecAnMX[]> specNFColdSource2;

        //Cold source Variables
        public double DutInputLossTemperature = 290; //Original Default value 297
        public double DutOutputLossTemperature = 290; //Original Default value 297

        public  double[] frequencyList = null;
        public  double[] DutInputLoss = new double[1], DutInputLossFrequency = new double[1];
        public  double[] DutOutputLoss = new double[1], DutOutputLossFrequency = new double[1];
        public  double[] calibrationLoss = new double[1], calibrationLossFrequency = new double[1];
        public  double[] externalPreampFrequency = new double[1], externalPreampGain = new double[1];
        public  double[] sParamFrequency = new double[1];
        public  double[] s11, s12, s21, s22;            /*dB*/
        public  double[] coldSourcePower;              /*dBm*/
        public  double[] dutGain;                      /*dB*/
        public  double[] dutNoiseFigure;               /*dB*/
        public  double[] dutNoiseTemperature;          /*K*/
        public  double[] frequencyListOut;
        public  double[] analyserNoiseFigure;          /*dB*/

        public RFmxSpecAnMXNFAveragingEnabled NFaveragingEnabled = RFmxSpecAnMXNFAveragingEnabled.True;
        public  RFmxSpecAnMXNFMeasurementMethod measurementMethod = RFmxSpecAnMXNFMeasurementMethod.ColdSource;
        public  RFmxSpecAnMXNFColdSourceMode coldSourceMode = RFmxSpecAnMXNFColdSourceMode.Measure;
        public  RFmxSpecAnMXNFDutInputLossCompensationEnabled DutInputLossCompEnabled = RFmxSpecAnMXNFDutInputLossCompensationEnabled.True;
        public  RFmxSpecAnMXNFDutOutputLossCompensationEnabled DutOutputLossCompEnabled = RFmxSpecAnMXNFDutOutputLossCompensationEnabled.True;
        public  RFmxSpecAnMXNFCalibrationLossCompensationEnabled calibrationLossCompensationEnabled = RFmxSpecAnMXNFCalibrationLossCompensationEnabled.False;
        public  RFmxSpecAnMXNFExternalPreampPresent externalPreampPresent = RFmxSpecAnMXNFExternalPreampPresent.False;
        public  FrequencyListConfigurationType frequencyListConfiguration = FrequencyListConfigurationType.Step;

        // Common Variables
        double timeout = 20;
        private string IOAddress;
        public  int averagingCount;
        public  int numberOfPoints;
        public  string calSetupID = "";

        public static bool manualReferenceLevel;

        static IntPtr niRfsaHandle;
        public ManualResetEvent DoneNFCommit = new ManualResetEvent(false);

        public class Config_RFmx
        {
            public int Iteration;
            public int HotIteraton;
            public double[] Freq;
            public double[] RxGain;

            public Config_RFmx()
            {

            }

            public Config_RFmx(int Iteration, int HotIteraton, double[] Freq, double[] RxGain)
            {
                this.Iteration = Iteration;
                this.HotIteraton = HotIteraton;
                this.Freq = Freq;
                this.RxGain = RxGain;
            }
        }

        public static IntPtr InitializeInstr(string resourceName)
        {
            /* Create a new RFmx Session */
            instrSession = new RFmxInstrMX(resourceName, "DriverSetup=Bitfile:NI Power Servoing for VST.lvbitx");
            instrSession.ConfigureFrequencyReference("", RFmxInstrMXConstants.PxiClock, 10e6);
            instrSession.DangerousGetNIRfsaHandle(out niRfsaHandle);
            return niRfsaHandle;
        }
        
        public enum FrequencyListConfigurationType
        {
            Step = (int)0,
            Points = (int)1,
            Frequency = (int)2,
        }

        public NF_NI_RFmx(string IOAddress)
        {
            this.IOAddress = IOAddress;
        }

        public void InitList(int numOfTests)
        {
            specNFColdSource = new List<RFmxSpecAnMX>(numOfTests);
            specNFColdSource2 = new List<RFmxSpecAnMX[]>(numOfTests);

            for (int i = 0; i < numOfTests; i++)
            {
                specNFColdSource.Add(null);
                specNFColdSource2.Add(null);
            }
        }

        public void SetDownConverterOffset(double LoOffset)
        {
            instrSession.SetDownconverterFrequencyOffset("", LoOffset);
        }

        public void CalibratioSpeNFCouldSource(int Iteration, string NF_CalTag, double[] Freq, double[] DutInputLoss, double[] DutOutputLoss, double NF_BW)
        {
            string caLSetID = "";
            double[] rxFreqArray_Conversion = new double[Freq.Length];
            for (int i = 0; i < Freq.Length; i++)
            {
                rxFreqArray_Conversion[i] = Math.Round(Freq[i] * 1e6, 3);
            }

            double[] dutInputLoss_Conversion = new double[DutInputLoss.Length];
            double[] dutOutputLoss_Conversion = new double[DutOutputLoss.Length];

            for (int i = 0; i < DutInputLoss.Length; i++)
            {
                dutInputLoss_Conversion[i] = DutInputLoss[i] * (-1);
                dutOutputLoss_Conversion[i] = DutOutputLoss[i] * (-1);
            }

            instrSession.SetDownconverterFrequencyOffset("", ((NF_BW / 2) + 2.2) * 1e6);

            specNFColdSource[Iteration].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqArray_Conversion, dutInputLoss_Conversion, DutInputLossTemperature);
            specNFColdSource[Iteration].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqArray_Conversion, dutOutputLoss_Conversion, DutOutputLossTemperature);

            //Calibration Setup ID for Full Array (NF Cold power)
            specNFColdSource[Iteration].NF.Configuration.SetCalibrationSetupId("", NF_CalTag);
            specNFColdSource[Iteration].NF.Configuration.ConfigureColdSourceMode("", RFmxSpecAnMXNFColdSourceMode.Calibrate);
            specNFColdSource[Iteration].NF.Configuration.ConfigureFrequencyList("", rxFreqArray_Conversion);
            specNFColdSource[Iteration].Commit("");

            specNFColdSource[Iteration].Initiate("", "");

            specNFColdSource[Iteration].NF.Results.FetchColdSourcePower("", timeout, ref coldSourcePower);
            specNFColdSource[Iteration].NF.Results.FetchAnalyzerNoiseFigure("", timeout, ref analyserNoiseFigure);
            specNFColdSource[Iteration].NF.Configuration.GetFrequencyList("", ref frequencyListOut);
            specNFColdSource[Iteration].NF.Configuration.GetCalibrationSetupId("", out caLSetID);
           
        }

        public void ListConfigureSpecNFColdSource(int Iteration, double NFBW, double NFSweepTime, int NFAverage, double NFRefLevel)
        {
            string test, RFSAmodel;

            instrSession.GetInstrumentModel("", out RFSAmodel);//as in NF initialize

            test = "NF" + Iteration.ToString();
            specNFColdSource.Insert(Iteration, instrSession.GetSpecAnSignalConfiguration(test)); // Get SpecAn Signal
            specNFColdSource[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.NF, false); //Configure Measurement
            specNFColdSource[Iteration].NF.Configuration.ConfigureMeasurementMethod("", measurementMethod);
            specNFColdSource[Iteration].NF.Configuration.ConfigureMeasurementBandwidth("", NFBW * 1e6);
            specNFColdSource[Iteration].NF.Configuration.ConfigureMeasurementInterval("", Convert.ToDouble(NFSweepTime));
            specNFColdSource[Iteration].NF.Configuration.ConfigureAveraging("", NFaveragingEnabled, NFAverage);
            specNFColdSource[Iteration].ConfigureReferenceLevel("", NFRefLevel);
            specNFColdSource[Iteration].NF.Configuration.ConfigureColdSourceMode("", RFmxSpecAnMXNFColdSourceMode.Measure);
        }

        public void ListConfigureSpecNFColdSource(int Iteration, double NFBW, double NFSweepTime, int NFAverage, double NFRefLevel, string TestParam, double[] DutInputLoss, double[] DutOutputLoss, double[] DutFreq, string CalSetID)
        {
            string test, RFSAmodel;
            instrSession.GetInstrumentModel("", out RFSAmodel);//as in NF initialize
            RFmxSpecAnMX[] rfmxList;

            double[] rxFreqArray_Conversion = new double[DutFreq.Length];
            for (int i = 0; i < DutFreq.Length; i++)
            {
                rxFreqArray_Conversion[i] = Math.Round(DutFreq[i] * 1e6, 3);
            }

            double[] dutInputLoss_Conversion = new double[DutInputLoss.Length];
            double[] dutOutputLoss_Conversion = new double[DutOutputLoss.Length];

            for (int i = 0; i < DutInputLoss.Length; i++)
            {
                dutInputLoss_Conversion[i] = DutInputLoss[i] * (-1);
                dutOutputLoss_Conversion[i] = DutOutputLoss[i] * (-1);
            }

            switch (TestParam)
            {
                case "NF_CAL":
                    test = "NF" + Iteration.ToString();
                    specNFColdSource.Insert(Iteration, instrSession.GetSpecAnSignalConfiguration(test)); // Get SpecAn Signal
                    specNFColdSource[Iteration].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.NF, false); //Configure Measurement
                    specNFColdSource[Iteration].NF.Configuration.ConfigureMeasurementMethod("", measurementMethod);
                    specNFColdSource[Iteration].NF.Configuration.ConfigureMeasurementBandwidth("", NFBW * 1e6);
                    specNFColdSource[Iteration].NF.Configuration.ConfigureMeasurementInterval("", Convert.ToDouble(NFSweepTime));
                    specNFColdSource[Iteration].NF.Configuration.ConfigureAveraging("", NFaveragingEnabled, NFAverage);
                    specNFColdSource[Iteration].NF.Configuration.ConfigureColdSourceMode("", RFmxSpecAnMXNFColdSourceMode.Measure);
                    specNFColdSource[Iteration].NF.Configuration.ConfigureFrequencyList("", rxFreqArray_Conversion);
                    specNFColdSource[Iteration].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqArray_Conversion, dutInputLoss_Conversion, DutInputLossTemperature); //DUT Input Loss List
                    specNFColdSource[Iteration].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqArray_Conversion, dutOutputLoss_Conversion, DutOutputLossTemperature); //DUT Output Loss List
                    specNFColdSource[Iteration].NF.Configuration.SetExternalPreampPresent("", externalPreampPresent);
                    specNFColdSource[Iteration].NF.Configuration.SetCalibrationSetupId("", CalSetID);
                    specNFColdSource[Iteration].ConfigureReferenceLevel("", NFRefLevel);
                    break;

                case "PXI_NF_COLD":                                       
                    test = "NF" + Iteration.ToString();
                    rfmxList = new RFmxSpecAnMX[2];

                    //COLD NF Region
                    for (int i = 0; i < 2; i++)
                    {
                        string testsuffix = test + '_' + i;
                        rfmxList[i] = instrSession.GetSpecAnSignalConfiguration(testsuffix);
                    }
                    specNFColdSource2.Insert(Iteration, rfmxList);

                    specNFColdSource2[Iteration][0].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.NF, false); //Configure Measurement
                    specNFColdSource2[Iteration][0].NF.Configuration.ConfigureMeasurementMethod("", measurementMethod);
                    specNFColdSource2[Iteration][0].NF.Configuration.ConfigureMeasurementBandwidth("", NFBW * 1e6);
                    specNFColdSource2[Iteration][0].NF.Configuration.ConfigureMeasurementInterval("", Convert.ToDouble(NFSweepTime));
                    specNFColdSource2[Iteration][0].NF.Configuration.ConfigureAveraging("", NFaveragingEnabled, NFAverage);
                    specNFColdSource2[Iteration][0].NF.Configuration.ConfigureColdSourceMode("", RFmxSpecAnMXNFColdSourceMode.Measure);
                    specNFColdSource2[Iteration][0].NF.Configuration.ConfigureFrequencyList("", rxFreqArray_Conversion);
                    specNFColdSource2[Iteration][0].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqArray_Conversion, dutInputLoss_Conversion, DutInputLossTemperature); //DUT Input Loss List
                    specNFColdSource2[Iteration][0].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqArray_Conversion, dutOutputLoss_Conversion, DutOutputLossTemperature); //DUT Output Loss List
                    specNFColdSource2[Iteration][0].NF.Configuration.SetExternalPreampPresent("", externalPreampPresent);
                    specNFColdSource2[Iteration][0].NF.Configuration.SetCalibrationSetupId("", CalSetID);
                    specNFColdSource2[Iteration][0].ConfigureReferenceLevel("", NFRefLevel);
                    SetDownConverterOffset(((NFBW/2)+2.2)*1e6); //Set RFmx downconverter offset 2.2MHz away from measumrent BW edge CH 

                    specNFColdSource2[Iteration][0].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                    specNFColdSource2[Iteration][0].Commit("");


                    //Dummy setting for re-configuration of RFmx
                    specNFColdSource2[Iteration][1].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.NF, false); //Configure Measurement
                    specNFColdSource2[Iteration][1].NF.Configuration.ConfigureMeasurementMethod("", measurementMethod);
                    specNFColdSource2[Iteration][1].NF.Configuration.ConfigureMeasurementBandwidth("", 15 * 1e6); //Dummy NF BW 15MHz
                    specNFColdSource2[Iteration][1].NF.Configuration.ConfigureMeasurementInterval("", Convert.ToDouble(1)); //Dummy Sweep time 1s
                    specNFColdSource2[Iteration][1].NF.Configuration.ConfigureAveraging("", NFaveragingEnabled, 100); //Dummy AVG 100
                    specNFColdSource2[Iteration][1].NF.Configuration.ConfigureColdSourceMode("", RFmxSpecAnMXNFColdSourceMode.Measure);
                    specNFColdSource2[Iteration][1].NF.Configuration.ConfigureFrequencyList("", rxFreqArray_Conversion);
                    specNFColdSource2[Iteration][1].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqArray_Conversion, dutInputLoss_Conversion, DutInputLossTemperature); //DUT Input Loss List
                    specNFColdSource2[Iteration][1].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqArray_Conversion, dutOutputLoss_Conversion, DutOutputLossTemperature); //DUT Output Loss List
                    specNFColdSource2[Iteration][1].NF.Configuration.SetExternalPreampPresent("", externalPreampPresent);
                    specNFColdSource2[Iteration][1].NF.Configuration.SetCalibrationSetupId("", CalSetID);
                    specNFColdSource2[Iteration][1].ConfigureReferenceLevel("", 30); //Dummy Ref lv 30
                    SetDownConverterOffset(((15 / 2) + 2.2) * 1e6); //Set Dummy RFmx downconverter offset

                    specNFColdSource2[Iteration][1].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                    specNFColdSource2[Iteration][1].Commit("");
                    break;

                case "PXI_NF_HOT":
                    test = "NF" + Iteration.ToString();
                    rfmxList = new RFmxSpecAnMX[DutFreq.Length + 1];

                    for (int i = 0; i < (DutFreq.Length + 1); i++)
                    {
                        string testsuffix = test + '_' + i;
                        rfmxList[i] = instrSession.GetSpecAnSignalConfiguration(testsuffix);
                    }

                    specNFColdSource2.Insert(Iteration, rfmxList);

                    //HOT NF Region
                    for (int i = 0; i < DutFreq.Length + 1; i++)
                    {
                        double[] rxFreqPoint = new double[1];
                        double[] dutInputLossPoint = new double[1];
                        double[] dutOutputLossPoint = new double[1];

                        if (i < DutFreq.Length)
                        {
                            rxFreqPoint[0] = rxFreqArray_Conversion[i];
                            dutInputLossPoint[0] = dutInputLoss_Conversion[i];
                            dutOutputLossPoint[0] = dutOutputLoss_Conversion[i];

                            specNFColdSource2[Iteration][i].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.NF, false); //Configure Measurement
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureMeasurementMethod("", measurementMethod);
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureMeasurementBandwidth("", NFBW * 1e6);
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureMeasurementInterval("", Convert.ToDouble(NFSweepTime));
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureAveraging("", NFaveragingEnabled, NFAverage);
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureColdSourceMode("", RFmxSpecAnMXNFColdSourceMode.Measure);
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureFrequencyListStartStopPoints("", rxFreqArray_Conversion[i], rxFreqArray_Conversion[i], 1);
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqPoint, dutInputLossPoint, DutInputLossTemperature); //DUT Input Loss List
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqPoint, dutOutputLossPoint, DutOutputLossTemperature); //DUT Output Loss List
                            specNFColdSource2[Iteration][i].NF.Configuration.SetExternalPreampPresent("", externalPreampPresent);
                            specNFColdSource2[Iteration][i].NF.Configuration.SetCalibrationSetupId("", CalSetID);
                            specNFColdSource2[Iteration][i].ConfigureReferenceLevel("", NFRefLevel);
                            SetDownConverterOffset(((NFBW / 2) + 2.2) * 1e6); //Set RFmx downconverter offset 2.2MHz away from measumrent BW edge CH 

                            specNFColdSource2[Iteration][i].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                            specNFColdSource2[Iteration][i].Commit("");
                        }

                        else
                        {
                            //Dummy setting for re-configuration of RFmx
                            rxFreqPoint[0] = rxFreqArray_Conversion[0];
                            dutInputLossPoint[0] = dutInputLoss_Conversion[0];
                            dutOutputLossPoint[0] = dutOutputLoss_Conversion[0];

                            specNFColdSource2[Iteration][i].SelectMeasurements("", RFmxSpecAnMXMeasurementTypes.NF, false); //Configure Measurement
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureMeasurementMethod("", measurementMethod);
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureMeasurementBandwidth("", 15 * 1e6); //Dummy NF BW 15MHz
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureMeasurementInterval("", Convert.ToDouble(1)); //Dummy Sweep time 1s
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureAveraging("", NFaveragingEnabled, 100); //Dummy AVG 100
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureColdSourceMode("", RFmxSpecAnMXNFColdSourceMode.Measure);
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureFrequencyListStartStopPoints("", rxFreqArray_Conversion[0], rxFreqArray_Conversion[0], 1);
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqPoint, dutInputLossPoint, DutInputLossTemperature); //DUT Input Loss List
                            specNFColdSource2[Iteration][i].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqPoint, dutOutputLossPoint, DutOutputLossTemperature); //DUT Output Loss List
                            specNFColdSource2[Iteration][i].NF.Configuration.SetExternalPreampPresent("", externalPreampPresent);
                            specNFColdSource2[Iteration][i].NF.Configuration.SetCalibrationSetupId("", CalSetID);
                            specNFColdSource2[Iteration][i].ConfigureReferenceLevel("", 30); //Dummy Ref lv 30
                            SetDownConverterOffset(((15 / 2) + 2.2) * 1e6); //Set Dummy RFmx downconverter offset

                            specNFColdSource2[Iteration][i].SetLimitedConfigurationChange("", RFmxSpecAnMXLimitedConfigurationChange.NoChange);
                            specNFColdSource2[Iteration][i].Commit("");
                        }
                    }
                    break;

                default:
                    MessageBox.Show("Need to check NF Configuration setting");
                    break;
            }
        }

        public void InitiateSpecAnNFColdSourece(int Iteration, string resultName, string NF_CalTag, double[] Freq, double[] DutInputLoss, double[] DutOutputLoss, double[] RxGain, double NFRefLevel, double NFSweepTime)
        {
            try
            {
                string calSetID = "";
                double[] rxFreqArray_Conversion = new double[Freq.Length];
                for (int i = 0; i < Freq.Length; i++)
                {
                    rxFreqArray_Conversion[i] = Freq[i] * 1e6;
                }

                double[] dutInputLoss_Conversion = new double[DutInputLoss.Length];
                double[] dutOutputLoss_Conversion = new double[DutOutputLoss.Length];

                for (int i = 0; i < DutInputLoss.Length; i++)
                {
                    dutInputLoss_Conversion[i] = DutInputLoss[i] * (-1);
                    dutOutputLoss_Conversion[i] = DutOutputLoss[i] * (-1);
                }

                specNFColdSource[Iteration].NF.Configuration.SetMeasurementInterval("", 1);
                specNFColdSource[Iteration].Commit("");

                specNFColdSource[Iteration].ConfigureReferenceLevel("", NFRefLevel);

                specNFColdSource[Iteration].NF.Configuration.SetMeasurementInterval("", NFSweepTime);

                specNFColdSource[Iteration].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqArray_Conversion, dutInputLoss_Conversion, DutInputLossTemperature); //DUT Input Loss List
                specNFColdSource[Iteration].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqArray_Conversion, dutOutputLoss_Conversion, DutOutputLossTemperature); //DUT Output Loss List
                specNFColdSource[Iteration].NF.Configuration.ConfigureColdSourceDutSParameters("", rxFreqArray_Conversion, RxGain, s12, s11, s22); //NF Freq & RxGain List

                specNFColdSource[Iteration].NF.Configuration.SetExternalPreampPresent("", externalPreampPresent);

                specNFColdSource[Iteration].NF.Configuration.SetCalibrationSetupId("", NF_CalTag);
                specNFColdSource[Iteration].Commit("");
                specNFColdSource[Iteration].NF.Configuration.GetCalibrationSetupId("", out calSetID);

                specNFColdSource[Iteration].Initiate("", resultName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void ConfigSpecAnNFColdSoureceDutSpara(int Iteration, double[] Freq, double[] RxGain)
        {
            try
            {
                double[] rxFreqArray_Conversion = new double[Freq.Length];
                for (int i = 0; i < Freq.Length; i++)
                {
                    rxFreqArray_Conversion[i] = Freq[i] * 1e6;
                }

                specNFColdSource[Iteration].NF.Configuration.ConfigureColdSourceDutSParameters("", rxFreqArray_Conversion, RxGain, s12, s11, s22); //NF Freq & RxGain List
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void ConfigSpecAnNFColdSourece(int Iteration, double[] Freq, double[] DutInputLoss, double[] DutOutputLoss, double[] RxGain, double NFRefLevel, double NFSweepTime)
        {
            try
            {
                double[] rxFreqArray_Conversion = new double[Freq.Length];
                for (int i = 0; i < Freq.Length; i++)
                {
                    rxFreqArray_Conversion[i] = Freq[i] * 1e6;
                }

                double[] dutInputLoss_Conversion = new double[DutInputLoss.Length];
                double[] dutOutputLoss_Conversion = new double[DutOutputLoss.Length];

                for (int i = 0; i < DutInputLoss.Length; i++)
                {
                    dutInputLoss_Conversion[i] = DutInputLoss[i] * (-1);
                    dutOutputLoss_Conversion[i] = DutOutputLoss[i] * (-1);
                }

                specNFColdSource[Iteration].ConfigureReferenceLevel("", NFRefLevel);
                specNFColdSource[Iteration].NF.Configuration.SetMeasurementInterval("", NFSweepTime);

                specNFColdSource[Iteration].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqArray_Conversion, dutInputLoss_Conversion, DutInputLossTemperature); //DUT Input Loss List
                specNFColdSource[Iteration].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqArray_Conversion, dutOutputLoss_Conversion, DutOutputLossTemperature); //DUT Output Loss List
                specNFColdSource[Iteration].NF.Configuration.ConfigureColdSourceDutSParameters("", rxFreqArray_Conversion, RxGain, s12, s11, s22); //NF Freq & RxGain List

                specNFColdSource[Iteration].NF.Configuration.SetExternalPreampPresent("", externalPreampPresent);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void ConfigSpecAnNFColdSourece(int Iteration, double[] Freq, double[] DutInputLoss, double[] DutOutputLoss, double[] RxGain, double NFRefLevel, double NFSweepTime, int IndexN)
        {
            try
            {
                double[] rxFreqArray_Conversion = new double[1];

                    rxFreqArray_Conversion[0] = Freq[IndexN] * 1e6;


                double[] dutInputLoss_Conversion = new double[1];
                double[] dutOutputLoss_Conversion = new double[1];

                    dutInputLoss_Conversion[0] = DutInputLoss[IndexN] * (-1);
                    dutOutputLoss_Conversion[0] = DutOutputLoss[IndexN] * (-1);

                double[] rxGain_Conversion = new double[1];

                rxGain_Conversion[0] = RxGain[IndexN];

                specNFColdSource[Iteration].ConfigureReferenceLevel("", NFRefLevel);
                specNFColdSource[Iteration].NF.Configuration.SetMeasurementInterval("", NFSweepTime);

                specNFColdSource[Iteration].NF.Configuration.ConfigureDutInputLoss("", DutInputLossCompEnabled, rxFreqArray_Conversion, dutInputLoss_Conversion, DutInputLossTemperature); //DUT Input Loss List
                specNFColdSource[Iteration].NF.Configuration.ConfigureDutOutputLoss("", DutOutputLossCompEnabled, rxFreqArray_Conversion, dutOutputLoss_Conversion, DutOutputLossTemperature); //DUT Output Loss List
                specNFColdSource[Iteration].NF.Configuration.ConfigureColdSourceDutSParameters("", rxFreqArray_Conversion, rxGain_Conversion, s12, s11, s22); //NF Freq & RxGain List

                specNFColdSource[Iteration].NF.Configuration.SetExternalPreampPresent("", externalPreampPresent);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void FreqListSpecAnNFColdSourece(int Iteration, double[] Freq)
        {
            double[] rxFreqArray_Conversion = new double[Freq.Length];

            for (int i = 0; i < Freq.Length; i++)
            {
                rxFreqArray_Conversion[i] = Freq[i] * 1e6;
            }
            specNFColdSource[Iteration].NF.Configuration.ConfigureFrequencyList("", rxFreqArray_Conversion);  //NF Freq List
        }

        public void FreqStepSpecAnNFColdSourece(int Iteration, double StartFreq, double StopFreq, int StepPoints)
        {
            double startFreq_Conversion = StartFreq * 1e6;
            double stopFreq_Conversion = StopFreq * 1e6;

            specNFColdSource[Iteration].NF.Configuration.ConfigureFrequencyListStartStopPoints("", startFreq_Conversion, stopFreq_Conversion, StepPoints);
        }

        public void RetrieveResults_NFColdSource(int Iteration)
        {
            try
            {
                specNFColdSource[Iteration].NF.Results.FetchColdSourcePower("", timeout, ref coldSourcePower);
                specNFColdSource[Iteration].NF.Results.FetchAnalyzerNoiseFigure("", timeout, ref analyserNoiseFigure);
                specNFColdSource[Iteration].NF.Results.FetchDutNoiseFigureAndGain("", timeout, ref dutNoiseFigure, ref dutNoiseTemperature, ref dutGain);
                //specNFColdSource[Iteration].NF.Configuration.GetFrequencyList("", ref frequencyListOut);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void RetrieveResults_NFColdSource(int Iteration, string Count)
        {
            try
            {
                specNFColdSource[Iteration].NF.Results.FetchColdSourcePower(Count, timeout, ref coldSourcePower);
                specNFColdSource[Iteration].NF.Results.FetchAnalyzerNoiseFigure(Count, timeout, ref analyserNoiseFigure);
                specNFColdSource[Iteration].NF.Results.FetchDutNoiseFigureAndGain(Count, timeout, ref dutNoiseFigure, ref dutNoiseTemperature, ref dutGain);
                //specNFColdSource[Iteration].NF.Configuration.GetFrequencyList(Count, ref frequencyListOut);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void RetrieveResults_NFColdSource(int Iteration, int HotIteration, string Count)
        {
            try
            {
                specNFColdSource2[Iteration][HotIteration].NF.Results.FetchColdSourcePower(Count, timeout, ref coldSourcePower);
                specNFColdSource2[Iteration][HotIteration].NF.Results.FetchAnalyzerNoiseFigure(Count, timeout, ref analyserNoiseFigure);
                specNFColdSource2[Iteration][HotIteration].NF.Results.FetchDutNoiseFigureAndGain(Count, timeout, ref dutNoiseFigure, ref dutNoiseTemperature, ref dutGain);
                //specNFColdSource[Iteration].NF.Configuration.GetFrequencyList(Count, ref frequencyListOut);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void WaitForAcquisitionComplete()
        {
            instrSession.WaitForAcquisitionComplete(10);
        }

        public void CloseSession()
        {
            instrSession.Close();
        }

        public void NFcommit(object o)
        {
            Config_RFmx rfmx = new Config_RFmx();
            rfmx = (Config_RFmx)o;

            specNFColdSource[rfmx.Iteration].NF.Configuration.ConfigureFrequencyListStartStopStep("", rfmx.Freq[0] * 1e6, rfmx.Freq[0] * 1e6, 1);
            ConfigSpecAnNFColdSoureceDutSpara(rfmx.Iteration, rfmx.Freq, rfmx.RxGain);
            specNFColdSource[rfmx.Iteration].Commit("");
           
            DoneNFCommit.Set();
        }

        public void NFcommit2(object o)
        {
            Config_RFmx rfmx = new Config_RFmx();
            rfmx = (Config_RFmx)o;

            specNFColdSource2[rfmx.Iteration][rfmx.HotIteraton].Commit("");

            DoneNFCommit.Set();
        }
    }
}
