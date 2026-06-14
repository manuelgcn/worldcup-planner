using ProjectCopa;

namespace Services.DTOs;

// Request de creacion de usuario. Desacopla la UI de la construccion de la entidad
// de dominio User: la UI arma este DTO y el servicio crea el User (aplicando sus
// invariantes) y le asigna el rol inicial.
public sealed class CreateUserRequestDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Lastname { get; init; } = "";
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";
    public DateTime DateOfBirth { get; init; }
    public UserRoles InitialRole { get; init; }
}
