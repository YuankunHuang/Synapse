using UnityEngine;

namespace YuankunHuang.Unity.SimpleObjectPool
{
    /// <summary>
    /// Identify a GameObject as a pooled instance and store its pool key.
    /// Suggested to be managed by the pool without being known by the user.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PooledObject : MonoBehaviour
    {
        /// <summary>
        /// The prefab used as the pool key.
        /// </summary>
        public GameObject Prefab { get; internal set; } // can only be updated within the tool assembly itself
    }
}