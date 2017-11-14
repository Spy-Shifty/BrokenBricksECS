using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

namespace ECSExample.Beginner {

    public class Main : ECSController<UnityStandardSystemRoot, UnityEntityManager> {
        protected override void Initialize() {
            AddSystem<InitEntitiesSystem>();
            AddSystem<SumFloatSystem>();
            AddSystem<UpdateFloatSystem>();
        }
    }
}
