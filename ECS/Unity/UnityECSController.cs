using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {

    public abstract class ECSController<TSystemRoot, TEntityManager> : ScriptBehaviour 
        where TSystemRoot : SystemRoot<TEntityManager> 
        where TEntityManager : EntityManager {

        [InjectDependency]
        private TEntityManager _entityManager;

        [InjectDependency]
        private TSystemRoot _systemRoot;

        public void AddSystem(ComponentSystem componentSystem) {
            _systemRoot.AddSystem(componentSystem);
        }

        public void AddSystem<TComponentSystem>() where TComponentSystem : ComponentSystem {
            _systemRoot.AddSystem<TComponentSystem>();
        }

        protected sealed override void Awake() {
            base.Awake();
            Initialize();
            AddSceneEntitiesToSystem();
            _entityManager.ProcessMessageQueue();
        }

        protected abstract void Initialize();

        protected virtual void AddSceneEntitiesToSystem() {
            GameObjectEntity[] sceneEntities = FindObjectsOfType<GameObjectEntity>();
            for (int i = 0; i < sceneEntities.Length; i++) {
                Entity entity = _entityManager.Instantiate(sceneEntities[i].gameObject);
                sceneEntities[i].SetEntity(entity, _entityManager);
            }
        }

        // Dont change this lines of code!
        void Start() {
            _systemRoot.Start();
        }

        // Dont change this lines of code!
        void Update() {
            _systemRoot.Update();
        }

        // Dont change this lines of code!
        void FixedUpdate() {
            _systemRoot.FixedUpdate();
        }
    }
}
