using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RemoteHealthcare.Logger;

public static class Log
{
    private const string Gray = "\u001B[90m";
    private const string Red = "\u001B[31m";
    private const string Green = "\u001B[32m";
    private const string Yellow = "\u001B[33m";
    private const string Blue = "\u001B[34m";
    private const string Magenta = "\u001B[35m";
    private const string Cyan = "\u001B[36m";
    public const string White = "\u001B[37m";

    private static bool hasEnabledColorSupport = false;
    
    private static void LogMessage(LogLevel level, Exception? exception, string message)
    {
        if (!hasEnabledColorSupport)
        {
            // Get the STD handle
            IntPtr iStdOut = GetStdHandle(StdOutputHandle);

            // Try to enable the use of ANSI codes
            bool colorSupported = GetConsoleMode(iStdOut, out uint outConsoleMode) &&
                                  SetConsoleMode(iStdOut, outConsoleMode | EnableVirtualTerminalProcessing);
        }
        
        var builder = new StringBuilder();
        var stack = new StackTrace();
        
        // [FATAL]
        builder.Append(Gray);
        builder.Append('[');
        builder.Append(GetColorCode(level));
        builder.Append(level.ToString().PadRight(5).ToUpper());
        builder.Append(Gray);
        builder.Append("] ");
        
        // [Class.Method:line]
        builder.Append(Gray);
        builder.Append('[');
        builder.Append(BuildStackTraceElement(stack.GetFrame(5)));
        builder.Append(Gray);
        builder.Append("] ");
        
        // Message
        builder.Append(GetColorCode(level));
        builder.Append(message);

        if (exception != null)
        {
            var ex = exception;

            while (ex != null)
            {
                builder.AppendLine();
                builder.Append("         ");
                builder.Append(Red);
                builder.Append(exception.GetType().Name.Replace("Exception", string.Empty));
                builder.Append(Gray);
                builder.Append(": ");
                builder.Append(GetColorCode(level));
                builder.Append(exception.Message);
                
                ex = ex.InnerException;
            }
            
            if(exception.StackTrace != null)
            {
                var stackTraceElements = exception.StackTrace.Split('\n').Select(x => x.Trim()).ToList();

                foreach (var stackTraceElement in stackTraceElements)
                {
                    builder.AppendLine();
                    builder.Append(Gray);
                    builder.Append("          ");
                    builder.Append(stackTraceElement);
                }
            }
        }

        Console.WriteLine(builder.ToString());
    }

    public static void Debug(Exception exception, string message) => LogMessage(LogLevel.Debug, exception, message);
    public static void Debug(string message) => LogMessage(LogLevel.Debug, null, message);
    
    public static void Information(Exception exception, string message) => LogMessage(LogLevel.Information, exception, message);
    public static void Information(string message) => LogMessage(LogLevel.Information, null, message);
    
    public static void Warning(Exception exception, string message) => LogMessage(LogLevel.Warning, exception, message);
    public static void Warning(string message) => LogMessage(LogLevel.Warning, null, message);
    
    public static void Error(Exception exception, string message) => LogMessage(LogLevel.Error, exception, message);
    public static void Error(string message) => LogMessage(LogLevel.Error, null, message);
    
    public static void Critical(Exception exception, string message) => LogMessage(LogLevel.Critical, exception, message);
    public static void Critical(string message) => LogMessage(LogLevel.Critical, null, message);
    
    private static string GetColorCode(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => Gray,
            LogLevel.Information => Blue,
            LogLevel.Warning => Yellow,
            LogLevel.Error => Red,
            LogLevel.Critical => Magenta,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
    
    private static string BuildStackTraceElement(StackFrame stack) {
        var builder = new StringBuilder();

        builder.Append(Cyan);
        builder.Append(stack.GetMethod()?.DeclaringType?.Name);
        builder.Append(Gray);
        builder.Append('.');
        builder.Append(Green);
        builder.Append(stack.GetMethod()?.Name);
        builder.Append(Gray);
        builder.Append("()");

        return builder.ToString();
    }
    
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
}

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error,
    Critical
}