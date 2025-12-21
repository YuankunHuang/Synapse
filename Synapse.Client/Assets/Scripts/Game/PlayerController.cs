using Synapse.Client.Core;
using Synapse.Client.Core.Network;
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

            _isSelf = ModuleRegistry.Get<INetworkManager>().ConnectionId == id;
            _config.MeshRendererTorso.sharedMaterial = _isSelf
                ? _config.MatSelfTorso
                : _config.MatOtherTorso;
        }
    }
}