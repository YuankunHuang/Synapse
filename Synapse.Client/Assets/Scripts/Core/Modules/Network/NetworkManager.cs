using System;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using Synapse.Shared.Protocol;

namespace Synapse.Client.Core.Network
{
    public class NetworkManager : INetworkManager
    {
        public string ConnectionId => _connection?.ConnectionId ?? string.Empty;

        private long _lastPing;
        private int _bytesPerSec;
        private int _bytesReceivedThisSec;
        private float _lastByteCheckTime;
        private bool _isConnectionInitialized;
        
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
                    
                    // Time can only be accessed from main thread
                    MonoBehaviourUtil.Instance.RunOnMainThread(() =>
                    {
                        // stats
                        // 1. bytes per sec
                        _bytesReceivedThisSec += data.Length;
                        if (Time.time - _lastByteCheckTime >= 1f)
                        {
                            _bytesPerSec = _bytesReceivedThisSec;
                            _bytesReceivedThisSec = 0;
                            _lastByteCheckTime = Time.time;
                            EventBus.Publish(EventKeys.NetworkBandwidthUpdated, _bytesPerSec);
                        }
                        // 2. ping
                        var lag = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - worldState.ServerTime;
                        _lastPing = lag > 0 ? lag : 0;
                        EventBus.Publish(EventKeys.NetworkPingUpdated, _lastPing);
                    });

                    EventBus.Publish(EventKeys.WorldStateUpdate, worldState);
                    // Debug.Log($"[SignalR] ReceiveWorldState. ServerTime: {worldState.ServerTime} | PlayerCount: {worldState.Players.Count}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Parse Error: {e.Message}");
                }
            });
            
            // 3. start connection
            try
            {
                _isConnectionInitialized = false;
                await _connection.StartAsync();
                
                Debug.Log($"[Client] Connection Successful!");
                MonoBehaviourUtil.Instance.StartCoroutine(SimulateMovementLoop());
            }
            catch (Exception e)
            {
                Debug.LogError($"[Client] Connection Failed: {e.Message}");
            }

            MonoBehaviourUtil.OnUpdate += OnUpdate;
        }

        private System.Collections.IEnumerator SimulateMovementLoop()
        {
            while (true)
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    if (!string.IsNullOrEmpty(_connection.ConnectionId))
                    {
                        var deltaX = 0f;
                        var deltaZ = 0f;
                        
                        // test moving self with input
                        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                        {
                            deltaX = Input.GetAxisRaw("Horizontal") * Time.deltaTime * 100;
                            deltaZ = Input.GetAxisRaw("Vertical") * Time.deltaTime * 100;
                        }
                        
                        // keep polling
                        EventBus.Publish<(string, Action<PlayerState>)>(EventKeys.GetPlayerState, (_connection.ConnectionId, state =>
                        {
                            if (state != null)
                            {
                                state.Position = new Vec3()
                                {
                                    X = state.Position.X + deltaX,
                                    Y = state.Position.Y,
                                    Z = state.Position.Z + deltaZ
                                };
                                _connection.InvokeAsync("SyncPosition", state.ToByteArray());
                            }
                        }));  
                    }
                }

                yield return null;
            }
        }

        public async void Dispose()
        {
            MonoBehaviourUtil.OnUpdate -= OnUpdate;
            
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
        }

        private void OnUpdate()
        {
            if (!_isConnectionInitialized)
            {
                if (!string.IsNullOrEmpty(ConnectionId))
                {
                    _isConnectionInitialized = true;
                    EventBus.Publish(EventKeys.NetworkConnectionInitialized, ConnectionId);
                }
            }
        }
    }
}