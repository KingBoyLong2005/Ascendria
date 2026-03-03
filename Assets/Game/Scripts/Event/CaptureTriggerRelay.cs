using UnityEngine;

public class CaptureTriggerRelay : MonoBehaviour
{
    private EventCaptureZone parentZone;
    [SerializeField] private LayerMask playerLayer;

    private void Awake()
    {
        parentZone = GetComponentInParent<EventCaptureZone>();
    }

    private bool IsPlayerLayer(GameObject obj)
    {
        return (playerLayer.value & (1 << obj.layer)) != 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject)) return;
        parentZone.OnPlayerEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject)) return;
        parentZone.OnPlayerExit();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject)) return;
        parentZone.OnPlayerStay();
    }
}