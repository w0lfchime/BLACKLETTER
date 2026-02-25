using UnityEngine;

public sealed class IDERoot : MonoBehaviour
{
    [Header("Master")]
    [SerializeField] private bool debugEnabled = false;              // master gate
    [SerializeField] private bool debugIDEInputVisible = true;
    [SerializeField] private bool debugIDETokenViewVisible = true;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.BackQuote; // ` (often the tilde key)
    [SerializeField] private KeyCode toggleInputKey = KeyCode.F1;
    [SerializeField] private KeyCode toggleTokenViewKey = KeyCode.F2;

    [Header("UI References (drag in)")]
    [SerializeField] private CanvasGroup debugIDEInputGroup;
    [SerializeField] private CanvasGroup debugIDETokenViewGroup;

    [Header("Behavior")]
    [SerializeField] private bool deactivateWhenHidden = false;
    [SerializeField] private bool toggleChildrenWithMaster = false;

    // Optional: if you want other scripts to query these
    public bool DebugEnabled => debugEnabled;
    public bool DebugIDEInputVisible => debugIDEInputVisible;
    public bool DebugIDETokenViewVisible => debugIDETokenViewVisible;

    private void Awake()
    {
        // If you don’t drag refs, try to auto-find by name under this root.
        if (debugIDEInputGroup == null)
            debugIDEInputGroup = FindCanvasGroupInChildren("Debug_IDE_Input");

        if (debugIDETokenViewGroup == null)
            debugIDETokenViewGroup = FindCanvasGroupInChildren("Debug_IDE_TokenView");

        ApplyAll();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            debugEnabled = !debugEnabled;

            // Optional: when toggling master on, restore children states.
            // When toggling master off, hides everything regardless.
            ApplyAll();

            // Optional behavior: also flip both children on each master toggle (some people like this).
            if (toggleChildrenWithMaster && debugEnabled)
            {
                // Example behavior: if both are currently off, turn them on; otherwise turn both off.
                bool anyVisible = debugIDEInputVisible || debugIDETokenViewVisible;
                debugIDEInputVisible = !anyVisible;
                debugIDETokenViewVisible = !anyVisible;
                ApplyAll();
            }

            return;
        }

        // If master is off, ignore sub-toggles
        if (!debugEnabled)
            return;

        if (Input.GetKeyDown(toggleInputKey))
        {
            debugIDEInputVisible = !debugIDEInputVisible;
            ApplyAll();
        }

        if (Input.GetKeyDown(toggleTokenViewKey))
        {
            debugIDETokenViewVisible = !debugIDETokenViewVisible;
            ApplyAll();
        }
    }

    /// <summary>
    /// Call this whenever any flags change.
    /// </summary>
    private void ApplyAll()
    {
        bool inputVisible = debugEnabled && debugIDEInputVisible;
        bool tokenVisible = debugEnabled && debugIDETokenViewVisible;

        SetCanvasGroupVisible(debugIDEInputGroup, inputVisible, deactivateWhenHidden);
        SetCanvasGroupVisible(debugIDETokenViewGroup, tokenVisible, deactivateWhenHidden);
    }

    /// <summary>
    /// Properly shows/hides a CanvasGroup: visibility + input blocking.
    /// Optionally deactivates GameObject for performance / to stop layout.
    /// </summary>
    public static void SetCanvasGroupVisible(CanvasGroup group, bool visible, bool deactivateGO)
    {
        if (group == null) return;

        if (deactivateGO)
        {
            // Avoid toggling active state every frame; only when it changes.
            if (group.gameObject.activeSelf != visible)
                group.gameObject.SetActive(visible);

            if (!visible) return; // if inactive, no need to set properties
        }

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
        // group.ignoreParentGroups = false; // default, leave it unless you need special behavior
    }

    private CanvasGroup FindCanvasGroupInChildren(string childName)
    {
        // Looks for an exact name match first, then partial match.
        var all = GetComponentsInChildren<CanvasGroup>(true);

        for (int i = 0; i < all.Length; i++)
            if (all[i].name == childName)
                return all[i];

        for (int i = 0; i < all.Length; i++)
            if (all[i].name.Contains(childName))
                return all[i];

        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Keep the scene view consistent when toggling in inspector.
        if (!Application.isPlaying)
            ApplyAll();
    }
#endif
}