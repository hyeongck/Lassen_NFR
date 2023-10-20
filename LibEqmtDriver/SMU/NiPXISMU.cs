using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using NationalInstruments.ModularInstruments.Interop;

namespace LibEqmtDriver.SMU
{
    public class NiPXISMU : iPowerSupply
    {

        public NiPXISMU(string[] val)
        {
            //Note : val data must be in this format Px_CHy_NIxxxx (eg P1_CH1_NI4143) , will decode the NI SMU aliasname (eg. NI4143_P1)
            //Where Px = part SMU aliasname
            //Where CHx = which SMU channel to set (eg NI4143 has 4x CH)
            //Where NIxxxx = which SMU model

            string tempVal = "";

            for (int i = 0; i < val.Length; i++)
            {
                string visaAlias;
                string chNum;
                string pinName;

                string[] arSelected = new string[4];
                tempVal = val[i];
                arSelected = tempVal.Split('_');

                visaAlias = arSelected[3];
                pinName = tempVal;
                chNum = arSelected[1].Substring(2, 1);

                getSMU(visaAlias, chNum, pinName, true);
            }  
        }
        ~NiPXISMU() { }

        
        public static Dictionary<string, iSmu> SmuResources = new Dictionary<string, iSmu>();

        public static iSmu getSMU(string VisaAlias, string ChanNumber, string PinName, bool Reset)
        {
            iSmu smu;

            if (VisaAlias.Contains("4154"))
            {
                smu = new NI4154(VisaAlias, ChanNumber, PinName, Reset);
            }
            else if (VisaAlias.Contains("4139"))
            {
                smu = new NI4139(VisaAlias, ChanNumber, PinName, Reset);
            }
            else if (VisaAlias.Contains("4143") || VisaAlias.Contains("4141"))
            {
                smu = new NI4143(VisaAlias, ChanNumber, PinName, Reset);
            }
            else
            {
                throw new Exception("Visa Alias \"" + VisaAlias + "\" is not in a recognized format.\nValid SMU Visa Aliases must include one of the following:\n"
                    + "\n\"4154\""
                    + "\n\"4139\""
                    + "\n\"4143\""
                    + "\n\"4141\""
                    + "\n\nFor example, Visa Alias \"SMU_NI4143_02\" will be recognized as an NI 4143 module.");
            }

            SmuResources.Add(PinName, smu);

            return smu;
        }

        #region iPowerSupply

        void iPowerSupply.Init()
        {
            throw new NotImplementedException();
        }

        void iPowerSupply.DcOn(string strSelection, ePSupply_Channel Channel)
        {
            SmuResources[strSelection].OutputEnable(true);
        }

        void iPowerSupply.DcOff(string strSelection, ePSupply_Channel Channel)
        {
            //SmuResources[strSelection].ForceVoltage(0.0, 1e-6);      //force voltage to 0V and very small current (cannot be zero)
            SmuResources[strSelection].OutputEnable(false);
        }

        void iPowerSupply.SetNPLC(string strSelection, ePSupply_Channel Channel, float val)
        {
            
        }

        void iPowerSupply.SetVolt(string strSelection, ePSupply_Channel Channel, double Volt, double iLimit, ePSupply_VRange VRange)
        {
            Volt = Math.Round(Volt, 5); //Seoul
            iLimit = Math.Round(iLimit, 5); //Seoul
            SmuResources[strSelection].ForceVoltage(Volt, iLimit);
        }

        float iPowerSupply.MeasI(string strSelection, ePSupply_Channel Channel, ePSupply_IRange IRange)
        {
            double imeas = -999;
            SmuResources[strSelection].SetupCurrentMeasure(false, false);
            SmuResources[strSelection].MeasureCurrent(1, false, ref imeas);
            return Convert.ToSingle(imeas);
        }

        float iPowerSupply.MeasV(string strSelection, ePSupply_Channel Channel, ePSupply_VRange VRange)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    #region iSMU class
    public class NI4154 : iSmu
    {
        public nidcpower SMUsession;
        public string VisaAlias { get; set; }
        public string ChanNumber { get; set; }
        public string PinName { get; set; }
        public double priorVoltage { get; set; }
        public double priorCurrentLim { get; set; }

        public NI4154(string VisaAlias, string ChanNumber, string PinName, bool Reset)
        {
            try
            {
                this.SMUsession = new NationalInstruments.ModularInstruments.Interop.nidcpower(VisaAlias, ChanNumber, Reset, "");

                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName;

                string model = SMUsession.GetString(NationalInstruments.ModularInstruments.Interop.nidcpowerProperties.InstrumentModel);

                SMUsession.SetDouble(nidcpowerProperties.SourceDelay, ChanNumber, 0.00003);
                SMUsession.ConfigureOutputEnabled(ChanNumber, false);
                SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcVoltage);
                SMUsession.ConfigureSense(ChanNumber, nidcpowerConstants.Remote);
                SMUsession.SetInt32(nidcpowerProperties.CurrentLimitAutorange, ChanNumber, nidcpowerConstants.On);
                SMUsession.SetInt32(nidcpowerProperties.VoltageLevelAutorange, ChanNumber, nidcpowerConstants.On);
                SMUsession.ConfigureVoltageLevel(ChanNumber, 0);
                SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, 0.01);
                SMUsession.ConfigureOutputEnabled(ChanNumber, true);
                SMUsession.Initiate();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SMU Initialize");
            }
        }

        public void ForceVoltage(double voltsForce, double currentLimit)
        {
            int error = -1;
            int chanInt = Convert.ToInt16(ChanNumber);

            try
            {
                if (currentLimit != priorCurrentLim)
                {
                    // ConfigureCurrentLimit appears to also set the range automatically, because auto-range is on
                    error = SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, currentLimit);
                    priorCurrentLim = currentLimit;

                    //double readRange = SMUsession.GetDouble(nidcpowerProperties.CurrentLimit);
                }

                if (voltsForce != priorVoltage)
                {
                    error = SMUsession.ConfigureVoltageLevel(ChanNumber, voltsForce);
                    priorVoltage = voltsForce;
                }


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ForceVoltage4154");
            }
        }

        public void SetupCurrentMeasure(bool UseIQsettings, bool triggerFromSA)
        {
            try
            {
                SMUsession.Abort();

                double measureTimeLength = 0;


                measureTimeLength = 0.0005;
                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);
                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, 1);


                double dcSampleRate = 200e3;   // this is fixed for NI hardware
                int SamplesToAvg = (int)(dcSampleRate * measureTimeLength);

                SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, SamplesToAvg);

                SMUsession.Initiate();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupCurrentMeasure4154");

            }
        }
        public void MeasureCurrent(int NumAverages, bool triggered, ref double Result)
        {
            int error = -1;

            try
            {
                double[] volt = new double[NumAverages];
                double[] curr = new double[NumAverages];
                ushort[] measComp = new ushort[NumAverages];
                int actCount = 0;
                double[] voltSingle = new double[1];
                double[] currSingle = new double[1];

                for (int avg = 0; avg < NumAverages; avg++)
                {
                    if (triggered)
                    {
                        error = SMUsession.FetchMultiple(ChanNumber, 1, 1, voltSingle, currSingle, measComp, out actCount);  // "Count" doesn't wait to re-trigger, just measures all immediately after 1st trigger

                        volt[avg] = voltSingle[0];
                        curr[avg] = currSingle[0];
                    }
                    else
                    {
                        error = SMUsession.Measure(ChanNumber, nidcpowerConstants.MeasureCurrent, out curr[avg]);
                    }
                }

                Result = curr.Average();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureCurrent4154");

            }
        }

        int NumTraceSamples;
        public void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, bool triggered, bool triggerFromSA = false)
        {
            int error = -1;

            try
            {
                SMUsession.Abort();

                double dcSampleRate = 200e3;   // this is fixed for NI hardware
                NumTraceSamples = (int)(dcSampleRate * measureTimeLength);

                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);

                if (triggered)
                {
                    SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);
                    if (triggerFromSA) SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "PXI_Trig1");
                    else SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "PXI_Trig0");
                }
                else
                {
                    SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.SoftwareEdge);
                }

                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, NumTraceSamples);
                SMUsession.SetDouble(nidcpowerProperties.SourceDelay, ChanNumber, 0.001);

                SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, 1);

                SMUsession.Initiate();

                SMUsession.WaitForEvent(nidcpowerConstants.SourceCompleteEvent, 0.02);

                if (!triggered) SMUsession.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupCurrentTraceMeasurement4154");

            }
        }
        public double[] MeasureCurrentTrace()
        {
            int error = -1;

            try
            {
                ushort[] measComp = new ushort[NumTraceSamples];
                int actCount = 0;
                double[] voltSingle = new double[NumTraceSamples];
                double[] currSingle = new double[NumTraceSamples];

                error = SMUsession.FetchMultiple(ChanNumber, 1, NumTraceSamples, voltSingle, currSingle, measComp, out actCount);  // "Count" doesn't wait to re-trigger, just measures all immediately after 1st trigger

                return currSingle;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureCurrentTrace4154");
                return new double[4];
            }
        }

        public void SetupVoltageMeasure()
        {
            int error = -1;

            try
            {
                SMUsession.Abort();

                double measureTimeLength = 0.001;

                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);
                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, 1);

                double dcSampleRate = 200e3;   // this is fixed for NI hardware
                int SamplesToAvg = (int)(dcSampleRate * measureTimeLength);

                SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, SamplesToAvg);

                SMUsession.Initiate();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupVoltageMeasure4154");

            }
        }
        public void MeasureVoltage(int NumAverages, ref double Result)
        {
            int error = -1;

            try
            {
                double[] volts = new double[NumAverages];

                for (int avg = 0; avg < NumAverages; avg++)
                {
                    error = SMUsession.Measure(ChanNumber, nidcpowerConstants.MeasureVoltage, out volts[avg]);
                }

                Result = volts.Average();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureVoltage4154");

            }
        }

        public void SetupContinuity(double currentForce)
        {
            throw new NotImplementedException();
        }

        public void MeasureContinuity(int avgs, ref double result)
        {
            throw new NotImplementedException();
        }

        public void OutputEnable(bool state)
        {
            SMUsession.ConfigureOutputEnabled(ChanNumber, state);
        }

    }

    public class NI4143 : iSmu
    {
        public nidcpower SMUsession;
        public string VisaAlias { get; set; }
        public string ChanNumber { get; set; }
        public string PinName { get; set; }
        public double priorVoltage { get; set; }
        public double priorCurrentLim { get; set; }

        public NI4143(string VisaAlias, string ChanNumber, string PinName, bool Reset)
        {
            try
            {
                this.SMUsession = new NationalInstruments.ModularInstruments.Interop.nidcpower(VisaAlias, ChanNumber, Reset, "");

                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName;

                string model = SMUsession.GetString(NationalInstruments.ModularInstruments.Interop.nidcpowerProperties.InstrumentModel);

                SMUsession.SetDouble(nidcpowerProperties.SourceDelay, ChanNumber, 0);
                SMUsession.SetDouble(nidcpowerProperties.PowerLineFrequency, ChanNumber, nidcpowerConstants._60Hertz);
                SMUsession.ConfigureOutputEnabled(ChanNumber, false);
                SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcVoltage);
                SMUsession.ConfigureSense(ChanNumber, nidcpowerConstants.Remote);
                SMUsession.SetInt32(nidcpowerProperties.CurrentLimitAutorange, ChanNumber, nidcpowerConstants.On);
                SMUsession.SetInt32(nidcpowerProperties.VoltageLevelAutorange, ChanNumber, nidcpowerConstants.On);
                SMUsession.ConfigureVoltageLevel(ChanNumber, 0);
                SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, 0.001);
                SMUsession.ConfigureOutputEnabled(ChanNumber, true);
                SMUsession.SetDouble(nidcpowerProperties.VoltageCompensationFrequency, 500);
                SMUsession.SetDouble(nidcpowerProperties.VoltageGainBandwidth, 20000);
                SMUsession.SetDouble(nidcpowerProperties.VoltagePoleZeroRatio, 8);
                SMUsession.Initiate();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SMU Initialize");
            }
        }

        public void ForceVoltage(double voltsForce, double currentLimit)
        {
            int error = -1;
            int chanInt = Convert.ToInt16(ChanNumber);

            try
            {
                if (currentLimit != priorCurrentLim)
                {
                    // ConfigureCurrentLimit appears to also set the range automatically, because auto-range is on
                    error = SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, currentLimit);
                    priorCurrentLim = currentLimit;

                    //double readRange = SMUsession.GetDouble(nidcpowerProperties.CurrentLimit);
                }
                if (voltsForce != priorVoltage)
                {
                    error = SMUsession.ConfigureVoltageLevel(ChanNumber, voltsForce);
                    priorVoltage = voltsForce;
                }


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureCurrentLimit");
            }
        }

        public void SetupCurrentMeasure(bool UseIQsettings, bool triggerFromSA)
        {
            int error = -1;

            try
            {
                SMUsession.Abort();

                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, 1);

                SMUsession.SetDouble(nidcpowerProperties.ApertureTime, ChanNumber, 0.0005);
                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);

                SMUsession.Initiate();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupCurrentMeasure4143");

            }
        }
        public void MeasureCurrent(int NumAverages, bool triggered, ref double Result)
        {
            int error = -1;

            try
            {
                double[] volt = new double[NumAverages];
                double[] curr = new double[NumAverages];
                ushort[] measComp = new ushort[NumAverages];
                int actCount = 0;
                double[] voltSingle = new double[1];
                double[] currSingle = new double[1];

                for (int avg = 0; avg < NumAverages; avg++)
                {
                    if (triggered)
                    {
                        error = SMUsession.FetchMultiple(ChanNumber, 1, 1, voltSingle, currSingle, measComp, out actCount);  // "Count" doesn't wait to re-trigger, just measures all immediately after 1st trigger

                        volt[avg] = voltSingle[0];
                        curr[avg] = currSingle[0];
                    }
                    else
                    {
                        error = SMUsession.Measure(ChanNumber, nidcpowerConstants.MeasureCurrent, out curr[avg]);
                    }
                }

                Result = curr.Average();


            }
            catch (Exception e)
            {
                MessageBox.Show("PinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "MeasureCurrent");
            }
        }

        int NumTraceSamples;
        public void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, bool triggered, bool triggerFromSA = false)
        {
            try
            {
                SMUsession.Abort();

                SMUsession.SetDouble(nidcpowerProperties.ApertureTime, ChanNumber, aperture);
                aperture = SMUsession.GetDouble(nidcpowerProperties.ApertureTime);

                NumTraceSamples = (int)(measureTimeLength / aperture);

                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);

                if (triggered)
                {
                    SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);
                    if (triggerFromSA) SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "PXI_Trig1");
                    else SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "PXI_Trig0");
                }
                else
                {
                    SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.SoftwareEdge);
                }

                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, NumTraceSamples);
                SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, 1);

                SMUsession.Initiate();

                if (!triggered) SMUsession.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupCurrentTraceMeasurement4143");

            }
        }
        public double[] MeasureCurrentTrace()
        {
            int error = -1;

            try
            {
                ushort[] measComp = new ushort[NumTraceSamples];
                int actCount = 0;
                double[] voltSingle = new double[NumTraceSamples];
                double[] currSingle = new double[NumTraceSamples];

                error = SMUsession.FetchMultiple(ChanNumber, 1, NumTraceSamples, voltSingle, currSingle, measComp, out actCount);  // "Count" doesn't wait to re-trigger, just measures all immediately after 1st trigger

                return currSingle;
            }
            catch (Exception e)
            {
                MessageBox.Show("PinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "MeasureCurrentTrace");
                return new double[4];
            }
        }

        public void SetupVoltageMeasure()
        {
            int error = -1;

            try
            {
                SMUsession.Abort();

                double measureTimeLength = 0.001;

                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);
                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, 1);

                double dcSampleRate = 200e3;   // this is fixed for NI hardware
                int SamplesToAvg = (int)(dcSampleRate * measureTimeLength);

                SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, SamplesToAvg);

                SMUsession.Initiate();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupVoltageMeasure4154");

            }
        }
        public void MeasureVoltage(int NumAverages, ref double Result)
        {
            int error = -1;

            try
            {
                double[] volts = new double[NumAverages];

                for (int avg = 0; avg < NumAverages; avg++)
                {
                    error = SMUsession.Measure(ChanNumber, nidcpowerConstants.MeasureVoltage, out volts[avg]);
                }

                Result = volts.Average();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureVoltage");

            }
        }

        public void SetupContinuity(double currentForce)
        {
            SMUsession.Abort();
            SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcCurrent);
            SMUsession.ConfigureCurrentLevel(ChanNumber, currentForce);
            SMUsession.ConfigureCurrentLevelRange(ChanNumber, Math.Abs(currentForce));

            SetupCurrentMeasure(false, false);
        }

        public void MeasureContinuity(int avgs, ref double result)
        {
            MeasureVoltage(avgs, ref result);

            SMUsession.Abort();
            SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcVoltage);
        }

        public void OutputEnable(bool state)
        {
            SMUsession.ConfigureOutputEnabled(ChanNumber, state);
        }
    }

    public class NI4139 : iSmu
    {
        public nidcpower SMUsession;
        public string VisaAlias { get; set; }
        public string ChanNumber { get; set; }
        public string PinName { get; set; }
        public double priorVoltage { get; set; }
        public double priorCurrentLim { get; set; }

        public NI4139(string VisaAlias, string ChanNumber, string PinName, bool Reset)
        {
            try
            {
                this.SMUsession = new NationalInstruments.ModularInstruments.Interop.nidcpower(VisaAlias, ChanNumber, Reset, "");

                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName;

                string model = SMUsession.GetString(NationalInstruments.ModularInstruments.Interop.nidcpowerProperties.InstrumentModel);

                SMUsession.SetDouble(nidcpowerProperties.SourceDelay, ChanNumber, 0);
                SMUsession.SetDouble(nidcpowerProperties.PowerLineFrequency, ChanNumber, nidcpowerConstants._60Hertz);
                SMUsession.ConfigureOutputEnabled(ChanNumber, false);
                SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcVoltage);
                SMUsession.ConfigureSense(ChanNumber, nidcpowerConstants.Remote);
                SMUsession.SetInt32(nidcpowerProperties.CurrentLimitAutorange, ChanNumber, nidcpowerConstants.On);
                SMUsession.SetInt32(nidcpowerProperties.VoltageLevelAutorange, ChanNumber, nidcpowerConstants.On);
                SMUsession.ConfigureVoltageLevel(ChanNumber, 0);
                SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, 0.001);
                SMUsession.ConfigureOutputEnabled(ChanNumber, true);
                SMUsession.SetDouble(nidcpowerProperties.VoltageCompensationFrequency, 500);
                SMUsession.SetDouble(nidcpowerProperties.VoltageGainBandwidth, 20000);
                SMUsession.SetDouble(nidcpowerProperties.VoltagePoleZeroRatio, 8);
                SMUsession.Initiate();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SMU Initialize");
            }
        }

        public void ForceVoltage(double voltsForce, double currentLimit)
        {
            int error = -1;
            int chanInt = Convert.ToInt16(ChanNumber);

            try
            {
                if (currentLimit != priorCurrentLim)
                {
                    // ConfigureCurrentLimit appears to also set the range automatically, because auto-range is on
                    error = SMUsession.ConfigureCurrentLimit(ChanNumber, nidcpowerConstants.CurrentRegulate, currentLimit);
                    priorCurrentLim = currentLimit;

                    //double readRange = SMUsession.GetDouble(nidcpowerProperties.CurrentLimit);
                }
                if (voltsForce != priorVoltage)
                {
                    error = SMUsession.ConfigureVoltageLevel(ChanNumber, voltsForce);
                    priorVoltage = voltsForce;
                }


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ConfigureCurrentLimit");
            }
        }

        public void SetupCurrentMeasure(bool UseIQsettings, bool triggerFromSA)
        {
            int error = -1;

            try
            {
                SMUsession.Abort();

                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, 1);

                SMUsession.SetDouble(nidcpowerProperties.ApertureTime, ChanNumber, 0.0005);
                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);

                SMUsession.Initiate();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupCurrentMeasure4139");

            }
        }
        public void MeasureCurrent(int NumAverages, bool triggered, ref double Result)
        {
            int error = -1;

            try
            {
                double[] volt = new double[NumAverages];
                double[] curr = new double[NumAverages];
                ushort[] measComp = new ushort[NumAverages];
                int actCount = 0;
                double[] voltSingle = new double[1];
                double[] currSingle = new double[1];

                for (int avg = 0; avg < NumAverages; avg++)
                {
                    if (triggered)
                    {
                        error = SMUsession.FetchMultiple(ChanNumber, 1, 1, voltSingle, currSingle, measComp, out actCount);  // "Count" doesn't wait to re-trigger, just measures all immediately after 1st trigger

                        volt[avg] = voltSingle[0];
                        curr[avg] = currSingle[0];
                    }
                    else
                    {
                        error = SMUsession.Measure(ChanNumber, nidcpowerConstants.MeasureCurrent, out curr[avg]);
                    }
                }

                Result = curr.Average();


            }
            catch (Exception e)
            {
                MessageBox.Show("PinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "MeasureCurrent");

            }
        }

        int NumTraceSamples;
        public void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, bool triggered, bool triggerFromSA = false)
        {
            try
            {
                SMUsession.Abort();

                SMUsession.SetDouble(nidcpowerProperties.ApertureTime, ChanNumber, aperture);
                aperture = SMUsession.GetDouble(nidcpowerProperties.ApertureTime);

                NumTraceSamples = (int)(measureTimeLength / aperture);

                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnMeasureTrigger);

                if (triggered)
                {
                    SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.DigitalEdge);
                    if (triggerFromSA) SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "PXI_Trig1");
                    else SMUsession.SetString(nidcpowerProperties.DigitalEdgeMeasureTriggerInputTerminal, "PXI_Trig0");
                }
                else
                {
                    SMUsession.SetInt32(nidcpowerProperties.MeasureTriggerType, nidcpowerConstants.SoftwareEdge);
                }

                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, NumTraceSamples);
                SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, 1);

                SMUsession.Initiate();

                if (!triggered) SMUsession.SendSoftwareEdgeTrigger(nidcpowerConstants.MeasureTrigger);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupCurrentTraceMeasurement4139");

            }
        }
        public double[] MeasureCurrentTrace()
        {
            int error = -1;

            try
            {
                ushort[] measComp = new ushort[NumTraceSamples];
                int actCount = 0;
                double[] voltSingle = new double[NumTraceSamples];
                double[] currSingle = new double[NumTraceSamples];

                error = SMUsession.FetchMultiple(ChanNumber, 1, NumTraceSamples, voltSingle, currSingle, measComp, out actCount);  // "Count" doesn't wait to re-trigger, just measures all immediately after 1st trigger

                return currSingle;
            }
            catch (Exception e)
            {
                MessageBox.Show("PinName: " + PinName + "\n\nVisaAlias: " + VisaAlias + "\n\nChannel: " + ChanNumber + "\n\n" + e.ToString(), "MeasureCurrentTrace");
                return new double[4];
            }
        }

        public void SetupVoltageMeasure()
        {
            int error = -1;

            try
            {
                SMUsession.Abort();

                double measureTimeLength = 0.001;

                SMUsession.SetInt32(nidcpowerProperties.MeasureWhen, nidcpowerConstants.OnDemand);
                SMUsession.SetInt32(nidcpowerProperties.MeasureRecordLength, 1);

                double dcSampleRate = 200e3;   // this is fixed for NI hardware
                int SamplesToAvg = (int)(dcSampleRate * measureTimeLength);

                SMUsession.SetInt32(nidcpowerProperties.SamplesToAverage, SamplesToAvg);

                SMUsession.Initiate();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SetupVoltageMeasure4154");

            }
        }
        public void MeasureVoltage(int NumAverages, ref double Result)
        {
            int error = -1;

            try
            {
                double[] volts = new double[NumAverages];

                for (int avg = 0; avg < NumAverages; avg++)
                {
                    error = SMUsession.Measure(ChanNumber, nidcpowerConstants.MeasureVoltage, out volts[avg]);
                }

                Result = volts.Average();


            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "MeasureVoltage4154");

            }
        }

        public void SetupContinuity(double currentForce)
        {
            SMUsession.Abort();
            SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcCurrent);
            SMUsession.ConfigureCurrentLevel(ChanNumber, currentForce);
            SMUsession.ConfigureCurrentLevelRange(ChanNumber, Math.Abs(currentForce));

            SetupCurrentMeasure(false, false);
        }

        public void MeasureContinuity(int avgs, ref double result)
        {
            MeasureVoltage(avgs, ref result);

            SMUsession.Abort();
            SMUsession.ConfigureOutputFunction(ChanNumber, nidcpowerConstants.DcVoltage);
        }

        public void OutputEnable(bool state)
        {
            SMUsession.ConfigureOutputEnabled(ChanNumber, state);
        }
    }
    #endregion

    public interface iSmu
    {
        string VisaAlias { get; set; }
        string ChanNumber { get; set; }
        string PinName { get; set; }
        double priorVoltage { get; set; }
        double priorCurrentLim { get; set; }

        void ForceVoltage(double voltsForce, double currentLimit);
        void SetupCurrentMeasure(bool UseIQsettings, bool triggerFromSA);
        void MeasureCurrent(int NumAverages, bool triggered, ref double Result);
        void SetupCurrentTraceMeasurement(double measureTimeLength, double aperture, bool triggered, bool triggerFromSA = false);
        double[] MeasureCurrentTrace();
        void SetupVoltageMeasure();
        void MeasureVoltage(int NumAverages, ref double Result);
        void SetupContinuity(double currentForce);
        void MeasureContinuity(int avgs, ref double result);
        void OutputEnable(bool state);
    }
}
