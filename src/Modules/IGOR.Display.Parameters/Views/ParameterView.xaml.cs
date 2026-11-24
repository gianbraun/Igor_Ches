using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IGOR.Display.Parameters.Views
{
    /// <summary>
    /// Interaktionslogik für ParameterView.xaml
    /// </summary>
    public partial class ParameterView : UserControl
    {
        public ParameterView()
        {
            InitializeComponent();
        }

        private void DataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var grid = (DataGrid)sender;
            grid.Columns[0].Width = 0;
            grid.Columns[1].Width = 0;
            grid.Columns[2].Width = 0;
            grid.Columns[3].Width = 0;
            grid.UpdateLayout();
            grid.Columns[1].Width = DataGridLength.Auto;
            grid.Columns[2].Width = DataGridLength.Auto;
            grid.Columns[3].Width = DataGridLength.Auto;
            grid.UpdateLayout();
            grid.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }
    }
}
