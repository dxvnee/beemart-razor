namespace BeeMart.Api.Services;

public interface ITimeService
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
    DateTimeOffset ToLocal(DateTime utc);
    DateOnly LocalToday { get; }
    (DateTime StartUtc, DateTime EndUtc) GetUtcRange(DateOnly localDate);
}