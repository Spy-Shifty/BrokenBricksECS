using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {

    [RequireComponent(typeof(GameObjectEntity))]
    public abstract class ComponentWrapper : MonoBehaviour {
        public abstract IComponent Component{ get; set; }
        public abstract Type ComponentType { get; }
        public abstract void AddComponentToEntity(Entity entity, EntityManager entityManager);

        /// <summary>
        /// Use this for Initializeation for example Unity Components like Transforms
        /// </summary>
        public virtual void Initialize() { }
        public abstract void SetEntityManager(Entity entity, EntityManager entityManager);
        public abstract void SetComponentToEntityManager();
        //public abstract void GetComponentFromEntityManager();
    }

    [DisallowMultipleComponent]
    public class ComponentWrapper<TComponent> : ComponentWrapper, IComponentChangedEventListener<TComponent> where TComponent : class, IComponent, ICloneable {
        [SerializeField]
        private TComponent _component;
        public TComponent TypedComponent { get { return _component; } }

        public override IComponent Component {
            get { return _component; }
            set { _component = (TComponent)value; }
        }

        public override Type ComponentType { get { return typeof(TComponent); } }

        public override void AddComponentToEntity(Entity entity, EntityManager entityManager) {
            entityManager.AddComponent(entity, (TComponent)_component.Clone());
        }

        public override void SetComponentToEntityManager() {
        }

        //public override void GetComponentFromEntityManager() {
        //    _component = GetComponent<GameObjectEntity>().GetComponentFromEntityManager<TComponent>();
        //}

        public override void SetEntityManager(Entity entity, EntityManager entityManager) {
            entityManager.SubscripeComponentChanged(entity, this);
            _component = entityManager.GetComponent<TComponent>(entity);
        }

        private void OnEnable() {
            GameObjectEntity gameObjectEntity = GetComponent<GameObjectEntity>();
            if (gameObjectEntity.IsInitialized) {
                gameObjectEntity.EntityManager.SubscripeComponentChanged(gameObjectEntity.Entity, this);
                _component = gameObjectEntity.GetComponentFromEntityManager<TComponent>();
            }
        }

        private void OnDisable() {
            GameObjectEntity gameObjectEntity = GetComponent<GameObjectEntity>();
            if (gameObjectEntity.IsInitialized) {
                gameObjectEntity.EntityManager.UnsubscripeComponentChanged(gameObjectEntity.Entity, this);
            }
        }
        
        public void OnComponentChanged(object sender, Entity entity, TComponent component) {
            _component = component;
        }

    }

    [DisallowMultipleComponent]
    public class ComponentDataWrapper<TComponent> : ComponentWrapper, IComponentChangedEventListener<TComponent> where TComponent : struct, IComponent {
        [SerializeField]
        private TComponent _component;

        public TComponent TypedComponent { get { return _component; } }

        public override IComponent Component {
            get { return _component; }
            set { _component = (TComponent)value; }
        }
        public override Type ComponentType { get { return typeof(TComponent); } }

        public override void AddComponentToEntity(Entity entity, EntityManager entityManager) {
            entityManager.AddComponent(entity, _component);
        }

        public override void SetComponentToEntityManager() {
            GetComponent<GameObjectEntity>().SetComponentToEntityManager(_component);
        }
        //public override void GetComponentFromEntityManager() {
        //    _component = GetComponent<GameObjectEntity>().GetComponentFromEntityManager<TComponent>();
        //}

        private void OnEnable() {
            GameObjectEntity gameObjectEntity = GetComponent<GameObjectEntity>();
            if (gameObjectEntity.IsInitialized) {
                gameObjectEntity.EntityManager.SubscripeComponentChanged(gameObjectEntity.Entity, this);
                _component = gameObjectEntity.GetComponentFromEntityManager<TComponent>();
            }
        }

        private void OnDisable() {
            GameObjectEntity gameObjectEntity = GetComponent<GameObjectEntity>();
            if (gameObjectEntity.IsInitialized) {
                gameObjectEntity.EntityManager.UnsubscripeComponentChanged(gameObjectEntity.Entity, this);
            }
        }

        public void OnComponentChanged(object sender, Entity entity, TComponent component) {
            _component = component;
        }

        public override void SetEntityManager(Entity entity, EntityManager entityManager) {
            entityManager.SubscripeComponentChanged(entity, this);
            _component = entityManager.GetComponent<TComponent>(entity);

        }
    }
}
