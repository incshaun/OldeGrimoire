using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Fusion;
using Fusion.Sockets;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public GameObject networkControlPanel;
    public GameObject avatarPrefab;
    
    private NetworkRunner networkManager;
    
    public void startServer ()
    {
        startNetwork (GameMode.Host);
    }
    
    public void startClient ()
    {
        startNetwork (GameMode.Client);
    }
    
    private async void startNetwork (GameMode mode)
    {
        networkManager = gameObject.AddComponent <NetworkRunner> ();
        
        await networkManager.StartGame (new StartGameArgs () { GameMode = mode, SessionName = "SampleRoom" });
        
        networkManager.ProvideInput = true;
        networkControlPanel.SetActive (false);
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    { 
        if (networkManager.IsServer)
        {
            NetworkObject participant = networkManager.Spawn (avatarPrefab, Vector3.zero, Quaternion.identity, player);
            networkManager.SetPlayerObject (player, participant);
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) 
    { 
        InputNetworkData ind = new InputNetworkData ();
        ind.turnAmount = Input.GetAxis ("Horizontal");
        ind.forwardAmount = Input.GetAxis ("Vertical");
//         Debug.Log ("Got Input " + ind.turnAmount + " " + ind.forwardAmount);
        input.Set (ind);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    
    public void updateNickName (string name)
    {
        // Add this only once the AvatarName class has been created.
        networkManager.GetPlayerObject (networkManager.LocalPlayer).gameObject.GetComponent <AvatarName> ().setName (name);
    }
}
