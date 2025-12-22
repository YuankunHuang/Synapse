using Synapse.Client.Core;
using Synapse.Client.Core.Network;
using UnityEngine;

namespace Synapse.Client.Game
{
    public class PlayerController
    {
        public PlayerConfig Config => _config;
        public float LastUpdateTime { get; private set; }
        
        private readonly PlayerConfig _config;
        private readonly bool _isSelf;
        
        public PlayerController(PlayerConfig config, string id)
        {
            _config = config;

            _isSelf = ModuleRegistry.Get<INetworkManager>().ConnectionId == id;
            _config.MeshRendererTorso.sharedMaterial = _isSelf
                ? _config.MatSelfTorso
                : _config.MatOtherTorso;
        }

        public Vector3 GetPosition()
        {
            return _config?.transform.position ?? Vector3.zero;
        }

        public Quaternion GetRotation()
        {
            return _config?.transform.rotation ?? Quaternion.identity;
        }

        public void Update(Vector3 position, Quaternion rotation)
        {
            if (_config == null)
            {
                return;
            }

            _config.transform.position = position;
            _config.transform.rotation = rotation;

            LastUpdateTime = Time.time;
        }
    }
}