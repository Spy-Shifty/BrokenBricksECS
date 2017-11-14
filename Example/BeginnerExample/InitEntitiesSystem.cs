using ECS;
using ECS.VisualDebugging;
using UnityEngine;

namespace ECSExample.Beginner {
    [DebugSystemGroup("Init")]
    class InitEntitiesSystem : ComponentSystem {

        private GameObject _gameObject;

        [InjectDependency]
        private UnityEntityManager entityManager;

        public override void OnStart() {
            _gameObject = new GameObject("Entities");
            for (int i = 0; i < 1000; i++) {
                Entity entity = entityManager.CreateEntity();

                GameObject gameObject = new GameObject("Entity-" + i);
                gameObject.transform.SetParent(_gameObject.transform);

                GameObjectEntity goEntity = gameObject.AddComponent<GameObjectEntity>();
                goEntity.SetEntity(entity, entityManager);
                entityManager.AddComponent(entity, new FloatComponent(1f));
            }
        }
    }
}
