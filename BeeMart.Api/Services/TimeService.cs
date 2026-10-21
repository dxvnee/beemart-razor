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

    public DateTime LocalNow => ToLocal(DateTime.UtcNow).DateTime;

    public DateTimeOffset ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)), _zone);
}