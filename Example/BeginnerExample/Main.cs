using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

namespace ECSExample.Beginner {

    public class Main : ECSController<UnityStandardSystemRoot, EntityManager> {
        protected override void Initialize() {
            AddSystem<InitEntitiesSystem>();
            AddSystem<SumFloatSystem>();
            AddSystem<UpdateFloatSystem>();
        }
    }
}
