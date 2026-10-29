using Azure.Messaging.ServiceBus;
using Core.Interfaces;
using Core.Models;

namespace Core.Services
{
    public class NotificacaoPublisher : INotificacaoPublisher
    {
        private readonly ConfiguracaoServiceBus _configuracao;
        private readonly Lazy<ServiceBusClient> _client;
        private readonly Lazy<ServiceBusSender> _sender;

        public NotificacaoPublisher(ConfiguracaoServiceBus configuracao)
        {
            _configuracao = configuracao;
            _client = new Lazy<ServiceBusClient>(() =>
            {
                return new ServiceBusClient(_configuracao.ConnectionString);
            });
            _sender = new Lazy<ServiceBusSender>(() =>
                _client.Value.CreateSender(_configuracao.NomeFila));
        }

        public async Task PublicarAsync(IEnumerable<MensagemServiceBus> mensagens)
        {
            var mensagensServiceBus = mensagens.Select((msg) =>
            {
                return new ServiceBusMessage(BinaryData.FromObjectAsJson(msg))
                {
                    MessageId = msg.Id.ToString(),
                };
            });

            await _sender.Value.SendMessagesAsync(mensagensServiceBus);
        }
    }
}
