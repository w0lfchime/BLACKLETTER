using UnityEngine;

public sealed class IDERoot : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool debugEnabled = false;

    [Header("Hotkey")]
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.BackQuote;

    [Header("Debug Root (drag in)")]
    [SerializeField] private GameObject debugRoot; // parent object of the whole debug UI


    public bool DebugEnabled => debugEnabled;

    private void Awake()
    {
        Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            debugEnabled = !debugEnabled;
            Apply();
        }
    }

    private void Apply()
    {
        if (debugRoot != null)
            debugRoot.SetActive(debugEnabled);
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Apply();
        }
    }
#endif

    // Optional: call from other scripts if you want
    public void SetDebugEnabled(bool enabled)
    {
        if (debugEnabled == enabled) return;
        debugEnabled = enabled;
        Apply();
    }

    public void ToggleDebug()
    {
        debugEnabled = !debugEnabled;
        Apply();
    }
}