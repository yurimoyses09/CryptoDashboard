# Revisão manual da classificação de sentimento

**Critério (Dia 3):** pelo menos 15 notícias classificadas, revisadas à mão.

Para cada notícia, compare o que o modelo gravou com a sua leitura do título e do resumo.
Marque **OK** se concordar, ou **ERRADO** se não concordar, e anote o motivo.

Consulta para gerar a amostra (SQLite):

```sql
SELECT n.Id, n.Title, n.Sentiment, n.Score,
       (SELECT group_concat(nc.CoinId, ', ') FROM NewsCoins nc WHERE nc.NewsItemId = n.Id) AS Moedas
FROM NewsItems n
WHERE n.Sentiment IS NOT NULL
ORDER BY n.Id DESC
LIMIT 15;
```

Confira o nome exato das tabelas no `AppDbContext` antes de rodar, pois o EF pode ter gerado nomes diferentes.

| # | Id | Título | Sentimento | Score | Moedas | Veredito (OK/ERRADO) | Motivo |
|---|----|--------|-----------|-------|--------|----------------------|--------|
| 1 |    |        |           |       |        |                      |        |
| 2 |    |        |           |       |        |                      |        |
| 3 |    |        |           |       |        |                      |        |
| 4 |    |        |           |       |        |                      |        |
| 5 |    |        |           |       |        |                      |        |
| 6 |    |        |           |       |        |                      |        |
| 7 |    |        |           |       |        |                      |        |
| 8 |    |        |           |       |        |                      |        |
| 9 |    |        |           |       |        |                      |        |
| 10 |   |        |           |       |        |                      |        |
| 11 |   |        |           |       |        |                      |        |
| 12 |   |        |           |       |        |                      |        |
| 13 |   |        |           |       |        |                      |        |
| 14 |   |        |           |       |        |                      |        |
| 15 |   |        |           |       |        |                      |        |

**Resultado:** ___ OK de 15 (___%). Meta sugerida: pelo menos 12 OK (80%).
