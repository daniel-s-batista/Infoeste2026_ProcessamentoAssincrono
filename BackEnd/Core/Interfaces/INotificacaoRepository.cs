using Core.Models;

namespace Core.Interfaces;

public interface INotificacaoRepository
{
    void Adicionar(Notificacao notificacao);
}
