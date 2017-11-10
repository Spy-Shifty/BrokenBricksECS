using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

namespace ECSExample.GameObjectExample {
    public class Main : ECSController<UnityStandardSystemRoot, EntityManager> {
        // Use this for initialization
        protected override void Initialize() {
            AddSystem<RotateObjectSystem>();
        }
    }
}
