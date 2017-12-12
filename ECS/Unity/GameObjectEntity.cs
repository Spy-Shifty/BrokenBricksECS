using System;
using System.Collections;
using System.Collections.Generic;

using ECS.Extensions;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

namespace ECS {
    public class GameObjectEntity : MonoBehaviour, IEntityRemovedEventListener, IComponentAddedToEntityEventListener, IComponentRemovedFromEntityEventListener {
        public int id;

        private Entity _entity;
        private UnityEntityManager _entityManager;
        private Dictionary<Type, ComponentWrapper> _componentWrapperMap = new Dictionary<Type, ComponentWrapper>();

        public bool IsInitialized { get; private set; }
        public Entity Entity { get { return _entity; } }
        public UnityEntityManager EntityManager { get { return _entityManager; } }

        public UnityEvent onInitialized;

        public void SetEntity(Entity entity, UnityEntityManager entityManager) {
            if (IsInitialized) {
                Debug.LogError(name + ": is already initialized by entity");
                return;
            }
            id = entity.Id;
            _componentWrapperMap.Clear();
            _entity = entity;
            _entityManager = entityManager;

            entityManager.SubscripeOnEntityRemoved(_entity, this);
            entityManager.SubscripeOnComponentAddedToEntity(entity, this);
            entityManager.SubscripeOnComponentRemovedFromEntity(entity, this);

            ComponentWrapper[] componentWrapper = GetComponents();
            for (int i = 0; i < componentWrapper.Length; i++) {
                _componentWrapperMap.Add(componentWrapper[i].ComponentType, componentWrapper[i]);
                componentWrapper[i].Initialize(entity, entityManager);
                //componentWrapper[i].Initialize();
            }
            IsInitialized = true;
            if (onInitialized != null) {
                onInitialized.Invoke();
            }
        }
        
        public void OnEntityRemoved(object sender, Entity entity) {
            Destroy(this);
        }

        public void OnComponentAddedToEntity(object sender, Entity entity, Type componentType) {
            if (_componentWrapperMap.ContainsKey(componentType)) {
                return;
            }
            Type componentWrapperType = componentType.Assembly.GetTypes()
            .Where(field => field.BaseType != null && field.BaseType.GetGenericArguments().Where(genericArgType => genericArgType == componentType).Any())
            .FirstOrDefault();

            if (componentWrapperType != null) {
                ComponentWrapper componentWrapper = (ComponentWrapper)gameObject.AddComponent(componentWrapperType);
                _componentWrapperMap.Add(componentType, componentWrapper);
            }
        }

        public void OnComponentRemovedFromEntity(object sender, Entity entity, Type componentType) {
            ComponentWrapper componentWrapper;
            if (_componentWrapperMap.TryGetValue(componentType, out componentWrapper)) {
                if (componentWrapper.enabled) {
                    Destroy(componentWrapper);
                    _componentWrapperMap.Remove(componentType);
                }
            }
        }

        private void OnDestroy() {
            if (_entityManager != null) {
                _entityManager.DestroyEntity(_entity);
            }
        }



        public ComponentWrapper[] GetComponents() {
            ComponentWrapper[] componentWrapper = GetComponents<ComponentWrapper>();
            return componentWrapper;
        }
                
        
        public void SetComponentToEntityManager<TComponent>(TComponent component) where TComponent : struct, IComponent {
            if (_entityManager != null) {
                _entityManager.SetComponent(_entity, component);
            }
        }

        public TComponent GetComponentFromEntityManager<TComponent>() where TComponent : IComponent {
            return _entityManager.GetComponent<TComponent>(_entity);
        }
    }
}