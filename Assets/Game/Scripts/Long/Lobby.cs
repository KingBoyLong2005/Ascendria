using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    public static Lobby Instance { get; private set; }
    private Unity.Services.Lobbies.Models.Lobby hostLobby;
    private Unity.Services.Lobbies.Models.Lobby joinLobby;
    private float hearBeatLobbyTimer = 15f;
    private float updateLobbyPollTimer = 5f;
    private float sendLastSeenTimer = 10f;
    private float checkInactiveTimer = 10f;
    private const float HEARTBEAT_INTERVAL = 15f;
    private const float LOBBY_POLL_INTERVAL = 5f;
    private const float LASTSEEN_INTERVAL = 10f;
    private const float CHECK_INACTIVE_INTERVAL = 10f;
    private const int TIMEOUT_SECONDS = 30;
    private string playerName;

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
        while (!Multiplayer.IsAuthenticated)
        {
            await Task.Delay(100); // Chờ xác thực từ Multiplayer
        }
        playerName = "Player" + UnityEngine.Random.Range(0, 100);
        Debug.Log("Tên người chơi: " + playerName);
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdate();
        ClientSendLastSeen();
        HostCheckInactivePlayers();
    }

    private bool IsLobbyHost()
    {
        return joinLobby != null && joinLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void HandleLobbyHeartBeat()
    {
        if (IsLobbyHost())
        {
            hearBeatLobbyTimer -= Time.deltaTime;
            if (hearBeatLobbyTimer <= 0f)
            {
                hearBeatLobbyTimer = HEARTBEAT_INTERVAL;
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                    Debug.Log("Heartbeat sent for lobby: " + hostLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"Heartbeat failed: {e.Message}");
                }
            }
        }
    }

    private async void HandleLobbyPollForUpdate()
    {
        if (joinLobby == null) return;

        updateLobbyPollTimer -= Time.deltaTime;
        if (updateLobbyPollTimer <= 0f)
        {
            updateLobbyPollTimer = LOBBY_POLL_INTERVAL;
            try
            {
                joinLobby = await LobbyService.Instance.GetLobbyAsync(joinLobby.Id);
                Debug.Log("Lobby updated: " + joinLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Lobby update failed: {e.Message}");
                updateLobbyPollTimer = LOBBY_POLL_INTERVAL + 5f;
            }
        }
    }

    private async void ClientSendLastSeen()
    {
        if (joinLobby == null || AuthenticationService.Instance.PlayerId == hostLobby?.HostId) return;

        sendLastSeenTimer -= Time.deltaTime;
        if (sendLastSeenTimer <= 0f)
        {
            sendLastSeenTimer = LASTSEEN_INTERVAL;
            try
            {
                long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                await LobbyService.Instance.UpdatePlayerAsync(joinLobby.Id,
                    AuthenticationService.Instance.PlayerId,
                    new UpdatePlayerOptions
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "lastSeen", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, unix.ToString()) }
                        }
                    });
                Debug.Log($"Sent lastSeen = {unix}");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning("Send lastSeen failed: " + e.Message);
            }
        }
    }

    private async void HostCheckInactivePlayers()
    {
        if (!IsLobbyHost() || hostLobby == null) return;

        checkInactiveTimer -= Time.deltaTime;
        if (checkInactiveTimer <= 0f)
        {
            checkInactiveTimer = CHECK_INACTIVE_INTERVAL;
            try
            {
                hostLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                foreach (var pl in hostLobby.Players)
                {
                    if (pl.Id == AuthenticationService.Instance.PlayerId) continue;
                    if (pl.Data != null && pl.Data.TryGetValue("lastSeen", out var ds)
                        && long.TryParse(ds.Value, out var last))
                    {
                        if (now - last > TIMEOUT_SECONDS)
                        {
                            Debug.Log($"Removing inactive {pl.Id} (lastSeen {now-last}s ago)");
                            await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, pl.Id);
                        }
                    }
                    else
                    {
                        Debug.Log($"Player {pl.Id} has no lastSeen → remove");
                        await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, pl.Id);
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning("Host check failed: " + e.Message);
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "hey";
            int maxPlayer = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag", DataObject.IndexOptions.S1) }
                }
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, createLobbyOptions);
            if (AuthenticationService.Instance.PlayerId == lobby.HostId)
            {
                hostLobby = lobby;
            }
            joinLobby = lobby;

            string relayCode = await Multiplayer.Instance.SetupRelayForHost();
            if (relayCode != null)
            {
                await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });
                Debug.Log("Created Lobby: " + lobby.Name + " " + lobby.MaxPlayers + " Code join: " + lobby.LobbyCode + " Relay Code: " + relayCode);
                LobbyUI.Instance.UpdateLobbyCodeJoin(lobby.LobbyCode);
            }
            else
            {
                Debug.LogError("Không thể tạo relay");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Lỗi khi tạo lobby: " + e);
        }
    }

    public async void joinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer(),
            };
            var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinLobby = lobby;
            Debug.Log("Đã tham gia lobby với mã " + lobbyCode);

            if (lobby.Data.TryGetValue("RelayCode", out DataObject relayCodeData))
            {
                string relayCode = relayCodeData.Value;
                LobbyUI.Instance.UpdateLobbyCodeJoin(lobby.LobbyCode);
                await Multiplayer.Instance.JoinRelayAsync(relayCode);
            }
            else
            {
                Debug.LogError("Không tìm thấy RelayCode trong dữ liệu lobby");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Lỗi khi tham gia lobby: " + e);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }
}