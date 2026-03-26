using System.Windows;
using System.Windows.Input;

namespace CalculadoraInteligente.UI.Tools;

public partial class ManualAjuda : Window
{
    private readonly HelpPage[] _pages =
    [
        new(
            "Comece por aqui",
            "Esta ajuda é dividida em páginas curtas. Use os botões na parte de baixo para avançar ou voltar.\n\n" +
            "Ferramentas disponíveis:\n" +
            "• Calculadora principal\n" +
            "• Calcular Data\n" +
            "• Calculadora de Tempo\n" +
            "• Conversor de Medidas\n" +
            "• Ajuste de Valor\n" +
            "• Juros Capitalizados\n" +
            "• Regra de Três\n" +
            "• Peso Ideal / IMC\n\n" +
            "Atalhos úteis:\n" +
            "F1 abre este manual\n" +
            "F6 abre outra calculadora\n" +
            "F11 abre a janela Sobre\n" +
            "F12 abre o registro"
        ),
        new(
            "Visão geral",
            "Esta calculadora reúne funções do dia a dia em uma interface simples.\n\n" +
            "Você pode usar a calculadora principal para contas rápidas e abrir ferramentas extras pelo menu Opções."
        ),
        new(
            "Calculadora principal",
            "A tela principal serve para contas comuns.\n\n" +
            "Botões numéricos digitam valores.\n" +
            "+, -, × e ÷ fazem as operações básicas.\n" +
            "% aplica percentual sobre o valor atual.\n" +
            "CE limpa a entrada atual.\n" +
            "Back apaga o último dígito.\n" +
            "M+, M-, MR e MC controlam a memória."
        ),
        new(
            "Calcular data e tempo",
            "Calcular Data ajuda a descobrir diferença entre datas ou projetar uma data final.\n\n" +
            "Calculadora de Tempo trabalha com horas, minutos e segundos. Ela é útil para somar ou subtrair tempos e registrar o histórico das contas."
        ),
        new(
            "Conversões e percentuais",
            "Conversor de Medidas transforma valores entre unidades.\n\n" +
            "Ajuste de Valor calcula aumento ou redução. Você informa o valor base, escolhe aumentar ou reduzir e pode digitar em porcentagem ou em valor. O sistema mostra o valor do ajuste e o valor final.\n\n" +
            "Regra de Três resolve proporções simples."
        ),
        new(
            "Juros e IMC",
            "Juros Capitalizados ajuda a simular parcelas, juros e valor final.\n\n" +
            "Peso Ideal / IMC calcula a faixa de peso com base na altura, no peso atual e no sexo informado."
        ),
        new(
            "Configurações e suporte",
            "Sempre Visível mantém a calculadora na frente das outras janelas.\n" +
            "Som ao Digitar liga ou desliga o aviso sonoro.\n\n" +
            "Limpar Histórico apaga os registros da calculadora atual.\n" +
            "Enviar e-mail ao suporte abre o contato de ajuda."
        )
    ];

    private int _pageIndex;

    public ManualAjuda()
    {
        InitializeComponent();
        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        _pageIndex = index;
        var page = _pages[_pageIndex];

        TbPagina.Text = $"Página {_pageIndex + 1} de {_pages.Length}";
        TbTitulo.Text = page.Title;
        TbConteudo.Text = page.Content;

        BtnAnterior.IsEnabled = _pageIndex > 0;
        BtnProxima.IsEnabled = _pageIndex < _pages.Length - 1;
    }

    private void Anterior_Click(object sender, RoutedEventArgs e)
    {
        if (_pageIndex > 0)
            ShowPage(_pageIndex - 1);
    }

    private void Proxima_Click(object sender, RoutedEventArgs e)
    {
        if (_pageIndex < _pages.Length - 1)
            ShowPage(_pageIndex + 1);
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Right:
            case Key.PageDown:
                Proxima_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.Left:
            case Key.PageUp:
                Anterior_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }

        base.OnKeyDown(e);
    }

    private sealed record HelpPage(string Title, string Content);
}
