using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.Interfaces;

public interface IStadiumService
{
    IReadOnlyCollection<Stadium> GetAllStadiums();
    void SeedDefaultStadiums(string? performedByEmail);
    void AddStadium(Stadium stadium, string? performedByEmail = null);
    Stadium GetById(int id);
    Stadium GetByName(string name);
    void RemoveById(int id, string? performedByEmail = null);
    void UpdateById(int id, string city, string? description, int capacity, string? performedByEmail = null);
}
