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
            bool isActive = debugPanel.activeSelf;
            debugPanel.SetActive(!isActive);
        }
    }
}
