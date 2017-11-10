using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {

    public abstract class ComponentWrapper : MonoBehaviour {
        public abstract IComponent Component{ get; set; }
        public abstract Type ComponentType { get; }
        public abstract void AddComponentToEntity(Entity entity, EntityManager entityManager);

        /// <summary>
        /// Use this for Initializeation for example Unity Components like Transforms
        /// </summary>
        public virtual void Initialize() { }
        public abstract void SetComponentToEntityManager();
        public abstract void GetComponentFromEntityManager();
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameObjectEntity))]
    public class ComponentWrapper<TComponent> : ComponentWrapper where TComponent : class, IComponent, ICloneable {
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

        public override void GetComponentFromEntityManager() {
            _component = GetComponent<GameObjectEntity>().GetComponentFromEntityManager<TComponent>();
        }

    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameObjectEntity))]
    public class ComponentDataWrapper<TComponent> : ComponentWrapper where TComponent : struct, IComponent {
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
        public override void GetComponentFromEntityManager() {
            _component = GetComponent<GameObjectEntity>().GetComponentFromEntityManager<TComponent>();
        }
    }
}
