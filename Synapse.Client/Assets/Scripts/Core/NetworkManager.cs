using System;
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
            _connection.On<byte[]>("ReceiveMessage", data =>
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
            }
            catch (Exception e)
            {
                Debug.LogError($"[Client] Connection Failed: {e.Message}");
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