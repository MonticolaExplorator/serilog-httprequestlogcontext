using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.ScopedLogContext;
sealed class HttpRequestLogContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpRequestLogContext.Enrich(logEvent, propertyFactory);
    }
}

