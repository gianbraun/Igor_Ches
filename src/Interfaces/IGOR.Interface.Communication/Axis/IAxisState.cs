using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace IGOR.Interface.Communication.Axis
{
    public interface IAxisState : INotifyPropertyChanged
    {
        bool ContiniusRefresh { get; set; }
        bool Ready { get; }
        bool MoveFW { get; }
        bool MoveBW { get; }
        bool NotMoving { get; }
        bool HasJob { get; }
        bool Referenzed { get; }
        bool InDestinationPos { get; }
        uint ErrorCode { get; }

        event PropertyChangedEventHandler ContiniusRefreshChanged;
    }
}
