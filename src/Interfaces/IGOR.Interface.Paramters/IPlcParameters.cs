using System.Collections.Generic;

namespace IGOR.Interface.Parameters
{
    public interface IPlcParameters
    {
        IReadOnlyCollection<PlcParameter> DynamicParameters { get; }
    }
}