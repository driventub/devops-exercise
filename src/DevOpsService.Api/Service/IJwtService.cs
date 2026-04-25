namespace DevOpsService.Api.Service
{
    public interface IJwtService
    {
        bool IsJwtUnique(string jwt);
        void MarkJwtAsUsed(string jwt);
    }
}

