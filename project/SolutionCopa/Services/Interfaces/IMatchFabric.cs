using ProjectCopa;
using Services.DTOs;

namespace Services.Interfaces;

public interface IMatchFactory
{
    Match Create(CreateMatchRequestDto request);
    void ApplyUpdate(Match match, UpdateMatchRequestDto request);
}