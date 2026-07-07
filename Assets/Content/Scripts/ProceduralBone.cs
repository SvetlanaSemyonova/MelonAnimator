using UnityEngine;

public class ProceduralBone : MonoBehaviour
{
    public float restRotation;

    private Rigidbody2D _body;
    private Vector3 _restLocalPosition;
    private Vector3 _pivot; 

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _restLocalPosition = transform.localPosition;

        var hinge = GetComponent<HingeJoint2D>();
        var distance = GetComponent<DistanceJoint2D>();
        if (hinge != null) _pivot = hinge.anchor;
        else if (distance != null) _pivot = distance.anchor;
        else _pivot = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (_body != null && _body.simulated)
            _body.simulated = false;

        Quaternion rot = Quaternion.Euler(0f, 0f, restRotation);

        transform.localRotation = rot;
        transform.localPosition = _restLocalPosition + (_pivot - rot * _pivot);
    }
}
