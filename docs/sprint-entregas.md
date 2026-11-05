# Sprint: Dashboard de Criptomoedas — Acompanhamento de entregas

**Atualizado em:** 03/11/2026
**Stack atual:** .NET 10, ASP.NET Core + Blazor Server, `BackgroundService`, EF Core + SQLite, Docker
**Legenda:** [x] concluído · [ ] pendente · [~] parcial

---

## Dia 1: Fundação e infraestrutura básica

- [x] Repositório GitHub e solução com projetos separados (`Collectors`, `Data`, `Infra`, `Services`, web)
- [x] Lista de moedas e limite em `appsettings.json` (10 moedas, IDs da CoinGecko)
- [x] Busca de preços em lote na CoinGecko (uma chamada para todas as moedas)
- [x] Estrutura de pastas definida
- [~] Verificar se o plano demo da CoinGecko exige chave de API (pendente de confirmação na documentação)

---

## Dia 2: Coleta de dados e persistência

- [x] Entidades `Coin`, `PricePoint`, `NewsItem` e `NewsCoin` (relação notícia-moeda)
- [x] `IPriceCollector` com busca em lote, devolvendo `CoinQuote`
- [x] Teste dos feeds RSS (Cointelegraph e CoinDesk responderam 200, com título, link e data)
- [x] `INewsCollector` com `CointelegraphNewsCollector` (ver desvio sobre o CoinDesk)
- [x] Cointelegraph como fonte única: feed geral mais um feed por tag de moeda
- [x] Mapa `NewsTags` na configuração para moedas cujo nome de tag difere (`binancecoin` → `binance-coin`)
- [x] Leitura do feed com `SyndicationFeed`: `link` vira `Url` (sem parâmetros de rastreamento) e `pubDate` vira `PublishedAt`
- [x] Deduplicação: filtro por URL no repositório, remoção de repetidos dentro do lote e índice único em `Url`
- [x] EF Core + SQLite, migração `InitialCreate` aplicada na inicialização, modo WAL
- [x] `CollectorWorker`: preços a cada 15 min, notícias a cada 30 min, cada etapa isolada
- [x] Retry e backoff com `Microsoft.Extensions.Http.Resilience`
- [x] Validado no container: 10 preços salvos por ciclo, notícias gravadas e sem duplicatas após a correção
- [ ] Conferir os termos de uso da CoinGecko e dos portais e registrar a atribuição no README

---

## Dia 3: Feature de IA (sentimento, moedas citadas e resumo)

- [x] `SentimentService` com provedor Ollama (local), atrás de `ILlmClient`
- [x] Prompt com saída em JSON (sentimento, score, moedas citadas, temas)
- [x] Passar a lista de moedas válidas ao modelo
- [~] Tratar ambiguidade de símbolos (SOL, ADA): regra no prompt, falta validar com notícias reais
- [x] Validar o JSON e registrar fallback (resposta inválida fica pendente e é tentada no próximo ciclo)
- [x] Classificar notícias pendentes em lotes pequenos (`BatchSize`, a cada 30 min)
- [x] Resumo diário das moedas mais citadas (`DailySummaryService`, últimas 24h, registrado no log a cada ciclo; compilação pendente de validação local)
- [~] Limitar tokens de saída do Ollama (`num_predict`): ainda sem limite
- [x] Preencher `NewsCoins` com as moedas citadas em cada notícia
- [~] Amostra revisada à mão: modelo em `docs/revisao-sentimento.md`, aguardando preenchimento (meta: 15 notícias, 80% OK)

---

## Dia 4: Dashboard

- [x] Tabela de moedas (preço, variação 24h, capitalização, sentimento) — código pronto, validação visual pendente
- [x] Seletor de moedas e gráfico comparativo (SVG, variação % desde o início do período, 24h e 7 dias) — código pronto, validação visual pendente
- [x] Lista de notícias com sentimento e filtro por moeda — código pronto, validação visual pendente
- [x] Bloco com o resumo do dia — código pronto, validação visual pendente

---

## Dia 5: Hardening, observabilidade e documentação

- [x] Logs em JSON no stdout, com rotação pelo Docker, e reinício automático (`restart: unless-stopped`) — código pronto
- [x] Endpoint `/health` — código pronto
- [x] README completo (arquitetura, como rodar, configuração, observabilidade, limitações, atribuição)
- [ ] Vídeo ou prints do dashboard
- [x] Seção "Decisões e trade-offs" no README

**Hardening aplicado no código:** portas 8080 e 11434 só locais; Ollama sem porta publicada; ambiente de produção; `UseHttpsRedirection` removido (quebraria o acesso HTTP atrás do Nginx); encaminhamento de cabeçalhos do proxy habilitado; configuração do Bedrock removida da execução com Docker.

---

## Desvios do plano original

| Assunto | Plano original | Como ficou | Motivo |
|---|---|---|---|
| Fonte de notícias | API do Reddit | RSS do Cointelegraph | A API do Reddit não aceita novos apps desde 31/10/2026 |
| Fontes de notícias | Cointelegraph e CoinDesk | Só Cointelegraph | Simplificação escolhida pelo time |
| CoinDesk Data API | Não estava no plano | Não usada | O plano gratuito foi encerrado em 21/05/2026 |
| Versão do .NET | .NET 8 | .NET 10 | Versão do projeto criado |
| Docker | Não estava no plano | Dockerfile, `.dockerignore` e `docker-compose.yml` | Testar sem instalar banco local; SQLite fica em volume |
| Migração do banco | Manual | Aplicada na inicialização (`Migrate()`) | Simplificar os testes |
| Edição do Cointelegraph | Não definida | Inglês | Decisão do time |
| Hospedagem | Lambda ou outra nuvem | Docker local, sem deploy em nuvem | Decisão do autor: o projeto fica como estudo local |

---

## Histórico de commits relevantes

| Commit | Descrição |
|---|---|
| `5c01afc` | Coletores e persistência |
| `0402e9d` | Docker para a aplicação, com SQLite em volume |
| `7100733` | Migração inicial |
| `0391bb3` | `CollectorWorker` com coleta de preços e notícias |
| `3340a2f` | Remoção dos parâmetros `utm_` das URLs de notícias |
| `5ea5091` | Mapa de tags de notícias por moeda (`NewsTags`) |

---

## Pendências imediatas

1. Confirmar a chave de API do plano demo da CoinGecko e atualizar a configuração, se necessário.
2. Confirmar os termos de uso da CoinGecko e do Cointelegraph e registrar a atribuição no README.
3. Preencher a amostra de revisão em `docs/revisao-sentimento.md` (15 notícias) e validar a classificação.
4. Validar a compilação e a execução do `DailySummaryService` localmente (`dotnet build`), pois o código foi escrito sem compilar.
5. Revogar a chave de demo da CoinGecko que estava hardcoded no `DailySummaryService`. Ela continua no histórico do Git, então deve ser tratada como exposta.
6. Monitorar nos logs se alguma outra tag de moeda retorna 404.

---

## Riscos em aberto

| Risco | Situação | Plano B |
|---|---|---|
| Feed do Cointelegraph sair do ar ou mudar de formato | Sem outra fonte ativa | Reativar o feed do CoinDesk, que ainda responde |
| Tag de moeda inexistente no Cointelegraph | BNB resolvido com `NewsTags` | Adicionar outro mapeamento na configuração |
| Ambiguidade de símbolos gera menções falsas | Previsto para o Dia 3 | Filtrar por nome completo e revisar uma amostra |
