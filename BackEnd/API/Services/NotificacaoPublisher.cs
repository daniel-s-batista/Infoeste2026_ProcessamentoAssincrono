using Azure.Messaging.ServiceBus;
using Core.Interfaces;
using Core.Models;

namespace API.Services;

public class NotificacaoPublisher : INotificacaoPublisher, IAsyncDisposable
{
    private readonly ConfiguracaoServiceBus _configuracao;
    private readonly Lazy<ServiceBusClient> _serviceBusClient;
    private readonly Lazy<ServiceBusSender> _sender;

    public NotificacaoPublisher(ConfiguracaoServiceBus configuracao)
    {
        _configuracao = configuracao;
        _serviceBusClient = new Lazy<ServiceBusClient>(() =>
        {
            ValidarConfiguracao(_configuracao);
            return new ServiceBusClient(_configuracao.ConnectionString);
        });
        _sender = new Lazy<ServiceBusSender>(() =>
            _serviceBusClient.Value.CreateSender(_configuracao.NomeFila));
    }

    public async Task PublicarAsync(IEnumerable<Mensagem> mensagens)
    {
        var mensagensServiceBus = mensagens.Select(mensagem => new ServiceBusMessage(
            BinaryData.FromObjectAsJson(mensagem))
        {
            MessageId = mensagem.Id.ToString()
        });

        await _sender.Value.SendMessagesAsync(mensagensServiceBus);
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceBusClient.IsValueCreated)
        {
            if (_sender.IsValueCreated)
            {
                await _sender.Value.DisposeAsync();
            }

            await _serviceBusClient.Value.DisposeAsync();
        }
    }

    private static void ValidarConfiguracao(ConfiguracaoServiceBus configuracao)
    {
        if (string.IsNullOrWhiteSpace(configuracao.ConnectionString))
        {
            throw new InvalidOperationException("Connection string do Service Bus não configurada.");
        }

        if (string.IsNullOrWhiteSpace(configuracao.NomeFila))
        {
            throw new InvalidOperationException("Nome da fila do Service Bus não configurado.");
        }
    }
}
