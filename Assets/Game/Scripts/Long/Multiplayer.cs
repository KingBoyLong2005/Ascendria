using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class Multiplayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    public static Multiplayer Instance { get; private set; }
    public static bool IsAuthenticated { get; private set; }
    public event Action OnAuthenticated; // Sự kiện thông báo xác thực hoàn tất

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
        if (!IsAuthenticated)
        {
            try
            {
                await UnityServices.InitializeAsync();
                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log("Đăng nhập thành công: " + AuthenticationService.Instance.PlayerId);
                    IsAuthenticated = true;
                    OnAuthenticated?.Invoke(); // Kích hoạt sự kiện
                };
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e)
            {
                Debug.LogError("Lỗi xác thực: " + e);
            }
        }
    }

    public async Task<string> SetupRelayForHost()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Relay Code: " + joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, "wss"));
            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Lỗi RelayService: " + e.Message);
            return null;
        }
    }

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
            Debug.LogError("Lỗi khi tham gia Relay: " + e);
        }
    }
}