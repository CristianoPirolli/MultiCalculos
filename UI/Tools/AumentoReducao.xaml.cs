using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CalculadoraInteligente.UI.Tools;

public partial class AumentoReducao : Window
{
    public AumentoReducao()
    {
        InitializeComponent();
        Loaded += (_, _) => Recalcular();
    }

    private void Calcular_Click(object sender, RoutedEventArgs e) => Recalcular();

    private void Entrada_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        Recalcular();
    }

    private void SelectionChanged_Recalcular(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        Recalcular();
    }

    private void Recalcular()
    {
        AtualizarRotulos();

        if (!TryParseDecimal(TbValorInicial.Text, out decimal valorInicial) || valorInicial < 0)
        {
            LimparSaidas();
            return;
        }

        if (!TryParseDecimal(TbEntrada.Text, out decimal entrada) || entrada < 0)
        {
            LimparSaidas();
            return;
        }

        bool entradaEmPorcentagem = GetModoEntrada() == "Porcentagem";
        bool reduzir = GetOperacao() == "Reduzir";

        decimal percentual = entradaEmPorcentagem
            ? entrada
            : valorInicial == 0m ? 0m : entrada / valorInicial * 100m;

        decimal valorAplicado = entradaEmPorcentagem
            ? valorInicial * (entrada / 100m)
            : entrada;

        decimal valorFinal = reduzir
            ? valorInicial - valorAplicado
            : valorInicial + valorAplicado;

        TbEntradaCalculada.Text = entradaEmPorcentagem
            ? FormatarMoeda(valorAplicado)
            : FormatarPercentual(percentual);
        TbResultadoFinal.Text = FormatarMoeda(valorFinal);

        TxtResumoPercentual.Text = $"Percentual: {FormatarPercentual(percentual)}";
        TxtResumoValor.Text = $"Valor do ajuste: {FormatarMoeda(valorAplicado)}";
        TxtResumoResultado.Text = $"Valor final: {FormatarMoeda(valorFinal)}";
    }

    private void AtualizarRotulos()
    {
        bool entradaEmPorcentagem = GetModoEntrada() == "Porcentagem";

        LblEntradaCalculada.Text = entradaEmPorcentagem
            ? "Valor do ajuste"
            : "Percentual";

        LblResultadoFinal.Text = "Valor final";
    }

    private void LimparSaidas()
    {
        TbEntradaCalculada.Text = string.Empty;
        TbResultadoFinal.Text = string.Empty;
        TxtResumoPercentual.Text = "Percentual: -";
        TxtResumoValor.Text = "Valor do ajuste: -";
        TxtResumoResultado.Text = "Valor final: -";
    }

    private string GetOperacao() =>
        (CbOperacao.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Aumentar";

    private string GetModoEntrada() =>
        (CbModoEntrada.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Porcentagem";

    private static string FormatarMoeda(decimal valor) => valor.ToString("N2", CultureInfo.CurrentCulture);

    private static string FormatarPercentual(decimal valor) => $"{valor:N2}%";

    private static bool TryParseDecimal(string texto, out decimal valor)
    {
        var cultura = CultureInfo.CurrentCulture;
        var normalizado = texto.Trim();

        return decimal.TryParse(normalizado, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, cultura, out valor)
            || decimal.TryParse(normalizado.Replace(".", string.Empty).Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
    }
}
