using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class PlayerNetworkManager : NetworkBehaviour
{
    // private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
    //     new MyCustomData
    //     {
    //         _int = 56,
    //         _bool = true,
    //     }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // struct MyCustomData : INetworkSerializable
    // {
    //     public int _int;
    //     public bool _bool;
    //     public FixedString128Bytes message;

    //     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter

    //     {
    //         serializer.SerializeValue(ref _int);
    //         serializer.SerializeValue(ref _bool);
    //         serializer.SerializeValue(ref message);
    //     }
    // }
    // public override void OnNetworkSpawn()
    // {
    //     randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
    //     {
    //         Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
    //     };
    // }
    private void Update()
    {
        if (!IsOwner) return;
        // Debug.Log(OwnerClientId + " ; random number: " + randomNumber.Value);

        // if (Input.GetKey(KeyCode.T))
        // {
        //     // randomNumber.Value = new MyCustomData
        //     // {
        //     //     _int = 10,
        //     //     _bool = false,
        //     //     message = " hello world",
        //     // };
        //     // testServerRPC1("hello");
        //     testServerRPC2(new ServerRpcParams());
        // }

        // if (Input.GetKey(KeyCode.E))
        // {

        //     testServerRPC1("hello");
        // }

        Vector3 moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    //[ServerRpc] // chủ host khi chạy sẽ nhìn thấy, nhưng client chạy thì không nhưng host vẫn thấy và có thể thấy ai chạy
    // private void testServerRPC1(string message)
    // {
    //     Debug.Log(message);
    // }

    // // chủ host khi chạy sẽ nhìn thấy, nhưng client chạy thì không nhưng host vẫn thấy và có thể thấy ai chạy
    // private void testServerRPC2(ServerRpcParams serverRpcParams)
    // {

    //     Debug.Log("test server rpc: " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    // }

    // [ClientRPC] thì ngược lại
}
