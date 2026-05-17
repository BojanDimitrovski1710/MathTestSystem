namespace MathTestSystem.TeacherApp.State;

public class AuthState
{
    public string? Token { get; private set; }
    public string? Username { get; private set; }
    public bool IsAuthenticated => Token is not null;

    public void SetToken(string token, string username)
    {
        Token = token;
        Username = username;
    }

    public void Clear()
    {
        Token = null;
        Username = null;
    }
}
