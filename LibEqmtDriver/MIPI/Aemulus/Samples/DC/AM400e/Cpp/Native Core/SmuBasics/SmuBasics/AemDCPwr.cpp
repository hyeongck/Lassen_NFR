///////////////////////////////////////////////////////////////////////////////
//               Aemulus SMUs
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2011.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  Title:    AemDCPwr.cpp
//  Purpose:  Aemulus SMUs Driver declarations
// 
///////////////////////////////////////////////////////////////////////////////
#include "AemDCPwr.h"

namespace aemdcpwr
{
typedef ViStatus (*PtrToAemDCPwr_InitChannels)(ViRsrc resourceName, ViConstString channels, ViInt32 init_options,ViConstString optionString,ViSession *vi);
typedef ViStatus (*PtrToAemDCPwr_Close)(ViSession vi);
typedef ViStatus (*PtrToAemDCPwr_QueryModuleType)(ViSession vi, ViUInt32* module_type);
typedef ViStatus (*PtrToAemDCPwr_GetError)(ViSession vi, ViStatus* code, ViInt32 bufferSize, ViChar* description);
typedef ViStatus (*PtrToAemDCPwr_GetErrorMessage)(ViSession vi, ViStatus errorCode, ViChar* errorMessage);
typedef ViStatus (*PtrToAemDCPwr_ClearError)(ViSession vi);
typedef ViStatus (*PtrToAemDCPwr_ReadRevision)(ViSession vi, ViChar* instrumentDriverRevision, ViChar* firmwareRevision);
typedef ViStatus (*PtrToAemDCPwr_ReadCurrentTemperature)(ViSession vi, ViReal64* temperature);
typedef ViStatus (*PtrToAemDCPwr_ReadAmbientTemperature)(ViSession vi, ViReal64* temperature);
typedef ViStatus (*PtrToAemDCPwr_ReadSerialNumber)(ViSession vi, ViChar* sn);
typedef ViStatus (*PtrToAemDCPwr_Reset)(ViSession vi);
typedef ViStatus (*PtrToAemDCPwr_ResetChannel)(ViSession vi, ViConstString channels);
typedef ViStatus (*PtrToAemDCPwr_ConfigureMultiSiteMode)(ViSession vi, ViInt32 mode);
typedef ViStatus (*PtrToAemDCPwr_ConfigureVoltageLevel)(ViSession vi, ViConstString channelName, ViReal64 level);
typedef ViStatus (*PtrToAemDCPwr_ConfigureVoltageLevelAndRange)(ViSession vi, ViConstString channelName, ViReal64 level, ViReal64 range);
typedef ViStatus (*PtrToAemDCPwr_ConfigureCurrentLimit)(ViSession vi, ViConstString channelName, ViInt32 behavior, ViReal64 limit);
typedef ViStatus (*PtrToAemDCPwr_ConfigureCurrentLimitAndRange)(ViSession vi, ViConstString channelName, ViInt32 behaviour, ViReal64 limit, ViReal64 range);
typedef ViStatus (*PtrToAemDCPwr_ConfigureCurrentLevel)(ViSession vi, ViConstString channelName, ViReal64 level);
typedef ViStatus (*PtrToAemDCPwr_ConfigureCurrentLevelAndRange)(ViSession vi, ViConstString channelName, ViReal64 level, ViReal64 range);
typedef ViStatus (*PtrToAemDCPwr_ConfigureVoltageLimit)(ViSession vi, ViConstString channelName, ViReal64 limit);
typedef ViStatus (*PtrToAemDCPwr_ConfigureVoltageLimitAndRange)(ViSession vi, ViConstString channelName, ViReal64 limit, ViReal64 range);
typedef ViStatus (*PtrToAemDCPwr_ReadVoltageLevelRange)(ViSession vi, ViConstString channelName, ViReal64* range);
typedef ViStatus (*PtrToAemDCPwr_ReadVoltageLimitRange)(ViSession vi, ViConstString channelName, ViReal64* range);
typedef ViStatus (*PtrToAemDCPwr_ReadCurrentLevelRange)(ViSession vi, ViConstString channelName, ViReal64* range);
typedef ViStatus (*PtrToAemDCPwr_ReadCurrentLimitRange)(ViSession vi, ViConstString channelName, ViReal64* range);
typedef ViStatus (*PtrToAemDCPwr_ConfigureOutputFunction)(ViSession vi, ViConstString channelName, ViInt32 function);
typedef ViStatus (*PtrToAemDCPwr_ConfigureOutputTransient)(ViSession vi, ViConstString channelName, ViInt32 transient);
typedef ViStatus (*PtrToAemDCPwr_ComputeAIBandwidth)(ViSession vi, ViConstString channelName, ViInt32 mode, ViReal64 vRange, ViReal64 iRange, ViReal64 drive_settling_time, ViReal64 clamp_settling_time, ViReal64 resistance, ViReal64 capacitance, ViReal64 reserved, ViInt32 store_location);
typedef ViStatus (*PtrToAemDCPwr_ConfigureOutputResistance)(ViSession vi, ViConstString channelName,ViReal64 resistance, ViReal64 range);
typedef ViStatus (*PtrToAemDCPwr_ConfigureOutputSwitch)(ViSession vi, ViConstString channelName, ViInt32 switches);
typedef ViStatus (*PtrToAemDCPwr_ConfigureOutputEnabled)(ViSession vi, ViConstString channelName, ViBoolean enabled);
typedef ViStatus (*PtrToAemDCPwr_ConfigureSamplingTime)(ViSession vi, ViConstString channelName, ViReal64 apertureTime, ViInt32 units);
typedef ViStatus (*PtrToAemDCPwr_ConfigurePLF)(ViSession vi, ViReal64 powerLineFrequency);
typedef ViStatus (*PtrToAemDCPwr_Measure)(ViSession vi, ViConstString channelName, ViInt32 measurementType, ViReal64* measurement);
typedef ViStatus (*PtrToAemDCPwr_MeasureArray)(ViSession vi, ViConstString channelName,  ViBoolean printToTxt, ViInt32 measurementType,ViReal64* measurement);
typedef ViStatus (*PtrToAemDCPwr_ConfigureSense)(ViSession vi, ViConstString channelName, ViInt32 sense);
typedef ViStatus (*PtrToAemDCPwr_MeasureMultiple)(ViSession vi, ViConstString channelName, ViReal64* voltageMeasurements,ViReal64* currentMeasurements);
typedef ViStatus (*PtrToAemDCPwr_QueryInCompliance)(ViSession vi, ViConstString channelName, ViBoolean* inCompliance);
typedef ViStatus (*PtrToAemDCPwr_QueryOutputState)(ViSession vi, ViConstString channelName, ViInt32 outputState, ViBoolean* inState);
typedef ViStatus (*PtrToAemDCPwr_QueryMaxVoltageLevel)(ViSession vi, ViConstString channelName, ViReal64 currentLimit, ViReal64* maxVoltageLevel);
typedef ViStatus (*PtrToAemDCPwr_QueryMaxCurrentLimit)(ViSession vi, ViConstString channelName, ViReal64 voltageLevel, ViReal64* maxCurrentLimit);
typedef ViStatus (*PtrToAemDCPwr_QueryMinCurrentLimit)(ViSession vi, ViConstString channelName, ViReal64 voltageLevel, ViReal64* minCurrentLimit);
typedef ViStatus (*PtrToAemDCPwr_ConfigureAcquireRecordLength)(ViSession vi, ViConstString channelName, ViInt32 length);
typedef ViStatus (*PtrToAemDCPwr_ConfigureAcquireRecordDeltaTime)(ViSession vi, ViConstString channelName, ViReal64 deltaTime);
typedef ViStatus (*PtrToAemDCPwr_QueryAcquireRecordLength)(ViSession vi, ViConstString channelName, ViInt32* length);
typedef ViStatus (*PtrToAemDCPwr_ClearAcquireRecordLength)(ViSession vi, ViConstString channelName);
typedef ViStatus (*PtrToAemDCPwr_AcquireMultiple)(ViSession vi, ViConstString channelName, ViReal64 timeout, ViInt32 count, ViReal64* voltageMeasurements, ViReal64* currentMeasurements, ViBoolean* inCompliance, ViInt32* actualCount);
typedef ViStatus (*PtrToAemDCPwr_AcquireArray)(ViSession vi, ViConstString channelName, ViReal64 timeout, ViReal64* voltageMeasurements, ViReal64* currentMeasurements, ViInt32* actualCount);
typedef ViStatus (*PtrToAemDCPwr_ConfigureMeasureMode)(ViSession vi, ViConstString channelName, ViInt32 mode);
typedef ViStatus (*PtrToAemDCPwr_ConfigureMeasureTriggerInput)(ViSession vi, ViInt32 triggerInput);
typedef ViStatus (*PtrToAemDCPwr_ConfigureTriggerEdgeLevel)(ViSession vi, ViInt32 triggerEnum, ViInt32 option);
typedef ViStatus (*PtrToAemDCPwr_MapTriggerInToTriggerOut)(ViSession vi, ViInt32 inputTerminal, ViInt32 outputTerminal);
typedef ViStatus (*PtrToAemDCPwr_MapExtTrigOut)(ViSession vi, ViInt32 inputTerminal);
typedef ViStatus (*PtrToAemDCPwr_DriveSoftwareTrigger)(ViSession vi, ViInt32 select, ViReal64 pulseWidth);
typedef ViStatus (*PtrToAemDCPwr_DriveSoftwareTriggerOut)(ViSession vi, ViInt32 outputTerminal, ViReal64 pulseWidth);
typedef ViStatus (*PtrToAemDCPwr_ConfigureSMUOutputTriggerMode)(ViSession vi, ViConstString channelName, ViInt32 mode);
typedef ViStatus (*PtrToAemDCPwr_ConfigureSMUOutputTriggerPulseWidth)(ViSession vi, ViConstString channelName, ViReal64 pulseWidth);
typedef ViStatus (*PtrToAemDCPwr_ConfigureSMUOutputTriggerDuringSource)(ViSession vi, ViConstString channelName, ViReal64 level, ViReal64 range, ViInt32 mode, ViInt32 edgeSetting);
typedef ViStatus (*PtrToAemDCPwr_GetExtCalRecommendedInterval)(ViSession vi, ViInt32* months);
typedef ViStatus (*PtrToAemDCPwr_GetExtCalLastDateAndTime)(ViSession vi, ViInt32* year, ViInt32* month, ViInt32* day, ViInt32* hour, ViInt32* minute);
typedef ViStatus (*PtrToAemDCPwr_ContactCheck)(ViSession vi, ViConstString channelName, ViInt32 shi_h_slo_l, ViInt32 threshold_mohm, ViReal64 delay_s, ViInt32* pass_h_fail_l, ViInt32* mohm);
typedef ViStatus (*PtrToAemDCPwr_ConfigureInputTriggerSelect)(ViSession vi, ViConstString channelName, ViInt32 triggerInput, ViReal64 delay_s);
typedef ViStatus (*PtrToAemDCPwr_ConfigureOutputTriggerSelect)(ViSession vi, ViConstString channelName, ViInt32 triggerOutput, ViReal64 delay_s);
typedef ViStatus (*PtrToAemDCPwr_ConfigureTriggerEdgeLevelExtra)(ViSession vi, ViInt32 triggerEnum, ViInt32 option, ViInt32 ignore_trigger_count);

PtrToAemDCPwr_InitChannels pAemDCPwr_InitChannels;
PtrToAemDCPwr_Close pAemDCPwr_Close;
PtrToAemDCPwr_QueryModuleType pAemDCPwr_QueryModuleType;
PtrToAemDCPwr_GetError pAemDCPwr_GetError;
PtrToAemDCPwr_GetErrorMessage pAemDCPwr_GetErrorMessage;
PtrToAemDCPwr_ClearError pAemDCPwr_ClearError;
PtrToAemDCPwr_ReadRevision pAemDCPwr_ReadRevision;
PtrToAemDCPwr_ReadCurrentTemperature pAemDCPwr_ReadCurrentTemperature;
PtrToAemDCPwr_ReadAmbientTemperature pAemDCPwr_ReadAmbientTemperature;
PtrToAemDCPwr_ReadSerialNumber pAemDCPwr_ReadSerialNumber;
PtrToAemDCPwr_Reset pAemDCPwr_Reset;
PtrToAemDCPwr_ResetChannel pAemDCPwr_ResetChannel;
PtrToAemDCPwr_ConfigureMultiSiteMode pAemDCPwr_ConfigureMultiSiteMode;
PtrToAemDCPwr_ConfigureVoltageLevel pAemDCPwr_ConfigureVoltageLevel;
PtrToAemDCPwr_ConfigureVoltageLevelAndRange pAemDCPwr_ConfigureVoltageLevelAndRange;
PtrToAemDCPwr_ConfigureCurrentLimit pAemDCPwr_ConfigureCurrentLimit;
PtrToAemDCPwr_ConfigureCurrentLimitAndRange pAemDCPwr_ConfigureCurrentLimitAndRange;
PtrToAemDCPwr_ConfigureCurrentLevel pAemDCPwr_ConfigureCurrentLevel;
PtrToAemDCPwr_ConfigureCurrentLevelAndRange pAemDCPwr_ConfigureCurrentLevelAndRange;
PtrToAemDCPwr_ConfigureVoltageLimit pAemDCPwr_ConfigureVoltageLimit;
PtrToAemDCPwr_ConfigureVoltageLimitAndRange pAemDCPwr_ConfigureVoltageLimitAndRange;
PtrToAemDCPwr_ReadVoltageLevelRange pAemDCPwr_ReadVoltageLevelRange;
PtrToAemDCPwr_ReadVoltageLimitRange pAemDCPwr_ReadVoltageLimitRange;
PtrToAemDCPwr_ReadCurrentLevelRange pAemDCPwr_ReadCurrentLevelRange;
PtrToAemDCPwr_ReadCurrentLimitRange pAemDCPwr_ReadCurrentLimitRange;
PtrToAemDCPwr_ConfigureOutputFunction pAemDCPwr_ConfigureOutputFunction;
PtrToAemDCPwr_ConfigureOutputTransient pAemDCPwr_ConfigureOutputTransient;
PtrToAemDCPwr_ComputeAIBandwidth pAemDCPwr_ComputeAIBandwidth;
PtrToAemDCPwr_ConfigureOutputResistance pAemDCPwr_ConfigureOutputResistance;
PtrToAemDCPwr_ConfigureOutputSwitch pAemDCPwr_ConfigureOutputSwitch;
PtrToAemDCPwr_ConfigureOutputEnabled pAemDCPwr_ConfigureOutputEnabled;
PtrToAemDCPwr_ConfigurePLF pAemDCPwr_ConfigurePLF;
PtrToAemDCPwr_ConfigureSamplingTime pAemDCPwr_ConfigureSamplingTime;
PtrToAemDCPwr_ConfigureSense pAemDCPwr_ConfigureSense;
PtrToAemDCPwr_Measure pAemDCPwr_Measure;
PtrToAemDCPwr_MeasureArray pAemDCPwr_MeasureArray;
PtrToAemDCPwr_MeasureMultiple pAemDCPwr_MeasureMultiple;
PtrToAemDCPwr_QueryInCompliance pAemDCPwr_QueryInCompliance;
PtrToAemDCPwr_QueryOutputState pAemDCPwr_QueryOutputState;
PtrToAemDCPwr_QueryMaxVoltageLevel pAemDCPwr_QueryMaxVoltageLevel;
PtrToAemDCPwr_QueryMaxCurrentLimit pAemDCPwr_QueryMaxCurrentLimit;
PtrToAemDCPwr_QueryMinCurrentLimit pAemDCPwr_QueryMinCurrentLimit;
PtrToAemDCPwr_ConfigureAcquireRecordLength pAemDCPwr_ConfigureAcquireRecordLength;
PtrToAemDCPwr_ClearAcquireRecordLength pAemDCPwr_ClearAcquireRecordLength;
PtrToAemDCPwr_ConfigureAcquireRecordDeltaTime pAemDCPwr_ConfigureAcquireRecordDeltaTime;
PtrToAemDCPwr_QueryAcquireRecordLength pAemDCPwr_QueryAcquireRecordLength;
PtrToAemDCPwr_AcquireMultiple pAemDCPwr_AcquireMultiple;
PtrToAemDCPwr_AcquireArray pAemDCPwr_AcquireArray;
PtrToAemDCPwr_ConfigureMeasureMode pAemDCPwr_ConfigureMeasureMode;
PtrToAemDCPwr_ContactCheck pAemDCPwr_ContactCheck;
PtrToAemDCPwr_ConfigureMeasureTriggerInput pAemDCPwr_ConfigureMeasureTriggerInput;
PtrToAemDCPwr_ConfigureTriggerEdgeLevel pAemDCPwr_ConfigureTriggerEdgeLevel;
PtrToAemDCPwr_MapTriggerInToTriggerOut pAemDCPwr_MapTriggerInToTriggerOut;
PtrToAemDCPwr_MapExtTrigOut pAemDCPwr_MapExtTrigOut;
PtrToAemDCPwr_DriveSoftwareTrigger pAemDCPwr_DriveSoftwareTrigger;
PtrToAemDCPwr_DriveSoftwareTriggerOut pAemDCPwr_DriveSoftwareTriggerOut;
PtrToAemDCPwr_ConfigureSMUOutputTriggerMode pAemDCPwr_ConfigureSMUOutputTriggerMode;
PtrToAemDCPwr_ConfigureSMUOutputTriggerDuringSource pAemDCPwr_ConfigureSMUOutputTriggerDuringSource;
PtrToAemDCPwr_ConfigureSMUOutputTriggerPulseWidth pAemDCPwr_ConfigureSMUOutputTriggerPulseWidth;
PtrToAemDCPwr_GetExtCalRecommendedInterval pAemDCPwr_GetExtCalRecommendedInterval;
PtrToAemDCPwr_GetExtCalLastDateAndTime pAemDCPwr_GetExtCalLastDateAndTime;
PtrToAemDCPwr_ConfigureInputTriggerSelect pAemDCPwr_ConfigureInputTriggerSelect;
PtrToAemDCPwr_ConfigureOutputTriggerSelect pAemDCPwr_ConfigureOutputTriggerSelect;
PtrToAemDCPwr_ConfigureTriggerEdgeLevelExtra pAemDCPwr_ConfigureTriggerEdgeLevelExtra;

int aemdcpwr_LoadFunctions(HMODULE m_hmodule)
{
	pAemDCPwr_InitChannels = (PtrToAemDCPwr_InitChannels)GetProcAddress(m_hmodule, "AemDCPwr_InitChannels");
	if(pAemDCPwr_InitChannels == NULL)
		return -1;

	pAemDCPwr_Close = (PtrToAemDCPwr_Close)GetProcAddress(m_hmodule, "AemDCPwr_Close");
	if(pAemDCPwr_Close == NULL)
		return -1;

	pAemDCPwr_QueryModuleType = (PtrToAemDCPwr_QueryModuleType)GetProcAddress(m_hmodule, "QueryModuleType");
	if(pAemDCPwr_QueryModuleType == NULL)
		return -1;

	pAemDCPwr_ReadRevision = (PtrToAemDCPwr_ReadRevision)GetProcAddress(m_hmodule, "AemDCPwr_ReadRevision");
	if(pAemDCPwr_ReadRevision == NULL)
		return -1;

	pAemDCPwr_ReadCurrentTemperature = (PtrToAemDCPwr_ReadCurrentTemperature)GetProcAddress(m_hmodule, "AemDCPwr_ReadCurrentTemperature");
	if(pAemDCPwr_ReadCurrentTemperature == NULL)
		return -1;

	pAemDCPwr_ReadAmbientTemperature = (PtrToAemDCPwr_ReadAmbientTemperature)GetProcAddress(m_hmodule, "AemDCPwr_ReadAmbientTemperature");
	if(pAemDCPwr_ReadAmbientTemperature == NULL)
		return -1;

	pAemDCPwr_ReadSerialNumber = (PtrToAemDCPwr_ReadSerialNumber)GetProcAddress(m_hmodule, "AemDCPwr_ReadSerialNumber");
	if(pAemDCPwr_ReadSerialNumber == NULL)
		return -1;

	pAemDCPwr_Reset = (PtrToAemDCPwr_Reset)GetProcAddress(m_hmodule, "AemDCPwr_Reset");
	if(pAemDCPwr_Reset == NULL)
		return -1;	

	pAemDCPwr_ResetChannel = (PtrToAemDCPwr_ResetChannel)GetProcAddress(m_hmodule, "AemDCPwr_ResetChannel");
	if(pAemDCPwr_ResetChannel == NULL)
		return -1;	

	pAemDCPwr_ConfigureMultiSiteMode = (PtrToAemDCPwr_ConfigureMultiSiteMode)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureMultiSiteMode");
	if(pAemDCPwr_ConfigureMultiSiteMode == NULL)
		return -1;	

	pAemDCPwr_ConfigureVoltageLevel = (PtrToAemDCPwr_ConfigureVoltageLevel)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureVoltageLevel");
	if(pAemDCPwr_ConfigureVoltageLevel == NULL)
		return -1;

	pAemDCPwr_ConfigureVoltageLevelAndRange = (PtrToAemDCPwr_ConfigureVoltageLevelAndRange)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureVoltageLevelAndRange");
	if(pAemDCPwr_ConfigureVoltageLevelAndRange == NULL)
		return -1;

	pAemDCPwr_ConfigureCurrentLimit = (PtrToAemDCPwr_ConfigureCurrentLimit)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureCurrentLimit");
	if(pAemDCPwr_ConfigureCurrentLimit == NULL)
		return -1;

	pAemDCPwr_ConfigureCurrentLimitAndRange = (PtrToAemDCPwr_ConfigureCurrentLimitAndRange)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureCurrentLimitAndRange");
	if(pAemDCPwr_ConfigureCurrentLimitAndRange == NULL)
		return -1;

	pAemDCPwr_ConfigureCurrentLevel = (PtrToAemDCPwr_ConfigureCurrentLevel)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureCurrentLevel");
	if(pAemDCPwr_ConfigureCurrentLevel == NULL)
		return -1;

	pAemDCPwr_ConfigureCurrentLevelAndRange = (PtrToAemDCPwr_ConfigureCurrentLevelAndRange)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureCurrentLevelAndRange");
	if(pAemDCPwr_ConfigureCurrentLevelAndRange == NULL)
		return -1;
	
	pAemDCPwr_ConfigureVoltageLimit = (PtrToAemDCPwr_ConfigureVoltageLimit)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureVoltageLimit");
	if(pAemDCPwr_ConfigureVoltageLimit == NULL)
		return -1;

	pAemDCPwr_ConfigureVoltageLimitAndRange = (PtrToAemDCPwr_ConfigureVoltageLimitAndRange)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureVoltageLimitAndRange");
	if(pAemDCPwr_ConfigureVoltageLimitAndRange == NULL)
		return -1;

	pAemDCPwr_ReadVoltageLevelRange = (PtrToAemDCPwr_ReadVoltageLevelRange)GetProcAddress(m_hmodule, "AemDCPwr_ReadVoltageLevelRange");
	if(pAemDCPwr_ReadVoltageLevelRange == NULL)
		return -1;

	pAemDCPwr_ReadVoltageLimitRange = (PtrToAemDCPwr_ReadVoltageLimitRange)GetProcAddress(m_hmodule, "AemDCPwr_ReadVoltageLimitRange");
	if(pAemDCPwr_ReadVoltageLimitRange == NULL)
		return -1;

	pAemDCPwr_ReadCurrentLevelRange = (PtrToAemDCPwr_ReadCurrentLevelRange)GetProcAddress(m_hmodule, "AemDCPwr_ReadCurrentLevelRange");
	if(pAemDCPwr_ReadCurrentLevelRange == NULL)
		return -1;

	pAemDCPwr_ReadCurrentLimitRange = (PtrToAemDCPwr_ReadCurrentLimitRange)GetProcAddress(m_hmodule, "AemDCPwr_ReadCurrentLimitRange");
	if(pAemDCPwr_ReadCurrentLimitRange == NULL)
		return -1;

	pAemDCPwr_ConfigureOutputFunction = (PtrToAemDCPwr_ConfigureOutputFunction)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureOutputFunction");
	if(pAemDCPwr_ConfigureOutputFunction == NULL)
		return -1;

	pAemDCPwr_ConfigureOutputTransient = (PtrToAemDCPwr_ConfigureOutputTransient)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureOutputTransient");
	if(pAemDCPwr_ConfigureOutputTransient == NULL)
		return -1;

	pAemDCPwr_ComputeAIBandwidth = (PtrToAemDCPwr_ComputeAIBandwidth)GetProcAddress(m_hmodule, "AemDCPwr_ComputeAIBandwidth");
	if(pAemDCPwr_ComputeAIBandwidth == NULL)
		return -1;

	pAemDCPwr_ConfigureOutputResistance = (PtrToAemDCPwr_ConfigureOutputResistance)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureOutputResistance");
	if(pAemDCPwr_ConfigureOutputResistance == NULL)
		return -1;

	pAemDCPwr_ConfigureOutputSwitch = (PtrToAemDCPwr_ConfigureOutputSwitch)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureOutputSwitch");
	if(pAemDCPwr_ConfigureOutputSwitch == NULL)
		return -1;

	pAemDCPwr_ConfigureOutputEnabled = (PtrToAemDCPwr_ConfigureOutputEnabled)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureOutputEnabled");
	if(pAemDCPwr_ConfigureOutputEnabled == NULL)
		return -1;

	pAemDCPwr_ConfigurePLF = (PtrToAemDCPwr_ConfigurePLF)GetProcAddress(m_hmodule, "AemDCPwr_ConfigurePLF");
	if(pAemDCPwr_ConfigurePLF == NULL)
		return -1;

	pAemDCPwr_Measure = (PtrToAemDCPwr_Measure)GetProcAddress(m_hmodule, "AemDCPwr_Measure");
	if(pAemDCPwr_Measure == NULL)
		return -1;

	pAemDCPwr_ConfigureSamplingTime = (PtrToAemDCPwr_ConfigureSamplingTime)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureSamplingTime");
	if(pAemDCPwr_ConfigureSamplingTime == NULL)
		return -1;

	pAemDCPwr_ConfigureSense =(PtrToAemDCPwr_ConfigureSense)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureSense");
	if(pAemDCPwr_ConfigureSense == NULL)
		return -1;

	pAemDCPwr_MeasureArray = (PtrToAemDCPwr_MeasureArray)GetProcAddress(m_hmodule, "AemDCPwr_MeasureArray");
	if(pAemDCPwr_MeasureArray == NULL)
		return -1;

	pAemDCPwr_MeasureMultiple = (PtrToAemDCPwr_MeasureMultiple)GetProcAddress(m_hmodule, "AemDCPwr_MeasureMultiple");
	if(pAemDCPwr_MeasureMultiple == NULL)
		return -1;

	pAemDCPwr_QueryInCompliance = (PtrToAemDCPwr_QueryInCompliance)GetProcAddress(m_hmodule, "AemDCPwr_QueryInCompliance");
	if(pAemDCPwr_QueryInCompliance == NULL)
		return -1;	

	pAemDCPwr_QueryOutputState = (PtrToAemDCPwr_QueryOutputState)GetProcAddress(m_hmodule, "AemDCPwr_QueryOutputState");
	if(pAemDCPwr_QueryOutputState == NULL)
		return -1;	

	pAemDCPwr_QueryMaxVoltageLevel = (PtrToAemDCPwr_QueryMaxVoltageLevel)GetProcAddress(m_hmodule, "AemDCPwr_QueryMaxVoltageLevel");
	if(pAemDCPwr_QueryMaxVoltageLevel == NULL)
		return -1;	

	pAemDCPwr_QueryMaxCurrentLimit= (PtrToAemDCPwr_QueryMaxCurrentLimit)GetProcAddress(m_hmodule, "AemDCPwr_QueryMaxCurrentLimit");
	if(pAemDCPwr_QueryMaxCurrentLimit == NULL)
		return -1;	
	
	pAemDCPwr_QueryMinCurrentLimit= (PtrToAemDCPwr_QueryMinCurrentLimit)GetProcAddress(m_hmodule, "AemDCPwr_QueryMinCurrentLimit");
	if(pAemDCPwr_QueryMinCurrentLimit == NULL)
		return -1;	

	pAemDCPwr_ConfigureAcquireRecordLength = (PtrToAemDCPwr_ConfigureAcquireRecordLength)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureAcquireRecordLength");
	if(pAemDCPwr_ConfigureAcquireRecordLength == NULL)
		return -1;

	pAemDCPwr_ConfigureAcquireRecordDeltaTime = (PtrToAemDCPwr_ConfigureAcquireRecordDeltaTime)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureAcquireRecordDeltaTime");
	if(pAemDCPwr_ConfigureAcquireRecordDeltaTime == NULL)
		return -1;

	pAemDCPwr_QueryAcquireRecordLength = (PtrToAemDCPwr_QueryAcquireRecordLength)GetProcAddress(m_hmodule, "AemDCPwr_QueryAcquireRecordLength");
	if(pAemDCPwr_QueryAcquireRecordLength == NULL)
		return -1;

	pAemDCPwr_ClearAcquireRecordLength = (PtrToAemDCPwr_ClearAcquireRecordLength)GetProcAddress(m_hmodule, "AemDCPwr_ClearAcquireRecordLength");
	if(pAemDCPwr_ClearAcquireRecordLength == NULL)
		return -1;

	pAemDCPwr_AcquireMultiple = (PtrToAemDCPwr_AcquireMultiple)GetProcAddress(m_hmodule, "AemDCPwr_AcquireMultiple");
	if(pAemDCPwr_AcquireMultiple == NULL)
		return -1;

	pAemDCPwr_AcquireArray = (PtrToAemDCPwr_AcquireArray)GetProcAddress(m_hmodule, "AemDCPwr_AcquireArray");
	if(pAemDCPwr_AcquireArray == NULL)
		return -1;

	pAemDCPwr_ConfigureMeasureMode = (PtrToAemDCPwr_ConfigureMeasureMode)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureMeasureMode");
	if(pAemDCPwr_ConfigureMeasureMode == NULL)
		return -1;
    
	pAemDCPwr_ContactCheck = (PtrToAemDCPwr_ContactCheck)GetProcAddress(m_hmodule, "AemDCPwr_ContactCheck");
	if(pAemDCPwr_ContactCheck == NULL)
		return -1;

	pAemDCPwr_ConfigureMeasureTriggerInput = (PtrToAemDCPwr_ConfigureMeasureTriggerInput)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureMeasureTriggerInput");
	if(pAemDCPwr_ConfigureMeasureTriggerInput == NULL)
		return -1;

	pAemDCPwr_ConfigureTriggerEdgeLevel = (PtrToAemDCPwr_ConfigureTriggerEdgeLevel)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureTriggerEdgeLevel");
	if(pAemDCPwr_ConfigureTriggerEdgeLevel == NULL)
		return -1;

	pAemDCPwr_MapTriggerInToTriggerOut = (PtrToAemDCPwr_MapTriggerInToTriggerOut)GetProcAddress(m_hmodule, "AemDCPwr_MapTriggerInToTriggerOut");
	if(pAemDCPwr_MapTriggerInToTriggerOut == NULL)
		return -1;

	pAemDCPwr_MapExtTrigOut = (PtrToAemDCPwr_MapExtTrigOut)GetProcAddress(m_hmodule, "AemDCPwr_MapExtTrigOut");
	if(pAemDCPwr_MapExtTrigOut == NULL)
		return -1;

	pAemDCPwr_DriveSoftwareTrigger = (PtrToAemDCPwr_DriveSoftwareTrigger)GetProcAddress(m_hmodule, "AemDCPwr_DriveSoftwareTrigger");
	if(pAemDCPwr_DriveSoftwareTrigger == NULL)
		return -1;

	pAemDCPwr_DriveSoftwareTriggerOut = (PtrToAemDCPwr_DriveSoftwareTriggerOut)GetProcAddress(m_hmodule, "AemDCPwr_DriveSoftwareTriggerOut");
	if(pAemDCPwr_DriveSoftwareTriggerOut == NULL)
		return -1;
    
	pAemDCPwr_ConfigureSMUOutputTriggerMode = (PtrToAemDCPwr_ConfigureSMUOutputTriggerMode)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureSMUOutputTriggerMode");
	if(pAemDCPwr_ConfigureSMUOutputTriggerMode == NULL)
		return -1;

	pAemDCPwr_ConfigureSMUOutputTriggerPulseWidth = (PtrToAemDCPwr_ConfigureSMUOutputTriggerPulseWidth)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureSMUOutputTriggerPulseWidth");
	if(pAemDCPwr_ConfigureSMUOutputTriggerPulseWidth == NULL)
		return -1;

	pAemDCPwr_ConfigureSMUOutputTriggerDuringSource = (PtrToAemDCPwr_ConfigureSMUOutputTriggerDuringSource)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureSMUOutputTriggerDuringSource");
	if(pAemDCPwr_ConfigureSMUOutputTriggerDuringSource == NULL)
		return -1;
    
	pAemDCPwr_GetExtCalRecommendedInterval = (PtrToAemDCPwr_GetExtCalRecommendedInterval)GetProcAddress(m_hmodule, "AemDCPwr_GetExtCalRecommendedInterval");
	if(pAemDCPwr_GetExtCalRecommendedInterval == NULL)
		return -1;
    
	pAemDCPwr_GetExtCalLastDateAndTime = (PtrToAemDCPwr_GetExtCalLastDateAndTime)GetProcAddress(m_hmodule, "AemDCPwr_GetExtCalLastDateAndTime");
	if(pAemDCPwr_GetExtCalLastDateAndTime == NULL)
		return -1; 

	pAemDCPwr_ConfigureInputTriggerSelect = (PtrToAemDCPwr_ConfigureInputTriggerSelect)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureInputTriggerSelect");
	if(pAemDCPwr_ConfigureInputTriggerSelect == NULL)
		return -1;

	pAemDCPwr_ConfigureOutputTriggerSelect = (PtrToAemDCPwr_ConfigureOutputTriggerSelect)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureOutputTriggerSelect");
	if(pAemDCPwr_ConfigureOutputTriggerSelect == NULL)
		return -1;

	pAemDCPwr_ConfigureTriggerEdgeLevelExtra = (PtrToAemDCPwr_ConfigureTriggerEdgeLevelExtra)GetProcAddress(m_hmodule, "AemDCPwr_ConfigureTriggerEdgeLevelExtra");
	if(pAemDCPwr_ConfigureTriggerEdgeLevelExtra == NULL)
		return -1;
	
	return 0;
}

ViStatus AemDCPwr_InitChannels(ViRsrc resourceName, ViConstString channels, ViInt32 init_options,ViConstString optionString,ViSession *vi)
{
	return pAemDCPwr_InitChannels(resourceName, channels, init_options, optionString,vi);
}
ViStatus AemDCPwr_Close(ViSession vi)
{
	return pAemDCPwr_Close(vi);
}
ViStatus AemDCPwr_QueryModuleType(ViSession vi, ViUInt32* module_type)
{
	return pAemDCPwr_QueryModuleType(vi, module_type);
}
ViStatus AemDCPwr_ReadRevision(ViSession vi, ViChar* instrumentDriverRevision, ViChar* firmwareRevision)
{
	return pAemDCPwr_ReadRevision(vi,instrumentDriverRevision,firmwareRevision);
}
ViStatus AemDCPwr_ReadSerialNumber(ViSession vi, ViChar* sn)
{
	return pAemDCPwr_ReadSerialNumber(vi, sn);
}
ViStatus AemDCPwr_ReadCurrentTemperature(ViSession vi, ViReal64* temperature)
{
	return pAemDCPwr_ReadCurrentTemperature(vi, temperature);
}
ViStatus AemDCPwr_ReadAmbientTemperature(ViSession vi, ViReal64* temperature)
{
	return pAemDCPwr_ReadAmbientTemperature(vi, temperature);
}
ViStatus AemDCPwr_GetError(ViSession vi, ViStatus* code, ViInt32 bufferSize, ViChar* description)
{
  return pAemDCPwr_GetError(vi,code,bufferSize,description);
}
ViStatus AemDCPwr_GetErrorMessage(ViSession vi, ViStatus errorCode, ViChar* errorMessage)
{
  return pAemDCPwr_GetErrorMessage( vi,  errorCode, errorMessage);
}
ViStatus AemDCPwr_ClearError(ViSession vi)
{
  return pAemDCPwr_ClearError(vi);
}
ViStatus AemDCPwr_Reset(ViSession vi)
{
	return pAemDCPwr_Reset(vi);
}
ViStatus AemDCPwr_ResetChannel(ViSession vi, ViConstString channels)
{
	return pAemDCPwr_ResetChannel(vi, channels);
}
ViStatus AemDCPwr_ConfigureMultiSiteMode(ViSession vi, ViInt32 mode)
{
	return pAemDCPwr_ConfigureMultiSiteMode(vi, mode);
}
ViStatus AemDCPwr_ConfigureVoltageLevel(ViSession vi, ViConstString channelName, ViReal64 level)
{
	return pAemDCPwr_ConfigureVoltageLevel(vi,channelName,level);
}
ViStatus AemDCPwr_ConfigureVoltageLevelAndRange(ViSession vi, ViConstString channelName, ViReal64 level, ViReal64 range)
{
	return pAemDCPwr_ConfigureVoltageLevelAndRange(vi,channelName,level, range);
}
ViStatus AemDCPwr_ConfigureCurrentLimit(ViSession vi, ViConstString channelName, ViInt32 behavior, ViReal64 limit)
{
	return pAemDCPwr_ConfigureCurrentLimit(vi,channelName,behavior,limit);
}
ViStatus AemDCPwr_ConfigureCurrentLimitAndRange(ViSession vi, ViConstString channelName, ViInt32 behaviour, ViReal64 limit, ViReal64 range)
{
	return pAemDCPwr_ConfigureCurrentLimitAndRange(vi,channelName,behaviour,limit, range);
}
ViStatus AemDCPwr_ConfigureCurrentLevel(ViSession vi, ViConstString channelName, ViReal64 level)
{
	return pAemDCPwr_ConfigureCurrentLevel(vi,channelName,level);
}
ViStatus AemDCPwr_ConfigureCurrentLevelAndRange(ViSession vi, ViConstString channelName,ViReal64 level,  ViReal64 range)
{
	return pAemDCPwr_ConfigureCurrentLevelAndRange(vi,channelName,level, range);;
}
ViStatus AemDCPwr_ConfigureVoltageLimit(ViSession vi, ViConstString channelName, ViReal64 limit)
{
	return pAemDCPwr_ConfigureVoltageLimit(vi,channelName,limit);
}
ViStatus AemDCPwr_ConfigureVoltageLimitAndRange(ViSession vi, ViConstString channelName, ViReal64 limit, ViReal64 range)
{
	return pAemDCPwr_ConfigureVoltageLimitAndRange(vi,channelName,limit,range);
}
ViStatus AemDCPwr_ReadVoltageLevelRange(ViSession vi,  ViConstString channelName, ViReal64* range)
{
	return pAemDCPwr_ReadVoltageLevelRange(vi,channelName,range);
}
ViStatus AemDCPwr_ReadVoltageLimitRange(ViSession vi,  ViConstString channelName, ViReal64* range)
{
	return pAemDCPwr_ReadVoltageLimitRange(vi,channelName,range);
}
ViStatus AemDCPwr_ReadCurrentLevelRange(ViSession vi,  ViConstString channelName, ViReal64* range)
{
	return pAemDCPwr_ReadCurrentLevelRange(vi,channelName,range);
}
ViStatus AemDCPwr_ReadCurrentLimitRange(ViSession vi,  ViConstString channelName, ViReal64* range)
{
	return pAemDCPwr_ReadCurrentLimitRange(vi,channelName,range);
}
ViStatus AemDCPwr_ConfigureOutputFunction(ViSession vi, ViConstString channelName, ViInt32 function)
{
	return pAemDCPwr_ConfigureOutputFunction(vi,channelName,function);
}
ViStatus AemDCPwr_ConfigureOutputTransient(ViSession vi, ViConstString channelName, ViInt32 transient)
{
	return pAemDCPwr_ConfigureOutputTransient(vi,channelName,transient);
}
ViStatus AemDCPwr_ComputeAIBandwidth(ViSession vi, ViConstString channelName, ViInt32 mode, ViReal64 vRange, ViReal64 iRange, ViReal64 drive_settling_time, ViReal64 clamp_settling_time, ViReal64 resistance, ViReal64 capacitance, ViReal64 reserved, ViInt32 store_location)
{
	return pAemDCPwr_ComputeAIBandwidth( vi, channelName, mode,vRange, iRange,  drive_settling_time, clamp_settling_time, resistance, capacitance, reserved, store_location);
}
ViStatus AemDCPwr_ConfigureOutputSwitch(ViSession vi, ViConstString channelName, ViInt32 switches)
{
	return pAemDCPwr_ConfigureOutputSwitch(vi,channelName,switches);
}
ViStatus AemDCPwr_ConfigureOutputEnabled(ViSession vi, ViConstString channelName, ViBoolean enabled)
{
	return pAemDCPwr_ConfigureOutputEnabled(vi,channelName,enabled);
}
ViStatus AemDCPwr_ConfigureOutputResistance(ViSession vi, ViConstString channelName,ViReal64 resistance, ViReal64 range)
{
	return pAemDCPwr_ConfigureOutputResistance(vi,channelName,resistance,range);
}
ViStatus AemDCPwr_ConfigureSamplingTime(ViSession vi, ViConstString channelName, ViReal64 apertureTime, ViInt32 units)
{
	return pAemDCPwr_ConfigureSamplingTime(vi,channelName,apertureTime,units);
}
ViStatus AemDCPwr_ConfigureSense(ViSession vi, ViConstString channelName, ViInt32 sense)
{
	return pAemDCPwr_ConfigureSense(vi, channelName,sense);
}
ViStatus AemDCPwr_ConfigurePLF(ViSession vi, ViReal64 powerLineFrequency)
{
	return pAemDCPwr_ConfigurePLF(vi, powerLineFrequency);
}
ViStatus AemDCPwr_Measure(ViSession vi, ViConstString channelName, ViInt32 measurementType, ViReal64* measurement)
{
	return pAemDCPwr_Measure(vi,channelName,measurementType,measurement);
}
ViStatus AemDCPwr_MeasureArray(ViSession vi, ViConstString channelName,  ViBoolean printToTxt, ViInt32 measurementType,ViReal64* measurement)
{
	return pAemDCPwr_MeasureArray(vi,channelName,printToTxt,measurementType,measurement);
}
ViStatus AemDCPwr_MeasureMultiple(ViSession vi, ViConstString channelName, ViReal64* voltageMeasurements,ViReal64* currentMeasurements)
{
	return pAemDCPwr_MeasureMultiple(vi, channelName, voltageMeasurements, currentMeasurements);
}
ViStatus AemDCPwr_QueryInCompliance(ViSession vi, ViConstString channelName,ViBoolean* inCompliance)
{
	return pAemDCPwr_QueryInCompliance(vi,channelName,inCompliance);
}
ViStatus AemDCPwr_QueryOutputState(ViSession vi, ViConstString channelName,ViInt32 outputState, ViBoolean* inState)
{
	return pAemDCPwr_QueryOutputState(vi,channelName,outputState, inState);
}
ViStatus AemDCPwr_QueryMaxVoltageLevel(ViSession vi, ViConstString channelName,ViReal64 currentLimit, ViReal64* maxVoltageLevel)
{
	return pAemDCPwr_QueryMaxVoltageLevel(vi,channelName,currentLimit, maxVoltageLevel);
}
ViStatus AemDCPwr_QueryMaxCurrentLimit(ViSession vi, ViConstString channelName,ViReal64 voltageLevel, ViReal64* maxCurrentLimit)
{
	return pAemDCPwr_QueryMaxCurrentLimit(vi,channelName, voltageLevel, maxCurrentLimit);
}
ViStatus AemDCPwr_QueryMinCurrentLimit(ViSession vi, ViConstString channelName,ViReal64 voltageLevel, ViReal64* minCurrentLimit)
{
	return pAemDCPwr_QueryMinCurrentLimit(vi,channelName, voltageLevel, minCurrentLimit);
}
ViStatus AemDCPwr_ConfigureAcquireRecordLength(ViSession vi, ViConstString channelName, ViInt32 length)
{
	return pAemDCPwr_ConfigureAcquireRecordLength(vi, channelName, length);
}
ViStatus AemDCPwr_ConfigureAcquireRecordDeltaTime(ViSession vi, ViConstString channelName, ViReal64 deltaTime)
{
	return pAemDCPwr_ConfigureAcquireRecordDeltaTime(vi, channelName, deltaTime);
}
ViStatus AemDCPwr_QueryAcquireRecordLength(ViSession vi, ViConstString channelName, ViInt32* length)
{
	return pAemDCPwr_QueryAcquireRecordLength(vi, channelName, length);
}
ViStatus AemDCPwr_ClearAcquireRecordLength(ViSession vi, ViConstString channelName)
{
	return pAemDCPwr_ClearAcquireRecordLength(vi, channelName);
}
ViStatus AemDCPwr_AcquireMultiple(ViSession vi, ViConstString channelName, ViReal64 timeout, ViInt32 count, ViReal64* voltageMeasurements, ViReal64* currentMeasurements, ViBoolean* inCompliance, ViInt32* actualCount)
{
	return pAemDCPwr_AcquireMultiple(vi,channelName,timeout,count,voltageMeasurements, currentMeasurements, inCompliance,actualCount);
}
ViStatus AemDCPwr_AcquireArray(ViSession vi, ViConstString channelName, ViReal64 timeout, ViReal64* voltageMeasurements, ViReal64* currentMeasurements, ViInt32* actualCount)
{
	return pAemDCPwr_AcquireArray(vi, channelName, timeout, voltageMeasurements, currentMeasurements, actualCount);
}	
ViStatus AemDCPwr_ConfigureMeasureMode(ViSession vi, ViConstString channelName, ViInt32 mode)
{
	return pAemDCPwr_ConfigureMeasureMode(vi,  channelName, mode);
}
ViStatus AemDCPwr_ContactCheck(ViSession vi, ViConstString channelName, ViInt32 shi_h_slo_l, ViInt32 threshold_mohm, ViReal64 delay_s, ViInt32* pass_h_fail_l, ViInt32* mohm)
{
	return pAemDCPwr_ContactCheck(vi,channelName,shi_h_slo_l,threshold_mohm,delay_s,pass_h_fail_l,mohm);
}
ViStatus AemDCPwr_ConfigureMeasureTriggerInput(ViSession vi, ViInt32 triggerInput)
{
	return pAemDCPwr_ConfigureMeasureTriggerInput( vi, triggerInput);
}
ViStatus AemDCPwr_ConfigureTriggerEdgeLevel(ViSession vi, ViInt32 triggerEnum, ViInt32 option)
{
	return pAemDCPwr_ConfigureTriggerEdgeLevel( vi,  triggerEnum,  option);
}
ViStatus AemDCPwr_MapTriggerInToTriggerOut(ViSession vi, ViInt32 inputTerminal, ViInt32 outputTerminal)
{
	return pAemDCPwr_MapTriggerInToTriggerOut( vi,  inputTerminal,  outputTerminal);
}
ViStatus AemDCPwr_MapExtTrigOut(ViSession vi, ViInt32 inputTerminal)
{
	return pAemDCPwr_MapExtTrigOut( vi,  inputTerminal);
}
ViStatus AemDCPwr_DriveSoftwareTrigger(ViSession vi, ViInt32 select, ViReal64 pulseWidth)
{
	return pAemDCPwr_DriveSoftwareTrigger( vi, select, pulseWidth);
}
ViStatus AemDCPwr_DriveSoftwareTriggerOut(ViSession vi, ViInt32 outputTerminal, ViReal64 pulseWidth)
{
	return pAemDCPwr_DriveSoftwareTriggerOut( vi, outputTerminal, pulseWidth);
}
ViStatus AemDCPwr_ConfigureSMUOutputTriggerMode(ViSession vi, ViConstString channelName, ViInt32 mode)
{
	return pAemDCPwr_ConfigureSMUOutputTriggerMode(vi,  channelName, mode);
}
ViStatus AemDCPwr_ConfigureSMUOutputTriggerPulseWidth(ViSession vi, ViConstString channelName, ViReal64 pulseWidth)
{
	return pAemDCPwr_ConfigureSMUOutputTriggerPulseWidth(vi,  channelName, pulseWidth);
}
ViStatus AemDCPwr_ConfigureSMUOutputTriggerDuringSource(ViSession vi, ViConstString channelName, ViReal64 level, ViReal64 range, ViInt32 mode, ViInt32 edgeSetting)
{
	return pAemDCPwr_ConfigureSMUOutputTriggerDuringSource(vi,  channelName, level, range, mode, edgeSetting);
}
ViStatus AemDCPwr_GetExtCalRecommendedInterval(ViSession vi, ViInt32* months)
{
	return pAemDCPwr_GetExtCalRecommendedInterval( vi, months);
}
ViStatus AemDCPwr_GetExtCalLastDateAndTime(ViSession vi, ViInt32* year, ViInt32* month, ViInt32* day, ViInt32* hour, ViInt32* minute)
{
	return pAemDCPwr_GetExtCalLastDateAndTime( vi, year, month, day, hour ,minute);
}
ViStatus AemDCPwr_ConfigureInputTriggerSelect(ViSession vi, ViConstString channelName, ViInt32 triggerInput, ViReal64 delay_s)
{
	return pAemDCPwr_ConfigureInputTriggerSelect(vi, channelName, triggerInput, delay_s);
}
ViStatus AemDCPwr_ConfigureOutputTriggerSelect(ViSession vi, ViConstString channelName, ViInt32 triggerOutput, ViReal64 delay_s)
{
	return pAemDCPwr_ConfigureOutputTriggerSelect(vi, channelName, triggerOutput, delay_s);
}
ViStatus AemDCPwr_ConfigureTriggerEdgeLevelExtra(ViSession vi, ViInt32 triggerEnum, ViInt32 option, ViInt32 ignore_trigger_count)
{
	return pAemDCPwr_ConfigureTriggerEdgeLevelExtra(vi, triggerEnum, option, ignore_trigger_count);
}
}


