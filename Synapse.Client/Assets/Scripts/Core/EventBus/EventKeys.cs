namespace Synapse.Client.Core
{
    public class EventKeys
    {
        public static int KeyStart = 0;
        public static readonly int WorldStateUpdate = GetNewKey();
        public static readonly int PlayerStateUpdate = GetNewKey();
        public static readonly int GameInitialized = GetNewKey();
        public static readonly int GetPlayerState = GetNewKey();
        public static readonly int NetworkPingUpdated = GetNewKey();
        public static readonly int NetworkBandwidthUpdated = GetNewKey();
        public static readonly int NetworkConnectionInitialized = GetNewKey();
        
        public static int GetNewKey()
        {
            return ++KeyStart;
        }
    }
}