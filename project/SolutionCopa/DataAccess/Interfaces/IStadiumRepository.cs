using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IStadiumRepository
{
    Stadium? GetById(int stadiumId);
}
