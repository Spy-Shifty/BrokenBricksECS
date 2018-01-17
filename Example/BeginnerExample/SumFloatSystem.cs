using ECS;

namespace ECSExample.Beginner {
    [DebugSystemGroup("Sum")]
    class SumFloatSystem : ComponentSystem {

        [InjectTuple]
        ComponentArray<FloatComponent> floats;
        public override void OnUpdate() {
            float sum = 0;
            for (int i = 0; i < floats.Length; i++) {
                sum += floats[i].value;
            }
        }
    }
}
