using IGOR.Interface.Navigation;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Mvvm;

namespace IGOR.Service.Navigation.ViewModels
{
    public class NavigationViewModel : BindableBase
    {
        public NavigationService NavigationService { get; }

        public DelegateCommand<INavigationButton> NavigateCommand => new DelegateCommand<INavigationButton>(OnNavigateCommand);

        public NavigationViewModel(INavigationService navigationService)
        {
            NavigationService = (NavigationService)navigationService;
        }

        private void OnNavigateCommand(INavigationButton navigationButton)
        {
            NavigationService.NavigateTo(navigationButton);
        }
    }
}
