using Core.Interfaces;
using Core.Models;

namespace Core.Repositories;

public class NotificacaoRepository : INotificacaoRepository
{
    private readonly List<Notificacao> _notificacoes = new List<Notificacao>();
    private readonly object _bloqueio = new object();

    public void Adicionar(Notificacao notificacao)
    {
        /**
         * INSERT INTO tb_notificacoes ( ... ) VALUES ( ... );
         */
        lock (_bloqueio)
        {
            _notificacoes.Add(notificacao);
        }
    }
}
