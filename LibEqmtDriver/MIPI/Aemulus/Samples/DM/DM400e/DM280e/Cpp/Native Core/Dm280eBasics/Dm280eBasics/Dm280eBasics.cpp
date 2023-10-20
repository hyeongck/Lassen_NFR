// Dm280eBasics.cpp : main project file.
#include "stdafx.h"
#include <iostream>
#include "windows.h"
#include "AemDM.h"
//#include "stdio.h"
using namespace std;
using namespace aemdm;
using namespace System;
using namespace System::IO;



void CheckError(ViSession vi, int statusCode, char* msg)
{
	if (statusCode != 0)
	{
		strcpy_s(msg, sizeof(msg), "");
		DM280e_GetErrorMessage(vi, statusCode, msg);
		cout << "ErrorCode: " << statusCode << "\nErrorMsg: " << msg << endl;
	}
}

int main(array<System::String ^> ^args)
{
	int ret = 0;
	char msgbuf[BUFSIZ];
	ViSession vi;
	ViInt32 rddata[10]={0};
	ViInt32 rd_byte_data_count=0;
	ViInt32 parity[10]={0};

	ViInt32	Command  = 0;
	ViInt32	Data[10] = {0};
	ViInt32 DataRead[10] = {0};
	ViInt32 speed = 1; // 1 for full speed, 0 for half speed
	ViInt32 ch = 0 ;
	ViReal64 vio=0;

	HMODULE DM280elib = LoadLibrary(L"DM280e.dll");
	if (DM280elib == NULL)
	{
		cout << "Fail to load DM280e.dll!" << endl;
		return 0;
	}

	ret += aemdm_LoadFunctions(DM280elib);
	if (ret)
	{
		cout << "Fail to load DM280e.dll functions!" << endl;
		return 0;
	}


	ret += DM280e_Initialize("PXI2::0::INSTR",0x0,"Simulate=0, DriverSetup=Model:DM280e", &vi);
	if (ret)
	{
		if (vi == NULL)
		{
			cout << "Could not initialize the specified channels." << endl;
		}
		else
		{
			CheckError(vi, ret, msgbuf);
			cout << "Fail to DM280e_Initialize!\nErrorCode: " << ret << "\nErrorMsg: " << msgbuf << endl;			
		}

		return 0;
	}
	
	CheckError(vi,DM280e_CONFIGURE_VOLTAGE_SUPPLY (vi, 1.5,&vio) ,msgbuf);
	CheckError(vi, DM280e_CONFIGURE_MIPI_CLOCK(vi,320000) , msgbuf);
	CheckError(vi, DM280e_CONFIGURE_LOOPBACK (vi,0,1),msgbuf);
	Command =(0xF <<8) | (0x6 << 3) | (0x2);
	//Extended register write long, 3 bytes of data
	Data[0] = 0x1; //Address [15:8]
	Data[1] = 0x23; //Address [7:0]
	Data[2] = 0x31; //Byte 1 data
	Data[3] = 0x31; //Byte 2 data
	Data[4] = 0x31; //Byte 3 data
	CheckError(vi, DM280e_MIPI_RFFE_WR(vi, 0, Command, Data),msgbuf);

	DataRead[0] = 0x1;
	DataRead[1] = 0x23;

	Command = (0xF <<8) | (0x2 << 4) | (0x2);
	//Specifies command for extended Read
	CheckError(vi, DM280e_MIPI_RFFE_RD(vi,ch,speed,Command, DataRead),msgbuf);

	CheckError(vi, DM280e_MIPI_RFFE_RETRIEVE(vi, 0,&rd_byte_data_count,rddata,parity ),msgbuf);
	Console::ReadLine();
	
	FreeLibrary(DM280elib);

	return 0;

}

