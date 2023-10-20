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
using Aemulus.Hardware.DM482e;

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
            DM482e dm = new DM482e("PXI30::0::INSTR",3, 0x0, "Simulate=1,DriverSetup=Model:DM482e");
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

            ret += dm.ResetGroup(0);
            ret += dm.ResetGroup(1);
            ret += dm.DPINForce(0, 0);
            ret += dm.DPINForce(1, 0);
            ret += dm.DPINForce(2, 0);
            ret += dm.DPINForce(3, 0);
            ret += dm.DPINForce(4, 0);
            ret += dm.DPINForce(5, 0);
            ret += dm.DPINForce(6, 0);
            ret += dm.DPINForce(7, 0);
            ret += dm.DPINForce(8, 0);
            ret += dm.DPINForce(9, 0);
            ret += dm.DPINForce(10, 0);
            ret += dm.DPINForce(11, 0);
            ret += dm.DPINOn(0);
            ret += dm.DPINOn(1);
            ret += dm.DPINOn(2);
            ret += dm.DPINOn(3);
            ret += dm.DPINOn(4);
            ret += dm.DPINOn(5);
            ret += dm.DPINOn(6);
            ret += dm.DPINOn(7);
            ret += dm.DPINOn(8);
            ret += dm.DPINOn(9);
            ret += dm.DPINOn(10);
            ret += dm.DPINOn(11);

            ret += dm.MIPI_Configure_Clock(3, 26000000);
            ret += dm.MIPI_Connect(1,1);
            ret += dm.MIPI_RFFE_WR( 1, (0xF << 8) | (0x0 << 4) | (0x2), data); //Extended reg write
            ret += dm.MIPI_RFFE_RD( 1, 1, (0xF << 8) | (0x2 << 4) | (0x2), dataRead);//Extended reg read 
            ret += dm.MIPI_RFFE_RETRIEVE(1, out count,out dataRead_1,out parity);
            ret += dm.ResetGroup( 0);
            ret += dm.ResetGroup( 1);
            
	        Console.ReadLine();
        }
    }
}
