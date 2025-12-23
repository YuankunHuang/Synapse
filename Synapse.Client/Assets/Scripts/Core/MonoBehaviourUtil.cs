using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Client.Core
{
    public class MonoBehaviourUtil : MonoBehaviour
    {
        public static MonoBehaviourUtil Instance { get; private set; }

        public static event Action OnUpdate;
        public static event Action OnLateUpdate;
        public static event Action OnFixedUpdate;

        #region Context Action
        private List<IMainThreadAction> _pendingActions = new List<IMainThreadAction>();
        private List<IMainThreadAction> _runningActions = new List<IMainThreadAction>();
        private readonly object _mainThreadActionLock = new object();
        
        private interface IMainThreadAction
        {
            void Execute();
            void ReturnToPool();
        }

        private class ContextAction : IMainThreadAction
        {
            public Action Action;
            
            private static readonly Stack<ContextAction> _pool = new Stack<ContextAction>();

            public static ContextAction Get(Action action)
            {
                ContextAction item;
                lock (_pool)
                {
                    item =  _pool.Count > 0 ? _pool.Pop() : new ContextAction();
                }
                item.Action = action;
                return item;
            }
            
            public void Execute()
            {
                Action?.Invoke();
            }

            public void ReturnToPool()
            {
                Action = null;
                lock (_pool)
                {
                    _pool.Push(this);
                }
            }
        }

        private class ContextAction<T> : IMainThreadAction
        {
            private Action<T> Action;
            private T State;
            
            private static readonly Stack<ContextAction<T>> _pool = new Stack<ContextAction<T>>();

            public static ContextAction<T> Get(Action<T> action, T state)
            {
                ContextAction<T> item;
                lock (_pool)
                {
                    item =  _pool.Count > 0 ? _pool.Pop() : new ContextAction<T>();
                }

                item.Action = action;
                item.State = state;
                return item;
            }

            public void Execute()
            {
                Action?.Invoke(State);
            }

            public void ReturnToPool()
            {
                Action = null;
                State = default;
                lock (_pool)
                {
                    _pool.Push(this);
                }
            }
        }
        
        #endregion
        
        public MonoBehaviourUtil()
        {
            Instance = this;
        }

        public void RunOnMainThread(Action action)
        {
            var contextAction = ContextAction.Get(action);
            lock (_mainThreadActionLock)
            {
                _pendingActions.Add(contextAction);
            }
        }

        public void RunOnMainThread<T>(Action<T> action, T state)
        {
            var contextAction = ContextAction<T>.Get(action, state);
            lock (_mainThreadActionLock)
            {
                _pendingActions.Add(contextAction);
            }
        }

        private void Update()
        {
            OnUpdate?.Invoke();

            lock (_mainThreadActionLock)
            {
                if (_pendingActions.Count > 0)
                {
                    (_pendingActions, _runningActions) = (_runningActions, _pendingActions);
                    _pendingActions.Clear();
                }
            }

            var count = _runningActions.Count;
            if (count > 0)
            {
                for (var i = 0; i < count; ++i)
                {
                    var task = _runningActions[i];
                    try
                    {
                        task.Execute();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    finally
                    {
                        task.ReturnToPool();
                    }
                }
                
                _runningActions.Clear();
            }
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }
    }    
}