using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivi.Visa.Interop;
using System.Windows.Forms;

namespace LibEqmtDriver.DC
{
    public class PS662xA : iDCSupply
    {
        public static string ClassName = "PS662xA 4-Channel PowerSupply Class";
        private FormattedIO488 myVisaEq = new FormattedIO488();
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
        public FormattedIO488 parseIO
        {
            get
            {
                return myVisaEq;
            }
            set
            {
                myVisaEq = parseIO;
            }
        }
        public void OpenIO()
        {
            if (IOAddress.Length > 3)
            {
                try
                {
                    ResourceManager mgr = new ResourceManager();
                    myVisaEq.IO = (IMessage)mgr.Open(IOAddress, AccessMode.NO_LOCK, 2000, "");
                }
                catch (SystemException ex)
                {
                    MessageBox.Show("Class Name: " + ClassName + "\nParameters: OpenIO" + "\n\nErrorDesciption: \n"
                        + ex, "Error found in Class " + ClassName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    myVisaEq.IO = null;
                    return;
                }
            }
        }
        public PS662xA(string ioAddress)
        {
            Address = ioAddress;
            OpenIO();
        }
        ~PS662xA() { }

        #region iDCSupply Members

        void iDCSupply.Init()
        {
            RESET();
        }

        void iDCSupply.DcOn(int Channel)
        {
            try
            {
                myVisaEq.IO.WriteString("OUTP ON, (@" + Channel.ToString() + ")");
            }
            catch (Exception ex)
            {
                throw new Exception("PS662xA: DcOn -> " + ex.Message);
            }
        }
        void iDCSupply.DcOff(int Channel)
        {
            try
            {
                myVisaEq.IO.WriteString("OUTP OFF, (@" + Channel.ToString() + ")");
            }
            catch (Exception ex)
            {
                throw new Exception("PS662xA: DcOff -> " + ex.Message);
            }
        }

        void iDCSupply.SetVolt(int Channel, double Volt, double iLimit)
        {
            VSourceSet(Channel, Volt);
            ISourceSet(Channel, iLimit);
        }

        float iDCSupply.MeasI(int Channel)
        {
            try
            {
                return WRITE_READ_SINGLE("MEAS:CURR? (@" + Channel.ToString() + ")");
            }
            catch (Exception ex)
            {
                throw new Exception("PS662xA:: MeasI -> " + ex.Message);
            }
        }

        float iDCSupply.MeasV(int Channel)
        {
            try
            {
                return WRITE_READ_SINGLE("MEAS:VOLT? (@" + Channel.ToString() + ")");
            }
            catch (Exception ex)
            {
                throw new Exception("PS662xA:: MeasV -> " + ex.Message);
            }
        }

        #endregion

        private void RESET()
        {
            try
            {
                myVisaEq.WriteString("*CLS; *RST", true);
            }
            catch (Exception ex)
            {
                throw new Exception("PS662xA: Initialize -> " + ex.Message);
            }
        }

        private void VSourceSet(int val, double dblVoltage)
        {
            try
            {
                myVisaEq.IO.WriteString("VOLT " + dblVoltage.ToString() + ",(@" + val.ToString() + ")");
            }
            catch (Exception ex)
            {
                throw new Exception("PS662xA: VSourceSet -> " + ex.Message);
            }
        }
        private void ISourceSet(int val, double dblAmps)
        {
            try
            {
                myVisaEq.IO.WriteString("CURR " + dblAmps.ToString() + ",(@" + val.ToString() + ")");
            }
            catch (Exception ex)
            {
                throw new Exception("PS662xA: ISourceSet -> " + ex.Message);
            }
        }

        #region generic READ function

        public double WRITE_READ_DOUBLE(string _cmd)
        {
            myVisaEq.WriteString(_cmd, true);
            return Convert.ToDouble(myVisaEq.ReadString());
        }
        public string WRITE_READ_STRING(string _cmd)
        {
            myVisaEq.WriteString(_cmd, true);
            return myVisaEq.ReadString();
        }
        public float WRITE_READ_SINGLE(string _cmd)
        {
            myVisaEq.WriteString(_cmd, true);
            return Convert.ToSingle(myVisaEq.ReadString());
        }
        public float[] READ_IEEEBlock(IEEEBinaryType _type)
        {
            return (float[])myVisaEq.ReadIEEEBlock(_type, true, true);
        }
        public float[] WRITE_READ_IEEEBlock(string _cmd, IEEEBinaryType _type)
        {
            myVisaEq.WriteString(_cmd, true);
            return (float[])myVisaEq.ReadIEEEBlock(_type, true, true);
        }

        #endregion

    }
}
