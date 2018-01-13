using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {
    public abstract class ComponentArray : IEntityRemovedEventListener {
        public abstract bool Contains(Entity entity);
        public abstract bool Add(Entity entity, IComponent component);
        public abstract bool Remove(Entity entity);

        public abstract void TryGetValue(Entity entity, out IComponent component);
        public abstract bool Add(Entity entity, EntityManager entityManager);

        public abstract Type ComponentType { get; }


        #region Events

        protected ComponentAddedToEntityEvent _componentAddedToEntityEvent = new ComponentAddedToEntityEvent();
        protected ComponentRemovingFromEntityEvent _componentRemovingFromEntityEvent = new ComponentRemovingFromEntityEvent();
        protected ComponentRemovedFromEntityEvent _componentRemovedFromEntityEvent = new ComponentRemovedFromEntityEvent();
        protected ComponentChangedOfEntityEvent _componentChangedOfEntityEvent = new ComponentChangedOfEntityEvent();


        public void SubscribeOnComponentAddedToEntity(Entity entity, IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentAddedToEntity(IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnComponentAddedToEntity(Entity entity, IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsubscribeOnComponentAddedToEntity(IComponentAddedToEntityEventListener eventListener) {
            _componentAddedToEntityEvent.Unsubscribe(eventListener);
        }

        public void SubscribeOnComponentRemovingFromEntity(Entity entity, IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentRemovingFromEntity(IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnComponentRemovingFromEntity(Entity entity, IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsubscribeOnComponentRemovingFromEntity(IComponentRemovingFromEntityEventListener eventListener) {
            _componentRemovingFromEntityEvent.Unsubscribe(eventListener);
        }

        public void SubscribeOnComponentRemovedFromEntity(Entity entity, IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentRemovedFromEntity(IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnComponentRemovedFromEntity(Entity entity, IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Unsubscribe(ref entity, eventListener);
        }

        public void UnsubscribeOnComponentRemovedFromEntity(IComponentRemovedFromEntityEventListener eventListener) {
            _componentRemovedFromEntityEvent.Unsubscribe(eventListener);
        }

        public void SubscribeOnComponentChangedOfEntity(Entity entity, IComponentChangedOfEntityEventListener eventListener) {
            _componentChangedOfEntityEvent.Subscribe(ref entity, eventListener);
        }

        public void SubscribeOnComponentChangedOfEntity(IComponentChangedOfEntityEventListener eventListener) {
            _componentChangedOfEntityEvent.Subscribe(eventListener);
        }

        public void UnsbscribeOnComponentChangedOfEntity(Entity entity, IComponentChangedOfEntityEventListener eventListener) {
            _componentChangedOfEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsbscribeOnComponentChangedOfEntity(IComponentChangedOfEntityEventListener eventListener) {
            _componentChangedOfEntityEvent.Unsubscribe(eventListener);
        }

        public virtual void OnEntityRemoved(object sender, Entity entity) {
            _componentAddedToEntityEvent.RemoveEntityFromEvent(ref entity);
            _componentRemovingFromEntityEvent.RemoveEntityFromEvent(ref entity);
            _componentRemovedFromEntityEvent.RemoveEntityFromEvent(ref entity);
            _componentChangedOfEntityEvent.RemoveEntityFromEvent(ref entity);
        }

        #endregion

    }

    public sealed class ComponentArray<TComponent> : ComponentArray, /*IComponentArray,*/ IEnumerable<TComponent> where TComponent : IComponent {
        private static Type componentType = typeof(TComponent);
        private const int StartSize = 8;
        private const int ResizeFactor = 2;
        private readonly Dictionary<Entity, int> _componentsMap = new Dictionary<Entity, int>();


        private Entity[] _entities = new Entity[StartSize];
        private TComponent[] _components = new TComponent[StartSize];
        private int _size;

        #region Events

        private ComponentAddedToEntityEvent<TComponent> _typedComponentAddedToEntityEvent = new ComponentAddedToEntityEvent<TComponent>();
        private ComponentRemovingFromEntityEvent<TComponent> _typedComponentRemovingFromEntityEvent = new ComponentRemovingFromEntityEvent<TComponent>();
        private ComponentRemovedFromEntityEvent<TComponent> _typedComponentRemovedFromEntityEvent = new ComponentRemovedFromEntityEvent<TComponent>();
        private ComponentChangedOfEntityEvent<TComponent> _typedComponentChangedOfEntityEvent = new ComponentChangedOfEntityEvent<TComponent>();


        public void SubscribeOnComponentAddedToEntity(Entity entity, IComponentAddedToEntityEventListener<TComponent> eventListener) {
            _typedComponentAddedToEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentAddedToEntity(IComponentAddedToEntityEventListener<TComponent> eventListener) {
            _typedComponentAddedToEntityEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnComponentAddedToEntity(Entity entity, IComponentAddedToEntityEventListener<TComponent> eventListener) {
            _typedComponentAddedToEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsubscribeOnComponentAddedToEntity(IComponentAddedToEntityEventListener<TComponent> eventListener) {
            _typedComponentAddedToEntityEvent.Unsubscribe(eventListener);
        }

        public void SubscribeOnComponentRemovingFromEntity(Entity entity, IComponentRemovingFromEntityEventListener<TComponent> eventListener) {
            _typedComponentRemovingFromEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentRemovingFromEntity(IComponentRemovingFromEntityEventListener<TComponent> eventListener) {
            _typedComponentRemovingFromEntityEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnComponentRemovingFromEntity(Entity entity, IComponentRemovingFromEntityEventListener<TComponent> eventListener) {
            _typedComponentRemovingFromEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsubscribeOnComponentRemovingFromEntity(IComponentRemovingFromEntityEventListener<TComponent> eventListener) {
            _typedComponentRemovingFromEntityEvent.Unsubscribe(eventListener);
        }

        public void SubscribeOnComponentRemovedFromEntity(Entity entity, IComponentRemovedFromEntityEventListener<TComponent> eventListener) {
            _typedComponentRemovedFromEntityEvent.Subscribe(ref entity, eventListener);
        }
        public void SubscribeOnComponentRemovedFromEntity(IComponentRemovedFromEntityEventListener<TComponent> eventListener) {
            _typedComponentRemovedFromEntityEvent.Subscribe(eventListener);
        }

        public void UnsubscribeOnComponentRemovedFromEntity(Entity entity, IComponentRemovedFromEntityEventListener<TComponent> eventListener) {
            _typedComponentRemovedFromEntityEvent.Unsubscribe(ref entity, eventListener);
        }

        public void UnsubscribeOnComponentRemovedFromEntity(IComponentRemovedFromEntityEventListener<TComponent> eventListener) {
            _typedComponentRemovedFromEntityEvent.Unsubscribe(eventListener);
        }

        public void SubscribeOnComponentChangedOfEntity(Entity entity, IComponentChangedOfEntityEventListener<TComponent> eventListener) {
            _typedComponentChangedOfEntityEvent.Subscribe(ref entity, eventListener);
        }

        public void SubscribeOnComponentChangedOfEntity(IComponentChangedOfEntityEventListener<TComponent> eventListener) {
            _typedComponentChangedOfEntityEvent.Subscribe(eventListener);
        }

        public void UnsbscribeOnComponentChangedOfEntity(Entity entity, IComponentChangedOfEntityEventListener<TComponent> eventListener) {
            _typedComponentChangedOfEntityEvent.Unsubscribe(ref entity, eventListener);
        }
        public void UnsbscribeOnComponentChangedOfEntity(IComponentChangedOfEntityEventListener<TComponent> eventListener) {
            _typedComponentChangedOfEntityEvent.Unsubscribe(eventListener);
        }

        public override void OnEntityRemoved(object sender, Entity entity) {
            base.OnEntityRemoved(sender, entity);
            _typedComponentAddedToEntityEvent.RemoveEntityFromEvent(ref entity);            
            _typedComponentRemovingFromEntityEvent.RemoveEntityFromEvent(ref entity);            
            _typedComponentRemovedFromEntityEvent.RemoveEntityFromEvent(ref entity);            
            _typedComponentChangedOfEntityEvent.RemoveEntityFromEvent(ref entity);
        }

        #endregion

        public Entity GetEntity(int index) { return _entities[index]; }
        public TComponent this[int index] { get { return _components[index]; } }

        public int Length { get { return _size; } }

        public override Type ComponentType { get { return componentType; } }
        

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
            _componentAddedToEntityEvent.CallEvent(this, ref entity, ref component);
            _typedComponentAddedToEntityEvent.CallEvent(this, ref entity, ref component);
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
            _componentAddedToEntityEvent.CallEvent(this, ref entity, ref component);
            _typedComponentAddedToEntityEvent.CallEvent(this, ref entity, ref component);

            return true;
        }


        public override bool Remove(Entity entity) {
            int index;
            if (_componentsMap.TryGetValue(entity, out index)) {
                int lastId = _size - 1;
                TComponent component = _components[index];
                _componentRemovingFromEntityEvent.CallEvent(this, ref entity, ref component);
                _typedComponentRemovingFromEntityEvent.CallEvent(this, ref entity, ref component);
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
                _componentRemovedFromEntityEvent.CallEvent(this, ref entity, ComponentType);
                _typedComponentRemovedFromEntityEvent.CallEvent(this, ref entity);
                return true;
            }
            return false;
        }


        public void Update(Entity entity, TComponent component) {
            int index;
            if (_componentsMap.TryGetValue(entity, out index)) {
                _components[index] = component;
                _componentChangedOfEntityEvent.CallEvent(this, ref entity, ref component);
                _typedComponentChangedOfEntityEvent.CallEvent(this, ref entity, ref component);
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
