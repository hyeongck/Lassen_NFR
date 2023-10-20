///////////////////////////////////////////////////////////////////////////////
//               Aemulus DM482e
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2013.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  This example shows how to operate DM482e in PMU mode (DVCI or DICV).
// 
///////////////////////////////////////////////////////////////////////////////
#include <iostream>
#include "windows.h"
#include "stdafx.h"
#include "DM482e.h"

using namespace std;
using namespace dm482e;

void Wait_Sec(double sec)
{
	LARGE_INTEGER startTime;
	LARGE_INTEGER stopTime;
	LARGE_INTEGER sysFreq;

	QueryPerformanceFrequency(&sysFreq);
	QueryPerformanceCounter(&startTime);
	do
	{
		QueryPerformanceCounter(&stopTime);
	}
	while (((double)(stopTime.QuadPart - startTime.QuadPart) / (double)sysFreq.QuadPart) < sec);
}

int _tmain(int argc, _TCHAR* argv[])
{
	ViSession vi;

	HMODULE DM400eLib = LoadLibrary(L"DM482e.dll");
	if (DM400eLib == NULL)
	{
		cout << "Fail to load DM482e.dll!" << endl;
		return 0;
	}

	int ret = DM482e_LoadFunctions(DM400eLib);
	if (ret)
	{
		cout << "Fail to load DM482e.dll functions!" << endl;
		return 0;
	}

	int dpingroup_sel = 3; //PIN0-11
	int pinNo = 0;
	double volt = 0;
	double curr = 0;

	ret = DM482e_DPINOpen("PXI48::0::INSTR", dpingroup_sel, 0xf, "Simulate=0, DriverSetup=Model:DM482e", &vi);
	if (ret)
	{
		if (vi == NULL)
			cout << "Could not initialize the module." << endl;
		else
			cout << "Fail to DM482e_DPINOpen!" << endl;			

		return 0;
	}

	ret += DM482e_Reset(vi);
	ret += DM482e_DPINForce(vi, pinNo, 1); //PMU (Pin Measurement Unit)
	ret += DM482e_ConfigurePEAttribute(vi, pinNo, false, false, false, false);
	ret += DM482e_DPINOn(vi, pinNo);

	ret += DM482e_ConfigurePMUOutputFunction(vi, pinNo, 0); //DVCI
	ret += DM482e_ConfigurePowerLineFrequency(vi, 50.0); //50Hz
	ret += DM482e_ConfigurePMUSamplingTime(vi, pinNo, 1, 1); //1 PLC
	ret += DM482e_ConfigurePMUSense(vi, pinNo, 1); //remote sense

	ret += DM482e_ConfigurePMUCurrentLimitRange(vi, pinNo, 20e-6);
	ret += DM482e_ConfigurePMUVoltageLevel(vi, pinNo, 5);
	Wait_Sec(0.001);
	ret += DM482e_PMUMeasure(vi, pinNo, 1, &volt); //Measure voltage
	ret += DM482e_PMUMeasure(vi, pinNo, 0, &curr); //Measure current

	cout << "DVCI (V=5V, I=20uA)" << endl;
	cout << "Measured voltage: " << volt << endl;
	cout << "Measured current: " << curr << endl;

	ret += DM482e_ConfigurePMUVoltageLevel(vi, pinNo, 0);
	Wait_Sec(0.001);

	ret += DM482e_ConfigurePMUOutputFunction(vi, pinNo, 1); //DICV
	ret += DM482e_ConfigurePMUVoltageLimit(vi, pinNo, 5.5, 0);
	ret += DM482e_ConfigurePMUCurrentLevelAndRange(vi, pinNo, 20e-6, 25e-3);
	Wait_Sec(0.001);
	ret += DM482e_PMUMeasure(vi, pinNo, 1, &volt); //Measure voltage
	ret += DM482e_PMUMeasure(vi, pinNo, 0, &curr); //Measure current

	cout << "DICV (I=20uA, V=5.5V)" << endl;
	cout << "Measured voltage: " << volt << endl;
	cout << "Measured current: " << curr << endl;

	ret += DM482e_ConfigurePMUCurrentLevel(vi, pinNo, 0);
	Wait_Sec(0.001);

	ret += DM482e_DPINOff(vi, pinNo);

	ret += DM482e_Reset(vi);
	ret += DM482e_DPINClose(vi);

	return 0;
}

