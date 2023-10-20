///////////////////////////////////////////////////////////////////////////////
//               Aemulus DM482e
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2013.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  This example shows how to operate DM482e as MIPI controller.
// 
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using Aemulus.Hardware;

namespace DM482eMipi
{
    class Program
    {
        static void delay_s(double secondsToWait)
        {
            double millisecondsToWait = secondsToWait * 1000;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
                {
                    break;
                }
            }
        }

        static void Main(string[] args)
        {
            int ret	= 0;
            int dpingroup_sel = 3;
            int testHead = 0;
            int testSite = 0;
            int initOption = 0xf;
            bool offline = true;
            String HardwareProfile = null;

            HardwareProfile = Path.Combine(Environment.CurrentDirectory, "example.amap");
            DM dm = null;

            try
            {
                dm = new DM(HardwareProfile, dpingroup_sel, testHead, testSite, offline, initOption);
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
            int[] data = new int[data_count];
            int[] parity = new int[data_count];
            data[0] = 0x12; //Address [15:8]
            data[1] = 0x31; //Byte 1 data
            data[2] = 0x31; //Byte 2 data
            data[3] = 0x31; //Byte 3 data
            dataRead[0] = 0x12;

            ret += dm.ResetGroup("482", 0);
            ret += dm.ResetGroup("482", 1);
            ret += dm.Force("482P0", 0);
            ret += dm.Force("482P1", 0);
            ret += dm.Force("482P2", 0);
            ret += dm.Force("482P3", 0);
            ret += dm.Force("482P4", 0);
            ret += dm.Force("482P5", 0);
            ret += dm.Force("482P6", 0);
            ret += dm.Force("482P7", 0);
            ret += dm.Force("482P8", 0);
            ret += dm.Force("482P9", 0);
            ret += dm.Force("482P10", 0);
            ret += dm.Force("482P11", 0);
            ret += dm.DPINOn("482P0");
            ret += dm.DPINOn("482P1");
            ret += dm.DPINOn("482P2");
            ret += dm.DPINOn("482P3");
            ret += dm.DPINOn("482P4");
            ret += dm.DPINOn("482P5");
            ret += dm.DPINOn("482P6");
            ret += dm.DPINOn("482P7");
            ret += dm.DPINOn("482P8");
            ret += dm.DPINOn("482P9");
            ret += dm.DPINOn("482P10");
            ret += dm.DPINOn("482P11");
			ret += dm.DPINLevel("482P0", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P1", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P2", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P3", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P4", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P5", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P6", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P7", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P8", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P9", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P10", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);
            ret += dm.DPINLevel("482P11", 2, 0, 6, -2, 0, 0, 7.5, -2.5, 0);

            ret += dm.MIPI_ConfigureClock("482", 1, 26000000);
            ret += dm.MIPI_Connect("482",1,1);
            ret += dm.MIPI_Write("482", 1, (0xF << 8) | (0x0 << 4) | (0x2), data); //Extended reg write
            ret += dm.MIPI_Read("482",1, 1, (0xF << 8) | (0x2 << 4) | (0x2), dataRead);//Extended reg read 
            ret += dm.MIPI_Retrieve("482",1, ref count, dataRead_1, parity);
            ret += dm.ResetGroup("482",0);
            ret += dm.ResetGroup("482",1);
            ret += dm.Close ("482");
	        Console.ReadLine();
        }
    }
}
