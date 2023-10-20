// Dm280eBasics.cpp : main project file.

#include "stdafx.h"

using namespace System;
using namespace System::Diagnostics;
using namespace Aemulus::Hardware::DM280e;

int main(array<System::String ^> ^args)
{
	int data_count = 10;
	array<int>^ dataRead = gcnew array <int>(data_count);
	array<int>^ dataRead_1 = gcnew array <int>(data_count);
	DM280e ^ dm = gcnew DM280e("PXI2::0::INSTR", 0x0,"Simulate=0, DriverSetup=Model:DM280e");
	int count = 0;
	double vio = 0;
	array<int> ^ parity =gcnew array <int>(data_count);
	array<int> ^ data = gcnew array <int>(data_count); 
	data[0] = 0x12; //Address [15:8]
	data[1] = 0x31; //Byte 1 data
	data[2] = 0x31; //Byte 2 data
	data[3] = 0x31; //Byte 3 data
	dataRead [0] = 0x12;

	dm->MIPI_ConfigureVoltageSupply(2,vio);
	dm->MIPI_ConfigureClock((26000000));
	dm->MIPI_ConfigureLoopback(1,1);	
	dm->MIPI_ConfigureDelay(1,0);
	dm->MIPI_Write(1,(0xF <<8) | (0x0 << 4) | (0x2),data);//reg write
	dm->MIPI_Read(1,1, (0xF <<8) | (0x2 << 4) | (0x2),dataRead);//reg read 
	dm->MIPI_Retrieve(1,count,dataRead_1,parity);
	dm->Reset();
	Console::ReadLine();
    return 0;
}
