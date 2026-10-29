using Core.Models;

namespace Core.Interfaces
{
    public interface INotificacaoPublisher
    {
        Task PublicarAsync(IEnumerable<MensagemServiceBus> mensagens);
    }
}
