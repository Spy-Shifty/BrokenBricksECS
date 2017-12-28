using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {
    
    public class ECSEvent<T> {
        private List<Action<T>> eventListeners = new List<Action<T>>();

        public void Subscripe(Action<T> eventListener) {
            if (!eventListeners.Contains(eventListener)) {
                eventListeners.Add(eventListener);
            }
        }

        public void Unsubscripe(Action<T> eventListener) {
            eventListeners.Remove(eventListener);
        }

        public void CallEvent(T eventArg) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].Invoke(eventArg);
            }
        }
    }

    class EntityAddedEvent {
        private List<IEntityAddedEventListener> eventListeners = new List<IEntityAddedEventListener>();

        public void Subscripe(IEntityAddedEventListener eventListener) {
            eventListeners.Add(eventListener);
        }

        public void Unsubscripe(IEntityAddedEventListener eventListener) {
            eventListeners.Remove(eventListener);

        }

        public void CallEvent(object sender, ref Entity entity) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnEntityAdded(sender, entity);
            }
        }
    }

    class EntityRemovedEvent {
        private Dictionary<Entity, List<IEntityRemovedEventListener>> eventListenerMap = new Dictionary<Entity, List<IEntityRemovedEventListener>>();
        private List<IEntityRemovedEventListener> eventListeners = new List<IEntityRemovedEventListener>();

        public void Subscripe(ref Entity entity, IEntityRemovedEventListener eventListener) {
            List<IEntityRemovedEventListener> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<IEntityRemovedEventListener>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscripe(ref Entity entity, IEntityRemovedEventListener eventListener) {
            List<IEntityRemovedEventListener> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void Subscripe(IEntityRemovedEventListener eventListener) {
            eventListeners.Add(eventListener);
        }

        public void Unsubscripe(IEntityRemovedEventListener eventListener) {
            eventListeners.Remove(eventListener);
        }

        public void CallEvent(object sender, ref Entity entity) {
            List<IEntityRemovedEventListener> mapEventListeners;
            if (eventListenerMap.TryGetValue(entity, out mapEventListeners)) {
                for (int i = 0; i < mapEventListeners.Count; i++) {
                    mapEventListeners[i].OnEntityRemoved(sender, entity);
                }
            }

            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnEntityRemoved(sender, entity);
            }
        }

    }

    class ComponentAddedToEntityEvent {
        private Dictionary<Entity, List<IComponentAddedToEntityEventListener>> eventListenerMap = new Dictionary<Entity, List<IComponentAddedToEntityEventListener>>();

        public void Subscripe(ref Entity entity, IComponentAddedToEntityEventListener eventListener) {
            List<IComponentAddedToEntityEventListener> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<IComponentAddedToEntityEventListener>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscripe(ref Entity entity, IComponentAddedToEntityEventListener eventListener) {
            List<IComponentAddedToEntityEventListener> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void CallEvent(object sender, ref Entity entity, Type componentType) {
            List<IComponentAddedToEntityEventListener> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                for (int i = 0; i < eventListeners.Count; i++) {
                    eventListeners[i].OnComponentAddedToEntity(sender, entity, componentType);
                }
            }
        }

        public void RemoveEntityFromEvent(ref Entity entity) {
            eventListenerMap.Remove(entity);
        }
    }

    class ComponentRemovedFromEntityEvent {
        private Dictionary<Entity, List<IComponentRemovedFromEntityEventListener>> eventListenerMap = new Dictionary<Entity, List<IComponentRemovedFromEntityEventListener>>();

        public void Subscripe(ref Entity entity, IComponentRemovedFromEntityEventListener eventListener) {
            List<IComponentRemovedFromEntityEventListener> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<IComponentRemovedFromEntityEventListener>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscripe(ref Entity entity, IComponentRemovedFromEntityEventListener eventListener) {
            List<IComponentRemovedFromEntityEventListener> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void CallEvent(object sender, ref Entity entity, Type componentType) {
            List<IComponentRemovedFromEntityEventListener> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                for (int i = 0; i < eventListeners.Count; i++) {
                    eventListeners[i].OnComponentRemovedFromEntity(sender, entity, componentType);
                }
            }
        }

        public void RemoveEntityFromEvent(Entity entity) {
            eventListenerMap.Remove(entity);
        }
    }


    class EntityAddedEvent<TComponent> where TComponent : IComponent {
        private Dictionary<Entity, List<IEntityAddedEventListener<TComponent>>> eventListenerMap = new Dictionary<Entity, List<IEntityAddedEventListener<TComponent>>>();

        public void Subscripe(ref Entity entity, IEntityAddedEventListener<TComponent> eventListener) {
            List<IEntityAddedEventListener<TComponent>> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<IEntityAddedEventListener<TComponent>>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscripe(ref Entity entity, IEntityAddedEventListener<TComponent> eventListener) {
            List<IEntityAddedEventListener<TComponent>> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void CallEvent(object sender, ref Entity entity, ref TComponent component) {
            List<IEntityAddedEventListener<TComponent>> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                for (int i = 0; i < eventListeners.Count; i++) {
                    eventListeners[i].OnEntityAdded(sender, entity, component);
                }
            }
        }
    }

    class EntityRemovedEvent<TComponent> where TComponent : IComponent {
        private Dictionary<Entity, List<IEntityRemovedEventListener<TComponent>>> eventListenerMap = new Dictionary<Entity, List<IEntityRemovedEventListener<TComponent>>>();

        public void Subscripe(ref Entity entity, IEntityRemovedEventListener<TComponent> eventListener) {
            List<IEntityRemovedEventListener<TComponent>> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<IEntityRemovedEventListener<TComponent>>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscripe(ref Entity entity, IEntityRemovedEventListener<TComponent> eventListener) {
            List<IEntityRemovedEventListener<TComponent>> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void CallEvent(object sender, Entity entity, TComponent component) {
            List<IEntityRemovedEventListener<TComponent>> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                for (int i = 0; i < eventListeners.Count; i++) {
                    eventListeners[i].OnEntityRemoved(sender, entity, component);
                }
            }
        }
    }


    class EntityComponentChangedEvent<TComponent> where TComponent : IComponent {
        private Dictionary<Entity, List<IComponentChangedEventListener<TComponent>>> eventListenerMap = new Dictionary<Entity, List<IComponentChangedEventListener<TComponent>>>();

        public void Subscripe(ref Entity entity, IComponentChangedEventListener<TComponent> eventListener) {
            List<IComponentChangedEventListener<TComponent>> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<IComponentChangedEventListener<TComponent>>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscripe(ref Entity entity, IComponentChangedEventListener<TComponent> eventListener) {
            List<IComponentChangedEventListener<TComponent>> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void CallEvent(object sender, ref Entity entity, ref TComponent component) {
            List<IComponentChangedEventListener<TComponent>> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                for (int i = 0; i < eventListeners.Count; i++) {
                    eventListeners[i].OnComponentChanged(sender, entity, component);
                }
            }
        }
    }
}
