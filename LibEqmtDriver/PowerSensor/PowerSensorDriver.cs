using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEqmtDriver.PS
{
    public interface iPowerSensor
    {
        void Initialize(int ch);
        void Reset();
        void SetOffset(int ch, double val);
        void EnableOffset(int ch, bool status);
        void SetFreq(int ch, double val);
        float MeasPwr(int ch);
    }
}
