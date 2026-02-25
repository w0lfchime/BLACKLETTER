using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;


public enum LogType
{
    NoCategory,
    General,
    Fatal,
    Error,
    Warning,
    Grid,
    Drone,
    Item
}


public static class LogCore
{
    private static int messageCount = 0;
    public static bool loggingEnabled = true;

    public static event Action<string> OnLog;

    private static Dictionary<LogType, bool> logTypeStates = new();

    static LogCore()
    {
        foreach (LogType type in Enum.GetValues(typeof(LogType)))
        {
            logTypeStates[type] = true; // Enable all by default
        }

        //disable logtypes here 
        //TODO: Active logtype filtering by command
    }

    // === Public API ===

    public static void Log(LogType type, string message,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "")
    {
        if (!loggingEnabled || !IsLogTypeEnabled(type)) return;

        messageCount++;
        string typeName = type.ToString();
        string formattedMessage = $"({messageCount}) [{typeName}] {message} (at {System.IO.Path.GetFileName(file)}:{line} in {member})";

        // Convert enum to string and decide logging severity

        Debug.Log(formattedMessage);


        OnLog?.Invoke(formattedMessage);
    }


    public static void Log(string message,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "")
    {
        if (!loggingEnabled) return;

        messageCount++;

        string formattedMessage = $"({messageCount}) [{"NoCategory"}] {message} (at {System.IO.Path.GetFileName(file)}:{line} in {member})";

        // Convert enum to string and decide logging severity

        Debug.Log(formattedMessage);

        OnLog?.Invoke(formattedMessage);
    }

    public static void SetLogTypeEnabled(LogType type, bool enabled)
    {
        logTypeStates[type] = enabled;
    }

    public static bool IsLogTypeEnabled(LogType type)
    {
        return logTypeStates.TryGetValue(type, out var enabled) && enabled;
    }

    public static void EnableAllLogTypes()
    {
        foreach (LogType type in Enum.GetValues(typeof(LogType)))
            logTypeStates[type] = true;
    }

    public static void DisableAllLogTypes()
    {
        foreach (LogType type in Enum.GetValues(typeof(LogType)))
            logTypeStates[type] = false;
    }

    public static void PrintLogTypeStatus()
    {
        foreach (var kvp in logTypeStates)
        {
            Debug.Log($"[LogCore] {kvp.Key}: {(kvp.Value ? "ENABLED" : "DISABLED")}");
        }
    }
}
