using TMPro;
using UnityEngine;
using Unity;
using UnityEngine.UI;

namespace Synapse.Client.UI
{
    public class UIRoot : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statsTxt;

        public TMP_Text StatsTxt => _statsTxt;
    }
}