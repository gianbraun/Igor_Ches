using IGOR.Common;
using IGOR.Common.WPF.Icons;
using IGOR.Display.Parameters.Views;
using IGOR.Service.Navigation.Interface;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Display.Parameters
{
    public class ParameterDisplayModule : IModule
    {
        private INavigationService _navigationService;

        public ParameterDisplayModule(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _navigationService.AddNavigationButton("Parameter", IconKey.Parameter, "ParameterView");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation(typeof(ParameterView), "ParameterView");
            containerRegistry.RegisterDialog<EditParameterDialogView>(DialogNames.EditParameters);
        }
    }
}
