using Core.Models;

namespace Core.Interfaces;

public interface INotificacaoService
{
    List<MensagemServiceBus> CriarMensagens(int quantidade);
    ResultadoProcessamento ProcessarLote(int quantidade);
    Task<ResultadoProcessamento> ProcessarLoteAsync(int quantidade);
    Task ProcessarNotificacaoAsync(MensagemServiceBus mensagem);
    Task PublicarAsync(IEnumerable<MensagemServiceBus> mensagens);
}
