using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ECS.VisualDebugging {

    public enum AvgResetInterval {
        Always = 1,
        VeryFast = 30,
        Fast = 60,
        Normal = 120,
        Slow = 300,
        Never = int.MaxValue
    }


    public class DebugSystems : ComponentSystem {

        public static AvgResetInterval avgResetInterval = AvgResetInterval.Never;
        private readonly GameObject _gameObject;

        private readonly string _name;
        private readonly Stopwatch _stopwatch;


        private readonly List<ComponentSystem> _startSystems;
        private readonly List<ComponentSystem> _updateSystems;
        private readonly List<ComponentSystem> _fixedUpdateSystems;

        private List<SystemInfo> _startSystemInfos;
        private List<SystemInfo> _updateSystemInfos;
        private List<SystemInfo> _fixedUpdateSystemInfos;

        public bool paused;
        public SystemInfo[] startSystemInfos { get { return _startSystemInfos.ToArray(); } }
        public SystemInfo[] updateSystemInfos { get { return _updateSystemInfos.ToArray(); } }
        public SystemInfo[] fixedUpdateSystemInfos { get { return _fixedUpdateSystemInfos.ToArray(); } }

        public int totalOnStartSystemsCount {
            get {
                var total = 0;
                foreach (var system in _startSystems) {
                    var debugSystems = system as DebugSystems;
                    total += debugSystems != null ? debugSystems.totalOnStartSystemsCount : 1;
                }
                return total;
            }
        }
        public int totalOnUpdateSystemsCount {
            get {
                var total = 0;
                foreach (var system in _updateSystems) {
                    var debugSystems = system as DebugSystems;
                    total += debugSystems != null ? debugSystems.totalOnUpdateSystemsCount : 1;
                }
                return total;
            }
        }
        public int totalOnFixedUpdateSystemsCount {
            get {
                var total = 0;
                foreach (var system in _fixedUpdateSystems) {
                    var debugSystems = system as DebugSystems;
                    total += debugSystems != null ? debugSystems.totalOnFixedUpdateSystemsCount : 1;
                }
                return total;
            }
        }

        public GameObject gameObject { get { return _gameObject; } }
        public string name { get { return _name; } }
        public double startExecutionDuration { get; private set; }
        public double updateExecutionDuration { get; private set; }
        public double fixedUpdateExecutionDuration { get; private set; }


        public DebugSystems(string name) {
            _name = name;

            _gameObject = new GameObject(_name);
            _gameObject.AddComponent<DebugSystemBehaviour>().Init(this);

            _startSystems = new List<ComponentSystem>();
            _updateSystems = new List<ComponentSystem>();
            _fixedUpdateSystems = new List<ComponentSystem>();
            _startSystemInfos = new List<SystemInfo>();
            _updateSystemInfos = new List<SystemInfo>();
            _fixedUpdateSystemInfos = new List<SystemInfo>();

            _stopwatch = new Stopwatch();
        }

        public void AddSystem(ComponentSystem componentSystem) {
            if (componentSystem == null) {
                return;
            }

            var debugSystems = componentSystem as DebugSystems;
            if (debugSystems != null) {
                debugSystems.gameObject.transform.SetParent(_gameObject.transform, false);
            }


            Type componentSystemType = componentSystem.GetType();
            if (componentSystemType.GetMethod("OnStart").DeclaringType == componentSystemType) {
                _startSystemInfos.Add(new SystemInfo(componentSystem));
                _startSystems.Add(componentSystem);
            }
            if (componentSystemType.GetMethod("OnUpdate").DeclaringType == componentSystemType) {
                _updateSystemInfos.Add(new SystemInfo(componentSystem));
                _updateSystems.Add(componentSystem);
            }
            if (componentSystemType.GetMethod("OnFixedUpdate").DeclaringType == componentSystemType) {
                _fixedUpdateSystemInfos.Add(new SystemInfo(componentSystem));
                _fixedUpdateSystems.Add(componentSystem);
            }
        }
        public void RemoveSystem(ComponentSystem componentSystem) {
            if (componentSystem == null) { return; }

            _startSystems.Remove(componentSystem);
            _updateSystems.Remove(componentSystem);
            _fixedUpdateSystems.Remove(componentSystem);

            _startSystemInfos.Remove(_startSystemInfos.Find(x => x.system == componentSystem));
            _updateSystemInfos.Remove(_updateSystemInfos.Find(x => x.system == componentSystem));
            _fixedUpdateSystemInfos.Remove(_fixedUpdateSystemInfos.Find(x => x.system == componentSystem));
        }


        public void ResetDuration() {
            foreach (var systemInfo in _updateSystemInfos) {
                systemInfo.ResetDuration();
            }
            foreach (var systemInfo in _fixedUpdateSystemInfos) {
                systemInfo.ResetDuration();
            }
        }


        public override void OnStart() {
            for (int i = 0; i < _startSystemInfos.Count; i++) {
                if (_startSystemInfos[i].isActive) {
                    double duration = MonitorSystemStartDuration(_startSystems[i]);
                    startExecutionDuration += duration;
                    _startSystemInfos[i].AddExecutionDuration(duration);
                }
            }            
        }

        public override void OnUpdate() {
            if (!paused) {
                StepExecuteOnUpdate();
            }
        }

        public override void OnFixedUpdate() {
            if (!paused) {
                StepExecuteOnFixedUpdate();
            }
        }
              

        public void StepExecuteOnUpdate() {
            updateExecutionDuration = 0;
            if (Time.frameCount % (int)avgResetInterval == 0) {
                ResetDuration();
            }
            for (int i = 0; i < _updateSystemInfos.Count; i++) {
                if (_updateSystemInfos[i].isActive) {
                    double duration = MonitorSystemUpdateDuration(_updateSystems[i]);
                    updateExecutionDuration += duration;
                    _updateSystemInfos[i].AddExecutionDuration(duration);
                }
            }
        }
        public void StepExecuteOnFixedUpdate() {
            fixedUpdateExecutionDuration = 0;
            if (Time.frameCount % (int)avgResetInterval == 0) {
                ResetDuration();
            }
            for (int i = 0; i < _fixedUpdateSystemInfos.Count; i++) {
                if (_fixedUpdateSystemInfos[i].isActive) {
                    double duration = MonitorSystemFixedUpdateDuration(_fixedUpdateSystems[i]);
                    fixedUpdateExecutionDuration += duration;
                    _fixedUpdateSystemInfos[i].AddExecutionDuration(duration);
                }
            }
        }

        private double MonitorSystemStartDuration(ComponentSystem _componentSystem) {
            _stopwatch.Reset();
            _stopwatch.Start();
            _componentSystem.OnStart();
            _stopwatch.Stop();
            return _stopwatch.Elapsed.TotalMilliseconds;
        }

        private double MonitorSystemUpdateDuration(ComponentSystem _componentSystem) {
            _stopwatch.Reset();
            _stopwatch.Start();
            _componentSystem.OnUpdate();
            _stopwatch.Stop();
            return _stopwatch.Elapsed.TotalMilliseconds;
        }

        private double MonitorSystemFixedUpdateDuration(ComponentSystem _componentSystem) {
            _stopwatch.Reset();
            _stopwatch.Start();
            _componentSystem.OnFixedUpdate();
            _stopwatch.Stop();
            return _stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
