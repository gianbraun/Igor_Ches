using System.ComponentModel;

namespace IGOR.Interface.Communication.Axis
{
    public interface IAxisPos : INotifyPropertyChanged
    {
        bool ContiniusRefresh { get; set; }
        double ActualPos { get; }
        double TargetPos { get; }

        event PropertyChangedEventHandler ContiniusRefreshChanged;
    }
}
