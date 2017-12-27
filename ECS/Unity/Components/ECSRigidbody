using System;
using UnityEngine;

namespace ECS {
    // classes are slower than structs 
    // its not recommendet to use this because it has an importend impact no the computation speed
    // use this class if you deal with unity Monobehaviour scripts like Transform
    [Serializable]
    public class RigidbodyComponent : IComponent, ICloneable {
        public Rigidbody rigidbody;

        public object Clone() {
            return MemberwiseClone();
        }
    }

    // this wrapps the component tfor Scene & Prefab workflow
    [HideInInspector]
    public class ECSRigidbody : ComponentWrapper<RigidbodyComponent> {
        private void Awake() {
            TypedComponent.rigidbody = gameObject.GetComponent<Rigidbody>();
        }
    }
}
