using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Interface.Parameters
{
    public class ParameterGroup : INotifyPropertyChanged
    {

        public string Name { get; }

        public bool OutDated { get; set; }

        public ObservableCollection<Parameter> Parameters { get; set; } = new();

        public ObservableCollection<ParameterGroup> SubGroups { get; set; } = new();


        public ParameterGroup(string name)
        {
            Name = name;
            Parameters.CollectionChanged += ParameterCollectionChanged;
            SubGroups.CollectionChanged += SubGroupCollectionChanged;
        }

        private void SubGroupCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.OldItems is not  null)
            {
                var parameterGroups = e.OldItems as IEnumerable<ParameterGroup>;
                UnRegisterParameters(parameterGroups);
            }

            if(e.NewItems is not null)
            {
                var parameterGroups = e.NewItems as IEnumerable;
                RegisterParameters(parameterGroups);
            }
        }

        private void RegisterParameters(IEnumerable parameterGroups)
        {
            foreach (ParameterGroup paramGroup in parameterGroups)
            {
                RegisterParameters(paramGroup.SubGroups);
                foreach (var param in paramGroup.Parameters)
                {
                    param.PropertyChanged += ParameterChanged;
                }
            }
        }

        private void UnRegisterParameters(IEnumerable parameterGroups)
        {
            foreach (ParameterGroup paramGroup in parameterGroups)
            {
                RegisterParameters(paramGroup.SubGroups);
                foreach (var param in paramGroup.Parameters)
                {
                    param.PropertyChanged -= ParameterChanged;
                }
            }
        }

        private void ParameterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    item.PropertyChanged -= ParameterChanged;
                }
            }
            if(e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += ParameterChanged;
                }
            }
        }

        private void ParameterChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not Parameter parameter)
                return;
            if (e.PropertyName is (nameof(Parameter.OutDated)))
            {
                OutDated = parameter.OutDated;
            }
        }

      
        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return Name;
        }
    }
}
