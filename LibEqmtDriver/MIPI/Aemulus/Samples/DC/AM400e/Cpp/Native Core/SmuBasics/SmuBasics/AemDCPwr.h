///////////////////////////////////////////////////////////////////////////////
//               Aemulus SMUs
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2011.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  Title:    AemDCPwr.h
//  Purpose:  Aemulus SMUs Driver declarations
// 
///////////////////////////////////////////////////////////////////////////////

#ifndef __AEMDCPWR_HEADER
#define __AEMDCPWR_HEADER

#include "visa.h"
#include <windows.h>

namespace aemdcpwr
{
	int aemdcpwr_LoadFunctions(HMODULE m_hmodule);
	ViStatus AemDCPwr_InitChannels(ViRsrc resourceName, ViConstString channels, ViInt32 init_options,ViConstString optionString,ViSession *vi);
	ViStatus AemDCPwr_Close(ViSession vi);      
	ViStatus AemDCPwr_QueryModuleType(ViSession vi, ViUInt32* module_type);
	ViStatus AemDCPwr_GetError(ViSession vi, ViStatus* code, ViInt32 bufferSize, ViChar* description);
	ViStatus AemDCPwr_GetErrorMessage(ViSession vi, ViStatus errorCode, ViChar* errorMessage);
	ViStatus AemDCPwr_ClearError(ViSession vi);
	ViStatus AemDCPwr_Reset(ViSession vi);
	ViStatus AemDCPwr_ResetChannel(ViSession vi, ViConstString channelName);
	ViStatus AemDCPwr_ConfigureMultiSiteMode(ViSession vi, ViInt32 mode);
  	ViStatus AemDCPwr_ReadRevision(ViSession vi, ViChar* instrumentDriverRevision, ViChar* firmwareRevision);
	ViStatus AemDCPwr_ReadCurrentTemperature(ViSession vi, ViReal64* temperature);
	ViStatus AemDCPwr_ReadAmbientTemperature(ViSession vi, ViReal64* temperature);
	ViStatus AemDCPwr_ReadSerialNumber(ViSession vi, ViChar* sn);
	ViStatus AemDCPwr_ConfigureVoltageLevel(ViSession vi, ViConstString channelName, ViReal64 level);
	ViStatus AemDCPwr_ConfigureVoltageLevelAndRange(ViSession vi, ViConstString channelName, ViReal64 level, ViReal64 range);
	ViStatus AemDCPwr_ConfigureCurrentLimit(ViSession vi, ViConstString channelName, ViInt32 behavior, ViReal64 limit);
	ViStatus AemDCPwr_ConfigureCurrentLimitAndRange(ViSession vi, ViConstString channelName, ViInt32 behaviour,ViReal64 limit, ViReal64 range);
	ViStatus AemDCPwr_ConfigureCurrentLevel(ViSession vi, ViConstString channelName, ViReal64 level);
	ViStatus AemDCPwr_ConfigureCurrentLevelAndRange(ViSession vi, ViConstString channelName, ViReal64 level, ViReal64 range);
	ViStatus AemDCPwr_ConfigureVoltageLimit(ViSession vi, ViConstString channelName, ViReal64 limit);
	ViStatus AemDCPwr_ConfigureVoltageLimitAndRange(ViSession vi, ViConstString channelName, ViReal64 limit, ViReal64 range);
	ViStatus AemDCPwr_ReadVoltageLevelRange(ViSession vi, ViConstString channelName, ViReal64* range);
	ViStatus AemDCPwr_ReadVoltageLimitRange(ViSession vi, ViConstString channelName, ViReal64* range);
	ViStatus AemDCPwr_ReadCurrentLevelRange(ViSession vi, ViConstString channelName, ViReal64* range);
	ViStatus AemDCPwr_ReadCurrentLimitRange(ViSession vi, ViConstString channelName, ViReal64* range);
	ViStatus AemDCPwr_ConfigureOutputFunction(ViSession vi, ViConstString channelName, ViInt32 function);
	ViStatus AemDCPwr_ConfigureOutputTransient(ViSession vi, ViConstString channelName,ViInt32 transient);
	ViStatus AemDCPwr_ComputeAIBandwidth(ViSession vi, ViConstString channelName, ViInt32 mode, ViReal64 vRange, ViReal64 iRange, ViReal64 drive_settling_time, ViReal64 clamp_settling_time, ViReal64 resistance, ViReal64 capacitance, ViReal64 reserved, ViInt32 store_location);
	ViStatus AemDCPwr_ConfigureOutputSwitch(ViSession vi, ViConstString channelName,ViInt32 switches);
	ViStatus AemDCPwr_ConfigureOutputResistance(ViSession vi, ViConstString channelName,ViReal64 resistance, ViReal64 range);
	ViStatus AemDCPwr_ConfigureOutputEnabled(ViSession vi, ViConstString channelName, ViBoolean enabled);
	ViStatus AemDCPwr_ConfigurePLF(ViSession vi, ViReal64 powerLineFrequency);
	ViStatus AemDCPwr_ConfigureSamplingTime(ViSession vi, ViConstString channelName, ViReal64 apertureTime, ViInt32 units);
	ViStatus AemDCPwr_ConfigureSense(ViSession vi, ViConstString channelName, ViInt32 sense);
	ViStatus AemDCPwr_Measure(ViSession vi, ViConstString channelName, ViInt32 measurementType, ViReal64* measurement);
	ViStatus AemDCPwr_MeasureArray(ViSession vi, ViConstString channelName, ViBoolean printToTxt,ViInt32 measurementType,  ViReal64* measurement);
	ViStatus AemDCPwr_MeasureMultiple(ViSession vi, ViConstString channelName, ViReal64* voltageMeasurements,ViReal64* currentMeasurements);
	ViStatus AemDCPwr_QueryInCompliance(ViSession vi, ViConstString channelName, ViBoolean* inCompliance);
	ViStatus AemDCPwr_QueryOutputState(ViSession vi, ViConstString channelName, ViInt32 outputState, ViBoolean* inState);
	ViStatus AemDCPwr_QueryMaxVoltageLevel(ViSession vi, ViConstString channelName, ViReal64 currentLimit, ViReal64* maxVoltageLevel);
	ViStatus AemDCPwr_QueryMaxCurrentLimit(ViSession vi, ViConstString channelName, ViReal64 voltageLevel, ViReal64* maxCurrentLimit);
	ViStatus AemDCPwr_QueryMinCurrentLimit(ViSession vi, ViConstString channelName, ViReal64 voltageLevel, ViReal64* minCurrentLimit);
	ViStatus AemDCPwr_ConfigureAcquireRecordLength(ViSession vi, ViConstString channelName, ViInt32 length);
	ViStatus AemDCPwr_ClearAcquireRecordLength(ViSession vi, ViConstString channelName);
	ViStatus AemDCPwr_ConfigureAcquireRecordDeltaTime(ViSession vi, ViConstString channelName, ViReal64 deltaTime);
	ViStatus AemDCPwr_QueryAcquireRecordLength(ViSession vi, ViConstString channelName, ViInt32* length);
	ViStatus AemDCPwr_AcquireMultiple(ViSession vi, ViConstString channelName, ViReal64 timeout, ViInt32 count, ViReal64* voltageMeasurements, ViReal64* currentMeasurements, ViBoolean* inCompliance, ViInt32* actualCount);
	ViStatus AemDCPwr_AcquireArray(ViSession vi, ViConstString channelName, ViReal64 timeout, ViReal64* voltageMeasurements, ViReal64* currentMeasurements, ViInt32* actualCount);
	ViStatus AemDCPwr_ConfigureMeasureMode(ViSession vi, ViConstString channelName, ViInt32 mode);
	ViStatus AemDCPwr_ContactCheck(ViSession vi, ViConstString channelName, ViInt32 shi_h_slo_l, ViInt32 threshold_mohm, ViReal64 delay_s, ViInt32* pass_h_fail_l, ViInt32* mohm);
  	ViStatus AemDCPwr_ConfigureInputTriggerSelect(ViSession vi, ViConstString channelName, ViInt32 triggerInput, ViReal64 delay_s);
	ViStatus AemDCPwr_ConfigureOutputTriggerSelect(ViSession vi, ViConstString channelName, ViInt32 triggerOutput, ViReal64 delay_s);
	ViStatus AemDCPwr_ConfigureMeasureTriggerInput(ViSession vi, ViInt32 triggerInput);
	ViStatus AemDCPwr_ConfigureTriggerEdgeLevel(ViSession vi, ViInt32 triggerEnum, ViInt32 option);
	ViStatus AemDCPwr_MapTriggerInToTriggerOut(ViSession vi, ViInt32 inputTerminal, ViInt32 outputTerminal);
	ViStatus AemDCPwr_MapExtTrigOut(ViSession vi, ViInt32 inputTerminal);
	ViStatus AemDCPwr_DriveSoftwareTrigger(ViSession vi, ViInt32 select, ViReal64 pulseWidth);
	ViStatus AemDCPwr_DriveSoftwareTriggerOut(ViSession vi, ViInt32 outputTerminal, ViReal64 pulseWidth);
	ViStatus AemDCPwr_ConfigureSMUOutputTriggerMode(ViSession vi, ViConstString channelName, ViInt32 mode);
	ViStatus AemDCPwr_ConfigureSMUOutputTriggerPulseWidth(ViSession vi, ViConstString channelName, ViReal64 pulseWidth);
	ViStatus AemDCPwr_ConfigureSMUOutputTriggerDuringSource(ViSession vi, ViConstString channelName, ViReal64 level, ViReal64 range, ViInt32 mode, ViInt32 edgeSetting);
	ViStatus AemDCPwr_GetExtCalRecommendedInterval(ViSession vi, ViInt32* months);
	ViStatus AemDCPwr_GetExtCalLastDateAndTime(ViSession vi, ViInt32* year, ViInt32* month, ViInt32* day, ViInt32* hour, ViInt32* minute);
	ViStatus AemDCPwr_ConfigureTriggerEdgeLevelExtra(ViSession vi, ViInt32 triggerEnum, ViInt32 option, ViInt32 ignore_trigger_count);
}

#endif // __AEMDCPWR_HEADER
