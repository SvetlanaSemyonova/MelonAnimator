using UnityEngine;

public class ProceduralBone : MonoBehaviour
{
    public float restRotation;

    private void LateUpdate()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, restRotation);
    }
}
