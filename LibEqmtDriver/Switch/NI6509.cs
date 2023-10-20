using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.DAQmx;

namespace LibEqmtDriver.SCU
{
    public class NI6509 : iSwitch 
    {
        private static ArrayList TaskList;

        private static Task digitalWriteTaskP00;
        private static Task digitalWriteTaskP01;
        private static Task digitalWriteTaskP02;
        private static Task digitalWriteTaskP03;
        private static Task digitalWriteTaskP04;
        private static Task digitalWriteTaskP05;

        private static Task digitalWriteTaskP09;
        private static Task digitalWriteTaskP10;
        private static Task digitalWriteTaskP11;

        private static DigitalSingleChannelWriter writerP00;
        private static DigitalSingleChannelWriter writerP01;
        private static DigitalSingleChannelWriter writerP02;
        private static DigitalSingleChannelWriter writerP03;
        private static DigitalSingleChannelWriter writerP04;
        private static DigitalSingleChannelWriter writerP05;

        private static DigitalSingleChannelWriter writerP09;
        private static DigitalSingleChannelWriter writerP10;
        private static DigitalSingleChannelWriter writerP11;

        public string IOAddress;

        /// <summary>
        /// Parsing Equpment Address
        /// </summary>
        public string Address
        {
            get
            {
                return IOAddress;
            }
            set
            {
                IOAddress = value;
            }
        }
        //Constructor
        public NI6509(string ioAddress)
        {
            Address = ioAddress;
            Initialize();
        }
        NI6509() { }


        #region iSwitch Interface

        public void Initialize()
        {
            try
            {
                digitalWriteTaskP00 = new Task();
                digitalWriteTaskP01 = new Task();
                digitalWriteTaskP02 = new Task();
                digitalWriteTaskP03 = new Task();
                digitalWriteTaskP04 = new Task();
                digitalWriteTaskP05 = new Task();

                digitalWriteTaskP09 = new Task();
                digitalWriteTaskP10 = new Task();
                digitalWriteTaskP11 = new Task();

                digitalWriteTaskP00.DOChannels.CreateChannel(IOAddress + "/port0", "port0",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP01.DOChannels.CreateChannel(IOAddress + "/port1", "port1",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP02.DOChannels.CreateChannel(IOAddress + "/port2", "port2",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP03.DOChannels.CreateChannel(IOAddress + "/port3", "port3",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP04.DOChannels.CreateChannel(IOAddress + "/port4", "port4",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP05.DOChannels.CreateChannel(IOAddress + "/port5", "port5",
                                ChannelLineGrouping.OneChannelForAllLines);

                digitalWriteTaskP09.DOChannels.CreateChannel(IOAddress + "/port9", "port9",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP10.DOChannels.CreateChannel(IOAddress + "/port10", "port10",
                                ChannelLineGrouping.OneChannelForAllLines);
                digitalWriteTaskP11.DOChannels.CreateChannel(IOAddress + "/port11", "port11",
                                ChannelLineGrouping.OneChannelForAllLines);

                writerP00 = new DigitalSingleChannelWriter(digitalWriteTaskP00.Stream);
                writerP01 = new DigitalSingleChannelWriter(digitalWriteTaskP01.Stream);
                writerP02 = new DigitalSingleChannelWriter(digitalWriteTaskP02.Stream);
                writerP03 = new DigitalSingleChannelWriter(digitalWriteTaskP03.Stream);
                writerP04 = new DigitalSingleChannelWriter(digitalWriteTaskP04.Stream);
                writerP05 = new DigitalSingleChannelWriter(digitalWriteTaskP05.Stream);

                writerP09 = new DigitalSingleChannelWriter(digitalWriteTaskP09.Stream);
                writerP10 = new DigitalSingleChannelWriter(digitalWriteTaskP10.Stream);
                writerP11 = new DigitalSingleChannelWriter(digitalWriteTaskP11.Stream);

                writerP00.WriteSingleSamplePort(true, 0);
                writerP01.WriteSingleSamplePort(true, 0);
                writerP02.WriteSingleSamplePort(true, 0);
                writerP03.WriteSingleSamplePort(true, 0);
                writerP04.WriteSingleSamplePort(true, 0);
                writerP05.WriteSingleSamplePort(true, 0);
                writerP09.WriteSingleSamplePort(true, 0);
                writerP10.WriteSingleSamplePort(true, 0);
                writerP11.WriteSingleSamplePort(true, 0);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Initialize");
            }
        }

        public void SetPath(string val)
        {
            string[] tempdata;
            tempdata = val.Split(';');
            string[] tempdata2;
            
            try
            {
                for (int i = 0; i < tempdata.Length; i++)
                {
                    tempdata2 = tempdata[i].Split('_');
                    
                    switch(tempdata2[0].ToUpper())
                    {
                        case "P0":
                            writerP00.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        case "P1":
                            writerP01.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        case "P2":
                            writerP02.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        case "P3":
                            writerP03.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        case "P4":
                            writerP04.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        case "P5":
                            writerP05.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        case "P9":
                            writerP09.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        case "P10":
                            writerP10.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        case "P11":
                            writerP11.WriteSingleSamplePort(true, Convert.ToUInt32(tempdata2[1]));
                            break;
                        default :
                            MessageBox.Show("Port No : " + tempdata2[1].ToUpper(), "Only P0,P1,P2,P3,P4,P5 AND P9,P10,P11 ALLOWED !!!!\n" + "Pls check your switching configuration in Input Folder");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("NI6509 DIO : SetPath -> " + ex.Message);
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
