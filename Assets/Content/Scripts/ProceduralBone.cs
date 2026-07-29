using UnityEngine;

/// <summary>
/// A single animation-driven bone. Each frame it rotates the transform to
/// <see cref="restRotation"/> (this is the value the animation clips drive) around
/// its pivot, keeping the joint anchor fixed. Everything tweakable is now exposed
/// in the Inspector.
/// </summary>
public class ProceduralBone : MonoBehaviour
{
    [Header("Pose")]
    [Tooltip("Local Z rotation (degrees) of this bone. This is the value the " +
             "animation clips key to make the character move.")]
    public float restRotation;

    [Tooltip("When frozen, the bone stops driving its transform (lets physics or " +
             "manual editing take over).")]
    public bool freeze = false;

    [Header("Pivot")]
    [Tooltip("Override the pivot instead of reading it from the Hinge/Distance joint anchor.")]
    public bool usePivotOverride = false;

    [Tooltip("Pivot (local space) used when Use Pivot Override is on.")]
    public Vector3 pivotOverride = Vector3.zero;

    [Header("Rest position")]
    [Tooltip("Override the rest local position instead of capturing it on Awake.")]
    public bool useRestPositionOverride = false;

    [Tooltip("Rest local position used when Use Rest Position Override is on.")]
    public Vector3 restPositionOverride = Vector3.zero;

    private Rigidbody2D _body;
    private Vector3 _restLocalPosition;
    private Vector3 _pivot;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();

        _restLocalPosition = useRestPositionOverride ? restPositionOverride : transform.localPosition;

        if (usePivotOverride)
        {
            _pivot = pivotOverride;
        }
        else
        {
            var hinge = GetComponent<HingeJoint2D>();
            var distance = GetComponent<DistanceJoint2D>();
            if (hinge != null) _pivot = hinge.anchor;
            else if (distance != null) _pivot = distance.anchor;
            else _pivot = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (freeze)
            return;

        if (_body != null && _body.simulated)
            _body.simulated = false;

        Quaternion rot = Quaternion.Euler(0f, 0f, restRotation);

        transform.localRotation = rot;
        transform.localPosition = _restLocalPosition + (_pivot - rot * _pivot);
    }
}
