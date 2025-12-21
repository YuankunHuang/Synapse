using System.Collections.Generic;
using UnityEngine;
using Synapse.Client.UI;
using Synapse.Client.Util;

namespace Synapse.Client.Core.UI
{
    public class UIManager : IUIManager
    {
        private UIRoot _uiRoot;
        
        // =========== Stats ===========
        private float _deltaTime;
        private long _ping;
        private int _bytesPerSec;
        private string _clientId;

        private bool _isStatsDirty = false;
        // =============================
        
        public UIManager(UIRoot uiRoot)
        {
            _uiRoot = uiRoot;
        }
        
        public void Init()
        {
            MonoBehaviourUtil.OnUpdate += OnUpdate;
            
            EventBus.Subscribe<long>(EventKeys.NetworkPingUpdated, OnNetworkPingUpdated);
            EventBus.Subscribe<int>(EventKeys.NetworkBandwidthUpdated, OnNetworkBandwidthUpdated);
            EventBus.Subscribe<string>(EventKeys.NetworkConnectionInitialized, OnNetworkConnectionInitialized);
        }

        public void Dispose()
        {
            MonoBehaviourUtil.OnUpdate -= OnUpdate;
            
            EventBus.Unsubscribe<long>(EventKeys.NetworkPingUpdated, OnNetworkPingUpdated);
            EventBus.Unsubscribe<int>(EventKeys.NetworkBandwidthUpdated, OnNetworkBandwidthUpdated);
            EventBus.Unsubscribe<string>(EventKeys.NetworkConnectionInitialized, OnNetworkConnectionInitialized);
        }

        private void OnUpdate()
        {
            var deltaTime = _deltaTime + (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            if (_deltaTime != deltaTime)
            {
                _isStatsDirty = true;
                _deltaTime = deltaTime;
            }

            if (_isStatsDirty)
            {
                RefreshStats();
                _isStatsDirty = false;
            }
        }

        private void OnNetworkPingUpdated(long ping)
        {
            if (ping != _ping)
            {
                _ping = ping;
                _isStatsDirty = true;
            }
        }

        private void OnNetworkBandwidthUpdated(int bytesPerSec)
        {
            if (bytesPerSec != _bytesPerSec)
            {
                _bytesPerSec = bytesPerSec;
                _isStatsDirty = true;
            }
        }

        private void OnNetworkConnectionInitialized(string connectionId)
        {
            if (connectionId != _clientId)
            {
                _clientId = connectionId;
                _isStatsDirty = true;
            }
        }

        private void RefreshStats()
        {
            // ping
            var pingStr = string.Empty;
            if (_ping < 100)
            {
                pingStr = UIUtil.GetColoredString($"{_ping}", UIUtil.TextColor.Green);
            }
            else if (_ping < 200)
            {
                pingStr = UIUtil.GetColoredString($"{_ping}", UIUtil.TextColor.Yellow);
            }
            else
            {
                pingStr = UIUtil.GetColoredString($"{_ping}", UIUtil.TextColor.Red);
            }
            
            // bandwidth
            var bandwidthStr = string.Empty;
            var kbps = _bytesPerSec / 1024.0f;
            if (kbps > 1)
            {
                var mbps = kbps / 1024.0f;
                if (mbps > 1)
                {
                    bandwidthStr = $"Bandwidth: {mbps:0.00} MB/s";
                }
                else
                {
                    bandwidthStr = $"Bandwidth: {kbps:0.00} KB/s";
                }
            }
            else
            {
                bandwidthStr = $"Bandwidth: {_bytesPerSec:0.00} B/s";
            }

            _uiRoot.StatsTxt.text = $@"
SYNAPSE ENGINE DEBUG
----------------------------------------
FPS: {1.0f / _deltaTime:F0}
Ping (Latency): {pingStr} ms
Bandwidth: {bandwidthStr}
Connection ID: {_clientId}
";
        }
    }    
}