using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Enrichers.HttpRequestLogContext.Test.Support
{
    internal class Some
    {
        private static int Counter;

        public static int Int() => Interlocked.Increment(ref Counter);

        public static string String(string? tag = null) => (tag ?? "") + "__" + Int();

        public static TimeSpan TimeSpan() => System.TimeSpan.FromMinutes(Int());

        public static DateTime Instant() => new DateTime(2012, 10, 28) + TimeSpan();

        public static DateTimeOffset OffsetInstant() => new(Instant());

        public static LogEvent InformationEvent(DateTimeOffset? timestamp = null)
        {
            return LogEvent(timestamp, LogEventLevel.Information);
        }

        public static LogEvent LogEvent(DateTimeOffset? timestamp = null, LogEventLevel level = LogEventLevel.Information)
        {
            return new(timestamp ?? OffsetInstant(), level,
                null, MessageTemplate(), Enumerable.Empty<LogEventProperty>());
        }

        public static MessageTemplate MessageTemplate()
        {
            return new MessageTemplateParser().Parse(String());
        }
    }
}
