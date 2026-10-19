// Script para testar o envio de múltiplas requisições para a API

const QUANTIDADE_REQUISICOES = 50;
const MAX_NOTIFICACOES = 20;
const URL_PROCESSAMENTO = 'http://localhost:5100/api/notificacoes/processar';

async function enviarRequisicao(numero) {
  const inicio = performance.now();

  try {
    const resposta = await fetch(URL_PROCESSAMENTO, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ quantidade: MAX_NOTIFICACOES }),
    });

    const dados = await resposta.json();
    const tempoCliente = (performance.now() - inicio) / 1000;

    return {
      requisicao: numero,
      status: resposta.status,
      tempoClienteSegundos: tempoCliente.toFixed(2),
      tempoServicoSegundos:
        dados.tempoTotalEmMilissegundos == null
          ? '-'
          : (dados.tempoTotalEmMilissegundos / 1000).toFixed(2),
    };
  } catch (erro) {
    return {
      requisicao: numero,
      status: 'Erro',
      tempoClienteSegundos: ((performance.now() - inicio) / 1000).toFixed(2),
      tempoServicoSegundos: '-',
      erro: erro.message,
    };
  }
}

const inicioTeste = performance.now();

const requisicoes = [];
for (let i = 0; i < QUANTIDADE_REQUISICOES; i++) {
  requisicoes[i] = enviarRequisicao(i);
}
const resultados = await Promise.all(requisicoes);

console.table(resultados);
console.log(
  `Tempo total do teste: ${((performance.now() - inicioTeste) / 1000).toFixed(2)} s`,
);
