using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.HttpRequestLogContext.Test.Support
{
    public class DelegatingSink : ILogEventSink
    {
        readonly Action<LogEvent> _write;

        public DelegatingSink(Action<LogEvent> write)
        {
            _write = write ?? throw new ArgumentNullException(nameof(write));
        }

        public void Emit(LogEvent logEvent)
        {
            _write(logEvent);
        }

        public static LogEvent GetLogEvent(Action<ILogger> writeAction, Func<LoggerConfiguration, LoggerConfiguration>? configure = null)
        {
            LogEvent? result = null;
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Sink(new DelegatingSink(le => result = le));

            if (configure != null)
                configuration = configure(configuration);

            var l = configuration.CreateLogger();

            writeAction(l);
            Assert.NotNull(result);
            return result!;
        }
    }
}
