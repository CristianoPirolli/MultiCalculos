using System.Globalization;
using System.Text;

namespace CalculadoraInteligente.Application;

/// <summary>
/// Motor da calculadora de tempo — comportamento de "fita somadora":
///   • Ao digitar o 2º número com operação pendente, o display mostra
///     o total acumulado em tempo real (acumulador OP digitos_atuais).
///   • = entrega o resultado no histórico e zera o display.
///   • = duas vezes seguidas → limpa tudo.
///   • Operador após = → continua do resultado anterior.
/// </summary>
public sealed class TimeCalculatorEngine
{
    private readonly List<int> _digits = new();   // buffer de entrada (máx 6 → HHMMSS)

    private TimeSpan _accumulator = TimeSpan.Zero; // total acumulado
    private char?    _pendingOp   = null;          // operação aguardando segundo operando
    private bool     _justEqualed = false;         // última ação foi "="

    private TimeSpan _memory    = TimeSpan.Zero;
    private bool     _hasMemory = false;

    private readonly StringBuilder _history = new();
    private int _opCount = 0;

    // ── Propriedades públicas ──────────────────────────────────────────────

    public string DisplayText { get; private set; } = "00:00:00";
    public string MemoryText  => _hasMemory ? "MEM" : "000";
    public string HistoryText => _history.ToString();
    public string StatusText  { get; private set; } = "Modo Tempo ativo.";

    public TimeCalculatorEngine()
    {
        AddHeader();
        UpdateDisplay();
    }

    // ── Ponto de entrada ───────────────────────────────────────────────────

    public void ExecuteCommand(string cmd)
    {
        switch (cmd)
        {
            case "0": case "1": case "2": case "3": case "4":
            case "5": case "6": case "7": case "8": case "9":
                Digit(int.Parse(cmd)); break;

            case "00": Digit(0); Digit(0); break;

            case "Back":
                if (_digits.Count > 0) { _digits.RemoveAt(_digits.Count - 1); UpdateDisplay(); }
                StatusText = "Digito removido.";
                break;

            case "CE":
                _digits.Clear(); _justEqualed = false;
                DisplayText = "00:00:00"; StatusText = "Entrada limpa.";
                break;

            case "C": ClearAll(); break;

            case "+": case "-": case "*": case "/":
                Operator(cmd[0]); break;

            case "=": Equals(); break;

            case "%": Percent(); break;

            case "MC": _memory = TimeSpan.Zero; _hasMemory = false;   StatusText = "Memoria limpa.";        break;
            case "MR": if (_hasMemory) { LoadDigits(_memory); UpdateDisplay(); } StatusText = "Memoria carregada.";  break;
            case "M+": _memory += BufferTime(); _hasMemory = true;    StatusText = "Somado na memoria.";    break;
            case "M-": _memory -= BufferTime(); _hasMemory = true;    StatusText = "Subtraido da memoria."; break;
        }
    }

    // ── Dígitos ────────────────────────────────────────────────────────────

    private void Digit(int d)
    {
        // Após = : próximo dígito começa número novo (ignora resultado anterior)
        if (_justEqualed)
        {
            _digits.Clear();
            _accumulator = TimeSpan.Zero;
            _pendingOp   = null;
            _justEqualed = false;
        }

        if (_digits.Count >= 6) return;
        _digits.Add(d);
        UpdateDisplay();
    }

    // ── Operadores ─────────────────────────────────────────────────────────

    private void Operator(char op)
    {
        _justEqualed = false;

        if (_pendingOp.HasValue && _digits.Count > 0)
        {
            // Já havia pendência: commita (ex: 30+ , digitou 20, agora pressiona +)
            _accumulator = Calc(_accumulator, BufferTime(), _pendingOp.Value);
        }
        else if (!_pendingOp.HasValue && _digits.Count > 0)
        {
            // Primeiro operando
            _accumulator = BufferTime();
        }
        // else: continuando de resultado anterior (digits=0) — accumulator já está correto

        _history.AppendLine($"  {Fmt(_accumulator)} {OpStr(op)}");
        _pendingOp = op;
        _digits.Clear();
        UpdateDisplay();
        StatusText = "Aguardando proximo valor...";
    }

    // ── Igual ──────────────────────────────────────────────────────────────

    private void Equals()
    {
        // = duas vezes seguidas sem nada digitado → novo cálculo
        if (_justEqualed && _digits.Count == 0 && !_pendingOp.HasValue)
        {
            ClearAll();
            return;
        }

        if (_pendingOp.HasValue)
        {
            if (_digits.Count > 0)
            {
                var rhs = BufferTime();
                _history.AppendLine($"  {Fmt(rhs)} =");
                _accumulator = Calc(_accumulator, rhs, _pendingOp.Value);
            }

            _opCount++;
            _history.AppendLine($"{_opCount:D3}");
            _history.AppendLine($"  {Fmt(_accumulator)} T");
            _history.AppendLine(new string('=', 26));
            _history.AppendLine();
        }
        else
        {
            _accumulator = BufferTime();
        }

        _pendingOp   = null;
        _justEqualed = true;
        _digits.Clear();
        UpdateDisplay();   // → "00:00:00"
        StatusText = "Resultado no historico.";
    }

    // ── Display em tempo real ──────────────────────────────────────────────

    private void UpdateDisplay()
    {
        if (_digits.Count == 0)
        {
            // Sem dígitos: mostra acumulador se há operação, senão zeros
            DisplayText = _pendingOp.HasValue ? Fmt(_accumulator) : "00:00:00";
        }
        else
        {
            // Sempre mostra o que está sendo digitado — não antecipa o resultado
            DisplayText = BufferToHms();
        }
    }

    // ── Utilidades ─────────────────────────────────────────────────────────

    public void ClearAll()
    {
        _digits.Clear(); _accumulator = TimeSpan.Zero;
        _pendingOp = null; _justEqualed = false;
        UpdateDisplay(); StatusText = "Pronto.";
    }

    public void ClearHistory()
    {
        _history.Clear(); _opCount = 0;
        AddHeader(); StatusText = "Historico limpo.";
    }

    private void Percent()
    {
        long sec = (long)(_accumulator.TotalSeconds * BufferTime().TotalSeconds / 100.0);
        _digits.Clear(); LoadDigitsFromSeconds(sec); UpdateDisplay();
    }

    private TimeSpan BufferTime()
    {
        int[] p = Padded();
        return TimeSpan.FromSeconds(p[0] * 36000 + p[1] * 3600 + p[2] * 600 + p[3] * 60 + p[4] * 10 + p[5]);
    }

    private string BufferToHms()
    {
        int[] p = Padded();
        return $"{p[0]}{p[1]}:{p[2]}{p[3]}:{p[4]}{p[5]}";
    }

    private int[] Padded()
    {
        var arr = new int[6];
        int s   = 6 - _digits.Count;
        for (int i = 0; i < _digits.Count; i++) arr[s + i] = _digits[i];
        return arr;
    }

    private void LoadDigits(TimeSpan t)             => LoadDigitsFromSeconds((long)Math.Abs(t.TotalSeconds));
    private void LoadDigitsFromSeconds(long sec)
    {
        long h = sec / 3600; long m = (sec % 3600) / 60; long s = sec % 60;
        string raw = $"{h:D2}{m:D2}{s:D2}".TrimStart('0');
        _digits.Clear();
        foreach (char c in raw) _digits.Add(c - '0');
    }

    private static TimeSpan Calc(TimeSpan lhs, TimeSpan rhs, char op) => op switch
    {
        '+' => lhs + rhs,
        '-' => lhs - rhs,
        '*' => TimeSpan.FromSeconds(lhs.TotalSeconds * rhs.TotalSeconds),
        '/' => rhs.TotalSeconds > 0 ? TimeSpan.FromSeconds(lhs.TotalSeconds / rhs.TotalSeconds) : lhs,
        _   => lhs
    };

    public static string Fmt(TimeSpan t)
    {
        long sec = (long)Math.Abs(t.TotalSeconds);
        return $"{(t.TotalSeconds < 0 ? "-" : "")}{sec / 3600:D2}:{sec % 3600 / 60:D2}:{sec % 60:D2}";
    }

    private static string OpStr(char op) => op switch { '+' => "+", '-' => "-", '*' => "x", '/' => "÷", _ => op.ToString() };

    private void AddHeader()
    {
        _history.AppendLine(DateTime.Now.ToString("dd/MM/yyyy ddd", new CultureInfo("pt-BR")));
        _history.AppendLine(new string('=', 26));
        _history.AppendLine();
    }
}
