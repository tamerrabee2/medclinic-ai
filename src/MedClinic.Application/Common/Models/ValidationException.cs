namespace MedClinic.Application.Common.Models;

public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base(string.Join(" | ", errors))
    {
        Errors = errors;
    }

    public ValidationException(string error)
        : base(error)
    {
        Errors = new[] { error };
    }
}
