using UnityEngine;

namespace YuankunHuang.Unity.SimpleObjectPool
{
    /// <summary>
    /// Object pool for a specific prefab.
    /// </summary>
    public sealed class UnityPrefabPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _root;
        private readonly ObjectPool<GameObject> _pool;

        public int CountInactive => _pool.CountInactive;

        public UnityPrefabPool(GameObject prefab, Transform root, int prewarmCount = 0)
        {
            _prefab = prefab;
            _root = root;

            _pool = new ObjectPool<GameObject>(
                CreateInstance,
                OnGet,
                OnRelease,
                prewarmCount
            );
        }

        private GameObject CreateInstance()
        {
            var instance = Object.Instantiate(_prefab, _root);
            instance.name = _prefab.name;

            var pooledObject = instance.GetComponent<PooledObject>();
            if (!pooledObject)
                pooledObject = instance.AddComponent<PooledObject>();

            pooledObject.Prefab = _prefab;
            instance.SetActive(false);

            return instance;
        }

        private void OnGet(GameObject instance)
        {
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnPoolGet();
            }
        }

        private void OnRelease(GameObject instance)
        {
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnPoolRelease();
            }

            instance.SetActive(false);
            instance.transform.SetParent(_root, false);
        }

        public GameObject Get(Transform parent = null)
        {
            var instance = _pool.Get();
            if (parent != null)
                instance.transform.SetParent(parent, false);

            instance.SetActive(true);
            return instance;
        }

        public void Release(GameObject instance)
        {
            _pool.Release(instance);
        }

        public void Clear()
        {
            // Destroy all instances under the pool root to avoid unreachable objects.
            if (_root != null)
            {
                for (var i = _root.childCount - 1; i >= 0; --i)
                {
                    Object.Destroy(_root.GetChild(i).gameObject);
                }    
            }

            _pool.Clear();
        }
    }
}
