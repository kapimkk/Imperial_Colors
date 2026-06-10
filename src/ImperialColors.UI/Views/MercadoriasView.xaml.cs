using ImperialColors.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImperialColors.UI.Views;

public partial class MercadoriasView : UserControl
{
    private readonly FornecedorViewModel _viewModel;

    public MercadoriasView(FornecedorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.FornecedorSelecionado is not null)
            _viewModel.EditarFornecedorCommand.Execute(null);
    }
}
