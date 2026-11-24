using System.Collections.Generic;

namespace IGOR.Interface.Communication.Actor
{
    public interface IActor
    {

        string Name { get; }
        IEnumerable<IActorAction> Actions { get; }
    }
}
