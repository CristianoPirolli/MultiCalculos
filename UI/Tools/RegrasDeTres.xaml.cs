using System.Globalization;
using System.Windows;

namespace CalculadoraInteligente.UI.Tools;

public partial class RegrasDeTres : Window
{
    public RegrasDeTres()
    {
        InitializeComponent();
    }

    private void Calcular_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseDecimal(TbA.Text, "A", out decimal a)) return;
        if (!TryParseDecimal(TbB.Text, "B", out decimal b)) return;
        if (!TryParseDecimal(TbC.Text, "C", out decimal c)) return;

        if (a == 0)
        {
            MessageBox.Show(this, "A não pode ser zero.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        decimal x = RbDireta.IsChecked == true
            ? b * c / a          // direta:  A/B = C/X  →  X = B*C/A
            : a * b / c;         // inversa: A*B = C*X  →  X = A*B/C  (c≠0)

        TbX.Text = x.ToString("0.############################", CultureInfo.CurrentCulture);
    }

    private bool TryParseDecimal(string texto, string campo, out decimal valor)
    {
        var normalizado = texto.Trim().Replace(',', '.');
        if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out valor))
            return true;

        MessageBox.Show(this, $"{campo}: informe um número válido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }
}
