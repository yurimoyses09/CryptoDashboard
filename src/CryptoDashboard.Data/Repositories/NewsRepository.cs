using CryptoDashboard.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CryptoDashboard.Data.Repositories
{
    public class NewsRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<NewsRepository> _logger;

        public NewsRepository(AppDbContext db, ILogger<NewsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> SaveNewNewsAsync(IEnumerable<NewsItem> incoming, CancellationToken ct = default)
        {
            // 1. Remove repetições dentro do próprio lote
            var batch = incoming
                .DistinctBy(n => n.Url, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (batch.Count == 0)
                return 0;

            // 2. Descarta as URLs que já existem no banco
            var urls = batch.Select(n => n.Url).ToList();

            var existing = await _db.NewsItems
                .Where(n => urls.Contains(n.Url))
                .Select(n => n.Url)
                .ToListAsync(ct);

            var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
            var novos = batch.Where(n => !existingSet.Contains(n.Url)).ToList();

            if (novos.Count == 0)
                return 0;

            // 3. Insere. Se o índice único recusar algo, ignora o lote com aviso
            _db.NewsItems.AddRange(novos);

            try
            {
                return await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Lote de notícias recusado pelo índice único; nada foi salvo");
                _db.ChangeTracker.Clear();
                return 0;
            }
        }
    }
}
