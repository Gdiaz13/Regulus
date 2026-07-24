namespace Regulas.MauiApp.Models;

public sealed record StockComment(
    int Id,
    string Title,
    string Content,
    DateTime CreatedOn,
    int StockId
);

public sealed record CreateStockCommentRequest(
    string? Title,
    string? Content
);

public sealed record StockNoteRow(
    int Id,
    string Title,
    string Content,
    string Created
);
