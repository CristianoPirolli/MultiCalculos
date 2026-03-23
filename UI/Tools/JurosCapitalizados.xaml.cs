using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CalculadoraInteligente.UI.Tools;

public partial class JurosCapitalizados : Window
{
    private bool _modoSimples = false;

    public JurosCapitalizados()
    {
        InitializeComponent();
        AtualizarDataPrimeira();
    }

    private void AtualizarDataPrimeira()
    {
        int dias = ChkAVista.IsChecked == true ? 0
            : int.TryParse(TbDiasPrimeira.Text.Trim(), out int d) ? d : 30;

        var dataPrimeira = DateTime.Today.AddDays(dias);
        TbDataPrimeira.Text    = dataPrimeira.ToString("dd/MM/yyyy");
        TbDiaVencimento.Text   = dataPrimeira.Day.ToString();
    }

    private void Calcular_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseDecimal(TbValorInicial.Text, "Valor Inicial", out decimal pv)) return;
        if (!TryParseDecimal(TbTaxa.Text,         "Juro %",        out decimal taxaPct)) return;
        if (!TryParseInt    (TbParcelas.Text,     "Qtd. Parcelas", out int n)) return;

        AtualizarDataPrimeira();

        decimal i = taxaPct / 100m;
        decimal prestacao, montante;

        if (_modoSimples)
        {
            // Juros simples: M = P*(1 + i*n), prestação = M/n
            montante  = pv * (1m + i * n);
            prestacao = montante / n;
        }
        else
        {
            // Price (juros compostos): PMT = PV * i*(1+i)^n / ((1+i)^n - 1)
            double fator = Math.Pow((double)(1m + i), n);
            prestacao = pv * i * (decimal)fator / ((decimal)fator - 1m);
            montante  = prestacao * n;
        }

        decimal jurosTotal = montante - pv;

        TbPrestacao.Text    = $"{prestacao:N2}";
        TbValorPrimeira.Text = $"{prestacao:N2}";
        TbJurosTotal.Text   = $"{jurosTotal:N2}";
        TbValorFinal.Text   = $"{montante:N2}";
    }

    private void BtnModo_Click(object sender, RoutedEventArgs e)
    {
        _modoSimples = !_modoSimples;
        BtnModo.Content = _modoSimples ? "Modo\nSimples" : "Modo\nComposto";
    }

    private void Ajuda_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(this,
            "Preencha:\n" +
            "• Valor Inicial $  → capital emprestado\n" +
            "• Juro % a.m.      → taxa mensal\n" +
            "• Qtd. Parcelas    → número de meses\n\n" +
            "Clique em Calcular para obter a prestação fixa,\n" +
            "o total de juros e o valor final.\n\n" +
            "Modo Simples: juros simples (J = P·i·n)\n" +
            "Modo Composto: tabela Price (padrão)",
            "Ajuda — Juros Capitalizados",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DescontarCheque_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseDecimal(TbValorFinal.Text.Replace("R$", "").Trim(), "Valor Final", out decimal vf)) return;
        if (!TryParseDecimal(TbTaxa.Text, "Juro %", out decimal taxaPct)) return;
        if (!TryParseInt(TbParcelas.Text, "Qtd. Parcelas", out int n)) return;

        decimal i = taxaPct / 100m;
        // Desconto comercial: VP = VF / (1 + i*n)
        decimal vp = vf / (1m + i * n);
        decimal desconto = vf - vp;

        MessageBox.Show(this,
            $"Valor Presente (líquido): R$ {vp:N2}\n" +
            $"Desconto (juros):         R$ {desconto:N2}",
            "Desconto de Cheque", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Poupanca_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseDecimal(TbValorInicial.Text, "Valor Inicial", out decimal pv)) return;
        if (!TryParseDecimal(TbTaxa.Text, "Juro %", out decimal taxaPct)) return;
        if (!TryParseInt(TbParcelas.Text, "Qtd. Parcelas", out int n)) return;

        decimal i = taxaPct / 100m;
        decimal montante = pv * (decimal)Math.Pow((double)(1m + i), n);
        decimal juros    = montante - pv;

        MessageBox.Show(this,
            $"Aplicando R$ {pv:N2} por {n} mês(es) a {taxaPct:N2}% a.m.:\n\n" +
            $"Montante final: R$ {montante:N2}\n" +
            $"Juros gerados:  R$ {juros:N2}",
            "Simulação de Poupança", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private bool TryParseDecimal(string texto, string campo, out decimal valor)
    {
        if (decimal.TryParse(texto.Trim().Replace(',', '.'),
            NumberStyles.Number, CultureInfo.InvariantCulture, out valor) && valor >= 0)
            return true;
        MessageBox.Show(this, $"{campo}: informe um número positivo.", "Erro",
            MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    private bool TryParseInt(string texto, string campo, out int valor)
    {
        if (int.TryParse(texto.Trim(), out valor) && valor > 0) return true;
        MessageBox.Show(this, $"{campo}: informe um número inteiro positivo.", "Erro",
            MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }
}
