using System;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public class Multiplayer : NetworkBehaviour
{   
    [SerializeField] private GameObject playerPrefab;
    public static Multiplayer Instance { get; private set; }
    private string relayJoinCode;
    public static bool isAuthenticated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        if (!isAuthenticated)
        {
            try
            {
                await UnityServices.InitializeAsync();
                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log("Đăng nhập thành công: " + AuthenticationService.Instance.PlayerId);
                    isAuthenticated = true;
                };
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e)
            {
                Debug.LogError("Lỗi xác thực: " + e);
            }
        }
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joincode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, "wss"));
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }


    private async void JoinRelay(string code)
    {
        try
        {
            Debug.Log("Joining Relay with " + code);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                AllocationUtils.ToRelayServerData(joinAllocation, "wss"));
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    // Hàm mới để thiết lập relay cho host và trả về mã relay
    public async Task<string> SetupRelayForHost()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joincode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, "wss"));
            NetworkManager.Singleton.StartHost();
            return joincode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Lỗi RelayService: " + e.Message);
            return null;
        }
    }

    // Hàm mới để client tham gia relay
    public async Task JoinRelayAsync(string relayCode)
    {
        try
        {
            Debug.Log("Joining Relay with " + relayCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                AllocationUtils.ToRelayServerData(joinAllocation, "wss"));
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}