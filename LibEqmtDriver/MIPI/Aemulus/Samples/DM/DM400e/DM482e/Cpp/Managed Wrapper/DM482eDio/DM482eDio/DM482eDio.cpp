///////////////////////////////////////////////////////////////////////////////
//               Aemulus DM482e
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2013.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  This example shows how to operate DM482e in DIO mode.
// 
///////////////////////////////////////////////////////////////////////////////
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
	int pingroup				= 1;
	int initOption				= 0xf;
	bool offline				= false;
	String ^ HardwareProfile	= nullptr;

	HardwareProfile = Path::Combine(Environment::CurrentDirectory, "example.amap");

	DM ^ dm = nullptr;
	try 
	{
		 dm = gcnew DM(HardwareProfile, pingroup, testHead, testSite, offline, initOption);
	}
	catch (Exception ^ex)
	{
		Console::WriteLine(ex);
		Console::ReadLine();
		return 0;
	}

	double vih = 2.8;
	double vil = 0;
	double voh = 2.0;
	double vol = 0.5;
	double vch = 3.0;
	double vcl = -0.5;
	double ioh = 0; //not used (active load disabled)
	double iol = 0; //not used (active load disabled)
	double vt = 0; //not used (input termination disabled)
	int val = 0;
	int bit[2] = {0,0};
	String ^ readval = nullptr;

	ret += dm->Reset("dm482e");

	ret += dm->Force("p0", 2);
	ret += dm->ConfigurePEAttribute("p0", false, false, false, false);
	ret += dm->DPINLevel("p0", vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
	ret += dm->DPINOn("p0");

	ret += dm->Force("p1", 2);
	ret += dm->ConfigurePEAttribute("p1", false, false, false, false);
	ret += dm->DPINLevel("p1", vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
	ret += dm->DPINOn("p1");

	ret += dm->Force("p2", 2);
	ret += dm->ConfigurePEAttribute("p2", false, false, false, false);
	ret += dm->DPINLevel("p2", vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
	ret += dm->DPINOn("p2");

	ret += dm->Force("p3", 2);
	ret += dm->ConfigurePEAttribute("p3", false, false, false, false);
	ret += dm->DPINLevel("p3", vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
	ret += dm->DPINOn("p3");

	ret += dm->Force("p4", 2);
	ret += dm->ConfigurePEAttribute("p4", false, false, false, false);
	ret += dm->DPINLevel("p4", vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
	ret += dm->DPINOn("p4");

	ret += dm->Force("p5", 2);
	ret += dm->ConfigurePEAttribute("p5", false, false, false, false);
	ret += dm->DPINLevel("p5", vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
	ret += dm->DPINOn("p5");

	ret += dm->SetPortDirection("dm482e", 0x3f); //all as outputs
	ret += dm->DrivePort("dm482e", 0x2a); //101010 = HLHLHL
	delay_s(1e-3);
	ret += dm->ReadPort("dm482e", val);

	Console::WriteLine("DrivePort:\t\tHLHLHL (0x2a)");
	Console::WriteLine("ReadPort (Expected):\tHLHLHL (10001000100)");
	
	for (int i=11; i>=0; i=i-2) //12 binary bits are returned for 6 pins (2 bits per pin)
	{
		bit[1] = val >> (i) & 0x1;
		bit[0] = val >> (i - 1) & 0x1;
		if(bit[1]==0 && bit[0]==0)
			readval = readval + "L";
		else if(bit[1]==0 && bit[0]==1)
			readval = readval + "H";
		else if(bit[1]==1 && bit[0]==0)
			readval = readval + "Z";
		else
			readval = readval + "?";
	}

	Console::WriteLine("ReadPort:\t\t" + readval + " (" + Convert::ToString(val, 2) + ")");	

	ret += dm->DrivePin("p0", 1);
	ret += dm->ReadPin("p0", val);

	ret += dm->DrivePort("dm482e", 0x0);
		
	ret += dm->DPINHVOff("p0");
	ret += dm->DPINHVOff("p1");
	ret += dm->DPINHVOff("p2");
	ret += dm->DPINHVOff("p3");
	ret += dm->DPINHVOff("p4");
	ret += dm->DPINHVOff("p5");
	ret += dm->Reset("dm482e");

	Console::ReadLine();
   
    return 0;
}
