using Microsoft.AspNetCore.Http;
using Serilog.Core.Enrichers;
using Serilog.Enrichers.HttpRequestLogContext.Test.Support;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Enrichers.HttpRequestLogContext.Test.Context
{
    public class HttpRequestLogContextTests
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public HttpRequestLogContextTests()
        {
            HttpContext httpContext = new DefaultHttpContext();
            _contextAccessor = new HttpContextAccessor();
            _contextAccessor.HttpContext = httpContext;
        }

        [Fact]
        public void WhenThereIsNoHttpContext()
        {
            LogEvent? lastEvent = null;
            _contextAccessor.HttpContext = null;
            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();

            using (Serilog.Context.HttpRequestLogContext.PushProperty("A", 1))
            {
                log.Write(Some.InformationEvent());
                Assert.NotNull(lastEvent);
                Assert.Empty(lastEvent!.Properties);
            }
        }

        [Fact]
        public void PushedPropertiesAreAvailableToLoggers()
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();

            using (Serilog.Context.HttpRequestLogContext.PushProperty("A", 1))
            using (Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("B", 2)))
            using (Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("C", 3), new PropertyEnricher("D", 4))) // Different overload
            {
                log.Write(Some.InformationEvent());
                Assert.NotNull(lastEvent);
                Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["B"].LiteralValue());
                Assert.Equal(3, lastEvent.Properties["C"].LiteralValue());
                Assert.Equal(4, lastEvent.Properties["D"].LiteralValue());
            }
        }

        [Fact]
        public void DisposePropertyDisposesAllOnTopOfIt()
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();

            using (Serilog.Context.HttpRequestLogContext.PushProperty("A", 1))
            {
                Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("B", 2));
                Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("C", 3), new PropertyEnricher("D", 4));

                log.Write(Some.InformationEvent());
                Assert.NotNull(lastEvent);
                Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["B"].LiteralValue());
                Assert.Equal(3, lastEvent.Properties["C"].LiteralValue());
                Assert.Equal(4, lastEvent.Properties["D"].LiteralValue());
            }

            log.Write(Some.InformationEvent());

            Assert.False(lastEvent!.Properties.ContainsKey("A"));
            Assert.False(lastEvent.Properties.ContainsKey("B"));
            Assert.False(lastEvent.Properties.ContainsKey("C"));
            Assert.False(lastEvent.Properties.ContainsKey("D"));
        }

        [Fact]
        public void PushedPropertiesAreNotAvailableToLoggersWhenDisposed()
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();

            using (Serilog.Context.HttpRequestLogContext.PushProperty("A", 1))
            using (Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("B", 2)))
            using (Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("C", 3), new PropertyEnricher("D", 4))) // Different overload
            {
                log.Write(Some.InformationEvent());
                Assert.NotNull(lastEvent);
                Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["B"].LiteralValue());
                Assert.Equal(3, lastEvent.Properties["C"].LiteralValue());
                Assert.Equal(4, lastEvent.Properties["D"].LiteralValue());
            }
            log.Write(Some.InformationEvent());
            Assert.NotNull(lastEvent);
            Assert.False(lastEvent!.Properties.ContainsKey("A"));
            Assert.False(lastEvent.Properties.ContainsKey("B"));
            Assert.False(lastEvent.Properties.ContainsKey("C"));
            Assert.False(lastEvent.Properties.ContainsKey("D"));

        }

        [Fact]
        public void PushedPropertiesAreNotAvailableToLoggersWhenRequestEnds()
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();

            Serilog.Context.HttpRequestLogContext.PushProperty("A", 1);
            Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("B", 2));
            Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("C", 3), new PropertyEnricher("D", 4)); // Different overload

            log.Write(Some.InformationEvent());
            Assert.NotNull(lastEvent);
            Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());
            Assert.Equal(2, lastEvent.Properties["B"].LiteralValue());
            Assert.Equal(3, lastEvent.Properties["C"].LiteralValue());
            Assert.Equal(4, lastEvent.Properties["D"].LiteralValue());

            _contextAccessor.HttpContext = null;
            log.Write(Some.InformationEvent());
            Assert.NotNull(lastEvent);
            Assert.False(lastEvent!.Properties.ContainsKey("A"));
            Assert.False(lastEvent.Properties.ContainsKey("B"));
            Assert.False(lastEvent.Properties.ContainsKey("C"));
            Assert.False(lastEvent.Properties.ContainsKey("D"));

        }

        [Fact]
        public void MoreNestedPropertiesOverrideLessNestedOnes()
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();

            Serilog.Context.HttpRequestLogContext.PushProperty("A", 1);

            log.Write(Some.InformationEvent());
            Assert.NotNull(lastEvent);
            Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());

            using (Serilog.Context.HttpRequestLogContext.PushProperty("A", 2))
            {
                log.Write(Some.InformationEvent());
                Assert.Equal(2, lastEvent.Properties["A"].LiteralValue());
            }

            log.Write(Some.InformationEvent());
            Assert.Equal(1, lastEvent.Properties["A"].LiteralValue());

            _contextAccessor.HttpContext = null;
            log.Write(Some.InformationEvent());
            Assert.False(lastEvent.Properties.ContainsKey("A"));
        }

        [Fact]
        public void MultipleNestedPropertiesOverrideLessNestedOnes()
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();


            using (Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("A1", 1), new PropertyEnricher("A2", 2)))
            {
                log.Write(Some.InformationEvent());
                Assert.NotNull(lastEvent);
                Assert.Equal(1, lastEvent!.Properties["A1"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["A2"].LiteralValue());

                using (Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("A1", 10), new PropertyEnricher("A2", 20)))
                {
                    log.Write(Some.InformationEvent());
                    Assert.Equal(10, lastEvent.Properties["A1"].LiteralValue());
                    Assert.Equal(20, lastEvent.Properties["A2"].LiteralValue());
                }

                log.Write(Some.InformationEvent());
                Assert.Equal(1, lastEvent.Properties["A1"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["A2"].LiteralValue());
            }

            log.Write(Some.InformationEvent());
            Assert.False(lastEvent.Properties.ContainsKey("A1"));
            Assert.False(lastEvent.Properties.ContainsKey("A2"));
        }

        [Fact]
        public async Task ContextPropertiesCrossAsyncCalls()
        {
            await TestWithSyncContext(async () =>
            {
                LogEvent? lastEvent = null;

                var log = new LoggerConfiguration()
                    .Enrich.FromHttpRequestLogContext()
                    .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                    .CreateLogger();

                using (Serilog.Context.HttpRequestLogContext.PushProperty("A", 1))
                {
                    var pre = Thread.CurrentThread.ManagedThreadId;

                    await Task.Yield();

                    var post = Thread.CurrentThread.ManagedThreadId;

                    log.Write(Some.InformationEvent());
                    Assert.NotNull(lastEvent);
                    Assert.Equal(1, lastEvent.Properties["A"].LiteralValue());

                    Assert.False(Thread.CurrentThread.IsThreadPoolThread);
                    Assert.True(Thread.CurrentThread.IsBackground);
                    Assert.NotEqual(pre, post);
                }
            },
                new ForceNewThreadSyncContext());
        }

        [Fact]
        public async Task ContextEnrichersInAsyncScopeCanBeCleared()
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();

            using (Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("A", 1)))
            {
                await Task.Run(() =>
                {
                    Serilog.Context.HttpRequestLogContext.Reset();
                    log.Write(Some.InformationEvent());
                });

                Assert.NotNull(lastEvent);
                Assert.Empty(lastEvent!.Properties);

                // Reset should only work for the whole async scope
                log.Write(Some.InformationEvent());
                Assert.Empty(lastEvent!.Properties);
            }
        }

        [Fact]
        public async Task ContextEnrichersCanBeTemporarilyCleared()
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromHttpRequestLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();

            using (Serilog.Context.HttpRequestLogContext.Push(new PropertyEnricher("A", 1)))
            {
                using (Serilog.Context.HttpRequestLogContext.Suspend())
                {
                    await Task.Run(() =>
                    {
                        log.Write(Some.InformationEvent());
                    });

                    Assert.NotNull(lastEvent);
                    Assert.Empty(lastEvent!.Properties);
                }

                // Suspend should only work for scope of using. After calling Dispose all enrichers
                // should be restored.
                log.Write(Some.InformationEvent());
                Assert.Equal(1, lastEvent.Properties["A"].LiteralValue());
            }
        }

        static async Task TestWithSyncContext(Func<Task> testAction, SynchronizationContext syncContext)
        {
            var prevCtx = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(syncContext);

            Task t;
            try
            {
                t = testAction();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }

            await t;
        }
    }

    class ForceNewThreadSyncContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state) => new Thread(x => d(x)) { IsBackground = true }.Start(state);
    }
}
