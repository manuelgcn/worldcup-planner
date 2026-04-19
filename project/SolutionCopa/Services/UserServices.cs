
using ProjectCopa;

namespace Services;

public class UserServices
{
    private List<User> users = new List<User>();

    public IReadOnlyCollection<User> GetAllUsers()
    {
        return users.AsReadOnly();
    }

    
    public void AddUser(User user)
    {
        if (users.Any(u => u.email == user.email))
            throw new ArgumentException("Email already exists");

        users.Add(user);
    }

}