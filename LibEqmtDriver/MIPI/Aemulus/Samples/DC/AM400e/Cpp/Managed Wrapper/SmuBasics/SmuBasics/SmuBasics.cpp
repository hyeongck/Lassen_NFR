// SmuBasics.cpp : main project file.

#include "stdafx.h"

using namespace System;
using namespace System::IO;
using namespace System::Diagnostics;
using namespace System::Reflection;
using namespace Aemulus::Hardware;

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
	int ret						= 0;
	int testHead				= 0;
	int testSite				= 0;
	int initOption				= 0xf;
	bool offline				= false;
	String ^ HardwareProfile	= nullptr;
	
	array<double> ^ volt = gcnew array<double>(4);
	array<double> ^ curr = gcnew array<double>(4);

	HardwareProfile = Path::Combine(Environment::CurrentDirectory, "example.amap");

	SMU ^ smu = nullptr;
	try 
	{
		 smu = gcnew SMU(HardwareProfile, testHead, testSite, offline, initOption);
	}
	catch (Exception ^ex)
	{
		Console::WriteLine(ex);
		Console::ReadLine();
		return 0;
	}

	//V1-4 (AM430e)
	
	ret += smu->SetIntegration("V1-4", SmuIntegration::Is50Hz);
	ret += smu->SetNPLC("V1-4", 1);
	ret += smu->SetBandwidth("V1-4", SmuBandwidth::Normal);

	ret += smu->ClampCurrent("V1-4", 100e-6);
    ret += smu->DriveVoltage("V1-4", 0);
	ret += smu->OnSmuPin("V1-4", true, false);
    delay_s(1e-3);
	ret += smu->DriveVoltage("V1-4", 3.3);
    delay_s(1e-3);
	ret += smu->ReadVoltage("V1-4", volt);
	ret += smu->ReadCurrent("V1-4", curr);

	Console::WriteLine("V1-4:");
    for (int i = 0; i < 4; i++)
		Console::WriteLine("curr[" + i + "]: " + curr[i] + "; volt[" + i + "]: " + volt[i]);

	ret += smu->DriveVoltage("V1-4", 0);
    delay_s(1e-3);
	ret += smu->OffSmuPin("V1-4");

	//Vcc (AM471e)

	ret += smu->SetIntegration("Vcc", SmuIntegration::Is50Hz);
	ret += smu->SetNPLC("Vcc", 1);
	ret += smu->SetBandwidth("Vcc", SmuBandwidth::Normal);

	ret += smu->ClampCurrent("Vcc", 100e-6);
    ret += smu->DriveVoltage("Vcc", 0);
	ret += smu->OnSmuPin("Vcc", true, false);
    delay_s(1e-3);
	ret += smu->DriveVoltage("Vcc", 3.3);
    delay_s(1e-3);
	ret += smu->ReadVoltage("Vcc", volt);
	ret += smu->ReadCurrent("Vcc", curr);

	Console::WriteLine("");
	Console::WriteLine("Vcc:");
	Console::WriteLine("curr: " + curr[0] + "; volt: " + volt[0]);

	ret += smu->DriveVoltage("Vcc", 0);
    delay_s(1e-3);
	ret += smu->OffSmuPin("Vcc");

	Console::ReadLine();
   
    return 0;
}
