using ImperialColors.Application.DTOs;
using ImperialColors.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ImperialColors.UI.Helpers;

public static class WindowHelper
{
    public static void ExibirCupom(IServiceProvider serviceProvider, VendaDto venda, Window? owner = null)
    {
        var cupom = serviceProvider.GetRequiredService<CupomView>();
        cupom.InicializarVenda(venda);
        cupom.Owner = owner ?? System.Windows.Application.Current.MainWindow;
        cupom.ShowDialog();
    }
}
