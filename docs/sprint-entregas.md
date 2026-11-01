# Sprint: Dashboard de Criptomoedas — Acompanhamento de entregas

**Atualizado em:** 01/11/2026
**Stack atual:** .NET 10, ASP.NET Core + Blazor Server, `BackgroundService`, EF Core + SQLite, Docker
**Legenda:** [x] concluído · [ ] pendente · [~] parcial

---

## Dia 1: Fundação e infraestrutura básica

- [x] Repositório GitHub e solução com projetos separados (`Collectors`, `Data`, `Infra`, `Services`, web)
- [ ] AWS Budget com alertas de US$ 5 e US$ 10
- [ ] Acesso ao modelo no Amazon Bedrock
- [ ] Role IAM com `bedrock:InvokeModel`
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

- [ ] `SentimentService` com a API Converse do Bedrock
- [ ] Prompt com saída em JSON (sentimento, score, moedas citadas, temas)
- [ ] Passar a lista de moedas válidas ao modelo
- [ ] Tratar ambiguidade de símbolos (SOL, ADA)
- [ ] Validar o JSON e registrar fallback
- [ ] Classificar notícias pendentes em lotes pequenos
- [ ] Resumo diário das moedas mais citadas
- [ ] Limitar tokens de entrada e saída
- [ ] Preencher `tb_news_coin` com as moedas citadas em cada notícia

---

## Dia 4: Dashboard e deploy na EC2

- [ ] Tabela de moedas (preço, variação 24h, capitalização, sentimento)
- [ ] Seletor de moedas e gráfico comparativo
- [ ] Lista de notícias com sentimento e filtro por moeda
- [ ] Bloco com o resumo do dia
- [ ] Subir instância EC2 com Amazon Linux 2023
- [ ] Security Group, IAM Role e Elastic IP
- [ ] Publicar o app (ou o container) e configurar `systemd` ou Docker com reinício automático
- [ ] Nginx como reverse proxy

---

## Dia 5: Hardening, observabilidade e documentação

- [ ] Logs e reinício automático do serviço
- [ ] Alarmes de CPU e status check
- [ ] Revisão de permissões da IAM Role
- [ ] Atualização do SO e acesso SSH restrito
- [ ] Script de deploy
- [ ] README completo (arquitetura, como rodar, custo, lista de moedas, atribuição)
- [ ] Vídeo ou prints do dashboard
- [ ] Seção "Decisões e trade-offs"

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
| Hospedagem | Lambda ou EC2 | EC2 | O worker é um processo contínuo |

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
3. Iniciar o Dia 3 com o `SentimentService`. Antes, configurar o acesso ao Bedrock e a role IAM.
4. Monitorar nos logs se alguma outra tag de moeda retorna 404.

---

## Riscos em aberto

| Risco | Situação | Plano B |
|---|---|---|
| Feed do Cointelegraph sair do ar ou mudar de formato | Sem outra fonte ativa | Reativar o feed do CoinDesk, que ainda responde |
| Tag de moeda inexistente no Cointelegraph | BNB resolvido com `NewsTags` | Adicionar outro mapeamento na configuração |
| Custo do Bedrock crescer com o volume de notícias | Ainda não medido | Reduzir a frequência e o tamanho do lote |
| Ambiguidade de símbolos gera menções falsas | Previsto para o Dia 3 | Filtrar por nome completo e revisar uma amostra |
