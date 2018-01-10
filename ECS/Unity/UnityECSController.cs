using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {

    public abstract class ECSController<TSystemRoot, TEntityManager> : ScriptBehaviour 
        where TSystemRoot : UnitySystemRoot<TEntityManager> 
        where TEntityManager : UnityEntityManager {

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
            //_entityManager.ProcessMessageQueue();

#if UNITY_EDITOR && ECS_DEBUG
            var gameObject = new GameObject("DebugEntityManager (" + _entityManager.GetType().Name + ")");
            var debugEntityManager = gameObject.AddComponent<VisualDebugging.DebugEntityManagerBehaviour>();
            debugEntityManager.Init(_entityManager);
#endif
        }

        protected abstract void Initialize();

        protected virtual void AddSceneEntitiesToSystem() {
            GameObjectEntity[] sceneEntities = FindObjectsOfType<GameObjectEntity>();
            for (int i = 0; i < sceneEntities.Length; i++) {
                if (sceneEntities[i].gameObject.activeInHierarchy) {
                    Entity entity = _entityManager.Instantiate(sceneEntities[i].gameObject);
                    sceneEntities[i].SetEntity(entity, _entityManager);
                }
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
