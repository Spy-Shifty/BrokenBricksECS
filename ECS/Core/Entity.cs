using System;

namespace ECS
{
    public struct Entity : IEquatable<Entity> {
        private int id;

        EntityManager _entityManager;

        public int Id { get { return id; } }

        public Entity(int id, EntityManager entityManager) {
            this.id = id;
            _entityManager = entityManager;
        }

        public override int GetHashCode() {
            return id;
        }
        public override bool Equals(object obj) {
            return obj is Entity && ((Entity)obj).id == id;
        }

        public bool Equals(Entity other) {
            return id == other.id;
        }

        #region Components methods

        public bool HasComponent<T>() where T : IComponent
        {
            return _entityManager.HasComponent<T>(this);
        }

        public void AddComponent<T>(T component) where T : IComponent
        {
            _entityManager.AddComponent<T>(this, component);
        }

        public void RemoveComponent<T>() where T : IComponent
        {
            _entityManager.RemoveComponent<T>(this);
        }

        public T GetComponent<T>() where T : IComponent
        {
            return _entityManager.GetComponent<T>(this);
        }

        #endregion

        public static bool operator ==(Entity a, Entity b) {
            return a.id == b.id;
        }

        public static bool operator !=(Entity a, Entity b) {
            return a.id != b.id;
        }
    }

}
