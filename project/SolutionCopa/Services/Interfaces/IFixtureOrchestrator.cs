namespace Services.Interfaces;

public interface IFixtureOrchestrator
{
    void GenerateGroupStage(int? fixtureSeedOverride, string? selectedEngineName, string? performedByEmail);
    void RegenerateGroupStage(int? fixtureSeedOverride, string? selectedEngineName, string? performedByEmail);
    void DeleteGroupStage(string? performedByEmail);
    bool IsFixtureGenerated();
    bool IsKnockoutGenerated();
}
