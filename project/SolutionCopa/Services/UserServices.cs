
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
        if (users.Any(u => u.email == user.email) || users.Any(u => u.idUser == user.idUser))
            throw new ArgumentException("Email already exists");

        users.Add(user);
    }
    
    
    public User GetUserByEmail(string email)
    {
        User user = users.FirstOrDefault(u => u.email == email);

        if (user == null)
            throw new ArgumentException("User not found");

        return user;
    }

    
    public User GetUserById(int id)
    {
        User user = users.FirstOrDefault(u => u.idUser == id);

        if (user == null)
            throw new ArgumentException("User not found");

        return user;
    }

    
    public void RemoveUser(int id)
    {
        User user = GetUserById(id);
        users.Remove(user);
    }

    
    public void UpdateUser(int id, string name, string lastname, string email)
    {
        User user = GetUserById(id);

        if (users.Any(u => u.email == email && u.idUser != id))
            throw new ArgumentException("Email already exists");

        user.UpdatePersonalData(name, lastname, email);
    }

    
    

 

}