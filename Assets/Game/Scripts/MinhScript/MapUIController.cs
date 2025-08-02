using UnityEngine;

public class MapUIController : MonoBehaviour
{
    public GameObject mapPanel;
    public GameObject EButton;
    public PlayerController playerController;
    public TPCameraController cameraController;

    private bool isMapOpen = false;

    void Update()
    {
        if (isMapOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMap();
        }
    }

    public void OpenMap()
    {
        mapPanel.SetActive(true);
        EButton.SetActive(false);

        if (playerController != null)
            playerController.isUIOpen = true;

        if (cameraController != null)
            cameraController.isUIOpen = true;

        isMapOpen = true;
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
        EButton.SetActive(true);

        if (playerController != null)
            playerController.isUIOpen = false;

        if (cameraController != null)
            cameraController.isUIOpen = false;

        isMapOpen = false;
    }
}
