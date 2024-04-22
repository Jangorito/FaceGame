using UnityEngine;
using UnityEditor;
using System.Diagnostics;

[InitializeOnLoad]
public class RunPythonOnPlay
{
    private static Process pythonProcess;

    static RunPythonOnPlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Path to your Python script
            string pythonScriptPath = @"..\mediapipe\main.py";

            // Start Python process
            var startInfo = new ProcessStartInfo
            {
                FileName = "python", // Use the system's default Python interpreter
                Arguments = pythonScriptPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            pythonProcess = new Process
            {
                StartInfo = startInfo,
            };
            pythonProcess.OutputDataReceived += (sender, e) => { UnityEngine.Debug.Log(e.Data); };
            pythonProcess.ErrorDataReceived += (sender, e) => { UnityEngine.Debug.LogError(e.Data); };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            UnityEngine.Debug.Log("Python script started!");
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Check if the process is running before attempting to kill it
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                // Kill the Python process
                pythonProcess.Kill();
                UnityEngine.Debug.Log("Python script terminated.");
            }
        }
    }
}