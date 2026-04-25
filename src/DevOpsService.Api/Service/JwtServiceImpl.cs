using System.Collections.Concurrent;

namespace DevOpsService.Api.Services;

public class JwtService : IJwtService
{
    private readonly ConcurrentDictionary<string, bool> _usedTokens = new();

    public bool IsJwtUnique(string jwt) =>
        !_usedTokens.ContainsKey(jwt);

    public void MarkJwtAsUsed(string jwt) =>
        _usedTokens.TryAdd(jwt, true);
}