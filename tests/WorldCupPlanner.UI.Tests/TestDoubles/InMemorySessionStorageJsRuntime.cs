using Microsoft.JSInterop;

namespace WorldCupPlanner.UI.Tests.TestDoubles;

// El sessionStorage de una pestaña: lo único del navegador que usa ProtectedSessionStorage.
// Duplicar una pestaña copia su sessionStorage, y CopyTab hace lo mismo.
public sealed class InMemorySessionStorageJsRuntime : IJSRuntime
{
    private readonly Dictionary<string, string> items;

    public InMemorySessionStorageJsRuntime() : this(new Dictionary<string, string>())
    {
    }

    private InMemorySessionStorageJsRuntime(Dictionary<string, string> items)
    {
        this.items = items;
    }

    public bool IsEmpty => items.Count == 0;

    public InMemorySessionStorageJsRuntime CopyTab() => new(new Dictionary<string, string>(items));

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        string key = (string)args![0]!;

        switch (identifier)
        {
            case "sessionStorage.getItem":
                return new ValueTask<TValue>((TValue)(object?)items.GetValueOrDefault(key)!);
            case "sessionStorage.setItem":
                items[key] = (string)args[1]!;
                return default;
            case "sessionStorage.removeItem":
                items.Remove(key);
                return default;
            default:
                throw new NotSupportedException(identifier);
        }
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        InvokeAsync<TValue>(identifier, args);
}
