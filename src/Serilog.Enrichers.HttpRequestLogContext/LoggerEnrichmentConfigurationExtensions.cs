﻿using Serilog.Configuration;
using Serilog.Enrichers.ScopedLogContext;

namespace Serilog;

/// <summary>
/// Extends <see cref="LoggerEnrichmentConfiguration"/> with scoped enrichment methods.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Enrich log events with properties from <see cref="Context.HttpRequestLogContext"/>.
    /// </summary>
    /// <returns>Configuration object allowing method chaining.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration FromHttpRequestLogContext(this LoggerEnrichmentConfiguration enrich)
    {
        return enrich.With<HttpRequestLogContextEnricher>();
    }
}
