using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    // structs are way faster than class 
    // use this when ever you can it has an importen impact on the computation speedup
    [Serializable]
    public struct RotationSpeed : IComponent {
        public float speed;
	}	
	
	// this wrapps the component tfor Scene & Prefab workflow
	public class RotationSpeedComponent : ComponentDataWrapper<RotationSpeed>{}	
}
