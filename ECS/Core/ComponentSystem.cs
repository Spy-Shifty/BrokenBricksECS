using System;
using System.Collections.Generic;

namespace ECS {
    public class ComponentSystem : IComponentSystemSetup, IEntityAddedEventListener, IEntityRemovedEventListener, IEntityRemovingEventListener {
        public IEnumerable<ComponentGroup> Groups { get { return groups.Keys; } }

        private readonly Dictionary<ComponentGroup, string> groups = new Dictionary<ComponentGroup,string>();

        public virtual void OnEntityAdded(string groupName, Entity entity) { }
        public virtual void OnEntityRemoving(string groupName, Entity entity) { }
        public virtual void OnEntityRemoved(string groupName, Entity entity) { }
        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }

        void IComponentSystemSetup.AddGroup(ComponentGroup group, string groupName) {
            if (group != null) {
                groups.Add(group, groupName);
                group.SubscribeOnEntityAdded(this);
                group.SubscribeOnEntityRemoving(this);
                group.SubscribeOnEntityRemoved(this);
                //group.OnEntityAdded += OnEntityAdded;
                //group.OnEntityRemoved += OnEntityRemoved;
            }
        }

        void IComponentSystemSetup.RemoveGroup(ComponentGroup group) {
            if (group != null) {
                group.UnsubscribeOnEntityAdded(this);
                group.UnsubscribeOnEntityRemoving(this);
                group.UnsubscribeOnEntityRemoved(this);
                //group.OnEntityAdded -= OnEntityAdded;
                //group.OnEntityRemoved -= OnEntityRemoved;      
                groups.Remove(group);
            }
        }


        void IComponentSystemSetup.RemoveAllGroups() {
            foreach (var group in Groups) {
                group.UnsubscribeOnEntityAdded(this);
                group.UnsubscribeOnEntityRemoving(this);
                group.UnsubscribeOnEntityRemoved(this);
            }
        }

         void IEntityAddedEventListener.OnEntityAdded(object sender, Entity entity) {
            string groupName;
            if (groups.TryGetValue(sender as ComponentGroup, out groupName) ) {
                OnEntityAdded(groupName, entity);
            }
        }

        void IEntityRemovedEventListener.OnEntityRemoved(object sender, Entity entity) {
            string groupName;
            if (groups.TryGetValue(sender as ComponentGroup, out groupName)) {
                OnEntityRemoved(groupName, entity);
            }
        }

        void IEntityRemovingEventListener.OnEntityRemoving(object sender, Entity entity) {
            string groupName;
            if (groups.TryGetValue(sender as ComponentGroup, out groupName)) {
                OnEntityRemoving(groupName, entity);
            }
        }
    }    

    interface IComponentSystemSetup {
        void AddGroup(ComponentGroup group, string groupName);
        void RemoveGroup(ComponentGroup group);
        void RemoveAllGroups();
    }
}
