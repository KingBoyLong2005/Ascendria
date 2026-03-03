using UnityEngine;

public class EventCaptureZone : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float captureTime = 30f;
    [SerializeField] private Collider triggerZone;
    [SerializeField] private GameObject visualZone;
    [SerializeField] private LayerMask playerLayer;

    private float timer = 0f;
    private bool playerInside = false;
    private bool isActivated = false;
    private bool completed = false;
    private int lastShownSecond = -1;

    private Interactable interactable;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();

        if (visualZone != null)
            visualZone.SetActive(false);
    }

    private void Update()
    {
        if (!isActivated || completed || !playerInside) return;

        timer += Time.deltaTime;

        float remaining = captureTime - timer;
        int remainingInt = Mathf.CeilToInt(remaining);

        if (remainingInt != lastShownSecond && remainingInt >= 0)
        {
            lastShownSecond = remainingInt;
            InteractionUI.Instance.ShowMessage($"Capturing... {remainingInt}s");
        }

        if (timer >= captureTime)
        {
            CompleteEvent();
        }
    }

    // ================= ACTIVATE =================

    public void ActivateEvent()
    {
        if (completed || isActivated) return;

        Debug.Log("Capture Event Activated");

        isActivated = true;
        timer = 0f;
        lastShownSecond = -1;

        if (visualZone != null)
            visualZone.SetActive(true);

        CheckPlayerAlreadyInside();

        InteractionUI.Instance.ShowMessage("Stay in the area for 30 seconds!");
    }

    // ================= RELAY CALLS =================

    public void OnPlayerEnter()
    {
        if (!isActivated || completed) return;

        playerInside = true;
        Debug.Log("Player entered capture zone");
    }

    public void OnPlayerExit()
    {
        if (!isActivated || completed) return;

        Debug.Log("Player left capture zone → reset");
        ResetEvent();
    }

    public void OnPlayerStay()
    {
        if (!isActivated || completed) return;

        playerInside = true;
    }

    // ================= INTERNAL =================

    private void ResetEvent()
    {
        isActivated = false;
        timer = 0f;
        playerInside = false;
        lastShownSecond = -1;

        if (visualZone != null)
            visualZone.SetActive(false);
    }

    private void CompleteEvent()
    {
        completed = true;
        isActivated = false;

        if (visualZone != null)
            visualZone.SetActive(false);

        InteractionUI.Instance.ShowMessage("Capture complete!");
        interactable.Interacted();
        InteractionUI.Instance.Hide();
        StatRewardPopup.Instance.ShowRewardPopup();
    }

    private void CheckPlayerAlreadyInside()
    {
        if (triggerZone == null) return;

        var bounds = triggerZone.bounds;
        Collider[] hits = Physics.OverlapBox(
            bounds.center,
            bounds.extents,
            triggerZone.transform.rotation,
            playerLayer
        );

        foreach (var hit in hits)
        {
            if (((1 << hit.gameObject.layer) & playerLayer) != 0)
            {
                playerInside = true;
                Debug.Log("Player already inside at activation");
                return;
            }
        }
    }
}