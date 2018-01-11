using System;

namespace ECS
{
    public struct Entity : IEquatable<Entity> {
        private int id;

        public int Id { get { return id; } }
        public Entity(int id) {
            this.id = id;
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

        public static bool operator ==(Entity a, Entity b) {
            return a.id == b.id;
        }

        public static bool operator !=(Entity a, Entity b) {
            return a.id != b.id;
        }
    }

}
