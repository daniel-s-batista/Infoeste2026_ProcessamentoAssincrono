using Core.Enums;

namespace Core.Models;

public class Notificacao
{
    public Guid Id { get; set; }

    public DateTime CriadaEm { get; set; }

    public DateTime? ProcessadaEm { get; set; }

    public EstadoNotificacao Status { get; set; } = EstadoNotificacao.Pendente;
}
