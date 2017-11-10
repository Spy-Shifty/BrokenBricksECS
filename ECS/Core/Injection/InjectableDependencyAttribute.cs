using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ECS {

    public enum LifeTime { Singleton, PerInstance }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class InjectableDependencyAttribute : Attribute{
        public LifeTime Lifetime { get; private set; }
        public InjectableDependencyAttribute(LifeTime lifetime) {
            Lifetime = lifetime;
        }
    }
}
