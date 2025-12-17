using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform camTransform;
    public Transform target;       // player transform
    public float height = 2f;
    public float forwardOffset = 0.5f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 worldPos = target.position + Vector3.up * height;
        Vector3 dirToCam = (camTransform.position - worldPos).normalized;
        worldPos += dirToCam * forwardOffset;

        transform.position = worldPos;
        transform.LookAt(transform.position + camTransform.forward);
    }
}

