using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Aemulus.Hardware.DM280e;

namespace DmBasics
{
    class Program
    {
        static void Main(string[] args)
        {
            
            int data_count = 10;
            int [] dataRead = new int [data_count];
	        int [] dataRead_1 = new int[data_count];
            DM280e dm = new DM280e("PXI2::0::INSTR", 0x0, "Simulate=1,DriverSetup=Model:DM280e");
            int count = 0;
            double vio;
	        int [] parity = new int [data_count];
	        int [] data = new int [data_count]; 
	        data[0] = 0x12; //Address [15:8]
	        data[1] = 0x31; //Byte 1 data
	        data[2] = 0x31; //Byte 2 data
	        data[3] = 0x31; //Byte 3 data
	        dataRead [0] = 0x12;
            dm.MIPI_ConfigureVoltageSupply(1.9, out vio);
            dm.MIPI_ConfigureClock(26000000);
	        dm.MIPI_ConfigureLoopback(1,1);	
	        dm.MIPI_ConfigureDelay(1,0);
	        dm.MIPI_Write(1,(0xF <<8) | (0x0 << 4) | (0x2),data);//reg write
	        dm.MIPI_Read(1,1, (0xF <<8) | (0x2 << 4) | (0x2),dataRead);//reg read 
	        dm.MIPI_Retrieve(1,out count, dataRead_1, parity);
	        dm.Reset();
	        dm.Close();
	        Console.ReadLine();
            
        }
            
    }
}
