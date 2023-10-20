///////////////////////////////////////////////////////////////////////////////
//               Aemulus DMs
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2012.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  Title:    AemDM.cpp
//  Purpose:  Aemulus DMs Driver declarations
// 
///////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include "AemDM.h"


namespace aemdm
{
// TypeDefs ----------------------------------------

// Hardware management

// Initialize and Close Functions
typedef ViStatus (*PtrToDM280e_Initialize)(ViRsrc resourceName, ViInt32 init_options, ViConstString optionString, ViSession* vi);
typedef ViStatus (*PtrToDM280e_Close)(ViSession vi);
typedef ViStatus (*PtrToDM280e_Reset)(ViSession vi);

typedef ViStatus (*PtrToDM280e_MIPI_RFFE_WR)(ViSession vi, ViInt32 ch, ViInt32 Command, ViInt32* Data);
typedef ViStatus (*PtrToDM280e_MIPI_RFFE_RD)(ViSession vi, ViInt32 ch,ViInt32 speed, ViInt32 Command, ViInt32* Data);
typedef ViStatus (*PtrToDM280e_MIPI_RFFE_RETRIEVE)(ViSession vi, ViInt32 ch, ViInt32* rd_byte_data_count, ViInt32* rd_data, ViInt32* parity_check);

typedef ViStatus (*PtrToDM280e_CONFIGURE_MIPI_CLOCK)(ViSession vi, ViInt32 freq_Hz);
typedef ViStatus (*PtrToDM280e_CONFIGURE_MIPI_DELAY)(ViSession vi, ViInt32 ch, ViInt32 delay);

typedef ViStatus (*PtrToDM280e_CONFIGURE_VOLTAGE_SUPPLY)(ViSession vi, ViReal64 target_vio, ViReal64* actual_vio);

typedef ViStatus (*PtrToDM280e_CONFIGURE_LOOPBACK )(ViSession vi, ViInt32 ch, ViInt32 loopback);

typedef ViStatus (*PtrToDM280e_ReadDLLVersion)(ViChar* version);
typedef ViStatus (*PtrToDM280e_ReadFirmwareVersion) (ViSession vi, ViChar* version);
typedef ViStatus (*PtrToDM280e_ReadSerialNumber )(ViSession vi, ViChar* sn);
typedef ViStatus (*PtrToQueryModuleType) (ViSession vi, ViUInt32* module_type);

typedef ViStatus (*PtrToDM280e_CONFIGURE_BPC) (ViSession vi, ViInt32 BPC);
typedef ViStatus (*PtrToDM280e_ConfigureMultiSiteMode)(ViSession vi, ViInt32 mode );
typedef ViStatus (*PtrToDM280e_GetError)(ViSession vi, ViStatus* code, ViInt32 bufferSize, ViChar* description);
typedef ViStatus (*PtrToDM280e_GetErrorMessage)(ViSession vi,ViStatus errorCode, ViChar* errorMessage);
typedef ViStatus (*PtrToDM280e_ClearError)(ViSession vi);
///////////////////////////////////////////////////////////////////////////
// Function Pointers ----------------------------------------
PtrToDM280e_Initialize					pDM280e_Initialize;
PtrToDM280e_Close						pDM280e_Close;
PtrToDM280e_Reset						pDM280e_Reset;

PtrToDM280e_MIPI_RFFE_WR				pDM280e_MIPI_RFFE_WR;
PtrToDM280e_MIPI_RFFE_RD				pDM280e_MIPI_RFFE_RD;
PtrToDM280e_MIPI_RFFE_RETRIEVE			pDM280e_MIPI_RFFE_RETRIEVE;

PtrToDM280e_CONFIGURE_MIPI_CLOCK		pDM280e_CONFIGURE_MIPI_CLOCK;
PtrToDM280e_CONFIGURE_MIPI_DELAY		pDM280e_CONFIGURE_MIPI_DELAY;

PtrToDM280e_CONFIGURE_VOLTAGE_SUPPLY	pDM280e_CONFIGURE_VOLTAGE_SUPPLY;

PtrToDM280e_CONFIGURE_LOOPBACK			pDM280e_CONFIGURE_LOOPBACK;
PtrToDM280e_ReadDLLVersion				pDM280e_ReadDLLVersion;
PtrToDM280e_ReadFirmwareVersion			pDM280e_ReadFirmwareVersion;
PtrToDM280e_ReadSerialNumber			pDM280e_ReadSerialNumber;
PtrToQueryModuleType					pQueryModuleType;
PtrToDM280e_CONFIGURE_BPC				pDM280e_CONFIGURE_BPC;
PtrToDM280e_ConfigureMultiSiteMode		pDM280e_ConfigureMultiSiteMode;
PtrToDM280e_GetError					pDM280e_GetError;
PtrToDM280e_GetErrorMessage				pDM280e_GetErrorMessage;
PtrToDM280e_ClearError					pDM280e_ClearError;

int aemdm_LoadFunctions(HMODULE m_hmodule)
{
	pDM280e_Initialize = (PtrToDM280e_Initialize)GetProcAddress(m_hmodule, "DM280e_Initialize");
	if(pDM280e_Initialize == NULL)
		return -1;

	pDM280e_Close = (PtrToDM280e_Close)GetProcAddress(m_hmodule, "DM280e_Close");
	if(pDM280e_Close == NULL)
		return -1;

	pDM280e_Reset = (PtrToDM280e_Reset)GetProcAddress(m_hmodule, "DM280e_Reset");
	if(pDM280e_Reset == NULL)
		return -1;	

	pDM280e_MIPI_RFFE_WR = (PtrToDM280e_MIPI_RFFE_WR)GetProcAddress(m_hmodule, "DM280e_MIPI_RFFE_WR");
	if(pDM280e_MIPI_RFFE_WR == NULL)
		return -1;	
	pDM280e_MIPI_RFFE_RD = (PtrToDM280e_MIPI_RFFE_RD)GetProcAddress(m_hmodule, "DM280e_MIPI_RFFE_RD");
	if(pDM280e_MIPI_RFFE_RD == NULL)
		return -1;	

	pDM280e_MIPI_RFFE_RETRIEVE = (PtrToDM280e_MIPI_RFFE_RETRIEVE)GetProcAddress(m_hmodule, "DM280e_MIPI_RFFE_RETRIEVE");
	if(pDM280e_MIPI_RFFE_RETRIEVE == NULL)
		return -1;

	pDM280e_CONFIGURE_MIPI_CLOCK = (PtrToDM280e_CONFIGURE_MIPI_CLOCK)GetProcAddress(m_hmodule, "DM280e_CONFIGURE_MIPI_CLOCK");
	if(pDM280e_CONFIGURE_MIPI_CLOCK == NULL)
		return -1;

	pDM280e_CONFIGURE_MIPI_DELAY = (PtrToDM280e_CONFIGURE_MIPI_DELAY)GetProcAddress(m_hmodule, "DM280e_CONFIGURE_MIPI_DELAY");
	if(pDM280e_CONFIGURE_MIPI_DELAY == NULL)
		return -1;

	pDM280e_CONFIGURE_VOLTAGE_SUPPLY = (PtrToDM280e_CONFIGURE_VOLTAGE_SUPPLY)GetProcAddress(m_hmodule, "DM280e_CONFIGURE_VOLTAGE_SUPPLY");
	if(pDM280e_CONFIGURE_VOLTAGE_SUPPLY == NULL)
		return -1;

	pDM280e_CONFIGURE_LOOPBACK = (PtrToDM280e_CONFIGURE_LOOPBACK)GetProcAddress(m_hmodule, "DM280e_CONFIGURE_LOOPBACK");
	if(pDM280e_CONFIGURE_LOOPBACK == NULL)
		return -1;

	pDM280e_ReadDLLVersion = (PtrToDM280e_ReadDLLVersion)GetProcAddress(m_hmodule, "DM280e_ReadDLLVersion");
	if(pDM280e_ReadDLLVersion == NULL)
		return -1;
	
	pDM280e_ReadFirmwareVersion = (PtrToDM280e_ReadFirmwareVersion)GetProcAddress(m_hmodule, "DM280e_ReadFirmwareVersion");
	if(pDM280e_ReadFirmwareVersion == NULL)
		return -1;
	
	pDM280e_ReadSerialNumber = (PtrToDM280e_ReadSerialNumber)GetProcAddress(m_hmodule, "DM280e_ReadSerialNumber");
	if(pDM280e_ReadSerialNumber == NULL)
		return -1;

	pQueryModuleType = (PtrToQueryModuleType)GetProcAddress(m_hmodule, "QueryModuleType");
	if(pQueryModuleType == NULL)
		return -1;

	pDM280e_CONFIGURE_BPC = (PtrToDM280e_CONFIGURE_BPC)GetProcAddress(m_hmodule, "DM280e_CONFIGURE_BPC");
	if(pDM280e_CONFIGURE_BPC == NULL)
		return -1;
	
	pDM280e_ConfigureMultiSiteMode = (PtrToDM280e_ConfigureMultiSiteMode)GetProcAddress(m_hmodule, "DM280e_ConfigureMultiSiteMode");
	if(pDM280e_ConfigureMultiSiteMode == NULL)
		return -1;
	
	pDM280e_GetError = (PtrToDM280e_GetError)GetProcAddress(m_hmodule, "DM280e_GetError");
	if(pDM280e_GetError == NULL)
		return -1;
	pDM280e_GetErrorMessage = (PtrToDM280e_GetErrorMessage)GetProcAddress(m_hmodule, "DM280e_GetErrorMessage");
	if(pDM280e_GetErrorMessage == NULL)
		return -1;
	pDM280e_ClearError = (PtrToDM280e_ClearError)GetProcAddress(m_hmodule, "DM280e_ClearError");
	if(pDM280e_ClearError == NULL)
		return -1;

	return 0;
}

ViStatus DM280e_Initialize(ViRsrc resourceName, ViInt32 init_options, ViConstString optionString, ViSession* vi)
{
	return pDM280e_Initialize(resourceName, init_options, optionString, vi);
}
ViStatus DM280e_Close(ViSession vi)
{
	return pDM280e_Close(vi);
}
ViStatus DM280e_Reset(ViSession vi)
{
	return pDM280e_Reset(vi);
}
ViStatus DM280e_MIPI_RFFE_WR (ViSession vi, ViInt32 ch, ViInt32 Command, ViInt32* Data)
{
	return pDM280e_MIPI_RFFE_WR(vi,ch,Command,Data);
}
ViStatus DM280e_MIPI_RFFE_RD (ViSession vi, ViInt32 ch,ViInt32 speed, ViInt32 Command, ViInt32* Data)
{
	return pDM280e_MIPI_RFFE_RD (vi,ch,speed,Command,Data);
}
ViStatus DM280e_MIPI_RFFE_RETRIEVE(ViSession vi, ViInt32 ch, ViInt32* rd_byte_data_count, ViInt32* rd_data, ViInt32* parity_check)
{
	return pDM280e_MIPI_RFFE_RETRIEVE(vi,ch, rd_byte_data_count,rd_data,parity_check);
}
ViStatus DM280e_CONFIGURE_MIPI_CLOCK(ViSession vi, ViInt32 freq_Hz)
{
	return pDM280e_CONFIGURE_MIPI_CLOCK( vi,freq_Hz);
}
ViStatus DM280e_CONFIGURE_MIPI_DELAY(ViSession vi, ViInt32 ch, ViInt32 delay)
{
	return pDM280e_CONFIGURE_MIPI_DELAY( vi,ch,delay);
}
ViStatus DM280e_CONFIGURE_VOLTAGE_SUPPLY(ViSession vi, ViReal64 target_vio, ViReal64* actual_vio)
{
	return pDM280e_CONFIGURE_VOLTAGE_SUPPLY( vi,target_vio, actual_vio);
}
ViStatus DM280e_CONFIGURE_LOOPBACK(ViSession vi, ViInt32 ch, ViInt32 loopback)
{
	return pDM280e_CONFIGURE_LOOPBACK(vi, ch, loopback);
}
ViStatus DM280e_ReadDLLVersion (ViChar* version)
{
	return pDM280e_ReadDLLVersion(version);
}
ViStatus DM280e_ReadFirmwareVersion (ViSession vi, ViChar* version)
{
	return pDM280e_ReadFirmwareVersion(vi, version);
}
ViStatus DM280e_ReadSerialNumber (ViSession vi, ViChar* sn)
{
	return DM280e_ReadSerialNumber (vi, sn);
}
ViStatus QueryModuleType (ViSession vi, ViUInt32* module_type)
{
	return QueryModuleType (vi, module_type);
}
ViStatus DM280e_CONFIGURE_BPC(ViSession vi, ViInt32 BPC)
{
	return DM280e_CONFIGURE_BPC (vi,BPC);
}
ViStatus DM280e_ConfigureMultiSiteMode(ViSession vi, ViInt32 mode )
{
	return pDM280e_ConfigureMultiSiteMode(vi,mode);
}
ViStatus DM280e_GetError (ViSession vi, ViStatus* code, ViInt32 bufferSize, ViChar* description)
{
	return pDM280e_GetError(vi, code, bufferSize,description);
}
ViStatus DM280e_GetErrorMessage(ViSession vi,ViStatus errorCode, ViChar* errorMessage)
{
	return pDM280e_GetErrorMessage(vi,errorCode,errorMessage);
}	
ViStatus DM280e_ClearError(ViSession vi)
{
	return pDM280e_ClearError(vi);
}
}


