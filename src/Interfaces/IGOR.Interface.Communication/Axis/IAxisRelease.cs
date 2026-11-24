using System.ComponentModel;
using System.Runtime.InteropServices;

namespace IGOR.Interface.Communication.Axis
{
    public interface IAxisRelease : INotifyPropertyChanged
    {
        bool ContiniusRefresh { get; set; }
        bool ControllerRelease { get; }
        bool FeedFW { get; }
        bool FeedBW { get; }
        double Override { get; }

        event PropertyChangedEventHandler ContiniusRefreshChanged;
    }
}
