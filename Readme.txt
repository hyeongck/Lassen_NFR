Rev2.75 - 23/11/2017
1) Fix small bug in BURN_OTP_JEDI2 function . Both UNIT_ID and MFG_ID cannot burn 255 (0xFF) because of some logic problem if (dataDec_Conv < (Convert.ToInt32(dataSizeHex, 16))) ---> change to if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
2) Added cases for CM_ID in the BURN_OTP_JEDI2

Rev0.02 - 28/11/2017
1) Uprev the current Rev2.75 (FBAR NOISE TEST) to incorporate the NF RISE TEST that was develop by Seoul Team
2) This version are use for JEDI2 to include both TESTING (FBAR NOISE & NF RISE)

Rev1.00 - 28/03/2018
1) Update VST driver code to support VST 5644R for NF Rise only (note : changes will not work for convenstional FBAR Noise Test)
2) Update Calibration VST driver code to support VST5644 .. Will effect all pathloss calibration (Note : Will work for both VST)
3) Yoon Chun updated the code to include multiple sweep for NF rise

Rev1.01 - 03/04/2018
1) Update myDUT.cs - encounter coding bug when using no power tuning option for FBAR Noise 
Code never return R_Pin1 when select no tuning power tuning option in TCF
Added R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3)

Rev1.02 - 05/06/2018
1) update myDUT.cs to change the TestTime build result to include the test number after _TestTimeXX

Rev1.03 - 07/06/2018
1) update myDUT.cs in the OTP_JEDI2 function , to include initialization of this parameter before testing
dataBinary = new string[2];
appendBinary = null;
dataDec = new int[2];

Rev1.04 - 26 September 2018
1) Modified MyUtility.CS function ReadTCF to make the loading time during init will be much faster
Previously using ATFCrossDomainWrapper.Excel_Get_Input toread excel file line by line
Change to ATFCrossDomainWrapper.Excel_Get_IputRangeByValue to read excel file in one go
also change the selection of excel sheet from TCF_Sheet.ConstPASheetNo to TCF_Sheet.ConstPASheetName (added in MyDUT_Variable.cs)

2) Add a new test method for Customize Read the CMOS OTP register  
This Function in myDUT.cs (case : "READ_OTP_SELECTIVE_BIT") is use to read any register with customize bit selection reading
Mostly will be use in Module_ID register where PE already define that need to read only 14bits of data from 2 register sets (instead of 16bit)


3) Add a new test method for Customize Burn the CMOS OTP register  
This Function in myDUT.cs (case : "BURN_OTP_SELECTIVE_BIT") is use to burn and read any register with customize bit selection reading
Additional check on Lockbit/Testflag register are define in "Search_Method" and "Search_Value" column in Test Codtion Sheet
Example: Module ID Burn - "Search_Method = UNIT_ID" and "Search_Value = E3:01" where Search_value is register address of Noise Test Flag @ bit 0
This data will be compared , if '0' than never burn Noise Flag will proceed with Burn The Module ID 

Explaination :-
Note : effective bit are selected if any of the bit is set to '1' in CusMipiRegMap data column (in hex format >> register:effective bits) => 42:03 (regAdd:regDataBits)
Example CusMipiRegMap must be in '42:03 43:FF' where 0x43 is LSB reg address and 0xFF (1111 1111) all 8 bits are to be effectively read
and 0x42 is MSB reg address and 0x03 (0000 0011) where on bit0 and bit1 are to be effectively read
                            
After MIPI read =>LSB  reg 0x43 = data read 0xCB (11001011) &  MSB reg 0x42 = data read 0x2E (00101110) 
Effective bit decode example => for reg 0x42 since all 8bits will be use , effectiveBitData(0xCB) = 11001011 
while reg 0x43 only bit0 and bit1 will be taken (*note: shown in bracket) , effectiveBitData(0x2E) = 001011 (10)

Test Report will be generate base on this data only >>> binary 10 11001011 => convert to dec = 715

Rev1.05 - 18 October 2018
1) Uprev NI6570_Rev3.cs to NI6570_Rev4.cs. This uprev included the capability to read ''Éxtended Long'' register address which is required when reading the Seoraksan 1.7 LNA OTP for Wafer ID/LOT/X-Y
''Extended Long" represent 0xFFFF address range , previous code only supported "Extended'' address 0xFF . Note : this class is backward compatible with all mipi function in MyDUT.cs

2) Add CM_ID burn case in (case : "BURN_OTP_SELECTIVE_BIT") 


Rev1.06 - 13 November 2018
1) Change NPLC setting for aemulus in in MyDUT.cs (in testcase of "SMU") from float _NPLC = 0.001f to float _NPLC = 1f
This is to make the aemulus result during leakage current similar trend to NI SMU
Measurement bandwidth still remain normal

Rev1.07 - 14 November 2018
1) Added additional Handler ID for ASEKr in FrmDataInput.cs

