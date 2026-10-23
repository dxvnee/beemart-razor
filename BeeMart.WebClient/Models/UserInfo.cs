namespace BeeMart.WebClient.Models;

public class UserInfo
{
    public string IdUser { get; set; } = "";
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public bool Stsrc { get; set; }
}
