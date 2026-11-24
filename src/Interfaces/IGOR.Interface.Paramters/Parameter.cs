using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;

namespace IGOR.Interface.Parameters
{
    public class Parameter : INotifyPropertyChanged
    {
        public string Name { get; }
        public string TypeName { get; }
        public object _value;
        public object Value
        {
            get => _value;
            set => _value = value;
        }

        [JsonIgnore]
        public object OnlineValue { get; set; }

        public string Unit { get; }
        public string Group { get; }

        public double? Min { get; }
        public double? Max { get; }

        [JsonIgnore]
        public bool OutDated { get; set; }
        [JsonIgnore]
        public object UpdatedValue { get; set; }


        public Parameter(string name, string group, string unit, object value, double? min, double? max)
        {
            Name = name;
            Value = value;
            Group = group;
            Unit = unit;
            Min = min;
            Max = max;
            TypeName = value.GetType().FullName;
            UpdatedValue = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
            => Name;

    }

    public class Parameter<T> : Parameter
    {
        [JsonIgnore]
        public T GenericValue { get; set; }

        [JsonIgnore]
        public new T OnlineValue { get; set; }

        [JsonIgnore]
        public new T EditedValue { get; set; }


        public Parameter(string name, string group, string unit, T value, double? min, double? max) : base(name, group, unit, value, min, max)
        {
            GenericValue = value;
            Value = value;
        }

        public static implicit operator T(Parameter<T> parameter)
        {
            if (parameter == null)
                return default;

            return parameter.GenericValue;
        }
    }
}