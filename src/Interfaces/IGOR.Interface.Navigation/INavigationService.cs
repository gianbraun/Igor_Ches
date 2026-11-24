using IGOR.Interface.Navigation;
using Prism.Regions;
using System;

namespace IGOR.Service.Navigation.Interface
{
    public interface INavigationService
    {
        string ActualViewName { get; }
        
        string ActualBoardName { get; }

        void NavigateTo(string viewName, params (string key, object value)[] navigationParameters);

        void NavigateToBoard(string viewName);
        void NavigateToBoard(string viewName, Action<NavigationResult> callback);

        void NavigateTo(INavigationButton navigationButton);

        void AddNavigationButton(string name, string iconRessourceKey, string destination);

        void AddNavigationButton(string name, string iconRessourceKey, string destination, int index);

        void RemoveNavigationButton(string name);
    }
}
