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

        public event Action<Entity> OnEntityAdded;
        public event Action<Entity> OnEntityRemoved;

        public ComponentGroup(EntityManager entityManager, params Type[] componentTypes) {
            _entityManager = entityManager;
            _groupMatcher = new GroupMatcher(componentTypes);

            foreach (Type type in componentTypes) {
                Type componentArrayType = typeof(ComponentArray<>);
                Type genericComponentArrayType = componentArrayType.MakeGenericType(type);
                _components.Add(type, (ComponentArray)Activator.CreateInstance(genericComponentArrayType));
            }
        }

        public void Inspect(Entity entity) {
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

        public void Update<TComponent>(Entity entity, TComponent component) where TComponent : IComponent{
            Type componentType = typeof(TComponent);
            ComponentArray componentArray;
            if (_components.TryGetValue(componentType, out componentArray)) {
                ((ComponentArray<TComponent>)componentArray).Update(entity, component);
            }
        }

        private void AddEntity(Entity entity) {
            foreach (var groupType in _groupMatcher) {
                _components[groupType].Add(entity, _entityManager);
            }

            if (OnEntityAdded != null) {
                OnEntityAdded.Invoke(entity);
            }
        }

        private void RemoveEntity(Entity entity) {
            foreach (ComponentArray componentArray in _components.Values) {
                componentArray.Remove(entity);
            }

            if (OnEntityRemoved != null) {
                OnEntityRemoved.Invoke(entity);
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

    public abstract class ComponentArray {
        public abstract bool Contains(Entity entity);
        public abstract void Add(Entity entity, IComponent component);
        public abstract void Remove(Entity entity);

        public abstract void TryGetValue(Entity entity, out IComponent component);
        public abstract void Add(Entity entity, EntityManager entityManager);
    }

    public sealed class ComponentArray<TComponent> : ComponentArray, /*IComponentArray,*/ IEnumerable<TComponent> where TComponent : IComponent {

        private const int StartSize = 8;
        private const int ResizeFactor = 2;
        private readonly Dictionary<Entity, int> _componentsMap = new Dictionary<Entity, int>();


        private Entity[] _entities = new Entity[StartSize];
        private TComponent[] _components = new TComponent[StartSize];
        private int size;

        public Entity GetEntity(int index) {  return _entities[index];  }
        public TComponent this[int index] {  get{ return _components[index]; }  }

        public int Length { get { return size; } }

        public override void Add(Entity entity, IComponent component) {
            Add(entity, (TComponent)component);
        }

        public void Add(Entity entity, TComponent component) {
            if (Contains(entity)) {
                return;
            }

            if(_components.Length == size) {
                var newEntityArray = new Entity[_entities.Length * ResizeFactor];
                var newComponentArray = new TComponent[_components.Length * ResizeFactor];
                Array.Copy(_entities, newEntityArray, size);
                Array.Copy(_components, newComponentArray, size);
                _entities = newEntityArray;
                _components = newComponentArray;
            }

            _entities[size] = entity;
            _components[size] = component;
            _componentsMap.Add(entity, size);
            size++;
        }

        public override void Remove(Entity entity) {
            int index;
            if (_componentsMap.TryGetValue(entity, out index)) {
                int lastId = size-1;
                _entities[index] = _entities[lastId];
                _components[index] = _components[lastId];
                _componentsMap[_entities[index]] = index;
                _componentsMap.Remove(entity);
                size--;

                int shrinkSize = _components.Length / (2 * ResizeFactor);
                if (size <= shrinkSize && shrinkSize > StartSize) {
                    var newEntityArray = new Entity[_entities.Length / ResizeFactor];
                    var newComponentArray = new TComponent[_components.Length / ResizeFactor];
                    Array.Copy(_entities, newEntityArray, size);
                    Array.Copy(_components, newComponentArray, size);
                    _entities = newEntityArray;
                    _components = newComponentArray;
                }
            }            
        }

        public override bool Contains(Entity entity) {
            return _componentsMap.ContainsKey(entity);
        }

        public IEnumerator<TComponent> GetEnumerator() {
            int index = 0;
            while(index < size) {
                yield return _components[index];
                index++;
            }
        }

        public void Update(Entity entity, TComponent component) {
            int index;
            if (_componentsMap.TryGetValue(entity, out index)) {
                _components[index] = component;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            int index = 0;
            while (index < size) {
                yield return _components[index];
                index++;
            }
        }

        public override void TryGetValue(Entity entity, out IComponent component) {
            int index;
            if (_componentsMap.TryGetValue(entity, out index)) {
                component = _components[index];
            } else {
                component = null;
            }            
        }

        public override void Add(Entity entity, EntityManager entityManager) {
            if (Contains(entity)) {
                return;
            }

            
            if (_components.Length == size) {
                var newEntityArray = new Entity[_entities.Length * ResizeFactor];
                var newComponentArray = new TComponent[_components.Length * ResizeFactor];
                Array.Copy(_entities, newEntityArray, size);
                Array.Copy(_components, newComponentArray, size);
                _entities = newEntityArray;
                _components = newComponentArray;
            }

            _entities[size] = entity;
            _components[size] = entityManager.GetComponent<TComponent>(entity);
            _componentsMap.Add(entity, size);
            size++;
        }

        public TComponent GetComponent(Entity entity) {
            return _components[_componentsMap[entity]];
        }
    }    
}
