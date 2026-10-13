using WorldCupPlanner.Domain;
using WorldCupPlanner.Services.DTOs;

namespace WorldCupPlanner.Services.Interfaces;

public interface IMatchFactory
{
    Match Create(CreateMatchRequestDto request);
    void ApplyUpdate(Match match, UpdateMatchRequestDto request);
}