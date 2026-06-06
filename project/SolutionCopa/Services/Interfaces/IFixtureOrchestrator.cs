namespace Services.Interfaces;

public interface IFixtureOrchestrator
{
    void GenerateGroupStage(int? fixtureSeedOverride, string? performedByEmail);
    void RegenerateGroupStage(int? fixtureSeedOverride, string? performedByEmail);
    void DeleteGroupStage(string? performedByEmail);
    bool IsFixtureGenerated();
    bool IsKnockoutGenerated();
}
