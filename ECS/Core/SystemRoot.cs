using ECS.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ECS {
    public sealed class StandardSystemRoot : SystemRoot<EntityManager> { }

    [InjectableDependency(LifeTime.PerInstance)]
    public class SystemRoot<TEntityManager> where TEntityManager : EntityManager {
        private readonly List<ComponentSystem> _startSystemList = new List<ComponentSystem>();
        private readonly List<ComponentSystem> _updateSystemList = new List<ComponentSystem>();
        private readonly List<ComponentSystem> _fixedUpdateSystemList = new List<ComponentSystem>();

        [InjectDependency]
        protected TEntityManager _entityManager;

        public SystemRoot() {
            InjectionManager.ResolveDependency(this);
        }

        public virtual void AddSystem<TComponentSystem>() where TComponentSystem : ComponentSystem {
            TComponentSystem componentSystem = InjectionManager.CreateObject<TComponentSystem>();
            
            Type componentSystemType = componentSystem.GetType();
            if (componentSystemType.GetMethod("OnStart").DeclaringType == componentSystemType) {
                _startSystemList.Add(componentSystem);
            }
            if (componentSystemType.GetMethod("OnUpdate").DeclaringType == componentSystemType) {
                _updateSystemList.Add(componentSystem);
            }
            if (componentSystemType.GetMethod("OnFixedUpdate").DeclaringType == componentSystemType) {
                _fixedUpdateSystemList.Add(componentSystem);
            }
            HandleTupleInjection(componentSystem);
        }

        public virtual void AddSystem(ComponentSystem componentSystem) {
            Type componentSystemType = componentSystem.GetType();

            if (componentSystemType.GetMethod("OnStart").DeclaringType == componentSystemType) {
                _startSystemList.Add(componentSystem);
            }
            if (componentSystemType.GetMethod("OnUpdate").DeclaringType == componentSystemType) {
                _updateSystemList.Add(componentSystem);
            }
            if (componentSystemType.GetMethod("OnFixedUpdate").DeclaringType == componentSystemType) {
                _fixedUpdateSystemList.Add(componentSystem);
            }
            InjectionManager.ResolveDependency(componentSystem);
            HandleTupleInjection(componentSystem);
        }

        protected void HandleTupleInjection(ComponentSystem system) {
            Type systemType = system.GetType();

            Type injectTupleAttributeType = typeof(InjectTupleAttribute);
            Type iComponentArrayType = typeof(ComponentArray);
            FieldInfo[] allFields = systemType.GetFieldsRecursive(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToArray();
            IGrouping<InjectTupleAttribute, FieldInfo>[] injectionTypeGroups = systemType.GetFieldsRecursive(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                .Where(field => field.GetCustomAttributes(injectTupleAttributeType, false).Any())
                .Where(field => iComponentArrayType.IsAssignableFrom(field.FieldType))
                .GroupBy(field => (field.GetCustomAttributes(injectTupleAttributeType, false).First() as InjectTupleAttribute))
                .ToArray();


            IComponentSystemSetup systemSetup = system;
            foreach (IGrouping<InjectTupleAttribute, FieldInfo> injectionTypeGroup in injectionTypeGroups) {
                FieldInfo[] injectionTypeFields = injectionTypeGroup.ToArray();
                Type[] injectionComponentTypes = injectionTypeGroup
                    .Select(field => field.FieldType.GetGenericArguments()[0])
                    .ToArray();

                ComponentGroup group = _entityManager.GetComponentGroup(injectionComponentTypes);

                for (int i = 0; i < injectionComponentTypes.Length; i++) {
                    ComponentArray componentArray = group.GetComponent(injectionComponentTypes[i]);
                    injectionTypeFields[i].SetValue(system, componentArray);
                }

                systemSetup.AddGroup(group, injectionTypeGroup.Key.GroupName);
            }
            systemSetup.SetEntityManager(_entityManager);
        }

        public virtual void RemoveSystem(ComponentSystem system) {
            _startSystemList.Remove(system);
            _updateSystemList.Remove(system);
            _fixedUpdateSystemList.Remove(system);

            IComponentSystemSetup systemSetup = system;
            systemSetup.RemoveAllGroups();
        }

        protected virtual void OnError(Exception ex) {
            Console.WriteLine(ex.Message);
        }

        public virtual void Start() {
            for (int i = 0; i < _startSystemList.Count; i++) {
                try {  _startSystemList[i].OnStart(); } catch (Exception ex) { OnError(ex); }
            }
            //_entityManager.ProcessMessageQueue();
        }
        public virtual void Update() {
            for (int i = 0; i < _updateSystemList.Count; i++) {
                try { _updateSystemList[i].OnUpdate(); } catch(Exception ex) { OnError(ex); }
            }
            _entityManager.HandleDeletion();
            //_entityManager.ProcessMessageQueue();
        }

        public virtual void FixedUpdate() {
            for (int i = 0; i < _fixedUpdateSystemList.Count; i++) {
                try { _fixedUpdateSystemList[i].OnFixedUpdate(); } catch (Exception ex) { OnError(ex); }
            }
            //_entityManager.ProcessMessageQueue();
        }
    }
}
