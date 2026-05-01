using ProjectCopa.Interfaces;

namespace DataAccess.Interfaces;

public interface IJourneyRepository
{
    IJourney? GetById(int journeyId);
}
