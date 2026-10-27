namespace BeeMart.Api.Services;

public class TimeService : ITimeService
{
    private readonly TimeZoneInfo _zone;

    public TimeService(IConfiguration config)
    {
        var id = config["TimeZoneId"] ?? "Asia/Jakarta";
        try
        {
            _zone = TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            _zone = TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        }
    }
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => ToLocal(DateTime.UtcNow);

    public DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _zone);

    public DateOnly LocalToday => DateOnly.FromDateTime(LocalNow);

    public (DateTime StartUtc, DateTime EndUtc) GetUtcRange(DateOnly localDate)
    {
        var startLocal = DateTime.SpecifyKind(localDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        var endLocal = startLocal.AddDays(1);

        return (TimeZoneInfo.ConvertTimeToUtc(startLocal, _zone),
                TimeZoneInfo.ConvertTimeToUtc(endLocal, _zone));
    }
}