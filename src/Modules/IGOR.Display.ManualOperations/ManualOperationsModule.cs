using IGOR.Common;
using IGOR.Common.WPF.Icons;
using IGOR.Display.ManualOperations.Views;
using IGOR.Interface.Parameters;
using IGOR.Service.Navigation.Interface;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Display.ManualOperations
{
    public class ManualOperationsModule : IModule
    {

        public ManualOperationsModule(INavigationService navigationService, IParameterService parameterService)
        {
            navigationService.AddNavigationButton("Handbetrieb", IconKey.HandMode, "ManualOperationsView");
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ManualOperationsView>();
            containerRegistry.RegisterForNavigation<AxisDetailView>();
            containerRegistry.RegisterForNavigation<ActorsDetialView>();
        }
    }
}
