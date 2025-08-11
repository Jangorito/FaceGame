using UnityEngine;

public class DebugToggle : MonoBehaviour
{
    public GameObject debugPanel;

    // toggle the active state of the debug panel.
    public void ToggleDebugPanel()
    {
        if (debugPanel != null)
        {
            // SetActive toggles the visibility of the GameObject and all its children.
            Debug.Log("Toggling Debug Panel visibility.");
            bool isActive = debugPanel.activeSelf;
            Debug.Log($"Debug Panel is currently {(isActive ? "active" : "inactive")}. Toggling to {(isActive ? "inactive" : "active")}.");
            debugPanel.SetActive(!isActive);
        }
    }
}
