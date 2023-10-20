using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.Windows.Forms; 

using Avago.ATF.Shares;
using Avago.ATF.CrossDomainAccess;
using Avago.ATF.ScriptParser;
using Avago.ATF.IOLibrary;
using Avago.ATF.StandardLibrary;
using Avago.ATF.DataProcessEngine;


namespace Avago.ATF.LightweightDriver
{
    public class TestPlanLightweightRunner
    {
        public string ResultFileName { get; private set; }
        public string BuddyFileName { get; private set; }
        public string TraceFileName { get; private set; }

        private int m_currentSN = 0;

        private Dictionary<string, string> TestPlanProperties;
        private Dictionary<string, HWUnit> HWConfig;
        private Dictionary<string, PublishedParameter> TestParameters;
        private TestLimitDefinition TestLimitDef = new TestLimitDefinition();
        private CorrelationDefinition CorrleationDef;
        private bool TestLimitAndCorrelationAvail = false;

        private string m_testplanfilePath;
        private IATFTest m_TestPlanInstance = null;

        private IATFAdaptiveSampling m_ruleInstance = null;
        private string m_ruleFilePath = "";


        public TestPlanLightweightRunner()
        {
            ResultFileName = "";
            BuddyFileName = "";
            TraceFileName = ""; 
        }


        #region INIT

        public string Init(IATFTest testplan_instance, string initArg, string testplanfilePath, IATFAdaptiveSampling rule_instance, string rulefilepath, bool isBuddyFileEnabled, bool isTraceFileEnabled = false)
        {
            #region Init for Test Plan Instance

            m_testplanfilePath = testplanfilePath;
            m_TestPlanInstance = testplan_instance;

            // Init the result file name 

            string timestr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            ResultFileName = string.Format("LiteDriver_{0}_{1}.csv", Path.GetFileNameWithoutExtension(m_testplanfilePath), timestr);
            if (isBuddyFileEnabled)
            {
                BuddyFileName = string.Format("LiteDriver_{0}_{1}{2}", Path.GetFileNameWithoutExtension(m_testplanfilePath), timestr, ATFMiscConstants.TagBuddyFilePostfix);
                ATFSharedData.Instance.IsBuddyFileOn = true; 
            }
            else
            {
                ATFSharedData.Instance.IsBuddyFileOn = false; 
            }

            if (isTraceFileEnabled)
            {
                // In TestPlanTemplate, no header rows, no zip, use special type to avoid contaiminating zDB collection
                TraceFileName = string.Format("LiteDriver_{0}_{1}.tracetxt", Path.GetFileNameWithoutExtension(m_testplanfilePath), timestr);

                // For LiteDriver testing, ALWAYS collect trace data for each DUT
                ATFSharedData.Instance.NextDUTNeedCollectTraceData = true; 
            }
            else
            {
                // Forever false
                ATFSharedData.Instance.NextDUTNeedCollectTraceData = false;
            }

            m_currentSN = 0;

            ATFSharedData.Instance.ResetTestPlanConfig();

            Tuple<bool, string> ret = TryToParseSourceCode(m_testplanfilePath); 
            if(!ret.Item1)
                return TestPlanRunConstants.RunFailureFlag + ret.Item2;

            if (TestLimitAndCorrelationAvail)
            {
                ret = TryToLoadCorrAndLimit(); 
                if(!ret.Item1)
                    return TestPlanRunConstants.RunFailureFlag + ret.Item2;
            }

            ret = TryToActiveBuddyExcel(); 
            if(!ret.Item1)
                return TestPlanRunConstants.RunFailureFlag + ret.Item2;

            ATFSharedData.Instance.HWConfig = HWConfig;

            // After loading completed
            ATFRTE.Instance.StatOnline.UpdateAfterTestPlanLoad(TestLimitDef);


            #endregion


            #region Rule Engine Instance

            m_ruleFilePath = rulefilepath;
            m_ruleInstance = rule_instance;

            ATFSharedData.Instance.ResetAdaptiveSamplingRuleConfig();

            #endregion


            // Finally, run the INIT method for Test Plan Instance, nothing needed by Rule Instance 
            string initRet = m_TestPlanInstance.DoATFInit(initArg);
            return initRet;
        }


        /// <summary>
        /// This will be called when R&D mode, so the ProductionMode is "false" 
        /// </summary>
        /// <param name="testplanfilePath"></param>
        /// <returns></returns>
        private Tuple< bool, string> TryToParseSourceCode(string testplanfilePath)
        {
            string info = "";

            // Parse 
            IScriptParser parseCSharp = ScriptParserFactory.Create(Avago.ATF.Shares.ScriptType.CSharpTestPlan);
            try
            {
                parseCSharp.ParseFile(testplanfilePath, false);
            }
            catch (Exception ex)
            {
                info = "TestPlan .cs Parsing Failure: " + ex.Message;
                Trace.WriteLine(info);                 
                return new Tuple<bool, string>(false, info); 
            }

            TestPlanProperties = parseCSharp.PropetyEntries;
            HWConfig = parseCSharp.HWUnitDefs;
            TestParameters = parseCSharp.PublishedParameters;
            TestLimitAndCorrelationAvail = parseCSharp.TestLimitAndCorrelationAvail;

            return new Tuple<bool, string>(true, ""); 
        }


        private Tuple< bool, string> TryToLoadCorrAndLimit()
        {
            string info = "";

            string devTLFileFullPath = ATFRTE.Instance.TestPlanRootFolder + @"\" + ATFRTE.Instance.CurPackageTag + @"\" + TestPlanProperties[TestPlanContentConstants.TagParseTestLimitsItem];
            string devCFileFullPath = ATFMiscConstants.ClothoLocalCorrelationPathDevelop + TestPlanProperties[TestPlanContentConstants.TagParseCorrelationItem];


            // Load TestLimit Buddy
            try
            {
                TestResultHeader header = new TestResultHeader();

                if (!TestLimitsFileLoader.LoadTestLimitsFromCSVFile(
                    devTLFileFullPath,
                    ref TestLimitDef,
                    ref header,
                    ref info))
                {
                    info = string.Format("Fail to Load Buddy TestLimits '{0}' Fail '{1}'", devTLFileFullPath, info);
                    Trace.WriteLine(info);
                    return new Tuple<bool, string>(false, info); 
                }

                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PACKAGE_TL_FULLPATH, devTLFileFullPath);

                if (info.Length > 0)
                {
                    info = string.Format("Load Buddy TestLimits '{0}' Go Through but with WARNINGS '{1}'", devTLFileFullPath, info);
                    Trace.WriteLine(info); 
                }
            }
            catch (Exception ex)
            {
                info = string.Format("Fail to Load Buddy TestLimits '{0}': '{1}'", devTLFileFullPath, ex.Message);

                Trace.WriteLine(info);
                return new Tuple<bool, string>(false, info); 
            }

            Trace.WriteLine("BuddyTestLimits Loading Succeed");


            // this is dummy here, since R&D mode never need IccGuCal
            // Just to meet API requirement
            try
            {
                CorrleationDef = CorrelationFileLoaderV3.LoadCorrelationFromCSVFile(
                    devCFileFullPath,
                    ref info);
                if (CorrleationDef == null)
                {
                    info = string.Format("Fail to Load Buddy Correlation '{0}': {1}", devCFileFullPath, info);

                    Trace.WriteLine(info);
                    return new Tuple<bool, string>(false, info); 
                }

                ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, devCFileFullPath);

            }
            catch (Exception ex)
            {
                info = string.Format("Fail to Load Buddy Correlation '{0}': '{1}'", devCFileFullPath, ex.Message);

                Trace.WriteLine(info);
                return new Tuple<bool, string>(false, info);
            }



            // Compare Correlation and TestLimits "TestParameter" Name list must be same 
            List<string> tpNameTestLimit = TestLimitDef.CompleteParameterList.Keys.ToList<string>();
            if (!CorrleationDef.Body.Keys.SequenceEqual(tpNameTestLimit))
            {
                info = "Mismatch Parameter List of TestLimit File and Correlation File";

                Trace.WriteLine(info);
                return new Tuple<bool, string>(false, info);
            }

            ATFSharedData.Instance.CorData = CorrleationDef;
            ATFSharedData.Instance.TestLimitData = TestLimitDef;

            return new Tuple<bool, string>(true, ""); 
        }



        private Tuple<bool, string> TryToActiveBuddyExcel()
        {
            string info = "";

            if (TestPlanProperties.ContainsKey(TestPlanContentConstants.TagParseExcelItem))
            {
                string devTCFFileFullPath = ATFRTE.Instance.TestPlanRootFolder + @"\" + ATFRTE.Instance.CurPackageTag + @"\" + TestPlanProperties[TestPlanContentConstants.TagParseExcelItem];
                try
                {
                    ATFExcel.Instance.LoadExcel(devTCFFileFullPath, (TestPlanProperties[TestPlanContentConstants.TagParseExcelDisplay] == "1"));
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, devTCFFileFullPath);
                }
                catch (Exception ex)
                {
                    info = string.Format("Fail to Activate Buddy Excel '{0}'. Exception '{1}'", devTCFFileFullPath, ex.Message);
                    Trace.WriteLine(info);
                    return new Tuple<bool, string>(false, info); 
                }
            }

            return new Tuple<bool, string>(true, ""); 
        }

        #endregion


        #region TEST


        public ATFReturnResult Test(string testArg, bool adaptiveSamplingIsOn, ref string ruleRet, bool is1stTime)
        {
            ATFReturnResult testRet = null;
            try
            {
                testRet = m_TestPlanInstance.DoATFTest(testArg);
                // no need to processed these failure string 
                if (testRet.Err != "")
                {
                    Trace.WriteLine("Error from run: " + testRet.Err);
                    return testRet;
                }
                else if (testRet.Data.Count < 1)
                {
                    Trace.WriteLine("Run return no parameters");
                    return testRet;
                }
            }
            catch (Exception ex)
            {
                // Exception failures 
                Trace.WriteLine("DoATFTest Exception: " + ex.Message);
                return null;
            }


            #region Write into raw result file file

            if (m_currentSN == 0)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sbheader = new StringBuilder();

                foreach (ATFReturnPararResult item in testRet.Data)
                {
                    sbheader.AppendFormat("{0},", item.Name);
                    sb.AppendFormat("{0},", item.ToValString());
                }

                // Open, Write to result file, then Close
                // With minimum 100ms delay, that's enough

                using (StreamWriter sw = new StreamWriter(ResultFileName, true))
                {
                    sw.WriteLine(string.Format("ParameterName,{0}", sbheader.ToString().TrimEnd(',')));
                    sw.WriteLine(string.Format("PID-{0},{1}", m_currentSN+1, sb.ToString().TrimEnd(',')));
                    sw.Close();
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (ATFReturnPararResult item in testRet.Data)
                {
                    sb.AppendFormat("{0},", item.ToValString());
                }

                // Open, Write to result file, then Close
                // With minimum 100ms delay, that's enough
                using (StreamWriter sw = new StreamWriter(ResultFileName, true))
                {
                    sw.WriteLine(string.Format("PID-{0},{1}", m_currentSN + 1, sb.ToString().TrimEnd(',')));
                    sw.Close();
                }
            }


            if ( BuddyFileName.Length > 10)
            {
                using (StreamWriter swBuddy = new StreamWriter(BuddyFileName, true))
                {
                    if (m_currentSN == 0)
                    {
                        // Only once
                        // Write middle section 
                        List<string> lines = (ATFCrossDomainWrapper.GetObjectFromCache(PublishTags.PUBTAG_BUDDY_BEFORE_PARAMETER_LINES, null) as List<string>);
                        if ((lines != null) && (lines.Count > 0))
                        {
                            foreach (string mid_row in lines)
                                // To make them as "comments" in Galaxy world
                                swBuddy.WriteLine("#" + mid_row);
                        }
                        ATFCrossDomainWrapper.StoreObjectToCache(PublishTags.PUBTAG_BUDDY_BEFORE_PARAMETER_LINES, new List<string>());

                        /* For 2nd result file, write middle "Parameter" row */
                        swBuddy.WriteLine("Parameter," + ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_BUDDY_PARAMETER_LINE, ""));
                        ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_BUDDY_PARAMETER_LINE, "");
                    }

                    string buddyline = string.Format("{0}{1},{2}", TestResultFileConstants.TagSNPrefix, m_currentSN + 1, ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_BUDDY_COMMENT, ""));
                    swBuddy.WriteLine(buddyline);
                    ATFCrossDomainWrapper.StoreStringToCache(PublishTags.PUBTAG_BUDDY_COMMENT, "");

                    swBuddy.Close();
                }
            }

            if (TraceFileName.Length > 10)
            {
                using (StreamWriter swTrace = new StreamWriter(string.Format("{0}_PID-{1}.tracetxt",  Path.GetFileNameWithoutExtension(TraceFileName), m_currentSN), true))
                {
                    // Single site only
                    int siteIndex = 1; 
                    List<string> tracedata = ATFSharedData.Instance.GetTraceDataThenResetBySite(siteIndex);
                    if (tracedata.Count > 0)
                    {
                        swTrace.WriteLine(); 
                        swTrace.WriteLine(string.Format("--------------- DUT: {0}{1}", TestResultFileConstants.TagSNPrefix, m_currentSN + 1));
                        foreach (string item in tracedata)
                        {
                            swTrace.WriteLine(item); 
                        }
                    }
                    swTrace.Close();
                }
            }

            m_currentSN++; 

            #endregion

            return testRet;
        }



        #endregion


        #region Un-INIT

        public string UnInit(string uninitArg)
        {
            string uninitRet = m_TestPlanInstance.DoATFUnInit(uninitArg);

            Tuple<bool, string> unloadRet = TryToUnloadBuddyExcel();
            if (!unloadRet.Item1)
                return TestPlanRunConstants.RunFailureFlag + unloadRet.Item2; 

            ATFSharedData.Instance.ResetTestPlanConfig();
                        
            return uninitRet;
        }

        public string CloseLot(string closeLotArg)
        {
            return m_TestPlanInstance.DoATFLot(closeLotArg);
        }


        private Tuple<bool, string> TryToUnloadBuddyExcel()
        {
            string info = "";

            if (TestPlanProperties.ContainsKey(TestPlanContentConstants.TagParseExcelItem))
            {
                Tuple<bool, string> ret = ATFExcel.Instance.UnloadExcel();
                if (!ret.Item1)
                {
                    info = string.Format("Unload Buddy Excel '{0}' Exception '{1}'", TestPlanProperties[TestPlanContentConstants.TagParseExcelItem], ret.Item2);
                    Trace.WriteLine(info);
                    return new Tuple<bool, string>(false, info); 
                }
                else if (ret.Item2 != string.Empty)
                {
                    info = string.Format("Unload Buddy Excel '{0}' Non-Fatal Logic Error '{1}'", TestPlanProperties[TestPlanContentConstants.TagParseExcelItem], ret.Item2);

                    Trace.WriteLine(info); 
                }
                else
                {
                    Trace.WriteLine(string.Format("Unload Buddy Excel '{0}' Succeed", TestPlanProperties[TestPlanContentConstants.TagParseExcelItem]));
                }
            }

            return new Tuple<bool, string>(true, ""); 
        }

        #endregion

    }
}
