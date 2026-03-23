using System.Globalization;

namespace CalculadoraInteligente.Core;

public sealed class CalculatorEngine
{
    private decimal _storedValue;
    private decimal _memoryValue;
    private string _display = "0";
    private string? _pendingOperator;
    private bool _newEntry = true;
    private bool _hasStoredValue;

    public string Display => _display;
    public bool MemoryHasValue => _memoryValue != 0m;

    public string ExpressionPreview => _hasStoredValue && !string.IsNullOrWhiteSpace(_pendingOperator)
        ? $"{Format(_storedValue)} {_pendingOperator} {_display}"
        : _display;

    public void InputDigit(char digit)
    {
        if (_newEntry || _display == "0")
        {
            _display = digit.ToString();
            _newEntry = false;
            return;
        }

        _display += digit;
    }

    public void InputDecimalSeparator()
    {
        var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        if (_newEntry)
        {
            _display = "0" + separator;
            _newEntry = false;
            return;
        }

        if (!_display.Contains(separator, StringComparison.Ordinal))
        {
            _display += separator;
        }
    }

    public void ToggleSign() => _display = Format(ParseDisplay() * -1m);

    public void SetOperator(string op)
    {
        if (_hasStoredValue && !_newEntry && !string.IsNullOrWhiteSpace(_pendingOperator))
        {
            Evaluate();
        }

        _storedValue = ParseDisplay();
        _pendingOperator = op;
        _newEntry = true;
        _hasStoredValue = true;
    }

    public void Evaluate()
    {
        if (!_hasStoredValue || string.IsNullOrWhiteSpace(_pendingOperator))
        {
            return;
        }

        var right = ParseDisplay();
        var result = _pendingOperator switch
        {
            "+" => _storedValue + right,
            "-" => _storedValue - right,
            "*" => _storedValue * right,
            "/" when right == 0m => throw new DivideByZeroException(),
            "/" => _storedValue / right,
            _ => right
        };

        _display = Format(result);
        _storedValue = result;
        _pendingOperator = null;
        _newEntry = true;
        _hasStoredValue = false;
    }

    public void Percent()
    {
        var current = ParseDisplay();
        current = _hasStoredValue ? _storedValue * current / 100m : current / 100m;
        _display = Format(current);
        _newEntry = true;
    }

    public void Backspace()
    {
        if (_newEntry)
        {
            return;
        }

        if (_display.Length <= 1)
        {
            _display = "0";
            _newEntry = true;
            return;
        }

        _display = _display[..^1];
        if (string.IsNullOrWhiteSpace(_display) || _display == "-")
        {
            _display = "0";
            _newEntry = true;
        }
    }

    public void ClearEntry()
    {
        _display = "0";
        _newEntry = true;
    }

    public void ClearAll()
    {
        _display = "0";
        _storedValue = 0m;
        _pendingOperator = null;
        _newEntry = true;
        _hasStoredValue = false;
    }

    public void MemoryClear() => _memoryValue = 0m;

    public void MemoryRecall()
    {
        _display = Format(_memoryValue);
        _newEntry = true;
    }

    public void MemoryAdd()
    {
        _memoryValue += ParseDisplay();
        _newEntry = true;
    }

    public void MemorySubtract()
    {
        _memoryValue -= ParseDisplay();
        _newEntry = true;
    }

    private decimal ParseDisplay()
    {
        return decimal.TryParse(_display, NumberStyles.Number, CultureInfo.CurrentCulture, out var value) ? value : 0m;
    }

    private static string Format(decimal value)
    {
        return value.ToString("0.############################", CultureInfo.CurrentCulture);
    }
}
