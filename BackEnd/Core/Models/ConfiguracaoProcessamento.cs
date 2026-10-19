namespace Core.Models;

public class ConfiguracaoProcessamento
{
    public int AtrasoPorNotificacaoEmMilissegundos { get; set; } = 700;

    public int QuantidadeMaxima { get; set; } = 20;
}
