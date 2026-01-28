using Buglens.Model;

namespace Buglens.Contract.IServices;

public interface ITokenService
{
    string GenerateToken(User user);
    int? ValidateToken(string token);
}