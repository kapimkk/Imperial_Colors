using ClosedXML.Excel;

using ImperialColors.Application.Configuration;

using ImperialColors.Application.DTOs;

using ImperialColors.Domain.Enums;

using iText.IO.Font.Constants;

using iText.IO.Image;

using iText.Kernel.Colors;

using iText.Kernel.Font;

using iText.Kernel.Pdf;

using iText.Layout;

using iText.Layout.Element;

using iText.Layout.Properties;

using Microsoft.Extensions.Options;

using System.IO;



namespace ImperialColors.UI.Services;



using ITextParagraph = iText.Layout.Element.Paragraph;

using ITextTable = iText.Layout.Element.Table;

using ITextCell = iText.Layout.Element.Cell;



public class RelatorioService : IRelatorioService

{

    private readonly IAppConfigService _config;

    private readonly IOptionsMonitor<EmpresaConfig> _empresa;



    public RelatorioService(IAppConfigService config, IOptionsMonitor<EmpresaConfig> empresa)

    {

        _config = config;

        _empresa = empresa;

    }



    private EmpresaConfig Empresa => _empresa.CurrentValue;



    private static PdfFont ObterFonte(bool negrito = false)

        => PdfFontFactory.CreateFont(negrito ? StandardFonts.HELVETICA_BOLD : StandardFonts.HELVETICA);



    public Task GerarCupomPdfAsync(VendaDto venda, string caminhoArquivo)

    {

        return Task.Run(() =>

        {

            using var writer = new PdfWriter(caminhoArquivo);

            using var pdf = new PdfDocument(writer);

            using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A6);

            document.SetMargins(16, 16, 16, 16);



            AdicionarCabecalhoCupom(document);



            AdicionarLinhaSeparadora(document);



            document.Add(CriarLinhaRotuloValor("Venda Nº:", venda.NumeroVenda));

            document.Add(CriarLinhaRotuloValor("Data:", venda.DataVenda.ToString("dd/MM/yyyy HH:mm")));

            document.Add(CriarLinhaRotuloValor("Cliente:", venda.ClienteNome ?? "Consumidor Final").SetMarginBottom(4));



            AdicionarLinhaSeparadora(document);



            var tabela = new ITextTable(new float[] { 3, 1, 1.5f, 1.5f }).UseAllAvailableWidth();

            AdicionarCabecalhoTabela(tabela, "Produto", "Qtd", "Preco", "Total");



            foreach (var item in venda.Itens)

            {

                tabela.AddCell(CelulaCupom(item.NomeProduto));

                tabela.AddCell(CelulaCupom($"{item.Quantidade} {item.Unidade}", TextAlignment.RIGHT));

                tabela.AddCell(CelulaCupom(item.PrecoUnitario.ToString("C2", new System.Globalization.CultureInfo("pt-BR")), TextAlignment.RIGHT));

                tabela.AddCell(CelulaCupom(item.Subtotal.ToString("C2", new System.Globalization.CultureInfo("pt-BR")), TextAlignment.RIGHT));

            }



            document.Add(tabela);

            AdicionarLinhaSeparadora(document);



            document.Add(CriarLinhaRotuloValor("Subtotal:", venda.Subtotal.ToString("C2", new System.Globalization.CultureInfo("pt-BR"))));



            if (venda.Desconto > 0)

                document.Add(CriarLinhaRotuloValor("Desconto:", venda.Desconto.ToString("C2", new System.Globalization.CultureInfo("pt-BR"))));



            document.Add(new ITextParagraph($"TOTAL: {venda.Total.ToString("C2", new System.Globalization.CultureInfo("pt-BR"))}")

                .SetFont(ObterFonte(true)).SetFontSize(13).SetTextAlignment(TextAlignment.RIGHT)

                .SetFontColor(ColorConstants.BLACK).SetMarginTop(4));



            AdicionarLinhaSeparadora(document);



            document.Add(new ITextParagraph("PAGAMENTO")

                .SetFont(ObterFonte(true)).SetFontSize(10).SetFontColor(ColorConstants.BLACK).SetMarginBottom(4));



            document.Add(CriarLinhaRotuloValor("Forma:", venda.FormaPagamentoDescricao));



            if (venda.FormaPagamento == FormaPagamento.Dinheiro)

            {

                document.Add(CriarLinhaRotuloValor("Valor Recebido:", venda.ValorPago.ToString("C2", new System.Globalization.CultureInfo("pt-BR"))));

                document.Add(CriarLinhaRotuloValor("Troco:", venda.Troco.ToString("C2", new System.Globalization.CultureInfo("pt-BR"))));

            }



            AdicionarLinhaSeparadora(document);

            document.Add(new ITextParagraph(_config.CupomRodape)

                .SetFont(ObterFonte(true)).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)

                .SetFontColor(ColorConstants.BLACK));

        });

    }



    private void AdicionarCabecalhoCupom(Document document)

    {

        if (File.Exists(_config.LogoSemFundoPath))

        {

            var logo = new Image(ImageDataFactory.Create(_config.LogoSemFundoPath))

                .SetWidth(42)

                .SetHorizontalAlignment(HorizontalAlignment.CENTER)

                .SetMarginBottom(4);

            document.Add(logo);

        }



        document.Add(new ITextParagraph(Empresa.NomeFantasia.ToUpperInvariant())

            .SetFont(ObterFonte(true)).SetFontColor(ColorConstants.BLACK).SetFontSize(13)

            .SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(2));



        if (!string.IsNullOrWhiteSpace(Empresa.RazaoSocial))

        {

            document.Add(new ITextParagraph(Empresa.RazaoSocial)

                .SetFont(ObterFonte(true)).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)

                .SetFontColor(ColorConstants.BLACK).SetMarginBottom(2));

        }



        if (!string.IsNullOrWhiteSpace(Empresa.CNPJ))

        {

            document.Add(new ITextParagraph($"CNPJ: {Empresa.CNPJ}")

                .SetFont(ObterFonte(true)).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)

                .SetFontColor(ColorConstants.BLACK).SetMarginBottom(1));

        }



        if (!string.IsNullOrWhiteSpace(Empresa.Endereco))

        {

            document.Add(new ITextParagraph(Empresa.Endereco)

                .SetFont(ObterFonte(true)).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)

                .SetFontColor(ColorConstants.BLACK).SetMarginBottom(1));

        }



        if (!string.IsNullOrWhiteSpace(Empresa.Telefone))

        {

            document.Add(new ITextParagraph($"Tel: {Empresa.Telefone}")

                .SetFont(ObterFonte(true)).SetFontSize(8).SetTextAlignment(TextAlignment.CENTER)

                .SetFontColor(ColorConstants.BLACK).SetMarginBottom(2));

        }



        document.Add(new ITextParagraph("CUPOM NÃO FISCAL")

            .SetFont(ObterFonte(true)).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)

            .SetFontColor(ColorConstants.BLACK).SetMarginBottom(2));

    }



    private static ITextParagraph CriarLinhaRotuloValor(string rotulo, string valor)

    {

        return new ITextParagraph()

            .Add(new Text(rotulo).SetFont(ObterFonte(true)).SetFontColor(ColorConstants.BLACK))

            .Add(new Text(" ").SetFont(ObterFonte(true)).SetFontColor(ColorConstants.BLACK))

            .Add(new Text(valor).SetFont(ObterFonte(true)).SetFontColor(ColorConstants.BLACK))

            .SetFontSize(9)

            .SetMarginBottom(2);

    }



    private static ITextCell CelulaCupom(string texto, TextAlignment alignment = TextAlignment.LEFT)

        => new ITextCell().Add(new ITextParagraph(texto).SetFont(ObterFonte(true)).SetFontSize(8)

                .SetTextAlignment(alignment).SetFontColor(ColorConstants.BLACK))

            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)

            .SetPadding(2);



    public Task GerarRelatorioVendasPdfAsync(IEnumerable<VendaDto> vendas, DateTime inicio, DateTime fim, string caminhoArquivo)
    {
        return Task.Run(() =>
        {
            using var writer = new PdfWriter(caminhoArquivo);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
            document.SetMargins(30, 30, 30, 30);

            AdicionarCabecalhoRelatorio(document, "Relatorio de Vendas",
                $"Periodo: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}");

            var tabela = new ITextTable(new float[] { 2, 2, 3, 1.5f, 1.5f, 1.5f, 1.5f }).UseAllAvailableWidth();
            AdicionarCabecalhoTabela(tabela, "N Venda", "Data", "Cliente", "Subtotal", "Desconto", "Total", "Status");

            var lista = vendas.ToList();
            foreach (var venda in lista)
            {
                tabela.AddCell(CelulaTabela(venda.NumeroVenda));
                tabela.AddCell(CelulaTabela(venda.DataVenda.ToString("dd/MM/yy HH:mm")));
                tabela.AddCell(CelulaTabela(venda.ClienteNome ?? "Consumidor Final"));
                tabela.AddCell(CelulaTabela(venda.Subtotal.ToString("C2", new System.Globalization.CultureInfo("pt-BR")), TextAlignment.RIGHT));
                tabela.AddCell(CelulaTabela(venda.Desconto.ToString("C2", new System.Globalization.CultureInfo("pt-BR")), TextAlignment.RIGHT));
                tabela.AddCell(CelulaTabela(venda.Total.ToString("C2", new System.Globalization.CultureInfo("pt-BR")), TextAlignment.RIGHT));
                tabela.AddCell(CelulaTabela(venda.StatusDescricao));
            }

            document.Add(tabela);

            var total = lista.Sum(v => v.Total);
            document.Add(new ITextParagraph($"\nTotal do Periodo: {total.ToString("C2", new System.Globalization.CultureInfo("pt-BR"))}")
                .SetFont(ObterFonte(true)).SetFontSize(13).SetTextAlignment(TextAlignment.RIGHT));
        });
    }

    public Task GerarRelatorioEstoquePdfAsync(IEnumerable<ProdutoDto> produtos, string caminhoArquivo)
    {
        return Task.Run(() =>
        {
            using var writer = new PdfWriter(caminhoArquivo);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
            document.SetMargins(30, 30, 30, 30);

            AdicionarCabecalhoRelatorio(document, "Relatorio de Estoque", $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");

            var tabela = new ITextTable(new float[] { 1.5f, 3, 2, 1.5f, 1, 1.5f, 1.5f }).UseAllAvailableWidth();
            AdicionarCabecalhoTabela(tabela, "Codigo", "Nome", "Categoria", "Marca", "Estoque", "Un", "Preco");

            foreach (var produto in produtos)
            {
                tabela.AddCell(CelulaTabela(produto.CodigoInterno));
                tabela.AddCell(CelulaTabela(produto.Nome));
                tabela.AddCell(CelulaTabela(produto.CategoriaNome ?? "-"));
                tabela.AddCell(CelulaTabela(produto.MarcaNome ?? "-"));
                var celEstoque = CelulaTabela(produto.QuantidadeEstoque.ToString("G"), TextAlignment.RIGHT);
                if (produto.SemEstoque) celEstoque.SetFontColor(new DeviceRgb(220, 53, 69));
                else if (produto.EstoqueBaixo) celEstoque.SetFontColor(new DeviceRgb(253, 126, 20));
                tabela.AddCell(celEstoque);
                tabela.AddCell(CelulaTabela(produto.Unidade));
                tabela.AddCell(CelulaTabela(produto.PrecoVenda.ToString("C2", new System.Globalization.CultureInfo("pt-BR")), TextAlignment.RIGHT));
            }

            document.Add(tabela);
            document.Add(new ITextParagraph($"\nTotal de produtos: {produtos.Count()}").SetFont(ObterFonte()).SetFontSize(11));
        });
    }

    public Task GerarRelatorioVendasExcelAsync(IEnumerable<VendaDto> vendas, DateTime inicio, DateTime fim, string caminhoArquivo)
    {
        return Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Vendas");

            ws.Cell(1, 1).Value = $"{_config.EmpresaNome} - Relatorio de Vendas";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 7).Merge();

            ws.Cell(2, 1).Value = $"Periodo: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}";
            ws.Range(2, 1, 2, 7).Merge();

            var headers = new[] { "N Venda", "Data", "Cliente", "Subtotal", "Desconto", "Total", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(245, 194, 0);
                cell.Style.Font.Bold = true;
            }

            int row = 5;
            foreach (var venda in vendas)
            {
                ws.Cell(row, 1).Value = venda.NumeroVenda;
                ws.Cell(row, 2).Value = venda.DataVenda.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(row, 3).Value = venda.ClienteNome ?? "Consumidor Final";
                ws.Cell(row, 4).Value = venda.Subtotal;
                ws.Cell(row, 4).Style.NumberFormat.Format = "R$ #,##0.00";
                ws.Cell(row, 5).Value = venda.Desconto;
                ws.Cell(row, 5).Style.NumberFormat.Format = "R$ #,##0.00";
                ws.Cell(row, 6).Value = venda.Total;
                ws.Cell(row, 6).Style.NumberFormat.Format = "R$ #,##0.00";
                ws.Cell(row, 7).Value = venda.StatusDescricao;
                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 249, 250);
                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(caminhoArquivo);
        });
    }

    public Task GerarRelatorioEstoqueExcelAsync(IEnumerable<ProdutoDto> produtos, string caminhoArquivo)
    {
        return Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Estoque");

            ws.Cell(1, 1).Value = $"{_config.EmpresaNome} - Relatorio de Estoque";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 7).Merge();

            var headers = new[] { "Codigo", "Nome", "Categoria", "Marca", "Estoque", "Unidade", "Preco Venda" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(3, i + 1);
                cell.Value = headers[i];
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(245, 194, 0);
                cell.Style.Font.Bold = true;
            }

            int row = 4;
            foreach (var p in produtos)
            {
                ws.Cell(row, 1).Value = p.CodigoInterno;
                ws.Cell(row, 2).Value = p.Nome;
                ws.Cell(row, 3).Value = p.CategoriaNome ?? "-";
                ws.Cell(row, 4).Value = p.MarcaNome ?? "-";
                ws.Cell(row, 5).Value = p.QuantidadeEstoque;
                ws.Cell(row, 6).Value = p.Unidade;
                ws.Cell(row, 7).Value = p.PrecoVenda;
                ws.Cell(row, 7).Style.NumberFormat.Format = "R$ #,##0.00";

                if (p.SemEstoque) ws.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
                else if (p.EstoqueBaixo) ws.Cell(row, 5).Style.Font.FontColor = XLColor.Orange;

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(248, 249, 250);
                row++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(caminhoArquivo);
        });
    }

    private void AdicionarCabecalhoRelatorio(Document document, string titulo, string subtitulo)
    {
        document.Add(new ITextParagraph(_config.EmpresaNome.ToUpperInvariant()).SetFont(ObterFonte(true)).SetFontSize(18)
            .SetFontColor(ColorConstants.BLACK).SetTextAlignment(TextAlignment.CENTER));
        document.Add(new ITextParagraph(_config.EmpresaSubtitulo).SetFont(ObterFonte()).SetFontSize(11)
            .SetFontColor(ColorConstants.BLACK).SetTextAlignment(TextAlignment.CENTER));
        document.Add(new ITextParagraph(titulo).SetFont(ObterFonte(true)).SetFontSize(14)
            .SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10));
        document.Add(new ITextParagraph(subtitulo).SetFont(ObterFonte()).SetFontSize(10)
            .SetFontColor(ColorConstants.BLACK).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(15));

        var separador = new ITextTable(1).UseAllAvailableWidth();
        separador.AddCell(new ITextCell().SetHeight(2).SetBackgroundColor(new DeviceRgb(245, 194, 0))
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
        document.Add((IBlockElement)separador.SetMarginBottom(15));
    }

    private static void AdicionarCabecalhoTabela(ITextTable tabela, params string[] headers)
    {
        foreach (var header in headers)
        {
            tabela.AddHeaderCell(new ITextCell().Add(new ITextParagraph(header).SetFont(ObterFonte(true)).SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(245, 194, 0))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetPadding(5));
        }
    }

    private static ITextCell CelulaTabela(string texto, TextAlignment alignment = TextAlignment.LEFT)
        => new ITextCell().Add(new ITextParagraph(texto).SetFont(ObterFonte()).SetFontSize(9).SetTextAlignment(alignment))
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetBorderBottom(new iText.Layout.Borders.SolidBorder(new DeviceRgb(233, 236, 239), 0.5f))
            .SetPadding(4);

    private static void AdicionarLinhaSeparadora(Document document)
    {
        var tabela = new ITextTable(1).UseAllAvailableWidth().SetMarginTop(3).SetMarginBottom(3);
        tabela.AddCell(new ITextCell().SetHeight(0.5f).SetBackgroundColor(new DeviceRgb(233, 236, 239))
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
        document.Add((IBlockElement)tabela);
    }
}
