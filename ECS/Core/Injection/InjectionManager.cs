using ECS.Extensions;
using ECS.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ECS {
    public static class InjectionManager {
         
        private static readonly IocContainer iocContainer = new IocContainer();

        static InjectionManager() {
            Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblyArray.Length; i++) {
                Type[] typeArray = assemblyArray[i].GetTypes();
                for (int j = 0; j < typeArray.Length; j++) {
                    var injectableDependencyAttributes = (InjectableDependencyAttribute[])typeArray[j].GetCustomAttributes(typeof(InjectableDependencyAttribute), true);
                    if (injectableDependencyAttributes.Any()) {
                        iocContainer.Register(typeArray[j], typeArray[j], injectableDependencyAttributes[0].Lifetime);
                    }
                }
            }
        }

        public static void ResolveDependency(object obj) {
            Type type = obj.GetType();
            IEnumerable<FieldInfo> dependencyFields = type.GetFieldsRecursive(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Where(field => field.GetCustomAttributes(typeof(InjectDependencyAttribute), true).Any());
            foreach (var field in dependencyFields) {
                if (field.GetValue(obj) == null) {
                    field.SetValue(obj, iocContainer.Resolve(field.FieldType));
                }
            }
        }
        

        public static object CreateObject(Type type) {
            if (!iocContainer.IsRegistered(type)) {
                iocContainer.Register(type, type, LifeTime.PerInstance);
            }
            object instance = iocContainer.Resolve(type);
            ResolveDependency(instance);
            return instance;
        }

        public static T CreateObject<T>() {
            if (!iocContainer.IsRegistered(typeof(T))) {
                iocContainer.Register<T,T>(LifeTime.PerInstance);
            }
            T instance = iocContainer.Resolve<T>();
            ResolveDependency(instance);
            return instance;
        }
    }
}
