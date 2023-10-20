using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Aemulus.Hardware.SMU;

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
            double[] curr = new double[4];
            double[] volt = new double[4];

            PxiSmu smu = new PxiSmu("PXI52::0::INSTR", "0-3", 0xf, "Simulate=0, DriverSetup=Model:AM430e");

            smu.ConfigurePowerLineFrequency(50);
            smu.ConfigureSamplingTime("0-3", 1, PxiSmuConstants.UnitPowerLineCycles);
            smu.ConfigureOutputTransient("0-3", 1);
            smu.ConfigureSense("0-3", PxiSmuConstants.SenseRemote);

            smu.ConfigureOutputFunction("0-3", PxiSmuConstants.DVCI);           
            smu.ConfigureCurrentLimit("0-3", 0, 100e-6);
            smu.ConfigureVoltageLevel("0-3", 0);
            smu.ConfigureOutputSwitch("0-3", 1);
            delay_s(1e-3);
            smu.ConfigureVoltageLevel("0-3", 3.3);
            delay_s(1e-3);
            smu.Measure("0-3", PxiSmuConstants.MeasureVoltage, volt);
            smu.Measure("0-3", PxiSmuConstants.MeasureCurrent, curr);

            for (int i = 0; i < 4; i++)
                Console.WriteLine("curr[" + i + "]: " + curr[i] + "; volt[" + i + "]: " + volt[i]);

            smu.ConfigureVoltageLevel("0-3", 0);
            delay_s(1e-3);
            smu.ConfigureOutputSwitch("0-3", 0);
            Console.ReadLine();
        }
    }
}
