///////////////////////////////////////////////////////////////////////////////
//               Aemulus DM482e
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2013.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  This example shows how to operate DM482e in CLOCK mode.
//  2 pins are configured to generate differential outputs.
// 
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Aemulus.Hardware.DM482e;

namespace SmuBasics
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
            int ret = 0;
            int pingroup = 1;
            double vih = 2.8;
	        double vil = 0;
	        double voh = 2.0;
	        double vol = 0.5;
	        double vch = 3.0;
	        double vcl = -0.5;
	        double ioh = 0; //not used (active load disabled)
	        double iol = 0; //not used (active load disabled)
	        double vt = 0; //not used (active load disabled)

            DM482e dm = new DM482e("PXI48::0::INSTR", pingroup, 0xf, "Simulate=0, DriverSetup=Model:DM482e");

            ret += dm.Reset();

            ret += dm.DPINForce(0, 5); //clock
            ret += dm.DPINForce(1, 6); //inverted clock
            
            ret += dm.ConfigureClockFrequency(1e6);

            for (int i = 0; i < 2; i++)
            {
                ret += dm.ConfigurePEAttribute(i, 0, 0, 0, 0);
                ret += dm.DPINLevel(i, vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
                ret += dm.DPINOn(i);
            }

            //Pins 0-1 will generate differential outputs here

            ret += dm.Reset();

            for (int i = 0; i < 2; i++)
            {
                ret += dm.DPINOff(i);
            }
            
            Console.ReadLine();
        }
    }
}
