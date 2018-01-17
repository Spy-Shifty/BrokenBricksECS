using ECS;

namespace ECSExample.GameObjectExample {
    public class Main : ECSController<UnityStandardSystemRoot, UnityEntityManager> {
        // Use this for initialization
        protected override void Initialize() {
            AddSystem<RotateObjectSystem>();
        }
    }
}
