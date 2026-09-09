namespace MedClinic.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; init; }
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
    public static Result Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors };
}

public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public new static Result<T> Failure(params string[] errors) => new() { Succeeded = false, Errors = errors };
    public new static Result<T> Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors };
}

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PaginatedList(List<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
