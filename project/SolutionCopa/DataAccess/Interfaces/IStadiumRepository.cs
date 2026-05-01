using ProjectCopa.Interfaces;

namespace DataAccess.Interfaces;

public interface IStadiumRepository
{
    IStadium? GetById(int stadiumId);
}
