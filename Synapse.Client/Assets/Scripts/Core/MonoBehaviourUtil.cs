using System;
using System.Collections;
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
        
        private Queue<Action> _mainThreadActions = new Queue<Action>();
        private static readonly object _queueLock = new object();
        
        public MonoBehaviourUtil()
        {
            Instance = this;
        }

        public void RunOnMainThread(Action action)
        {
            lock (_queueLock)
            {
                _mainThreadActions.Enqueue(action);    
            }
        }

        private void Update()
        {
            OnUpdate?.Invoke();

            if (_mainThreadActions.Count > 0)
            {
                Action[] actionsToRun = null;
                lock (_queueLock)
                {
                    if (_mainThreadActions.Count > 0)
                    {
                        actionsToRun = _mainThreadActions.ToArray();
                        _mainThreadActions.Clear();
                    }
                }    
                
                // run outside of lock block to avoid dead lock
                if (actionsToRun != null)
                {
                    foreach (var act in actionsToRun)
                    {
                        act?.Invoke();
                    }
                }
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