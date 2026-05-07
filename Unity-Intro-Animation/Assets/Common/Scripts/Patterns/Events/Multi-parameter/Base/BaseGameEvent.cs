using System.Collections.Generic;
using UnityEngine;

namespace GD
{
    public abstract class BaseGameEvent<T> : ScriptableGameObject
    {
        private List<BaseGameEventListener<T>> listeners = new List<BaseGameEventListener<T>>();

        [ContextMenu("Raise Event")]
        public virtual void Raise(T data)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(data);
            }
        }

        public void RegisterListener(BaseGameEventListener<T> listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void UnregisterListener(BaseGameEventListener<T> listener)
        {
            if (listeners.Contains(listener))
                listeners.Remove(listener);
        }
    }
}