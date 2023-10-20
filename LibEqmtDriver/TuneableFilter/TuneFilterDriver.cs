using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEqmtDriver.TuneableFilter
{
    public interface iTuneFilterDriver
    {
        void Initialize();
        void Reset();
        void SetFreqMHz(double freqMHz);
        double ReadFreqMHz();
    }
}
