using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEqmtDriver.SCU
{
    public interface iSwitch
    {
        void Initialize();
        void SetPath(string val);
        void Reset();
    }
    
}
