using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cMapReader
{
    public struct s_Map
    {
        public string XMLtype;
        public string SubstrateType;
        public string SubstrateID;
        public string CarrierType;
        public string FormatRevision;
        public s_Device DeviceInfo;
        public int[,] Data;
        public string[] RawData;
        public s_RascoAddInfo RowInfo;
    }
    public struct s_RascoAddInfo
    {
        public int Map_Row;
        public int Device_Row;
        public int Bin_Row;
        public int Data_Row;
        public int DataRow_Row;
    }
    public struct s_Device
    {
        public string ProductID;
        public string LotID;
        public int Orientation;
        public int Rows;
        public int Columns;
        public string BinType;
        public int NullBin;
        public int OriginLocation;
        public int HandlerLowerLeftOriginLocation;
        public s_BinData[] BinData;
    }
    public struct s_BinData
    {
        public int BinCode;
        public string BinQuality;
    }
    public class cRascoMap
    {
        public string Map_Filename { get; set; }
        public s_Map MapData;
        public cXMLParser.cXMLParser XMLparse = new cXMLParser.cXMLParser();

        private const string sMap = "/Map";
        private const string sDevice = sMap + "/Device";
        private const string sBin = sDevice + "/Bin";
        private const string sData = sDevice + "/Data/Row";

        public bool Read_Map()
        {
            string[] tmpStr;
            if (Map_Filename == null || Map_Filename == "")
            {
                return false;
            }
            try
            {

                XMLparse.XMLFileName = Map_Filename;
                XMLparse.InitXML();

                MapData.RawData = System.IO.File.ReadAllLines(Map_Filename);

                {
                    
                    for (int iRow = 0; iRow < MapData.RawData.Length; iRow++)
                    {
                        tmpStr = MapData.RawData[iRow].Trim().ToUpper().Split(' ');
                        switch (tmpStr[0])
                        {
                            case "<MAP":
                                if (MapData.RowInfo.Map_Row == 0)
                                {
                                    MapData.RowInfo.Map_Row = iRow;
                                }
                                break;
                            case "<DEVICE":
                                if (MapData.RowInfo.Device_Row == 0)
                                {
                                    MapData.RowInfo.Device_Row = iRow;
                                }
                                break;
                            case "<BIN":
                                if (MapData.RowInfo.Bin_Row == 0)
                                {
                                    MapData.RowInfo.Bin_Row = iRow;
                                }
                                break;
                            case "<DATA>":
                                if (MapData.RowInfo.Data_Row == 0)
                                {
                                    MapData.RowInfo.Data_Row = iRow;
                                }
                                break;
                            default:
                                if(tmpStr[0].Contains("<ROW>"))
                                {
                                    if (MapData.RowInfo.DataRow_Row == 0)
                                    {
                                        MapData.RowInfo.DataRow_Row = iRow;
                                    }
                                }
                                break;
                        }
                    }
                }

                MapData.XMLtype = XMLparse.GetAttribute(sMap, "xmlns:semi");
                MapData.SubstrateType = XMLparse.GetAttribute(sMap, "SubstrateType");
                MapData.SubstrateID = XMLparse.GetAttribute(sMap, "SubstrateId");
                MapData.CarrierType = XMLparse.GetAttribute(sMap, "CarrierType");
                MapData.FormatRevision = XMLparse.GetAttribute(sMap, "FormatRevision");

                MapData.DeviceInfo.ProductID = XMLparse.GetAttribute(sDevice, "ProductId");
                MapData.DeviceInfo.LotID = XMLparse.GetAttribute(sDevice, "LotID");
                MapData.DeviceInfo.Orientation = Convert.ToInt32(XMLparse.GetAttribute(sDevice, "Orientation"));
                MapData.DeviceInfo.Rows = Convert.ToInt32(XMLparse.GetAttribute(sDevice, "Rows"));
                MapData.DeviceInfo.Columns = Convert.ToInt32(XMLparse.GetAttribute(sDevice, "Columns"));
                MapData.DeviceInfo.BinType = XMLparse.GetAttribute(sDevice, "BinType");
                MapData.DeviceInfo.NullBin = Convert.ToInt32(XMLparse.GetAttribute(sDevice, "NullBin"));
                MapData.DeviceInfo.OriginLocation = Convert.ToInt32(XMLparse.GetAttribute(sDevice, "OriginLocation"));
                MapData.DeviceInfo.HandlerLowerLeftOriginLocation = Convert.ToInt32(XMLparse.GetAttribute(sDevice, "HandlerLowerLeftOriginLocation"));

                int BinCount = Convert.ToInt32(XMLparse.GetNodeCount(sBin));
                MapData.DeviceInfo.BinData = new s_BinData[BinCount];
                for (int iCnt = 0; iCnt < BinCount; iCnt++)
                {
                    MapData.DeviceInfo.BinData[iCnt].BinCode = Convert.ToInt32(XMLparse.GetNodeInfo(sBin, iCnt, "BinCode"));
                    MapData.DeviceInfo.BinData[iCnt].BinQuality = XMLparse.GetNodeInfo(sBin, iCnt, "BinQuality");
                }

                MapData.Data = new int[MapData.DeviceInfo.Rows, MapData.DeviceInfo.Columns];
                for (int iData = 0; iData < XMLparse.GetNodeCount(sData); iData++)
                {
                    tmpStr = XMLparse.GetInnerText(sData, iData).Split(' ');
                    for (int iCol = 0; iCol < tmpStr.Length; iCol++)
                    {
                        MapData.Data[iData, iCol] = Convert.ToInt32(tmpStr[iCol]);

                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
            
        }

    }
}