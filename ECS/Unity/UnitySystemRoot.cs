using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
#if UNITY_EDITOR
using ECS.VisualDebugging;
#endif

namespace ECS {
    
    [InjectableDependency(LifeTime.PerInstance)]
    public class UnityStandardSystemRoot : UnitySystemRoot<UnityEntityManager>  { }

    public partial class UnitySystemRoot<TEntityManager> : SystemRoot<TEntityManager> where TEntityManager : UnityEntityManager {
        protected override void OnError(Exception ex) {
            Debug.LogError(ex.Message + "  " + ex.StackTrace);
        }
    }

#if (UNITY_EDITOR && ECS_DEBUG)

    [InjectableDependency(LifeTime.PerInstance)]
    public partial class UnitySystemRoot<TEntityManager> : SystemRoot<TEntityManager> where TEntityManager : UnityEntityManager {
        
        private readonly Dictionary<string, DebugSystems> _componentSystemList = new Dictionary<string, DebugSystems>();

        private DebugSystems _rootDebugSystems;
        public UnitySystemRoot() {
            _rootDebugSystems = new DebugSystems("Systems");
        }
        public override void AddSystem<TComponentSystem>() {
            TComponentSystem componentSystem = InjectionManager.CreateObject<TComponentSystem>();           
            HandleTupleInjection(componentSystem);
            AddToDebugSystem(componentSystem);
        }

        public override void AddSystem(ComponentSystem system) {
            InjectionManager.ResolveDependency(system);
            HandleTupleInjection(system);
            AddToDebugSystem(system);
        }

        private void AddToDebugSystem(ComponentSystem system) {
            string groupName = GetGroupNameFromSystem(system.GetType());
            if (string.IsNullOrEmpty(groupName)) {
                _rootDebugSystems.AddSystem(system);
                return;
            }

            DebugSystems debugSystems;
            if (!_componentSystemList.TryGetValue(groupName, out debugSystems)) {
                debugSystems = new DebugSystems(groupName);
                _rootDebugSystems.AddSystem(debugSystems);
                _componentSystemList.Add(groupName, debugSystems);
            }
            debugSystems.AddSystem(system);
        }

        private static string GetGroupNameFromSystem(Type systemType) {

            DebugSystemGroupAttribute debugSystemGroupAttribute = systemType
                .GetCustomAttributes(typeof(DebugSystemGroupAttribute), false)
                .Cast<DebugSystemGroupAttribute>().FirstOrDefault();

            if (debugSystemGroupAttribute == null) {
                return "";
            }
            return debugSystemGroupAttribute.Group;
        }

        public override void RemoveSystem(ComponentSystem system) {
            DebugSystems debugSystems;
            string groupName = GetGroupNameFromSystem(system.GetType());
            if (!_componentSystemList.TryGetValue(groupName, out debugSystems)) {
                return;
            }
            _rootDebugSystems.RemoveSystem(system);
            debugSystems.RemoveSystem(system);
            IComponentSystemSetup systemSetup = system;
            systemSetup.RemoveAllGroups();
        }

        public override void Start() {
            _rootDebugSystems.OnStart();
        }

        public override void Update() {
            _rootDebugSystems.OnUpdate();
        }

        public override void FixedUpdate() {
            _rootDebugSystems.OnFixedUpdate();
        }
    }
#endif
}
