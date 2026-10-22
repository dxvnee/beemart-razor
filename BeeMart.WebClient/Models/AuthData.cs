namespace BeeMart.WebClient.Models;

public class AuthData
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = null!;
}
