using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
	// classes are slower than structs 
	// its not recommendet to use this because it has an importend impact no the computation speed
	// use this class if you deal with unity Monobehaviour scripts like Transform
	[Serializable]
	public class ColliderComponent : IComponent, ICloneable {
        public Collider[] collider;
		public object Clone() {
            return MemberwiseClone();
        }
	}	
	
	// this wrapps the component tfor Scene & Prefab workflow	
	[DisallowMultipleComponent]
	public class ECSColliders : ComponentWrapper<ColliderComponent>{
        private void Awake() {
            if(TypedComponent.collider.Length == 0) {
                TypedComponent.collider = GetComponents<Collider>();
            }
        }
    }	
}
