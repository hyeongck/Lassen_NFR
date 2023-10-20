// SmuBasics.cpp : main project file.

#include "stdafx.h"

using namespace System;
using namespace System::Diagnostics;
using namespace Aemulus::Hardware::SMU;

void delay_s(double secondsToWait)
{
	double millisecondsToWait = secondsToWait * 1000;
	Stopwatch ^ stopwatch = Stopwatch::StartNew();
    while (true)
    {
		if (stopwatch->ElapsedMilliseconds >= millisecondsToWait)
        {
            break;
        }
    }
}

int main(array<System::String ^> ^args)
{
	array<double> ^ volt = gcnew array<double>(4);
	array<double> ^ curr = gcnew array<double>(4);
	PxiSmu ^ smu = gcnew PxiSmu("PXI52::0::INSTR", "0-3", 0xf, "Simulate=1, DriverSetup=Model:AM430e");
	
	smu->ConfigurePowerLineFrequency(50);
	smu->ConfigureSamplingTime("0-3", 1, PxiSmuConstants::UnitPowerLineCycles);
	smu->ConfigureOutputTransient("0-3", 1);
	smu->ConfigureSense("0-3", PxiSmuConstants::SenseRemote);

	smu->ConfigureOutputFunction("0-3", PxiSmuConstants::DVCI);           
    smu->ConfigureCurrentLimit("0-3", 0, 100e-6);
    smu->ConfigureVoltageLevel("0-3", 0);
    smu->ConfigureOutputSwitch("0-3", 1);
    delay_s(1e-3);
    smu->ConfigureVoltageLevel("0-3", 3.3);
    delay_s(1e-3);
	smu->Measure("0-3", PxiSmuConstants::MeasureVoltage, volt);
	smu->Measure("0-3", PxiSmuConstants::MeasureCurrent, curr);

    for (int i = 0; i < 4; i++)
		Console::WriteLine("curr[" + i + "]: " + curr[i] + "; volt[" + i + "]: " + volt[i]);

	smu->ConfigureVoltageLevel("0-3", 0);
    delay_s(1e-3);
	smu->ConfigureOutputSwitch("0-3", 0);
	Console::ReadLine();
   
    return 0;
}
