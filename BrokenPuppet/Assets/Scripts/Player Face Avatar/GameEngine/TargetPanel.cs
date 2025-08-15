using UnityEngine;

public class DebugToggle : MonoBehaviour
{
    public GameObject target;

    // toggle the active state of the debug panel.
    public void ToggleDebugPanel()
    {

        if (target != null)
        {
            // SetActive toggles the visibility of the GameObject and all its children.
            Debug.Log("Toggling Debug Panel visibility.");
            bool isActive = target.activeSelf;
            Debug.Log($"Debug Panel is currently {(isActive ? "active" : "inactive")}. Toggling to {(isActive ? "inactive" : "active")}.");
            target.SetActive(!isActive);
        }
    }
}
