using Azure.Messaging.ServiceBus;
using Core.Interfaces;
using Core.Models;

namespace Worker
{
    public class Worker : BackgroundService
    {
        private readonly ConfiguracaoServiceBus _configuracao;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(
            ILogger<Worker> logger,
            IServiceScopeFactory scopeFactory,
            ConfiguracaoServiceBus configuracao)
        {
            _logger = logger;
            _serviceScopeFactory = scopeFactory;
            _configuracao = configuracao;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var serviceBusClient = new ServiceBusClient(_configuracao.ConnectionString);
            ServiceBusProcessor processor = serviceBusClient.CreateProcessor(
                _configuracao.NomeFila,
                new ServiceBusProcessorOptions()
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1,
                    MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(5)
                }
            );

            processor.ProcessMessageAsync += ProcessarMensagemAsync;
            processor.ProcessErrorAsync += ProcessarErroAsync;

            await processor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation($"Aguardando mensagens da fila {_configuracao.NomeFila}");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch
            {
                await processor.StopProcessingAsync(CancellationToken.None);
                await processor.DisposeAsync();
            }
        }

        private async Task ProcessarMensagemAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var mensagem = args.Message.Body.ToObjectFromJson<MensagemServiceBus>();

                if (mensagem == null)
                {
                    await args.DeadLetterMessageAsync(
                        args.Message,
                        "Mensagem vazia",
                        "O corpo da mensagem chegou vazio.");
                    return;
                }

                using IServiceScope escopo = _serviceScopeFactory.CreateScope();
                INotificacaoService notificacaoService = escopo.ServiceProvider
                    .GetRequiredService<INotificacaoService>();

                await notificacaoService.ProcessarNotificacaoAsync(mensagem);
                await args.CompleteMessageAsync(args.Message);

                _logger.LogInformation($"Notificação {mensagem.Id} processada com sucesso!");
            }
            catch
            {
                _logger.LogError("Ocorreu um erro inesperado!");
            }
        }

        private Task ProcessarErroAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError("Ocorreu um erro durante o processamento!");
            return Task.CompletedTask;
        }
    }
}
