using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Netcode;
using System;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class NetworkConnect : MonoBehaviour
{
    [SerializeField]
    UnityTransport transport;
    public const int MaxPlayers = 100;

    private Lobby connectedLobby;
    private const string joinCode = "j";

    private async void Awake()
    {
        if (transport == null)
            transport = FindFirstObjectByType<UnityTransport>();

        await Authenticate();
        CreateOrJoin();
    }


    private static async Task Authenticate()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
        }

        if(!AuthenticationService.Instance.IsAuthorized)
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateOrJoin()
    {
        connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
    }

    private void OnPlayerConnected(ulong clientId)
    {

        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            GameStartManager.Instance.StartGame();
        }
    }

    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[joinCode].Value);
            SetTransforAsClient(a);
            NetworkManager.Singleton.StartClient();
            return lobby;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    private async Task<Lobby> CreateLobby()
    {
        try
        {
            var a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
            var joinCode2 = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {joinCode, new DataObject(DataObject.VisibilityOptions.Public, joinCode2) }
                }
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync("A Name", MaxPlayers, options);

            StartCoroutine(HeartbeatLobby(lobby.Id, 15));

            transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            NetworkManager.Singleton.StartHost();
            return lobby;
        }

        catch (Exception e)
        {
            return null;
        }
    }

    private static IEnumerator HeartbeatLobby(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void SetTransforAsClient(JoinAllocation a)
    {
        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

    }

    private void OnDestroy()
    {
        try
        {
            StopAllCoroutines();
            if (connectedLobby != null)
            {
                var playerId = PlayerPrefs.GetString("playerId", "");
                if (connectedLobby.HostId == playerId)
                    LobbyService.Instance.DeleteLobbyAsync(connectedLobby.Id);
                else
                    LobbyService.Instance.RemovePlayerAsync(connectedLobby.Id, playerId);
            }
        }

        catch (Exception e)
        {
        }
    }
}
