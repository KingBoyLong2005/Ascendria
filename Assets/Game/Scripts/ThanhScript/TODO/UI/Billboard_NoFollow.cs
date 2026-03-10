using UnityEngine;

public class Billboard_NoFollow : MonoBehaviour
{
    private Transform camTransform;
    private Transform target;       // player transform
    //public float height = 2f;
    //public float forwardOffset = 0.5f;

    private void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        camTransform = FindFirstObjectByType<Camera>().transform;
    }

    void LateUpdate()
    {
        if (!target) return;

        //Vector3 worldPos = target.position + Vector3.up * height;
        //Vector3 dirToCam = (camTransform.position - worldPos).normalized;
        //worldPos += dirToCam * forwardOffset;

        //transform.position = worldPos;
        transform.LookAt(transform.position + camTransform.forward);
    }
}
