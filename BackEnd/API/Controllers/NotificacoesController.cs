using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

    // POST https://localhost/api/notificacoes/processar HTTP 1.1
    // <headers>
    //
    // { "Quantidade": 5 }
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
    public async Task<ActionResult> Enfileirar(SolicitacaoProcessamento solicitacaoProcessamento)
    {
        try
        {
            var cronometro = Stopwatch.StartNew();

            var mensagens = _notificacaoService.CriarMensagens(solicitacaoProcessamento.Quantidade);
            await _notificacaoService.PublicarAsync(mensagens);

            cronometro.Stop();

            var resultado = new ResultadoProcessamento()
            {
                QuantidadeProcessada = solicitacaoProcessamento.Quantidade,
                TempoTotalEmMilissegundos = cronometro.ElapsedMilliseconds / 1000
            };

            return Ok(resultado);
        }
        catch
        {
            return BadRequest("Ocorreu um erro.");
        }
    }
}
