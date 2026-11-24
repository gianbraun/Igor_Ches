using System;
using System.Collections.Generic;

namespace IGOR.Interface.Parameters
{
    public interface IParameterService
    {

        IAppParameters AppParameters { get; }

        IPlcParameters PlcParameters { get; }

        IReadOnlyCollection<ParameterGroup> ParameterGroups {  get; }

        void SaveParameters(IEnumerable<Parameter> parameters);
        void SaveParameter(Parameter parameters);
        void SaveAllOutDateParameters();
        void ReadAllParametersFromPlc();
        void ReadParameterFromPlc(Parameter parameter);

        void WriteParametersToPlc(IEnumerable<Parameter> plcParameters);
        bool TryWriteParameterToPlc(Parameter plcParameter);

        bool TryWriteParameterToPlc(Parameter plcParameter, object value);
    }
}
