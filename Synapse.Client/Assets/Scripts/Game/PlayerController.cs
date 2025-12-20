using Synapse.Client.Core;
using UnityEngine;

namespace Synapse.Client.Game
{
    public class PlayerController
    {
        public Transform Transform => _config?.transform;
        
        private readonly PlayerConfig _config;
        private readonly bool _isSelf;
        
        public PlayerController(PlayerConfig config, string id)
        {
            _config = config;

            _isSelf = ModuleRegistry.Get<NetworkManager>().ConnectionId == id;
            _config.MeshRendererTorso.sharedMaterial = _isSelf
                ? _config.MatSelfTorso
                : config.MatOtherTorso;
        }
    }
}