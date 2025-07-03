using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger
{
    private bool shouldLog = false;
    public Logger(bool val) => shouldLog = val;

    public void LogMsg(string message) {
        if (shouldLog)
            Debug.Log(message);
    }

    public void LogWar(string message) {
        if (shouldLog)
            Debug.LogWarning(message);
    }

    public void LogError(string message) {
        if (shouldLog)
            Debug.LogError(message);
    }
  
}
