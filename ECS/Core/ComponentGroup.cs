using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {
    public class ComponentGroup {
        private readonly GroupMatcher _groupMatcher;
        private readonly Dictionary<Type, ComponentArray> _components = new Dictionary<Type, ComponentArray>();

        private readonly EntityManager _entityManager;

        EntityAddedEvent entityAddedEvent = new EntityAddedEvent();
        EntityRemovingEvent entityRemovingEvent = new EntityRemovingEvent();
        EntityRemovedEvent entityRemovedEvent = new EntityRemovedEvent();
        //public event Action<Entity> OnEntityAdded;
        //public event Action<Entity> OnEntityRemoved;

        public void SubscribeOnEntityAdded(IEntityAddedEventListener eventListener) {
            entityAddedEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnEntityAdded(IEntityAddedEventListener eventListener) {
            entityAddedEvent.Unsubscribe(eventListener);
        }

        public void SubscribeOnEntityRemoving(IEntityRemovingEventListener eventListener) {
            entityRemovingEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnEntityRemoving(IEntityRemovingEventListener eventListener) {
            entityRemovingEvent.Unsubscribe(eventListener);
        }


        public void SubscribeOnEntityRemoved(IEntityRemovedEventListener eventListener) {
            entityRemovedEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnEntityRemoved(IEntityRemovedEventListener eventListener) {
            entityRemovedEvent.Unsubscribe(eventListener);
        }
        
        public ComponentGroup(EntityManager entityManager, params Type[] componentTypes) {
            _entityManager = entityManager;
            _groupMatcher = new GroupMatcher(componentTypes);

            foreach (Type type in componentTypes) {
                Type componentArrayType = typeof(ComponentArray<>);
                Type genericComponentArrayType = componentArrayType.MakeGenericType(type);
                _components.Add(type, (ComponentArray)Activator.CreateInstance(genericComponentArrayType));
            }
        }

        internal void Inspect(ref Entity entity) {
            foreach (var groupType in _groupMatcher) {
                if (!_entityManager.HasComponent(entity, groupType)) {
                    if (_components[groupType].Contains(entity)) {
                        RemoveEntity(entity);
                    }
                    return;
                }
            }
            AddEntity(entity);
        }

        internal void Update<TComponent>(ref Entity entity,ref TComponent component) where TComponent : IComponent{
            Type componentType = typeof(TComponent);
            ComponentArray componentArray;
            if (_components.TryGetValue(componentType, out componentArray)) {
                ((ComponentArray<TComponent>)componentArray).Update(entity, component);
            }
        }

        private void AddEntity(Entity entity) {
            bool added = false;
            foreach (var groupType in _groupMatcher) {
                if(_components[groupType].Add(entity, _entityManager)) {
                    added = true;
                }
            }
            if (added) {
                entityAddedEvent.CallEvent(this, ref entity);
            }
        }

        private void RemoveEntity(Entity entity) {
            entityRemovingEvent.CallEvent(this, ref entity);
            bool removed = false;
            foreach (ComponentArray componentArray in _components.Values) {
                if (componentArray.Remove(entity)) {
                    removed = true;
                }
            }
            if (removed) {

                entityRemovedEvent.CallEvent(this, ref entity);
            }
        }

        public ComponentArray<TComponent> GetComponent<TComponent>() where TComponent : IComponent {
            Type componentType = typeof(TComponent);
            return (ComponentArray<TComponent>)_components[componentType];
        }

        public ComponentArray GetComponent(Type componentType) {
            ComponentArray componentArray;
            if (_components.TryGetValue(componentType, out componentArray)) {
                return componentArray;
            }
            return null;
        }
    }

    public struct GroupMatcher : IEnumerable<Type> {
        private readonly Type[] _types;
        private readonly int _hash;

        public static int GenerateHash(params Type[] types) {
            unchecked {
                int hash = 5381;
                foreach (var type in types) {
                    hash = hash * 33 + type.GetHashCode();
                }
                return hash;
            }
        }

        public Type this[int index] { get { return _types[index]; } }
        public int Length { get { return _types.Length; } }

        public GroupMatcher(params Type[] types) {
            _types = types;
            _hash = GenerateHash(types);
        }
        public override bool Equals(object obj) {
            return (obj is GroupMatcher) && GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode() {
            return _hash;
        }

        public IEnumerator<Type> GetEnumerator() {
            int index = 0;
            while(index < _types.Length) {
                yield return _types[index];
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            int index = 0;
            while (index < _types.Length) {
                yield return _types[index];
                index++;
            }
        }
    }
}
