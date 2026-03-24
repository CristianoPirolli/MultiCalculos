using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CalculadoraInteligente.Application;
using CalculadoraInteligente.UI.Tools;

namespace CalculadoraInteligente.UI;

public partial class MainWindow : Window
{
    private readonly CalculatorController   _controller  = new();
    private readonly TimeCalculatorEngine   _timeEngine  = new();
    private bool _timeMode = false;

    public MainWindow()
    {
        InitializeComponent();
        RefreshUi();
    }

    private void RefreshUi()
    {
        if (DisplayTextBox is null) return;

        if (_timeMode)
        {
            DisplayTextBox.Text  = _timeEngine.DisplayText;
            MemoryIndicator.Text = _timeEngine.MemoryText;
            HistoryTextBox.Text  = _timeEngine.HistoryText;
            StatusText.Text      = _timeEngine.StatusText;
        }
        else
        {
            DisplayTextBox.Text  = _controller.DisplayText;
            MemoryIndicator.Text = _controller.MemoryText;
            HistoryTextBox.Text  = _controller.HistoryText;
            StatusText.Text      = _controller.StatusText;
        }

        HistoryTextBox.CaretIndex = HistoryTextBox.Text.Length;
        HistoryScrollViewer.ScrollToBottom();
    }

    private void Execute(string command)
    {
        if (_controller.BeepOnType)
            System.Media.SystemSounds.Beep.Play();

        if (_timeMode)
        {
            if (command == "Sign" || command == ",") return;
            _timeEngine.ExecuteCommand(command);
            RefreshUi();
            return;
        }

        _controller.ExecuteCommand(command);
        RefreshUi();

        if (!string.IsNullOrWhiteSpace(_controller.LastErrorMessage))
            MessageBox.Show(this, _controller.LastErrorMessage, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void SetTimeMode(bool active)
    {
        _timeMode = active;
        if (active)
        {
            HeaderTitle.Text       = "â±  CALCULADORA DE TEMPO";
            HeaderTitle.Foreground = new SolidColorBrush(Color.FromRgb(250, 204, 21));
            HeaderBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(234, 179, 8));
        }
        else
        {
            HeaderTitle.Text         = "CALCULADORA INTELIGENTE";
            HeaderTitle.Foreground   = (Brush)FindResource("Accent");
            HeaderBorder.BorderBrush = (Brush)FindResource("BorderBrushSoft");
        }
        RefreshUi();
    }

    private void CommandButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string command)
            Execute(command);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key >= Key.D0 && e.Key <= Key.D9)
        {
            Execute(((int)(e.Key - Key.D0)).ToString(CultureInfo.InvariantCulture));
            return;
        }
        if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        {
            Execute(((int)(e.Key - Key.NumPad0)).ToString(CultureInfo.InvariantCulture));
            return;
        }

        switch (e.Key)
        {
            case Key.Add:
            case Key.OemPlus when Keyboard.Modifiers == ModifierKeys.None:
                Execute("+"); break;
            case Key.Subtract:
            case Key.OemMinus:
                Execute("-"); break;
            case Key.Multiply:
                Execute("*"); break;
            case Key.Divide:
            case Key.Oem2:
                Execute("/"); break;
            case Key.Decimal:
            case Key.OemComma:
            case Key.OemPeriod:
                Execute(","); break;
            case Key.Enter:
                Execute("="); break;
            case Key.Back:
                Execute("Back"); break;
            case Key.Delete:
                Execute("CE"); break;
            case Key.Escape:
                if (_timeMode) _timeEngine.ClearAll();
                else _controller.ClearAll();
                RefreshUi();
                break;
            case Key.F1:
                ShowHelp();
                break;
            case Key.F11:
                ShowAbout();
                break;
            case Key.F12:
                ShowLicense();
                break;
        }
    }

    private void OpenAnotherCalculator_Click(object sender, RoutedEventArgs e)
    {
        var exe = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(exe))
        {
            _controller.MarkNewInstanceFailed();
            RefreshUi();
            return;
        }
        Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
        _controller.MarkNewInstanceOpened();
        RefreshUi();
    }

    private void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        if (_timeMode) _timeEngine.ClearHistory();
        else _controller.ClearHistory();
        RefreshUi();
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void AlwaysOnTop_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            Topmost = item.IsChecked;
            _controller.SetAlwaysOnTop(item.IsChecked);
            RefreshUi();
        }
    }

    private void BeepOnType_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            _controller.SetBeep(item.IsChecked);
            RefreshUi();
        }
    }

    private void CalculoData_Click(object sender, RoutedEventArgs e)        => ShowToolWindow(() => new CalculoData { Owner = this });
    private void ConversorMedidas_Click(object sender, RoutedEventArgs e)   => ShowToolWindow(() => new ConversorMedidas { Owner = this });
    private void JurosCapitalizados_Click(object sender, RoutedEventArgs e) => ShowToolWindow(() => new JurosCapitalizados { Owner = this });
    private void RegrasDeTres_Click(object sender, RoutedEventArgs e)       => ShowToolWindow(() => new RegrasDeTres { Owner = this });
    private void CalculoIMC_Click(object sender, RoutedEventArgs e)         => ShowToolWindow(() => new CalculoIMC { Owner = this });

    private void CalculoTempo_Click(object sender, RoutedEventArgs e) =>
        SetTimeMode(!_timeMode);

    private void Help_Click(object sender, RoutedEventArgs e) => ShowHelp();
    private void About_Click(object sender, RoutedEventArgs e) => ShowAbout();
    private void License_Click(object sender, RoutedEventArgs e) => ShowLicense();
    private void SupportEmail_Click(object sender, RoutedEventArgs e) => OpenSupportEmail();

    private void ShowHelp()
    {
        MessageBox.Show(this,
            "Ajuda rapida:\n" +
            "- F1: abre esta ajuda.\n" +
            "- F11: abre Sobre a Calculadora.\n" +
            "- F12: abre Registro (Licenca de Uso).\n" +
            "- F6: abre outra instancia da calculadora.",
            "Ajuda", MessageBoxButton.OK, MessageBoxImage.Question);
    }

    private void ShowAbout()
    {
        MessageBox.Show(this,
            "Calculadora Inteligente\nArquitetura em camadas (UI/Application/Core).",
            "Sobre a Calculadora", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ShowLicense()
    {
        MessageBox.Show(this,
            "Registro (Licenca de Uso)\nEm avaliacao ate 18/04/2026.",
            "Registro", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenSupportEmail()
    {
        const string supportEmail = "suporte@calculadora-inteligente.local";
        var subject = Uri.EscapeDataString("Suporte - Calculadora Inteligente");
        var body = Uri.EscapeDataString("Descreva aqui sua duvida ou problema.");
        var mailto = $"mailto:{supportEmail}?subject={subject}&body={body}";

        try
        {
            Process.Start(new ProcessStartInfo(mailto) { UseShellExecute = true });
        }
        catch (Exception)
        {
            MessageBox.Show(this,
                $"Nao foi possivel abrir o cliente de e-mail automaticamente.\nContato: {supportEmail}",
                "Suporte", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ShowToolWindow(Func<Window> createWindow)
    {
        try
        {
            createWindow().Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"Nao foi possivel abrir a ferramenta solicitada.\n\nDetalhes: {ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
