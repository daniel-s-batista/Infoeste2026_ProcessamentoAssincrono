using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/notificacoes")]
public class NotificacoesController : ControllerBase
{
    private readonly INotificacaoService _notificacaoService;

    public NotificacoesController(INotificacaoService notificacaoService)
    {
        _notificacaoService = notificacaoService;
    }

    [HttpPost("processar")]
    public ActionResult<ResultadoProcessamento> Processar(SolicitacaoProcessamento solicitacao)
    {
        try
        {
            ResultadoProcessamento resultado = _notificacaoService.ProcessarLote(solicitacao.Quantidade);
            return Ok(resultado);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
    }
}
