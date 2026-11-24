using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IGOR.Interface.Communication.Axis
{
    public interface IAxisParam : INotifyPropertyChanged
    {
        double HandSlow { get; }
        double HandFast { get; }
        bool SoftlimitNegAktive { get; }
        bool SoftlimitPosAktive { get; }
        double MinPos { get; }
        double MaxPos { get; }
    }
}
