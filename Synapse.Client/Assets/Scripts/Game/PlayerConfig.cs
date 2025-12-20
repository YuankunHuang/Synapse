using UnityEngine;

namespace Synapse.Client.Game
{
    public class PlayerConfig : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRendererTorso;
        [SerializeField] private Material _matSelfTorso;
        [SerializeField] private Material _matOtherTorso;

        public MeshRenderer MeshRendererTorso => _meshRendererTorso;
        public Material MatSelfTorso => _matSelfTorso;
        public Material MatOtherTorso => _matOtherTorso;
    }    
}