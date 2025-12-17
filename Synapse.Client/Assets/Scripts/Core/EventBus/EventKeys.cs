namespace Synapse.Client.Core
{
    public class EventKeys
    {
        public static int KeyStart = 0;
        public static readonly int StateUpdate = GetNewKey();
        public static readonly int GameInitialized = GetNewKey();

        public static int GetNewKey()
        {
            return ++KeyStart;
        }
    }
}