// DmBasics.cpp : main project file.

#include "stdafx.h"

using namespace System;
using namespace System::IO;
using namespace System::Diagnostics;
using namespace System::Reflection;
using namespace Aemulus::Hardware;

int main(array<System::String ^> ^args)
{
   int ret						= 0;
	int testHead				= 0;
	int testSite				= 0;
	int initOption				= 0x0;
	bool offline				= true;
	String ^ HardwareProfile	= nullptr;
	HardwareProfile = Path::Combine(Environment::CurrentDirectory, "example.amap");
	DM ^ dm						= nullptr;
	if (dm == nullptr)
	{ 
		try
		{
			dm	= gcnew DM(HardwareProfile, testHead, testSite, offline, initOption);
		}
		catch (Exception ^ex)
		{
			Console::WriteLine(ex);
			Console::ReadLine();
			return 0;
		}
	}
	//DM280e
	int count = 0;
	int data_count = 10;
	double vio;
	
	array<int> ^ parity =gcnew array <int>(data_count);
	array<int> ^ data = gcnew array <int>(data_count); 
	array<int>^ dataRead = gcnew array <int>(data_count);
	array<int>^ dataRead_1 = gcnew array <int>(data_count);

	data[0] = 0x12; //Address [15:8]
	data[1] = 0x31; //Byte 1 data
	data[2] = 0x31; //Byte 2 data
	data[3] = 0x31; //Byte 3 data
	dataRead [0] = 0x12;

	ret += dm->MIPI_ConfigureVoltageSupply("280e",1.9,vio);
	ret += dm->MIPI_ConfigureClock("280e",26000000);
	ret += dm->MIPI_ConfigureLoopback("280e_1",1);	
	ret += dm->MIPI_ConfigureDelay("280e_1",0);
	ret += dm->MIPI_Write("280e_1",(0xF <<8) | (0x0 << 4) | (0x2),data);//reg write
	ret += dm->MIPI_Read("280e_1",1, (0xF <<8) | (0x2 << 4) | (0x2),dataRead);//reg read 
	ret += dm->MIPI_Retrieve("280e_1",count,dataRead_1,parity);
	ret += dm->Reset("280e");
	
	return 0;
}
