using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Services;

public class FavoritosService
{
    private const string CookieName = "crialab-favoritos";
    private readonly IHttpContextAccessor _http;
    private readonly ApplicationDbContext _db;

    public FavoritosService(IHttpContextAccessor http, ApplicationDbContext db)
    {
        _http = http;
        _db = db;
    }

    public async Task<HashSet<string>> ObterIdsAsync()
    {
        var ctx = _http.HttpContext;
        if (ctx?.Items["crialab-favoritos-ids"] is HashSet<string> cached)
            return cached;

        HashSet<string> ids;
        var userId = UserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var lista = await _db.Favoritos.AsNoTracking()
                .Where(x => x.UsuarioId == userId)
                .Select(x => x.ProdutoId)
                .ToListAsync();
            ids = lista.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            ids = ObterCookie().ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        if (ctx != null)
            ctx.Items["crialab-favoritos-ids"] = ids;
        return ids;
    }

    public async Task<bool> EstaFavoritoAsync(string produtoId)
    {
        if (string.IsNullOrWhiteSpace(produtoId)) return false;
        var ids = await ObterIdsAsync();
        return ids.Contains(produtoId);
    }

    public async Task<bool> AlternarAsync(string produtoId)
    {
        if (string.IsNullOrWhiteSpace(produtoId)) return false;
        var userId = UserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var fav = await _db.Favoritos.FirstOrDefaultAsync(x => x.UsuarioId == userId && x.ProdutoId == produtoId);
            if (fav == null)
            {
                if (!await _db.Produtos.AnyAsync(x => x.Id == produtoId))
                    return false;
                _db.Favoritos.Add(new Favorito { UsuarioId = userId, ProdutoId = produtoId, CriadoEm = DateTime.UtcNow });
                await _db.SaveChangesAsync();
                InvalidarCache();
                return true;
            }

            _db.Favoritos.Remove(fav);
            await _db.SaveChangesAsync();
            InvalidarCache();
            return false;
        }

        var lista = ObterCookie();
        var idx = lista.FindIndex(x => string.Equals(x, produtoId, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0)
        {
            lista.RemoveAt(idx);
            SalvarCookie(lista);
            InvalidarCache();
            return false;
        }

        lista.Add(produtoId);
        SalvarCookie(lista);
        InvalidarCache();
        return true;
    }

    public async Task RemoverAsync(string produtoId)
    {
        if (string.IsNullOrWhiteSpace(produtoId)) return;
        var userId = UserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var fav = await _db.Favoritos.FirstOrDefaultAsync(x => x.UsuarioId == userId && x.ProdutoId == produtoId);
            if (fav == null) return;
            _db.Favoritos.Remove(fav);
            await _db.SaveChangesAsync();
            InvalidarCache();
            return;
        }

        var lista = ObterCookie();
        lista.RemoveAll(x => string.Equals(x, produtoId, StringComparison.OrdinalIgnoreCase));
        SalvarCookie(lista);
        InvalidarCache();
    }

    public async Task SincronizarSessaoNoUsuarioAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;
        var cookie = ObterCookie();
        if (cookie.Count == 0) return;

        var existentes = await _db.Favoritos.AsNoTracking()
            .Where(x => x.UsuarioId == userId)
            .Select(x => x.ProdutoId)
            .ToListAsync();
        var set = existentes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var id in cookie)
        {
            if (set.Contains(id)) continue;
            if (!await _db.Produtos.AnyAsync(x => x.Id == id)) continue;
            _db.Favoritos.Add(new Favorito { UsuarioId = userId, ProdutoId = id, CriadoEm = DateTime.UtcNow });
            set.Add(id);
        }

        await _db.SaveChangesAsync();
        SalvarCookie(new List<string>());
        InvalidarCache();
    }

    private void InvalidarCache()
    {
        _http.HttpContext?.Items.Remove("crialab-favoritos-ids");
    }

    private string? UserId() =>
        _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    private List<string> ObterCookie()
    {
        var ctx = _http.HttpContext;
        if (ctx == null) return new List<string>();
        if (!ctx.Request.Cookies.TryGetValue(CookieName, out var raw) || string.IsNullOrWhiteSpace(raw))
            return new List<string>();
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(raw));
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private void SalvarCookie(List<string> ids)
    {
        var ctx = _http.HttpContext;
        if (ctx == null) return;
        var options = new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = ctx.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(180),
            Path = "/"
        };
        if (ids.Count == 0)
        {
            ctx.Response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
            return;
        }

        var json = JsonSerializer.Serialize(ids);
        var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        ctx.Response.Cookies.Append(CookieName, raw, options);
    }
}
