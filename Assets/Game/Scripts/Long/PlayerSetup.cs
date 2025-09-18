using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private GameObject cameraRigPrefab; // assign prefab CameraRig trong Inspector

    private GameObject localCameraRig;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Spawn camera rig local
            localCameraRig = Instantiate(cameraRigPrefab);

            // Tìm Cinemachine camera trong prefab vừa spawn
            var cc = localCameraRig.GetComponentInChildren<CinemachineCamera>();
            if (cc != null)
            {
                cc.Follow = transform;
                cc.LookAt = transform;
                cc.Priority = 1;
            }

            // Bật AudioListener (chỉ cho local client)
            var listener = localCameraRig.GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && localCameraRig != null)
        {
            Destroy(localCameraRig); // Xóa camera khi player rời game
        }
    }
}
