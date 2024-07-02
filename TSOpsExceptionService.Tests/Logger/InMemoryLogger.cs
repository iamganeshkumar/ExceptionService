using Microsoft.Extensions.Logging;

namespace TSOpsExceptionService.Tests.Logger
{
    public class InMemoryLogger<T> : ILogger<T>
    {
        private readonly List<LogEntry> _logs = new();

        public IReadOnlyList<LogEntry> Logs => _logs;

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);
                _logs.Add(new LogEntry { LogLevel = logLevel, Message = message, Exception = exception });
                Console.WriteLine($"Logged: {logLevel} - {message}");
            }
        }

        public class LogEntry
        {
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; }
            public Exception Exception { get; set; }
        }
    }
}
