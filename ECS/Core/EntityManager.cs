using System;
using System.Collections.Generic;

namespace ECS {
    public class InvalidTComponentException : Exception {
        private const string TComponentIsIComponentTypeException = "TComponent cant be IComponent directly! Please cast to the specific Type";
        public InvalidTComponentException() : base(TComponentIsIComponentTypeException) { }
    }

    [InjectableDependency(LifeTime.Singleton)]
    public class EntityManager : IComponentAddedToEntityEventListener,  IComponentRemovingFromEntityEventListener, IComponentRemovedFromEntityEventListener, IComponentChangedOfEntityEventListener {
        public EntityManager() {
            
        }

        private readonly static Type _iComponentType = typeof(IComponent);

        private int _nextEntityId = 0;
        private readonly Stack<int> _freeEntityIds = new Stack<int>();
        
        protected readonly List<Entity> _entities = new List<Entity>();
        protected readonly Dictionary<Entity, HashSet<Type>> _entityComponents = new Dictionary<Entity, HashSet<Type>>();
        protected readonly Dictionary<Type, ComponentArray> _components = new Dictionary<Type, ComponentArray>(TypeComparer.typeComparer);
        protected readonly Dictionary<GroupMatcher, ComponentGroup> _componentGroups = new Dictionary<GroupMatcher, ComponentGroup>();

        private struct DeletionPair {
            public readonly Entity entity;
            public readonly ComponentArray componentArray;
            private readonly int _hash;

            public DeletionPair(Entity entity, ComponentArray componentArray) {
                this.entity = entity;
                this.componentArray = componentArray;
                _hash = GenerateHash(entity.GetHashCode(), componentArray.GetHashCode());
            }

            public override int GetHashCode() {
                return _hash;
            }
            public static int GenerateHash(int hash1, int hash2) {
                unchecked {
                    int hash = 5381 * 33 + hash1 * 33 + hash2;
                    return hash;
                }
            }
        }
        private readonly HashSet<DeletionPair> _deletableComponents = new HashSet<DeletionPair>();
        private readonly HashSet<Entity> _destroyableEntities = new HashSet<Entity>();

        
        #region Events
        private EntityAddedEvent _entityAddedEvent = new EntityAddedEvent();
        private EntityRemovingEvent _entityRemovingEvent = new EntityRemovingEvent();
        private EntityRemovedEvent _entityRemovedEvent = new EntityRemovedEvent();

        private ComponentAddedToEntityEvent _componentAddedToEntityEvent = new ComponentAddedToEntityEvent();
        private ComponentRemovingFromEntityEvent _componentRemovingFromEntityEvent = new ComponentRemovingFromEntityEvent();
        private ComponentRemovedFromEntityEvent _componentRemovedFromEntityEvent = new ComponentRemovedFromEntityEvent();
        private ComponentChangedOfEntityEvent _componentChangedOfEntityEvent = new ComponentChangedOfEntityEvent();



        public void SubscribeOnEntityAdded(IEntityAddedEventListener eventListener) {
            _entityAddedEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnEntityAdded(IEntityAddedEventListener eventListener) {
            _entityAddedEvent.Unsubscribe(eventListener);
        }


        public void SubscribeOnEntityRemoving(Entity entity, IEntityRemovingEventListener eventListener) {
            _entityRemovingEvent.Subscribe(ref entity, eventListener);
        }

        public void UnsubscribeOnEntityRemoving(Entity entity, IEntityRemovingEventListener eventListener) {
            _entityRemovingEvent.Unsubscribe(ref entity, eventListener);
        }


        public void SubscribeOnEntityRemoved(Entity entity, IEntityRemovedEventListener eventListener) {
            _entityRemovedEvent.Subscribe(ref entity, eventListener);
        }

        public void UnsubscribeOnEntityRemoved(Entity entity, IEntityRemovedEventListener eventListener) {
            _entityRemovedEvent.Unsubscribe(ref entity, eventListener);
        }


        public void SubscribeOnComponentAddedToEntity(Entity entity, IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentAddedToEntity(IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Subscribe(eventListener);
        }
        public void SubscribeOnComponentAddedToEntity<TComponent>(Entity entity, IComponentAddedToEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).SubscribeOnComponentAddedToEntity(entity, eventListener);
        }
        public void SubscribeOnComponentAddedToEntity<TComponent>(IComponentAddedToEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).SubscribeOnComponentAddedToEntity(eventListener);
        }


        public void UnsubscribeOnComponentAddedToEntity(Entity entity, IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsubscribeOnComponentAddedToEntity(IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Unsubscribe(eventListener);
        }
        public void UnsubscribeOnComponentAddedToEntity<TComponent>(Entity entity, IComponentAddedToEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).UnsubscribeOnComponentAddedToEntity(entity, eventListener);
        }
        public void UnsubscribeOnComponentAddedToEntity<TComponent>(IComponentAddedToEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).UnsubscribeOnComponentAddedToEntity(eventListener);
        }


        public void SubscribeOnComponentRemovingFromEntity(Entity entity, IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentRemovingFromEntity(IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Subscribe(eventListener);
        }
        public void SubscribeOnComponentRemovingFromEntity<TComponent>(Entity entity, IComponentRemovingFromEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).SubscribeOnComponentRemovingFromEntity(entity, eventListener);
        }
        public void SubscribeOnComponentRemovingFromEntity<TComponent>(IComponentRemovingFromEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).SubscribeOnComponentRemovingFromEntity(eventListener);
        }


        public void UnsubscribeOnComponentRemovingFromEntity(Entity entity, IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsubscribeOnComponentRemovingFromEntity(IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Unsubscribe(eventListener);
        }
        public void UnsubscribeOnComponentRemovingFromEntity<TComponent>(Entity entity, IComponentRemovingFromEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).UnsubscribeOnComponentRemovingFromEntity(entity, eventListener);
        }
        public void UnsubscribeOnComponentRemovingFromEntity<TComponent>(IComponentRemovingFromEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).UnsubscribeOnComponentRemovingFromEntity(eventListener);
        }


        public void SubscribeOnComponentRemovedFromEntity(Entity entity, IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentRemovedFromEntity(IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Subscribe(eventListener);
        }
        public void SubscribeOnComponentRemovedFromEntity<TComponent>(Entity entity, IComponentRemovedFromEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).SubscribeOnComponentRemovedFromEntity(entity, eventListener);
        }
        public void SubscribeOnComponentRemovedFromEntity<TComponent>(IComponentRemovedFromEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).SubscribeOnComponentRemovedFromEntity(eventListener);
        }


        public void UnsubscribeOnComponentRemovedFromEntity(Entity entity, IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsubscribeOnComponentRemovedFromEntity(IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Unsubscribe(eventListener);
        }
        public void UnsubscribeOnComponentRemovedFromEntity<TComponent>(Entity entity, IComponentRemovedFromEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).UnsubscribeOnComponentRemovedFromEntity(entity, eventListener);
        }
        public void UnsubscribeOnComponentRemovedFromEntity<TComponent>(IComponentRemovedFromEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).UnsubscribeOnComponentRemovedFromEntity(eventListener);
        }


        public void SubscribeOnComponentChangedOfEntity(Entity entity, IComponentChangedOfEntityEventListener eventListener) {
            _componentChangedOfEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentChangedOfEntity(IComponentChangedOfEntityEventListener eventListener) {
            _componentChangedOfEntityEvent.Subscribe(eventListener);
        }
        public void SubscribeOnComponentChangedOfEntity<TComponent>(Entity entity, IComponentChangedOfEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).SubscribeOnComponentChangedOfEntity(entity, eventListener);
        }
        public void SubscribeOnComponentChangedOfEntity<TComponent>(IComponentChangedOfEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).SubscribeOnComponentChangedOfEntity(eventListener);
        }


        public void UnsbscribeOnComponentChangedOfEntity(Entity entity, IComponentChangedOfEntityEventListener eventListener) {
            _componentChangedOfEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsbscribeOnComponentChangedOfEntity(IComponentChangedOfEntityEventListener eventListener) {
            _componentChangedOfEntityEvent.Unsubscribe(eventListener);
        }
        public void UnsbscribeOnComponentChangedOfEntity<TComponent>(Entity entity, IComponentChangedOfEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).UnsbscribeOnComponentChangedOfEntity(entity, eventListener);
        }
        public void UnsbscribeOnComponentChangedOfEntity<TComponent>(IComponentChangedOfEntityEventListener<TComponent> eventListener) where TComponent : IComponent {
            GetComponentMap<TComponent>(true).UnsbscribeOnComponentChangedOfEntity(eventListener);
        }


        public void OnComponentAddedToEntity<TComponent>(object sender, Entity entity, TComponent component) {
            _componentAddedToEntityEvent.CallEvent(sender, ref entity, ref component);
        }
        public void OnComponentRemovingFromEntity<TComponent>(object sender, Entity entity, TComponent component) {
            _componentRemovingFromEntityEvent.CallEvent(sender, ref entity, ref component);
        }
        public void OnComponentRemovedFromEntity(object sender, Entity entity, Type componentType) {
            _componentRemovedFromEntityEvent.CallEvent(sender, ref entity, componentType);
        }
        public void OnComponentChangedOfEntity<TComponent>(object sender, Entity entity, TComponent component) {
            _componentChangedOfEntityEvent.CallEvent(sender, ref entity, ref component);
        }


        #endregion Events


        public virtual Entity CreateEntity() {
            Entity entity;
            if (_freeEntityIds.Count != 0) {
                entity = new Entity(_freeEntityIds.Pop());
            } else {
                entity = new Entity(_nextEntityId);
                _nextEntityId++;
            }
            _entityComponents.Add(entity, new HashSet<Type>());
            _entities.Add(entity);
            _entityAddedEvent.CallEvent(this, ref entity);
            //_entityCreatedList.Add(entity);           

            return entity;
        }

        public virtual void DestroyEntity(Entity entity) {
            if (_destroyableEntities.Contains(entity)) {
                return;
            }
            _destroyableEntities.Add(entity);
        }

        public virtual void DestroyEntityImmediate(Entity entity) {
            if (!_entities.Contains(entity)) {
                return;
            }

            _entityRemovingEvent.CallEvent(this, ref entity);
            foreach (Type componentType in _entityComponents[entity]) {
                _components[componentType].Remove(entity);
                //_compoentRemovedList.Add(new KeyValuePair<Entity, Type>(entity, componentType));
            }
            
            _entities.Remove(entity);
            _entityComponents.Remove(entity);
            //_entityDestroyedList.Add(entity);
            _freeEntityIds.Push(entity.Id);

            _componentAddedToEntityEvent.RemoveEntityFromEvent(ref entity);
            _componentRemovingFromEntityEvent.RemoveEntityFromEvent(ref entity);
            _componentRemovedFromEntityEvent.RemoveEntityFromEvent(ref entity);
            _componentChangedOfEntityEvent.RemoveEntityFromEvent(ref entity);

            InspectComponentGroups(entity);
            _entityRemovedEvent.CallEvent(this, ref entity);
        }

        public void AddComponent<TComponent>(Entity entity, TComponent component) where TComponent : IComponent {
           
            if(typeof(TComponent) == _iComponentType) {
                throw new InvalidTComponentException();
            }

            Type componentType = typeof(TComponent);
            ComponentArray<TComponent> entityComponentMap = GetComponentMap<TComponent>(true);

            HashSet<Type> componentTypes= _entityComponents[entity];
            if (!componentTypes.Contains(componentType)) {
                _entityComponents[entity].Add(componentType);
                entityComponentMap.Add(entity, component);
                InspectComponentGroups(entity);
            }
        }

        public void SetComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct, IComponent {
            Type componentType = typeof(TComponent);
            if (componentType == _iComponentType) {
                throw new InvalidTComponentException();
            }
            ComponentArray<TComponent> entityComponentMap = GetComponentMap<TComponent>(false);
            entityComponentMap.Update(entity, component);
            //_compoentChangedList.Add(new KeyValuePair<Entity, Type>(entity, componentType));
            UpdateComponentGroups(ref entity, ref component);            
        }

        public void RemoveComponent<TComponent>(Entity entity) where TComponent : IComponent {
            var deletionPair = new DeletionPair(entity, GetComponentMap<TComponent>(false));
            if (!_deletableComponents.Contains(deletionPair)) {
                _deletableComponents.Add(deletionPair);
            }
        }

        public void RemoveComponentImmediate<TComponent>(Entity entity) where TComponent : IComponent {
            if (typeof(TComponent) == _iComponentType) {
                throw new InvalidTComponentException();               
            }
            Type componentType = typeof(TComponent);
            ComponentArray<TComponent> entityComponentMap = GetComponentMap<TComponent>(false);
            if (entityComponentMap != null && entityComponentMap.Contains(entity)) {
                _entityComponents[entity].Remove(componentType);
                entityComponentMap.Remove(entity);
                InspectComponentGroups(entity);
            }
        }

        public bool HasComponent(Entity entity, Type componentType) {
            HashSet<Type> componentTypes;
            if (!_entityComponents.TryGetValue(entity, out componentTypes)) {
                return false;
            }
            return componentTypes.Contains(componentType);
        }

        public IComponent GetComponent(Entity entity, Type componentType) {
            ComponentArray componentEntityMap;
            if (_components.TryGetValue(componentType, out componentEntityMap)) {
                IComponent component;
                componentEntityMap.TryGetValue(entity, out component);
                return component;
            }
            return null;
        }

        public TComponent GetComponent<TComponent>(Entity entity) where TComponent : IComponent {
            if (typeof(TComponent) == _iComponentType) {
                throw new InvalidTComponentException();
            }
            Type componentType = typeof(TComponent);

            ComponentArray componentEntityMap;
            if (_components.TryGetValue(componentType, out componentEntityMap)) {
                return ((ComponentArray<TComponent>)componentEntityMap).GetComponent(entity);
            }
            throw new Exception("Entity doesn't have component " + componentType);
        }

        public bool TryGetComponent<TComponent>(Entity entity, out TComponent component) where TComponent : IComponent {
            if (typeof(TComponent) == _iComponentType) {
                throw new InvalidTComponentException();
            }
            Type componentType = typeof(TComponent);

            ComponentArray componentEntityMap;
            if (_components.TryGetValue(componentType, out componentEntityMap)) {
                return ((ComponentArray<TComponent>)componentEntityMap).TryGetComponent(entity, out component);
            }
            component = default(TComponent);
            return false;
        }

        public bool HasComponent<TComponent>(Entity entity) where TComponent : IComponent {
            if (typeof(TComponent) == _iComponentType) {
                throw new InvalidTComponentException();
            }
            Type componentType = typeof(TComponent);

            ComponentArray componentEntityMap;
            if (_components.TryGetValue(componentType, out componentEntityMap)) {
                 return componentEntityMap.Contains(entity);
            }
            return false;
        }

        public IEnumerable<IComponent> GetComponents(Entity entity) {
            IEnumerable<Type> componentTypes = _entityComponents[entity];
            List<IComponent> componentList = new List<IComponent>();

            foreach (Type componentType in componentTypes) {
                IComponent component = GetComponent(entity, componentType);
                if (component != null) {
                    componentList.Add(component);
                }
            }

            return componentList;
        }

        public ComponentGroup GetComponentGroup(params Type[] componentTypes) {
            if (componentTypes.Length == 0) {
                return null;
            }

            GroupMatcher groupMatcher = new GroupMatcher(componentTypes);
            ComponentGroup componentGroup;
            if (!_componentGroups.TryGetValue(groupMatcher, out componentGroup)) {
                componentGroup = new ComponentGroup(this, componentTypes);
                for (int i = 0; i < _entities.Count; i++) {
                    Entity entity = _entities[i];
                    componentGroup.Inspect(ref entity);
                }
                _componentGroups.Add(groupMatcher, componentGroup);
            }
            return componentGroup;
        }


        private ComponentArray<TComponent> GetComponentMap<TComponent>(bool createIfNotFound) where TComponent : IComponent {
            ComponentArray entityComponentMap;
            Type componentType = typeof(TComponent);
            if (!_components.TryGetValue(componentType, out entityComponentMap) && createIfNotFound) {
                ComponentArray<TComponent>  typedEntityComponentMap = new ComponentArray<TComponent>();
                typedEntityComponentMap.SubscribeOnComponentAddedToEntity(this);
                typedEntityComponentMap.SubscribeOnComponentRemovingFromEntity(this);
                typedEntityComponentMap.SubscribeOnComponentRemovedFromEntity(this);
                typedEntityComponentMap.SubscribeOnComponentChangedOfEntity(this);

                entityComponentMap = typedEntityComponentMap;
                _entityRemovedEvent.Subscribe(typedEntityComponentMap);

                _components.Add(componentType, typedEntityComponentMap);
            }
            return (ComponentArray<TComponent>)entityComponentMap;
        }



        private void InspectComponentGroups(Entity entity) {
            foreach (KeyValuePair<GroupMatcher, ComponentGroup> group in _componentGroups) {
                group.Value.Inspect(ref entity);
            }
        }

        private void UpdateComponentGroups<TComponent>(ref Entity entity, ref TComponent component) where TComponent : struct, IComponent {
            foreach (KeyValuePair<GroupMatcher, ComponentGroup> group in _componentGroups) {
                group.Value.Update(ref entity,ref component);
            }
        }

        internal void HandleDeletion() {
            foreach (DeletionPair item in _deletableComponents) {
                
                Entity entity = item.entity;
                ComponentArray entityComponentMap = item.componentArray;
                if (entityComponentMap != null && entityComponentMap.Contains(entity)) {
                    entityComponentMap.Remove(entity);
                    _entityComponents[entity].Remove(entityComponentMap.ComponentType);
                    InspectComponentGroups(entity);
                }
            }
            _deletableComponents.Clear();

            foreach (Entity entity in _destroyableEntities) {
                DestroyEntityImmediate(entity);
            }
            _destroyableEntities.Clear();
        }
    }

    class TypeComparer : IEqualityComparer<Type> {
        public static readonly TypeComparer typeComparer = new TypeComparer();
        public bool Equals(Type x, Type y) {
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(Type obj) {
            return obj.GetHashCode();
        }
    }


}
