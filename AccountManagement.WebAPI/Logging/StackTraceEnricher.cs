using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace AccountManagement.WebAPI.Logging
{
    public class StackTraceEnricher : ILogEventEnricher
    {

        public StackTraceEnricher() { } // parameterless constructor

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception == null) return; 
            
            var ex = logEvent.Exception; 
            var trace = new StackTrace(ex, true); 
            var frame = trace.GetFrames()?.FirstOrDefault(f => f.GetFileLineNumber() > 0); 
            var methodName = ex.TargetSite?.Name ?? "UnknownMethod"; 
            var lineNumber = frame?.GetFileLineNumber() ?? -1; 
            var fileName = frame?.GetFileName() ?? "UnknownFile"; 
            
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MethodName", methodName)); 
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LineNumber", lineNumber)); 
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("FileName", fileName));

        }
    }
}
