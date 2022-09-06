using System.Diagnostics;
using System.Text;

namespace RemoteHealthcare.Logger;

public static class Log
{
    private static readonly string Gray = "\u001B[90m";
    private static readonly string Red = "\u001B[31m";
    private static readonly string Green = "\u001B[32m";
    private static readonly string Yellow = "\u001B[33m";
    private static readonly string Blue = "\u001B[34m";
    private static readonly string Magenta = "\u001B[35m";
    private static readonly string Cyan = "\u001B[36m";
    private static readonly string White = "\u001B[37m";
    
    private static void LogMessage(LogLevel level, Exception? exception, string message)
    {
        var builder = new StringBuilder();
        var stack = new StackTrace();

        const int startingStackIndex = 1;
        
        var stacks = stack.GetFrames().Skip(startingStackIndex).ToList();
        var callingMethod = stack.GetFrame(startingStackIndex);
        var callingClass = callingMethod?.GetMethod()?.DeclaringType;
        
        // [FATAL]
        builder.Append(Gray);
        builder.Append('[');
        builder.Append(GetColorCode(level));
        builder.Append(level.ToString().PadRight(5));
        builder.Append(Gray);
        builder.Append("] ");
        
        // [Class.Method:line]
        builder.Append(Gray);
        builder.Append('[');
        builder.Append(BuildStackTraceElement(stacks[startingStackIndex]));
        builder.Append(Gray);
        builder.Append("] ");
        
        // Message
        builder.Append(GetColorCode(level));
        builder.Append(message);

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
}

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error,
    Critical
}