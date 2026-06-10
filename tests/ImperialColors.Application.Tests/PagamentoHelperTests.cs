using ImperialColors.Domain.Enums;
using ImperialColors.Domain.Exceptions;
using Xunit;

namespace ImperialColors.Application.Tests;

public class PagamentoHelperTests
{
    [Theory]
    [InlineData(FormaPagamento.Dinheiro, 100, 150, 1, 150, 50, 1)]
    [InlineData(FormaPagamento.Pix, 100, 0, 1, 100, 0, 1)]
    [InlineData(FormaPagamento.CartaoCredito, 200, 0, 3, 200, 0, 3)]
    public void CalcularPagamento_RetornaValoresCorretos(
        FormaPagamento forma, decimal total, decimal recebido, int parcelas,
        decimal valorPagoEsperado, decimal trocoEsperado, int parcelasEsperadas)
    {
        var (valorPago, troco, parcelasFinal) =
            ImperialColors.Application.Helpers.PagamentoHelper.CalcularPagamento(forma, total, recebido, parcelas);

        Assert.Equal(valorPagoEsperado, valorPago);
        Assert.Equal(trocoEsperado, troco);
        Assert.Equal(parcelasEsperadas, parcelasFinal);
    }

    [Fact]
    public void ValidarPagamento_DinheiroInsuficiente_LancaDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            ImperialColors.Application.Helpers.PagamentoHelper.ValidarPagamento(
                FormaPagamento.Dinheiro, 100, 50, 1));

        Assert.Contains("insuficiente", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ObterDescricao_CreditoParcelado_RetornaTextoFormatado()
    {
        var descricao = ImperialColors.Application.Helpers.PagamentoHelper.ObterDescricao(
            FormaPagamento.CartaoCredito, 3);

        Assert.Equal("Cartão de Crédito - 3x", descricao);
    }
}
