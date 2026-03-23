using System.Globalization;
using System.Text;
using CalculadoraInteligente.Core;

namespace CalculadoraInteligente.Application;

public sealed class CalculatorController
{
    private readonly CalculatorEngine _engine = new();
    private readonly StringBuilder _history = new();

    public CalculatorController()
    {
        BeepOnType = true;
        StatusText = "Pronto";
        AddHeader();
    }

    public bool BeepOnType { get; set; }
    public string DisplayText => _engine.Display;
    public string MemoryText => _engine.MemoryHasValue ? "MEM" : "000";
    public string HistoryText => _history.ToString();
    public string StatusText { get; private set; }
    public string? LastErrorMessage { get; private set; }

    public void ExecuteCommand(string command)
    {
        LastErrorMessage = null;

        try
        {
            var isEquals = command == "=";
            var expression = _engine.ExpressionPreview;

            switch (command)
            {
                case "MC":
                    _engine.MemoryClear();
                    StatusText = "Memoria limpa.";
                    break;
                case "MR":
                    _engine.MemoryRecall();
                    StatusText = "Memoria carregada.";
                    break;
                case "M+":
                    _engine.MemoryAdd();
                    StatusText = "Valor somado na memoria.";
                    break;
                case "M-":
                    _engine.MemorySubtract();
                    StatusText = "Valor subtraido da memoria.";
                    break;
                case "CE":
                    _engine.ClearEntry();
                    StatusText = "Entrada limpa.";
                    break;
                case "Back":
                    _engine.Backspace();
                    StatusText = "Ultimo digito removido.";
                    break;
                case "Sign":
                    _engine.ToggleSign();
                    StatusText = "Sinal invertido.";
                    break;
                case ",":
                    _engine.InputDecimalSeparator();
                    break;
                case "+":
                case "-":
                case "*":
                case "/":
                    _engine.SetOperator(command);
                    break;
                case "%":
                    _engine.Percent();
                    StatusText = "Percentual aplicado.";
                    break;
                case "=":
                    _engine.Evaluate();
                    StatusText = "Resultado calculado.";
                    break;
                default:
                    if (command.Length == 1 && char.IsDigit(command[0]))
                    {
                        _engine.InputDigit(command[0]);
                    }

                    break;
            }

            if (isEquals)
            {
                _history.AppendLine($"> {expression} = {_engine.Display}");
            }
        }
        catch (DivideByZeroException)
        {
            StatusText = "Erro: divisao por zero.";
            LastErrorMessage = "Divisao por zero nao e permitida.";
            _engine.ClearAll();
        }
    }

    public void ClearAll()
    {
        _engine.ClearAll();
        StatusText = "Calculadora reiniciada.";
    }

    public void ClearHistory()
    {
        _history.Clear();
        AddHeader();
        StatusText = "Historico limpo.";
    }

    public void SetAlwaysOnTop(bool isEnabled)
    {
        StatusText = isEnabled ? "Janela fixada no topo." : "Janela liberada.";
    }

    public void SetBeep(bool isEnabled)
    {
        BeepOnType = isEnabled;
        StatusText = BeepOnType ? "Som ativado." : "Som desativado.";
    }

    public void MarkNewInstanceOpened()
    {
        StatusText = "Nova instancia aberta.";
    }

    public void MarkNewInstanceFailed()
    {
        StatusText = "Nao foi possivel abrir outra instancia.";
    }

    private void AddHeader()
    {
        var culture = new CultureInfo("pt-BR");
        _history.AppendLine(DateTime.Now.ToString("dd/MM/yyyy ddd", culture));
        _history.AppendLine("========================");
        _history.AppendLine();
    }
}
