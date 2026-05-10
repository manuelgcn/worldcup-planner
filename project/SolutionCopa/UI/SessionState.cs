using ProjectCopa;

namespace UI;

public class SessionState
{
    public User? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser != null;

    public event Action? OnChange;

    public void Login(User user)
    {
        CurrentUser = user;
        NotifyStateChanged();
    }

    public void Logout()
    {
        CurrentUser = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}