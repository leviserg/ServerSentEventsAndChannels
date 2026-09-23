using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace WebApi_SSE.Services
{
    public class TokenService
    {
        private readonly ConcurrentDictionary<string, string> _tokenToUserEmail = new();

        private readonly TimeSpan _tokenExpiration = TimeSpan.FromSeconds(900);

        public string GenerateToken(string userEmail)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            _tokenToUserEmail.TryAdd(token, userEmail);

            // Schedule token removal after expiration

            _ = Task.Run(async () =>
            {
                await Task.Delay(_tokenExpiration);
                _tokenToUserEmail.TryRemove(token, out _);
            });

            return token;
        }

        public string? ValidateToken(string token)
        {
            return _tokenToUserEmail.TryRemove(token, out var userEmail) ? userEmail : null;
        }

    }
}
