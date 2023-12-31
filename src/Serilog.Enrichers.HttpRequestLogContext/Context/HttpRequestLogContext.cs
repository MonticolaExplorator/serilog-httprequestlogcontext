﻿// Copyright 2013-2015 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Serilog.Core.Enrichers;
using Serilog.Core;
using Serilog.Events;
using System;
using Microsoft.AspNetCore.Http;

namespace Serilog.Context;

/// <summary>
/// Log context with the same lifecycle as the current Http request. Holds ambient properties that can be attached to log events. To
/// configure, use the <see cref="Serilog.LoggerEnrichmentConfigurationExtensions.FromHttpRequestLogContext"/> method.
/// </summary>
/// <example>
/// Configuration:
/// <code lang="C#">
/// var log = new LoggerConfiguration()
///     .Enrich.FromHttpRequestLogContext()
///     ...
/// </code>
/// </example>
public static class HttpRequestLogContext
{
    internal static IHttpContextAccessor _contextAccessor = new HttpContextAccessor();
    internal const string _httpContextItemName = "__LogContext";

    /// <summary>
    /// Push a property onto the context, returning an <see cref="IDisposable"/>
    /// that must later be used to remove the property, along with any others that
    /// may have been pushed on top of it and not yet popped. The property must
    /// be popped from the same thread/logical call context.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>A handle to later remove the property from the context.</returns>
    /// <param name="destructureObjects">If <see langword="true"/>, and the value is a non-primitive, non-array type,
    /// then the value will be converted to a structure; otherwise, unknown types will
    /// be converted to scalars, which are generally stored as strings.</param>
    /// <returns>A token that must be disposed, in order, to pop properties back off the stack.</returns>
    public static IDisposable PushProperty(string name, object? value, bool destructureObjects = false)
    {
        return Push(new PropertyEnricher(name, value, destructureObjects));
    }

    /// <summary>
    /// Push an enricher onto the context, returning an <see cref="IDisposable"/>
    /// that must later be used to remove the property, along with any others that
    /// may have been pushed on top of it and not yet popped. The property must
    /// be popped from the same thread/logical call context.
    /// </summary>
    /// <param name="enricher">An enricher to push onto the log context</param>
    /// <returns>A token that must be disposed, in order, to pop properties back off the stack.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="enricher"/> is <code>null</code></exception>
    public static IDisposable Push(ILogEventEnricher enricher)
    {
        enricher = enricher ?? throw new ArgumentNullException(nameof(enricher));

        var stack = GetOrCreateEnricherStack();
        var bookmark = new ContextStackBookmark(stack);

        Enrichers = stack.Push(enricher);

        return bookmark;
    }

    /// <summary>
    /// Push multiple enrichers onto the context, returning an <see cref="IDisposable"/>
    /// that must later be used to remove the property, along with any others that
    /// may have been pushed on top of it and not yet popped. The property must
    /// be popped from the same thread/logical call context.
    /// </summary>
    /// <seealso cref="PropertyEnricher"/>.
    /// <param name="enrichers">Enrichers to push onto the log context</param>
    /// <returns>A token that must be disposed, in order, to pop properties back off the stack.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="enrichers"/> is <code>null</code></exception>
    public static IDisposable Push(params ILogEventEnricher[] enrichers)
    {
        enrichers = enrichers ?? throw new ArgumentNullException(nameof(enrichers));

        var stack = GetOrCreateEnricherStack();
        var bookmark = new ContextStackBookmark(stack);

        for (var i = 0; i < enrichers.Length; ++i)
            stack = stack.Push(enrichers[i]);

        Enrichers = stack;

        return bookmark;
    }

    /// <summary>
    /// Remove all enrichers from the <see cref="LogContext"/>, returning an <see cref="IDisposable"/>
    /// that must later be used to restore enrichers that were on the stack before <see cref="Suspend"/> was called.
    /// </summary>
    /// <returns>A token that must be disposed, in order, to restore properties back to the stack.</returns>
    public static IDisposable Suspend()
    {
        var stack = GetOrCreateEnricherStack();
        var bookmark = new ContextStackBookmark(stack);

        Enrichers = EnricherStack.Empty;

        return bookmark;
    }

    /// <summary>
    /// Remove all enrichers from <see cref="LogContext"/> for the current async scope.
    /// </summary>
    public static void Reset()
    {
        if (Enrichers != null && Enrichers != EnricherStack.Empty)
        {
            Enrichers = EnricherStack.Empty;
        }
    }

    static EnricherStack GetOrCreateEnricherStack()
    {
        var enrichers = Enrichers;
        if (enrichers == null)
        {
            enrichers = EnricherStack.Empty;
            Enrichers = enrichers;
        }
        return enrichers;
    }

    internal static void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var enrichers = Enrichers;
        if (enrichers == null || enrichers == EnricherStack.Empty)
            return;

        foreach (var enricher in enrichers)
        {
            enricher.Enrich(logEvent, propertyFactory);
        }
    }

    sealed class ContextStackBookmark : IDisposable
    {
        readonly EnricherStack _bookmark;

        public ContextStackBookmark(EnricherStack bookmark)
        {
            _bookmark = bookmark;
        }

        public void Dispose()
        {
            Enrichers = _bookmark;
        }
    }

    static EnricherStack? Enrichers
    {
        get
        {
            HttpContext? context = _contextAccessor.HttpContext;
            if (context == null)
                return null;

            return context!.Items[_httpContextItemName] as EnricherStack;
        }
        set
        {
            HttpContext? context = _contextAccessor.HttpContext;
            if (context != null)
                context.Items[_httpContextItemName] = value;
        }
    }
}

