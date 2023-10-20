// SmuBasics.cpp : Defines the entry point for the console application.
//
#include <iostream>
#include "windows.h"
#include "stdafx.h"
#include "AemDCPwr.h"

using namespace std;
using namespace aemdcpwr;

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

void CheckError(ViSession vi, int statusCode, char* msg)
{
	if (statusCode != 0)
	{
		strcpy_s(msg, sizeof(msg), "");
		AemDCPwr_GetErrorMessage(vi, statusCode, msg);
		cout << "ErrorCode: " << statusCode << "\nErrorMsg: " << msg << endl;
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	ViSession vi;
	char msgbuf[BUFSIZ];

	HMODULE AM400eLib = LoadLibrary(L"AemDCPwr.dll");
	if (AM400eLib == NULL)
	{
		cout << "Fail to load AemDCPwr.dll!" << endl;
		return 0;
	}

	int ret = aemdcpwr_LoadFunctions(AM400eLib);
	if (ret)
	{
		cout << "Fail to load AemDCPwr.dll functions!" << endl;
		return 0;
	}

	int status = AemDCPwr_InitChannels("PXI52::0::INSTR", "0-3", 0xf, "Simulate=1, DriverSetup=Model:AM430e", &vi);
	if (status)
	{
		if (vi == NULL)
		{
			cout << "Could not initialize the specified channels." << endl;
		}
		else
		{
			CheckError(vi, status, msgbuf);
			cout << "Fail to AemDCPwr_InitChannels!\nErrorCode: " << status << "\nErrorMsg: " << msgbuf << endl;			
		}

		return 0;
	}

	CheckError(vi, AemDCPwr_ConfigurePLF(vi, 50), msgbuf);
	CheckError(vi, AemDCPwr_ConfigureSamplingTime(vi, "0-3", 1, 1), msgbuf);
	CheckError(vi, AemDCPwr_ConfigureOutputTransient(vi, "0-3", 1), msgbuf);
	CheckError(vi, AemDCPwr_ConfigureSense(vi, "0-3", 1), msgbuf);

	CheckError(vi, AemDCPwr_ConfigureOutputFunction(vi, "0-3", 0), msgbuf);
	CheckError(vi, AemDCPwr_ConfigureCurrentLimit(vi, "0-3", 0, 100e-6), msgbuf);
	CheckError(vi, AemDCPwr_ConfigureVoltageLevel(vi, "0-3", 0), msgbuf);
	CheckError(vi, AemDCPwr_ConfigureOutputSwitch(vi, "0-3", 1), msgbuf); 

	Wait_Sec(0.001);
	CheckError(vi, AemDCPwr_ConfigureVoltageLevel(vi, "0-3", 3.3), msgbuf);
	Wait_Sec(0.001);

	double curr[4];
	double volt[4];

	CheckError(vi, AemDCPwr_Measure(vi, "0-3", 1, volt), msgbuf);
	CheckError(vi, AemDCPwr_Measure(vi, "0-3", 0, curr), msgbuf);

	for (int i=0; i<4; i++)
		cout << "curr[" << i << "]: " << curr[i] << "; volt[" << i << "]: " << volt[i] << endl;

	CheckError(vi, AemDCPwr_ConfigureVoltageLevel(vi, "0-3", 0), msgbuf);
	Wait_Sec(0.001);
	CheckError(vi, AemDCPwr_ConfigureOutputSwitch(vi, "0-3", 0), msgbuf);

	return 0;
}

