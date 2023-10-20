///////////////////////////////////////////////////////////////////////////////
//               Aemulus DM
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2011.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  Title:    AemDM.h
//  Purpose:  Aemulus DMs Driver declarations
// 
///////////////////////////////////////////////////////////////////////////////

#ifndef __AEMDM_HEADER
#define __AEMDM_HEADER

#include "visa.h"
#include <windows.h>

namespace aemdm
{
	int aemdm_LoadFunctions(HMODULE m_hmodule);

	// Initialize and Close Functions
	
	ViStatus DM280e_Initialize(ViRsrc resourceName, ViInt32 init_options, ViConstString optionString, ViSession* vi);
	ViStatus DM280e_Close(ViSession vi);     
	ViStatus DM280e_Reset(ViSession vi);

	ViStatus DM280e_MIPI_RFFE_WR(ViSession vi, ViInt32 ch, ViInt32 Command, ViInt32* Data);
	ViStatus DM280e_MIPI_RFFE_RD(ViSession vi, ViInt32 ch,ViInt32 speed, ViInt32 Command, ViInt32* Data);
	ViStatus DM280e_MIPI_RFFE_RETRIEVE(ViSession vi, ViInt32 ch, ViInt32* rd_byte_data_count, ViInt32* rd_data, ViInt32* parity_check);
	

	ViStatus DM280e_CONFIGURE_MIPI_CLOCK (ViSession vi, ViInt32 freq_Hz);
	ViStatus DM280e_CONFIGURE_MIPI_DELAY (ViSession vi, ViInt32 ch, ViInt32 delay);

	ViStatus DM280e_CONFIGURE_VOLTAGE_SUPPLY (ViSession vi, ViReal64 target_vio, ViReal64* actual_vio);

	ViStatus DM280e_CONFIGURE_LOOPBACK (ViSession vi, ViInt32 ch, ViInt32 loopback);

	ViStatus DM280e_ReadDLLVersion (ViChar* version);
	ViStatus DM280e_ReadFirmwareVersion (ViSession vi, ViChar* version);
	ViStatus DM280e_ReadSerialNumber (ViSession vi, ViChar* sn);
	ViStatus QueryModuleType (ViSession vi, ViUInt32* module_type);
	ViStatus DM280e_CONFIGURE_BPC(ViSession vi, ViInt32 BPC);
	ViStatus DM280e_ConfigureMultiSiteMode(ViSession vi, ViInt32 mode );
	ViStatus DM280e_GetError (ViSession vi, ViStatus* code, ViInt32 bufferSize, ViChar* description);
	ViStatus DM280e_GetErrorMessage(ViSession vi,ViStatus errorCode, ViChar* errorMessage);
	ViStatus DM280e_ClearError(ViSession vi);
}



#endif // __AEMDIO_HEADER
