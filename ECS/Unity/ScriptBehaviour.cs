using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    public class ScriptBehaviour : MonoBehaviour {

        protected virtual void Awake() {
            InjectionManager.ResolveDependency(this);
        }
    }
}
