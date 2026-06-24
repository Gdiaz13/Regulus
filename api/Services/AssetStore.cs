using api.Contracts;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

// Generic assets live here so stocks, cards, ETFs, and crypto can share one path.
public static class AssetStore
{
    public static Task<List<AssetResponse>> ListAsync(ApplicationDBContext db, AssetType? type)
    {
        var query = db.Assets.AsNoTracking().Include(asset => asset.Category);
        return SelectResponse(FilterByType(query, type)).ToListAsync();
    }

    public static Task<AssetResponse?> FindAsync(ApplicationDBContext db, int id)
    {
        var query = db.Assets.AsNoTracking().Include(asset => asset.Category);
        return SelectResponse(query.Where(asset => asset.Id == id)).FirstOrDefaultAsync();
    }

    public static async Task<AssetCreateResult> CreateAsync(ApplicationDBContext db, AssetCommand command)
    {
        var clean = Normalize(command);
        if (await Exists(db, clean))
        {
            return Duplicate(clean);
        }
        var category = await EnsureCategoryAsync(db, clean);
        var asset = NewAsset(clean, category);
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
        return Created(asset);
    }

    private static IQueryable<Asset> FilterByType(IQueryable<Asset> query, AssetType? type)
    {
        return type is null ? query : query.Where(asset => asset.AssetType == type);
    }

    private static IQueryable<AssetResponse> SelectResponse(IQueryable<Asset> query)
    {
        return query
            .OrderBy(asset => asset.AssetType)
            .ThenBy(asset => asset.Symbol)
            .Select(asset => new AssetResponse(
                asset.Id,
                asset.Symbol,
                asset.Name,
                asset.AssetType.ToString(),
                asset.Category == null ? null : asset.Category.Name,
                asset.CreatedOn
            ));
    }

    private static AssetResponse Response(Asset asset)
    {
        return new AssetResponse(
            asset.Id,
            asset.Symbol,
            asset.Name,
            asset.AssetType.ToString(),
            asset.Category == null ? null : asset.Category.Name,
            asset.CreatedOn
        );
    }

    private static Task<bool> Exists(ApplicationDBContext db, AssetCommand command)
    {
        return db.Assets.AnyAsync(asset => asset.Symbol == command.Symbol && asset.AssetType == command.AssetType);
    }

    private static AssetCreateResult Duplicate(AssetCommand command)
    {
        return new AssetCreateResult(null, $"{command.Symbol} is already tracked as {command.AssetType}.", true);
    }

    private static async Task<AssetCategory?> EnsureCategoryAsync(ApplicationDBContext db, AssetCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Category))
        {
            return null;
        }
        return await FindOrCreateCategoryAsync(db, command);
    }

    private static async Task<AssetCategory> FindOrCreateCategoryAsync(ApplicationDBContext db, AssetCommand command)
    {
        var slug = Slug(command.Category!);
        return await db.AssetCategories.FirstOrDefaultAsync(category => category.Slug == slug)
            ?? CreateCategory(db, command, slug);
    }

    private static AssetCategory CreateCategory(ApplicationDBContext db, AssetCommand command, string slug)
    {
        var category = new AssetCategory { Name = Clean(command.Category), Slug = slug, AssetType = command.AssetType };
        db.AssetCategories.Add(category);
        return category;
    }

    private static Asset NewAsset(AssetCommand command, AssetCategory? category)
    {
        return new Asset { Symbol = command.Symbol, Name = command.Name, AssetType = command.AssetType, Category = category };
    }

    private static AssetCreateResult Created(Asset asset)
    {
        return new AssetCreateResult(Response(asset), null, false);
    }

    private static AssetCommand Normalize(AssetCommand command)
    {
        var symbol = command.Symbol.Trim().ToUpperInvariant();
        var name = string.IsNullOrWhiteSpace(command.Name) ? symbol : command.Name.Trim();
        return new AssetCommand(symbol, name, command.AssetType, Clean(command.Category));
    }

    private static string Slug(string value)
    {
        return Clean(value).ToLowerInvariant().Replace(' ', '-');
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
