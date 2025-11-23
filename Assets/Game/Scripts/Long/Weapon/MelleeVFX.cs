using UnityEngine;

/// <summary>
/// Simple VFX helper:
/// - root rotates on Y to face aimDirection (so slash orients to direction)
/// - visual child keeps facing camera (billboard) so sprite is readable
/// - optional scale/flip adjustments
/// </summary>
public class MeleeVFX : MonoBehaviour
{
    public Transform visual;      // assign Visual child (SpriteRenderer)
    public float lifetime = 0.6f; // destroy after this time
    public float localRotationOffsetY = 0f; // tweak if sprite's forward axis differs

    void Start()
    {
        // safety: destroy after lifetime
        Destroy(gameObject, lifetime);
        if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
    }

    /// <summary>
    /// Initialize orientation. aimDirection in world-space (normalized).
    /// </summary>
    public void Init(Vector3 aimDirection)
    {
        // flatten on XZ plane (no vertical tilt)
        Vector3 dirFlat = new Vector3(aimDirection.x, 0f, aimDirection.z).normalized;
        if (dirFlat.sqrMagnitude < 0.001f) dirFlat = transform.forward;

        // rotate the root so that its forward points to dirFlat (rotate around Y)
        float angleY = Mathf.Atan2(dirFlat.x, dirFlat.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, angleY + localRotationOffsetY, 0f);

        // Make visual face camera (billboard)
        if (Camera.main != null && visual != null)
        {
            // set visual rotation to camera rotation so it faces camera,
            // but keep its own local Y rotation zero so global yaw from root is used.
            visual.rotation = Camera.main.transform.rotation;
        }
    }

    void Update()
    {
        // Keep visual facing camera each frame (in case camera moves)
        if (visual != null && Camera.main != null)
            visual.rotation = Camera.main.transform.rotation;
    }
}
