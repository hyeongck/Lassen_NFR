using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aemulus.Hardware.DM280e;

namespace LibEqmtDriver.MIPI
{
    public struct s_MIPI_PAIR
    {
        public int PAIRNO;

        public string SCLK;
        public int SCLK_pinNo;

        public string SDATA;
        public int SDATA_pinNo;

        public string SVIO;
        public int SVIO_pinNo;   
    }

    public class Lib_Var
    {
        //MIPI
        public static bool ºMIPI_Enable { get; set; }
        public static string ºMIPI_Reg0 { get; set; }
        public static string ºMIPI_Reg1 { get; set; }
        public static string ºMIPI_Reg2 { get; set; }
        public static string ºMIPI_Reg3 { get; set; }
        public static string ºMIPI_Reg4 { get; set; }
        public static string ºMIPI_Reg5 { get; set; }
        public static string ºMIPI_Reg6 { get; set; }
        public static string ºMIPI_Reg7 { get; set; }
        public static string ºMIPI_Reg8 { get; set; }
        public static string ºMIPI_Reg9 { get; set; }
        public static string ºMIPI_RegA { get; set; }
        public static string ºMIPI_RegB { get; set; }
        public static string ºMIPI_RegC { get; set; }
        public static string ºMIPI_RegD { get; set; }
        public static string ºMIPI_RegE { get; set; }
        public static string ºMIPI_RegF { get; set; }

        public static int ºSlaveAddress { get; set; }
        public static int ºChannelUsed { get; set; }
        public static int ºPMTrig { get; set; }
        public static int ºPMTrig_Data { get; set; }
        public static int ºREG_Data { get; set; }
        public static bool ºReadSuccessful { get; set; }
        public static int ºDM280_CH0 { get; set; }
        public static int ºDM280_CH1 { get; set; }
        public static bool ºTestSwMarker { get; set; }
        public static bool ºORFS { get; set; }
        public static bool ºReadFunction { get; set; }

        public static int ºDM482_CH0 { get; set; }
        public static int ºDM482_CH1 { get; set; }

        // DM280 MIPI
        public static string ºmyDM280Address { get; set; }
        public static Aemulus_DM280e ºmyDM280 { get; set; }
        public static int ºChn_VIO { get; set; }

        // DM482 MIPI
        public static string ºmyDM482Address { get; set; }
        public static Aemulus_DM482e ºmyDM482e { get; set; }
        public static string ºHW_Profile { get; set; }

        // NI PXIe-6570 MIPI
        public static string ºmyNI6570Address { get; set; }
        public static NI_PXIe6570 ºmyNI6570 { get; set; }
        public static bool b_setNIVIO { get; set; }

        public class TestResult
        {
            public static double[] Icc_Arry, Pout_Arry, Icc_OnArry, Icc_OffArry;
            public static float
             leakage_VIO, leakage_SDATA, leakage_SCLK, Ich1, Ich2, Ich3, Ich4, Isum,
             Pout, Gain, Pin, Pae, Aclr1L, Aclr1U, Aclr2L, Aclr2U, Aclr3L, Aclr3U,
             EVM, Coup, H2, H3, NS, TxLeakage, TxLeakage2, TxLeakage3, Mipi_value, MaxGain,
             Gain_P1dB, Pout_P1dB, Pin_P1dB, Icc_P1dB, Pae_P1dB,
             Gain_P2dB, Pout_P2dB, Pin_P2dB, Icc_P2dB, Pae_P2dB,
             Gain_P3dB, Pout_P3dB, Pin_P3dB, Icc_P3dB, Pae_P3dB,
             RfOnTime, DcOnTime, DcOffTime, fBurstTime, TempSensor;
            public static bool
                Mipi_bool;
            public static int
                varLoop, indexP1db, indexP2db, indexP3db,
                MipiNumBitErrors, MipiOTPResult, MipiReg08Result, MipiReg09Result, MipiReg0FResult, MipiRegE3Result,
                MipiMID, MipiPID, MipiUSID, OTP_StatReg08, OTP_StatReg09, OTP_StatReg0F, OTP_StatRegE3;

        }
        public class TestParameter
        {
            public static bool
                 TestPout, TestPin, TestGain, TestCF, TestIch1, TestIch2, TestIch3, TestIch4, TestIsum, TestPAE, TestACP, TestNS07, TestNS12, TestNS15,
                TestTxleakage, TestTxleakage2, TestTxleakage3, TestH2, TestH3, TestEVM, TestMipiVio, TestMipiSclk, TestMipiSdata, TestP1dB, TestP2dB, TestP3dB, OTP_QA;
        }
    }

    public struct s_MIPI_DCSet
    {
        public int ChNo;
        public float VChSet;
        public float IChSet;
    }
    public struct s_MIPI_DCMeas
    {
        public int ChNo;
        public string MipiPinNames;
        public float VChMeas;
        public float IChMeas;
    }

    public interface iMiPiCtrl
    {
        void Init(s_MIPI_PAIR[] mipiPairCfg);
        void SendAndReadMIPICodes(out bool ReadSuccessful, int Mipi_Reg);
        void TurnOff_VIO(int MiPi_Pair);
        void TurnOn_VIO(int MiPi_Pair);
        void SendAndReadMIPICodesRev2(out bool ReadSuccessful, int Mipi_Reg, int MipiPairNo, int SlaveAddr);
        void SendAndReadMIPICodesCustom(out bool ReadSuccessful, string MipiRegMap, string PMTRigMap, int MipiPairNo, int SlaveAddr);
        void ReadMIPICodesCustom(out int ReadResult, string MipiRegMap, string PMTRigMap, int MipiPairNo, int SlaveAddr);
        void WriteOTPRegister(string efuseCtlReg_hex, string data_hex, int MipiPairNo, int SlaveAddr, bool invertData = false);
        void WriteMIPICodesCustom(string data_hex, int MipiPairNo, int SlaveAddr);
        void SetMeasureMIPIcurrent(int delayMs, int MipiPairNo, int SlaveAddr, s_MIPI_DCSet[] setDC_Mipi, string[] measDC_MipiCh, out s_MIPI_DCMeas[] measDC_Mipi); 
    }
}
