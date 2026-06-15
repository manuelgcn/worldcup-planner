using Services.Exceptions;
using Services.Interfaces;

namespace Services;

// OCP: agregar un motor nuevo es solo registrarlo en DI; esta clase y sus consumidores no cambian.
public sealed class SimulationEngineRegistry : ISimulationEngineRegistry
{
    private const string EnginesRequiredMessage = "Debe registrarse al menos un motor de simulacion.";
    private const string EngineNameRequiredMessage = "El nombre del motor es obligatorio para resolver un motor de simulacion.";
    private const string DuplicateEngineNameMessageTemplate = "Nombre de motor de simulacion duplicado: {0}.";
    private const string UnknownEngineNameMessageTemplate = "Motor de simulacion desconocido: {0}.";

    private readonly Dictionary<string, IMatchSimulationEngine> _enginesByName;
    private readonly List<string> _engineNames;

    public SimulationEngineRegistry(IEnumerable<IMatchSimulationEngine> engines)
    {
        ArgumentNullException.ThrowIfNull(engines);

        _enginesByName = new Dictionary<string, IMatchSimulationEngine>();
        _engineNames = new List<string>();

        foreach (IMatchSimulationEngine engine in engines)
        {
            RegisterEngine(engine);
        }

        EnsureAtLeastOneEngine();
    }

    public IReadOnlyCollection<string> AvailableEngineNames => _engineNames.AsReadOnly();

    public IMatchSimulationEngine Resolve(string engineName)
    {
        if (string.IsNullOrWhiteSpace(engineName))
        {
            throw new SimulationEngineException(EngineNameRequiredMessage);
        }

        if (!_enginesByName.TryGetValue(engineName, out IMatchSimulationEngine? engine))
        {
            throw new SimulationEngineException(string.Format(UnknownEngineNameMessageTemplate, engineName));
        }

        return engine;
    }

    private void RegisterEngine(IMatchSimulationEngine engine)
    {
        if (_enginesByName.ContainsKey(engine.Name))
        {
            throw new SimulationEngineException(string.Format(DuplicateEngineNameMessageTemplate, engine.Name));
        }

        _enginesByName.Add(engine.Name, engine);
        _engineNames.Add(engine.Name);
    }

    private void EnsureAtLeastOneEngine()
    {
        if (_enginesByName.Count == 0)
        {
            throw new SimulationEngineException(EnginesRequiredMessage);
        }
    }
}
