namespace YuankunHuang.Unity.SimpleObjectPool
{
    /// <summary>
    /// Generic object pool contract.
    /// Defines the minimal behaviour of a reusable container.
    /// </summary>
    public interface IPool<T>
    {
        /// <summary>
        /// Get an instance from the pool.
        /// </summary>
        T Get();

        /// <summary>
        /// Return an instance back to the pool.
        /// </summary>
        void Release(T item);

        /// <summary>
        /// Pre-create instances and put them into the pool.
        /// </summary>
        /// <param name="count"></param>
        void Prewarm(int count);

        /// <summary>
        /// Clear pooled instances.
        /// </summary>
        void Clear();

        /// <summary>
        /// Number of inactive instances currently in the pool.
        /// </summary>
        int CountInactive { get; }
    }
}