using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IGOR.Interface.Communication.Actor
{
    public interface IActorAction : INotifyPropertyChanged
    {
        public string Name { get; }
        public string ExecuteAddress { get; }
        public string IniAddress { get; }
        public bool IsIniActive { get; set; }
    }
}
