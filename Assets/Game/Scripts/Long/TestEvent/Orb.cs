using UnityEngine;

public class Orb : MonoBehaviour
{
    public float captureOrbProgress = 0f;
    public float captureNeededOrb = 5f; // thời gian/điểm cần để bắt cờ
    public float decayRateOrb = 1f; // tốc độ giảm khi không click
    public Vector3 halfExtents = new Vector3(3f, 3f, 3f);
    public LayerMask playerLayer;
    private bool beingCapturedOrb = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIManager.Instance.ShowCaptureBar(captureNeededOrb);
    }

    void Update()
    {
        // bool pressed = Input.GetKey(KeyCode.D) || Input.GetMouseButton(0);

        // if (pressed) {
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     if (Physics.Raycast(ray, out RaycastHit hit)) {
        //         if (hit.collider != null && hit.collider.gameObject == gameObject) {
        //             beingCapturedOrb = true;
        //             captureOrbProgress += Time.deltaTime * 2f; // tốc độ chiếm
        //         } else {
        //             beingCapturedOrb = false;
        //         }
        //     }
        // }
        // else {
        //     beingCapturedOrb = false;
        // }

        Collider[] hits = Physics.OverlapBox(
            transform.position,
            halfExtents,
            Quaternion.identity,
            playerLayer
        );
        bool inRange = hits.Length > 0;

        // Chỉ khi trong phạm vi và nhấn R thì mới capture
        if (inRange && Input.GetKey(KeyCode.R))
        {
            beingCapturedOrb = true;
            captureOrbProgress += Time.deltaTime * 2f;
        }
        else
        {
            beingCapturedOrb = false;
        }

        // Nếu không capture thì giảm dần
        if (!beingCapturedOrb)
        {
            captureOrbProgress -= decayRateOrb * Time.deltaTime;
            if (captureOrbProgress < 0) captureOrbProgress = 0;
        }

        // Cập nhật UI
        UIManager.Instance.UpdateCapturebar(captureOrbProgress);

        // Nếu hoàn thành
        if (captureOrbProgress >= captureNeededOrb)
        {
            EventManager.Instance.EndEvent(true);
            Destroy(gameObject); // Xóa cờ
        }
    }

    private void OnDestroy()
    {
        UIManager.Instance.HideCaptureBar();
        Debug.Log("Orb đã bị hủy.");
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
