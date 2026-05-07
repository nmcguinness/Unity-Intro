using UnityEngine;

namespace GD
{
    //struct is a value type, class is a reference type
    public struct MyPickup
    {
        public string name;
        public int value;
        public Vector3 position;
    }

    public class PickupGameEventListener : BaseGameEventListener<MyPickup>
    { }
}