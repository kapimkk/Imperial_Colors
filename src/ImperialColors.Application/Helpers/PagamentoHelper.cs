using ImperialColors.Domain.Enums;
using ImperialColors.Domain.Exceptions;

namespace ImperialColors.Application.Helpers;

public static class PagamentoHelper
{
    public static string ObterDescricao(FormaPagamento forma, int parcelas = 1)
        => forma switch
        {
            FormaPagamento.Dinheiro => "Dinheiro",
            FormaPagamento.CartaoDebito => "Cartão de Débito",
            FormaPagamento.CartaoCredito when parcelas > 1 => $"Cartão de Crédito - {parcelas}x",
            FormaPagamento.CartaoCredito => "Cartão de Crédito",
            FormaPagamento.Pix => "Pix",
            FormaPagamento.Boleto => "Boleto",
            _ => forma.ToString()
        };

    public static bool PermiteParcelamento(FormaPagamento forma)
        => forma == FormaPagamento.CartaoCredito;

    public static bool UsaTroco(FormaPagamento forma)
        => forma == FormaPagamento.Dinheiro;

    public static bool ValorPagoAutomatico(FormaPagamento forma)
        => forma != FormaPagamento.Dinheiro;

    public static (decimal ValorPago, decimal Troco, int Parcelas) CalcularPagamento(
        FormaPagamento forma,
        decimal total,
        decimal valorRecebido,
        int parcelas)
    {
        return forma switch
        {
            FormaPagamento.Dinheiro => (valorRecebido, Math.Max(0, valorRecebido - total), 1),
            FormaPagamento.CartaoCredito => (total, 0, Math.Clamp(parcelas, 1, 12)),
            _ => (total, 0, 1)
        };
    }

    public static void ValidarPagamento(FormaPagamento forma, decimal total, decimal valorRecebido, int parcelas)
    {
        if (total <= 0)
            throw new DomainException("Total da venda deve ser maior que zero.");

        if (forma == FormaPagamento.Dinheiro && valorRecebido < total)
            throw new DomainException("Valor recebido insuficiente para cobrir o total da venda.");

        if (forma == FormaPagamento.CartaoCredito && (parcelas < 1 || parcelas > 12))
            throw new DomainException("Selecione parcelas entre 1x e 12x.");
    }
}
