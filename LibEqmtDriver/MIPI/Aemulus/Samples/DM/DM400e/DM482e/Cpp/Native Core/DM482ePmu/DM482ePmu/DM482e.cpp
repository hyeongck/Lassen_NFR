///////////////////////////////////////////////////////////////////////////////
//               Aemulus DM482e
// --------------------------------------------------------------------------
//       Copyright (c) Aemulus 2012.  All Rights Reserved.      
// --------------------------------------------------------------------------
// 
//  Title:    DM482e.cpp
//  Purpose:  Aemulus DM482e API declarations
// 
///////////////////////////////////////////////////////////////////////////////
#include "DM482e.h"

namespace dm482e
{
// TypeDefs ----------------------------------------

// Hardware management

// General Functions
typedef ViStatus (*PtrToDM482e_DPINOpen)(ViRsrc resourceName, ViInt32 dpingroup_sel, ViInt32 init_options, ViConstString optionString, ViSession* vi);
typedef ViStatus (*PtrToDM482e_DPINClose)(ViSession vi);
typedef ViStatus (*PtrToDM482e_DPINForce)(ViSession vi, ViInt32 pinNo, ViInt32 state);
typedef ViStatus (*PtrToDM482e_ReadRevision)(ViSession vi, ViChar* instrumentDriverRevision, ViChar* firmwareRevision);
typedef ViStatus (*PtrToDM482e_ReadChannelTemperature)(ViSession vi, ViInt32 pinNo, ViReal64* temperature);
typedef ViStatus (*PtrToDM482e_ReadAmbientTemperature)(ViSession vi, ViReal64* temperature);
typedef ViStatus (*PtrToDM482e_ReadSerialNumber)(ViSession vi, ViChar* sn);
typedef ViStatus (*PtrToDM482e_Reset)(ViSession vi);
typedef ViStatus (*PtrToDM482e_ResetGroup)(ViSession vi, ViInt32 group);
typedef ViStatus (*PtrToDM482e_ConfigureMultiSiteMode)(ViSession vi, ViInt32 mode);

// Pin Electronics Related API
typedef ViStatus (*PtrToDM482e_DPINVectorResourceAllocation)(ViSession vi, ViInt32 vecSetCount, ViInt32* resourceArray);
typedef ViStatus (*PtrToDM482e_DPINLevel)(ViSession vi, ViInt32 pinNo, ViReal64 VIH, ViReal64 VIL, ViReal64 VOH, ViReal64 VOL, ViReal64 IOH, ViReal64 IOL, ViReal64 VCH, ViReal64 VCL, ViReal64 VTERM);
typedef ViStatus (*PtrToDM482e_DPINVecLoad)(ViSession vi, ViInt32 vecSetNo, char* vecFileName);
typedef ViStatus (*PtrToDM482e_DPINPeriod)(ViSession vi, ViInt32 timingSetNo, ViReal64 period_s);
typedef ViStatus (*PtrToDM482e_DPINOn)(ViSession vi, ViInt32 pinNo);
typedef ViStatus (*PtrToDM482e_DPINOff)(ViSession vi, ViInt32 pinNo);
typedef ViStatus (*PtrToDM482e_DPINHVOn)(ViSession vi, ViInt32 pinNo);
typedef ViStatus (*PtrToDM482e_DPINHVOff)(ViSession vi, ViInt32 pinNo);
typedef ViStatus (*PtrToDM482e_RunVector)(ViSession vi, ViInt32 vecSetNo);
typedef ViStatus (*PtrToDM482e_AcquireVecEngineStatus)(ViSession vi, ViInt32* status);
typedef ViStatus (*PtrToDM482e_ReadHistoryRam)(ViSession vi, ViInt32 vecSetNo, ViUInt32* history_ram_data);
typedef ViStatus (*PtrToDM482e_ConfigurePEAttribute)(ViSession vi, ViInt32 pinNo, ViBoolean inputTerminationEnable, ViBoolean HVEnable, ViBoolean activeLoadEnable, ViBoolean differentialComparatorEnable);
typedef ViStatus (*PtrToDM482e_ConfigureVectorEngineAttribute)(ViSession vi, ViBoolean triggerEnable, ViBoolean continuousEnable);
typedef ViStatus (*PtrToDM482e_ConfigureClockFrequency)(ViSession vi, ViReal64 frequency);
typedef ViStatus (*PtrToDM482e_ConfigureInputChannelDelay)(ViSession vi, ViInt32 pinNo, ViReal64 delay);
typedef ViStatus (*PtrToDM482e_AcquireVectorFailCount)(ViSession vi, ViInt32* failCount);
typedef ViStatus (*PtrToDM482e_AcquireChannelVectorFailCount)(ViSession vi, ViInt32 pinNo, ViInt32* failCount);
typedef ViStatus (*PtrToDM482e_AcquireChannelFirstFailVectorCount)(ViSession vi, ViInt32 pinNo, ViInt32* failCount);

// Pin Measurement Unit Related API
typedef ViStatus (*PtrToDM482e_ConfigurePMUVoltageLevel)(ViSession vi, ViInt32 pinNo, ViReal64 level);
typedef ViStatus (*PtrToDM482e_ConfigurePMUVoltageLimit)(ViSession vi, ViInt32 pinNo, ViReal64 high_limit, ViReal64 low_limit);
typedef ViStatus (*PtrToDM482e_CONFIGUREPMUCURRENTLEVEL)(ViSession vi, ViInt32 pinNo, ViReal64 level);
typedef ViStatus (*PtrToDM482e_CONFIGUREPMUCURRENTLEVELANDRANGE)(ViSession vi, ViInt32 pinNo, ViReal64 level, ViReal64 range);
typedef ViStatus (*PtrToDM482e_CONFIGUREPMUCURRENTLIMITRANGE)(ViSession vi, ViInt32 pinNo, ViReal64 range);
typedef ViStatus (*PtrToDM482e_GETPMUVOLTAGELEVELRANGE)(ViSession vi, ViInt32 pinNo, ViReal64* range);
typedef ViStatus (*PtrToDM482e_GETPMUVOLTAGELIMITRANGE)(ViSession vi, ViInt32 pinNo, ViReal64* range);
typedef ViStatus (*PtrToDM482e_GETPMUCURRENTLEVELRANGE)(ViSession vi, ViInt32 pinNo, ViReal64* range);
typedef ViStatus (*PtrToDM482e_GETPMUCURRENTLIMITRANGE)(ViSession vi, ViInt32 pinNo, ViReal64* range);
typedef ViStatus (*PtrToDM482e_CONFIGUREPMUOUTPUTFUNCTION)(ViSession vi, ViInt32 pinNo, ViInt32 function);
typedef ViStatus (*PtrToDM482e_CONFIGUREPMUSAMPLINGTIME)(ViSession vi, ViInt32 pinNo, ViReal64 samplingTime, ViInt32 units);
typedef ViStatus (*PtrToDM482e_CONFIGUREPOWERLINEFREQUENCY)(ViSession vi, ViReal64 powerLineFrequency);
typedef ViStatus (*PtrToDM482e_CONFIGUREPMUSENSE)(ViSession vi, ViInt32 pinNo, ViInt32 sense);
typedef ViStatus (*PtrToDM482e_GETPMUSENSE)(ViSession vi, ViInt32 pinNo, ViInt32* sense);
typedef ViStatus (*PtrToDM482e_PMUMEASURE)(ViSession vi, ViInt32 pinNo, ViInt32 measurementType, ViReal64* measurement);

// Trigger Related API
typedef ViStatus (*PtrToDM482e_ConfigureTriggerEdgeLevel)(ViSession vi, ViInt32 triggerEnum, ViInt32 option);
typedef ViStatus (*PtrToDM482e_MapTriggerInToTriggerOut)(ViSession vi, ViInt32 inputTerminal, ViInt32 outputTerminal);
typedef ViStatus (*PtrToDM482e_ConfigureInputTriggerSelect)(ViSession vi, ViInt32 triggerInput);
typedef ViStatus (*PtrToDM482e_ConfigureOutputTriggerSelect)(ViSession vi, ViInt32 triggerOutput0, ViInt32 triggerOutput1);
typedef ViStatus (*PtrToDM482e_DriveSoftwareTrigger)(ViSession vi, ViReal64 pulseWidth);

// MIPI Related API
typedef ViStatus (*PtrToDM482e_MIPI_Configure_Clock)(ViSession vi, ViInt32 mipi_pair, ViInt32 freq_Hz);
typedef ViStatus (*PtrToDM482e_MIPI_Connect)(ViSession vi, ViInt32 mipi_pair, ViInt32 setting);
typedef ViStatus (*PtrToDM482e_MIPI_RFFE_WR)(ViSession vi, ViInt32 mipi_pair, ViInt32 Command, ViInt32* Data);
typedef ViStatus (*PtrToDM482e_MIPI_RFFE_RD)(ViSession vi, ViInt32 mipi_pair, ViInt32 full_speed, ViInt32 Command, ViInt32* Data);
typedef ViStatus (*PtrToDM482e_MIPI_RFFE_Retrieve)(ViSession vi, ViInt32 mipi_pair, ViInt32* rd_byte_data_count, ViInt32* rd_data_array, ViInt32* parity_check_array);

// DIO Related API
typedef ViStatus (*PtrToDM482e_DrivePort)(ViSession vi, ViInt32 value);
typedef ViStatus (*PtrToDM482e_DrivePin)(ViSession vi, ViInt32 pinNo, ViInt32 value);
typedef ViStatus (*PtrToDM482e_SetPortDirection)(ViSession vi, ViInt32 value);
typedef ViStatus (*PtrToDM482e_SetPinDirection)(ViSession vi, ViInt32 pinNo, ViInt32 value);
typedef ViStatus (*PtrToDM482e_ReadPort)(ViSession vi, ViInt32* value);
typedef ViStatus (*PtrToDM482e_ReadPin)(ViSession vi, ViInt32 pinNo, ViInt32* valuee);

///////////////////////////////////////////////////////////////////////////
// Function Pointers ----------------------------------------
// General Functions
PtrToDM482e_DPINOpen pDM482e_DPINOpen;
PtrToDM482e_DPINClose pDM482e_DPINClose;
PtrToDM482e_DPINForce pDM482e_DPINForce;
PtrToDM482e_ReadRevision pDM482e_ReadRevision;
PtrToDM482e_ReadChannelTemperature pDM482e_ReadChannelTemperature;
PtrToDM482e_ReadAmbientTemperature pDM482e_ReadAmbientTemperature;
PtrToDM482e_ReadSerialNumber pDM482e_ReadSerialNumber;
PtrToDM482e_Reset pDM482e_Reset;
PtrToDM482e_ResetGroup pDM482e_ResetGroup;
PtrToDM482e_ConfigureMultiSiteMode pDM482e_ConfigureMultiSiteMode;

// Pin Electronics Related API
PtrToDM482e_DPINVectorResourceAllocation pDM482e_DPINVectorResourceAllocation;
PtrToDM482e_DPINLevel pDM482e_DPINLevel;
PtrToDM482e_DPINVecLoad pDM482e_DPINVecLoad;
PtrToDM482e_DPINPeriod pDM482e_DPINPeriod;
PtrToDM482e_DPINOn pDM482e_DPINOn;
PtrToDM482e_DPINOff pDM482e_DPINOff;
PtrToDM482e_DPINHVOn pDM482e_DPINHVOn;
PtrToDM482e_DPINHVOff pDM482e_DPINHVOff;
PtrToDM482e_RunVector pDM482e_RunVector;
PtrToDM482e_AcquireVecEngineStatus pDM482e_AcquireVecEngineStatus;
PtrToDM482e_ReadHistoryRam pDM482e_ReadHistoryRam;
PtrToDM482e_ConfigurePEAttribute pDM482e_ConfigurePEAttribute;
PtrToDM482e_ConfigureVectorEngineAttribute pDM482e_ConfigureVectorEngineAttribute;
PtrToDM482e_ConfigureClockFrequency pDM482e_ConfigureClockFrequency;
PtrToDM482e_ConfigureInputChannelDelay pDM482e_ConfigureInputChannelDelay;
PtrToDM482e_AcquireVectorFailCount pDM482e_AcquireVectorFailCount;
PtrToDM482e_AcquireChannelVectorFailCount pDM482e_AcquireChannelVectorFailCount;
PtrToDM482e_AcquireChannelFirstFailVectorCount pDM482e_AcquireChannelFirstFailVectorCount;

// Pin Measurement Unit Related API
PtrToDM482e_ConfigurePMUVoltageLevel pDM482e_ConfigurePMUVoltageLevel;
PtrToDM482e_ConfigurePMUVoltageLimit pDM482e_ConfigurePMUVoltageLimit;
PtrToDM482e_CONFIGUREPMUCURRENTLEVEL pDM482e_CONFIGUREPMUCURRENTLEVEL;
PtrToDM482e_CONFIGUREPMUCURRENTLEVELANDRANGE pDM482e_CONFIGUREPMUCURRENTLEVELANDRANGE;
PtrToDM482e_CONFIGUREPMUCURRENTLIMITRANGE pDM482e_CONFIGUREPMUCURRENTLIMITRANGE;
PtrToDM482e_GETPMUVOLTAGELEVELRANGE pDM482e_GETPMUVOLTAGELEVELRANGE;
PtrToDM482e_GETPMUVOLTAGELIMITRANGE pDM482e_GETPMUVOLTAGELIMITRANGE;
PtrToDM482e_GETPMUCURRENTLEVELRANGE pDM482e_GETPMUCURRENTLEVELRANGE;
PtrToDM482e_GETPMUCURRENTLIMITRANGE pDM482e_GETPMUCURRENTLIMITRANGE;
PtrToDM482e_CONFIGUREPMUOUTPUTFUNCTION pDM482e_CONFIGUREPMUOUTPUTFUNCTION;
PtrToDM482e_CONFIGUREPMUSAMPLINGTIME pDM482e_CONFIGUREPMUSAMPLINGTIME;
PtrToDM482e_CONFIGUREPOWERLINEFREQUENCY pDM482e_CONFIGUREPOWERLINEFREQUENCY;
PtrToDM482e_CONFIGUREPMUSENSE pDM482e_CONFIGUREPMUSENSE;
PtrToDM482e_GETPMUSENSE pDM482e_GETPMUSENSE;
PtrToDM482e_PMUMEASURE pDM482e_PMUMEASURE;

// Trigger Related API
PtrToDM482e_ConfigureTriggerEdgeLevel pDM482e_ConfigureTriggerEdgeLevel;
PtrToDM482e_MapTriggerInToTriggerOut pDM482e_MapTriggerInToTriggerOut;
PtrToDM482e_ConfigureInputTriggerSelect pDM482e_ConfigureInputTriggerSelect;
PtrToDM482e_ConfigureOutputTriggerSelect pDM482e_ConfigureOutputTriggerSelect;
PtrToDM482e_DriveSoftwareTrigger pDM482e_DriveSoftwareTrigger;

// MIPI Related API
PtrToDM482e_MIPI_Configure_Clock pDM482e_MIPI_Configure_Clock;
PtrToDM482e_MIPI_Connect pDM482e_MIPI_Connect;
PtrToDM482e_MIPI_RFFE_WR pDM482e_MIPI_RFFE_WR;
PtrToDM482e_MIPI_RFFE_RD pDM482e_MIPI_RFFE_RD;
PtrToDM482e_MIPI_RFFE_Retrieve pDM482e_MIPI_RFFE_Retrieve;

// DIO Related API
PtrToDM482e_DrivePort pDM482e_DrivePort;
PtrToDM482e_DrivePin pDM482e_DrivePin;
PtrToDM482e_SetPortDirection pDM482e_SetPortDirection;
PtrToDM482e_SetPinDirection pDM482e_SetPinDirection;
PtrToDM482e_ReadPort pDM482e_ReadPort;
PtrToDM482e_ReadPin pDM482e_ReadPin;

int DM482e_LoadFunctions(HMODULE m_hmodule)
{
	// General Function
	pDM482e_DPINOpen = (PtrToDM482e_DPINOpen)GetProcAddress(m_hmodule, "DM482e_DPINOpen");
	if(pDM482e_DPINOpen == NULL)
		return -1;

	pDM482e_DPINClose = (PtrToDM482e_DPINClose)GetProcAddress(m_hmodule, "DM482e_DPINClose");
	if(pDM482e_DPINClose == NULL)
		return -2;
    
	pDM482e_DPINForce = (PtrToDM482e_DPINForce)GetProcAddress(m_hmodule, "DM482e_DPINForce");
	if(pDM482e_DPINForce == NULL)
		return -3;
    
	pDM482e_ReadRevision = (PtrToDM482e_ReadRevision)GetProcAddress(m_hmodule, "DM482e_ReadRevision");
	if(pDM482e_ReadRevision == NULL)
		return -4;

	pDM482e_ReadChannelTemperature = (PtrToDM482e_ReadChannelTemperature)GetProcAddress(m_hmodule, "DM482e_ReadChannelTemperature");
	if(pDM482e_ReadChannelTemperature == NULL)
		return -5;
    
	pDM482e_ReadAmbientTemperature = (PtrToDM482e_ReadAmbientTemperature)GetProcAddress(m_hmodule, "DM482e_ReadAmbientTemperature");
	if(pDM482e_ReadAmbientTemperature == NULL)
		return -6;
    
	pDM482e_ReadSerialNumber = (PtrToDM482e_ReadSerialNumber)GetProcAddress(m_hmodule, "DM482e_ReadSerialNumber");
	if(pDM482e_ReadSerialNumber == NULL)
		return -7;
    
	pDM482e_Reset = (PtrToDM482e_Reset)GetProcAddress(m_hmodule, "DM482e_Reset");
	if(pDM482e_Reset == NULL)
		return -8;
    
	pDM482e_ResetGroup = (PtrToDM482e_ResetGroup)GetProcAddress(m_hmodule, "DM482e_ResetGroup");
	if(pDM482e_ResetGroup == NULL)
		return -9;

	pDM482e_ConfigureMultiSiteMode = (PtrToDM482e_ConfigureMultiSiteMode)GetProcAddress(m_hmodule, "DM482e_ConfigureMultiSiteMode");
	if(pDM482e_ConfigureMultiSiteMode == NULL)
		return -10;
    
	// Pin Electronics Related API
	pDM482e_DPINVectorResourceAllocation = (PtrToDM482e_DPINVectorResourceAllocation)GetProcAddress(m_hmodule, "DM482e_DPINVectorResourceAllocation");
	if(pDM482e_DPINVectorResourceAllocation == NULL)
		return -11;
    
	pDM482e_DPINLevel = (PtrToDM482e_DPINLevel)GetProcAddress(m_hmodule, "DM482e_DPINLevel");
	if(pDM482e_DPINLevel == NULL)
		return -12;

	pDM482e_DPINVecLoad = (PtrToDM482e_DPINVecLoad)GetProcAddress(m_hmodule, "DM482e_DPINVecLoad");
	if(pDM482e_DPINVecLoad == NULL)
		return -13;

	pDM482e_DPINPeriod = (PtrToDM482e_DPINPeriod)GetProcAddress(m_hmodule, "DM482e_DPINPeriod");
	if(pDM482e_DPINPeriod == NULL)
		return -14;	

	pDM482e_DPINOn = (PtrToDM482e_DPINOn)GetProcAddress(m_hmodule, "DM482e_DPINOn");
	if(pDM482e_DPINOn == NULL)
		return -15;

	pDM482e_DPINOff = (PtrToDM482e_DPINOff)GetProcAddress(m_hmodule, "DM482e_DPINOff");
	if(pDM482e_DPINOff == NULL)
		return -16;

	pDM482e_DPINHVOn = (PtrToDM482e_DPINHVOn)GetProcAddress(m_hmodule, "DM482e_DPINHVOn");
	if(pDM482e_DPINHVOn == NULL)
		return -17;

	pDM482e_DPINHVOff = (PtrToDM482e_DPINHVOff)GetProcAddress(m_hmodule, "DM482e_DPINHVOff");
	if(pDM482e_DPINHVOff == NULL)
		return -18;

	pDM482e_RunVector = (PtrToDM482e_RunVector)GetProcAddress(m_hmodule, "DM482e_RunVector");
	if(pDM482e_RunVector == NULL)
		return -19;

	pDM482e_AcquireVecEngineStatus = (PtrToDM482e_AcquireVecEngineStatus)GetProcAddress(m_hmodule, "DM482e_AcquireVecEngineStatus");
	if(pDM482e_AcquireVecEngineStatus == NULL)
		return -20;
    
	pDM482e_ReadHistoryRam = (PtrToDM482e_ReadHistoryRam)GetProcAddress(m_hmodule, "DM482e_ReadHistoryRam");
	if(pDM482e_ReadHistoryRam == NULL)
		return -21;
     
	pDM482e_ConfigurePEAttribute = (PtrToDM482e_ConfigurePEAttribute)GetProcAddress(m_hmodule, "DM482e_ConfigurePEAttribute");
	if(pDM482e_ConfigurePEAttribute == NULL)
		return -22;
    
	pDM482e_ConfigureVectorEngineAttribute = (PtrToDM482e_ConfigureVectorEngineAttribute)GetProcAddress(m_hmodule, "DM482e_ConfigureVectorEngineAttribute");
	if(pDM482e_ConfigureVectorEngineAttribute == NULL)
		return -23;
    
	pDM482e_ConfigureClockFrequency = (PtrToDM482e_ConfigureClockFrequency)GetProcAddress(m_hmodule, "DM482e_ConfigureClockFrequency");
	if(pDM482e_ConfigureClockFrequency == NULL)
		return -24;
    
	pDM482e_ConfigureInputChannelDelay = (PtrToDM482e_ConfigureInputChannelDelay)GetProcAddress(m_hmodule, "DM482e_ConfigureInputChannelDelay");
	if(pDM482e_ConfigureInputChannelDelay == NULL)
		return -25;

	pDM482e_AcquireVectorFailCount = (PtrToDM482e_AcquireVectorFailCount)GetProcAddress(m_hmodule, "DM482e_AcquireVectorFailCount");
	if(pDM482e_AcquireVectorFailCount == NULL)
		return -26;

	pDM482e_AcquireChannelVectorFailCount = (PtrToDM482e_AcquireChannelVectorFailCount)GetProcAddress(m_hmodule, "DM482e_AcquireChannelVectorFailCount");
	if(pDM482e_AcquireChannelVectorFailCount == NULL)
		return -27;

	pDM482e_AcquireChannelFirstFailVectorCount = (PtrToDM482e_AcquireChannelFirstFailVectorCount)GetProcAddress(m_hmodule, "DM482e_AcquireChannelFirstFailVectorCount");
	if(pDM482e_AcquireChannelFirstFailVectorCount == NULL)
		return -28;
   
	// Pin Measurement Unit Related API
	pDM482e_ConfigurePMUVoltageLevel = (PtrToDM482e_ConfigurePMUVoltageLevel)GetProcAddress(m_hmodule, "DM482e_ConfigurePMUVoltageLevel");
	if(pDM482e_ConfigurePMUVoltageLevel == NULL)
		return -39;

	pDM482e_ConfigurePMUVoltageLimit = (PtrToDM482e_ConfigurePMUVoltageLimit)GetProcAddress(m_hmodule, "DM482e_ConfigurePMUVoltageLimit");
	if(pDM482e_ConfigurePMUVoltageLimit == NULL)
		return -40;

	pDM482e_CONFIGUREPMUCURRENTLEVEL = (PtrToDM482e_CONFIGUREPMUCURRENTLEVEL)GetProcAddress(m_hmodule, "DM482e_ConfigurePMUCurrentLevel");
	if(pDM482e_CONFIGUREPMUCURRENTLEVEL == NULL)
		return -41;

	pDM482e_CONFIGUREPMUCURRENTLEVELANDRANGE = (PtrToDM482e_CONFIGUREPMUCURRENTLEVELANDRANGE)GetProcAddress(m_hmodule, "DM482e_ConfigurePMUCurrentLevelAndRange");
	if(pDM482e_CONFIGUREPMUCURRENTLEVELANDRANGE == NULL)
		return -42;

	pDM482e_CONFIGUREPMUCURRENTLIMITRANGE = (PtrToDM482e_CONFIGUREPMUCURRENTLIMITRANGE)GetProcAddress(m_hmodule, "DM482e_ConfigurePMUCurrentLimitRange");
	if(pDM482e_CONFIGUREPMUCURRENTLIMITRANGE == NULL)
		return -43;

	pDM482e_GETPMUVOLTAGELEVELRANGE = (PtrToDM482e_GETPMUVOLTAGELEVELRANGE)GetProcAddress(m_hmodule, "DM482e_GetPMUVoltageLevelRange");
	if(pDM482e_GETPMUVOLTAGELEVELRANGE == NULL)
		return -44;

	pDM482e_GETPMUVOLTAGELIMITRANGE = (PtrToDM482e_GETPMUVOLTAGELIMITRANGE)GetProcAddress(m_hmodule, "DM482e_GetPMUVoltageLimitRange");
	if(pDM482e_GETPMUVOLTAGELIMITRANGE == NULL)
		return -45;

	pDM482e_GETPMUCURRENTLEVELRANGE = (PtrToDM482e_GETPMUCURRENTLEVELRANGE)GetProcAddress(m_hmodule, "DM482e_GetPMUCurrentLevelRange");
	if(pDM482e_GETPMUCURRENTLEVELRANGE == NULL)
		return -46;

	pDM482e_GETPMUCURRENTLIMITRANGE = (PtrToDM482e_GETPMUCURRENTLIMITRANGE)GetProcAddress(m_hmodule, "DM482e_GetPMUCurrentLimitRange");
	if(pDM482e_GETPMUCURRENTLIMITRANGE == NULL)
		return -47;

	pDM482e_CONFIGUREPMUOUTPUTFUNCTION = (PtrToDM482e_CONFIGUREPMUOUTPUTFUNCTION)GetProcAddress(m_hmodule, "DM482e_ConfigurePMUOutputFunction");
	if(pDM482e_CONFIGUREPMUOUTPUTFUNCTION == NULL)
		return -48;

	pDM482e_CONFIGUREPMUSAMPLINGTIME = (PtrToDM482e_CONFIGUREPMUSAMPLINGTIME)GetProcAddress(m_hmodule, "DM482e_ConfigurePMUSamplingTime");
	if(pDM482e_CONFIGUREPMUSAMPLINGTIME == NULL)
		return -49;

	pDM482e_CONFIGUREPOWERLINEFREQUENCY = (PtrToDM482e_CONFIGUREPOWERLINEFREQUENCY)GetProcAddress(m_hmodule, "DM482e_ConfigurePowerLineFrequency");
	if(pDM482e_CONFIGUREPOWERLINEFREQUENCY == NULL)
		return -50;

	pDM482e_CONFIGUREPMUSENSE = (PtrToDM482e_CONFIGUREPMUSENSE)GetProcAddress(m_hmodule, "DM482e_ConfigurePMUSense");
	if(pDM482e_CONFIGUREPMUSENSE == NULL)
		return -51;

	pDM482e_GETPMUSENSE = (PtrToDM482e_GETPMUSENSE)GetProcAddress(m_hmodule, "DM482e_GetPMUSense");
	if(pDM482e_GETPMUSENSE == NULL)
		return -52;

	pDM482e_PMUMEASURE = (PtrToDM482e_PMUMEASURE)GetProcAddress(m_hmodule, "DM482e_PMUMeasure");
	if(pDM482e_PMUMEASURE == NULL)
		return -53;

	// Trigger Related API
	pDM482e_ConfigureTriggerEdgeLevel = (PtrToDM482e_ConfigureTriggerEdgeLevel)GetProcAddress(m_hmodule, "DM482e_ConfigureTriggerEdgeLevel");
	if(pDM482e_ConfigureTriggerEdgeLevel == NULL)
		return -54;

	pDM482e_MapTriggerInToTriggerOut = (PtrToDM482e_MapTriggerInToTriggerOut)GetProcAddress(m_hmodule, "DM482e_MapTriggerInToTriggerOut");
	if(pDM482e_MapTriggerInToTriggerOut == NULL)
		return -55;

	pDM482e_ConfigureInputTriggerSelect = (PtrToDM482e_ConfigureInputTriggerSelect)GetProcAddress(m_hmodule, "DM482e_ConfigureInputTriggerSelect");
	if(pDM482e_ConfigureInputTriggerSelect == NULL)
		return -56;

	pDM482e_ConfigureOutputTriggerSelect = (PtrToDM482e_ConfigureOutputTriggerSelect)GetProcAddress(m_hmodule, "DM482e_ConfigureOutputTriggerSelect");
	if(pDM482e_ConfigureOutputTriggerSelect == NULL)
		return -57;

	pDM482e_DriveSoftwareTrigger = (PtrToDM482e_DriveSoftwareTrigger)GetProcAddress(m_hmodule, "DM482e_DriveSoftwareTrigger");
	if(pDM482e_DriveSoftwareTrigger == NULL)
		return -58;

	// MIPI Related API

	pDM482e_MIPI_Configure_Clock = (PtrToDM482e_MIPI_Configure_Clock)GetProcAddress(m_hmodule, "DM482e_MIPI_Configure_Clock");
	if(pDM482e_MIPI_Configure_Clock == NULL)
		return -59;

	pDM482e_MIPI_Connect = (PtrToDM482e_MIPI_Connect)GetProcAddress(m_hmodule, "DM482e_MIPI_Connect");
	if(pDM482e_MIPI_Connect == NULL)
		return -60;

	pDM482e_MIPI_RFFE_WR = (PtrToDM482e_MIPI_RFFE_WR)GetProcAddress(m_hmodule, "DM482e_MIPI_RFFE_WR");
	if(pDM482e_MIPI_RFFE_WR == NULL)
		return -61;

	pDM482e_MIPI_RFFE_RD = (PtrToDM482e_MIPI_RFFE_RD)GetProcAddress(m_hmodule, "DM482e_MIPI_RFFE_RD");
	if(pDM482e_MIPI_RFFE_RD == NULL)
		return -62;

	pDM482e_MIPI_RFFE_Retrieve = (PtrToDM482e_MIPI_RFFE_Retrieve)GetProcAddress(m_hmodule, "DM482e_MIPI_RFFE_Retrieve");
	if(pDM482e_MIPI_RFFE_Retrieve == NULL)
		return -63;

	// DIO Related API
	pDM482e_DrivePort = (PtrToDM482e_DrivePort)GetProcAddress(m_hmodule, "DM482e_DrivePort");
	if(pDM482e_DrivePort == NULL)
		return -64;

	pDM482e_DrivePin = (PtrToDM482e_DrivePin)GetProcAddress(m_hmodule, "DM482e_DrivePin");
	if(pDM482e_DrivePin == NULL)
		return -65;

	pDM482e_SetPortDirection = (PtrToDM482e_SetPortDirection)GetProcAddress(m_hmodule, "DM482e_SetPortDirection");
	if(pDM482e_SetPortDirection == NULL)
		return -66;

	pDM482e_SetPinDirection = (PtrToDM482e_SetPinDirection)GetProcAddress(m_hmodule, "DM482e_SetPinDirection");
	if(pDM482e_SetPinDirection == NULL)
		return -67;

	pDM482e_ReadPort = (PtrToDM482e_ReadPort)GetProcAddress(m_hmodule, "DM482e_ReadPort");
	if(pDM482e_ReadPort == NULL)
		return -68;

	pDM482e_ReadPin = (PtrToDM482e_ReadPin)GetProcAddress(m_hmodule, "DM482e_ReadPin");
	if(pDM482e_ReadPin == NULL)
		return -69;

	return 0;
}

// General Functions
ViStatus DM482e_DPINOpen(ViRsrc resourceName, ViInt32 dpingroup_sel, ViInt32 init_options, ViConstString optionString, ViSession* vi)
{
	return pDM482e_DPINOpen(resourceName, dpingroup_sel, init_options, optionString, vi);
}

ViStatus DM482e_DPINClose(ViSession vi)
{
	return pDM482e_DPINClose(vi);
}

ViStatus DM482e_DPINForce(ViSession vi, ViInt32 pinNo, ViInt32 state)
{
	return pDM482e_DPINForce(vi,pinNo,state);
}

ViStatus DM482e_ReadRevision(ViSession vi, ViChar* instrumentDriverRevision, ViChar* firmwareRevision)
{
	return pDM482e_ReadRevision(vi,instrumentDriverRevision,firmwareRevision);
}

ViStatus DM482e_ReadChannelTemperature(ViSession vi, ViInt32 pinNo, ViReal64* temperature)
{
	return pDM482e_ReadChannelTemperature(vi,pinNo,temperature);
}

ViStatus DM482e_ReadAmbientTemperature(ViSession vi, ViReal64* temperature)
{
	return pDM482e_ReadAmbientTemperature(vi,temperature);
}

ViStatus DM482e_ReadSerialNumber(ViSession vi, ViChar* sn)
{
	return pDM482e_ReadSerialNumber(vi, sn);
}

ViStatus DM482e_Reset(ViSession vi)
{
	return pDM482e_Reset(vi);
}

ViStatus DM482e_ResetGroup(ViSession vi, ViInt32 group)
{
	return pDM482e_ResetGroup(vi, group);
}

ViStatus DM482e_ConfigureMultiSiteMode(ViSession vi, ViInt32 mode)
{
	return pDM482e_ConfigureMultiSiteMode(vi, mode);
}

// Pin Electronics Related API
ViStatus DM482e_DPINVectorResourceAllocation(ViSession vi, ViInt32 vecSetCount, ViInt32* resourceArray)
{
	return pDM482e_DPINVectorResourceAllocation(vi, vecSetCount, resourceArray);
}

ViStatus DM482e_DPINLevel(ViSession vi, ViInt32 pinNo, ViReal64 VIH, ViReal64 VIL, ViReal64 VOH, ViReal64 VOL, ViReal64 IOH, ViReal64 IOL, ViReal64 VCH, ViReal64 VCL, ViReal64 VTERM)
{
	return pDM482e_DPINLevel(vi, pinNo, VIH, VIL, VOH, VOL, IOH, IOL, VCH, VCL, VTERM);
}

ViStatus DM482e_DPINVecLoad(ViSession vi, ViInt32 vecSetNo, char* vecFileName)
{
	return pDM482e_DPINVecLoad(vi, vecSetNo, vecFileName);
}

ViStatus DM482e_DPINPeriod(ViSession vi, ViInt32 timingSetNo, ViReal64 period_s)
{
	return pDM482e_DPINPeriod(vi, timingSetNo, period_s);
}

ViStatus DM482e_DPINOn(ViSession vi, ViInt32 pinNo)
{
	return pDM482e_DPINOn(vi, pinNo);
}

ViStatus DM482e_DPINOff(ViSession vi, ViInt32 pinNo)
{
	return pDM482e_DPINOff(vi, pinNo);
}

ViStatus DM482e_DPINHVOn(ViSession vi, ViInt32 pinNo)
{
	return pDM482e_DPINHVOn(vi, pinNo);
}

ViStatus DM482e_DPINHVOff(ViSession vi, ViInt32 pinNo)
{
	return pDM482e_DPINHVOff(vi, pinNo);
}

ViStatus DM482e_RunVector(ViSession vi, ViInt32 vecSetNo)
{
	return pDM482e_RunVector(vi, vecSetNo);
}

ViStatus DM482e_AcquireVecEngineStatus(ViSession vi, ViInt32* status)
{
	return pDM482e_AcquireVecEngineStatus(vi, status);
}

ViStatus DM482e_ReadHistoryRam(ViSession vi, ViInt32 vecSetNo, ViUInt32* history_ram_data)
{
	return pDM482e_ReadHistoryRam(vi, vecSetNo, history_ram_data);
}

ViStatus DM482e_ConfigurePEAttribute(ViSession vi, ViInt32 pinNo, ViBoolean inputTerminationEnable, ViBoolean HVEnable, ViBoolean activeLoadEnable, ViBoolean differentialComparatorEnable)
{
	return pDM482e_ConfigurePEAttribute(vi, pinNo, inputTerminationEnable, HVEnable, activeLoadEnable, differentialComparatorEnable);
}

ViStatus DM482e_ConfigureVectorEngineAttribute(ViSession vi, ViBoolean triggerEnable, ViBoolean continuousEnable)
{
	return pDM482e_ConfigureVectorEngineAttribute(vi, triggerEnable, continuousEnable);
}

ViStatus DM482e_ConfigureClockFrequency(ViSession vi, ViReal64 frequency)
{
	return pDM482e_ConfigureClockFrequency(vi, frequency);
}

ViStatus DM482e_ConfigureInputChannelDelay(ViSession vi, ViInt32 pinNo, ViReal64 delay)
{
	return pDM482e_ConfigureInputChannelDelay(vi, pinNo, delay);
}

ViStatus DM482e_AcquireVectorFailCount(ViSession vi, ViInt32* failCount)
{
	return pDM482e_AcquireVectorFailCount(vi, failCount);
}

ViStatus DM482e_AcquireChannelVectorFailCount(ViSession vi, ViInt32 pinNo, ViInt32* failCount)
{
	return pDM482e_AcquireChannelVectorFailCount(vi, pinNo, failCount);
}

ViStatus DM482e_AcquireChannelFirstFailVectorCount(ViSession vi, ViInt32 pinNo, ViInt32* failCount)
{
	return pDM482e_AcquireChannelFirstFailVectorCount(vi, pinNo, failCount);
}

// Pin Measurement Unit Related API
ViStatus DM482e_ConfigurePMUVoltageLevel(ViSession vi, ViInt32 pinNo, ViReal64 level)
{
	return pDM482e_ConfigurePMUVoltageLevel( vi,  pinNo,  level);
}

ViStatus DM482e_ConfigurePMUVoltageLimit(ViSession vi, ViInt32 pinNo, ViReal64 high_limit, ViReal64 low_limit)
{
	return pDM482e_ConfigurePMUVoltageLimit(vi, pinNo, high_limit,  low_limit);
}

ViStatus DM482e_ConfigurePMUCurrentLevel(ViSession vi, ViInt32 pinNo, ViReal64 level)
{
	return pDM482e_CONFIGUREPMUCURRENTLEVEL(vi, pinNo, level);
}

ViStatus DM482e_ConfigurePMUCurrentLevelAndRange(ViSession vi, ViInt32 pinNo, ViReal64 level, ViReal64 range)
{
	return pDM482e_CONFIGUREPMUCURRENTLEVELANDRANGE(vi, pinNo, level, range);
}

ViStatus DM482e_ConfigurePMUCurrentLimitRange(ViSession vi, ViInt32 pinNo, ViReal64 range)
{
	return pDM482e_CONFIGUREPMUCURRENTLIMITRANGE(vi, pinNo, range);
}

ViStatus DM482e_GetPMUVoltageLevelRange(ViSession vi, ViInt32 pinNo, ViReal64* range)
{
	return pDM482e_GETPMUVOLTAGELEVELRANGE(vi, pinNo, range);
}

ViStatus DM482e_GetPMUVoltageLimitRange(ViSession vi, ViInt32 pinNo, ViReal64* range)
{
	return pDM482e_GETPMUVOLTAGELIMITRANGE(vi, pinNo, range);
}

ViStatus DM482e_GetPMUCurrentLevelRange(ViSession vi, ViInt32 pinNo, ViReal64* range)
{
	return pDM482e_GETPMUCURRENTLEVELRANGE(vi, pinNo, range);
}

ViStatus DM482e_GetPMUCurrentLimitRange(ViSession vi, ViInt32 pinNo, ViReal64* range)
{
	return pDM482e_GETPMUCURRENTLIMITRANGE(vi, pinNo, range);
}

ViStatus DM482e_ConfigurePMUOutputFunction(ViSession vi, ViInt32 pinNo, ViInt32 function)
{
	return pDM482e_CONFIGUREPMUOUTPUTFUNCTION(vi, pinNo, function);
}

ViStatus DM482e_ConfigurePMUSamplingTime(ViSession vi, ViInt32 pinNo, ViReal64 samplingTime, ViInt32 units)
{
	return pDM482e_CONFIGUREPMUSAMPLINGTIME(vi, pinNo, samplingTime, units);
}

ViStatus DM482e_ConfigurePowerLineFrequency(ViSession vi, ViReal64 powerLineFrequency)
{
	return pDM482e_CONFIGUREPOWERLINEFREQUENCY(vi, powerLineFrequency);
}

ViStatus DM482e_ConfigurePMUSense(ViSession vi, ViInt32 pinNo, ViInt32 sense)
{
	return pDM482e_CONFIGUREPMUSENSE(vi, pinNo, sense);
}

ViStatus DM482e_GetPMUSense(ViSession vi, ViInt32 pinNo, ViInt32* sense)
{
	return pDM482e_GETPMUSENSE(vi, pinNo, sense);
}

ViStatus DM482e_PMUMeasure(ViSession vi, ViInt32 pinNo, ViInt32 measurementType, ViReal64* measurement)
{
	return pDM482e_PMUMEASURE(vi, pinNo, measurementType, measurement);
}

// Trigger Related API
ViStatus DM482e_ConfigureTriggerEdgeLevel(ViSession vi, ViInt32 triggerEnum, ViInt32 option)
{
	return pDM482e_ConfigureTriggerEdgeLevel(vi, triggerEnum, option);
}

ViStatus DM482e_MapTriggerInToTriggerOut(ViSession vi, ViInt32 inputTerminal, ViInt32 outputTerminal)
{
	return pDM482e_MapTriggerInToTriggerOut(vi, inputTerminal, outputTerminal);
}

ViStatus DM482e_ConfigureInputTriggerSelect(ViSession vi, ViInt32 triggerInput)
{
	return pDM482e_ConfigureInputTriggerSelect(vi, triggerInput);
}

ViStatus DM482e_ConfigureOutputTriggerSelect(ViSession vi, ViInt32 triggerOutput0, ViInt32 triggerOutput1)
{
	return pDM482e_ConfigureOutputTriggerSelect(vi, triggerOutput0, triggerOutput1);
}

ViStatus DM482e_DriveSoftwareTrigger(ViSession vi, ViReal64 pulseWidth)
{
	return pDM482e_DriveSoftwareTrigger(vi, pulseWidth);
}

// MIPI Related API
ViStatus DM482e_MIPI_Configure_Clock(ViSession vi, ViInt32 mipi_pair, ViInt32 freq_Hz)
{
	return pDM482e_MIPI_Configure_Clock(vi, mipi_pair, freq_Hz);
}

ViStatus DM482e_MIPI_Connect(ViSession vi, ViInt32 mipi_pair, ViInt32 setting)
{
	return pDM482e_MIPI_Connect(vi, mipi_pair, setting);
}

ViStatus DM482e_MIPI_RFFE_WR(ViSession vi, ViInt32 mipi_pair, ViInt32 Command, ViInt32* Data)
{
	return pDM482e_MIPI_RFFE_WR(vi, mipi_pair, Command, Data);
} 

ViStatus DM482e_MIPI_RFFE_RD(ViSession vi, ViInt32 mipi_pair, ViInt32 full_speed, ViInt32 Command, ViInt32* Data)
{
	return pDM482e_MIPI_RFFE_RD(vi, mipi_pair, full_speed, Command, Data);
} 

ViStatus DM482e_MIPI_RFFE_Retrieve(ViSession vi, ViInt32 mipi_pair, ViInt32* rd_byte_data_count, ViInt32* rd_data_array, ViInt32* parity_check_array)
{
	return pDM482e_MIPI_RFFE_Retrieve(vi, mipi_pair, rd_byte_data_count, rd_data_array, parity_check_array);
}

// DIO Related API
ViStatus DM482e_DrivePort(ViSession vi, ViInt32 value)
{
	return pDM482e_DrivePort(vi, value);
}

ViStatus DM482e_DrivePin(ViSession vi, ViInt32 pinNo, ViInt32 value)
{
	return pDM482e_DrivePin(vi, pinNo, value);
}

ViStatus DM482e_SetPortDirection(ViSession vi, ViInt32 value)
{
	return pDM482e_SetPortDirection(vi, value);
}

ViStatus DM482e_SetPinDirection(ViSession vi, ViInt32 pinNo, ViInt32 value)
{
	return pDM482e_SetPinDirection(vi, pinNo, value);
}

ViStatus DM482e_ReadPort(ViSession vi, ViInt32* value)
{
	return pDM482e_ReadPort(vi, value);
}

ViStatus DM482e_ReadPin(ViSession vi, ViInt32 pinNo, ViInt32* value)
{
	return pDM482e_ReadPin(vi, pinNo, value);
}

}
