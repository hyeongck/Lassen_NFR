#define NIDEEPDEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using NationalInstruments.ModularInstruments.NIDigital;


namespace InstrLib
{
    public static class HSDIO
    {
        public static Dictionary<string, string> PinNamesAndChans = new Dictionary<string, string>();

        public static bool usingMIPI = false;
        public const string Reset = "RESET", HiZ = "HiZ", RegIO = "regIO";
        public const string TrigChanName = "TRIG";
        public const string SclkPair0ChanName = "SCLKP0", SdataPair0ChanName = "SDATAP0", VioPair0ChanName = "VIOP0";       

        public struct MIPIChanName
        {
            public string SclkChanName;
            public string SdataChanName;
            public string VioChanName;
        };

        // To add pair, write one more line of MIPIChanName Definition here. Please make sure all the channel names are unique 
        // To define pin and channel mapping, go to region "Start Index of PhysicalPinsDefinition" 
        public static MIPIChanName [] allMipiChanNames =  {
                new MIPIChanName { SclkChanName = "SCLKP0", SdataChanName = "SDATAP0", VioChanName = "VIOP0" }, 
                new MIPIChanName { SclkChanName = "SCLKP1", SdataChanName = "SDATAP1", VioChanName = "VIOP1"} } ;


        public static bool useScript = false;
        public static Dictionary<string, bool> datalogResults = new Dictionary<string, bool>();
        public static iHsdioInstrument Instrument;
        public static string tmpVisaAlias;
        public static Dictionary<string, string> ConfigRegisterSettings = new Dictionary<string, string>();


        public interface iHsdioInstrument
        {
            bool LoadVector(List<string> fullPaths, string nameInMemory, bool datalogResults);
            bool LoadVector_MipiHiZ();
            bool LoadVector_MipiReset();
            bool LoadVector_MipiRegIO();
            bool SendVector(string nameInMemory);
            void SendNextVectors(bool firstTest, List<string> MipiWaveformNames);

            void ConfigureSetting(bool script, string TestMode);
            void SendTRIGVectors();
                        
            int GetNumExecErrors(string nameInMemory);
            int InterpretPID(string nameInMemory);
            int Read_AutoCal_ID(); // [Burhan]
            void RegWrite(int pair, string slaveAddress_hex, string registerAddress_hex, string data_hex, bool sendTrigger = false);
            string RegRead(int pair, string slaveAddress_hex, string registerAddress_hex);                        
            void Close();
        }


        public class NI6570 : iHsdioInstrument
        {
            /*
             * Notes:  Requires the following References added to project (set Copy Local = false):
             *   - NationalInstruments.ModularInstruments.NIDigital.Fx40
             *   - Ivi.Driver
             */

            // The Instrument Session
            public static NIDigital DIGI;

            #region Private Variables
            private string allRffeChans;
            private DigitalPinSet allRffePins, sdataPin, sclkPin, vioPin, trigPin;
            private string[] allDutPins = new string[] { };
            private double pidval;  // Stores the acquired PID value after executing the PID pattern.
            private List<string> loadedPatternFiles; // used to store previously loaded patterns so we don't try and double load.  Double Loading will cause an error, so always check this list to see if pattern was previously loaded.
            private double MIPIClockRate;  // MIPI NRZ Clock Rate (2 x Vector Rate)            
            private double StrobePoint;
            private bool forceDigiPatRegeneration = false; //false;  // Set this to true if you want to re-generate all .digipat files from the .vec files, even if the .vec files haven't changed.
            private int NumExecErrors; // Stores the number of bit errors from the most recently executed pattern.
            private Dictionary<string, uint> captureCounters = new Dictionary<string, uint>(); // This dictionary stores the # of captures each .vec contains (for .vec files that are converted to capture format)
            private string fileDir; // This is the path used to store intermediate digipatsrc, digipat, and other files.
            private TrigConfig triggerConfig = TrigConfig.None;  // No Triggering by default
            private PXI_Trig pxiTrigger = PXI_Trig.PXI_Trig7;  // TRIG0 - TRIG2 used by various other instruments in Clotho;  TRIG7 shouldn't interfere.
            private uint regWriteTriggerCycleDelay = 0;

            private bool debug = true; // Turns on additional console messages if true
            

            
            

            #region Vectors for RFONOFF and RFONOFFSwitch Test
            uint[] TrigOff = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] TrigOn = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0 };
            uint[] TrigMaskOn = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] SWREG01 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 };
            uint[] SWREG10 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] SWREG09 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0 };
            uint[] SWREG90 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0 };
            uint[] SWREG06 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0 };
            uint[] SWREG60 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0 };
            uint[] SWREG05 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0 };
            uint[] SWREG50 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0 };
            uint[] SWREG08 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] SWREG80 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] SWREG03 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0 };
            uint[] SWREG30 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0 };
            uint[] SWREG02 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0 };
            uint[] SWREG20 = new uint[46] { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            #endregion

            #endregion

            /// <summary>
            /// Initialize the NI 6570 Instrument:
            ///   - Open Instrument session
            ///   - Reset Instrument and Unload All Patterns from Instrument Memory
            ///   - Configure Pin -> Channel Mapping
            ///   - Configure Timing for: MIPI (6556 style NRZ) & MIPI_SCLK_RZ (6570 style RZ)
            ///   - Configure 6570 in Digital mode by default (instead of PPMU mode)
            /// </summary>
            /// <param name="visaAlias">The VISA Alias of the instrument, typically NI6570.</param>
            public NI6570(string visaAlias, bool AutoSubCal)
            {
                // Clock Rate & Cable Delay
                MIPIClockRate = 26e6;//26e6;  // This is the Non-Return to Zero rate, Actual Vector rate is 1/2 of this.                
                
                // Set these values based on calling ((HSDIO.NI6570)HSDIO.Instrument).shmoo("QC_Test");
                // Ideally, try to set UserDelay = 0 if possible and only modify StrobePoint.
                StrobePoint = 120E-9; // 69 changed to 65 for Max-Q
                regWriteTriggerCycleDelay = 0;

                // Trigger Configuration;  This applies to the RegWrite command and will send out a hardware trigger
                // on the specified triggers (Digital Pin, PXI Backplane, or Both) at the end of the Register Write operation.
                triggerConfig = TrigConfig.Digital_Pin;
                pxiTrigger = PXI_Trig.PXI_Trig7;  // TRIG0 - TRIG2 used by various other instruments in Clotho;  TRIG7 shouldn't interfere.               

                #region Initialize Private Variables
                fileDir = Path.GetTempPath() + "NI.Temp\\NI6570";
                Directory.CreateDirectory(fileDir);
                
                loadedPatternFiles = new List<string> { };                
                
                #endregion

                // Initialize Instrument                
                DIGI = new NIDigital(visaAlias, false, true);
                

                #region NI Pin Map Configuration
                // Make sure you add all needed pins here so that they get auto-added to all NI-6570 digipat files.  If they aren't in allDutPins or allSystemPins, you can't use them.                

                #region Start Index of PhysicalPinsDefinition
                // Define pin and channel mapping here
                int i = 10;  // first index of Channel number - First MIPI pair sclk                
                foreach (MIPIChanName mipichans in allMipiChanNames)
                {
                    PinNamesAndChans[mipichans.SclkChanName] = i.ToString();
                    i++;
                    PinNamesAndChans[mipichans.SdataChanName] = i.ToString();
                    i++;
                    PinNamesAndChans[mipichans.VioChanName] = i.ToString();
                    i++;
                }                 
                #endregion

                // Map extra pins that are not included in the TCF as of 10/07/2015

                //PinNamesAndChans[SclkPair0ChanName] = "10";  // needs automation               
                //PinNamesAndChans[SdataPair0ChanName] = "11";
                //PinNamesAndChans[VioPair0ChanName] = "12";
                PinNamesAndChans[TrigChanName] = i.ToString();

                this.allDutPins = PinNamesAndChans.Keys.ToArray();

                allRffeChans = string.Join(", ", allDutPins);                

                string allSclkChans = string.Join(", ", from m in allMipiChanNames select new { m.SclkChanName }.SclkChanName);
                string allSdataChans = string.Join(", ", from m in allMipiChanNames select new { m.SdataChanName}.SdataChanName);
                string allVioChans = string.Join(", ", from m in allMipiChanNames select new { m.VioChanName }.VioChanName);
                
                // Configure 6570 Pin Map with all pins
                DIGI.PinAndChannelMap.CreatePinMap(allDutPins, null);
                DIGI.PinAndChannelMap.CreateChannelMap(1);                
                foreach (string pin in allDutPins)
                    DIGI.PinAndChannelMap.MapPinToChannel(pin, 0, PinNamesAndChans[pin]);                
                
                DIGI.PinAndChannelMap.EndChannelMap();

                // Get DigitalPinSets
                allRffePins = DIGI.PinAndChannelMap.GetPinSet(allRffeChans);
                sclkPin = DIGI.PinAndChannelMap.GetPinSet(allSclkChans);
                sdataPin = DIGI.PinAndChannelMap.GetPinSet(allSdataChans);
                vioPin = DIGI.PinAndChannelMap.GetPinSet(allVioChans);  
                trigPin = DIGI.PinAndChannelMap.GetPinSet(TrigChanName);

                #endregion

                #region MIPI Level Configuration
                double vih = 1.8;
                double vil = 0.0;
                double voh = 0.9;
                double vol = 0.8;
                double vtt = 3.0;

                sclkPin.DigitalLevels.ConfigureVoltageLevels(vil, vih, vol, voh, vtt);
                sdataPin.DigitalLevels.ConfigureVoltageLevels(vil, vih, vol, voh, vtt);
                vioPin.DigitalLevels.ConfigureVoltageLevels(vil, vih, vol, voh, vtt);
                trigPin.DigitalLevels.ConfigureVoltageLevels(0.0, 5.0, 0.5, 2.5, 5.0); // Set VST Trigger Channel to 5V logic.  VST's PFI0 VIH is 2.0V, absolute max is 5.5V
                #endregion

                #region Timing Variable Declarations
                // Variables
                double period_dbl;
                Ivi.Driver.PrecisionTimeSpan period;
                Ivi.Driver.PrecisionTimeSpan driveOn;
                Ivi.Driver.PrecisionTimeSpan driveData;
                Ivi.Driver.PrecisionTimeSpan driveReturn;
                Ivi.Driver.PrecisionTimeSpan driveOff;
                Ivi.Driver.PrecisionTimeSpan compareStrobe;
                Ivi.Driver.PrecisionTimeSpan clockRisingEdgeDelay;
                Ivi.Driver.PrecisionTimeSpan clockFallingEdgeDelay;
                #endregion

                #region MIPI Timing Configuration
                #region Timing configuration for Return to Zero format Patterns.
                // All RegRead / RegWrite functions use the RZ format for SCLK

                // Vector Rate is 1/2 Clock Toggle Rate.
                // Compute timing values, shift all clocks out by 2 x periods so we can adjust the strobe "backwards" if needed.
                period_dbl = 1.0 / (MIPIClockRate / 2.0);
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.5 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(3.0 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * StrobePoint);

                clockRisingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl / 8.0);  // This is the amount of time after SDATA is set to high or low before SCLK is set high.
                // By setting this > 0, this will slightly delay the SCLK rising edge which can help ensure
                // SDATA is settled before clocking in the value at the DUT.
                // Note: This does not shift the Falling Edge of SCLK.  This means that adjusting this value will
                //  reduce the overall duty cycle of SCLK.  You must adjuct clockFallingEdgeDelay by the same amount
                //  if you would like to maintain a 50% duty cycle.
                clockFallingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);

                // Create Timeset
                DigitalTimeSet tsRZ = DIGI.Timing.CreateTimeSet(Timeset.MIPI_SCLK_RZ.ToString("g"));
                tsRZ.ConfigurePeriod(period);

                // Vio, Sdata, Trig
                tsRZ.ConfigureDriveEdges(vioPin, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsRZ.ConfigureCompareEdgesStrobe(vioPin, compareStrobe);
                tsRZ.ConfigureDriveEdges(sdataPin, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsRZ.ConfigureCompareEdgesStrobe(sdataPin, compareStrobe);
                tsRZ.ConfigureDriveEdges(trigPin, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsRZ.ConfigureCompareEdgesStrobe(trigPin, compareStrobe);
                // Sclk
                tsRZ.ConfigureDriveEdges(sclkPin, DriveFormat.ReturnToLow, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsRZ.ConfigureCompareEdgesStrobe(sclkPin, compareStrobe);
                #endregion

                #region Timing configuration for Non Return to Zero format Patterns (eg: 6556 style).
                // Standard .vec files use the Non Return to Zero Format

                //Actual Vector Rate is still 1/2 Clock Toggle Rate.
                // Compute timing values, shift all clocks out by 2 x periods so we can adjust the strobe "backwards" if needed.
                period_dbl = 1.0 / MIPIClockRate;
                period = Ivi.Driver.PrecisionTimeSpan.FromSeconds(period_dbl);
                driveOn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);
                driveData = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.0 * period_dbl);
                driveReturn = Ivi.Driver.PrecisionTimeSpan.FromSeconds(2.5 * period_dbl);
                driveOff = Ivi.Driver.PrecisionTimeSpan.FromSeconds(3.0 * period_dbl);
                compareStrobe = Ivi.Driver.PrecisionTimeSpan.FromSeconds(StrobePoint);

                clockRisingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0); //period / 8;  // This is the amount of time after SDATA is set to high or low before SCLK is set high.
                // By setting this > 0, this will slightly delay the SCLK rising edge which can help ensure
                // SDATA is settled before clocking in the value at the DUT.
                // Note: This does not shift the Falling Edge of SCLK.  This means that adjusting this value will
                //  reduce the overall duty cycle of SCLK.  You must adjuct clockFallingEdgeDelay by the same amount
                //  if you would like to maintain a 50% duty cycle.
                clockFallingEdgeDelay = Ivi.Driver.PrecisionTimeSpan.FromSeconds(0);

                // Create Timeset
                DigitalTimeSet tsNRZ = DIGI.Timing.CreateTimeSet(Timeset.MIPI.ToString("g"));
                tsNRZ.ConfigurePeriod(period);


                // Vio, Sdata, Trig
                tsNRZ.ConfigureDriveEdges(vioPin, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsNRZ.ConfigureCompareEdgesStrobe(vioPin, compareStrobe);
                tsNRZ.ConfigureDriveEdges(sdataPin, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsNRZ.ConfigureCompareEdgesStrobe(sdataPin, compareStrobe);
                tsNRZ.ConfigureDriveEdges(trigPin, DriveFormat.NonReturn, driveOn, driveData, driveReturn, driveOff);
                tsNRZ.ConfigureCompareEdgesStrobe(trigPin, compareStrobe);
                // Sclk
                tsNRZ.ConfigureDriveEdges(sclkPin, DriveFormat.NonReturn, driveOn + clockRisingEdgeDelay, driveData + clockRisingEdgeDelay, driveReturn + clockFallingEdgeDelay, driveOff + clockFallingEdgeDelay);
                tsNRZ.ConfigureCompareEdgesStrobe(sclkPin, compareStrobe);
                #endregion
                #endregion

                #region Configure 6570 for Digital Mode with HighZ Termination
                allRffePins.SelectedFunction = SelectedFunction.Digital;                
                allRffePins.DigitalLevels.TerminationMode =TerminationMode.HighZ;
                
                #endregion

                #region Load Vectors for MIPI Register Write Read
                // Assert that the HSDIO is ready to load MIPI vectors
                usingMIPI = true; 

                LoadVector_MipiRegIO();

                #endregion
                
            }

            public void shmoo(string nameInMemory)
            {
                double originalStrobePoint = this.StrobePoint;
                //double originalUserDelay = this.UserDelay;

                //double maxdelay = 25e-9;
                double maxstrobe = 175e-9; // (1.0 / ClockRate) * 8.0;
                //double delaystep = 1e-9;
                double strobestep = 1e-9;
                //Console.WriteLine("X-Axis: UserDelay 0nS to " + (Math.Round(maxdelay / 1e-9)).ToString() + "nS");
                Console.WriteLine("Y-Axis: StrobePoint 0nS to " + (Math.Round(maxstrobe / 1e-9)).ToString() + "nS");

                //Console.WindowHeight = Math.Min((int)(maxstrobe / strobestep) + 10, Console.LargestWindowHeight);
                //Console.WindowWidth = Math.Min((int)(maxdelay / delaystep + 2) * 5 + 5, Console.LargestWindowWidth);
                DigitalTimeSet tsNRZ = DIGI.Timing.GetTimeSet(Timeset.MIPI.ToString("g"));
                for (double compareStrobe = 0; compareStrobe < maxstrobe; compareStrobe += strobestep)
                {
                    tsNRZ.ConfigureCompareEdgesStrobe(sdataPin, Ivi.Driver.PrecisionTimeSpan.FromSeconds(compareStrobe));
                    DIGI.PatternControl.Commit();
                    Console.Write(Math.Round(compareStrobe / 1e-9).ToString().PadLeft(2, ' '));
                    //for (double delay = 0; delay < maxdelay; delay += delaystep)
                    //double delay = 0;
                    {
                        //DIGI.ConfigureUserDelay(HSDIO.SdataChanName.ToUpper(), delay);
                        this.SendVector(nameInMemory);
                        int errors = this.GetNumExecErrors(nameInMemory);
                        //Console.WriteLine((errors > 0 ? "FAIL: " : "PASS: ") + nameInMemory + " CableDelay: " + delay.ToString() + " -- Bit Errors: " + errors.ToString());
                        Console.BackgroundColor = (errors > 0 ? ConsoleColor.Red : ConsoleColor.Green);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        string errstr = "";
                        if (errors >= 1000000)
                        {
                            errstr = (errors / 1000000).ToString("D");
                            errstr = errstr.PadLeft(3, ' ') + "M ";
                        }
                        else if (errors >= 1000)
                        {
                            errstr = (errors / 1000).ToString("D");
                            errstr = errstr.PadLeft(3, ' ') + "K ";
                        }
                        else
                        {
                            errstr = errors.ToString("D");
                            errstr = errstr.PadLeft(4, ' ') + " ";
                        }
                        Console.Write((errors > 0 ? errstr : "     "));
                        Console.ResetColor();
                        if(errors < 10)
                        {
                            double result = compareStrobe;
                        }
                    }
                    Console.Write("\n");
                }
                /*Console.Write("  ");
                for (double delay = 0; delay < maxdelay; delay += 1e-9)
                {
                    string str = (delay / 1e-9).ToString();
                    Console.Write(str.PadLeft(4,' ') + " ");
                }
                Console.Write("\n");*/

                //this.UserDelay = originalUserDelay;
                this.StrobePoint = originalStrobePoint;

                //DIGI.ConfigureUserDelay(HSDIO.SdataChanName.ToUpper(), originalUserDelay);
                tsNRZ.ConfigureCompareEdgesStrobe(sdataPin, Ivi.Driver.PrecisionTimeSpan.FromSeconds(originalStrobePoint));

            }


            /// <summary>
            /// Load the specified vector file into Instrument Memory.
            /// Will automatically convert from .vec format as needed and load into instrument memory.
            /// </summary>
            /// <param name="fullPaths">A list of absolute paths to be loaded.  Currenlty only supports 1 item in the list.</param>
            /// <param name="nameInMemory">The name by which to load and execute the pattern.</param>
            /// <param name="datalogResults">Specifies if the pattern's results should be added to the datalog</param>
            /// <returns>True if pattern load succeeds.</returns>
            public bool LoadVector(List<string> fullPaths, string nameInMemory, bool datalogResults)
            {

                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    nameInMemory = nameInMemory.Replace("_", "");   //CM Edited

                    HSDIO.datalogResults[nameInMemory] = datalogResults;
                    bool isDigipat = fullPaths[0].ToUpper().EndsWith(".DIGIPAT");
                    bool isVec = fullPaths[0].ToUpper().EndsWith(".VEC");
                    bool notLoaded = !loadedPatternFiles.Contains(fullPaths[0] + nameInMemory.ToLower());

                    // If this is a digipat file and it hasn't already been loaded into instrument memory, load it
                    if (isDigipat && notLoaded)
                    {
                        DIGI.LoadPattern(fullPaths[0]);
                        loadedPatternFiles.Add(fullPaths[0] + nameInMemory.ToLower());
                        return true;
                    }                    
                    else
                    {
                        throw new Exception("Unknown File Format for " + nameInMemory);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load vector file:\n" + fullPaths[0] + "\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }           
            
            /// <summary>
            /// Generate and load pattern for setting all pins (SCLK, SDATA, VIO) to HiZ mode
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            public bool LoadVector_MipiHiZ()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {                    
                    int p = 0;
                    foreach (MIPIChanName m in allMipiChanNames)
                    {                        
                        // Generate vector that will set all pins to High Z for 8 clock cycles.
                        // This is done by not sourcing or comparing any data.  This works because the instrument is configured
                        // to be in the High Z termination mode in the 6570 init section.
                        string[] pins = new string[] { m.SclkChanName, m.SdataChanName, m.VioChanName, TrigChanName };
                        string[,] pattern = new string[,]
                        { 
                        #region HiZ pattern
                            {"", "0", "0", "0", "X", ""},
                            {"", "0", "0", "0", "X", ""},
                            {"", "0", "0", "0", "X", ""},
                            {"", "0", "0", "0", "X", ""},
                            {"", "0", "0", "0", "X", ""},
                            {"", "0", "0", "0", "X", ""},
                            {"", "0", "0", "0", "X", ""},
                            {"halt", "0", "0", "0", "X", ""}
                        #endregion
                        };

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern(HiZ.ToLower(), pins, pattern, forceDigiPatRegeneration, Timeset.MIPI))
                        {
                            throw new Exception("Compile Failed");
                        }

                        HSDIO.datalogResults[HiZ + "Pair" + p.ToString()] = false;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi HiZ vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Generate and load pattern for sending MIPI Reset
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            public bool LoadVector_MipiReset()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    // Generate MIPI / RFFE Reset Waveform
                    // Set VIO Pin to HiZ (remove VIO Pin from DUT) for 1/2 of the specified number of seconds,
                    // then return VIO to DUT as 0;
                    double secondsReset = 0.002;
                    int numLines = (int)(MIPIClockRate * secondsReset);
                    int p = 0;
                    foreach (MIPIChanName m in allMipiChanNames)
                    {
                        // Generate MIPI / RFFE Non-Extended Register Read Pattern
                        string[] pins = new string[] { m.SclkChanName, m.SdataChanName, m.VioChanName, TrigChanName };
                        string[,] pattern = new string[,]
                        { 
                            #region Reset pattern
                        {"repeat(" + ((MIPIClockRate * secondsReset) / 2) + ")", "0", "0", "0", "X", ""},
                        {"repeat(" + ((MIPIClockRate * secondsReset) / 2) + ")", "0", "0", "1", "X", ""},
                        {"halt", "0", "0", "1", "X", ""}
                            #endregion
                        };

                        // Generate and load Pattern from the formatted array.
                        if (!this.GenerateAndLoadPattern(Reset.ToLower(), pins, pattern, forceDigiPatRegeneration, Timeset.MIPI))
                        {
                            throw new Exception("Compile Failed");
                        }

                        HSDIO.datalogResults[Reset + "Pair" + p.ToString()] = false;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi Reset vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Generate and load all patterns necessary for Register Read and Write (including extended R/W)
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            public bool LoadVector_MipiRegIO()
            {
                return LoadVector_MipiRegRead() & LoadVector_MipiRegWrite() & LoadVector_MipiExtendedRegWrite() & LoadVector_MipiExtendedRegRead();
            }

            /// <summary>
            /// Internal Function: Used to generate and load the non-extended register read pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MipiRegRead()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    int p = 0;
                    foreach (MIPIChanName m in allMipiChanNames)
                    {
                        // Generate MIPI / RFFE Non-Extended Register Read Pattern
                        string[] pins = new string[] { m.SclkChanName, m.SdataChanName, m.VioChanName, TrigChanName };
                        List<string[]> pattern = new List<string[]>
                            {
                            #region RegisterRead pattern
                                new string[] {"source_start(SrcRegisterReadPair" + p.ToString () + ")", "0", "0", "1", "X", "Configure source"},
                                new string[] {"capture_start(CapRegisterReadPair"+ p.ToString() + ")", "0", "0", "1", "X", "Configure capture"},
                                new string[] {"repeat(3000)", "0", "0", "1", "X", "Idle"},
                                new string[] {"source", "0", "D", "1", "X", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Register Read Command (011)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Register Read Command (011)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Register Read Command (011)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "X", "Pull Down Only"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 7"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 6"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 5"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 4"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 3"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 2"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 1"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 0"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Parity"},
                                new string[] {"", "0", "X", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "X", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"capture_stop", "X", "X", "1", "X", ""},
                                new string[] {"repeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                        // Generate and load Pattern from the formatted array.
                        // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                        if (!this.GenerateAndLoadPattern("RegisterRead" + "Pair" + p.ToString(), pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("Compile Failed");
                        }

                        HSDIO.datalogResults["RegisterRead" + "Pair" + p.ToString()] = false;
                        p++;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi RegisterRead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the non-extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MipiRegWrite()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    int p = 0; // Index of MIPI pair
                    // Generate MIPI / RFFE Non-Extended Register Write Pattern
                    foreach (MIPIChanName m in allMipiChanNames)
                    {

                        string[] pins = new string[] { m.SclkChanName, m.SdataChanName, m.VioChanName, TrigChanName };
                        string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.Both ? "1" : "0");
                        List<string[]> patternStart = new List<string[]>
                            {
                            #region RegisterWrite Pattern
                                new string[] {"source_start(SrcRegisterWritePair" + p.ToString() + ")", "0", "0", "1", "0", "Configure source"},
                                new string[] {"repeat(5000)", "0", "0", "1", "0", "Idle"},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Register Write Command (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                        List<string[]> triggerDelay = new List<string[]>
                            {
                                new string[] { "", "0", "0", "1", "0", "Trigger Delay Cycle" }
                            };
                        List<string[]> trigger = new List<string[]>
                            {
                            #region Trigger, Idle Halt
                                new string[] {"jump_if(!seqflag0, endofpattern)", "0", "0", "1", "0", "Check if Trigger Required, if not, go to halt."},
                                new string[] {"set_signal(event0)", "0", "0", "1", trigval, "Turn On PXI Backplane Trigger if enabled. Send Digital Pin Trigger if enabled."},
                                new string[] {"repeat(49)", "0", "0", "1", trigval, "Continue Sending Digital Pin Trigger if enabled."},
                                new string[] {"clear_signal(event0)", "0", "0", "1", "0", "PXI Backplane Trigger Off (if enabled). Digital Pin Trigger Off."},
                                new string[] {"repeat(49)", "0", "0", "1", "0", "Digital Pin Trigger Off."},
                                new string[] {"", "0", "0", "1", "X", "Digital Pin Trigger Tristate."},
                                new string[] {"endofpattern:\nrepeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                        // Generate full pattern with the correct number of source / capture sections based on the 1-16 bit count
                        List<string[]> pattern = new List<string[]> { };
                        pattern = pattern.Concat(patternStart).ToList();

                        for (int ff = 0; ff < this.regWriteTriggerCycleDelay; ff++)
                            pattern = pattern.Concat(triggerDelay).ToList();

                        pattern = pattern.Concat(trigger).ToList();

                        // Generate and load Pattern from the formatted array.
                        // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                        if (!this.GenerateAndLoadPattern("RegisterWrite" + "Pair" + p.ToString(), pins, pattern, true, Timeset.MIPI))
                        {
                            throw new Exception("Compile Failed");
                        }

                        HSDIO.datalogResults["RegisterWrite" + "Pair" + p.ToString()] = false;
                        p++;
                    }                                      

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi RegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the extended register write pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MipiExtendedRegWrite()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    int p = 0; // index for MIPI pair
                    foreach (MIPIChanName m in allMipiChanNames)
                    {
                        // Generate MIPI / RFFE Extended Register Write Patterns
                        string[] pins = new string[] { m.SclkChanName, m.SdataChanName, m.VioChanName, TrigChanName };
                        string trigval = (triggerConfig == TrigConfig.Digital_Pin || triggerConfig == TrigConfig.Both ? "1" : "0");
                        List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterWrite Pattern
                                new string[] {"repeat(300)", "0", "0", "1", "0", "Idle"},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Extended Register Write Command (0000)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                        List<string[]> writeData = new List<string[]>
                            {
                            #region Write Data...
                                new string[] {"source", "1", "D", "1", "0", "Write Data 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Write Data 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "0", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };
                        List<string[]> busPark = new List<string[]>
                            {
                            #region Bus Park
                                new string[] {"", "1", "0", "1", "0", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                            #endregion
                            };
                        List<string[]> triggerDelay = new List<string[]>
                            {
                                new string[] { "", "0", "0", "1", "0", "Trigger Delay Cycle" }
                            };
                        List<string[]> trigger = new List<string[]>
                            {
                            #region Trigger, Idle Halt
                                new string[] {"jump_if(!seqflag0, endofpattern)", "0", "0", "1", "0", "Check if Trigger Required, if not, go to halt."},
                                new string[] {"set_signal(event0)", "0", "0", "1", trigval, "Turn On PXI Backplane Trigger if enabled. Send Digital Pin Trigger if enabled."},
                                new string[] {"repeat(49)", "0", "0", "1", trigval, "Continue Sending Digital Pin Trigger if enabled."},
                                new string[] {"clear_signal(event0)", "0", "0", "1", "0", "PXI Backplane Trigger Off (if enabled). Digital Pin Trigger Off."},
                                new string[] {"repeat(49)", "0", "0", "1", "0", "Digital Pin Trigger Off."},
                                new string[] {"", "0", "0", "1", "X", "Digital Pin Trigger Tristate."},
                                new string[] {"endofpattern:\nrepeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                        for (int i = 1; i <= 16; i++)
                        {
                            // Generate full pattern with the correct number of source / capture sections based on the 1-16 bit count
                            List<string[]> pattern = new List<string[]>
                                {
                                    new string[] {"source_start(SrcExtendedRegisterWrite" + i + "Pair" + p.ToString() + ")", "0", "0", "1", "0", "Configure source"}
                                };
                            pattern = pattern.Concat(patternStart).ToList();

                            for (int j = 0; j < i; j++)
                                pattern = pattern.Concat(writeData).ToList();

                            pattern = pattern.Concat(busPark).ToList();

                            for (int ff = 0; ff < this.regWriteTriggerCycleDelay; ff++)
                                pattern = pattern.Concat(triggerDelay).ToList();

                            pattern = pattern.Concat(trigger).ToList();

                            // Generate and load Pattern from the formatted array.
                            // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                            if (!this.GenerateAndLoadPattern("ExtendedRegisterWrite" + i.ToString() + "Pair" + p.ToString(), pins, pattern, true, Timeset.MIPI))
                            {
                                throw new Exception("Compile Failed: ExtendedRegisterWrite" + i.ToString() + "Pair" + p.ToString());
                            }
                            HSDIO.datalogResults["ExtendedRegisterWrite" + i.ToString() + "Pair" + p.ToString()] = false;
                        }
                        p++;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi ExtendedRegisterWrite vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            /// <summary>
            /// Internal Function: Used to generate and load the extended register read pattern
            /// </summary>
            /// <returns>True if pattern load succeeds.</returns>
            private bool LoadVector_MipiExtendedRegRead()
            {
                if (!usingMIPI)
                {
                    MessageBox.Show("Attempted to load HSDIO MIPI vector but HSDIO.Initialize wasn't called", "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                try
                {
                    int p = 0; // index for MIPI pair
                    foreach (MIPIChanName m in allMipiChanNames)
                    {
                        // Generate MIPI / RFFE Extended Register Read Patterns
                        string[] pins = new string[] { m.SclkChanName, m.SdataChanName, m.VioChanName, TrigChanName };
                        List<string[]> patternStart = new List<string[]>
                            {
                            #region ExtendedRegisterRead Pattern
                                new string[] {"repeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"source", "0", "D", "1", "X", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", ""},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "0", "D", "1", "X", "Command Frame (010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Slave Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Extended Register Read Command (0010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Extended Register Read Command (0010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Extended Register Read Command (0010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Extended Register Read Command (0010)"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Byte Count 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Byte Count 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Byte Count 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Byte Count 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 7"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 6"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 5"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 4"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 3"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 2"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 1"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Address 0"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"source", "1", "D", "1", "X", "Parity"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"", "1", "0", "1", "X", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""}
                                #endregion
                            };
                        List<string[]> readData = new List<string[]>
                            {
                            #region Read Data...
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 7"},
                                new string[] {"", "0", "X", "1", "X", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 6"},
                                new string[] {"", "0", "X", "1", "X", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 5"},
                                new string[] {"", "0", "X", "1", "X", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 4"},
                                new string[] {"", "0", "X", "1", "X", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 3"},
                                new string[] {"", "0", "X", "1", "X", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 2"},
                                new string[] {"", "0", "X", "1", "X", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 1"},
                                new string[] {"", "0", "X", "1", "X", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Read Data 0"},
                                new string[] {"", "0", "X", "1", "X", ""},
                                new string[] {"capture", "1", "V", "1", "X", "Parity"},
                                new string[] {"", "0", "X", "1", "X", ""}
                            #endregion
                            };
                        List<string[]> busParkIdleHalt = new List<string[]>
                            {
                            #region Bus Park, Idle, and Halt
                                new string[] {"", "1", "0", "1", "X", "Bus Park"},
                                new string[] {"", "0", "-", "1", "-", ""},
                                new string[] {"repeat(300)", "0", "0", "1", "X", "Idle"},
                                new string[] {"halt", "0", "0", "1", "X", ""}
                            #endregion
                            };

                        for (int i = 1; i <= 16; i++)
                        {
                            // Generate full pattern with the correct number of source / capture sections based on the 1-16 bit count
                            List<string[]> pattern = new List<string[]>
                                {
                                    new string[] {"source_start(SrcExtendedRegisterRead" + i + "Pair" + p.ToString() + ")", "0", "0", "1", "X", "Configure source"},
                                    new string[] {"capture_start(CapExtendedRegisterRead" + i + "Pair" + p.ToString() + ")", "0", "0", "1", "X", "Configure capture"}
                                };
                            pattern = pattern.Concat(patternStart).ToList();
                            for (int j = 0; j < i; j++)
                                pattern = pattern.Concat(readData).ToList();
                            pattern = pattern.Concat(busParkIdleHalt).ToList();

                            // Generate and load Pattern from the formatted array.
                            // Note: Regsiter read/write patterns are generated as Return to Zero Patterns
                            if (!this.GenerateAndLoadPattern("ExtendedRegisterRead" + i.ToString() + "Pair" + p.ToString(), pins, pattern, true, Timeset.MIPI))
                            {
                                throw new Exception("Compile Failed: ExtendedRegisterRead" + i.ToString() + "Pair" + p.ToString());
                            }
                            HSDIO.datalogResults["ExtendedRegisterRead" + i.ToString() + "Pair" + p.ToString()] = false;
                        }
                        p++;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to load Mipi ExtendedRegisterRead vector\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }



            


            /// <summary>
            /// Send the pattern requested by nameInMemory.
            /// If requesting the PID pattern, generate the signal and store the result for later processing by InterpretPID
            /// If requesting the TempSense pattern, generate the signal and store the result for later processing by InterpretTempSense
            /// </summary>
            /// <param name="nameInMemory">The requested pattern to generate</param>
            /// <returns>True if the pattern generated without bit errors</returns>
            public bool SendVector(string nameInMemory)
            {
                if (!usingMIPI) return true;

                try
                {
                    nameInMemory = nameInMemory.Replace("_", "");

                    if (nameInMemory.ToUpper() == "READPID")
                    {
                        // NOTE:  We only need to do one style read, both dynamic read and hard coded
                        //        pattern read can be done here to prove both work the same.

                        // Read PID using the dynamic RegRead command instead of the hard coded vector
                        //int dyanmicPIDRead = Convert.ToInt32(this.RegRead("1D"), 16);

                        // Read PID using the hard coded pattern instead of the dynamic RegRead command
                        pidval = this.SendPIDVector(nameInMemory);
                        Console.WriteLine("PID: " + pidval);

                        // Check if dynamic read matches hard coded read.
                        //return dyanmicPIDRead == pidval;
                        return true;
                    }
                    else
                    {
                        // This is not a special case such as PID or TempSense.
                        allRffePins.SelectedFunction = SelectedFunction.Digital;
                        allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                        // Select pattern to burst
                        // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                        DIGI.PatternControl.StartLabel = nameInMemory.ToLower();

                        // Send the normal pattern file and store the number of bit errors from the SDATA pin
                        DIGI.PatternControl.Initiate();

                        // Wait for Pattern Burst to complete
                        DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

                        // Get PassFail Results
                        bool[] passFail = DIGI.PatternControl.GetSitePassFail("");
                        Int64[] failureCount = sdataPin.GetFailCount();
                        NumExecErrors = (int)failureCount[0];
                        if (debug) Console.WriteLine("SendVector " + nameInMemory + " Bit Errors: " + NumExecErrors.ToString());

                        return passFail[0];
                    }
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            /// <summary>
            /// Loop through each pattern name in the MipiWaveformNames list and execute them
            /// </summary>
            /// <param name="firstTest">Unused</param>
            /// <param name="MipiWaveformNames">The list of pattern names to execute</param>
            public void SendNextVectors(bool firstTest, List<string> MipiWaveformNames)
            {
                try
                {
                    foreach (string nameInMemory in MipiWaveformNames)
                    {
                        SendVector(nameInMemory);
                    }                        
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            /// <summary>
            /// Returns the number of bit errors from the most recently executed pattern.
            /// </summary>
            /// <param name="nameInMemory">Not Used</param>
            /// <returns>Number of bit errors</returns>
            public int GetNumExecErrors(string nameInMemory)
            {
                Int64[] failureCount = sdataPin.GetFailCount();
                return (int)failureCount[0];
            }

            /// <summary>
            /// The PID Value was recorded and stored by the SendVector("ReadPID") function call
            /// This function returns the value that was previously recorded.
            /// </summary>
            /// <param name="nameInMemory">"ReadPID", ignored</param>
            /// <returns>PID Value recorded by SendVector("ReadPID")</returns>
            public int InterpretPID(string nameInMemory)
            {

                try
                {
                    // PID is stored in pidval variable when SendVector(ReadPID) is called
                    return (int)pidval;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }
            }


            /// <summary>
            /// This function sends the ReadPID pattern.  This is the standard .vec pattern.
            /// The 6570 uses capture memory to get the data from every H/L location in the original .vec
            /// </summary>
            /// <param name="nameInMemory"></param>
            /// <returns></returns>
            private uint SendPIDVector(string nameInMemory)
            {
                try
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allRffePins.SelectedFunction = SelectedFunction.Digital;
                    allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    // Local Variables
                    bool[] passFail = new bool[] { };
                    uint numBits = (captureCounters.ContainsKey(nameInMemory.ToUpper()) ? captureCounters[nameInMemory.ToUpper()] : 8);

                    // Create the capture waveform.
                    DIGI.CaptureWaveforms.CreateSerial(SdataPair0ChanName.ToUpper(), "Cap" + nameInMemory.ToLower(), numBits, BitOrder.MostSignificantBitFirst);

                    // Choose Pattern to Burst (ReadTempSense)
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    DIGI.PatternControl.StartLabel = nameInMemory.ToLower();

                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

                    // Get PassFail Results for site 0
                    passFail = DIGI.PatternControl.GetSitePassFail("");
                    Int64[] failureCount = sdataPin.GetFailCount();
                    NumExecErrors = (int)failureCount[0];
                    if (debug) Console.WriteLine("SendPIDVector " + nameInMemory + " Bit Errors: " + NumExecErrors.ToString());

                    // Retreive captured waveform, sample count is 1 byte of data
                    uint[][] data = new uint[1][];
                    DIGI.CaptureWaveforms.Fetch("", "Cap" + nameInMemory.ToLower(), 1, new TimeSpan(0, 0, 0, 0, 100), ref data);

                    // Return PID Value as read from DUT.  Remove Parity and Bus Park bits by shifting right by 2.
                    return data[0][0] >> 2;  //*//CHANGE 10-15-2015

                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to generate vector for " + nameInMemory + ".\nPlease ensure LoadVector() was called.\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 0;
                }
            }


            /// <summary>
            /// Dynamic Register Write function.  This uses NI 6570 source memory to dynamically change
            /// the register address and write values in the pattern.
            /// This supports extended and non-extended register write.
            /// </summary>
            /// <param name="pair">The MIPI pair number to write</param>
            /// <param name="slaveAddress_hex">The slave address to write (hex)</param>
            /// <param name="registerAddress_hex">The register address to write (hex)</param>
            /// <param name="data_hex">The data to write into the specified register in Hex.  Note:  Maximum # of bytes to write is 16.</param>
            /// <param name="sendTrigger">If True, this function will send either a PXI Backplane Trigger, a Digital Pin Trigger, or both based on the configuration in the NI6570 constructor.</param>
            public void RegWrite(int pair, string slaveAddress_hex, string registerAddress_hex, string data_hex, bool sendTrigger = false)
            {
                try
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allRffePins.SelectedFunction = SelectedFunction.Digital;
                    allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    if (sendTrigger)
                    {
                        // Configure the NI 6570 to connect PXI_TrigX to "event0" that can be used with the set_signal, clear_signal, and pulse_trigger opcodes
                        if (triggerConfig == TrigConfig.Both || triggerConfig == TrigConfig.PXI_Backplane)
                        {
                            DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", pxiTrigger.ToString("g"));
                        }
                        else
                        {
                            // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                            DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", "");
                        }

                        // Set the Sequencer Flag 0 to indicate that a trigger should be sent on the TrigChan pin
                        if (triggerConfig == TrigConfig.Both || triggerConfig == TrigConfig.Digital_Pin)
                        {
                            DIGI.PatternControl.WriteSequencerFlag("seqflag0", true);
                        }
                        else
                        {
                            // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                            DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);
                        }

                        if (triggerConfig == TrigConfig.None)
                        {
                            throw new Exception("sendTrigger=True requested, but NI 6570 is not configured for Triggering.  Please update the NI6570 Constructor triggerConfig to use TrigConfig.Digital_Pin, TrigConfig.PXI_Backplane, or TrigConfig.Both.");
                        }
                    }
                    else
                    {
                        // Disable "event0" triggering.  This turns off PXI Backplane Trigger Generation.
                        DIGI.ExportSignal(SignalType.PatternOpcodeEvent, "patternOpcodeEvent0", "");

                        // Disable the Sequencer Flag 0 triggering.  This turns off Digital Pin Trigger Generation.
                        DIGI.PatternControl.WriteSequencerFlag("seqflag0", false);
                    }

                    DIGI.PatternControl.Commit();

                    // Pad data_hex string to ensure it always has full bytes (eg: "0F" instead of "F")
                    if (data_hex.Length % 2 == 1)
                        data_hex = data_hex.PadLeft(data_hex.Length + 1, '0');

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    bool extendedWrite = Convert.ToInt32(registerAddress_hex, 16) > 31;    // any register address > 5 bits requires extended read
                    uint writeByteCount = extendedWrite ? (uint)(data_hex.Length / 2) : 1;
                    string nameInMemory = extendedWrite ? "ExtendedRegisterWrite" + writeByteCount.ToString() : "RegisterWrite";
                    nameInMemory += "Pair" + pair.ToString(); 

                    // Source buffer must contain 512 elements, even if sourcing less
                    uint[] dataArray = new uint[512];
                    if (!extendedWrite)
                    {
                        // Build non-exteded write command
                        uint cmdBytesWithParity = generateRFFECommand(slaveAddress_hex, registerAddress_hex, Command.REGISTERWRITE);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                        dataArray[0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[2] = calculateParity(Convert.ToUInt32(data_hex, 16)); // final 9 bits
                    }
                    else
                    {
                        // Build extended read command data, setting read byte count and register address. 
                        // Note, write byte count is 0 indexed.
                        uint cmdBytesWithParity = generateRFFECommand(slaveAddress_hex, Convert.ToString(writeByteCount - 1, 16), Command.EXTENDEDREGISTERWRITE);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                        dataArray[0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[2] = calculateParity(Convert.ToUInt32(registerAddress_hex, 16)); // address 9 bits
                        // Convert Hex Data string to bytes and add to data Array
                        for (int i = 0; i < writeByteCount * 2; i += 2)
                            dataArray[3 + (i / 2)] = (uint)(calculateParity(Convert.ToByte(data_hex.Substring(i, 2), 16)));
                    }

                    // Configure 6570 to source data calculated above
                    DIGI.SourceWaveforms.CreateSerial(allMipiChanNames[pair].SdataChanName, "Src" + nameInMemory, SourceDataMapping.Broadcast, 9, BitOrder.MostSignificantBitFirst);
                    DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);

                    // Choose Pattern to Burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    DIGI.PatternControl.StartLabel = nameInMemory;

                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

                    // Get PassFail Results for site 0
                    Int64[] failureCount = sdataPin.GetFailCount();
                    NumExecErrors = (int)failureCount[0];
                    if (debug) Console.WriteLine("Pair " + pair + " Slave " + slaveAddress_hex + ", RegWrite " + registerAddress_hex + " Bit Errors: " + NumExecErrors.ToString());
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Write Register for Address " + registerAddress_hex + ".\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            /// <summary>
            /// Dynamic Register Read function.  This uses NI 6570 source memory to dynamically change
            /// the register address and uses NI 6570 capture memory to receive the values from the DUT.
            /// This supports extended and non-extended register read.
            /// </summary>
            /// <param name="pair">The MIPI pair number to write</param>
            /// <param name="slaveAddress_hex">The slave address to write (hex)</param>
            /// <param name="registerAddress_hex">The register address to read (hex)</param>
            /// <returns>The value of the specified register in Hex</returns>
            public string RegRead(int pair, string slaveAddress_hex, string registerAddress_hex)
            {
                try
                {
                    // Make sure the 6570 is in the correct mode in case we were previously in PPMU mode.
                    allRffePins.SelectedFunction = SelectedFunction.Digital;
                    allRffePins.DigitalLevels.TerminationMode = TerminationMode.HighZ;

                    // Burst pattern & check for Pass/Fail
                    bool[] passFail = new bool[] { };
                    bool extendedRead = Convert.ToInt32(registerAddress_hex, 16) > 31;    // any register address > 5 bits requires extended read
                    uint readByteCount = 1;
                    string nameInMemory = extendedRead ? "ExtendedRegisterRead" + readByteCount.ToString() : "RegisterRead";
                    nameInMemory += "Pair" + pair.ToString(); 

                    uint[] dataArray = new uint[512];
                    // Source buffer must contain 512 elements, even if sourcing less
                    if (!extendedRead)
                    {
                        // Build non-extended read command data
                        uint cmdBytesWithParity = generateRFFECommand(slaveAddress_hex, registerAddress_hex, Command.REGISTERREAD);
                        // Split data into array of data, all must be same # of bits (16) which must be specified when calling CreateSerial 
                        dataArray[0] = cmdBytesWithParity;
                    }
                    else
                    {
                        // Build extended read command data, setting read byte count and register address.
                        // Note, read byte count is 0 indexed.
                        uint cmdBytesWithParity = generateRFFECommand(slaveAddress_hex, Convert.ToString(readByteCount - 1, 16), Command.EXTENDEDREGISTERREAD);
                        // Split data into array of data, all must be same # of bits (9) which must be specified when calling CreateSerial 
                        dataArray[0] = cmdBytesWithParity >> 9; // first 8 bits (plus one extra 0 so this packet is also 9 bits)
                        dataArray[1] = cmdBytesWithParity & 0x1FF; // 2nd 9 bits
                        dataArray[2] = (uint)(calculateParity(Convert.ToUInt16(registerAddress_hex, 16)));  // Final 9 bits to contains the address (for extended read) + parity.
                    }

                    // Configure to source data
                    DIGI.SourceWaveforms.CreateSerial(allMipiChanNames[pair].SdataChanName, "Src" + nameInMemory, SourceDataMapping.Broadcast, (uint)(extendedRead ? 9 : 16), BitOrder.MostSignificantBitFirst);                    
                    DIGI.SourceWaveforms.WriteBroadcast("Src" + nameInMemory, dataArray);

                    // Configure to capture 8 bits (Ignore Parity)
                    DIGI.CaptureWaveforms.CreateSerial(allMipiChanNames[pair].SdataChanName, "Cap" + nameInMemory, readByteCount * 9, BitOrder.MostSignificantBitFirst);

                    // Choose Pattern to Burst
                    // Always use all lower case names for start labels due to known issue (some uppercase labels not valid)
                    DIGI.PatternControl.StartLabel = nameInMemory; 

                    // Burst Pattern
                    DIGI.PatternControl.Initiate();

                    // Wait for Pattern Burst to complete
                    DIGI.PatternControl.WaitUntilDone(new TimeSpan(0, 0, 10));

                    // Get PassFail Results for site 0
                    passFail = DIGI.PatternControl.GetSitePassFail("");
                    Int64[] failureCount = sdataPin.GetFailCount();
                    NumExecErrors = (int)failureCount[0];
                    if (debug) Console.WriteLine("Pair " + pair + " Slave " + slaveAddress_hex + ", RegRead " + registerAddress_hex + " Bit Errors: " + NumExecErrors.ToString());

                    // Retreive captured waveform
                    uint[][] capData = new uint[1][];
                    DIGI.CaptureWaveforms.Fetch("", "Cap" + nameInMemory, 1, new TimeSpan(0, 0, 0, 0, 100), ref capData);

                    // Remove the parity bit 
                    capData[0][0] = (capData[0][0] >> 1) & 0xFF;

                    // Convert captured data to hex string and return
                    string returnval = capData[0][0].ToString("X");
                    if (debug) Console.WriteLine("Slave " + slaveAddress_hex +", ReadReg " + registerAddress_hex + ": " + returnval);
                    return returnval;
                }
                catch (Exception e)
                {
                    DIGI.PatternControl.Abort();
                    MessageBox.Show("Failed to Read Register for Address " + registerAddress_hex + ".\n\n" + e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "";
                }
            }


            public void ConfigureSetting(bool script, string TestMode)
            {
            }

            public void SendTRIGVectors()
            {
                if (!usingMIPI) return;
                try
                {
                    // We will use sequencer flag 3 to send "script triggers" to the pattern.  The pattern will do a jump_cond(!seqflag3, <here>) to wait for this to be set in SW.
                    DIGI.PatternControl.WriteSequencerFlag("seqflag3", true);                   
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "HSDIO MIPI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            /// <summary>
            /// Close the NI 6570 session when shutting down the application
            /// and ensure all patterns are unloaded and all channels are disconnected.
            /// </summary>
            public void Close()
            {
                allRffePins.SelectedFunction = SelectedFunction.Disconnect;
                DIGI.Dispose();
            }

            private enum eSWREG
            {
                SWREG01, SWREG02, SWREG03, SWREG05, SWREG06, SWREG08, SWREG09,
                SWREG10, SWREG20, SWREG30, SWREG50, SWREG60, SWREG80, SWREG90,
            }
			// [Burhan]
            public int Read_AutoCal_ID()
            {
                shmoo("functional");
                return 0;
            }

            #region Avago SJC Specific Helper Functions

            /// <summary>
            /// NI Internal Function:  Generate the requested RFFE command
            /// </summary>
            /// <param name="registerAddress_hex_or_ByteCount">For non-extended read / write, this is the register address.  For extended read / write, this is the number of bytes to read.</param>
            /// <param name="instruction">EXTENDEDREGISTERWRITE, EXTENDEDREGISTERREAD, REGISTERWRITE, or REGISTERREAD</param>
            /// <returns>The RFFE Command + Parity</returns>
            private uint generateRFFECommand(string slaveAddress_hex, string registerAddress_hex_or_ByteCount, Command instruction)
            {
                int slaveAddress = (Convert.ToByte(slaveAddress_hex, 16)) << 8;
                int commandFrame = 1 << 14;
                Byte regAddress = Convert.ToByte(registerAddress_hex_or_ByteCount, 16);

                Byte maxRange = 0, modifiedAddress = 0;

                switch (instruction)
                {
                    case Command.EXTENDEDREGISTERWRITE:
                        maxRange = 0x0F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x00);
                        break;
                    case Command.EXTENDEDREGISTERREAD:
                        maxRange = 0x0F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x20);
                        break;
                    case Command.REGISTERWRITE:
                        maxRange = 0x1F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x40);
                        break;
                    case Command.REGISTERREAD:
                        maxRange = 0x1F;
                        modifiedAddress = Convert.ToByte(maxRange & regAddress | 0x60);
                        break;
                    default:
                        maxRange = 0x0F;
                        modifiedAddress = regAddress;
                        break;
                }

                if (regAddress != (maxRange & regAddress))
                    throw new Exception("Address out of range for requested command");

                // combine command frame, slave address, and modifiedAddress which contains the command and register address
                uint cmd = calculateParity((uint)(slaveAddress | modifiedAddress));
                cmd = (uint)(commandFrame) | cmd;
                return cmd;
            }

            /// <summary>
            /// NI Internal Function: Computes and appends parity 
            /// </summary>
            /// <param name="cmdWithoutParity"></param>
            /// <returns></returns>
            private uint calculateParity(uint cmdWithoutParity)
            {
                int x = (int)cmdWithoutParity;
                x ^= x >> 16;
                x ^= x >> 8;
                x ^= x >> 4;
                x &= 0x0F;
                bool parity = ((0x6996 >> x) & 1) != 0;
                return (uint)(cmdWithoutParity << 1 | Convert.ToByte(!parity));
            }

            /// <summary>
            /// Create a .digipatsrc file from the given inputs and compile into a .digipat file.
            /// Once compilation of the .digipat succeeds, load the pattern into instrument memory.
            /// </summary>
            /// <param name="patternName">The pattern name or "nameInMemory" used to execute this pattern later in the program</param>
            /// <param name="pinList">The pins associated with this pattern.  These must match the timeset.  For NRZ patterns, the timeset is "MIPI"; otherwise the timeset is "MIPI_SCLK_RZ"</param>
            /// <param name="pattern">The pattern specified by a 2-d array of strings, one column per pin, columns must correspond to pinList array.</param>
            /// <param name="overwrite">If a compiled .digipat already exists for this .vec and this is TRUE, re-compile and overwrite the original .digipat regardless of if the .vec has changed.  If FALSE, use the pre-existing .digipat if the .vec has not changed or create if it doesn't exist.</param>
            /// <param name="timeSet">Specify if this pattern should use the MIPI or the MIPI_SCLK_RZ timeset</param>
            /// <returns>True if pattern compilation and loading to instrument memory succeeds.</returns>
            public bool GenerateAndLoadPattern(string patternName, string[] pinList, string[,] pattern, bool overwrite, Timeset timeSet)
            {
                // Convert from string[,] into slightly better List<string[]>
                List<string[]> newPattern = new List<string[]>(pattern.GetLength(0));
                for (int x = 0; x < pattern.GetLength(0); x++)
                {
                    string[] tmp = new string[pattern.GetLength(1)];
                    for (int y = 0; y < pattern.GetLength(1); y++)
                        tmp[y] = pattern[x, y];
                    newPattern.Add(tmp);
                }
                return GenerateAndLoadPattern(patternName, pinList, newPattern, overwrite, timeSet);
            }

            /// <summary>
            /// Create a .digipatsrc file from the given inputs and compile into a .digipat file.
            /// Once compilation of the .digipat succeeds, load the pattern into instrument memory.
            /// </summary>
            /// <param name="patternName">The pattern name or "nameInMemory" used to execute this pattern later in the program</param>
            /// <param name="pinList">The pins associated with this pattern.  These must match the timeset.  For NRZ patterns, the timeset is "MIPI"; otherwise the timeset is "MIPI_SCLK_RZ"</param>
            /// <param name="pattern">The pattern specified by a list of string arrays, one list item containing a 1-d array of string for each line in the vector.  1-D array must correspond to pinList array.</param>
            /// <param name="overwrite">If a compiled .digipat already exists for this .vec and this is TRUE, re-compile and overwrite the original .digipat regardless of if the .vec has changed.  If FALSE, use the pre-existing .digipat if the .vec has not changed or create if it doesn't exist.</param>
            /// <param name="timeSet">Specify if this pattern should use the MIPI or the MIPI_SCLK_RZ timeset</param>
            /// <returns>True if pattern compilation and loading to instrument memory succeeds.</returns>
            public bool GenerateAndLoadPattern(string patternName, string[] vecpins, List<string[]> pattern, bool overwrite, Timeset timeSet)
            {
                #region Generate Paths & Constants
                patternName = patternName.Replace("_", "");
                string patternSavePath = fileDir + "\\" + patternName + ".digipat";
                string digipatsrcPath = Path.ChangeExtension(patternSavePath, "digipatsrc");
                string digipatPath = Path.ChangeExtension(patternSavePath, "digipat");
                #endregion

                #region Check if files exist and handle appropriately
                // Check if digipatsrc exists, and if so, check to see if the .vec checksum has changed
                // If the checksum has changed, set overwrite to true so that we force the regeneration
                // instead of loading a stale digipat file
                if (File.Exists(digipatsrcPath) && !overwrite)
                {
                    System.IO.StreamReader digipatsrcFileMD5 = new System.IO.StreamReader(digipatsrcPath);
                    try
                    {
                        string MD5 = digipatsrcFileMD5.ReadLine().Substring("// VECMD5: ".Length);
                        overwrite = MD5 != ComputeMD5Hash(pattern);
                    }
                    catch
                    {
                        overwrite = true;
                    }
                    digipatsrcFileMD5.Close();
                }

                if (File.Exists(digipatPath))
                {
                    if (overwrite)
                    {
#if NIDEEPDEBUG
                    Console.WriteLine("Overwriting previously compiled .digipat");
#endif
                        File.Delete(digipatPath);
                        if (File.Exists(Path.ChangeExtension(digipatPath, "digipat_index")))
                            File.Delete(Path.ChangeExtension(digipatPath, "digipat_index"));
                    }
                    else
                    {
                        // Compiled digipat already exists, just load it
                        DIGI.LoadPattern(digipatPath);
                        return true;
                    }
                }
                if (File.Exists(digipatsrcPath))
                {
                    // Delete digipatsrc file if it already exists, do this after digipat check (don't delete src if digipat already exists)
                    File.Delete(digipatsrcPath);
                }
                #endregion

                #region Generate .digipatsrc

                #region Open digipatsrc File
                System.IO.StreamWriter digipatsrcFile = new System.IO.StreamWriter(digipatsrcPath);
                #endregion

                #region Write Header
                digipatsrcFile.WriteLine("// VECMD5: " + ComputeMD5Hash(pattern));
                digipatsrcFile.WriteLine("// National Instruments Digital Pattern Text File.");
                digipatsrcFile.WriteLine("// Automatically Generated from the GenerateAndLoadPattern function.");
                digipatsrcFile.WriteLine("// Pattern Name: " + patternName);
                digipatsrcFile.WriteLine("// Generated Date: " + System.DateTime.Now.ToString());
                digipatsrcFile.WriteLine("//\n");
                digipatsrcFile.WriteLine("file_format_version 1.0;");
                digipatsrcFile.WriteLine("timeset " + timeSet.ToString("g") + ";");
                digipatsrcFile.Write("\n");
                #endregion

                #region Loop through vectors and store in digipatsrc File

                // Write start of pattern, line contains comma separated pin names
                string pinlist = string.Join(",", this.allDutPins).ToUpper();
                digipatsrcFile.WriteLine("pattern " + patternName + "(" + pinlist + ")");
                digipatsrcFile.WriteLine("{");

                // Write all vector lines
                foreach (string[] lineData in pattern)
                {
                    // Add Timeset and opcode at the start
                    string lineOutput = lineData[0] + "\t" + timeSet.ToString("g");
                    foreach (string pin in this.allDutPins)
                    {
                        if (vecpins.Contains(pin.ToUpper()))
                        {
                            lineOutput += "\t" + lineData[Array.IndexOf(vecpins, pin.ToUpper()) + 1];
                        }
                        else
                        {
                            lineOutput += "\t-";
                        }
                    }
                    // Handle Comment, it is always the last item in the string array
                    if (lineData[lineData.Count() - 1] != "")
                        lineOutput += @"; // " + lineData[lineData.Count() - 1] + "\n";
                    else
                        lineOutput += ";\n";

                    digipatsrcFile.Write(lineOutput);
                }

                // Close out pattern
                digipatsrcFile.WriteLine("}\n");
                #endregion

                #region close text digipatsrc File
                digipatsrcFile.Close();
                #endregion
                #endregion

                return this.CompileDigipatSrc(digipatsrcPath, digipatPath, patternName, this.allDutPins, true);
            }

            /// <summary>
            /// NI Internal Function:  Given a digipatsrc file, compile and save into the given digipat file, using
            /// the specified patternName and Pins.  Generate Paths, Check if compiler exists and handle appropriately, Create a dummy pinmap containing the specified pins (pinmap required by compiler), then compile the digipatsrc into digipat.
            /// </summary>
            /// <param name="digipatsrcPath">The Absolute path to the digipatsrc file</param>
            /// <param name="digipatPath">The Absolute path to the desired digipat file output</param>
            /// <param name="patternName">The name of the pattern, used later to load the file into memory</param>
            /// <param name="pins">The pins in the pattern file</param>
            /// <param name="addTrig">If True, this indicates that an extra trigger channel was added, but the pins array doesn't contain it so we should add it during compile</param>
            /// <param name="load">If True, this function will automatically load the pattern into instrument memory after a successful compile</param>
            /// <returns>True if compilation and loading succeeds</returns>
            private bool CompileDigipatSrc(string digipatsrcPath, string digipatPath, string patternName, string[] pins, bool load = true)
            {
                #region Generate Paths
                string compilerPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\National Instruments\\Digital Pattern Compiler\\DigitalPatternCompiler.exe";
                string pinmapPath = fileDir + "\\compiler.pinmap";
                #endregion

                #region Check if compiler exists and handle appropriately
                if (!File.Exists(compilerPath))
                {
                    // Compiler not found, can't proceed
                    throw new FileNotFoundException("Digital Pattern Compiler Not Found", compilerPath);
                }
                #endregion

                #region Constants
                patternName = patternName.Replace("_", "");
                string pinmapHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<PinMap schemaVersion=\"1.1\" xmlns=\"http://www.ni.com/TestStand/SemiconductorModule/PinMap.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n	<Instruments>\n		<NIDigitalPatternInstrument name=\"NI6570\" numberOfChannels=\"32\" />\n	</Instruments>\n	<Pins>\n";
                string pinmapMiddle = "\n	</Pins>\n	<PinGroups>\n	</PinGroups>\n	<Sites>\n		<Site siteNumber=\"0\" />\n	</Sites>\n	<Connections>\n";
                string pinmapFooter = "\n	</Connections>\n</PinMap>";
                #endregion

#if NIDEEPDEBUG
            Console.WriteLine("Compiling from .digipatsrc to .digipat");
#endif
                #region Create dummy pinmap to be used by compiler
                if (File.Exists(pinmapPath)) { File.Delete(pinmapPath); }
                System.IO.StreamWriter pinmapFile = new StreamWriter(pinmapPath);
                pinmapFile.Write(pinmapHeader);
                foreach (string pin in pins)
                {
                    pinmapFile.WriteLine("<DUTPin name=\"" + pin + "\" />");
                }
                
                pinmapFile.Write(pinmapMiddle);

                foreach (string pin in pins)
                {
                    pinmapFile.WriteLine("<Connection pin=\"" + pin + "\" siteNumber=\"0\" instrument=\"NI6570\" channel=\"" + PinNamesAndChans[pin] + "\" />");
                }
                
                pinmapFile.Write(pinmapFooter);
                pinmapFile.Close();
                #endregion

                #region Run Compiler
                // Setup Process
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                // Run Digital Pattern Compiler located at compilerPath
                startInfo.FileName = compilerPath;
                // Pass in the pinmap, compiled digipat path, and text digipatsrc paths; escape spaces properly for cmd line execution
                startInfo.Arguments = " -pinmap " + pinmapPath.Replace(" ", @"^ ") + " -o " + digipatPath.Replace(" ", @"^ ") + " " + digipatsrcPath.Replace(" ", @"^ ");
                // Run Process
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

#if NIDEEPDEBUG
            Console.WriteLine("Compilation " + (process.ExitCode == 0 ? "Succeeded.  Loading Pattern to Instrument Memory." : "Failed"));
#endif
                // Delete Temporary Pinmap
                //File.Delete(pinmapPath);
                #endregion

                #region Load Pattern to Instrument Memory
                // Check if process exited without error and return status.
                if (process.ExitCode == 0)
                {
                    // Compilation completed without error, try loading pattern now.
                    try
                    {
                        if (load)
                        {
                            DIGI.LoadPattern(digipatPath);
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                else
                {
                    return false;
                }
                #endregion
            }

            /// <summary>
            /// NI Internal Function:  Compute the MD5 Hash of any file.
            /// </summary>
            /// <param name="filePath">The absolute path of the file for which to comput the MD5 Hash</param>
            /// <returns>The computed MD5 Hash String</returns>
            private string ComputeMD5Hash(string filePath)
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    return BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(filePath))) + "_" + this.version.ToString();
                }
            }
            /// <summary>
            /// NI Internal Function:  Compute the MD5 Hash of any file.
            /// </summary>
            /// <param name="pattern">The List of String Arrays representing a Pattern in memory</param>
            /// <returns>The computed MD5 Hash String</returns>
            private string ComputeMD5Hash(List<string[]> pattern)
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    string flattenedPattern = "";
                    foreach (var line in pattern.ToArray())
                    {
                        flattenedPattern += string.Join(",", line);
                    }
                    return BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(flattenedPattern))) + "_" + this.version.ToString();
                }
            }
            #endregion


            #region Avago SJC Specific Enums
            /// <summary>
            /// Used to specify which timeset is used for a specified pattern.
            /// Get the string representation using Timeset.MIPI.ToString("g");
            /// </summary>
            public enum Timeset
            {
                MIPI,
                MIPI_SCLK_RZ,
                EEPROM,
                TEMPSENSE
            };

            public enum TrigConfig
            {
                PXI_Backplane,
                Digital_Pin,
                Both,
                None
            }

            public enum PXI_Trig
            {
                PXI_Trig0,
                PXI_Trig1,
                PXI_Trig2,
                PXI_Trig3,
                PXI_Trig4,
                PXI_Trig5,
                PXI_Trig6,
                PXI_Trig7
            }

            /// <summary>
            /// NI Internal Enum:  Used to select which command for which to generate and RFFE packet
            /// </summary>
            private enum Command
            {
                EXTENDEDREGISTERWRITE,
                EXTENDEDREGISTERREAD,
                REGISTERWRITE,
                REGISTERREAD
            };

            private System.Version version = new System.Version(1, 0, 1215, 1);
            #endregion
        }        
        


    }

}