using System;
using System.ComponentModel;

namespace IGOR.Interface.Navigation
{
    public interface INavigationButton : INotifyPropertyChanged
    {
        public string Name { get; }

        public string IconRessourceKey { get; }

        public string Destination { get; }

        public bool Active { get; set; }
    }
}
