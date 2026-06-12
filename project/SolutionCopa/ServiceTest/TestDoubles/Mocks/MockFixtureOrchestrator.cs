using Services.Interfaces;

namespace ServiceTest.TestDoubles.Mocks;

// Doble explicito de IFixtureOrchestrator para los tests de servicios de lectura (dashboard):
// permite fijar los flags de fase sin montar el orquestador real ni sus repositorios.
public sealed class MockFixtureOrchestrator : IFixtureOrchestrator
{
    public bool FixtureGenerated { get; set; }
    public bool KnockoutGenerated { get; set; }

    public bool IsFixtureGenerated() => FixtureGenerated;

    public bool IsKnockoutGenerated() => KnockoutGenerated;

    public void GenerateGroupStage(int? fixtureSeedOverride, string? selectedEngineName, string? performedByEmail)
    {
    }

    public void RegenerateGroupStage(int? fixtureSeedOverride, string? selectedEngineName, string? performedByEmail)
    {
    }

    public void DeleteGroupStage(string? performedByEmail)
    {
    }
}
