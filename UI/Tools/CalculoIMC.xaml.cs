using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CalculadoraInteligente.UI.Tools;

public partial class CalculoIMC : Window
{
    public CalculoIMC() => InitializeComponent();

    private void Calcular_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseDecimal(TbAltura.Text, "Altura", out decimal alturaM)) return;
        if (!TryParseDecimal(TbPeso.Text, "Peso", out decimal peso)) return;

        if (alturaM > 3m) alturaM /= 100m;
        if (alturaM <= 0.5m || alturaM > 2.5m)
        {
            MessageBox.Show(this, "Altura inválida. Use metros (ex: 1,86) ou cm (ex: 186).",
                "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        bool isHomem = RbHomem.IsChecked == true;
        decimal h2 = alturaM * alturaM;

        decimal bmiMagro, bmiMedia, bmiSobrepeso, bmiGordo, bmiObeso;
        if (isHomem)
        {
            bmiMagro = 20.70m;
            bmiMedia = 23.55m;
            bmiSobrepeso = 26.40m;
            bmiGordo = 27.80m;
            bmiObeso = 31.10m;
        }
        else
        {
            bmiMagro = 18.50m;
            bmiMedia = 21.70m;
            bmiSobrepeso = 24.90m;
            bmiGordo = 29.90m;
            bmiObeso = 34.90m;
        }

        decimal pesoMagro = bmiMagro * h2;
        decimal pesoMedia = bmiMedia * h2;
        decimal pesoSobrepeso = bmiSobrepeso * h2;
        decimal pesoGordo = bmiGordo * h2;
        decimal pesoObeso = bmiObeso * h2;

        string classificacao;
        Color cor;
        if (peso < pesoMagro) { classificacao = "MAGRO"; cor = Color.FromRgb(34, 197, 94); }
        else if (peso <= pesoSobrepeso) { classificacao = "NORMAL"; cor = Color.FromRgb(132, 204, 22); }
        else if (peso <= pesoGordo) { classificacao = "SOBREPESO"; cor = Color.FromRgb(234, 179, 8); }
        else if (peso <= pesoObeso) { classificacao = "GORDO"; cor = Color.FromRgb(249, 115, 22); }
        else { classificacao = "OBESO"; cor = Color.FromRgb(239, 68, 68); }

        string recomendacao = peso < pesoMagro
            ? $"Ganhar\n{(pesoMagro - peso):N2} kg\npara ficar no\npeso normal"
            : peso > pesoSobrepeso
                ? $"Perder\n{(peso - pesoSobrepeso):N2} kg\npara ficar no\npeso normal"
                : "Você está\nno peso\nideal!";

        TbClassificacaoLabel.Text = $"Resultado = {classificacao}";
        TbClassificacaoLabel.Foreground = new SolidColorBrush(cor);
        TbLimMagro.Text = $"{pesoMagro:N2}";
        TbLimMedia.Text = $"{pesoMedia:N2}";
        TbLimSobrepeso.Text = $"{pesoSobrepeso:N2}";
        TbLimGordo.Text = $"{pesoGordo:N2}";
        TbLimObeso.Text = $"{pesoObeso:N2}";
        TbRecomendacao.Text = recomendacao;

        PainelResultadoLabel.Visibility = Visibility.Visible;
        PainelResultado.Visibility = Visibility.Visible;
        PainelBarra.Visibility = Visibility.Visible;

        decimal bmiAtual = peso / h2;
        decimal bmiMin = isHomem ? 15m : 13m;
        decimal bmiMax = isHomem ? 40m : 42m;
        double frac = Math.Max(0.02, Math.Min(0.98, (double)((bmiAtual - bmiMin) / (bmiMax - bmiMin))));

        Dispatcher.InvokeAsync(() =>
        {
            double w = CanvasBarra.ActualWidth;
            if (w > 0) System.Windows.Controls.Canvas.SetLeft(MarkerTriangle, frac * w);
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private bool TryParseDecimal(string texto, string campo, out decimal valor)
    {
        if (decimal.TryParse(texto.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out valor) && valor > 0)
            return true;

        MessageBox.Show(this, $"{campo}: informe um número positivo.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }
}
