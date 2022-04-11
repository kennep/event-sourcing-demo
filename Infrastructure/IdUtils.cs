namespace UserService.Infrastructure;

public static class IdUtils
{
    public static string GenerateRandomId()
    {
        var id = new byte[16];
        Random.Shared.NextBytes(id);
        return Convert.ToBase64String(id);
    }
}