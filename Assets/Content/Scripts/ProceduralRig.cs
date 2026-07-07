using UnityEngine;

public class ProceduralRig : MonoBehaviour
{
    private Rigidbody2D[] _bodies;
    private ProceduralBone[] _bones;

    private void Awake()
    {
        _bodies = GetComponentsInChildren<Rigidbody2D>();
        _bones = GetComponentsInChildren<ProceduralBone>();
        SetKinematic(true);
    }

    public void SetKinematic(bool kinematic)
    {
        foreach (var bone in _bones)
            bone.enabled = kinematic;

        foreach (var body in _bodies)
        {
            body.bodyType = kinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            body.simulated = !kinematic;
        }
    }
}
