using IGOR.Interface.Communication;
using IGOR.Interface.Communication.Actor;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IGOR.Display.ManualOperations.ViewModels
{
    public class ActorsDetialViewModel : BindableBase, INavigationAware
    {
        private IActor _actor;
        private readonly IPlcCom _plcCom;
        private CancellationTokenSource pollingTokenSource;



        private DelegateCommand<IActorAction> _executeAction;
        public DelegateCommand<IActorAction> ExecuteActionCommand => _executeAction ??= new DelegateCommand<IActorAction>(ExecuteAction);

        public IEnumerable<IActorAction> Actions { get; private set; }


        public ActorsDetialViewModel(IPlcCom plcCom)
        {
            _plcCom = plcCom;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            pollingTokenSource.Cancel();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _actor = navigationContext.Parameters["Actor"] as IActor;
            Actions = _actor.Actions;
            pollingTokenSource = new CancellationTokenSource();
            StartPolling();
        }

        private void StartPolling()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    pollingTokenSource.Token.ThrowIfCancellationRequested();
                    foreach (var action in _actor.Actions)
                    {
                        if (_plcCom.TryRead<bool>(action.IniAddress, out var isActive))
                        {
                            action.IsIniActive = isActive;
                        }
                    }
                    Thread.Sleep(500);
                }

            });
        }

        private void ExecuteAction(IActorAction action)
        {
            Task.Run(() =>
            {
                _plcCom.TryWrite(action.ExecuteAddress, true);
                Thread.Sleep(500);
                _plcCom.TryWrite(action.ExecuteAddress, false);
            });
        }
    }
}
