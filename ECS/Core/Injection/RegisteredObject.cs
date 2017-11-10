using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ECS.Injection {
    public class RegisteredObject {
        public RegisteredObject(Type typeToResolve, Type concreteType, LifeTime lifetimeType) {
            TypeToResolve = typeToResolve;
            ConcreteType = concreteType;
            Lifetime = lifetimeType;
        }
        
        public Type TypeToResolve { get; private set; }
        public Type ConcreteType { get; private set; }
        public object Instance { get; private set; }
        public LifeTime Lifetime { get; private set; }
        public void  CreateInstance(params object[] args) {
            Instance = Activator.CreateInstance(ConcreteType, args);
        }
    }
}
