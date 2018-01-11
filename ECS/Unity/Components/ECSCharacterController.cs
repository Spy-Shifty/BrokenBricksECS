using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a wrapper around Unity's CharacterController component
namespace ECS
{
    // classes are slower than structs 
    // its not recommended to use this because it has an important impact on the computation speed
    // use this class if you deal with unity Monobehaviour scripts like Transform
    [Serializable]
    public class CharacterControllerComponent : IComponent, ICloneable
    {
        public CharacterController characterController;

        public object Clone() {
            return MemberwiseClone();
        }
    }    
    
    // this wraps the component for Scene & Prefab workflow    
    [DisallowMultipleComponent]
    [HideInInspector]
    [RequireComponent(typeof(CharacterController))]
    public class ECSCharacterController : ComponentWrapper<CharacterControllerComponent>
    {
        private void Awake()
        {
            TypedComponent.characterController = gameObject.GetComponent<CharacterController>();
        }
    }
}
