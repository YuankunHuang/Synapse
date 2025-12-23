using System;
using System.Collections.Generic;
using Synapse.Client.Game;
using Synapse.Shared.Protocol;
using UnityEngine;
using Synapse.Client.UI;
using YuankunHuang.Unity.SimpleObjectPool;

namespace Synapse.Client.Core.World
{
    public class WorldManager : IWorldManager
    {
        private WorldRoot _worldRoot;
        private string _localPlayerId;

        private Dictionary<string, PlayerController> _players;

        // expired player cleanup
        private const float PLAYER_TTL_SEC = .5f;
        private const float CLEANUP_PERIOD_SEC = 1f;
        private float _lastCleanupTime;
        private List<string> _idsToRemove;

        public WorldManager(WorldRoot worldRoot)
        {
            _worldRoot = worldRoot;
            _players = new Dictionary<string, PlayerController>();
            _idsToRemove = new List<string>();
        }

        public void Init()
        {
            EventBus.Subscribe<string>(EventKeys.NetworkConnectionInitialized, OnNetworkConnectionInitialized);
            EventBus.Subscribe<WorldState>(EventKeys.WorldStateUpdate, OnWorldStateUpdate);
            EventBus.Subscribe<PlayerState>(EventKeys.PlayerStateUpdate, OnPlayerStateUpdate);
            EventBus.Subscribe<(string, Action<PlayerState>)>(EventKeys.GetPlayerState, OnGetPlayerState);

            MonoBehaviourUtil.OnUpdate += OnUpdate;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<string>(EventKeys.NetworkConnectionInitialized, OnNetworkConnectionInitialized);
            EventBus.Unsubscribe<WorldState>(EventKeys.WorldStateUpdate, OnWorldStateUpdate);
            EventBus.Unsubscribe<PlayerState>(EventKeys.PlayerStateUpdate, OnPlayerStateUpdate);
            EventBus.Unsubscribe<(string, Action<PlayerState>)>(EventKeys.GetPlayerState, OnGetPlayerState);
            
            MonoBehaviourUtil.OnUpdate -= OnUpdate;
        }
        
        private void CleanupExpiredPlayers()
        {
            foreach (var kv in _players)
            {
                var id = kv.Key;
                if (id == _localPlayerId)
                {
                    continue; // never remove local player
                }   

                var player = kv.Value;
                if (Time.time - player.LastUpdateTime > PLAYER_TTL_SEC)
                {
                    _idsToRemove.Add(id);
                }
            }

            foreach (var id in _idsToRemove)
            {
                RemovePlayer(id);
            }

            _idsToRemove.Clear();
        }

        private void OnUpdate()
        {
            if (Time.time - _lastCleanupTime > CLEANUP_PERIOD_SEC)
            {
                _lastCleanupTime = Time.time;
                CleanupExpiredPlayers();
            }
        }

        private void OnGetPlayerState((string, Action<PlayerState>) data)
        {
            var player = GetOrAddPlayer(data.Item1, out var isNew);
            var state = new PlayerState()
            {
                Id = data.Item1,
                Position = player.GetPosition().ToVec3(),
            };
            data.Item2.Invoke(state);
        }

        private void OnNetworkConnectionInitialized(string connectionId)
        {
            _localPlayerId = connectionId;
        }

        private void OnWorldStateUpdate(WorldState worldState)
        {
            if (worldState == null)
            {
                Debug.LogError($"[SignalR] World state is null.");
                return;
            }

            MonoBehaviourUtil.Instance.RunOnMainThread(HandleWorldStateUpdate, worldState);
        }

        private void HandleWorldStateUpdate(WorldState worldState)
        {
            for (var i = 0; i < worldState.Players.Count; ++i)
            {
                if (worldState.Players[i] != null)
                {
                    OnPlayerStateUpdate(worldState.Players[i]);
                }
            }
        }

        public void OnPlayerStateUpdate(PlayerState state)
        {
            if (state == null)
            {
                Debug.LogError($"[SignalR] PlayerState is null.");
                return;
            }

            var player = GetOrAddPlayer(state.Id, out var isNew);
            var targetPos = state.Position != null
                ? new Vector3(state.Position.X, state.Position.Y, state.Position.Z)
                : Vector3.zero;
            var targetRot = state.Rotation != null
                ? Quaternion.Euler(state.Rotation.X, state.Rotation.Y, state.Rotation.Z)
                : Quaternion.identity;

            if (state.Id != _localPlayerId && !isNew)
            {
                targetPos = Vector3.Lerp(player.GetPosition(), targetPos, 10 * Time.deltaTime);
                targetRot = Quaternion.Lerp(player.GetRotation(), targetRot, 10 * Time.deltaTime);
            }
            
            player.Update(targetPos, targetRot);
        }

        private PlayerController GetOrAddPlayer(string id, out bool isNew)
        {
            isNew = false;

            if (!_players.TryGetValue(id, out var player))
            {
                var cfg = PoolService.Get<PlayerConfig>(_worldRoot.PlayerPrefab, Vector3.zero,  Quaternion.identity, _worldRoot.PlayerRoot);
                player = new PlayerController(cfg, id);
                _players[id] = player;
                isNew = true;
            }

            return player;
        }

        private void RemovePlayer(string id)
        {
            if (_players.TryGetValue(id, out var p))
            {
                PoolService.Release(p.Config.gameObject);
                _players.Remove(id);
            }
        }
    }
}