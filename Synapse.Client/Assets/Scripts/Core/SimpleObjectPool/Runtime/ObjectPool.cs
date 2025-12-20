using System;
using System.Collections.Generic;

namespace YuankunHuang.Unity.SimpleObjectPool
{
    /// <summary>
    /// Generic, lightweight object pool.
    /// </summary>
    public sealed class ObjectPool<T> : IPool<T> where T : class
    {
        private readonly Stack<T> _stack;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;

        public int CountInactive => _stack.Count;

        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            int initialCapacity = 0)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onRelease = onRelease;

            _stack = initialCapacity > 0
                ? new Stack<T>(initialCapacity)
                : new Stack<T>();

            if (initialCapacity > 0)
                Prewarm(initialCapacity);
        }

        public T Get()
        {
            T item = _stack.Count > 0
                ? _stack.Pop()
                : _createFunc();

            _onGet?.Invoke(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null)
                return;

            _onRelease?.Invoke(item);
            _stack.Push(item);
        }

        public void Prewarm(int count)
        {
            if (count <= 0)
                return;

            for (var i = 0; i < count; i++)
            {
                var item = _createFunc();
                _onRelease?.Invoke(item);
                _stack.Push(item);
            }
        }

        public void Clear()
        {
            _stack.Clear();
        }
    }
}