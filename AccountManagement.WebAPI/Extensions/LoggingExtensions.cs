using System.Diagnostics;
using System.Text;

namespace AccountManagement.WebAPI.Extensions
{
    public static class LoggingExtensions
    {
        public static string BuildCleanStackTrace(Exception ex)
        {
            var stackTrace = new StackTrace(ex, true);
            var frames = stackTrace.GetFrames();

            if (frames == null) return new StackFrame().ToString();

            var sb = new StringBuilder();
            var rootPath = Directory.GetCurrentDirectory();

            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                if (method == null) continue;

                var declaringType = method.DeclaringType;
                if (declaringType == null) continue;

                var fullName = declaringType?.FullName;
                if (string.IsNullOrWhiteSpace(fullName)) continue;

                // Skip framework noise
                if (fullName.StartsWith("Microsoft") || fullName.StartsWith("System"))
                    continue;

                var file = frame.GetFileName();
                var line = frame.GetFileLineNumber();

                if (!string.IsNullOrEmpty(file))
                    file = file.Replace(rootPath, "").TrimStart('\\');

                sb.AppendLine(
                    $" at {fullName}.{method.Name} ({file} & Line no:{line})"
                );
            }

            return sb.ToString();
        }

    }
}
