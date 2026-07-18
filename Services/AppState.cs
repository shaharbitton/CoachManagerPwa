namespace CoachManagerPwa.Services;

public class AppState
{
    public string CurrentRole { get; private set; } = "Coach";
    public string CurrentUserId { get; private set; } = string.Empty;
    public string CurrentUserName { get; private set; } = string.Empty;
    public bool IsAuthenticated { get; private set; }

    public event Action? OnChange;

    public void SetRole(string role)
    {
        CurrentRole = role;
        OnChange?.Invoke();
    }

    public void SetUser(string userId, string name, string role)
    {
        CurrentUserId = userId;
        CurrentUserName = name;
        CurrentRole = role;
        IsAuthenticated = true;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        CurrentUserId = string.Empty;
        CurrentUserName = string.Empty;
        CurrentRole = "Coach";
        IsAuthenticated = false;
        OnChange?.Invoke();
    }

    public bool IsAdmin => CurrentRole is "Admin" or "Operations_Lead";
    public bool IsCoach => CurrentRole == "Coach";
}
