using UnityEngine;
using UnityEngine.Events;

namespace GD
{
    public class BaseGameEventListener<T> : MonoBehaviour
    {
        [SerializeField]
        private string description;

        [SerializeField]
        [Tooltip("Specify the game event (scriptable object) which will raise the event")]
        private BaseGameEvent<T> Event;

        [SerializeField]
        private UnityEvent<T> Response;

        private void OnEnable() => Event.RegisterListener(this);

        private void OnDisable() => Event.UnregisterListener(this);

        public virtual void OnEventRaised(T data) => Response?.Invoke(data);
    }
}