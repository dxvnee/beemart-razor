namespace BeeMart.Api.Services;

public interface ITimeService
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
    DateTimeOffset ToLocal(DateTime utc);
}