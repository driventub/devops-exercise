using System.Collections.Concurrent;

namespace DevOpsService.Api.Service
{
    public class JwtService : IJwtService
    {
        private readonly ConcurrentDictionary<string, bool> _usedTokens = new();

        public bool IsJwtUnique(string jwt)
        {
            return !_usedTokens.ContainsKey(jwt);
        }

        public void MarkJwtAsUsed(string jwt)
        {
            _ = _usedTokens.TryAdd(jwt, true);
        }
    }
}

