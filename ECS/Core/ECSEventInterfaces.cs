
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {

    public interface IEntityAddedEventListener {
        void OnEntityAdded(Entity entity);
    }

    public interface IEntityRemovedEventListener {
        void OnEntityRemoved(Entity entity);
    }

    public interface IComponentAddedToEntityEventListener {
        void OnComponentAddedToEntity(Entity entity, Type componentType);
    }

    public interface IComponentRemovedFromEntityEventListener {
        void OnComponentRemovedFromEntity(Entity entity, Type componentType);
    }
    

    public interface IEntityRemoveEventListener {
        void OnEntityRemoved(Entity entity, Type componentType);
    }

    public interface IEntityAddedEventListener<TComponent> where TComponent : IComponent {
        void OnEntityAdded(Entity entity, TComponent component);
    }

    public interface IEntityRemovedEventListener<TComponent> where TComponent : IComponent {
        void OnEntityRemoved(Entity entity, TComponent component);
    }

    public interface IComponentChangedEventListener<TComponent> where TComponent : IComponent {
        void OnComponentChanged(Entity entity, TComponent component);
    }
}
