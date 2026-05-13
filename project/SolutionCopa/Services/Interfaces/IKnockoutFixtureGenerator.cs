using ProjectCopa;
using ProjectCopa.Models;

namespace Services.Interfaces;

public interface IKnockoutFixtureGenerator
{
    List<Match> GeneratePhase2(QualifiedTeams qualifiedTeams);
}
