using ImperialColors.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImperialColors.UI.Views;

public partial class EstoqueView : UserControl
{
    private readonly ProdutoViewModel _viewModel;

    public EstoqueView(ProdutoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.ProdutoSelecionado is not null)
            _viewModel.EditarProdutoCommand.Execute(null);
    }
}
