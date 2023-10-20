using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace LibEqmtDriver.MIPI
{
    public class Aemulus_DM482e : iMiPiCtrl
    {
        int slaveaddr, pairNo;
        int ret = 0;
        bool[] dataArray_Bool;
        string moduleAlias = Lib_Var.ºmyDM482Address;

        int pmTrigAddr = Lib_Var.ºPMTrig;
        int pmTrigData = Lib_Var.ºPMTrig_Data;

        public s_MIPI_PAIR[] mipiPair;

        private double vih, vil, voh, vol, ioh, iol, vch, vcl, vt;

        Aemulus.Hardware.DM myDM;
        LibEqmtDriver.Utility.HiPerfTimer HiTimer = new LibEqmtDriver.Utility.HiPerfTimer();
        Stopwatch Speedo = new Stopwatch();

        public Aemulus_DM482e()
        {
            myDM = new Aemulus.Hardware.DM(Lib_Var.ºHW_Profile, 3, 0, 0, false, 0x0f); //3
        }

        #region iMipiCtrl interface
        void iMiPiCtrl.Init(s_MIPI_PAIR[] mipiPairCfg)
        { 
            mipiPair = new s_MIPI_PAIR[mipiPairCfg.Length];

            for (int i = 0; i < mipiPair.Length; i++)
            {
                mipiPair[i].PAIRNO = mipiPairCfg[i].PAIRNO;

                //set MIPI pin alias name
                mipiPair[i].SCLK = "P" + mipiPairCfg[i].SCLK;
                mipiPair[i].SDATA = "P" + mipiPairCfg[i].SDATA;
                mipiPair[i].SVIO = "P" + mipiPairCfg[i].SVIO;

                //set mipi pin no
                mipiPair[i].SCLK_pinNo = Int32.Parse(mipiPairCfg[i].SCLK);
                mipiPair[i].SDATA_pinNo = Int32.Parse(mipiPairCfg[i].SDATA);
                mipiPair[i].SVIO_pinNo = Int32.Parse(mipiPairCfg[i].SVIO);
            }

            INITIALIZATION();
        }
        void iMiPiCtrl.TurnOn_VIO(int pair)
        {
            VIO_ON(pair);
        }
        void iMiPiCtrl.TurnOff_VIO(int pair)
        {
            if (pair < mipiPair.Length)
                VIO_OFF(pair);
        }
        void iMiPiCtrl.SendAndReadMIPICodes(out bool ReadSuccessful, int Mipi_Reg)
        {
            //This function is for fixed MIPI Pair and Slave address
            pairNo = 0;                             //default using MIPI pair no 0 (fixed - hardcoded)
            slaveaddr = Lib_Var.ºSlaveAddress;      //default setting from config file

            ReadSuccessful = Register_Change(Mipi_Reg);
        }
        void iMiPiCtrl.SendAndReadMIPICodesRev2(out bool ReadSuccessful, int Mipi_Reg, int pair, int slvaddr)
        {
            //Note : this function for using multiple MIPI pair and slave address
            pairNo = pair;          //pass value from tcf for different mipi pair controller
            slaveaddr = slvaddr;    //pass value from tcf for different mipi pair controller

            ReadSuccessful = Register_ChangeRev2(Mipi_Reg);
        }
        void iMiPiCtrl.SendAndReadMIPICodesCustom(out bool ReadSuccessful, string MipiRegMap, string TrigRegMap, int pair, int slvaddr)
        {
            //Note : this function for using multiple MIPI pair and slave address
            pairNo = pair;          //pass value from tcf for different mipi pair controller
            slaveaddr = slvaddr;    //pass value from tcf for different mipi pair controller

            ReadSuccessful = Register_Change_Custom(MipiRegMap, TrigRegMap, true);
        }
        void iMiPiCtrl.ReadMIPICodesCustom(out int Result, string MipiRegMap, string TrigRegMap, int pair, int slvaddr)
        {
            //Note : this function for using multiple MIPI pair and slave address
            pairNo = pair;          //pass value from tcf for different mipi pair controller
            slaveaddr = slvaddr;    //pass value from tcf for different mipi pair controller

            string tmpRslt = "";
            Result = 0;
            bool extReg = true;
            string[] tmpData = MipiRegMap.Split(':');

            Read_Register_Address_Rev2(ref tmpRslt, Convert.ToInt32(tmpData[0], 16), extReg);
            Result = int.Parse(tmpRslt, System.Globalization.NumberStyles.HexNumber);               //convert HEX to INT
        }
        void iMiPiCtrl.WriteMIPICodesCustom(string MipiRegMap, int pair, int slvaddr)
        {
            //Note : this function for using multiple MIPI pair and slave address
            pairNo = pair;          //pass value from tcf for different mipi pair controller
            slaveaddr = slvaddr;    //pass value from tcf for different mipi pair controller
            bool extReg = true;

            WriteRegister_Rev2(MipiRegMap, extReg);
        }
        void iMiPiCtrl.WriteOTPRegister(string efuseCtlReg_hex, string data_hex, int pair, int slvaddr, bool invertData = false)
        {
            //Note : this function for using multiple MIPI pair and slave address
            pairNo = pair;          //pass value from tcf for different mipi pair controller
            slaveaddr = slvaddr;    //pass value from tcf for different mipi pair controller
            string mipiRegMap = efuseCtlReg_hex + ":" + data_hex;   //construct mipiRegMap to this format only example "C0:9E" - where C0 is efuseCtrl Address , 9E is efuseData to burn
            bool extReg = true;

            WriteRegister_Rev2(mipiRegMap, extReg);
        }
        void iMiPiCtrl.SetMeasureMIPIcurrent(int delayMs, int pair, int slvaddr, s_MIPI_DCSet[] setDC_Mipi, string[] measDC_MipiCh, out s_MIPI_DCMeas[] measDC_Mipi)
        {
            //Not Implemented
            s_MIPI_DCMeas[] tmpMeasDC_Mipi = new s_MIPI_DCMeas[3];
            measDC_Mipi = tmpMeasDC_Mipi;
        }
        #endregion

        #region Init function
        public void INITIALIZATION()
        {
            for (int i = 0; i < mipiPair.Length; i++)
            {
                //Actual DUT MIPI
                InitMipi(mipiPair[i].PAIRNO, 26000000, 1.8);
                SetMipiInputDelay(mipiPair[i].PAIRNO, 2); //depend on cable length
                OnOff_VIO(true, mipiPair[i].PAIRNO);
                OnOff_CLKDATA(true, mipiPair[i].PAIRNO);
            }
        }
        public int InitMipi(int mipi_pair, int freq_Hz, double mipi_voltage)
        {
            double Threhold = 1.2;
            ret += myDM.MIPI_ConfigureClock(moduleAlias, mipiPair[mipi_pair].PAIRNO, freq_Hz);

            vih = mipi_voltage;
            vil = 0;
            voh = Threhold; //(vih - vil) / 3;
            vol = Threhold; // (vih - vil) / 3;
            ioh = 0.01;
            iol = 0.01;
            vch = mipi_voltage;
            vcl = 0;
            vt = 1.6;
            //DM482e spec
            //=====================================================
            //Driver (VIH, VIL)   -1.4V to 6V 
            //Comparator (VOH, VOL)      -2.0V to 7V
            //Current Load (IOH, IOL)    -12mA to 12mA
            //Clamp Voltage Range High Side (VCH)      -1.0V to 6.0V
            //Clamp Voltage Range Low Side (VCL)       -1.5V to 5.0V
            //Termination Voltage (VT)   -2.0V to 6.0V
            //=====================================================

            int state = 0; //Pin Electronics
            int statePMU = 1; //PMU
            int stateVIO = 2; //DIO

            //DM482e_DPINForce state  
            //=====================================================
            //0 : DM482E_CONST_FORCE_STATE_VECTOR(Pin Electronics) 
            //1 : DM482E_CONST_FORCE_STATE_PMU (Pin Measurement Unit)
            //2 : DM482E_CONST_FORCE_STATE_DIO
            //5 : DM482E_CONST_FORCE_STATE_CLOCK
            //6 : DM482E_CONST_FORCE_STATE_INVERTED_CLOCK
            //=====================================================

            #region Config MIPI

            //for Mipi
            ret += myDM.Force(mipiPair[mipi_pair].SVIO, stateVIO);
            ret += myDM.Force(mipiPair[mipi_pair].SCLK, state);
            ret += myDM.Force(mipiPair[mipi_pair].SDATA, state);

            ret += myDM.DPINLevel(mipiPair[mipi_pair].SVIO, vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
            ret += myDM.DPINLevel(mipiPair[mipi_pair].SCLK, vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
            ret += myDM.DPINLevel(mipiPair[mipi_pair].SDATA, vih, vil, voh, vol, ioh, iol, vch, vcl, vt);

            ret += myDM.ConfigurePEAttribute(mipiPair[mipi_pair].SVIO, false, false, false, false); //High Z
            ret += myDM.ConfigurePEAttribute(mipiPair[mipi_pair].SCLK, false, false, false, false); //High Z
            ret += myDM.ConfigurePEAttribute(mipiPair[mipi_pair].SDATA, true, false, false, false); //Termination Mode

            ret += myDM.SetPinDirection(mipiPair[mipi_pair].SVIO, 1);

            //for PMU
            ret += myDM.ConfigurePowerLineFrequency(moduleAlias, 50);

            ret += myDM.ConfigurePMUSense(mipiPair[mipi_pair].SVIO, 0); //0 local, 1 remote 
            ret += myDM.ConfigurePMUSense(mipiPair[mipi_pair].SCLK, 0); //0 local, 1 remote 
            ret += myDM.ConfigurePMUSense(mipiPair[mipi_pair].SDATA, 0); //0 local, 1 remote 

            ret += myDM.ConfigurePMUSamplingTime(mipiPair[mipi_pair].SVIO, 0.0001, 0);
            ret += myDM.ConfigurePMUSamplingTime(mipiPair[mipi_pair].SCLK, 0.0001, 0);
            ret += myDM.ConfigurePMUSamplingTime(mipiPair[mipi_pair].SDATA, 0.0001, 0);

            #endregion

            return ret;
        }
        public int SetMipiInputDelay(int mipi_pair, int delay)
        {
            ret += myDM.MIPI_ConfigureDelay(moduleAlias, mipi_pair, delay);
            return ret;
        }
        #endregion

        #region control mipi
        public void VIO_OFF(int mipi_pair)
        {
            ret += myDM.DrivePin(mipiPair[mipi_pair].SVIO, 0); //vio drive low
        }
        public void VIO_ON(int mipi_pair)
        {
            ret += myDM.DrivePin(mipiPair[mipi_pair].SVIO, 1); //vio drive high
        }
        public int OnOff_VIO(bool isON, int mipi_pair)
        {
            if (isON)
            {
                ret += myDM.DrivePin(mipiPair[mipi_pair].SVIO, 1); //vio drive high
            }
            else
            {
                ret += myDM.DrivePin(mipiPair[mipi_pair].SVIO, 0); //vio drive low
            }
            return ret;
        }
        public int OnOff_CLKDATA(bool isON, int mipi_pair)
        {

            if (isON)
            {
                //connect mipi
                ret += myDM.MIPI_Connect(moduleAlias, mipi_pair, 1);
                ret += myDM.DPINOn(mipiPair[mipi_pair].SVIO);
                ret += myDM.DPINOn(mipiPair[mipi_pair].SCLK);
                ret += myDM.DPINOn(mipiPair[mipi_pair].SDATA);
            }
            else
            {
                //disconnect mipi
                ret += myDM.MIPI_Connect(moduleAlias, mipi_pair, 0);
                ret += myDM.DPINOff(mipiPair[mipi_pair].SVIO);
                ret += myDM.DPINOff(mipiPair[mipi_pair].SCLK);
                ret += myDM.DPINOff(mipiPair[mipi_pair].SDATA);
            }

            return ret;
        }
        public int ClampCurrent(string PinAlias, double currentLevel)
        {
            ret += myDM.ConfigurePMUCurrentLimitRange(PinAlias, currentLevel);
            return ret;
        }
        public int DriveVoltage(string PinAlias, double voltageLevel)
        {
            ret += myDM.ConfigurePMUVoltageLevel(PinAlias, voltageLevel);
            return ret;
        }
        /// <summary>
        /// Register Read
        /// </summary>
        /// <param name="mipi_pair">pair 0 = pin 0,1,2 (CLK DATA VIO); pair 1 = pin 3,4,5 (CLK DATA VIO)...pair 3 max</param>
        /// <param name="slaveaddress">max 0x0f hex</param>
        /// <param name="address">max 0x1f hex [4:0]</param>
        /// <param name="data">max 0xff hex [7:0]</param>
        /// <param name="isfullSpeed">true = fullspeed(26MHz), false = halfspeed(13MHz)</param>
        /// <returns>0 = no error</returns>
        public int Mipi_Read(int mipi_pair, int slaveaddress, int address, int data, bool isfullSpeed)
        {
            int speed = 0;
            //full speed of half speed read
            if (isfullSpeed)
                speed = 1;
            else
                speed = 0;

            //command frame
            int command = (slaveaddress << 8) + 0x60 + (address & 0x1f);

            //data frame
            int[] tempdata = new int[1];
            tempdata[0] = data;

            //reg read
            ret += myDM.MIPI_Read(moduleAlias, mipi_pair, speed, command, tempdata);

            return ret;
        }
        /// <summary>
        /// Register Write
        /// </summary>
        /// <param name="mipi_pair">pair 0 = pin 0,1,2 (CLK DATA VIO); pair 1 = pin 3,4,5 (CLK DATA VIO)...pair 3 max</param>
        /// <param name="slaveaddress">max 0x0f hex</param>
        /// <param name="address">max 0x1f hex [4:0]</param>
        /// <param name="data">max 0xff (1 byte data)[7:0]</param>
        /// <returns></returns>
        public int Mipi_Write(int mipi_pair, int slaveaddress, int address, int data)
        {
            //command frame
            int command = ((slaveaddress & 0x1f) << 8) + 0x40 + (address & 0x1f);

            //data frame
            int[] tempdata = new int[1];
            tempdata[0] = data;

            //reg write
            ret += myDM.MIPI_Write(moduleAlias, mipi_pair, command, tempdata);//DM482.DM482e_MIPI_RFFE_WR(session, mipi_pair, command, tempdata);

            return ret;
        }
        public int Mipi_Retrieve(int mipi_pair, out int rd_byte_data_count, int[] rd_data_array, out int[] rd_data_array_hex, int[] parity_check_array)
        {
            int rd_byte_data_count_ = 0;
            ret += myDM.MIPI_Retrieve(moduleAlias, mipi_pair, ref rd_byte_data_count_, rd_data_array, parity_check_array);

            //decode to hex value
            rd_data_array_hex = new int[rd_data_array.Length];
            for (int i = 0; i < rd_data_array.Length; i++)
            {
                rd_data_array_hex[i] = decodetohexvalue(rd_data_array[i]);
            }
            rd_byte_data_count = rd_byte_data_count_;
            return ret;
        }
        private int decodetohexvalue(int raw)
        {
            int result = 0;
            int[] tempdata = new int[2];
            tempdata[0] = (raw & 0xff00) >> 8;
            tempdata[1] = raw & 0xff;

            //raw to hex table 
            result = ((rawtohex(tempdata[0])) << 4) | rawtohex(tempdata[1]);

            return result;
        }
        private int rawtohex(int rawbyte)
        {
            int result = 0;
            switch (rawbyte)
            {
                case 0x00:
                    result = 0x00;
                    break;
                case 0x01:
                    result = 0x01;
                    break;
                case 0x04:
                    result = 0x02;
                    break;
                case 0x05:
                    result = 0x03;
                    break;
                case 0x10:
                    result = 0x04;
                    break;
                case 0x11:
                    result = 0x05;
                    break;
                case 0x14:
                    result = 0x06;
                    break;
                case 0x15:
                    result = 0x07;
                    break;
                case 0x40:
                    result = 0x08;
                    break;
                case 0x41:
                    result = 0x09;
                    break;
                case 0x44:
                    result = 0x0A;
                    break;
                case 0x45:
                    result = 0x0B;
                    break;
                case 0x50:
                    result = 0x0C;
                    break;
                case 0x51:
                    result = 0x0D;
                    break;
                case 0x54:
                    result = 0x0E;
                    break;
                case 0x55:
                    result = 0x0F;
                    break;
                default:
                    result = -1;
                    break;
            }
            return result;
        }
        /// Extended Register Read
        /// </summary>
        /// <param name="mipi_pair">pair 0 = pin 0,1,2 (CLK DATA VIO); pair 1 = pin 3,4,5 (CLK DATA VIO)...pair 3 max</param>
        /// <param name="slaveaddress">max 0x0f hex</param>
        /// <param name="address">max 0xff (1 byte data)[7:0]</param>
        /// <param name="data">max 16 array size</param>
        /// <param name="byteCount">max 16</param>
        /// <param name="isfullSpeed">true = fullspeed(26MHz), false = halfspeed(13MHz)</param>
        /// <returns>0 = no error</returns>
        public int Mipi_Read_ext(int mipi_pair, int slaveaddress, int address, int data, int byteCount, bool isfullSpeed)
        {
            int speed = 0;

            //full speed of half speed read
            if (isfullSpeed)
                speed = 1;
            else
                speed = 0;

            //command frame
            int command = (slaveaddress << 8) + 0x20 + (byteCount & 0x0f);

            //data frame
            int[] Address_data = new int[1];
            Address_data[0] = address;

            //reg read
            ret += myDM.MIPI_Read(moduleAlias, mipi_pair, speed, command, Address_data);

            return ret;
        }
        /// Extended Register Write 
        /// </summary>
        /// <param name="mipi_pair">pair 0 = pin 0,1,2 (CLK DATA VIO); pair 1 = pin 3,4,5 (CLK DATA VIO)...pair 3 max</param>
        /// <param name="slaveaddress">max 0x0f hex</param>
        /// <param name="Address">max 0xff [7:0]</param>
        /// <param name="data">max 16 data array</param>
        /// <param name="byteCount">max 16</param>
        /// <returns></returns>
        public int Mipi_Write_ext(int mipi_pair, int slaveaddress, int Address, int data, int byteCount)
        {
            //command frame
            int command = ((slaveaddress & 0x0f) << 8) + (byteCount & 0x0f);

            //data frame
            int[] Address_data = new int[byteCount + 2];
            Address_data[0] = Address;
            for (int i = 0; i <= byteCount; i++)
            {
                Address_data[i + 1] = data;
            }

            //reg write
            ret += ret += myDM.MIPI_Write(moduleAlias, mipi_pair, command, Address_data);//DM482.DM482e_MIPI_RFFE_WR(session, mipi_pair, command, Address_data);

            return ret;
        }

        #endregion

        #region test mipi
        public bool Register_Change(int Mipi_Reg)
        {
            int limit = 0;
            int[] MIPI_arr = new int[Mipi_Reg];
            bool readSuccessful = false;
            bool[] T_ReadSuccessful = new bool[Mipi_Reg];
            string[] regX_value = new string[Mipi_Reg];
            string[] MIPI_RegCond = new string[Mipi_Reg];
            int i;
            int reg_Cnt;
            int PassRd, FailRd;
            string result = "";

            //Initialize variable
            i = 0; reg_Cnt = 0;
            for (reg_Cnt = 0; reg_Cnt < Mipi_Reg; reg_Cnt++)
            {

                switch (reg_Cnt)
                {
                    case 0:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg0;
                        break;
                    case 1:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg1;
                        break;
                    case 2:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg2;
                        break;
                    case 3:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg3;
                        break;
                    case 4:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg4;
                        break;
                    case 5:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg5;
                        break;
                    case 6:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg6;
                        break;
                    case 7:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg7;
                        break;
                    case 8:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg8;
                        break;
                    case 9:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg9;
                        break;
                    case 10:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegA;
                        break;
                    case 11:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegB;
                        break;
                    case 12:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegC;
                        break;
                    case 13:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegD;
                        break;
                    case 14:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegE;
                        break;
                    case 15:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegF;
                        break;
                    default:
                        MessageBox.Show("Total Register Number : " + Mipi_Reg + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                        break;
                }
            }

            while (true)
            {
                reg_Cnt = 0; PassRd = 0; FailRd = 0; //reset read success counter

                for (reg_Cnt = 0; reg_Cnt < Mipi_Reg; reg_Cnt++)
                {
                    if (MIPI_RegCond[reg_Cnt].ToUpper() != "X")
                        WRITE_Register_Address(MIPI_RegCond[reg_Cnt], reg_Cnt);
                }

                TRIG();

                for (reg_Cnt = 0; reg_Cnt < Mipi_Reg; reg_Cnt++)
                {
                    T_ReadSuccessful[reg_Cnt] = true;
                    regX_value[reg_Cnt] = "";

                    if (MIPI_RegCond[reg_Cnt].ToUpper() != "X")
                    {
                        Read_Register_Address(ref result, reg_Cnt);
                        regX_value[reg_Cnt] = result;
                    }
                    else
                    {
                        regX_value[reg_Cnt] = MIPI_RegCond[reg_Cnt];
                    }

                    if (MIPI_RegCond[reg_Cnt] != regX_value[reg_Cnt] && LibEqmtDriver.MIPI.Lib_Var.ºReadFunction == true)
                        T_ReadSuccessful[reg_Cnt] = false;
                    else
                        T_ReadSuccessful[reg_Cnt] = true;
                }

                for (reg_Cnt = 0; reg_Cnt < Mipi_Reg; reg_Cnt++)
                {
                    if (T_ReadSuccessful[reg_Cnt] == true)
                        PassRd++;
                    else
                        FailRd++;
                }

                if (PassRd == (Mipi_Reg))
                {
                    readSuccessful = true;
                    break;
                }
                else
                    readSuccessful = false;

                limit = limit + 1;


                if (limit > 10) break;
            }
            return readSuccessful;
        }
        public bool Register_Change_Custom(string _cmd, string _cmdTrig, bool ext_reg)
        {
            //_cmd must be in this format -> "01:01 02:00 05:00 06:40 07:00 08:00 09:00 1C:01 1C:02 1C:03 1C:04 1C:05 1C:06 1C:07"
            // ext_reg use when your register address is above 1F (5 bit)
            bool result;
            int limit = 0;

            while (true)
            {
                result = false;
                WriteRegister_Rev2(_cmd, ext_reg);
                if (_cmdTrig.ToUpper() != "NONE")
                {
                    WriteRegister_Rev2(_cmdTrig, ext_reg);      //write PM Trigger
                }
                ReadRegister_Rev2(_cmd, ext_reg, out result);

                if (result)
                    break;      //exit loop when result = true

                limit = limit + 1;
                if (limit > 10) break;      //allow 10 try before exit
            }
            return result;
        }
        public bool Register_ChangeRev2(int Mipi_Reg)
        {
            int limit = 0;
            int[] MIPI_arr = new int[Mipi_Reg];
            bool readSuccessful = false;
            bool[] T_ReadSuccessful = new bool[Mipi_Reg];
            string[] regX_value = new string[Mipi_Reg];
            string[] MIPI_RegCond = new string[Mipi_Reg];
            int i;
            int reg_Cnt;
            int PassRd, FailRd;
            string result = "";

            //Initialize variable
            i = 0; reg_Cnt = 0;
            for (reg_Cnt = 0; reg_Cnt < Mipi_Reg; reg_Cnt++)
            {

                switch (reg_Cnt)
                {
                    case 0:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg0;
                        break;
                    case 1:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg1;
                        break;
                    case 2:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg2;
                        break;
                    case 3:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg3;
                        break;
                    case 4:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg4;
                        break;
                    case 5:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg5;
                        break;
                    case 6:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg6;
                        break;
                    case 7:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg7;
                        break;
                    case 8:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg8;
                        break;
                    case 9:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_Reg9;
                        break;
                    case 10:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegA;
                        break;
                    case 11:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegB;
                        break;
                    case 12:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegC;
                        break;
                    case 13:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegD;
                        break;
                    case 14:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegE;
                        break;
                    case 15:
                        MIPI_RegCond[reg_Cnt] = LibEqmtDriver.MIPI.Lib_Var.ºMIPI_RegF;
                        break;
                    default:
                        MessageBox.Show("Total Register Number : " + Mipi_Reg + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                        break;
                }
            }
                while (true)
                {
                    reg_Cnt = 0; PassRd = 0; FailRd = 0; //reset read success counter

                    for (reg_Cnt = 0; reg_Cnt < Mipi_Reg; reg_Cnt++)
                    {
                        if (MIPI_RegCond[reg_Cnt].ToUpper() != "X")
                            WRITE_Register_Address(MIPI_RegCond[reg_Cnt], reg_Cnt);
                    }

                    TRIG_REV2();

                    for (reg_Cnt = 0; reg_Cnt < Mipi_Reg; reg_Cnt++)
                    {
                        T_ReadSuccessful[reg_Cnt] = true;
                        regX_value[reg_Cnt] = "";

                        if (MIPI_RegCond[reg_Cnt].ToUpper() != "X")
                        {
                            Read_Register_Address(ref result, reg_Cnt);
                            regX_value[reg_Cnt] = result;
                        }
                        else
                        {
                            regX_value[reg_Cnt] = MIPI_RegCond[reg_Cnt];
                        }

                        if (MIPI_RegCond[reg_Cnt] != regX_value[reg_Cnt] && LibEqmtDriver.MIPI.Lib_Var.ºReadFunction == true)
                            T_ReadSuccessful[reg_Cnt] = false;
                        else
                            T_ReadSuccessful[reg_Cnt] = true;
                    }

                    for (reg_Cnt = 0; reg_Cnt < Mipi_Reg; reg_Cnt++)
                    {
                        if (T_ReadSuccessful[reg_Cnt] == true)
                            PassRd++;
                        else
                            FailRd++;
                    }

                    if (PassRd == (Mipi_Reg))
                    {
                        readSuccessful = true;
                        break;
                    }
                    else
                        readSuccessful = false;

                    limit = limit + 1;


                    if (limit > 10) break;
                }
            return readSuccessful;
        }
        public void TRIG()
        {
            //Mipi_Write(pairNo, slaveaddr, 0x1c, 0x03);
            Mipi_Write(pairNo, slaveaddr, pmTrigAddr, pmTrigData);
        }
        public void TRIG_REV2()
        {
            //Note : use default PM TRigger for all 7 register 
            //Default Set -> 1C:01 1C:02 1C:03 1C:04 1C:05 1C:06 1C:07
            string pmTrig = "1C:01 1C:02 1C:03 1C:04 1C:05 1C:06 1C:07";
            string[] pmTrigArray = pmTrig.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

            for (int i = 0; i < pmTrigArray.Length; i++)
            {
                string[] tmpData = pmTrigArray[i].Split(':');
                Mipi_Write(pairNo, slaveaddr, Convert.ToInt32(tmpData[0], 16), Convert.ToInt32(tmpData[1],16));
            }
        }
        public void WRITE_Register_Address(string _cmd, int RegAddr)
        {
            int data = int.Parse(_cmd, System.Globalization.NumberStyles.HexNumber);
            Mipi_Write(pairNo, slaveaddr, RegAddr, data);
        }
        public string Read_Register_Address(ref string x, int RegAddr)
        {
            int bytecount = 1;
            int dum = 0x0;
            //read
            Mipi_Read(pairNo, slaveaddr, RegAddr, dum, true);

            //retrieve
            int count = 0;
            int[] dataarray = new int[bytecount + 1];
            int[] datahex = new int[bytecount + 1];
            int[] parityarray = new int[bytecount + 1];
            Mipi_Retrieve(pairNo, out count, dataarray, out datahex, parityarray);

            //Mipi_Retrieve(1, out count, dataarray, out datahex, parityarray);

            string tempresult = "";
            // F -> 0F
            if (datahex[0] <= 15)
            {
                tempresult = "0" + datahex[0].ToString("X");
            }
            else
            {
                tempresult = datahex[0].ToString("X");
            }
            x = tempresult;
            return x;
        }
        public string Read_Register_Address_Rev2(ref string x, int RegAddr, bool extReg)
        {
            int bytecount = 1;
            int dum = 0x0;
            //read
            if (!extReg)
            {
                Mipi_Read(pairNo, slaveaddr, RegAddr, dum, true);
            }
            else
            {
                Mipi_Read_ext(pairNo, slaveaddr, RegAddr, dum, 0, true);
            }
            
            //retrieve
            int count = 0;
            int[] dataarray = new int[bytecount + 1];
            int[] datahex = new int[bytecount + 1];
            int[] parityarray = new int[bytecount + 1];
            Mipi_Retrieve(pairNo, out count, dataarray, out datahex, parityarray);

            //Mipi_Retrieve(1, out count, dataarray, out datahex, parityarray);

            string tempresult = "";
            // F -> 0F
            if (datahex[0] <= 15)
            {
                tempresult = "0" + datahex[0].ToString("X");
            }
            else
            {
                tempresult = datahex[0].ToString("X");
            }
            x = tempresult;
            return x;
        }
        public void WriteRegister_Rev2(string _cmd, bool extReg)
        {
            string biasData = _cmd;
            string[] biasDataArr = biasData.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
            for (int i = 0; i < biasDataArr.Length; i++)
            {
                string[] tmpData = biasDataArr[i].Split(':');
                if (!extReg)
                {
                    Mipi_Write(pairNo, slaveaddr, Convert.ToInt32(tmpData[0], 16), Convert.ToInt32(tmpData[1], 16));
                }
                else
                {
                    Mipi_Write_ext(pairNo, slaveaddr, Convert.ToInt32(tmpData[0], 16), Convert.ToInt32(tmpData[1], 16), 0);
                }
            }
        }
        public void ReadRegister_Rev2(string _cmd, bool extReg, out bool readSuccessful)
        {
            int reg_Cnt;
            int PassRd, FailRd;

            //Initialize variable
            reg_Cnt = 0; PassRd = 0; FailRd = 0;
            string result = "";
            readSuccessful = false;

            string biasData = _cmd;
            string[] biasDataArr = biasData.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

            bool[] T_ReadSuccessful = new bool[biasDataArr.Length];
            string[] regX_value = new string[biasDataArr.Length];

            for (int i = 0; i < biasDataArr.Length; i++)
            {
                T_ReadSuccessful[i] = true;
                regX_value[i] = "";

                string[] tmpData = biasDataArr[i].Split(':');
                string mipi_RegCond = tmpData[1];

                Read_Register_Address_Rev2(ref result, Convert.ToInt32(tmpData[0], 16), extReg);
                regX_value[i] = result;

                if (mipi_RegCond != regX_value[i] && LibEqmtDriver.MIPI.Lib_Var.ºReadFunction == true)
                    T_ReadSuccessful[i] = false;
                else
                    T_ReadSuccessful[i] = true;
            }

            for (reg_Cnt = 0; reg_Cnt < biasDataArr.Length; reg_Cnt++)
            {
                if (T_ReadSuccessful[reg_Cnt] == true)
                    PassRd++;
                else
                    FailRd++;
            }

            if (PassRd == (biasDataArr.Length))
                readSuccessful = true;
            else
                readSuccessful = false;
        }
        #endregion

    }
}
