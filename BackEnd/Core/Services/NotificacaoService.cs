using System.Diagnostics;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

namespace Core.Services;

public class NotificacaoService : INotificacaoService
{
    private readonly INotificacaoRepository _notificacaoRepository;
    private readonly ConfiguracaoProcessamento _configuracao;

    public NotificacaoService(
        INotificacaoRepository notificacaoRepository,
        ConfiguracaoProcessamento configuracao)
    {
        _notificacaoRepository = notificacaoRepository;
        _configuracao = configuracao;
    }

    public ResultadoProcessamento ProcessarLote(int quantidade)
    {
        ValidarQuantidade(quantidade);

        Stopwatch cronometro = Stopwatch.StartNew();

        for (int indice = 1; indice <= quantidade; indice++)
        {
            ProcessarNotificacao();
        }

        cronometro.Stop();

        return new ResultadoProcessamento
        {
            QuantidadeProcessada = quantidade,
            TempoTotalEmMilissegundos = cronometro.ElapsedMilliseconds
        };
    }

    private void ProcessarNotificacao()
    {
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            CriadaEm = DateTime.Now
        };

        // Bloqueio intencional para mostrar um dos problemas da programação síncrona
        // A Thread pode ficar travada durante o prossamento de uma mensagem
        Thread.Sleep(_configuracao.AtrasoPorNotificacaoEmMilissegundos);

        notificacao.Status = EstadoNotificacao.Processada;
        notificacao.ProcessadaEm = DateTime.Now;
        _notificacaoRepository.Adicionar(notificacao);
    }

    private void ValidarQuantidade(int quantidade)
    {
        if (quantidade < 1)
        {
            throw new ArgumentException("Informe pelo menos uma notificação.");
        }

        if (quantidade > _configuracao.QuantidadeMaxima)
        {
            throw new ArgumentException(
                $"A quantidade máxima permitida é {_configuracao.QuantidadeMaxima} notificações.");
        }
    }
}
