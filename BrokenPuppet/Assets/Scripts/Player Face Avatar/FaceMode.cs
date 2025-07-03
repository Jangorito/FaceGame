using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO;

public class RunFacePython : MonoBehaviour
{
    private Process pythonProcess;
    private static RunFacePython instance;

    public string sceneToLoad = "EmotionMatchGame"; // Set this in the Inspector

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Call this from a button or collision event
    public void SwitchToFacePy()
    {
        KillMainPyProcess();
        StartFacePy();
        SceneManager.LoadScene(sceneToLoad);
    }

    private void KillMainPyProcess()
    {
        try
        {
            var processes = Process.GetProcessesByName("python");
            foreach (var proc in processes)
            {
                try
                {
                    proc.Kill();
                    UnityEngine.Debug.Log("Killed python process.");
                }
                catch { /* Ignore processes we can't access */ }
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning("Could not check for existing python processes: " + ex.Message);
        }
    }

    private void StartFacePy()
    {
        string pythonScriptPath = Path.Combine(Application.dataPath, "../../mediapipe/face.py");
        pythonScriptPath = Path.GetFullPath(pythonScriptPath);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{pythonScriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        pythonProcess = new Process();
        pythonProcess.StartInfo = startInfo;
        pythonProcess.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.Log(e.Data); };
        pythonProcess.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.LogError(e.Data); };

        pythonProcess.Start();
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

        UnityEngine.Debug.Log("Started face.py process.");
    }

    private void OnApplicationQuit()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
        }
    }
}