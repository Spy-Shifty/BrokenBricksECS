using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ECS {
#if !(UNITY_EDITOR && ECS_DEBUG)
    [InjectableDependency(LifeTime.Singleton)]
    public class UnityEntityManager : EntityManager {  }

#else
    [InjectableDependency(LifeTime.Singleton)]
    public class UnityEntityManager : EntityManager {

        private List<EntityInfo> entityInfoList = new List<EntityInfo>();

        public int TotalEntities { get { return _entities.Count; } }
        public int TotalComponentTypes { get { return _components.Count; } }

        public List<EntityInfo> EntityList { get { return entityInfoList; } }

        public UnityEntityManager() {
            EntityCreated += UnityEntityManager_EntityCreated;
            EntityDestroyed += UnityEntityManager_EntityDestroyed;
            
        }

        private void UnityEntityManager_EntityCreated(Entity entity) {
            entityInfoList.Add(entity);
        }

        private void UnityEntityManager_EntityDestroyed(Entity entity) {
            int index = entityInfoList.FindIndex(x => x.Equals(entity));
            if (index != -1) {
                entityInfoList.RemoveAt(index);
            }
        }

    }
    
    public class EntityInfo : IEquatable<Entity> {
        public static implicit operator EntityInfo(Entity entity) {
            return new EntityInfo(entity);
        }
        public static implicit operator Entity(EntityInfo entityInfo) {
            return entityInfo._entity;
        }

        private readonly Entity _entity;
        public bool expanded;

        public string Name { get; set; }

        public EntityInfo(Entity entity) {
            _entity = entity;
        }

        public bool Equals(Entity other) {
            return _entity.Equals(other);
        }
    }
#endif
}
