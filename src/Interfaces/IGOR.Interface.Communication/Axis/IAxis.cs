using System.ComponentModel;

namespace IGOR.Interface.Communication.Axis
{
    public interface IAxis : INotifyPropertyChanged
    {
        string Name { get; }

        uint Id { get; }

        IAxisState State { get; }

        IAxisPos Pos { get; }

        IAxisRelease Release { get; }

        IAxisParam Params { get; }

        void Move(double destination, double speed, PositioningType positioningType);

        void Stop();
    }
}
