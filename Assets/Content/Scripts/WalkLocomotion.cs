using UnityEngine;

/// <summary>
/// Moves the character horizontally while a walk / step animation is playing,
/// so the Human actually travels across the floor instead of stepping in place.
///
/// The Walk / Step clips only animate the bones' <c>restRotation</c> (the legs
/// cycle) but never translate the root, and <see cref="ProceduralRig"/> switches
/// every Rigidbody2D to Kinematic while animating (physics is off). So the root
/// has to be moved in code — that is what this component does.
///
/// Attach it to the Human root (the object that owns the <see cref="Animator"/>).
/// </summary>
[RequireComponent(typeof(Animator))]
public class WalkLocomotion : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("World units travelled per second while walking.")]
    public float walkSpeed = 2f;

    [Tooltip("Facing / travel direction along X. +1 = right, -1 = left.")]
    public float facing = 1f;

    [Tooltip("If true, travel speed scales with the Animator's playback speed " +
             "(state speed * AnimationSpeed) so the feet don't slip when the clip " +
             "is sped up or slowed down.")]
    public bool matchAnimationSpeed = true;

    [Header("Animator states")]
    [Tooltip("States that move the character forward (in the 'facing' direction).")]
    public string[] forwardStates = { "Walk", "Step" };

    [Tooltip("States that move the character backward.")]
    public string[] backwardStates = { "Step Backward" };

    [Tooltip("Animator layer to read the current state from.")]
    public int layer = 0;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return;

        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(layer);

        float dir = DirectionFor(state);
        if (dir == 0f)
            return;

        float cadence = 1f;
        if (matchAnimationSpeed)
        {
            // effective playback speed of the current state (state speed * multiplier)
            cadence = Mathf.Abs(state.speed) * Mathf.Max(0.01f, state.speedMultiplier);
            if (cadence <= 0f) cadence = 1f;
        }

        float delta = walkSpeed * cadence * dir * facing * Time.deltaTime;
        transform.Translate(delta, 0f, 0f, Space.World);
    }

    private float DirectionFor(AnimatorStateInfo state)
    {
        foreach (var s in forwardStates)
            if (!string.IsNullOrEmpty(s) && state.IsName(s))
                return 1f;

        foreach (var s in backwardStates)
            if (!string.IsNullOrEmpty(s) && state.IsName(s))
                return -1f;

        return 0f;
    }
}
