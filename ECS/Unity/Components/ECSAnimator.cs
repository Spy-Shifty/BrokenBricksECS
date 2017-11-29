using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    // classes are slower than structs 
    // its not recommendet to use this because it has an importend impact no the computation speed
    // use this class if you deal with unity Monobehaviour scripts like Transform
    [Serializable]
    public class AnimatorComponent : IComponent, ICloneable {
        public Animator animator;
        public object Clone() {
            return MemberwiseClone();
        }
    }

    // this wrapps the component tfor Scene & Prefab workflow
    [HideInInspector]
    [RequireComponent(typeof(Animator))]
    public class ECSAnimator : ComponentWrapper<AnimatorComponent> {
        public override void Initialize() {
            TypedComponent.animator = gameObject.GetComponent<Animator>();
        }
    }
}