
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS {

    public interface IEntityAddedEventListener {
        void OnEntityAdded(object sender, Entity entity);
    }
    public interface IEntityRemovingEventListener {
        void OnEntityRemoving(object sender, Entity entity);
    }
    public interface IEntityRemovedEventListener {
        void OnEntityRemoved(object sender, Entity entity);
    }

    public interface IComponentAddedToEntityEventListener {
        void OnComponentAddedToEntity(object sender, Entity entity, Type componentType);
    }

    public interface IComponentRemovingFromEntityEventListener {
        void OnComponentRemovingFromEntity<TComponent>(object sender, Entity entity, TComponent component);
    }

    public interface IComponentRemovedFromEntityEventListener {
        void OnComponentRemovedFromEntity(object sender, Entity entity, Type componentType);
    }
    

    public interface IEntityRemoveEventListener {
        void OnEntityRemoved(object sender, Entity entity, Type componentType);
    }

    public interface IEntityAddedEventListener<TComponent> where TComponent : IComponent {
        void OnEntityAdded(object sender, Entity entity, TComponent component);
    }

    public interface IEntityRemovedEventListener<TComponent> where TComponent : IComponent {
        void OnEntityRemoved(object sender, Entity entity, TComponent component);
    }

    public interface IComponentChangedEventListener<TComponent> where TComponent : IComponent {
        void OnComponentChanged(object sender, Entity entity, TComponent component);
    }
}
