using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CalculadoraInteligente.UI.Tools;

public partial class ConversorMedidas : Window
{
    private readonly StringBuilder _history = new();
    private bool _suppressEvents;

    // ── Definição de unidades por categoria ──────────────────────────────
    private record Unidade(string Nome, string Simbolo, double FatorParaBase);

    private static readonly Dictionary<string, (string NomeCategoria, Unidade[] Unidades)> _categorias = new()
    {
        ["comprimento"] = ("Comprimento", new Unidade[]
        {
            new("Milímetro",       "mm",  0.001),
            new("Centímetro",      "cm",  0.01),
            new("Metro",           "m",   1.0),
            new("Quilômetro",      "km",  1000.0),
            new("Polegada",        "in",  0.0254),
            new("Pé",              "ft",  0.3048),
            new("Jarda",           "yd",  0.9144),
            new("Milha",           "mi",  1609.344),
            new("Milha náutica",   "nmi", 1852.0),
        }),
        ["peso"] = ("Peso / Massa", new Unidade[]
        {
            new("Miligrama",  "mg", 0.000_001),
            new("Grama",      "g",  0.001),
            new("Quilograma", "kg", 1.0),
            new("Tonelada",   "t",  1000.0),
            new("Libra",      "lb", 0.453_592),
            new("Onça",       "oz", 0.028_349_5),
        }),
        ["temperatura"] = ("Temperatura", new Unidade[]
        {
            new("Celsius",    "°C", 1.0),
            new("Fahrenheit", "°F", 1.0),   // conversão especial
            new("Kelvin",     "K",  1.0),   // conversão especial
        }),
        ["area"] = ("Área", new Unidade[]
        {
            new("Milímetro²",  "mm²", 0.000_001),
            new("Centímetro²", "cm²", 0.0001),
            new("Metro²",      "m²",  1.0),
            new("Quilômetro²", "km²", 1_000_000.0),
            new("Hectare",     "ha",  10_000.0),
            new("Acre",        "ac",  4_046.856),
            new("Pé²",         "ft²", 0.092_903),
            new("Polegada²",   "in²", 0.000_645_16),
        }),
        ["volume"] = ("Volume", new Unidade[]
        {
            new("Mililitro",      "ml",    0.000_001),
            new("Centilitro",     "cl",    0.000_01),
            new("Litro",          "l",     0.001),
            new("Metro³",         "m³",    1.0),
            new("Galão (EUA)",    "gal",   0.003_785_41),
            new("Onça fluida",    "fl oz", 0.000_029_573_5),
            new("Xícara (EUA)",   "cup",   0.000_236_588),
            new("Pé³",            "ft³",   0.028_316_8),
        }),
        ["velocidade"] = ("Velocidade", new Unidade[]
        {
            new("Metro/segundo",    "m/s",  1.0),
            new("Quilômetro/hora",  "km/h", 1.0 / 3.6),
            new("Milha/hora",       "mph",  0.447_04),
            new("Nó",               "kn",   0.514_444),
            new("Pé/segundo",       "ft/s", 0.3048),
        }),
        ["pressao"] = ("Pressão", new Unidade[]
        {
            new("Pascal",       "Pa",  1.0),
            new("Kilopascal",   "kPa", 1_000.0),
            new("Bar",          "bar", 100_000.0),
            new("PSI",          "psi", 6_894.757),
            new("Atmosfera",    "atm", 101_325.0),
            new("mmHg (Torr)",  "mmHg",133.322),
        }),
    };

    public ConversorMedidas()
    {
        InitializeComponent();
        CarregarCategoria("comprimento");
    }

    // ── Carrega unidades nos ComboBoxes ───────────────────────────────────

    private void CarregarCategoria(string tag)
    {
        if (CbUnidadeDe == null || CbUnidadePara == null) return;

        _suppressEvents = true;

        var (_, unidades) = _categorias[tag];

        CbUnidadeDe.Items.Clear();
        CbUnidadePara.Items.Clear();

        foreach (var u in unidades)
        {
            CbUnidadeDe.Items.Add(new ComboBoxItem
            {
                Content = $"{u.Nome} ({u.Simbolo})",
                Tag     = u
            });
            CbUnidadePara.Items.Add(new ComboBoxItem
            {
                Content = $"{u.Nome} ({u.Simbolo})",
                Tag     = u
            });
        }

        int unidadeBaseIndex = Array.FindIndex(unidades, u => Math.Abs(u.FatorParaBase - 1.0) < 0.000_000_1);
        if (unidadeBaseIndex < 0)
            unidadeBaseIndex = 0;

        CbUnidadeDe.SelectedIndex   = unidadeBaseIndex;
        CbUnidadePara.SelectedIndex = unidadeBaseIndex == 0 && unidades.Length > 1 ? 1 : 0;

        _suppressEvents = false;
        Converter();
    }

    // ── Eventos ──────────────────────────────────────────────────────────

    private void CbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CbCategoria.SelectedItem is ComboBoxItem item)
            CarregarCategoria(item.Tag?.ToString() ?? "comprimento");
    }

    private void Unidade_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_suppressEvents) Converter();
    }

    private void TbValorDe_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_suppressEvents) Converter();
    }

    private void BtnConverter_Click(object sender, RoutedEventArgs e)
    {
        Converter(addHistory: true);
    }

    // ── Lógica de conversão ───────────────────────────────────────────────

    private void Converter(bool addHistory = false)
    {
        if (CbUnidadeDe == null || CbUnidadePara == null || TbResultado == null) return;
        if (CbUnidadeDe.SelectedItem  is not ComboBoxItem itemDe  || itemDe.Tag  is not Unidade uDe)  return;
        if (CbUnidadePara.SelectedItem is not ComboBoxItem itemPara || itemPara.Tag is not Unidade uPara) return;

        if (!double.TryParse(TbValorDe.Text.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out double valor))
        {
            TbResultado.Text = "—";
            return;
        }

        double resultado = ConvertValue(valor, uDe, uPara);
        string resultStr = FormatNumber(resultado);
        TbResultado.Text = resultStr;

        if (addHistory)
        {
            string linha = $"{FormatNumber(valor)} {uDe.Simbolo}  =  {resultStr} {uPara.Simbolo}";
            _history.Insert(0, linha + Environment.NewLine);
            HistBox.Text = _history.ToString();
            HistScroll.ScrollToTop();
        }
    }

    private double ConvertValue(double valor, Unidade de, Unidade para)
    {
        // Temperatura: conversão especial (não linear)
        if (de.Simbolo.Contains("°") || de.Simbolo == "K" ||
            para.Simbolo.Contains("°") || para.Simbolo == "K")
        {
            return ConvertTemperatura(valor, de.Simbolo, para.Simbolo);
        }

        // Demais: converte para base e depois para destino
        double emBase = valor * de.FatorParaBase;
        return emBase / para.FatorParaBase;
    }

    private static double ConvertTemperatura(double valor, string de, string para)
    {
        // Normaliza para Celsius primeiro
        double celsius = de switch
        {
            "°C" => valor,
            "°F" => (valor - 32) * 5.0 / 9.0,
            "K"  => valor - 273.15,
            _    => valor,
        };

        return para switch
        {
            "°C" => celsius,
            "°F" => celsius * 9.0 / 5.0 + 32,
            "K"  => celsius + 273.15,
            _    => celsius,
        };
    }

    private static string FormatNumber(double v)
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) return "Erro";
        if (Math.Abs(v) >= 1e10 || (v != 0 && Math.Abs(v) < 1e-6))
            return v.ToString("G8", CultureInfo.InvariantCulture);
        string s = v.ToString("G10", CultureInfo.InvariantCulture);
        // Só remove zeros após o ponto decimal, nunca de números inteiros
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }
}
