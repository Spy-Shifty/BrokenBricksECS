using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    // classes are slower than structs 
    // its not recommendet to use this because it has an importend impact no the computation speed
    // use this class if you deal with unity Monobehaviour scripts like Transform
    [Serializable]
    public class TransformComponent : IComponent, ICloneable {
        public Transform transform;

        public object Clone() {
            return MemberwiseClone();
        }
    }

    // this wrapps the component tfor Scene & Prefab workflow
    [HideInInspector]
    public class ECSTransform : ComponentWrapper<TransformComponent> {
        private void Awake() {
            TypedComponent.transform = gameObject.transform;
        }
        //public override void Initialize() {
        //    TypedComponent.transform = gameObject.transform;
        //}
    }
}