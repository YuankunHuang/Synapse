using UnityEngine;

namespace Synapse.Client.Core
{
    public class WorldRoot : MonoBehaviour
    {
        [SerializeField] private Transform _playerRoot;
        [SerializeField] private GameObject _playerPrefab;
        
        public Transform PlayerRoot => _playerRoot;
        public GameObject PlayerPrefab => _playerPrefab;
    }
}