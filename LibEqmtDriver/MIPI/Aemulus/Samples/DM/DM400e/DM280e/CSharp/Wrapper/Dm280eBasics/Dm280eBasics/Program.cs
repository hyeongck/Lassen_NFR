using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using Aemulus.Hardware;

namespace DmBasics
{
    class Program
    {
        static void Main(string[] args)
        {
            int ret						= 0;
	        int testHead				= 0;
	        int testSite				= 0;
	        int initOption				= 0x0;
	        bool offline				= true;
	        String HardwareProfile	    = null;
	        HardwareProfile             = Path.Combine(Environment.CurrentDirectory, "example.amap");
	        DM dm						= null;
	       
		       try
		       {
			       dm	= new DM(HardwareProfile, testHead, testSite, offline,initOption);
		       }
		       catch (Exception ex)
		       {
			       Console.WriteLine(ex);
			       Console.ReadLine();
			   }
            int count = 0;
            int data_count = 10;
            int[] dataRead = new int[data_count];
            int[] dataRead_1 = new int[data_count];
            double vio = 0;
            int[] parity = new int[data_count];
            int[] data = new int[data_count];
            data[0] = 0x12; //Address [15:8]
            data[1] = 0x31; //Byte 1 data
            data[2] = 0x31; //Byte 2 data
            data[3] = 0x31; //Byte 3 data
            dataRead[0] = 0x12;
            ret += dm.MIPI_ConfigureClock("280e",26000000);
            ret += dm.MIPI_ConfigureLoopback("280e_1", 1);
            ret += dm.MIPI_ConfigureVoltageSupply("280e", 1.9, ref vio);
            ret += dm.MIPI_ConfigureDelay("280e_1", 0);
            ret += dm.MIPI_Write("280e_1", (0xF << 8) | (0x0 << 4) | (0x2), data);//reg write
            ret += dm.MIPI_Read("280e_1", 1, (0xF << 8) | (0x2 << 4) | (0x2), dataRead);//reg read 
            ret += dm.MIPI_Retrieve("280e_1",ref count, ref dataRead_1, ref parity);
            ret += dm.Reset("280e");
            Console.ReadLine();
        }
    }
}
