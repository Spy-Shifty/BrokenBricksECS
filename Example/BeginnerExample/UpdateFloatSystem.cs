using ECS;

namespace ECSExample.Beginner {
    [DebugSystemGroup("Update")]
    class UpdateFloatSystem : ComponentSystem {

        [InjectDependency]
        private EntityManager entityManager;
        
        [InjectTuple]
        private ComponentArray<FloatComponent> floats;
        public override void OnUpdate() {
            float sum = 0;
            for (int i = 0; i < floats.Length; i++) {
                entityManager.SetComponent(floats.GetEntity(i), new FloatComponent(floats[i].value + 1));
            }
        }
    }
}
