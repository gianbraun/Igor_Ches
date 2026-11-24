using IGOR.Interface.Communication.Actor;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Service.Communication.TwinCat
{
    public class ActorAction : BindableBase, IActorAction
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; }
        public string ExecuteAddress { get; }
        public string IniAddress { get; }
        private bool _isIniActive;
        public bool IsIniActive 
        {
            get => _isIniActive;
            set => SetProperty(ref _isIniActive, value);
        }

        public ActorAction(string name, string executeAddress, string iniAddress)
        {
            Name = name;
            ExecuteAddress = executeAddress;
            IniAddress = iniAddress;
        }
    }
}
