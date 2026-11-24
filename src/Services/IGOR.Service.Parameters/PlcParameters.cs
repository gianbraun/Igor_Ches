using IGOR.Interface.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Service.Parameters
{
    internal class PlcParameters : IPlcParameters
    {
        public IReadOnlyCollection<PlcParameter> DynamicParameters { get; set; } = new List<PlcParameter>();
    }
}
