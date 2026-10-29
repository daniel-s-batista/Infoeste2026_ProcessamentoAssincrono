using Azure.Messaging.ServiceBus;
using Core.Interfaces;
using Core.Models;
using System.Text.Json;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly ConfiguracaoServiceBus _configuracao;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        ConfiguracaoServiceBus configuracao,
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _configuracao = configuracao;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ValidarConfiguracao();

        await using var serviceBusClient = new ServiceBusClient(_configuracao.ConnectionString);
        ServiceBusProcessor processor = serviceBusClient.CreateProcessor(
            _configuracao.NomeFila,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
            });

        processor.ProcessMessageAsync += ProcessarMensagemAsync;
        processor.ProcessErrorAsync += ProcessarErroAsync;

        await processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Worker aguardando mensagens da fila {Fila}.", _configuracao.NomeFila);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        finally
        {
            await processor.StopProcessingAsync(CancellationToken.None);
            await processor.DisposeAsync();
        }
    }

    private async Task ProcessarMensagemAsync(ProcessMessageEventArgs argumentos)
    {
        try
        {
            Mensagem? mensagem = argumentos.Message.Body.ToObjectFromJson<Mensagem>();

            if (mensagem is null)
            {
                await argumentos.DeadLetterMessageAsync(
                    argumentos.Message,
                    "MensagemInvalida",
                    "O corpo da mensagem não contém uma notificação válida.");
                return;
            }

            using IServiceScope escopo = _scopeFactory.CreateScope();
            INotificacaoService notificacaoService = escopo.ServiceProvider
                .GetRequiredService<INotificacaoService>();

            await notificacaoService.ProcessarNotificacaoAsync(mensagem);
            await argumentos.CompleteMessageAsync(argumentos.Message);

            _logger.LogInformation("Notificação {MensagemId} processada.", mensagem.Id);
        }
        catch (JsonException excecao)
        {
            _logger.LogError(excecao, "A mensagem {MensagemId} não está em um JSON válido.", argumentos.Message.MessageId);
            await argumentos.DeadLetterMessageAsync(
                argumentos.Message,
                "MensagemInvalida",
                "O corpo da mensagem não está em um JSON válido.");
        }
        catch (Exception excecao)
        {
            _logger.LogError(excecao, "Erro ao processar a mensagem {MensagemId}.", argumentos.Message.MessageId);
            await argumentos.AbandonMessageAsync(argumentos.Message);
        }
    }

    private Task ProcessarErroAsync(ProcessErrorEventArgs argumentos)
    {
        _logger.LogError(
            argumentos.Exception,
            "Erro no Service Bus. Origem: {Origem}.",
            argumentos.ErrorSource);

        return Task.CompletedTask;
    }

    private void ValidarConfiguracao()
    {
        if (string.IsNullOrWhiteSpace(_configuracao.ConnectionString))
        {
            throw new InvalidOperationException("Connection string do Service Bus não configurada.");
        }

        if (string.IsNullOrWhiteSpace(_configuracao.NomeFila))
        {
            throw new InvalidOperationException("Nome da fila do Service Bus não configurado.");
        }
    }
}
