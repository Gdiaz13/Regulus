using api.Models;
using api.Services;

namespace api.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this WebApplication app)
    {
        var comments = app.MapGroup("/api");
        comments.MapGet("stocks/{stockId:int}/comments", GetStockComments);
        comments.MapPost("stocks/{stockId:int}/comments", CreateStockComment);
        comments.MapPut("comments/{id:int}", UpdateComment);
        comments.MapDelete("comments/{id:int}", DeleteComment);
    }

    private static Task<IResult> GetStockComments(int stockId, StockCommentStore store)
    {
        return DatabaseRequest.Run(async () => Results.Ok(await ListResponses(stockId, store)));
    }

    private static async Task<List<CommentResponse>> ListResponses(int stockId, StockCommentStore store)
    {
        var comments = await store.ListAsync(stockId);
        return comments.Select(ToResponse).ToList();
    }

    private static Task<IResult> CreateStockComment(
        int stockId,
        CreateCommentRequest request,
        StockCommentStore store
    )
    {
        return DatabaseRequest.Run(() => CreateStockCommentCore(stockId, request, store));
    }

    private static async Task<IResult> CreateStockCommentCore(
        int stockId,
        CreateCommentRequest request,
        StockCommentStore store
    )
    {
        var validation = await ValidateCreateRequest(stockId, request, store);
        if (validation is not null)
        {
            return validation;
        }
        var comment = await store.CreateAsync(NewComment(stockId, request));
        return Results.Created($"/api/comments/{comment.Id}", ToResponse(comment));
    }

    private static Task<IResult> UpdateComment(int id, CreateCommentRequest request, StockCommentStore store)
    {
        return DatabaseRequest.Run(() => UpdateCommentCore(id, request, store));
    }

    private static async Task<IResult> UpdateCommentCore(int id, CreateCommentRequest request, StockCommentStore store)
    {
        var validation = ValidateCommentBody(request);
        if (validation is not null)
        {
            return validation;
        }
        var comment = await store.UpdateAsync(id, Clean(request.Title), Clean(request.Content));
        return comment is null ? CommentMissing(id) : Results.Ok(ToResponse(comment));
    }

    private static Task<IResult> DeleteComment(int id, StockCommentStore store)
    {
        return DatabaseRequest.Run(async () => await DeleteCommentCore(id, store));
    }

    private static async Task<IResult> DeleteCommentCore(int id, StockCommentStore store)
    {
        return await store.DeleteAsync(id) ? Results.NoContent() : CommentMissing(id);
    }

    private static async Task<IResult?> ValidateCreateRequest(
        int stockId,
        CreateCommentRequest request,
        StockCommentStore store
    )
    {
        var validation = ValidateCommentBody(request);
        if (validation is not null)
        {
            return validation;
        }
        return await store.StockExistsAsync(stockId) ? null : Results.NotFound($"Stock with id {stockId} was not found.");
    }

    private static Comment NewComment(int stockId, CreateCommentRequest request)
    {
        return new Comment
        {
            StockId = stockId,
            Title = Clean(request.Title),
            Content = Clean(request.Content),
            CreatedOn = DateTime.UtcNow,
        };
    }

    private static IResult? ValidateCommentBody(CreateCommentRequest request)
    {
        return IsBlank(request.Title) || IsBlank(request.Content)
            ? Results.BadRequest("A note title and content are required.")
            : null;
    }

    private static CommentResponse ToResponse(Comment comment)
    {
        return new CommentResponse(comment.Id, comment.Title, comment.Content, comment.CreatedOn, comment.StockId);
    }

    private static IResult CommentMissing(int id)
    {
        return Results.NotFound($"Comment with id {id} was not found.");
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
    int StockId
);
