using api.Models;

namespace api.Contracts;

public sealed record CreateAssetRequest(
    string? Symbol,
    string? Name,
    string? AssetType,
    string? Category
);

public sealed record AssetResponse(
    int Id,
    string Symbol,
    string Name,
    string AssetType,
    string? Category,
    DateTime CreatedOn
);

public sealed record AssetCreateResult(
    AssetResponse? Asset,
    string? Message,
    bool Duplicate
);

public sealed record AssetCommand(
    string Symbol,
    string Name,
    AssetType AssetType,
    string? Category
);
