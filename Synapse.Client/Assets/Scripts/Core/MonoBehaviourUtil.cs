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
        
        public MonoBehaviourUtil()
        {
            Instance = this;
        }

        public void RunOnMainThread(Action action)
        {
            _mainThreadActions.Enqueue(action);
        }

        private void Update()
        {
            OnUpdate?.Invoke();
            
            while (_mainThreadActions.Count > 0)
            {
                var act = _mainThreadActions.Dequeue();
                act?.Invoke();
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