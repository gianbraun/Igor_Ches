using IGOR.Interface.Communication.Axis;
using System.ComponentModel;

namespace IGOR.Service.Communication.TwinCat.Axis
{
    public class TcAxisPos : IAxisPos
    {
        public double ActualPos { get; set; }
        public double TargetPos { get; set; }
        public bool ContiniusRefresh { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler ContiniusRefreshChanged;

        private void OnContiniusRefreshChanged(object befor, object after)
        {
            ContiniusRefreshChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContiniusRefresh)));
        }
    }
}
