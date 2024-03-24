using UnityEngine;
using UnityEditor;
using System.Diagnostics;

[InitializeOnLoad]
public class RunPythonOnPlay
{
    private static Process pythonProcess;
    private static bool debugMode = false; // Set to true for debug messages, false to disable

    static RunPythonOnPlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void DebugLog(string message)
    {
        if (debugMode)
        {
            UnityEngine.Debug.Log(message);
        }
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
            startInfo.CreateNoWindow = true; // Prevents the command-line window from appearing

            pythonProcess = new Process();
            pythonProcess.StartInfo = startInfo;
            pythonProcess.OutputDataReceived += (sender, e) => { DebugLog(e.Data); };
            pythonProcess.ErrorDataReceived += (sender, e) => { DebugLog(e.Data); };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            DebugLog("Python script started!");
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

                DebugLog("Python script termination signals sent.");

                // Wait for a brief moment to allow the process to respond to the termination signals
                System.Threading.Thread.Sleep(1000); // Adjust the sleep duration as needed

                // Check again if the process has exited
                if (!pythonProcess.HasExited)
                {
                    // If the process has not exited, forcefully kill it
                    pythonProcess.Kill();
                    DebugLog("Python script forcibly terminated.");
                }
                else
                {
                    DebugLog("Python script terminated.");
                }
            }
        }
    }
}