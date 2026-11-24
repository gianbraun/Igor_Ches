using IGOR.Interface.Communication.Axis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Service.Communication.TwinCat.Axis
{
    public class TcAxisParam : IAxisParam
    {
        public double HandSlow { get; set; }
        public double HandFast { get; set; }
        public bool SoftlimitNegAktive { get; set; }
        public bool SoftlimitPosAktive { get; set; }
        public double MinPos { get; set; }
        public double MaxPos { get; set; }
        

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
