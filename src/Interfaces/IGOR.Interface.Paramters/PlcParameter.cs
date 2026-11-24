
using Newtonsoft.Json;

namespace IGOR.Interface.Parameters
{
    public class PlcParameter : Parameter
    {
        public string PlcVariableName { get; }

        public PlcParameter(string name, string group, string unit, string plcVariableName, object value, double? min, double? max) : base(name, group, unit, value, min, max)
        {
            PlcVariableName = plcVariableName;
        }
    }

    public class PlcParameter<T> : PlcParameter
    {
        [JsonIgnore]
        public T GenericValue { get; set; }

        public PlcParameter(string name, string group, string unit, string plcVariableName, T value, double? min, double? max) : base(name, group, unit, plcVariableName, value, min, max)
        {
            Value = value;
            GenericValue = value;
        }


    }
}
