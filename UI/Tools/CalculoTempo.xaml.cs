using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CalculadoraInteligente.UI.Tools;

public partial class CalculoTempo : Window
{
    // Buffer de dígitos digitados (máx 6 = HHMMSS)
    private readonly List<int> _digits = new();

    // Acumulador e operação pendente
    private TimeSpan _accumulator = TimeSpan.Zero;
    private char?    _pendingOp   = null;
    private bool     _afterEquals = false;

    // Memória
    private TimeSpan _memory    = TimeSpan.Zero;
    private bool     _hasMemory = false;

    // Histórico
    private readonly StringBuilder _history = new();
    private int _opCount = 0;

    public CalculoTempo()
    {
        InitializeComponent();
        _history.AppendLine(DateTime.Now.ToString("dd/MM/yyyy ddd").ToLower());
        _history.AppendLine(new string('=', 28));
        RefreshUi();
    }

    // ── Entrada de dígitos ─────────────────────────────────────────────────

    private void PushDigit(int d)
    {
        if (_afterEquals) { _digits.Clear(); _afterEquals = false; }
        if (_digits.Count >= 6) return;
        _digits.Add(d);
        RefreshDisplay();
    }

    private void PushDoubleZero()
    {
        PushDigit(0);
        PushDigit(0);
    }

    private void PopDigit()
    {
        if (_digits.Count > 0) _digits.RemoveAt(_digits.Count - 1);
        RefreshDisplay();
    }

    private void ClearEntry()
    {
        _digits.Clear();
        TimeDisplay.Text = "00:00:00";
    }

    private void ClearAll()
    {
        _digits.Clear();
        _accumulator = TimeSpan.Zero;
        _pendingOp   = null;
        _afterEquals = false;
        RefreshDisplay();
    }

    // ── Operações ──────────────────────────────────────────────────────────

    private void ApplyOperator(char op)
    {
        var current = DigitsToTime();

        if (_pendingOp.HasValue)
            CommitOperation(current);
        else
            _accumulator = current;

        _pendingOp   = op;
        _afterEquals = false;
        _digits.Clear();
        RefreshDisplay();
    }

    private void Equals()
    {
        var current = DigitsToTime();

        if (_pendingOp.HasValue)
        {
            CommitOperation(current);
            _pendingOp   = null;
            _afterEquals = true;
        }
        else
        {
            _accumulator = current;
        }

        _digits.Clear();
        RefreshDisplay();
    }

    private void CommitOperation(TimeSpan rhs)
    {
        string lhsStr  = FormatTime(_accumulator);
        string rhsStr  = FormatTime(rhs);
        string opStr   = _pendingOp.ToString()!;

        _accumulator = _pendingOp switch
        {
            '+' => _accumulator + rhs,
            '-' => _accumulator - rhs,
            '*' => TimeSpan.FromSeconds(_accumulator.TotalSeconds * rhs.TotalSeconds),
            '/' => rhs.TotalSeconds != 0
                       ? TimeSpan.FromSeconds(_accumulator.TotalSeconds / rhs.TotalSeconds)
                       : _accumulator,
            _   => _accumulator
        };

        _opCount++;
        _history.AppendLine($"  {lhsStr} {opStr}");
        _history.AppendLine($"  {rhsStr} =");
        _history.AppendLine($"{_opCount:D3}");
        _history.AppendLine($"  {FormatTime(_accumulator)} T");
        _history.AppendLine(new string('=', 28));

        RefreshUi();
    }

    private void Percent()
    {
        // Converte buffer como percentual do acumulador
        var current = DigitsToTime();
        double pct  = current.TotalSeconds / 100.0;
        _digits.Clear();
        long totalSec = (long)(_accumulator.TotalSeconds * pct);
        FillDigitsFromSeconds(totalSec);
        RefreshDisplay();
    }

    // ── Memória ────────────────────────────────────────────────────────────

    private void MC() { _memory = TimeSpan.Zero; _hasMemory = false; RefreshMem(); }
    private void MR() { if (_hasMemory) { FillDigitsFromTime(_memory); RefreshDisplay(); } }
    private void MPlus()  { _memory += DigitsToTime(); _hasMemory = true; RefreshMem(); }
    private void MMinus() { _memory -= DigitsToTime(); _hasMemory = true; RefreshMem(); }

    // ── Conversões ────────────────────────────────────────────────────────

    private TimeSpan DigitsToTime()
    {
        int[] p = Padded();
        int h = p[0] * 10 + p[1];
        int m = p[2] * 10 + p[3];
        int s = p[4] * 10 + p[5];
        // normaliza
        int total = h * 3600 + m * 60 + s;
        return TimeSpan.FromSeconds(total);
    }

    private int[] Padded()
    {
        var arr = new int[6];
        int start = 6 - _digits.Count;
        for (int i = 0; i < _digits.Count; i++) arr[start + i] = _digits[i];
        return arr;
    }

    private void FillDigitsFromTime(TimeSpan t)
    {
        long sec = (long)Math.Abs(t.TotalSeconds);
        FillDigitsFromSeconds(sec);
    }

    private void FillDigitsFromSeconds(long sec)
    {
        long h = sec / 3600; long m = (sec % 3600) / 60; long s = sec % 60;
        string raw = $"{h:D2}{m:D2}{s:D2}".TrimStart('0');
        _digits.Clear();
        foreach (char c in raw) _digits.Add(c - '0');
    }

    private static string FormatTime(TimeSpan t)
    {
        long sec  = (long)Math.Abs(t.TotalSeconds);
        long h    = sec / 3600;
        long m    = (sec % 3600) / 60;
        long s    = sec % 60;
        string sg = t.TotalSeconds < 0 ? "-" : "";
        return $"{sg}{h:D2}:{m:D2}:{s:D2}";
    }

    private string BufferToDisplayString()
    {
        int[] p = Padded();
        return $"{p[0]}{p[1]}:{p[2]}{p[3]}:{p[4]}{p[5]}";
    }

    // ── Refresh ───────────────────────────────────────────────────────────

    private void RefreshDisplay()
    {
        TimeDisplay.Text = _digits.Count == 0 && _pendingOp.HasValue
            ? FormatTime(_accumulator)
            : _afterEquals
                ? FormatTime(_accumulator)
                : BufferToDisplayString();
    }

    private void RefreshMem()
    {
        MemDisplay.Text = _hasMemory ? FormatTime(_memory).Replace(":", "") : "000";
    }

    private void RefreshUi()
    {
        RefreshDisplay();
        RefreshMem();
        HistBox.Text = _history.ToString();
        HistBox.CaretIndex = HistBox.Text.Length;
        HistScroll.ScrollToBottom();
    }

    // ── Dispatcher de comandos ────────────────────────────────────────────

    private void Execute(string cmd)
    {
        switch (cmd)
        {
            case "0": case "1": case "2": case "3": case "4":
            case "5": case "6": case "7": case "8": case "9":
                PushDigit(int.Parse(cmd)); break;
            case "00":   PushDoubleZero(); break;
            case "Back": PopDigit();       break;
            case "CE":   ClearEntry();     break;
            case "C":    ClearAll();       break;
            case "+": case "-": case "*": case "/":
                ApplyOperator(cmd[0]); break;
            case "=":  Equals();  break;
            case "%":  Percent(); break;
            case "MC": MC();      break;
            case "MR": MR();      break;
            case "M+": MPlus();   break;
            case "M-": MMinus();  break;
        }
    }

    private void Btn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is string tag)
            Execute(tag);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key >= Key.D0 && e.Key <= Key.D9) { Execute(((int)(e.Key - Key.D0)).ToString()); return; }
        if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) { Execute(((int)(e.Key - Key.NumPad0)).ToString()); return; }

        switch (e.Key)
        {
            case Key.Add:      Execute("+");    break;
            case Key.Subtract: Execute("-");    break;
            case Key.Multiply: Execute("*");    break;
            case Key.Divide:   Execute("/");    break;
            case Key.Enter:    Execute("=");    break;
            case Key.Back:     Execute("Back"); break;
            case Key.Delete:   Execute("CE");   break;
            case Key.Escape:   Execute("C");    break;
        }
    }
}
