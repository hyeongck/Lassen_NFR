///////////////////////////////////////////////////////////////////////////////
//               Aemulus DM482e
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2013.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  This example shows how to operate DM482e in VECTOR mode.
//  Example of vector file is "example.txt".
//  Even-numbered pins are configured as outputs, whereas odd-numbered pins are inputs.
//  Outputs are loop-backed to inputs via external connnection
// 
///////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"

using namespace System;
using namespace System::IO;
using namespace System::Diagnostics;
using namespace System::Reflection;
using namespace Aemulus::Hardware::DM482e;

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
	int ret = 0;
	int pingroup = 1;
	String ^ VectorFile	= Path::Combine(Environment::CurrentDirectory, "example.txt");

	double vih = 2.8;
	double vil = 0;
	double voh = 2.0;
	double vol = 0.5;
	double vch = 3.0;
	double vcl = -0.5;
	double ioh = 0; //not used (active load disabled)
	double iol = 0; //not used (active load disabled)
	double vt = 0; //not used (active load disabled)

	array<int> ^ resourceArr = gcnew array<int>(1); //512k bits
	array<unsigned int> ^ history_ram = gcnew array<unsigned int>(128);

	int vector_status = 1;
	double timeout_ms = 100;
	int total_fail_count = 0;
	int total_vector = 33;

	DM482e ^ dm = nullptr;
	try
	{
		dm = gcnew DM482e("PXI48::0::INSTR", pingroup, 0xf, "Simulate=0, DriverSetup=Model:DM482e");
	}
	catch (Exception ^ex)
	{
		Console::WriteLine(ex);
		Console::ReadLine();
		return 0;
	}
	
	ret += dm->Reset();

	for (int i=0; i<6; i++)
	{
		ret += dm->DPINForce(i, 0);
		ret += dm->ConfigurePEAttribute(i, 0, 0, 0, 0); //disable input termination for input pins (high-z)
		ret += dm->DPINLevel(i, vih, vil, voh, vol, ioh, iol, vch, vcl, vt);
		ret += dm->DPINOn(i);
	}

	//Vector setting
	resourceArr[0] = 1;
	ret += dm->ConfigureVectorEngineAttribute(0, 0);
	ret += dm->DPINVectorResourceAllocation(1, resourceArr);
	ret += dm->DPINVecLoad(1, 0, VectorFile); //dedicated I/O
	ret += dm->DPINPeriod(0, 100e-6);

	ret += dm->ConfigureInputChannelDelay(1, 0);
	ret += dm->ConfigureInputChannelDelay(3, 0);
	ret += dm->ConfigureInputChannelDelay(5, 0);

	//Execute vector
	ret += dm->RunVector(0);

	//Check status
	Stopwatch ^ stopwatch = Stopwatch::StartNew();
	do
	{
		ret += dm->AcquireVecEngineStatus(vector_status);
		if (ret == 0 && vector_status == 0) //completed
			break;
		else if (ret) //error
			break;

		if (stopwatch->ElapsedMilliseconds >= timeout_ms)
            break;
	}
	while (vector_status == 1); //busy
		
	//Read back
	if(ret == 0 && vector_status == 0)
	{
		Console::WriteLine("Vector completed!");

		ret += dm->AcquireVectorFailCount(total_fail_count);
		Console::WriteLine("Total fail count: " + total_fail_count);

		for (int i=1; i<6; i=i+2)
		{
			ret += dm->AcquireChannelVectorFailCount(i, total_fail_count);
			Console::WriteLine("Total fail count (Pin" + i + "): " + total_fail_count);
		}

		ret += dm->ReadHistoryRam(total_vector, 0, 0, history_ram);
		for (int i=0; i<total_vector; i++)
			Console::WriteLine("Vector Line (" + i + "): " + System::Convert::ToString((int)history_ram[i],2));
	}
	else
		Console::WriteLine("Vector error!");

	for (int i=0; i<6; i++)
		ret += dm->DPINOff(i);

	ret += dm->Reset();

	Console::ReadLine();
   
    return 0;
}
