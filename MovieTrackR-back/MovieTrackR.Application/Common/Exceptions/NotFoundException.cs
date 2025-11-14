namespace MovieTrackR.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public string Resource { get; }
    public object? Key { get; }

    public NotFoundException(string resource, object? key = null)
        : base(key is null ? $"{resource} not found." : $"{resource} ({key}) not found.")
    {
        Resource = resource;
        Key = key;
    }
}