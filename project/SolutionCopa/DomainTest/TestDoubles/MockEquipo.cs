using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class MockEquipo : IEquipo
{
    public int idEquipo { get; }
    public string nombre { get; }

    public MockEquipo(int idEquipo = 1, string nombre = "Mock Equipo")
    {
        this.idEquipo = idEquipo;
        this.nombre = nombre;
    }
}

