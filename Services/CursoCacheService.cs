using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using ExamenParcial.Models; 
using Microsoft.EntityFrameworkCore;
using ExamenParcial.Data; 
namespace ExamenParcial.Services;
public class CursoCacheService
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "CursosActivosCache";

    public CursoCacheService(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Curso>> GetCursosActivosAsync()
    {
        // Intentar leer de cache
        var cached = await _cache.GetStringAsync(CacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<List<Curso>>(cached)!;
        }

        // Si no hay cache, leer de DB
        var cursos = await _context.Cursos.Where(c => c.Activo).ToListAsync();

        // Guardar en cache 60s
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(cursos), options);

        return cursos;
    }

    public async Task InvalidateCacheAsync()
    {
        await _cache.RemoveAsync(CacheKey);
    }
}
