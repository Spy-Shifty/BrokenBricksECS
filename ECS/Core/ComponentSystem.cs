using System;

namespace ECS {
    public class ComponentSystem : IComponentSystemSetup, IEntityAddedEventListener, IEntityRemovedEventListener {
        public ComponentGroup group { get; private set; } 
        public virtual void OnEntityAdded(Entity entity) { }
        public virtual void OnEntityRemoved(Entity entity) { }
        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }

        void IComponentSystemSetup.AddGroup(ComponentGroup group) {
            this.group = group;
            if (group != null) {
                group.SubscripeOnEntityAdded(this);
                group.SubscripeOnEntityRemoved(this);
                //group.OnEntityAdded += OnEntityAdded;
                //group.OnEntityRemoved += OnEntityRemoved;
            }
        }

        void IComponentSystemSetup.RemoveGroup() {
            group.UnsubscripeOnEntityAdded(this);
            group.UnsubscripeOnEntityAdded(this);
            //group.OnEntityAdded -= OnEntityAdded;
            //group.OnEntityRemoved -= OnEntityRemoved;
            group = null;
        }
    }    

    interface IComponentSystemSetup {
        void AddGroup(ComponentGroup group);
        void RemoveGroup();
    }
}
