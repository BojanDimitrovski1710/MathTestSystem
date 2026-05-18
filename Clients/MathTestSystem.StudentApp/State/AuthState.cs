namespace MathTestSystem.StudentApp.State;

public class AuthState
{
    public string? Token { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public bool IsAuthenticated => Token is not null;

    public void SetToken(string token, string username, string role)
    {
        Token = token;
        Username = username;
        Role = role;
    }

    public void Clear()
    {
        Token = null;
        Username = null;
        Role = null;
    }
}
