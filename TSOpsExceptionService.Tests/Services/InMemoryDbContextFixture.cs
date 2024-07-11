using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TSOpsExceptionService.Data;

namespace TSOpsExceptionService.Tests.Services
{
    public class InMemoryLogger<T> : ILogger<T>
    {
        private readonly List<LogEntry> _logs = new List<LogEntry>();

        public IReadOnlyList<LogEntry> Logs => _logs;

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logs.Add(new LogEntry
            {
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            });
        }
    }

    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public class InMemoryDbContextFixture : IDisposable
    {
        public OpsMobWwfContext Context { get; private set; }

        public InMemoryDbContextFixture()
        {
            var options = new DbContextOptionsBuilder<OpsMobWwfContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new OpsMobWwfContext(options);
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
