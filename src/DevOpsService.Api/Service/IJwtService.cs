namespace DevOpsService.Api.Services;

public interface IJwtService
{
    bool IsJwtUnique(string jwt);
    void MarkJwtAsUsed(string jwt);
}