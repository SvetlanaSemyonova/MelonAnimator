using UnityEngine;

/// <summary>
/// One-stop Inspector panel for driving the character. It surfaces every
/// Animator parameter of the Human controller plus quick access to the rig and
/// the walk locomotion, so the whole character can be controlled from a single
/// component without writing code.
///
/// While playing, the float and bool values below are pushed to the Animator
/// every frame, so tweaking them in the Inspector updates the character live.
/// The trigger checkboxes fire once and reset themselves; the same triggers are
/// also available from the component's context menu (gear icon ▸).
/// </summary>
public class CharacterControlPanel : MonoBehaviour
{
    // Parameter names must match the "Human" AnimatorController.
    private const string P_Velocity        = "Velocity";
    private const string P_Angle           = "Angle";
    private const string P_AnimationSpeed  = "AnimationSpeed";
    private const string P_Walk            = "Walk";
    private const string P_WalkBackward    = "WalkBackward";
    private const string P_IsRotationFreeze= "IsRotationFreeze";
    private const string P_IsDrowning      = "IsDrowning";
    private const string P_IsOnFire        = "IsOnFire";
    private const string P_IsInGas         = "IsInGas";
    private const string P_WalkPermanently = "WalkPermanently";
    private const string P_StopAnimation   = "StopAnimation";
    private const string P_Sit             = "Sit";
    private const string P_Relax           = "Relax";

    [Header("References (auto-found if empty)")]
    [Tooltip("Animator that runs the Human controller.")]
    public Animator animator;

    [Tooltip("Rig that switches between Animated and Ragdoll.")]
    public ProceduralRig rig;

    [Tooltip("Component that moves the character across the floor while walking.")]
    public WalkLocomotion locomotion;

    [Header("Live apply")]
    [Tooltip("Push the float/bool values below to the Animator every frame while playing.")]
    public bool driveEveryFrame = true;

    [Header("Float parameters")]
    [Range(0f, 3f)]
    [Tooltip("AnimationSpeed — playback speed multiplier of the Walk states.")]
    public float animationSpeed = 1f;

    [Tooltip("Velocity — used by the controller's speed-based transitions.")]
    public float velocity = 0f;

    [Range(-1f, 1f)]
    [Tooltip("Angle — used by the tilt/slope transitions.")]
    public float angle = 0f;

    [Header("Bool parameters")]
    public bool walk = false;
    public bool walkBackward = false;
    public bool isRotationFreeze = false;
    public bool isDrowning = false;
    public bool isOnFire = false;
    public bool isInGas = false;

    [Header("Triggers (tick to fire — auto-resets)")]
    [Tooltip("Start the permanent walk loop.")]
    public bool triggerWalkPermanently = false;
    [Tooltip("Stop and return to Stand.")]
    public bool triggerStopAnimation = false;
    public bool triggerSit = false;
    public bool triggerRelax = false;

    [Header("Locomotion (mirrors WalkLocomotion)")]
    [Tooltip("World units travelled per second while walking.")]
    public float walkSpeed = 2f;
    [Tooltip("+1 = face/move right, -1 = left.")]
    public float facing = 1f;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rig == null) rig = GetComponent<ProceduralRig>();
        if (locomotion == null) locomotion = GetComponent<WalkLocomotion>();
    }

    private void Update()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        if (driveEveryFrame)
        {
            animator.SetFloat(P_AnimationSpeed, animationSpeed);
            animator.SetFloat(P_Velocity, velocity);
            animator.SetFloat(P_Angle, angle);

            animator.SetBool(P_Walk, walk);
            animator.SetBool(P_WalkBackward, walkBackward);
            animator.SetBool(P_IsRotationFreeze, isRotationFreeze);
            animator.SetBool(P_IsDrowning, isDrowning);
            animator.SetBool(P_IsOnFire, isOnFire);
            animator.SetBool(P_IsInGas, isInGas);
        }

        // Fire-and-reset trigger checkboxes.
        if (triggerWalkPermanently) { triggerWalkPermanently = false; WalkPermanently(); }
        if (triggerStopAnimation)   { triggerStopAnimation   = false; StopAnimation(); }
        if (triggerSit)             { triggerSit             = false; Sit(); }
        if (triggerRelax)           { triggerRelax           = false; Relax(); }

        // Keep the locomotion in sync with the mirrored fields.
        if (locomotion != null)
        {
            locomotion.walkSpeed = walkSpeed;
            locomotion.facing = facing;
        }
    }

    // ---- Triggers (also usable from the context menu) --------------------------

    [ContextMenu("Trigger/Walk Permanently")]
    public void WalkPermanently() => Fire(P_WalkPermanently);

    [ContextMenu("Trigger/Stop Animation")]
    public void StopAnimation() => Fire(P_StopAnimation);

    [ContextMenu("Trigger/Sit")]
    public void Sit() => Fire(P_Sit);

    [ContextMenu("Trigger/Relax")]
    public void Relax() => Fire(P_Relax);

    // ---- Rig shortcuts ---------------------------------------------------------

    [ContextMenu("Rig/Animated")]
    public void SetAnimated() { if (rig != null) rig.SetKinematic(true); }

    [ContextMenu("Rig/Ragdoll")]
    public void SetRagdoll() { if (rig != null) rig.SetKinematic(false); }

    private void Fire(string trigger)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger(trigger);
    }
}
