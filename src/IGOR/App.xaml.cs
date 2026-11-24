using DryIoc;
using IGOR.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using IGOR.Common.WPF.Modularity;
using IGOR.ViewModels;
using System.Threading.Tasks;
using System.Threading;
using IGOR.Interface.Communication;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Threading;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Diagnostics;

namespace IGOR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private ModuleCatalog _moduleCatalog;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }


        public App()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            DispatcherUnhandledException += (s, e) =>
            {
                //if (Debugger.IsAttached)
                //    return;
                e.Handled = true;
                UnhandeldExcpetion(e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                if (Debugger.IsAttached)
                    return;
                UnhandeldExcpetion(ex);
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                if (Debugger.IsAttached)
                    return;
                UnhandeldExcpetion(e.Exception);
            };

        }

        private void UnhandeldExcpetion(Exception ex)
        {
            var parameters = new DialogParameters();
            parameters.Add("excpetion", ex);
            Container.Resolve<ILogger>()
                .ForContext<App>()
                .Fatal(ex, "ein unbehandelte Ausnahme ist aufgetreten");
            Container.Resolve<IDialogService>().ShowDialog("ExceptionDialog", parameters, r => Current.Shutdown());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ExceptionDialog>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override IModuleCatalog CreateModuleCatalog()
        {
            _moduleCatalog = new PriorityDirectoryModuleCatalog { ModulePath = @"Modules/" };
            return _moduleCatalog;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void InitializeModules()
        {
            var splash = Container.Resolve<Views.SplashScreen>();
            var vm = splash.DataContext as SplashScreenViewModel;
            vm.ModulesLoaded += (s, e) =>
            {
                Current.Dispatcher.Invoke(() =>
                {
                    splash.Close();
                    vm = null;
                    MainWindow?.Show();
                });

            }; 
            splash.ContentRendered += async (s, e) => await vm.LoadModulesAsync();

            splash.Show();
        }

        protected override void OnInitialized()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
        typeof(FrameworkElement),
        new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(new CultureInfo("DE-de").IetfLanguageTag)));
        }

    }
}
