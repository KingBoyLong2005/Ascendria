using UnityEngine;

public class Flag : MonoBehaviour
{
    public float captureFlagProgress = 0f;
    public float captureFlagNeeded = 5f; // thời gian/điểm cần để bắt cờ
    public float decayRateFlag = 1f;     // tốc độ giảm khi không click
    public Vector3 halfExtents = new Vector3(7f, 7f, 7f); // kích thước vùng check
    public LayerMask playerLayer;        // layer của Player

    private bool beingCapturedFlag = false;



    void Start()
    {
        UIManager.Instance.ShowCaptureBar(captureFlagNeeded);
    }

    void Update()
    {
        // Tạo vùng hộp quanh flag và check xem player có trong đó không
        Collider[] hits = Physics.OverlapBox(
            transform.position,   // tâm hộp
            halfExtents,          // half extents
            Quaternion.identity,  // không xoay
            playerLayer           // chỉ check player
        );

        bool inRange = hits.Length > 0;

        // Chỉ khi trong phạm vi và nhấn R thì mới capture
        if (inRange && Input.GetKey(KeyCode.R))
        {
            beingCapturedFlag = true;
            captureFlagProgress += Time.deltaTime * 2f;
        }
        else
        {
            beingCapturedFlag = false;
        }

        // Nếu không capture thì giảm dần
        if (!beingCapturedFlag)
        {
            captureFlagProgress -= decayRateFlag * Time.deltaTime;
            if (captureFlagProgress < 0) captureFlagProgress = 0;
        }

        // Cập nhật UI
        UIManager.Instance.UpdateCapturebar(captureFlagProgress);

        // Nếu hoàn thành
        if (captureFlagProgress >= captureFlagNeeded)
        {
            Debug.Log("[Flag] Capture hoàn tất! Event thành công.");
            EventManager.Instance.EndEvent(true);
            Destroy(gameObject); // Xóa cờ
        }
    }

    private void OnDestroy()
    {
        UIManager.Instance.HideCaptureBar();
        Debug.Log("[Flag] Cờ đã bị hủy.");
    }
    private void OnDrawGizmos()
    {
        // Màu trong suốt để fill
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawCube(transform.position, halfExtents * 2);

        // Màu đậm để viền
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, halfExtents * 2);
    }
}
