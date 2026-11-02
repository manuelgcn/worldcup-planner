namespace WorldCupPlanner.Services.Tests.TestDoubles.Mocks;

// Reloj que solo avanza cuando el test lo pide: permite probar ventanas de tiempo sin esperar.
public sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset now = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => now;

    public void Advance(TimeSpan elapsed) => now = now.Add(elapsed);
}
