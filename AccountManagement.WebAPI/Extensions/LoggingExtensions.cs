using System.Diagnostics;
using System.Text;

namespace AccountManagement.WebAPI.Extensions
{
    public static class LoggingExtensions
    {
        public static string BuildCleanStackTrace(Exception exception)
        {
            var stackTrace = new System.Diagnostics.StackTrace(exception, true);
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

                // Skip Microsoft/system noise
                var fullName = declaringType?.FullName;
                if (!string.IsNullOrEmpty(fullName) &&
                    (fullName.StartsWith("Microsoft") || fullName.StartsWith("System")))
                    continue;

                var filePath = frame.GetFileName();
                var line = frame.GetFileLineNumber();

                if (!string.IsNullOrEmpty(filePath))
                {
                    filePath = filePath.Replace(rootPath, "").TrimStart('\\');
                }

                sb.AppendLine(
                    $"at {fullName}.{method.Name} " +
                    $"({filePath}:{line})"
                );
            }

            return sb.ToString();
        }

    }
}
