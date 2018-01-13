using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {
    public class EntityAddedEventListener : IEntityAddedEventListener {

        public delegate void EventHandler(Entity entity);
        private EventHandler _eventHandler;

        public EntityAddedEventListener(EventHandler eventHandler) {
            if (eventHandler == null) {
                throw new NullReferenceException();
            }
            _eventHandler = eventHandler;
        }

        public void OnEntityAdded(object sender, Entity entity) {
            _eventHandler(entity);
        }
    }

    public class EntityRemovedEventListener : IEntityRemovedEventListener {

        public delegate void EventHandler(Entity entity);
        private EventHandler _eventHandler;

        public EntityRemovedEventListener(EventHandler eventHandler) {
            if (eventHandler == null) {
                throw new NullReferenceException();
            }
            _eventHandler = eventHandler;
        }

        public void OnEntityRemoved(object sender, Entity entity) {
            _eventHandler(entity);
        }
    }

    public class ComponentAddedToEntityEventListener : IComponentAddedToEntityEventListener {

        public delegate void EventHandler(Entity entity, Type componentType);
        private EventHandler _eventHandler;

        public ComponentAddedToEntityEventListener(EventHandler eventHandler) {
            if (eventHandler == null) {
                throw new NullReferenceException();
            }
            _eventHandler = eventHandler;
        }

        public void OnComponentAddedToEntity<TComponent>(object sender, Entity entity, TComponent component) {
            _eventHandler(entity, typeof(TComponent));
        }
    }

    public class ComponentRemovedToEntityEventListener : IComponentRemovedFromEntityEventListener {

        public delegate void EventHandler(Entity entity, Type componentType);
        private EventHandler _eventHandler;

        public ComponentRemovedToEntityEventListener(EventHandler eventHandler) {
            if (eventHandler == null) {
                throw new NullReferenceException();
            }
            _eventHandler = eventHandler;
        }

        public void OnComponentRemovedFromEntity(object sender, Entity entity, Type componentType) {
            _eventHandler(entity, componentType);
        }
    }

    public class ComponentAddedToEntityEventListener<TComponent> : IComponentAddedToEntityEventListener<TComponent> where TComponent : IComponent {

        public delegate void EventHandler(Entity entity, TComponent component);
        private EventHandler _eventHandler;

        public ComponentAddedToEntityEventListener(EventHandler eventHandler) {
            if (eventHandler == null) {
                throw new NullReferenceException();
            }
            _eventHandler = eventHandler;
        }

        public void OnEntityAdded(object sender, Entity entity, TComponent component) {
            _eventHandler(entity, component);
        }
    }

    public class ComponentRemovedToEntityEventListener<TComponent> : IComponentRemovedFromEntityEventListener<TComponent> where TComponent : IComponent {

        public delegate void EventHandler(Entity entity);
        private EventHandler _eventHandler;

        public ComponentRemovedToEntityEventListener(EventHandler eventHandler) {
            if (eventHandler == null) {
                throw new NullReferenceException();
            }
            _eventHandler = eventHandler;
        }

        public void OnEntityRemoved(object sender, Entity entity) {
            _eventHandler(entity);
        }
    }

    public class ComponentChangedToEntityEventListener<TComponent> : IComponentChangedOfEntityEventListener<TComponent> where TComponent : IComponent {

        public delegate void EventHandler(Entity entity, TComponent component);
        private EventHandler _eventHandler;

        public ComponentChangedToEntityEventListener(EventHandler eventHandler) {
            if (eventHandler == null) {
                throw new NullReferenceException();
            }
            _eventHandler = eventHandler;
        }

        public void OnComponentChanged(object sender, Entity entity, TComponent component) {
            _eventHandler(entity, component);

        }
    }
}
