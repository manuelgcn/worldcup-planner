using ProjectCopa;

namespace Services.Interfaces;

public interface IStadiumService
{
    IReadOnlyCollection<Stadium> GetAllStadiums();
    void SeedDefaultStadiums(string? performedByEmail);
    void AddStadium(Stadium stadium);
    Stadium GetById(int id);
    Stadium GetByName(string name);
    void RemoveById(int id);
    void UpdateById(int id, string city, string? description, int capacity);
}
