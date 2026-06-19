using api.Data;
using api.Models;
using api.Services;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this WebApplication app)
    {
        var comments = app.MapGroup("/api");
        comments.MapGet("stocks/{stockId:int}/comments", GetStockComments);
        comments.MapPost("stocks/{stockId:int}/comments", CreateStockComment);
        comments.MapDelete("comments/{id:int}", DeleteComment);
    }

    private static Task<IResult> GetStockComments(int stockId, ApplicationDBContext db)
    {
        return DatabaseRequest.Run(async () => Results.Ok(await ListStockComments(stockId, db)));
    }

    private static Task<List<CommentResponse>> ListStockComments(int stockId, ApplicationDBContext db)
    {
        return db.Comments
            .AsNoTracking()
            .Where(comment => comment.StockId == stockId)
            .OrderByDescending(comment => comment.CreatedOn)
            .Select(comment => new CommentResponse(
                comment.Id,
                comment.Title,
                comment.Content,
                comment.CreatedOn,
                comment.StockId
            ))
            .ToListAsync();
    }

    private static Task<IResult> CreateStockComment(
        int stockId,
        CreateCommentRequest request,
        ApplicationDBContext db
    )
    {
        return DatabaseRequest.Run(() => CreateStockCommentCore(stockId, request, db));
    }

    private static async Task<IResult> CreateStockCommentCore(
        int stockId,
        CreateCommentRequest request,
        ApplicationDBContext db
    )
    {
        var validation = await ValidateCreateRequest(stockId, request, db);
        if (validation is not null)
        {
            return validation;
        }
        return await SaveComment(stockId, request, db);
    }

    private static Task<IResult> DeleteComment(int id, ApplicationDBContext db)
    {
        return DatabaseRequest.Run(() => DeleteCommentCore(id, db));
    }

    private static async Task<IResult> DeleteCommentCore(int id, ApplicationDBContext db)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment is null)
        {
            return Results.NotFound($"Comment with id {id} was not found.");
        }
        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult?> ValidateCreateRequest(
        int stockId,
        CreateCommentRequest request,
        ApplicationDBContext db
    )
    {
        if (IsBlank(request.Title) || IsBlank(request.Content))
        {
            return Results.BadRequest("A note title and content are required.");
        }
        return await StockExists(stockId, db) ? null : Results.NotFound($"Stock with id {stockId} was not found.");
    }

    private static async Task<IResult> SaveComment(
        int stockId,
        CreateCommentRequest request,
        ApplicationDBContext db
    )
    {
        var comment = CreateComment(stockId, request);
        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        return Results.Created($"/api/comments/{comment.Id}", ToResponse(comment));
    }

    private static Comment CreateComment(int stockId, CreateCommentRequest request)
    {
        return new Comment
        {
            StockId = stockId,
            Title = Clean(request.Title),
            Content = Clean(request.Content),
            CreatedOn = DateTime.UtcNow,
        };
    }

    private static CommentResponse ToResponse(Comment comment)
    {
        return new CommentResponse(comment.Id, comment.Title, comment.Content, comment.CreatedOn, comment.StockId);
    }

    private static Task<bool> StockExists(int stockId, ApplicationDBContext db)
    {
        return db.Stocks.AnyAsync(stock => stock.Id == stockId);
    }

    private static bool IsBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    private static string Clean(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}

public sealed record CreateCommentRequest(string? Title, string? Content);

public sealed record CommentResponse(
    int Id,
    string Title,
    string Content,
    DateTime CreatedOn,
    int? StockId
);
