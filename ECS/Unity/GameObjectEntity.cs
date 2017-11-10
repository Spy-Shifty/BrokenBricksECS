using System;
using System.Collections;
using System.Collections.Generic;
using ECS;
using ECS.Extensions;
using UnityEngine;
using System.Linq;

public class GameObjectEntity : MonoBehaviour {
    public int id;

    private Entity _entity;
    private EntityManager _entityManager;
    private Dictionary<Type, ComponentWrapper> _componentWrapperMap = new Dictionary<Type, ComponentWrapper>();

    public bool IsInitialized { get; private set; }

    public void SetEntity(Entity entity, EntityManager entityManager) {
        if (IsInitialized) {
            Debug.LogError(name + ": is already initialized by entity");
            return;
        }
        id = entity.Id;
        _componentWrapperMap.Clear();
        _entity = entity;
        _entityManager = entityManager;

        entityManager.EntityDestroyed += EntityManager_EntityDestroyed;

        _entityManager.CompoentChanged += EntityManager_ComponentChanged;
        _entityManager.CompoentAdded += EntityManager_ComponentAdded;
        _entityManager.CompoentRemoved += EntityManager_ComponentRemoved;

        ComponentWrapper[] componentWrapper = GetComponents();
        for (int i = 0; i < componentWrapper.Length; i++) {
            _componentWrapperMap.Add(componentWrapper[i].ComponentType, componentWrapper[i]);
            componentWrapper[i].GetComponentFromEntityManager();
            componentWrapper[i].Initialize();
        }

        IsInitialized = true;
    }

    private void OnDestroy() {
        if (_entityManager != null) {
            _entityManager.DestroyEntity(_entity);
        }
    }

    public ComponentWrapper[] GetComponents() {
        ComponentWrapper[] componentWrapper = GetComponentsInChildren<ComponentWrapper>();
        return componentWrapper;
    }


    private void EntityManager_EntityDestroyed(Entity entity) {
        if (!_entity.Equals(entity)) {
            return;
        }
        Destroy(this);
    }

    public void SetComponentToEntityManager<TComponent>(TComponent component) where TComponent : struct, IComponent {
        if (_entityManager != null) {
            _entityManager.SetComponent(_entity, component);
        }
    }

    public TComponent GetComponentFromEntityManager<TComponent>() where TComponent : IComponent {
        return _entityManager.GetComponent<TComponent>(_entity);
    }
    

    //This will be called for Components of struct type
    private void EntityManager_ComponentChanged(Entity entity, Type componentType) {
        if (!_entity.Equals(entity)) {
            return;
        }

        ComponentWrapper componentWrapper;
        if (_componentWrapperMap.TryGetValue(componentType, out componentWrapper)) {
            componentWrapper.GetComponentFromEntityManager();
        }
    }

    private void EntityManager_ComponentRemoved(Entity entity, Type componentType) {
        if (!_entity.Equals(entity)) {
            return;
        }       

        ComponentWrapper componentWrapper;
        if (_componentWrapperMap.TryGetValue(componentType, out componentWrapper)) {
            Destroy(componentWrapper);
            _componentWrapperMap.Remove(componentType);
        }
    }

    private void EntityManager_ComponentAdded(Entity entity, Type componentType) {
        if (!_entity.Equals(entity) || _componentWrapperMap.ContainsKey(componentType)) {
            return;
        }

        Type componentWrapperType = componentType.Assembly.GetTypes()
        .Where(field => field.BaseType != null && field.BaseType.GetGenericArguments().Where(genericArgType => genericArgType == componentType).Any())
        .FirstOrDefault();

        if (componentWrapperType != null) {
            ComponentWrapper componentWrapper = (ComponentWrapper)gameObject.AddComponent(componentWrapperType);
            _componentWrapperMap.Add(componentType, componentWrapper);
            componentWrapper.GetComponentFromEntityManager();
        }
    }
}
