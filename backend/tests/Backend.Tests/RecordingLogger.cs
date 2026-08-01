using Microsoft.Extensions.Logging;

namespace Backend.Tests;

/// Logger real (en memoria) que registra lo que se le emite, para poder assertar
/// sobre el comportamiento observable de auditoría. No es un mock de verificación:
/// implementa ILogger de verdad y guarda las entradas.
public class RecordingLogger<T> : ILogger<T>
{
    public record Entry(LogLevel Level, string Message);

    public List<Entry> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
        => Entries.Add(new Entry(logLevel, formatter(state, exception)));
}
