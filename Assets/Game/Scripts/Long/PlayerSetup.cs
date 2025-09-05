using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private AudioListener listener;
    [SerializeField] private CinemachineCamera cc;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (listener != null) listener.enabled = true;
            if (cc != null) cc.Priority = 1;


        }
        else
        {
            if (listener != null) listener.enabled = false;
            if (cc != null) cc.Priority = 0;


        }
    }

}
