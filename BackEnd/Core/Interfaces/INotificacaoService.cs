using Core.Models;

namespace Core.Interfaces;

public interface INotificacaoService
{
    ResultadoProcessamento ProcessarLote(int quantidade);
    Task<ResultadoProcessamento> ProcessarLoteAsync(int quantidade);
}
