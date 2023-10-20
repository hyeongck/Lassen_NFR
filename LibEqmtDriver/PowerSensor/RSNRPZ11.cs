using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InstrumentDrivers;
using System.Windows.Forms;

namespace LibEqmtDriver.PS
{
    public class RSNRPZ11 : iPowerSensor
    {
        public static rsnrpz myRSnrp;
        private static double previousMeasLength = 0;
        private static int previousNumAvgs = 0;
        private static bool isInitialized = false;
        public string IOAddress;

        /// <summary>
        /// Parsing Equpment Address
        /// </summary>
        public string Address
        {
            get
            {
                return IOAddress;
            }
            set
            {
                IOAddress = value;
            }
        }
        //Constructor
        public RSNRPZ11(string ioAddress)
        {
            Address = ioAddress;
        }
        RSNRPZ11() { }

        public void InitializeAndZero()
        {
            try
            {
                //if (isInitialized) return;

                //myRSnrp = new rsnrpz("*", true, true);
                myRSnrp = new rsnrpz();
                myRSnrp.Init(Address, true, true);
                myRSnrp.reset();
                //myRSnrp.chan_reset(1);
                myRSnrp.chan_zero(1);
                previousMeasLength = 0;

                isInitialized = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        #region iPowerSensor Members
        void iPowerSensor.Initialize(int chNo)
        {
            InitializeAndZero();
        }
        void iPowerSensor.SetFreq(int chNo, double freqMHz)
        {
            //SetupMeasurement(freqMHz, 0.001, 10); //Original
            SetupMeasurement(freqMHz, 0.001, 3);
        }
        void iPowerSensor.SetOffset(int chNo, double offset)
        {
            try
            {
                myRSnrp.corr_setOffset(1, offset);
                myRSnrp.corr_setOffsetEnabled(1, true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        void iPowerSensor.EnableOffset(int chNo, bool status)
        {
            try
            {
                myRSnrp.corr_setOffsetEnabled(1, status);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        float iPowerSensor.MeasPwr(int chNo)
        {
            double[] measDataWatts = new double[1];
            int readCount;
            float measValDbm = -2000;

            try
            {
                myRSnrp.meass_readBufferMeasurement(1, 50, 1, measDataWatts, out readCount);

                measValDbm = 10f * (float)Math.Log10(1000.0 * measDataWatts[0]);
            }
            catch (Exception e)
            {
                myRSnrp.chan_abort(1);
                //MessageBox.Show(measValDbm.ToString() + "/n/n" + e.ToString());
            }

            if (float.IsNaN(measValDbm) || (measValDbm < -100 || measValDbm > 100))    // need this in case of NAN or -inifinity
            {
                measValDbm = -999;
            }

            return measValDbm;
        }
        void iPowerSensor.Reset()
        {

        }
        #endregion

        public static void SetupMeasurement(double measureFreqMHz, double measLengthS, int numAvgs)
        {
            try
            {

                myRSnrp.chan_mode(1, InstrumentDrivers.rsnrpzConstants.SensorModeTimeslot);
                if (measureFreqMHz > 8000) myRSnrp.chan_setCorrectionFrequency(1, 8000 * 1e6);
                else myRSnrp.chan_setCorrectionFrequency(1, measureFreqMHz * 1e6); // Set corr frequency
                myRSnrp.trigger_setSource(1, InstrumentDrivers.rsnrpzConstants.TriggerSourceImmediate);
                SetupMeasLength(measLengthS);
                SetupNumAverages(numAvgs);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public static void SetupBurstMeasurement(double measureFreqMHz, double MeasLengthS, double triggerLevDbm, int numAvgs)
        {
            try
            {
                myRSnrp.chan_mode(1, InstrumentDrivers.rsnrpzConstants.SensorModeTimeslot);
                myRSnrp.chan_setCorrectionFrequency(1, measureFreqMHz * 1e6); // Set corr frequency
                myRSnrp.trigger_setSource(1, InstrumentDrivers.rsnrpzConstants.TriggerSourceInternal);
                SetupMeasLength(MeasLengthS);
                double trigLev = Math.Pow(10.0, triggerLevDbm / 10.0) / 1000.0;
                myRSnrp.trigger_setLevel(1, trigLev);
                SetupNumAverages(numAvgs);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void SetupMeasLength(double measLengthS)
        {
            try
            {
                if (true | measLengthS != previousMeasLength)
                {
                    myRSnrp.tslot_configureTimeSlot(1, 1, measLengthS);
                    previousMeasLength = measLengthS;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void SetupNumAverages(int numAvgs)
        {
            try
            {
                if (true | numAvgs != previousNumAvgs)
                {
                    myRSnrp.avg_configureAvgManual(1, numAvgs);
                    previousNumAvgs = numAvgs;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
