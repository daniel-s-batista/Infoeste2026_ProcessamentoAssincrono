using System.Diagnostics;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

namespace Core.Services;

public class NotificacaoService : INotificacaoService
{
    private readonly INotificacaoRepository _notificacaoRepository;
    private readonly INotificacaoPublisher _notificacaoPublisher;
    private readonly ConfiguracaoProcessamento _configuracao;

    public NotificacaoService(
        INotificacaoRepository notificacaoRepository,
        ConfiguracaoProcessamento configuracao)
    {
        _notificacaoRepository = notificacaoRepository;
        _notificacaoPublisher = null;
        _configuracao = configuracao;
    }

    public NotificacaoService(
        INotificacaoRepository notificacaoRepository,
        INotificacaoPublisher notificacaoPublisher,
        ConfiguracaoProcessamento configuracao)
    {
        _notificacaoRepository = notificacaoRepository;
        _notificacaoPublisher = notificacaoPublisher;
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

    public async Task<ResultadoProcessamento> ProcessarLoteAsync(int quantidade)
    {
        ValidarQuantidade(quantidade);

        Stopwatch cronometro = Stopwatch.StartNew();

        for (int indice = 1; indice <= quantidade; indice++)
        {
            await ProcessarNotificacaoAsync();
        }

        cronometro.Stop();

        return new ResultadoProcessamento
        {
            QuantidadeProcessada = quantidade,
            TempoTotalEmMilissegundos = cronometro.ElapsedMilliseconds
        };
    }

    private async Task ProcessarNotificacaoAsync()
    {
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            CriadaEm = DateTime.Now
        };

        await Task.Delay(_configuracao.AtrasoPorNotificacaoEmMilissegundos);

        notificacao.Status = EstadoNotificacao.Processada;
        notificacao.ProcessadaEm = DateTime.Now;
        _notificacaoRepository.Adicionar(notificacao);
    }

    public async Task ProcessarNotificacaoAsync(MensagemServiceBus mensagem)
    {
        var notificacao = new Notificacao
        {
            Id = mensagem.Id,
            CriadaEm = mensagem.CriadaEm
        };

        await Task.Delay(_configuracao.AtrasoPorNotificacaoEmMilissegundos);

        notificacao.Status = EstadoNotificacao.Processada;
        notificacao.ProcessadaEm = DateTime.Now;
        _notificacaoRepository.Adicionar(notificacao);
    }

    public List<MensagemServiceBus> CriarMensagens(int quantidade)
    {
        ValidarQuantidade(quantidade);

        var mensagens = new List<MensagemServiceBus>();

        for (int i = 0; i < quantidade; i++)
        {
            mensagens.Add(new MensagemServiceBus
            {
                Id = Guid.NewGuid(),
                CriadaEm = DateTime.Now
            });
        }

        return mensagens;
    }

    public async Task PublicarAsync(IEnumerable<MensagemServiceBus> mensagens)
    {
        await _notificacaoPublisher.PublicarAsync(mensagens);
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
