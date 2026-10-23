using BeeMart.Api.Models;

namespace BeeMart.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) Generate(MsUser user);
}
