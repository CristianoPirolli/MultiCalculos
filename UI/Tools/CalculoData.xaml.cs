using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CalculadoraInteligente.UI.Tools;

public partial class CalculoData : Window
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private int  _dispYear;
    private int  _dispMonth;
    private bool _activeIsInicial = true;  // qual campo recebe clique do calendário

    private DateTime? _dataInicial;
    private DateTime? _dataFinal;

    // 42 botões reutilizáveis do calendário
    private readonly Button[] _dayBtns = new Button[42];

    public CalculoData()
    {
        InitializeComponent();
        _dispYear  = DateTime.Today.Year;
        _dispMonth = DateTime.Today.Month;

        // Inicializar botões do calendário
        for (int i = 0; i < 42; i++)
        {
            var btn = new Button { Style = (Style)FindResource("DayBtnStyle") };
            btn.Click += DayBtn_Click;
            _dayBtns[i] = btn;
            DaysGrid.Children.Add(btn);
        }

        // Preencher datas iniciais
        TbDataInicial.Text = DateTime.Today.ToString("dd/MM/yyyy");
        TbDataFinal.Text   = DateTime.Today.ToString("dd/MM/yyyy");

        AtualizarBanner();
        RenderCalendar();
    }

    // ── Banner hoje ────────────────────────────────────────────────────────

    private void AtualizarBanner()
    {
        var hoje = DateTime.Today;
        TbHoje1.Text    = $"Hoje: {hoje:dd/MM/yyyy}";
        TbHoje2.Text    = hoje.ToString("dddd, dd 'de' MMMM 'de' yyyy", PtBr);
        TbTodayFooter.Text = $"Today: {hoje:dd/MM/yyyy}";
    }

    // ── Eventos dos campos de data ────────────────────────────────────────

    private void DataInicial_GotFocus(object sender, RoutedEventArgs e) =>
        _activeIsInicial = true;

    private void DataFinal_GotFocus(object sender, RoutedEventArgs e) =>
        _activeIsInicial = false;

    private void DataInicial_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (TryParseDate(TbDataInicial.Text, out var d))
        {
            _dataInicial = d;
            TbDiaSemanInicial.Text = d.ToString("dddd", PtBr);
            // Navegar o calendário para essa data
            _dispYear = d.Year; _dispMonth = d.Month;
            RenderCalendar();
            TryAutoCalculateDiff();
        }
        else
        {
            _dataInicial = null;
            TbDiaSemanInicial.Text = "";
        }
    }

    private void DataFinal_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (TryParseDate(TbDataFinal.Text, out var d))
        {
            _dataFinal = d;
            TbDiaSemanFinal.Text = d.ToString("dddd", PtBr);
            RenderCalendar();
            TryAutoCalculateDiff();
        }
        else
        {
            _dataFinal = null;
            TbDiaSemanFinal.Text = "";
            ClearUnits();
        }
    }

    // ── Calendário ────────────────────────────────────────────────────────

    private void RenderCalendar()
    {
        var firstDay   = new DateTime(_dispYear, _dispMonth, 1);
        int startOffset = (int)firstDay.DayOfWeek; // domingo = 0
        int daysInMonth = DateTime.DaysInMonth(_dispYear, _dispMonth);
        var today       = DateTime.Today;

        TbMesAno.Text = firstDay.ToString("MMMM yyyy", PtBr);

        int idx = 0;

        // Dias do mês anterior
        var prevLast = firstDay.AddDays(-1);
        for (int i = startOffset - 1; i >= 0; i--)
            ConfigureDay(idx++, prevLast.AddDays(-i), otherMonth: true, today);

        // Dias do mês atual
        for (int d = 1; d <= daysInMonth; d++)
            ConfigureDay(idx++, new DateTime(_dispYear, _dispMonth, d), otherMonth: false, today);

        // Dias do próximo mês
        var nextFirst = firstDay.AddMonths(1);
        int remain = 42 - idx;
        for (int i = 0; i < remain; i++)
            ConfigureDay(idx++, nextFirst.AddDays(i), otherMonth: true, today);
    }

    private void ConfigureDay(int idx, DateTime date, bool otherMonth, DateTime today)
    {
        var btn = _dayBtns[idx];
        btn.Content    = date.Day.ToString();
        btn.Tag        = date;
        btn.Visibility = Visibility.Visible;

        bool isInicial = _dataInicial.HasValue && date == _dataInicial.Value;
        bool isFinal   = _dataFinal.HasValue   && date == _dataFinal.Value;
        bool inRange   = _dataInicial.HasValue && _dataFinal.HasValue
                         && date > _dataInicial.Value && date < _dataFinal.Value;
        bool isToday   = date == today;

        if (isInicial || isFinal)
        {
            btn.Background   = new SolidColorBrush(Color.FromRgb(0, 84, 227));   // XP blue
            btn.Foreground   = Brushes.White;
            btn.BorderBrush  = new SolidColorBrush(Color.FromRgb(0, 48, 112));
            btn.Opacity      = 1.0;
        }
        else if (inRange)
        {
            btn.Background   = new SolidColorBrush(Color.FromRgb(184, 200, 220)); // azul-aço claro
            btn.Foreground   = new SolidColorBrush(Color.FromRgb(0, 0, 128));     // navy
            btn.BorderBrush  = Brushes.Transparent;
            btn.Opacity      = 1.0;
        }
        else if (isToday)
        {
            btn.Background   = Brushes.Transparent;
            btn.Foreground   = Brushes.Black;
            btn.BorderBrush  = new SolidColorBrush(Color.FromRgb(200, 0, 0));     // borda vermelha
            btn.Opacity      = 1.0;
        }
        else if (otherMonth)
        {
            btn.Background   = Brushes.Transparent;
            btn.Foreground   = new SolidColorBrush(Color.FromRgb(160, 160, 160)); // cinza médio
            btn.BorderBrush  = Brushes.Transparent;
            btn.Opacity      = 1.0;
        }
        else
        {
            btn.Background   = Brushes.Transparent;
            btn.Foreground   = Brushes.Black;                                      // preto legível
            btn.BorderBrush  = Brushes.Transparent;
            btn.Opacity      = 1.0;
        }
    }

    private void DayBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is DateTime date)
        {
            var text = date.ToString("dd/MM/yyyy");
            if (_activeIsInicial)
                TbDataInicial.Text = text;
            else
                TbDataFinal.Text = text;
        }
    }

    private void PrevMonth_Click(object sender, RoutedEventArgs e)
    {
        if (--_dispMonth < 1) { _dispMonth = 12; _dispYear--; }
        RenderCalendar();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        if (++_dispMonth > 12) { _dispMonth = 1; _dispYear++; }
        RenderCalendar();
    }

    // ── Cálculo automático da diferença ───────────────────────────────────

    private void TryAutoCalculateDiff()
    {
        if (!_dataInicial.HasValue || !_dataFinal.HasValue) return;

        var ini = _dataInicial.Value;
        var fim = _dataFinal.Value;
        if (fim < ini) (ini, fim) = (fim, ini);

        int totalDias   = (fim - ini).Days;
        int totalSemanas = totalDias / 7;

        int anos = 0; var tmp = ini;
        while (tmp.AddYears(1) <= fim) { anos++; tmp = tmp.AddYears(1); }
        int meses = 0;
        while (tmp.AddMonths(1) <= fim) { meses++; tmp = tmp.AddMonths(1); }

        TbAnos.Text    = anos.ToString();
        TbMeses.Text   = meses.ToString();
        TbSemanas.Text = totalSemanas.ToString();
        TbDias.Text    = totalDias.ToString();
    }

    private void ClearUnits()
    {
        TbAnos.Text = TbMeses.Text = TbSemanas.Text = TbDias.Text = "";
    }

    // ── Botão Calcular Data Final ─────────────────────────────────────────

    private void CalcularDataFinal_Click(object sender, RoutedEventArgs e)
    {
        if (!_dataInicial.HasValue)
        {
            MessageBox.Show(this, "Informe a Data Inicial primeiro.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = _dataInicial.Value;
        if (int.TryParse(TbAnos.Text,    out int a) && a != 0) result = result.AddYears(a);
        if (int.TryParse(TbMeses.Text,   out int m) && m != 0) result = result.AddMonths(m);
        if (int.TryParse(TbSemanas.Text, out int s) && s != 0) result = result.AddDays(s * 7);
        if (int.TryParse(TbDias.Text,    out int d) && d != 0) result = result.AddDays(d);

        TbDataFinal.Text = result.ToString("dd/MM/yyyy");
        _dispYear = result.Year; _dispMonth = result.Month;
        RenderCalendar();
    }

    // ── Botões de unidade (calculam o total naquela unidade) ──────────────

    private void BtnAnos_Click(object sender, RoutedEventArgs e)    => ShowUnit("anos");
    private void BtnMeses_Click(object sender, RoutedEventArgs e)   => ShowUnit("meses");
    private void BtnSemanas_Click(object sender, RoutedEventArgs e) => ShowUnit("semanas");
    private void BtnDias_Click(object sender, RoutedEventArgs e)    => ShowUnit("dias");

    private void ShowUnit(string unit)
    {
        if (!_dataInicial.HasValue || !_dataFinal.HasValue) return;
        var ini = _dataInicial.Value; var fim = _dataFinal.Value;
        if (fim < ini) (ini, fim) = (fim, ini);
        int totalDias = (fim - ini).Days;

        string msg = unit switch
        {
            "anos"    => $"{totalDias / 365.25:N2} anos  ({totalDias} dias)",
            "meses"   => $"{totalDias / 30.44:N2} meses  ({totalDias} dias)",
            "semanas" => $"{totalDias / 7} semanas  ({totalDias} dias)",
            "dias"    => $"{totalDias} dias",
            _         => ""
        };

        MessageBox.Show(this, msg, "Diferença", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── Utilitário ────────────────────────────────────────────────────────

    private static bool TryParseDate(string text, out DateTime date) =>
        DateTime.TryParseExact(text.Trim(), "dd/MM/yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}
