# Serilog.Enrichers.HttpRequestLogContext

[![build-and-test](https://github.com/MonticolaExplorator/serilog-httprequestlogcontext/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MonticolaExplorator/serilog-httprequestlogcontext/actions/workflows/dotnet.yml) [![release](https://github.com/MonticolaExplorator/serilog-httprequestlogcontext/actions/workflows/release.yml/badge.svg)](https://github.com/MonticolaExplorator/serilog-httprequestlogcontext/actions/workflows/release.yml)

A [Serilog](https://serilog.net/) enricher to add properties to your log events scoped to a [ASP.NET Core HttpRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest). 

## Getting started

Add the Serilog namespace to your startup or program C# file:

```csharp
using Serilog;
```

Include the Http request log context enricher in your logger configuration:

```csharp
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.Enrich.FromHttpRequestLogContext()
    //rest of the configuration...
```

The `FromHttpRequestLogContext()` enricher adds the properties present on the `Serilog.Context.HttpRequestLogContext`, to all log events produced on the scope of an Http request.

The properties can be added and removed from the Http request log context using `HttpRequestLogContext.PushProperty`:

```csharp
HttpRequestLogContext.PushProperty("User", "Jonh_Doe");
HttpRequestLogContext.PushProperty("APIVersion", "1.0.1.239");
```

After the above code is executed, any log event written to any Serilog sink on the scope of the current Http request contain the properties `User` and `APIVersion` automatically. 

### Removing properties

The `Serilog.Context.HttpRequestLogContext` is automatically cleared when the current Http request ends. However, properties can also be removed manually from the context by disposing the object returned by 
the `HttpRequestLogContext.PushProperty` method:

```csharp
HttpRequestLogContext.PushProperty("A", 1);

Log.Information("Carries property A = 1");

using (HttpRequestLogContext.PushProperty("A", 2))
using (HttpRequestLogContext.PushProperty("B", 1))
{
    Log.Information("Carries A = 2 and B = 1");
}

Log.Information("Carries property A = 1, again");
```

Pushing a property onto the `Serilog.Context.HttpRequestLogContext` will override any existing properties with the same name, until the object returned from `PushProperty()` is disposed, as the property `A` in the example demonstrates.

**Important:** popping a property also pops all the properties pushed to the http context on top of it, as the next example demonstrates.

```csharp


Log.Information("Carries no properties");

using (HttpRequestLogContext.PushProperty("A", 1))
{
    HttpRequestLogContext.PushProperty("B", 1);
    Log.Information("Carries A = 1 and B = 1");
}

Log.Information("Carries no properties, again");
```

### Use case

