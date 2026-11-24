using IGOR.Interface.Communication.Axis;
using System.ComponentModel;

namespace IGOR.Service.Communication.TwinCat.Axis
{
    public class TcAxisState : IAxisState
    {
        public bool Ready { get; set; }
        public bool MoveFW { get; set; }
        public bool MoveBW { get; set; }
        public bool NotMoving { get; set; }
        public bool HasJob { get; set; }
        public bool InDestinationPos { get; set; }
        public uint ErrorCode { get; set; }
        public bool ContiniusRefresh { get; set; }
        public bool Referenzed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ContiniusRefreshChanged;

        private void OnContiniusRefreshChanged(object befor, object after)
        {
            ContiniusRefreshChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContiniusRefresh)));
        }
    }
}
