using System;
using System.Collections.Generic;

namespace ECS {
    public class ComponentSystem : IComponentSystemSetup, IEntityAddedEventListener, IEntityRemovedEventListener {
        public IEnumerable<ComponentGroup> Groups { get { return groups; } }

        private readonly HashSet<ComponentGroup> groups = new HashSet<ComponentGroup>();

        public virtual void OnEntityAdded(Entity entity) { }
        public virtual void OnEntityRemoved(Entity entity) { }
        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }

        void IComponentSystemSetup.AddGroup(ComponentGroup group) {
            if (group != null) {
                groups.Add(group);
                group.SubscripeOnEntityAdded(this);
                group.SubscripeOnEntityRemoved(this);
                //group.OnEntityAdded += OnEntityAdded;
                //group.OnEntityRemoved += OnEntityRemoved;
            }
        }

        void IComponentSystemSetup.RemoveGroup(ComponentGroup group) {
            if (group != null) {
                group.UnsubscripeOnEntityAdded(this);
                group.UnsubscripeOnEntityAdded(this);
                //group.OnEntityAdded -= OnEntityAdded;
                //group.OnEntityRemoved -= OnEntityRemoved;      
                groups.Remove(group);
            }
        }


        void IComponentSystemSetup.RemoveAllGroups() {
            foreach (var group in Groups) {
                group.UnsubscripeOnEntityAdded(this);
                group.UnsubscripeOnEntityAdded(this);
            }
        }

        public virtual void OnEntityAdded(object sender, Entity entity) {
            if (groups.Contains(sender as ComponentGroup)) {
                OnEntityAdded(entity);
            }
        }

        public virtual void OnEntityRemoved(object sender, Entity entity) {
            if (groups.Contains(sender as ComponentGroup)) {
                OnEntityRemoved(entity);
            }
        }
    }    

    interface IComponentSystemSetup {
        void AddGroup(ComponentGroup group);
        void RemoveGroup(ComponentGroup group);
        void RemoveAllGroups();
    }
}
