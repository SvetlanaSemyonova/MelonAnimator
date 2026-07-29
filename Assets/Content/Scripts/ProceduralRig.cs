using UnityEngine;

/// <summary>
/// Drives the character's rig in one of two modes:
///   • <b>Animated</b> — bones are enabled and every Rigidbody2D is Kinematic, so
///     the Animator fully controls the pose (this is how the Human walks).
///   • <b>Ragdoll</b> — bones are disabled and the bodies turn Dynamic, so physics
///     takes over and the character collapses.
///
/// Everything that used to be hard-coded is now exposed in the Inspector.
/// </summary>
public class ProceduralRig : MonoBehaviour
{
    public enum RigMode { Animated, Ragdoll }

    [Header("Mode")]
    [Tooltip("Animated = Animator drives the pose (bodies Kinematic). " +
             "Ragdoll = physics drives the bodies (bodies Dynamic).")]
    public RigMode mode = RigMode.Animated;

    [Header("Ragdoll physics")]
    [Tooltip("Gravity scale applied to every body when switching to Ragdoll mode.")]
    public float ragdollGravityScale = 1f;

    [Tooltip("If true, ragdoll bodies keep their velocity; if false velocity is zeroed on switch.")]
    public bool preserveVelocityOnRagdoll = false;

    [Header("Members")]
    [Tooltip("Find the Rigidbody2D and ProceduralBone members automatically in children on Awake.")]
    public bool autoCollect = true;

    [Tooltip("Rigidbody2D members. Filled automatically when Auto Collect is on.")]
    public Rigidbody2D[] bodies;

    [Tooltip("ProceduralBone members. Filled automatically when Auto Collect is on.")]
    public ProceduralBone[] bones;

    private void Awake()
    {
        if (autoCollect || bodies == null || bodies.Length == 0)
            Collect();

        Apply();
    }

    /// <summary>(Re)collect the rig members from the children.</summary>
    [ContextMenu("Collect Members")]
    public void Collect()
    {
        bodies = GetComponentsInChildren<Rigidbody2D>(true);
        bones = GetComponentsInChildren<ProceduralBone>(true);
    }

    /// <summary>Apply the current <see cref="mode"/> to the rig.</summary>
    [ContextMenu("Apply Mode")]
    public void Apply()
    {
        SetKinematic(mode == RigMode.Animated);
    }

    /// <summary>Kept for backwards compatibility: true = Animated, false = Ragdoll.</summary>
    public void SetKinematic(bool kinematic)
    {
        mode = kinematic ? RigMode.Animated : RigMode.Ragdoll;

        if (bones != null)
            foreach (var bone in bones)
                if (bone != null) bone.enabled = kinematic;

        if (bodies != null)
            foreach (var body in bodies)
            {
                if (body == null) continue;
                body.bodyType = kinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
                body.simulated = !kinematic;
                body.gravityScale = ragdollGravityScale;
                if (!kinematic && !preserveVelocityOnRagdoll)
                {
                    body.linearVelocity = Vector2.zero;
                    body.angularVelocity = 0f;
                }
            }
    }

    /// <summary>Convenience toggle usable from UI events or other scripts.</summary>
    public void SetRagdoll(bool ragdoll) => SetKinematic(!ragdoll);

#if UNITY_EDITOR
    // Let the Inspector drive the rig live while playing.
    private void OnValidate()
    {
        if (Application.isPlaying)
            Apply();
    }
#endif
}
