using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -10);
    public Vector3 rotation = new Vector3(30, 0, 0);

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z) * target.rotation;
        }
    }
}
