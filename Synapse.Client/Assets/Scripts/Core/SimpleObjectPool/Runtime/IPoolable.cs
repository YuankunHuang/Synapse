namespace YuankunHuang.Unity.SimpleObjectPool
{
    /// <summary>
    /// Interface for pooled objects to receive lifecycle callbacks.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is retrieved from the pool.
        /// </summary>
        void OnPoolGet();

        /// <summary>
        /// Called when the object is returned to the pool.
        /// </summary>
        void OnPoolRelease();
    }
}