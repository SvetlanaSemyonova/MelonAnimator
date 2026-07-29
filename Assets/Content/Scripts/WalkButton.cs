using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Human's Animator from a UI Button. First click starts the walk
/// loop (fires the <see cref="startTrigger"/> — "WalkPermanently"), which sends
/// the Animator into its looping Walk state; <see cref="WalkLocomotion"/> then
/// moves the character across the floor. The next click stops it
/// (<see cref="stopTrigger"/> — "StopAnimation") and returns to Stand.
///
/// Put this on the Button GameObject. No manual wiring is required: the click
/// handler is registered in code, and the Animator / label are found
/// automatically if not assigned in the Inspector.
/// </summary>
[RequireComponent(typeof(Button))]
public class WalkButton : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Animator to drive. If left empty, the first Animator in the scene is used.")]
    public Animator animator;

    [Header("Animator triggers")]
    [Tooltip("Trigger fired to start walking.")]
    public string startTrigger = "WalkPermanently";

    [Tooltip("Trigger fired to stop and return to Stand.")]
    public string stopTrigger = "StopAnimation";

    [Header("UI")]
    [Tooltip("Optional label whose text toggles between 'Walk' and 'Stop'.")]
    public Text label;

    private Button _button;
    private bool _walking;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (animator == null)
#if UNITY_2023_1_OR_NEWER
            animator = FindFirstObjectByType<Animator>();
#else
            animator = FindObjectOfType<Animator>();
#endif
        if (label == null)
            label = GetComponentInChildren<Text>();

        UpdateLabel();
    }

    private void OnEnable()
    {
        if (_button != null) _button.onClick.AddListener(Toggle);
    }

    private void OnDisable()
    {
        if (_button != null) _button.onClick.RemoveListener(Toggle);
    }

    /// <summary>Start walking on the first click, stop on the next.</summary>
    public void Toggle()
    {
        if (animator == null)
        {
            Debug.LogWarning("[WalkButton] No Animator found to drive.", this);
            return;
        }

        _walking = !_walking;
        animator.SetTrigger(_walking ? startTrigger : stopTrigger);
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (label != null)
            label.text = _walking ? "Stop" : "Walk";
    }
}
