using System;
using Ivi.Visa.Interop;
using System.Windows.Forms;

namespace LibEqmtDriver.SCU
{
    public class Agilent3499:iSwitch 
    {
        public static string ClassName = "3449A Switch Control Unit Class";
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

        //Constructor
        public Agilent3499(string ioAddress) 
        {
            Address = ioAddress;
            OpenIO();
        }
        Agilent3499() { }

        #region iSwitch Members

        void iSwitch.Initialize()
        {
            try
            {
                myVisaEq.WriteString("*CLS; *RST", true);
            }
            catch (Exception ex)
            {
                throw new Exception("Agilent3499: Initialize -> " + ex.Message);
            }
        }

        void iSwitch.SetPath(string val)
        {
            string[] tempdata;
            tempdata = val.Split(';');

            try
            {
                for (int i = 0; i < tempdata.Length; i++)
                {
                    myVisaEq.WriteString(tempdata[i], true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Agilent3499: SetPath -> " + ex.Message);
            }
        }

        void iSwitch.Reset()
        {
             try
             {
                 myVisaEq.WriteString("*CLS; *RST", true);
             }
             catch (Exception ex)
             {
                 throw new Exception("Agilent3499: Reset -> " + ex.Message);
             }
        }

        #endregion

        private void WRITE(string _cmd)
        {
            myVisaEq.WriteString(_cmd, true);
        }
        private void SW_control(string _StatusSW,string _SwitchSlot)
        {
            myVisaEq.WriteString(_StatusSW +" (@"+_SwitchSlot+")", true);
        }
        private double WRITE_READ_DOUBLE(string _cmd)
        {
            myVisaEq.WriteString(_cmd, true);
            return Convert.ToDouble(myVisaEq.ReadString());
        }
        private string WRITE_READ_STRING(string _cmd)
        {
            myVisaEq.WriteString(_cmd, true);
            return myVisaEq.ReadString();
        }
        private float WRITE_READ_SINGLE(string _cmd)
        {
            myVisaEq.WriteString(_cmd, true);
            return Convert.ToSingle(myVisaEq.ReadString());
        }



      
    }
}
