///////////////////////////////////////////////////////////////////////////////
//               Aemulus DM482e
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2012.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  Title:    DM482e.h
//  Purpose:  Aemulus DM482e API declarations
// 
///////////////////////////////////////////////////////////////////////////////

#ifndef __DM482e_HEADER
#define __DM482e_HEADER

#include "visa.h"
#include <windows.h>

namespace dm482e
{

	int DM482e_LoadFunctions(HMODULE m_hmodule);

	// General Functions
	ViStatus DM482e_DPINOpen(ViRsrc resourceName, ViInt32 dpingroup_sel, ViInt32 init_options, ViConstString optionString, ViSession* vi);
	ViStatus DM482e_DPINClose(ViSession vi);
	ViStatus DM482e_DPINForce(ViSession vi, ViInt32 pinNo, ViInt32 state);
	ViStatus DM482e_ReadRevision(ViSession vi, ViChar* instrumentDriverRevision, ViChar* firmwareRevision);
	ViStatus DM482e_ReadChannelTemperature(ViSession vi, ViInt32 pinNo, ViReal64* temperature);
	ViStatus DM482e_ReadAmbientTemperature(ViSession vi, ViReal64* temperature);
	ViStatus DM482e_ReadSerialNumber(ViSession vi, ViChar* sn);
	ViStatus DM482e_Reset(ViSession vi);
	ViStatus DM482e_ResetGroup(ViSession vi, ViInt32 group);
	ViStatus DM482e_ConfigureMultiSiteMode(ViSession vi, ViInt32 mode);
  
	// Pin Electronics Related API
	ViStatus DM482e_DPINVectorResourceAllocation(ViSession vi, ViInt32 vecSetCount, ViInt32* resourceArray);
	ViStatus DM482e_DPINLevel(ViSession vi, ViInt32 pinNo, ViReal64 VIH, ViReal64 VIL, ViReal64 VOH, ViReal64 VOL, ViReal64 IOH, ViReal64 IOL, ViReal64 VCH, ViReal64 VCL, ViReal64 VTERM);
	ViStatus DM482e_DPINVecLoad(ViSession vi, ViInt32 vecSetNo, char* vecFileName);
	ViStatus DM482e_DPINPeriod(ViSession vi, ViInt32 timingSetNo, ViReal64 period_s);
	ViStatus DM482e_DPINOn(ViSession vi, ViInt32 pinNo);
	ViStatus DM482e_DPINOff(ViSession vi, ViInt32 pinNo);
	ViStatus DM482e_DPINHVOn(ViSession vi, ViInt32 pinNo);
	ViStatus DM482e_DPINHVOff(ViSession vi, ViInt32 pinNo);
	ViStatus DM482e_RunVector(ViSession vi, ViInt32 vecSetNo);
	ViStatus DM482e_AcquireVecEngineStatus(ViSession vi, ViInt32* status);
	ViStatus DM482e_ReadHistoryRam(ViSession vi, ViInt32 vecSetNo, ViUInt32* history_ram_data);
	ViStatus DM482e_ConfigurePEAttribute(ViSession vi, ViInt32 pinNo, ViBoolean inputTerminationEnable, ViBoolean HVEnable, ViBoolean activeLoadEnable, ViBoolean differentialComparatorEnable);
	ViStatus DM482e_ConfigureVectorEngineAttribute(ViSession vi, ViBoolean triggerEnable, ViBoolean continuousEnable);
	ViStatus DM482e_ConfigureClockFrequency(ViSession vi, ViReal64 frequency);
	ViStatus DM482e_ConfigureInputChannelDelay(ViSession vi, ViInt32 pinNo, ViReal64 delay);
	ViStatus DM482e_AcquireVectorFailCount(ViSession vi, ViInt32* failCount);
	ViStatus DM482e_AcquireChannelVectorFailCount(ViSession vi, ViInt32 pinNo, ViInt32* failCount);
	ViStatus DM482e_AcquireChannelFirstFailVectorCount(ViSession vi, ViInt32 pinNo, ViInt32* failCount);
  
	// Pin Measurement Unit Related API
	ViStatus DM482e_ConfigurePMUVoltageLevel(ViSession vi, ViInt32 pinNo, ViReal64 level);
	ViStatus DM482e_ConfigurePMUVoltageLimit(ViSession vi, ViInt32 pinNo, ViReal64 high_limit, ViReal64 low_limit);
	ViStatus DM482e_ConfigurePMUCurrentLevel(ViSession vi, ViInt32 pinNo, ViReal64 level);
	ViStatus DM482e_ConfigurePMUCurrentLevelAndRange(ViSession vi, ViInt32 pinNo, ViReal64 level, ViReal64 range);
	ViStatus DM482e_ConfigurePMUCurrentLimitRange(ViSession vi, ViInt32 pinNo, ViReal64 range);
	ViStatus DM482e_GetPMUVoltageLevelRange(ViSession vi, ViInt32 pinNo, ViReal64* range);
	ViStatus DM482e_GetPMUVoltageLimitRange(ViSession vi, ViInt32 pinNo, ViReal64* range);
	ViStatus DM482e_GetPMUCurrentLevelRange(ViSession vi, ViInt32 pinNo, ViReal64* range);
	ViStatus DM482e_GetPMUCurrentLimitRange(ViSession vi, ViInt32 pinNo, ViReal64* range);
	ViStatus DM482e_ConfigurePMUOutputFunction(ViSession vi, ViInt32 pinNo, ViInt32 function);
	ViStatus DM482e_ConfigurePMUSamplingTime(ViSession vi, ViInt32 pinNo, ViReal64 samplingTime, ViInt32 units);
	ViStatus DM482e_ConfigurePowerLineFrequency(ViSession vi, ViReal64 powerLineFrequency);
	ViStatus DM482e_ConfigurePMUSense(ViSession vi, ViInt32 pinNo, ViInt32 sense);
	ViStatus DM482e_GetPMUSense(ViSession vi, ViInt32 pinNo, ViInt32* sense);
	ViStatus DM482e_PMUMeasure(ViSession vi, ViInt32 pinNo, ViInt32 measurementType, ViReal64* measurement); 
  
	// Trigger Related API 
	ViStatus DM482e_ConfigureTriggerEdgeLevel(ViSession vi, ViInt32 triggerEnum, ViInt32 option);
	ViStatus DM482e_MapTriggerInToTriggerOut(ViSession vi, ViInt32 inputTerminal, ViInt32 outputTerminal);
	ViStatus DM482e_ConfigureInputTriggerSelect(ViSession vi, ViInt32 triggerInput);
	ViStatus DM482e_ConfigureOutputTriggerSelect(ViSession vi, ViInt32 triggerOutput0, ViInt32 triggerOutput1);
	ViStatus DM482e_DriveSoftwareTrigger(ViSession vi, ViReal64 pulseWidth);

	// MIPI Related API 
	ViStatus DM482e_MIPI_Configure_Clock(ViSession vi, ViInt32 mipi_pair, ViInt32 freq_Hz);
	ViStatus DM482e_MIPI_Connect(ViSession vi, ViInt32 mipi_pair, ViInt32 setting);
	ViStatus DM482e_MIPI_RFFE_WR(ViSession vi, ViInt32 mipi_pair, ViInt32 Command, ViInt32* Data);
	ViStatus DM482e_MIPI_RFFE_RD(ViSession vi, ViInt32 mipi_pair, ViInt32 full_speed, ViInt32 Command, ViInt32* Data);
	ViStatus DM482e_MIPI_RFFE_Retrieve(ViSession vi, ViInt32 mipi_pair, ViInt32* rd_byte_data_count, ViInt32* rd_data_array, ViInt32* parity_check_array);

	// DIO Related API
	ViStatus DM482e_DrivePort(ViSession vi, ViInt32 value);
	ViStatus DM482e_DrivePin(ViSession vi, ViInt32 pinNo, ViInt32 value);
	ViStatus DM482e_SetPortDirection(ViSession vi, ViInt32 value);
	ViStatus DM482e_SetPinDirection(ViSession vi, ViInt32 pinNo, ViInt32 value);
	ViStatus DM482e_ReadPort(ViSession vi, ViInt32* value);
	ViStatus DM482e_ReadPin(ViSession vi, ViInt32 pinNo, ViInt32* value);

}



#endif // __DM482e_HEADER
