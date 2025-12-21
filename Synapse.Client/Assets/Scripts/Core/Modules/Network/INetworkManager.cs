using UnityEngine;

namespace Synapse.Client.Core.Network
{
    public interface INetworkManager : ICoreManager
    {
        public string ConnectionId { get; }
    }    
}