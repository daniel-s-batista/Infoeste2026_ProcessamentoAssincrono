import { useState, type SubmitEvent } from 'react'
import './App.css'

const MAX_NOTIFICACOES = 20
const URL_PROCESSAMENTO =
  import.meta.env.VITE_API_URL ?? 'http://localhost:5100/api/notificacoes/processar'

interface ResultadoProcessamento {
  quantidadeProcessada: number
  tempoTotalEmMilissegundos: number
}

function App() {
  const [quantidade, setQuantidade] = useState('5')
  const [estaProcessando, setEstaProcessando] = useState(false)
  const [resultado, setResultado] = useState<ResultadoProcessamento | null>(null)
  const [mensagemErro, setMensagemErro] = useState('')

  async function iniciarProcessamento(evento: SubmitEvent) {
    evento.preventDefault()

    const quantidadeInformada = Number(quantidade)

    if (!Number.isInteger(quantidadeInformada) || quantidadeInformada < 1) {
      setMensagemErro('Informe uma quantidade inteira maior que zero.')
      return
    }

    if (quantidadeInformada > MAX_NOTIFICACOES) {
      setMensagemErro(`Informe no máximo ${MAX_NOTIFICACOES} notificações.`)
      return
    }

    setEstaProcessando(true)
    setResultado(null)
    setMensagemErro('')

    try {
      const resposta = await fetch(URL_PROCESSAMENTO, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ quantidade: quantidadeInformada }),
      })

      const dados = await resposta.json()

      if (!resposta.ok) {
        throw new Error(dados.mensagem ?? 'Não foi possível processar as notificações.')
      }

      setResultado(dados as ResultadoProcessamento)
    } catch (erro) {
      const mensagem = erro instanceof Error ? erro.message : 'Ocorreu um erro inesperado.'
      setMensagemErro(mensagem)
    } finally {
      setEstaProcessando(false)
    }
  }

  return (
    <main className="pagina">
      <section className="cartao" aria-labelledby="titulo-principal">
        <p className="identificador">Demonstração inicial</p>
        <h1 id="titulo-principal">Processamento de notificações</h1>
        <p className="introducao">
          Esta versão executa cada notificação uma após a outra. A tela aguardará a resposta
          somente quando todo o lote terminar.
        </p>

        <form onSubmit={iniciarProcessamento}>
          <label htmlFor="quantidade">Quantidade de notificações</label>
          <div className="grupo-campo">
            <input
              id="quantidade"
              type="number"
              min="1"
              max={MAX_NOTIFICACOES}
              step="1"
              value={quantidade}
              onChange={(evento) => setQuantidade(evento.target.value)}
              disabled={estaProcessando}
              required
            />
            <button type="submit" disabled={estaProcessando}>
              {estaProcessando ? 'Processando...' : 'Iniciar processamento'}
            </button>
          </div>
          <p className="ajuda">Máximo de {MAX_NOTIFICACOES} notificações por lote.</p>
        </form>

        {estaProcessando && (
          <div className="aviso-processamento" role="status">
            <span className="carregador" aria-hidden="true" />
            A API está processando o lote sequencialmente. Aguarde a resposta.
          </div>
        )}

        {resultado && (
          <section className="resultado sucesso" aria-live="polite">
            <h2>Lote concluído</h2>
            <p>
              <strong>{resultado.quantidadeProcessada}</strong> notificações foram processadas.
            </p>
            <p>
              Tempo total: <strong>{formatarTempo(resultado.tempoTotalEmMilissegundos)}</strong>
            </p>
          </section>
        )}

        {mensagemErro && (
          <p className="resultado erro" role="alert">
            {mensagemErro}
          </p>
        )}
      </section>
    </main>
  )
}

function formatarTempo(tempoEmMilissegundos: number) {
  return `${(tempoEmMilissegundos / 1000).toFixed(1).replace('.', ',')} segundos`
}

export default App
