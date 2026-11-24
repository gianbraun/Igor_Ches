using IGOR.Interface.Navigation;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Service.Navigation
{
    public class NavigationButton : INavigationButton
    {
        public string Name { get; }
        public string IconRessourceKey { get; }
        public string Destination { get; }
        public bool Active { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public NavigationButton(string name, string iconRessourceKey, string destination)
        {
            Name = name;
            IconRessourceKey = iconRessourceKey;
            Destination = destination;
        }
    }
}
