using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {
    
    public class ECSEvent<T> {
        private List<Action<T>> eventListeners = new List<Action<T>>();

        public void Subscribe(Action<T> eventListener) {
            if (!eventListeners.Contains(eventListener)) {
                eventListeners.Add(eventListener);
            }
        }

        public void Unsubscribe(Action<T> eventListener) {
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

        public void Subscribe(IEntityAddedEventListener eventListener) {
            eventListeners.Add(eventListener);
        }

        public void Unsubscribe(IEntityAddedEventListener eventListener) {
            eventListeners.Remove(eventListener);

        }

        public void CallEvent(object sender, ref Entity entity) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnEntityAdded(sender, entity);
            }
        }
    }
    class EntityRemovingEvent {
        private Dictionary<Entity, List<IEntityRemovingEventListener>> eventListenerMap = new Dictionary<Entity, List<IEntityRemovingEventListener>>();
        private List<IEntityRemovingEventListener> eventListeners = new List<IEntityRemovingEventListener>();

        public void Subscribe(ref Entity entity, IEntityRemovingEventListener eventListener) {
            List<IEntityRemovingEventListener> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<IEntityRemovingEventListener>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscribe(ref Entity entity, IEntityRemovingEventListener eventListener) {
            List<IEntityRemovingEventListener> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void Subscribe(IEntityRemovingEventListener eventListener) {
            eventListeners.Add(eventListener);
        }

        public void Unsubscribe(IEntityRemovingEventListener eventListener) {
            eventListeners.Remove(eventListener);
        }

        public void CallEvent(object sender, ref Entity entity) {
            List<IEntityRemovingEventListener> mapEventListeners;
            if (eventListenerMap.TryGetValue(entity, out mapEventListeners)) {
                for (int i = 0; i < mapEventListeners.Count; i++) {
                    mapEventListeners[i].OnEntityRemoving(sender, entity);
                }
            }

            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnEntityRemoving(sender, entity);
            }
        }

    }

    class EntityRemovedEvent {
        private Dictionary<Entity, List<IEntityRemovedEventListener>> eventListenerMap = new Dictionary<Entity, List<IEntityRemovedEventListener>>();
        private List<IEntityRemovedEventListener> eventListeners = new List<IEntityRemovedEventListener>();

        public void Subscribe(ref Entity entity, IEntityRemovedEventListener eventListener) {
            List<IEntityRemovedEventListener> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<IEntityRemovedEventListener>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscribe(ref Entity entity, IEntityRemovedEventListener eventListener) {
            List<IEntityRemovedEventListener> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void Subscribe(IEntityRemovedEventListener eventListener) {
            eventListeners.Add(eventListener);
        }

        public void Unsubscribe(IEntityRemovedEventListener eventListener) {
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




    public abstract class ECSComponentEvent<TComponentEventListener> {
        protected Dictionary<Entity, List<TComponentEventListener>> eventListenerMap = new Dictionary<Entity, List<TComponentEventListener>>();
        protected List<TComponentEventListener> eventListeners = new List<TComponentEventListener>();

        public void Subscribe(ref Entity entity, TComponentEventListener eventListener) {
            List<TComponentEventListener> eventListeners;
            if (!eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners = new List<TComponentEventListener>();
                eventListenerMap.Add(entity, eventListeners);
            }
            eventListeners.Add(eventListener);
        }

        public void Unsubscribe(ref Entity entity, TComponentEventListener eventListener) {
            List<TComponentEventListener> eventListeners;
            if (eventListenerMap.TryGetValue(entity, out eventListeners)) {
                eventListeners.Remove(eventListener);
                if (eventListeners.Count == 0) {
                    eventListenerMap.Remove(entity);
                }
            }
        }

        public void Subscribe(TComponentEventListener eventListener) {
            eventListeners.Add(eventListener);
        }

        public void Unsubscribe(TComponentEventListener eventListener) {
            eventListeners.Remove(eventListener);
        }
        
        public void RemoveEntityFromEvent(ref Entity entity) {
            eventListenerMap.Remove(entity);
        }
        
    }



    public class ComponentAddedToEntityEvent : ECSComponentEvent<IComponentAddedToEntityEventListener> {
        public void CallEvent<TComponent>(object sender, ref Entity entity, ref TComponent component) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnComponentAddedToEntity(sender, entity, component);
            }

            List<IComponentAddedToEntityEventListener> entityEventListeners;
            if (eventListenerMap.TryGetValue(entity, out entityEventListeners)) {
                for (int i = 0; i < entityEventListeners.Count; i++) {
                    entityEventListeners[i].OnComponentAddedToEntity(sender, entity, component);
                }
            }
        }
    }

    public class ComponentRemovingFromEntityEvent  : ECSComponentEvent<IComponentRemovingFromEntityEventListener> {
        public void CallEvent<TComponent>(object sender, ref Entity entity, ref TComponent component) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnComponentRemovingFromEntity(sender, entity, component);
            }

            List<IComponentRemovingFromEntityEventListener> entityEventListeners;
            if (eventListenerMap.TryGetValue(entity, out entityEventListeners)) {
                for (int i = 0; i < entityEventListeners.Count; i++) {
                    entityEventListeners[i].OnComponentRemovingFromEntity(sender, entity, component);
                }
            }
        }
    }

    public class ComponentRemovedFromEntityEvent : ECSComponentEvent<IComponentRemovedFromEntityEventListener> {
        public void CallEvent(object sender, ref Entity entity, Type componentType) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnComponentRemovedFromEntity(sender, entity, componentType);
            }

            List<IComponentRemovedFromEntityEventListener> entityEventListeners;
            if (eventListenerMap.TryGetValue(entity, out entityEventListeners)) {
                for (int i = 0; i < entityEventListeners.Count; i++) {
                    entityEventListeners[i].OnComponentRemovedFromEntity(sender, entity, componentType);
                }
            }
        }
    }

    public class ComponentChangedOfEntityEvent : ECSComponentEvent<IComponentChangedOfEntityEventListener> {
        public void CallEvent<TComponent>(object sender, ref Entity entity, ref TComponent component) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnComponentChangedOfEntity(sender, entity, component);
            }

            List<IComponentChangedOfEntityEventListener> entityEventListeners;
            if (eventListenerMap.TryGetValue(entity, out entityEventListeners)) {
                for (int i = 0; i < entityEventListeners.Count; i++) {
                    entityEventListeners[i].OnComponentChangedOfEntity(sender, entity, component);
                }
            }
        }
    }









    public class ComponentAddedToEntityEvent<TComponent>  : ECSComponentEvent<IComponentAddedToEntityEventListener<TComponent>> where TComponent : IComponent {
        public void CallEvent(object sender, ref Entity entity, ref TComponent component) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnEntityAdded(sender, entity, component);
            }

            List<IComponentAddedToEntityEventListener<TComponent>> entityEventListeners;
            if (eventListenerMap.TryGetValue(entity, out entityEventListeners)) {
                for (int i = 0; i < entityEventListeners.Count; i++) {
                    entityEventListeners[i].OnEntityAdded(sender, entity, component);
                }
            }
        }
    }

    public class ComponentRemovingFromEntityEvent<TComponent> : ECSComponentEvent<IComponentRemovingFromEntityEventListener<TComponent>> where TComponent : IComponent {
        public void CallEvent(object sender, ref Entity entity, ref TComponent component) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnEntityRemoved(sender, entity, component);
            }

            List<IComponentRemovingFromEntityEventListener<TComponent>> entityEventListeners;
            if (eventListenerMap.TryGetValue(entity, out entityEventListeners)) {
                for (int i = 0; i < entityEventListeners.Count; i++) {
                    entityEventListeners[i].OnEntityRemoved(sender, entity, component);
                }
            }
        }
    }

    public class ComponentRemovedFromEntityEvent<TComponent> : ECSComponentEvent<IComponentRemovedFromEntityEventListener<TComponent>> where TComponent : IComponent {
        public void CallEvent(object sender, ref Entity entity) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnEntityRemoved(sender, entity);
            }

            List<IComponentRemovedFromEntityEventListener<TComponent>> entityEventListeners;
            if (eventListenerMap.TryGetValue(entity, out entityEventListeners)) {
                for (int i = 0; i < entityEventListeners.Count; i++) {
                    entityEventListeners[i].OnEntityRemoved(sender, entity);
                }
            }
        }
    }

    public class ComponentChangedOfEntityEvent<TComponent>: ECSComponentEvent<IComponentChangedOfEntityEventListener<TComponent>> where TComponent : IComponent {
        public void CallEvent(object sender, ref Entity entity, ref TComponent component) {
            for (int i = 0; i < eventListeners.Count; i++) {
                eventListeners[i].OnComponentChanged(sender, entity, component);
            }

            List<IComponentChangedOfEntityEventListener<TComponent>> entityEventListeners;
            if (eventListenerMap.TryGetValue(entity, out entityEventListeners)) {
                for (int i = 0; i < entityEventListeners.Count; i++) {
                    entityEventListeners[i].OnComponentChanged(sender, entity, component);
                }
            }
        }
    }
}
