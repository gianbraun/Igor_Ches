using Prism.Mvvm;

namespace IGOR.ViewModels
{
    /// <summary>
    /// The ViewModel for the MainWindow
    /// </summary>
    public class ShellViewModel : BindableBase
    {

        /// <summary>
        /// Title of the Appliction
        /// </summary>
        public string Title { get; set; } = "Prism Application";

        /// <summary>
        /// The ViewModel for the MainWindow
        /// </summary>
        public ShellViewModel()
        {

        }
    }
}
