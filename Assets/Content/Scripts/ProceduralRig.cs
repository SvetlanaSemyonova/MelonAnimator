using UnityEngine;

public class ProceduralRig : MonoBehaviour
{
    private Rigidbody2D[] _bodies;

    private void Awake()
    {
        _bodies = GetComponentsInChildren<Rigidbody2D>();
        SetKinematic(true);
    }

    public void SetKinematic(bool kinematic)
    {
        foreach (var body in _bodies)
        {
            body.bodyType = kinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        }
    }
}
