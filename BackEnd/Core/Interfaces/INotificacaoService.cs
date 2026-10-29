using Core.Models;

namespace Core.Interfaces;

public interface INotificacaoService
{
    ResultadoProcessamento ProcessarLote(int quantidade);
    Task<ResultadoProcessamento> ProcessarLoteAsync(int quantidade);
    Task ProcessarNotificacaoAsync(Mensagem mensagem);
    List<Mensagem> CriarMensagens(int quantidade);
    Task PublicarAsync(IEnumerable<Mensagem> mensagens);
}
