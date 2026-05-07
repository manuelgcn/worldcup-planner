using ProjectCopa;

namespace DataAccess.Interfaces;

public interface IJourneyRepository
{
    Journey? GetById(int journeyId);
}
