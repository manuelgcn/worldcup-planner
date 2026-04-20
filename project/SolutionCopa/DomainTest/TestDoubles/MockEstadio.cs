using ProjectCopa.Interfaces;

namespace DomainTest.TestDoubles;

public class MockEstadio : IEstadio
{
    public int idEstadio { get; }
    public string nombre { get; }

    public MockEstadio(int idEstadio = 1, string nombre = "Mock Estadio")
    {
        this.idEstadio = idEstadio;
        this.nombre = nombre;
    }
}

