using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using Aemulus.Hardware.SMU;

namespace LibEqmtDriver.SMU
{
    public class AePXISMU : iPowerSupply
    {
        public AePXISMU(string[] val)
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
        ~AePXISMU() { }

        
        public static Dictionary<string, iAeSmu> SmuResources = new Dictionary<string, iAeSmu>();

        public static iAeSmu getSMU(string VisaAlias, string ChanNumber, string PinName, bool Reset)
        {
            iAeSmu smu;

            if (VisaAlias.Contains("430"))
            {
                smu = new AM430E(VisaAlias, ChanNumber, PinName, Reset);
            }
            else if (VisaAlias.Contains("471"))
            {
                smu = new AM471E(VisaAlias, ChanNumber, PinName, Reset);
            }
            else
            {
                throw new Exception("Visa Alias \"" + VisaAlias + "\" is not in a recognized format.\nValid SMU Visa Aliases must include one of the following:\n"
                    + "\n\"430\""
                    + "\n\"471\""
                    + "\n\nFor example, Visa Alias \"SMU_AM471E_P1\" will be recognized as an AEMULUS AM471E module.");
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
            SmuResources[strSelection].OutputEnable(true, Channel);
        }

        void iPowerSupply.DcOff(string strSelection, ePSupply_Channel Channel)
        {
            //SmuResources[strSelection].ForceVoltage(0.0, 1e-6);      //force voltage to 0V and very small current (cannot be zero)
            SmuResources[strSelection].OutputEnable(false, Channel);
        }

        void iPowerSupply.SetNPLC(string strSelection, ePSupply_Channel Channel, float val)
        {
            SmuResources[strSelection].SetNPLC(Channel, val);
        }

        void iPowerSupply.SetVolt(string strSelection, ePSupply_Channel Channel, double Volt, double iLimit, ePSupply_VRange VRange)
        {
            SmuResources[strSelection].SetVoltage(Channel, Volt, iLimit, VRange);
        }

        float iPowerSupply.MeasI(string strSelection, ePSupply_Channel Channel, ePSupply_IRange IRange)
        {
            float imeas = -999;
            imeas = SmuResources[strSelection].MeasureCurrent(Channel, IRange);
            return imeas;
        }

        float iPowerSupply.MeasV(string strSelection, ePSupply_Channel Channel, ePSupply_VRange VRange)
        {
            float vmeas = -999;
            vmeas = SmuResources[strSelection].MeasureVoltage(Channel, VRange);
            return vmeas;
        }

        #endregion
    }

    #region iAeSMU class

    public class AM430E : iAeSmu
    {
        public PxiSmu smu;
        public string VisaAlias { get; set; }
        public string ChanNumber { get; set; }
        public string PinName { get; set; }

        public AM430E(string VisaAlias, string ChanNumber, string PinName, bool Reset)
        {
            try
            {
                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName;

                smu = new PxiSmu(VisaAlias, "0-3", 0xf, "Simulate=0, DriverSetup=Model:AM430e");

                int ret = 0;
                ret += smu.Reset();
                ret += smu.ConfigurePowerLineFrequency(60);
                ret += smu.ConfigureOutputTransient("0-3", 1);
                ret += smu.ConfigureSamplingTime("0-3", 0.1, 1);
                ret += smu.ConfigureSense("0-3", PxiSmuConstants.SenseRemote);
                ret += smu.ConfigureOutputFunction("0-3", PxiSmuConstants.DVCI);
                ret += smu.ConfigureCurrentLimit("0-3".ToString(), 0, 100e-3);
                ret += smu.ConfigureVoltageLevelAndRange("0-3", 0, 2);
                //ret += smu.ConfigureVoltageLevel("0-3", 0);
                ret += smu.ConfigureOutputSwitch("0-3", 1);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SMU Initialize");
            }
        }

        public void OutputEnable(bool state, ePSupply_Channel Channel)
        {
            //int ret = 0;
            //if (state)
            //    ret += smu.ConfigureOutputSwitch(((int)Channel).ToString(), 1);
            //else
            //    smu.ConfigureOutputSwitch(((int)Channel).ToString(), 0);    
        }
        public void SetNPLC(ePSupply_Channel Channel, float val)
        {
            smu.ConfigureSamplingTime(((int)Channel).ToString(), val, 1);
        }
        public void SetVoltage(ePSupply_Channel Channel, double Volt, double iLimit, ePSupply_VRange VRange)
        {
            double _VRange = 0;
            switch (VRange)
            {
                case ePSupply_VRange._1V:
                    _VRange = 1; break;
                case ePSupply_VRange._10V:
                    _VRange = 10; break;
                case ePSupply_VRange._Auto:
                    _VRange = 10; break;
            }
            smu.ConfigureOutputFunction(((int)Channel).ToString(), PxiSmuConstants.DVCI);
            smu.ConfigureCurrentLimit(((int)Channel).ToString(), 0, iLimit);
            smu.ConfigureVoltageLevelAndRange(((int)Channel).ToString(), Volt, _VRange);
        }
        public float MeasureCurrent(ePSupply_Channel Channel, ePSupply_IRange IRange)
        {
            //we don't need to set range for measure
            double[] current = new double[4];
            int ret = 0;
            ret += smu.Measure(((int)Channel).ToString(), PxiSmuConstants.MeasureCurrent, current);
            return (float)current[0];
        }
        public float MeasureVoltage(ePSupply_Channel Channel, ePSupply_VRange VRange)
        {
            double[] volt = new double[4];
            int ret = 0;
            ret += smu.Measure(((int)Channel).ToString(), PxiSmuConstants.MeasureVoltage, volt);
            return (float)volt[0];
        }
    }

    public class AM471E : iAeSmu
    {
        public PxiSmu smu;
        public string VisaAlias { get; set; }
        public string ChanNumber { get; set; }
        public string PinName { get; set; }

        public AM471E(string VisaAlias, string ChanNumber, string PinName, bool Reset)
        {
            try
            {
                this.VisaAlias = VisaAlias;
                this.ChanNumber = ChanNumber;
                this.PinName = PinName;

                smu = new PxiSmu(VisaAlias, "0", 0xf, "Simulate=0, DriverSetup=Model:AM471e");

                int ret = 0;
                ret += smu.Reset();
                ret += smu.ConfigurePowerLineFrequency(60);
                ret += smu.ConfigureOutputTransient("0", 1);
                ret += smu.ConfigureSamplingTime("0", 0.1, 1);
                ret += smu.ConfigureSense("0", PxiSmuConstants.SenseRemote);
                ret += smu.ConfigureOutputFunction("0", PxiSmuConstants.DVCI);
                ret += smu.ConfigureCurrentLimit("0".ToString(), 0, 100e-3);
                ret += smu.ConfigureVoltageLevelAndRange("0", 0, 1);
                //ret += smu.ConfigureVoltageLevel("0", 0);
                ret += smu.ConfigureOutputSwitch("0", 1);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "SMU Initialize");
            }
        }

        public void OutputEnable(bool state, ePSupply_Channel Channel)
        {
            //if (state)
            //    smu.ConfigureOutputSwitch(((int)Channel).ToString(), 1);
            //else
            //    smu.ConfigureOutputSwitch(((int)Channel).ToString(), 0);      
        }
        public void SetNPLC(ePSupply_Channel Channel, float val)
        {
            smu.ConfigureSamplingTime(((int)Channel).ToString(), val, 1);
        }
        public void SetVoltage(ePSupply_Channel Channel, double Volt, double iLimit, ePSupply_VRange VRange)
        {
            double _VRange = 0;
            switch (VRange)
            {
                case ePSupply_VRange._1V:
                    _VRange = 1; break;
                case ePSupply_VRange._10V:
                    _VRange = 10; break;
                case ePSupply_VRange._Auto:
                    _VRange = 10; break;
            }
            smu.ConfigureOutputFunction(((int)Channel).ToString(), PxiSmuConstants.DVCI);
            smu.ConfigureCurrentLimit(((int)Channel).ToString(), 0, iLimit);
            smu.ConfigureVoltageLevelAndRange(((int)Channel).ToString(), Volt, _VRange);
            //smu.ConfigureVoltageLevel(((int)Channel).ToString(), Volt);
        }
        public float MeasureCurrent(ePSupply_Channel Channel, ePSupply_IRange IRange)
        {
            //we don't need to set range for measure
            double[] current = new double[4];
            int ret = 0;
            ret += smu.Measure(((int)Channel).ToString(), PxiSmuConstants.MeasureCurrent, current);
            return (float)current[0];
        }
        public float MeasureVoltage(ePSupply_Channel Channel, ePSupply_VRange VRange)
        {
            double[] volt = new double[4];
            int ret = 0;
            ret += smu.Measure(((int)Channel).ToString(), PxiSmuConstants.MeasureVoltage, volt);
            return (float)volt[0];
        }
    }

    #endregion

    public interface iAeSmu
    {
        string VisaAlias { get; set; }
        string ChanNumber { get; set; }
        string PinName { get; set; }

        void OutputEnable(bool state, ePSupply_Channel Channel);
        void SetNPLC(ePSupply_Channel Channel, float val);
        void SetVoltage(ePSupply_Channel Channel, double Volt, double iLimit, ePSupply_VRange VRange);
        float MeasureCurrent(ePSupply_Channel Channel, ePSupply_IRange IRange);
        float MeasureVoltage(ePSupply_Channel Channel, ePSupply_VRange VRange);
    }
}
 

