using System;
using System.Collections.Concurrent;
using System.Threading;
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

        private int _movementSpeed = 70;
        private int _syncHz = 10;
        private float _lastSyncTime;
        private PlayerState _cachedLocalState;
        
        private HubConnection _connection;
        private int _isJobScheduled = 0;
        private WorldState _latestWorldState;
        private int _hasLatestWorldState;

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
                // immediately count (as bandwidth is already consumed no matter what)
                Interlocked.Add(ref _bytesReceivedThisSec, data.Length);
                
                try
                {
                    var worldState = WorldState.Parser.ParseFrom(data);

                    _latestWorldState = worldState;
                    Volatile.Write(ref _hasLatestWorldState, 1);

                    if (Interlocked.CompareExchange(ref _isJobScheduled, 1, 0) == 0)
                    {
                        // Time can only be accessed from main thread
                        MonoBehaviourUtil.Instance.RunOnMainThread(ProcessLatestWorldState);
                    }
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
            }
            catch (Exception e)
            {
                Debug.LogError($"[Client] Connection Failed: {e.Message}");
            }

            MonoBehaviourUtil.OnUpdate += OnUpdate;
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

        private void ProcessLatestWorldState()
        {
            Interlocked.Exchange(ref _isJobScheduled, 0);
            if (Volatile.Read(ref _hasLatestWorldState) == 1)
            {
                Volatile.Write(ref _hasLatestWorldState, 0);
                
                EventBus.Publish(EventKeys.WorldStateUpdate, _latestWorldState);
                        
                // ping
                var lag = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _latestWorldState.ServerTime;
                _lastPing = lag > 0 ? lag : 0;
                EventBus.Publish(EventKeys.NetworkPingUpdated, _lastPing);
            }
        }

        private void OnUpdate()
        {
            // bandwidth -> processed apart from other stats
            if (Time.time - _lastByteCheckTime >= 1)
            {
                var totalBytes = Interlocked.Exchange(ref _bytesReceivedThisSec, 0);
                _bytesPerSec = totalBytes;
                _lastByteCheckTime = Time.time;
                
                EventBus.Publish(EventKeys.NetworkBandwidthUpdated, _bytesPerSec);
            }
            
            // monitor initialization state
            if (!_isConnectionInitialized)
            {
                if (!string.IsNullOrEmpty(ConnectionId))
                {
                    _isConnectionInitialized = true;
                    EventBus.Publish(EventKeys.NetworkConnectionInitialized, ConnectionId);
                    
                    _cachedLocalState = new PlayerState()
                    {
                        Id = ConnectionId,
                        Position = new Vec3() {X = 0, Y = 0, Z = 0},
                    };
                    
                    EventBus.Publish(EventKeys.PlayerStateUpdate, _cachedLocalState);
                    _lastSyncTime = Time.time;
                    _connection.InvokeAsync("SyncPosition", _cachedLocalState.ToByteArray());
                }
            }

            // movement tick
            if (_connection.State == HubConnectionState.Connected)
            {
                if (!string.IsNullOrEmpty(_connection.ConnectionId))
                {
                    var h = Input.GetAxisRaw("Horizontal");
                    var v = Input.GetAxisRaw("Vertical");
                    if (h != 0 || v != 0)
                    {
                        var deltaX = h * Time.deltaTime * _movementSpeed;
                        var deltaZ = v * Time.deltaTime * _movementSpeed;
                        _cachedLocalState.Position.X += deltaX;
                        _cachedLocalState.Position.Z += deltaZ;
                        
                        // update self (!!! server won't send back our own movement data package to ourselves)
                        // Otherwise it'll cause rollbacks.
                        EventBus.Publish(EventKeys.PlayerStateUpdate, _cachedLocalState);
                        
                        if (Time.time - _lastSyncTime >= 1f / _syncHz)
                        {
                            _lastSyncTime = Time.time;
                            _connection.InvokeAsync("SyncPosition", _cachedLocalState.ToByteArray());
                        }
                    }
                }
            }
        }
    }
}