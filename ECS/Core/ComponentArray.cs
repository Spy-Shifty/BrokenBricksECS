using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {
    public abstract class ComponentArray {
        public abstract bool Contains(Entity entity);
        public abstract bool Add(Entity entity, IComponent component);
        public abstract bool Remove(Entity entity);

        public abstract void TryGetValue(Entity entity, out IComponent component);
        public abstract bool Add(Entity entity, EntityManager entityManager);
        

    }

    public sealed class ComponentArray<TComponent> : ComponentArray, /*IComponentArray,*/ IEnumerable<TComponent> where TComponent : IComponent {

        private const int StartSize = 8;
        private const int ResizeFactor = 2;
        private readonly Dictionary<Entity, int> _componentsMap = new Dictionary<Entity, int>();


        private Entity[] _entities = new Entity[StartSize];
        private TComponent[] _components = new TComponent[StartSize];
        private int _size;

        public Entity GetEntity(int index) { return _entities[index]; }
        public TComponent this[int index] { get { return _components[index]; } }

        public int Length { get { return _size; } }

        

        #region Events   
        private EntityAddedEvent<TComponent> _entityAddedEvent = new EntityAddedEvent<TComponent>();
        private EntityRemovedEvent<TComponent> _entityRemovedEvent = new EntityRemovedEvent<TComponent>();
        private EntityComponentChangedEvent<TComponent> _entityComponentChangedEvent = new EntityComponentChangedEvent<TComponent>();
        public void SubscripOnEntityAdded(Entity entity, IEntityAddedEventListener<TComponent> eventListener) {
            _entityAddedEvent.Subscribe(ref entity, eventListener);
        }

        public void UnsubscripOnEntityAdded(Entity entity, IEntityAddedEventListener<TComponent> eventListener) {
            _entityAddedEvent.Unsubscribe(ref entity, eventListener);
        }
        
        public void SubscripOnEntityRemoved(Entity entity, IEntityRemovedEventListener<TComponent> eventListener) {
            _entityRemovedEvent.Subscribe(ref entity, eventListener);
        }

        public void UnsubscripOnEntityRemoved(Entity entity, IEntityRemovedEventListener<TComponent> eventListener) {
            _entityRemovedEvent.Unsubscribe(ref entity, eventListener);
        }
        

        public void SubscripOnComponentChanged(Entity entity, IComponentChangedEventListener<TComponent> eventListener) {
            _entityComponentChangedEvent.Subscribe(ref entity, eventListener);
        }

        public void UnsubscripOnComponentChanged(Entity entity, IComponentChangedEventListener<TComponent> eventListener) {
            _entityComponentChangedEvent.Unsubscribe(ref entity, eventListener);
        }
        
        #endregion Events

        public override bool Add(Entity entity, IComponent component) {
            return Add(entity, (TComponent)component);
        }

        public bool Add(Entity entity, TComponent component) {
            if (Contains(entity)) {
                return false;
            }

            if (_components.Length == _size) {
                var newEntityArray = new Entity[_entities.Length * ResizeFactor];
                var newComponentArray = new TComponent[_components.Length * ResizeFactor];
                Array.Copy(_entities, newEntityArray, _size);
                Array.Copy(_components, newComponentArray, _size);
                _entities = newEntityArray;
                _components = newComponentArray;
            }

            _entities[_size] = entity;
            _components[_size] = component;
            _componentsMap.Add(entity, _size);
            _size++;
            _entityAddedEvent.CallEvent(this, ref entity, ref component);

            return true;
        }

        public override bool Add(Entity entity, EntityManager entityManager) {
            if (Contains(entity)) {
                return false;
            }


            if (_components.Length == _size) {
                var newEntityArray = new Entity[_entities.Length * ResizeFactor];
                var newComponentArray = new TComponent[_components.Length * ResizeFactor];
                Array.Copy(_entities, newEntityArray, _size);
                Array.Copy(_components, newComponentArray, _size);
                _entities = newEntityArray;
                _components = newComponentArray;
            }

            TComponent component = entityManager.GetComponent<TComponent>(entity);
            _entities[_size] = entity;
            _components[_size] = component;
            _componentsMap.Add(entity, _size);
            _size++;
            _entityAddedEvent.CallEvent(this, ref entity, ref component);

            return true;
        }


        public override bool Remove(Entity entity) {
            int index;
            if (_componentsMap.TryGetValue(entity, out index)) {
                int lastId = _size - 1;
                TComponent component = _components[index];
                _entities[index] = _entities[lastId];
                _components[index] = _components[lastId];
                _componentsMap[_entities[index]] = index;
                _componentsMap.Remove(entity);
                _size--;

                int shrinkSize = _components.Length / (2 * ResizeFactor);
                if (_size <= shrinkSize && shrinkSize > StartSize) {
                    var newEntityArray = new Entity[_entities.Length / ResizeFactor];
                    var newComponentArray = new TComponent[_components.Length / ResizeFactor];
                    Array.Copy(_entities, newEntityArray, _size);
                    Array.Copy(_components, newComponentArray, _size);
                    _entities = newEntityArray;
                    _components = newComponentArray;
                }
                _entityRemovedEvent.CallEvent(this, entity, component);
                return true;
            }
            return false;
        }


        public void Update(Entity entity, TComponent component) {
            int index;
            if (_componentsMap.TryGetValue(entity, out index)) {
                _components[index] = component;
                _entityComponentChangedEvent.CallEvent(this, ref entity, ref component);
            }
        }



        public override bool Contains(Entity entity) {
            return _componentsMap.ContainsKey(entity);
        }

        public IEnumerator<TComponent> GetEnumerator() {
            int index = 0;
            while (index < _size) {
                yield return _components[index];
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            int index = 0;
            while (index < _size) {
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

        public TComponent GetComponent(Entity entity) {
            return _components[_componentsMap[entity]];
        }    
        
        public bool TryGetComponent(Entity entity, out TComponent component) {
            int index;
            if(_componentsMap.TryGetValue(entity, out index)) {
                component = _components[index];
                return true;
            }
            component = default(TComponent);
            return false;
        }
    }
}
