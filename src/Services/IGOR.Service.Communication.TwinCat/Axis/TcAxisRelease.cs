using IGOR.Interface.Communication.Axis;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace IGOR.Service.Communication.TwinCat.Axis
{
    public class TcAxisRelease : IAxisRelease
    {
        public bool ControllerRelease { get; set; }
        public bool FeedFW { get; set; }
        public bool FeedBW { get; set; }
        public double Override { get; set; }
        public bool ContiniusRefresh { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ContiniusRefreshChanged;

        private void OnContiniusRefreshChanged(object befor, object after)
        {
            ContiniusRefreshChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContiniusRefresh)));
        }
    }
}
