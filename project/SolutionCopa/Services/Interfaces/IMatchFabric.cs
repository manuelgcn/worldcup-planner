using DataAccess.Interfaces;
using ProjectCopa;
using Services.DTOs;
using Services.Exceptions;

namespace Services.Interfaces;

public interface IMatchFactory
{
    Match Create(CreateMatchRequestDto request);
    void ApplyUpdate(Match match, UpdateMatchRequestDto request);
}