using UnityEngine;

namespace Synapse.Client.Core
{
    public class MonoBehaviourUtil : MonoBehaviour
    {
        public static MonoBehaviourUtil Instance { get; private set; }

        public MonoBehaviourUtil()
        {
            Instance = this;
        }
    }    
}