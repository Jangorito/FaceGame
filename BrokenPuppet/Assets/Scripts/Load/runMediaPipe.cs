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
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "python"; // Use the system's default Python interpreter
            startInfo.Arguments = pythonScriptPath;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;  // Redirect standard input to allow sending signals
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            pythonProcess = new Process();
            pythonProcess.StartInfo = startInfo;
            pythonProcess.OutputDataReceived += (sender, e) => { UnityEngine.Debug.Log(e.Data); };
            pythonProcess.ErrorDataReceived += (sender, e) => { UnityEngine.Debug.LogError(e.Data); };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            UnityEngine.Debug.Log("Python script started!");
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Check if the process is running before attempting to send signals
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                // Send Escape key
                pythonProcess.StandardInput.Write((char)27);
                // Send Enter key
                pythonProcess.StandardInput.Write("\n");

                UnityEngine.Debug.Log("Python script terminated.");
            }
        }
    }
}
