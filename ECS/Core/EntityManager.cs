using System;
using System.Collections.Generic;

namespace ECS {
    public class InvalidTComponentException : Exception {
        private const string TComponentIsIComponentTypeException = "TComponent cant be IComponent directly! Please cast to the specific Type";
        public InvalidTComponentException() : base(TComponentIsIComponentTypeException) { }
    }

    [InjectableDependency(LifeTime.Singleton)]
    public class EntityManager {
        public EntityManager() {

        }

        private readonly static Type _iComponentType = typeof(IComponent);

        private int _nextEntityId = 0;
        private readonly Stack<int> _freeEntityIds = new Stack<int>();
        
        protected readonly List<Entity> _entities = new List<Entity>();
        protected readonly Dictionary<Entity, HashSet<Type>> _entityComponents = new Dictionary<Entity, HashSet<Type>>();
        protected readonly Dictionary<Type, ComponentArray> _components = new Dictionary<Type, ComponentArray>(TypeComparer.typeComparer);
        protected readonly Dictionary<GroupMatcher, ComponentGroup> componentGroups = new Dictionary<GroupMatcher, ComponentGroup>();


        //public event Action<Entity> EntityCreated;
        //public event Action<Entity> EntityDestroyed;

        //[Obsolete("Garbage Collection intensive operation! Boxing of structure type components! Only for debugging purpose!!! Dont use this in operative System!")]
        //public event Action<Entity, Type> CompoentAdded;

        //[Obsolete("Garbage Collection intensive operation! Boxing of structure type components! Only for debugging purpose!!! Dont use this in operative System!")]
        //public event Action<Entity, Type> CompoentRemoved;

        //[Obsolete("Garbage Collection intensive operation! Boxing of structure type components! Only for debugging purpose!!! Dont use this in operative System!")]
        //public event Action<Entity, Type> CompoentChanged;

        //private List<Entity> _entityCreatedList = new List<Entity>();
        //private List<Entity> _entityDestroyedList = new List<Entity>();
        //private List<KeyValuePair<Entity, Type>> _compoentAddedList = new List<KeyValuePair<Entity, Type>>();
        //private List<KeyValuePair<Entity, Type>> _compoentRemovedList = new List<KeyValuePair<Entity, Type>>();
        //private List<KeyValuePair<Entity, Type>> _compoentChangedList = new List<KeyValuePair<Entity, Type>>();

        #region Events
        private EntityAddedEvent _entityAddedEvent = new EntityAddedEvent();
        private EntityRemovingEvent _entityRemovingEvent = new EntityRemovingEvent();
        private EntityRemovedEvent _entityRemovedEvent = new EntityRemovedEvent();

        private ComponentAddedToEntityEvent _componentAddedToEntityEvent = new ComponentAddedToEntityEvent();
        private ComponentRemovingFromEntityEvent _componentRemovingFromEntityEvent = new ComponentRemovingFromEntityEvent();
        private ComponentRemovedFromEntityEvent _componentRemovedFromEntityEvent = new ComponentRemovedFromEntityEvent();


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

        public void UnsubscribeOnComponentAddedToEntity(Entity entity, IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Unsubscribe(ref entity, eventListener);
        }

        public void SubscribeOnComponentRemovingFromEntity(Entity entity, IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Subscribe(ref entity, eventListener);
        }

        public void UnsubscribeOnComponentRemovingFromEntity(Entity entity, IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Unsubscribe(ref entity, eventListener);
        }

        public void SubscribeOnComponentRemovedFromEntity(Entity entity, IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Subscribe(ref entity, eventListener);
        }

        public void UnsubscribeOnComponentRemovedFromEntity(Entity entity, IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Unsubscribe(ref entity, eventListener);
        }



        public void SubscribeComponentChanged<TComponent>(Entity entity, IComponentChangedEventListener<TComponent> eventListener) where TComponent : IComponent {
            Type componentType = typeof(TComponent);
            ComponentArray componentArray;
            if(_components.TryGetValue(componentType, out componentArray)) {
                ((ComponentArray<TComponent>)componentArray).SubscripOnComponentChanged(entity, eventListener);
            }
        }

        public void UnsubscribeComponentChanged<TComponent>(Entity entity, IComponentChangedEventListener<TComponent> eventListener) where TComponent : IComponent {
            Type componentType = typeof(TComponent);
            ComponentArray componentArray;
            if (_components.TryGetValue(componentType, out componentArray)) {
                ((ComponentArray<TComponent>)componentArray).UnsubscripOnComponentChanged(entity, eventListener);
            }
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
            _componentRemovingFromEntityEvent.RemoveEntityFromEvent(entity);
            _componentRemovedFromEntityEvent.RemoveEntityFromEvent(entity);
            InspectComponentGroups(entity);
            _entityRemovedEvent.CallEvent(this, ref entity);

        }

        public void AddComponent<TComponent>(Entity entity, TComponent component) where TComponent : IComponent {
           
            if(typeof(TComponent) == _iComponentType) {
                throw new InvalidTComponentException();
            }

            Type componentType = typeof(TComponent);
            ComponentArray<TComponent> entityComponentMap = GetComponentMap<TComponent>(true);

            if (entityComponentMap.Add(entity, component)) {
                var type = component.GetType();
                _entityComponents[entity].Add(componentType);
                InspectComponentGroups(entity);
                _componentAddedToEntityEvent.CallEvent(this, ref entity, componentType);
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


        public void RemoveComponent<TComponent>(Entity entity, TComponent component) where TComponent : IComponent {
            if (typeof(TComponent) == _iComponentType) {
                throw new InvalidTComponentException();
            }
            Type componentType = typeof(TComponent);
            ComponentArray<TComponent> entityComponentMap = GetComponentMap<TComponent>(false);
            if (entityComponentMap != null && entityComponentMap.Contains(entity)) {
                _componentRemovingFromEntityEvent.CallEvent(this, ref entity, entityComponentMap.GetComponent(entity));
                entityComponentMap.Remove(entity);
                _entityComponents[entity].Remove(componentType);
                InspectComponentGroups(entity);
                _componentRemovedFromEntityEvent.CallEvent(this, ref entity, componentType);
            }
            //_compoentRemovedList.Add(new KeyValuePair<Entity, Type>(entity, componentType));
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
            if (!componentGroups.TryGetValue(groupMatcher, out componentGroup)) {
                componentGroup = new ComponentGroup(this, componentTypes);
                for (int i = 0; i < _entities.Count; i++) {
                    Entity entity = _entities[i];
                    componentGroup.Inspect(ref entity);
                }
                componentGroups.Add(groupMatcher, componentGroup);
            }
            return componentGroup;
        }


        private ComponentArray<TComponent> GetComponentMap<TComponent>(bool createIfNotFound) where TComponent : IComponent {
            ComponentArray entityComponentMap;
            Type componentType = typeof(TComponent);
            if (!_components.TryGetValue(componentType, out entityComponentMap) && createIfNotFound) {
                entityComponentMap = new ComponentArray<TComponent>();
                _components.Add(componentType, entityComponentMap);
            }
            return (ComponentArray<TComponent>)entityComponentMap;
        }

        private void InspectComponentGroups(Entity entity) {
            foreach (KeyValuePair<GroupMatcher, ComponentGroup> group in componentGroups) {
                group.Value.Inspect(ref entity);
            }
        }

        private void UpdateComponentGroups<TComponent>(ref Entity entity, ref TComponent component) where TComponent : struct, IComponent {
            foreach (KeyValuePair<GroupMatcher, ComponentGroup> group in componentGroups) {
                group.Value.Update(ref entity,ref component);
            }
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