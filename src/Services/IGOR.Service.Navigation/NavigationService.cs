using IGOR.Common;
using IGOR.Interface.Navigation;
using IGOR.Service.Navigation.Interface;
using Prism.Regions;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace IGOR.Service.Navigation
{
    public class NavigationService : INavigationService
    {
        private IRegionManager _regionManager;

        private INavigationButton _lastActiveButton;
        private ILogger _log;

        public ObservableCollection<INavigationButton> NavigationButtons { get; } = new ObservableCollection<INavigationButton>();

        public string ActualViewName { get; private set; }
        public string ActualBoardName { get; private set; }

        public void AddNavigationButton(string name, string iconRessourceKey, string destination)
        {
            NavigationButtons.Add(new NavigationButton(name, iconRessourceKey, destination));
        }

        public void AddNavigationButton(string name, string iconRessourceKey, string destination, int index)
        {
            NavigationButtons.Insert(index, new NavigationButton(name, iconRessourceKey, destination));
        }

        public void NavigateTo(string viewName, params (string key, object value)[] parameters)
        {
            if (_lastActiveButton != null)
                _lastActiveButton.Active = false;
            ActualViewName = viewName;
            _lastActiveButton = NavigationButtons.FirstOrDefault(b => b.Destination == viewName);
            var navParameters = new NavigationParameters();

            foreach ((var key, var value) in parameters)
            {
                navParameters.Add(key, value);
            }

            _regionManager.RequestNavigate(RegionNames.MainRegion, viewName, p =>
            {

                if (!p.Result == true)
                {
                    _log.Warning(p.Error, "Navigation zu {0} fehlgeschlagen", viewName);
                }
            }, navParameters);

            if (_lastActiveButton != null)
                _lastActiveButton.Active = true;
        }

        public void NavigateTo(INavigationButton navigationButton)
        {

            NavigateToBoard("DefaultGameView");
            if (_lastActiveButton != null)
            {
                _lastActiveButton.Active = false;
            }

            navigationButton.Active = true;
            _lastActiveButton = navigationButton;
            ActualViewName = navigationButton.Destination;
            _regionManager.RequestNavigate(RegionNames.MainRegion, navigationButton.Destination, p =>
            {
                if (!p.Result == true)
                {
                    _log.Warning(p.Error, "Navigation zu {0} fehlgeschlagen", navigationButton.Destination);
                }
            });
        }

        public void RemoveNavigationButton(string name)
        {
            var element = NavigationButtons.First(n => n.Name == name);
            NavigationButtons.Remove(element);
        }

        public void NavigateToBoard(string viewName)
        {
            ActualBoardName = viewName;
            _regionManager.RequestNavigate(RegionNames.GameBoardRegion, viewName, p =>
            {
                if (!p.Result == true)
                {
                    _log.Warning(p.Error, "Navigation zu {0} fehlgeschlagen", viewName);
                }
            });
        }

        public void NavigateToBoard(string viewName, Action<NavigationResult> callback)
        {
            ActualBoardName = viewName;
            _regionManager.RequestNavigate(RegionNames.GameBoardRegion, viewName, callback);
        }

        public NavigationService(IRegionManager regionManager, ILogger logger)
        {
            _regionManager = regionManager;
            _log = logger.ForContext<NavigationService>();
        }
    }
}
