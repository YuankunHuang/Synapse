using System;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using Synapse.Shared.Protocol;

namespace Synapse.Client.Core
{
    public class NetworkManager : ICoreManager
    {
        private HubConnection _connection;

        private string _serverUrl = "http://localhost:5241/gamehub";
        
        public async void Init()
        {
            Debug.Log($"[Client] Connecting to {_serverUrl}...");

            // 1. build connection
            _connection = new HubConnectionBuilder()
                .WithUrl(_serverUrl)
                .WithAutomaticReconnect()
                .Build();
            
            // 2. listen to server
            _connection.On<byte[]>("ReceiveWorldState", data =>
            {
                try
                {
                    var worldState = WorldState.Parser.ParseFrom(data);
                    Debug.Log($"[SignalR] Received state. ServerTime: {worldState.ServerTime} | PlayerCount: {worldState.Players.Count}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Parse Error: {e.Message}");
                }
            });
            
            // 3. start connection
            try
            {
                await _connection.StartAsync();
                Debug.Log($"[Client] Connection Successful!");

                MonoBehaviourUtil.Instance.StartCoroutine(SimulateMovementLoop());
            }
            catch (Exception e)
            {
                Debug.LogError($"[Client] Connection Failed: {e.Message}");
            }
        }

        private System.Collections.IEnumerator SimulateMovementLoop()
        {
            while (true)
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    var currentState = new PlayerState()
                    {
                        Id = _connection.ConnectionId,
                        Position = new Vec3()
                        {
                            X = UnityEngine.Random.Range(0, 10f),
                            Y = 0,
                            Z = UnityEngine.Random.Range(0, 10f)
                        }
                    };
                    _connection.InvokeAsync("SyncPosition", currentState.ToByteArray());
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        public async void Dispose()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}