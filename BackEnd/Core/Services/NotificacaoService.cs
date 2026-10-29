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
        INotificacaoPublisher notificacaoPublisher,
        ConfiguracaoProcessamento configuracao)
    {
        _notificacaoRepository = notificacaoRepository;
        _notificacaoPublisher = notificacaoPublisher;
        _configuracao = configuracao;
    }

    public NotificacaoService(
        INotificacaoRepository notificacaoRepository,
        ConfiguracaoProcessamento configuracao)
    {
        _notificacaoRepository = notificacaoRepository;
        _configuracao = configuracao;
        _notificacaoPublisher = null;
    }

    public ResultadoProcessamento ProcessarLote(int quantidade)
    {
        List<Mensagem> mensagens = CriarMensagens(quantidade);

        Stopwatch cronometro = Stopwatch.StartNew();

        foreach (Mensagem mensagem in mensagens)
        {
            ProcessarNotificacao(mensagem);
        }

        cronometro.Stop();

        return new ResultadoProcessamento
        {
            QuantidadeProcessada = quantidade,
            TempoTotalEmMilissegundos = cronometro.ElapsedMilliseconds
        };
    }

    private void ProcessarNotificacao(Mensagem mensagem)
    {
        var notificacao = new Notificacao
        {
            Id = mensagem.Id,
            CriadaEm = mensagem.CriadaEm
        };

        // Bloqueio intencional para mostrar um dos problemas da programação síncrona
        // A Thread pode ficar travada durante o prossamento de uma mensagem
        Thread.Sleep(_configuracao.AtrasoPorNotificacaoEmMilissegundos);

        notificacao.Status = EstadoNotificacao.Processada;
        notificacao.ProcessadaEm = DateTime.Now;
        _notificacaoRepository.Adicionar(notificacao);
    }

    public async Task ProcessarNotificacaoAsync(Mensagem mensagem)
    {
        var notificacao = new Notificacao
        {
            Id = mensagem.Id,
            CriadaEm = mensagem.CriadaEm
        };

        // Diferentemente de Thread.Sleep(), que trava a thread e impede outros processamentos,
        // com Task.Delay() a thread pode ser usada para atender outros trabalhos.
        await Task.Delay(_configuracao.AtrasoPorNotificacaoEmMilissegundos);

        notificacao.Status = EstadoNotificacao.Processada;
        notificacao.ProcessadaEm = DateTime.Now;
        _notificacaoRepository.Adicionar(notificacao);
    }

    public async Task<ResultadoProcessamento> ProcessarLoteAsync(int quantidade)
    {
        List<Mensagem> mensagens = CriarMensagens(quantidade);

        Stopwatch cronometro = Stopwatch.StartNew();

        foreach (Mensagem mensagem in mensagens)
        {
            await ProcessarNotificacaoAsync(mensagem);
        }

        cronometro.Stop();

        return new ResultadoProcessamento
        {
            QuantidadeProcessada = quantidade,
            TempoTotalEmMilissegundos = cronometro.ElapsedMilliseconds
        };
    }

    public List<Mensagem> CriarMensagens(int quantidade)
    {
        ValidarQuantidade(quantidade);

        var mensagens = new List<Mensagem>();

        for (int indice = 1; indice <= quantidade; indice++)
        {
            mensagens.Add(new Mensagem
            {
                Id = Guid.NewGuid(),
                CriadaEm = DateTime.Now
            });
        }

        return mensagens;
    }

    public async Task PublicarAsync(IEnumerable<Mensagem> mensagens)
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
