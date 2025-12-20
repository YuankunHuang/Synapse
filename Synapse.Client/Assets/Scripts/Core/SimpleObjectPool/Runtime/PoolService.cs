using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.SimpleObjectPool
{
    public static class PoolService
    {
        private static readonly Dictionary<GameObject, UnityPrefabPool> _pools = new Dictionary<GameObject, UnityPrefabPool>();
        private static Transform _root;

        private static Transform Root
        {
            get
            {
                if (_root != null)
                {
                    return _root;
                }

                var go = new GameObject("[SimpleObjectPool]");
                Object.DontDestroyOnLoad(go);
                _root = go.transform;
                return _root;
            }
        }

        private static UnityPrefabPool GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            if (_pools.TryGetValue(prefab, out var pool))
            {
                return pool;
            }

            var poolRoot = new GameObject($"{prefab.name}_Pool");
            poolRoot.transform.SetParent(Root, false);
            pool = new UnityPrefabPool(prefab, poolRoot.transform);
            _pools.Add(prefab, pool);
            return pool;
        }

        public static void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            var pool = GetOrCreatePool(prefab);
            for (var i = 0; i < count; ++i)
            {
                var instance = pool.Get();
                pool.Release(instance);
            }
        }

        public static GameObject Get(GameObject prefab, Transform parent = null)
        {
            var pool = GetOrCreatePool(prefab);
            if (pool == null)
            {
                return null;
            }
            return pool.Get(parent);
        }

        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var instance = Get(prefab, parent);
            if (instance == null)
                return null;

            var t = instance.transform;
            t.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public static T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            var instance = Get(prefab, position, rotation, parent);
            return instance != null ? instance.GetComponent<T>() : null;
        }

        public static void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            var pooledObject = instance.GetComponent<PooledObject>();
            if (pooledObject == null || pooledObject.Prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[SimpleObjectPool] Instance is not pooled. Destroying: {instance.name}");
#endif
                Object.Destroy(instance);
                return;
            }

            if (!_pools.TryGetValue(pooledObject.Prefab, out var pool))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[SimpleObjectPool] Pool not found for prefab key. Destroying: {instance.name}");
#endif
                Object.Destroy(instance);
                return;
            }

            pool.Release(instance);
        }

        public static void ClearAll()
        {
            foreach (var kv in _pools)
            {
                kv.Value.Clear();
            }
            _pools.Clear();
            
            if (_root != null)
            {
                for (var i = _root.childCount - 1; i >= 0; --i)
                {
                    Object.Destroy(_root.GetChild(i).gameObject);
                }
            }
        }
    }
}