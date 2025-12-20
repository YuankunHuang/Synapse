using System;
using System.Collections.Generic;
using Synapse.Client.Game;
using Synapse.Shared.Protocol;
using UnityEngine;

namespace Synapse.Client.Core
{
    public class WorldManager : ICoreManager
    {
        private WorldRoot _worldRoot;

        private Dictionary<string, PlayerController> _players;

        public WorldManager(WorldRoot worldRoot)
        {
            _worldRoot = worldRoot;
            _players = new Dictionary<string, PlayerController>();
        }

        public void Init()
        {
            EventBus.Subscribe<WorldState>(EventKeys.WorldStateUpdate, OnWorldStateUpdate);
            EventBus.Subscribe<(string, Action<PlayerState>)>(EventKeys.GetPlayerState, OnGetPlayerState);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<WorldState>(EventKeys.WorldStateUpdate, OnWorldStateUpdate);
            EventBus.Unsubscribe<(string, Action<PlayerState>)>(EventKeys.GetPlayerState, OnGetPlayerState);
        }

        private void OnGetPlayerState((string, Action<PlayerState>) data)
        {
            var player = GetOrAddPlayer(data.Item1, out var isNew);
            var state = new PlayerState()
            {
                Id = data.Item1,
                Position = player.Transform.position.ToVec3(),
            };
            data.Item2.Invoke(state);
        }

        private void OnWorldStateUpdate(WorldState worldState)
        {
            if (worldState == null)
            {
                Debug.LogError($"[SignalR] World state is null.");
                return;
            }

            MonoBehaviourUtil.Instance.RunOnMainThread(() =>
            {
                var playerIds = new HashSet<string>(_players.Keys);
                for (var i = 0; i < worldState.Players.Count; ++i)
                {
                    if (worldState.Players[i] != null)
                    {
                        playerIds.Remove(worldState.Players[i].Id);
                        OnPlayerStateUpdate(worldState.Players[i]);
                    }
                }

                foreach (var id in playerIds)
                {
                    RemovePlayer(id);
                }
            });
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

            player.Transform.position = isNew
                ? targetPos
                : Vector3.Lerp(player.Transform.position, targetPos, 10 * Time.deltaTime);
            player.Transform.rotation = isNew
                ? targetRot
                : Quaternion.Lerp(player.Transform.rotation, targetRot, 10 * Time.deltaTime);
        }

        private PlayerController GetOrAddPlayer(string id, out bool isNew)
        {
            isNew = false;

            if (!_players.TryGetValue(id, out var player))
            {
                var cfg = GameObject.Instantiate(_worldRoot.PlayerPrefab, _worldRoot.PlayerRoot).GetComponent<PlayerConfig>();
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
                GameObject.Destroy(p.Transform.gameObject);
                _players.Remove(id);
            }
        }
    }
}