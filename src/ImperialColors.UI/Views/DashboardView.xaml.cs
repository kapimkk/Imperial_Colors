using ImperialColors.UI.ViewModels;
using System.Windows.Controls;

namespace ImperialColors.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
