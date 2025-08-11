using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Netcode.Components;

public class Lobby : MonoBehaviour
{
    public static Lobby Instance { get; private set; }
    private Unity.Services.Lobbies.Models.Lobby hostLobby;
    private Unity.Services.Lobbies.Models.Lobby lobby;
    private static Unity.Services.Lobbies.Models.Lobby joinLobby;
    private float hearBeatLobbyTimer = 15f;
    private float updateLobbyPollTimer = 5f;
    private float sendLastSeenTimer = 0f;
    private float checkInactiveTimer = 0f;


    private const float HEARTBEAT_INTERVAL = 15f;
    private const float LOBBY_POLL_INTERVAL = 5f;
    private const float LASTSEEN_INTERVAL = 10f;
    private const float CHECK_INACTIVE_INTERVAL = 10f;
    private const int TIMEOUT_SECONDS = 30;
    private string playerName;

    public event EventHandler OnLeftLobby;
    private async void Start()
    {
        if (!Multiplayer.isAuthenticated)
        {
            Debug.Log("Đang chờ xác thực từ Multiplayer...");
            while (!Multiplayer.isAuthenticated)
            {
                await Task.Delay(100); // Chờ cho đến khi xác thực hoàn tất
            }
        }
        playerName = "Player" + UnityEngine.Random.Range(0, 100);
        Debug.Log("Tên người chơi: " + playerName);


    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdate();
        ClientSendLastSeen();        // client heartbeat mỗi 10s
        HostCheckInactivePlayers();  // host kiểm tra timeout mỗi 10s
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
                const float hearBeatTimerMax = 15f; 
                hearBeatLobbyTimer = hearBeatTimerMax;
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
        if (joinLobby == null) return; // Avoid unnecessary checks

        updateLobbyPollTimer -= Time.deltaTime;
        if (updateLobbyPollTimer <= 0f)
        {
            const float updateLobbyTimerMax = 5f; // Increased to 5 seconds
            updateLobbyPollTimer = updateLobbyTimerMax;
            try
            {
                lobby = await LobbyService.Instance.GetLobbyAsync(joinLobby.Id);
                joinLobby = lobby;
                Debug.Log("Lobby updated: " + joinLobby.Id);
            }
            catch (LobbyServiceException e)
            {

                Debug.LogWarning("Rate limit exceeded for lobby polling. Retrying after delay.");
                updateLobbyPollTimer = updateLobbyTimerMax + 5f; // Add extra delay on rate limit

                Debug.Log($"Lobby update failed: {e.Message}");

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
                    new UpdatePlayerOptions {
                        Data = new Dictionary<string, PlayerDataObject> {
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

    [Command]
    private async void CreateLobby()
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
            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, createLobbyOptions);
            if (AuthenticationService.Instance.PlayerId == lobby.HostId)
            {
                hostLobby = lobby; // Chỉ gán hostLobby nếu là host
            }
            joinLobby = lobby; // Client và host đều gán joinLobby


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
            }
            else
            {
                Debug.LogError("Không thể tạo relay");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.S1, "CaptureTheFlag", QueryFilter.OpOptions.EQ),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                },
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log("Tìm thấy " + queryResponse.Results.Count + " lobby");
            foreach (Unity.Services.Lobbies.Models.Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void joinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer(),
            };
            lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinLobby = lobby;
            Debug.Log("Đã tham gia lobby với mã " + lobbyCode);
            // Lấy mã relay từ dữ liệu lobby
            if (lobby.Data.TryGetValue("RelayCode", out DataObject relayCodeData))
            {
                string relayCode = relayCodeData.Value;
                // Tham gia relay trước, sau đó lobby đã được tham gia ở trên
                await Multiplayer.Instance.JoinRelayAsync(relayCode);
            }
            else
            {
                Debug.LogError("Không tìm thấy RelayCode trong dữ liệu lobby");
            }
            PrintPlayer(joinLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void quickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log("Đã tham gia Lobby: " + lobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        return new Player()
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
    }

    [Command]
    private void PrintPlayer()
    {
        PrintPlayer(joinLobby);
    }

    private void PrintPlayer(Unity.Services.Lobbies.Models.Lobby lobby)
    {
        Debug.Log("Người chơi trong Lobby: " + lobby.Name + " " + lobby.Data["GameMode"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Người chơi: " + player.Id + " Tên: " + player.Data["PlayerName"].Value);
        }
    }

    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });
            joinLobby = hostLobby;
            PrintPlayer(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    public async Task LeaveLobby()
    {
        if (joinLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id, AuthenticationService.Instance.PlayerId);
                joinLobby = null;
                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    [Command]
    private async void KickLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id, joinLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void MigrateLobbyHost()
    {
        try
        {
            Debug.Log("Mỉgrate update");
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinLobby.Players[1].Id
            });
            joinLobby = hostLobby;

            // Nếu thiết bị hiện tại trở thành host mới
            if (AuthenticationService.Instance.PlayerId == hostLobby.HostId)
            {
                Debug.Log("Thiết bị hiện tại đã trở thành host mới.");
            }
            else
            {
                hostLobby = null; // Xóa hostLobby nếu không còn là host
            }
            PrintPlayer(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public bool IsLobbyActive()
    {
        return joinLobby != null;
    }
}