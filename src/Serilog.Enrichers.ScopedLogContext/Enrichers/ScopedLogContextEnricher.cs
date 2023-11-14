using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.ScopedLogContext;
sealed class ScopedLogContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Context.ScopedLogContext.Enrich(logEvent, propertyFactory);
    }
}

