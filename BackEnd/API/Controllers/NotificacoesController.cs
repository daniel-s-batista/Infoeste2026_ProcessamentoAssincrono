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

    [HttpPost("processar-async")]
    public async Task<ActionResult<ResultadoProcessamento>> ProcessarAsync(SolicitacaoProcessamento solicitacao)
    {
        try
        {
            ResultadoProcessamento resultado = await _notificacaoService.ProcessarLoteAsync(solicitacao.Quantidade);
            return Ok(resultado);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
    }

    [HttpPost("enfileirar")]
    public async Task<ActionResult> Enfileirar(SolicitacaoProcessamento solicitacao)
    {
        try
        {
            List<Mensagem> mensagens = _notificacaoService.CriarMensagens(solicitacao.Quantidade);
            await _notificacaoService.PublicarAsync(mensagens);

            return Accepted(new { quantidadeEnfileirada = mensagens.Count });
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
        catch (FormatException)
        {
            return Problem(
                "A connection string configurada para o Service Bus não é válida.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (InvalidOperationException excecao)
        {
            return Problem(excecao.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
