using api.Contracts;
using api.Data;
using api.Models;
using api.Services;

namespace api.Endpoints;

public static class AssetEndpoints
{
    public static void MapAssetEndpoints(this WebApplication app)
    {
        var assets = app.MapGroup("/api/assets");
        assets.MapGet("", ListAssets);
        assets.MapGet("{id:int}", GetAsset);
        assets.MapPost("", CreateAsset);
    }

    private static Task<IResult> ListAssets(string? assetType, ApplicationDBContext db)
    {
        if (!TryOptionalType(assetType, out var type))
        {
            return BadAssetType();
        }
        return DatabaseRequest.Run(async () => Results.Ok(await AssetStore.ListAsync(db, type)));
    }

    private static Task<IResult> GetAsset(int id, ApplicationDBContext db)
    {
        return DatabaseRequest.Run(() => GetAssetCore(id, db));
    }

    private static async Task<IResult> GetAssetCore(int id, ApplicationDBContext db)
    {
        var asset = await AssetStore.FindAsync(db, id);
        return asset is null ? Results.NotFound($"Asset with id {id} was not found.") : Results.Ok(asset);
    }

    private static Task<IResult> CreateAsset(CreateAssetRequest request, ApplicationDBContext db)
    {
        return DatabaseRequest.Run(() => CreateAssetCore(request, db));
    }

    private static async Task<IResult> CreateAssetCore(CreateAssetRequest request, ApplicationDBContext db)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return validation;
        }
        var result = await AssetStore.CreateAsync(db, ToCommand(request));
        return CreatedResult(result);
    }

    private static IResult CreatedResult(AssetCreateResult result)
    {
        return result.Duplicate
            ? Results.Conflict(result.Message)
            : Results.Created($"/api/assets/{result.Asset!.Id}", result.Asset);
    }

    private static IResult? Validate(CreateAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
        {
            return Results.BadRequest("An asset symbol is required.");
        }
        return ValidateDetails(request);
    }

    private static IResult? ValidateDetails(CreateAssetRequest request)
    {
        if (!TryRequiredType(request.AssetType, out _))
        {
            return Results.BadRequest("A valid asset type is required.");
        }
        return ValidateLengths(request);
    }

    private static IResult? ValidateLengths(CreateAssetRequest request)
    {
        if (request.Symbol!.Trim().Length > Asset.SymbolMaxLength)
        {
            return Results.BadRequest($"Asset symbols must be {Asset.SymbolMaxLength} characters or less.");
        }
        return CategoryTooLong(request) ? CategoryTooLongResult() : null;
    }

    private static AssetCommand ToCommand(CreateAssetRequest request)
    {
        var symbol = request.Symbol!.Trim().ToUpperInvariant();
        var name = string.IsNullOrWhiteSpace(request.Name) ? symbol : request.Name.Trim();
        return new AssetCommand(symbol, name, ParseType(request.AssetType!), Clean(request.Category));
    }

    private static bool TryOptionalType(string? value, out AssetType? assetType)
    {
        assetType = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }
        return TryRequiredType(value, out assetType);
    }

    private static bool TryRequiredType(string? value, out AssetType? assetType)
    {
        var parsed = Enum.TryParse<AssetType>(value, ignoreCase: true, out var type);
        assetType = parsed ? type : null;
        return parsed;
    }

    private static AssetType ParseType(string value)
    {
        return Enum.Parse<AssetType>(value, ignoreCase: true);
    }

    private static bool CategoryTooLong(CreateAssetRequest request)
    {
        return Clean(request.Category)?.Length > AssetCategory.NameMaxLength;
    }

    private static IResult CategoryTooLongResult()
    {
        return Results.BadRequest($"Categories must be {AssetCategory.NameMaxLength} characters or less.");
    }

    private static Task<IResult> BadAssetType()
    {
        return Task.FromResult<IResult>(Results.BadRequest("Asset type is not supported."));
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
