using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cGeneral2
{
    public class cGeneral2
    {
        public bool cStr2Bool(string inputStr)
        {
            if ((inputStr.Trim() == "1") || (inputStr.ToUpper().Trim() == "YES") || (inputStr.ToUpper().Trim() == "V"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
