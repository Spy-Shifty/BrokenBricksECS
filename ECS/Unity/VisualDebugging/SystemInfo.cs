using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ECS.VisualDebugging {

    public class SystemInfo {

        public double AccumulatedExecutionDuration { get { return _accumulatedExecutionDuration; } }
        public double MinExecutionDuration { get { return _minExecutionDuration; } }
        public double MaxExecutionDuration { get { return _maxExecutionDuration; } }
        public double AverageExecutionDuration { get { return _durationsCount == 0 ? 0 : _accumulatedExecutionDuration / _durationsCount; } }
        
        public ComponentSystem system { get { return _system; } }
        public string systemName { get { return _systemName; } }

        public bool isActive;

        readonly ComponentSystem _system;
        readonly string _systemName;

        double _accumulatedExecutionDuration;
        double _minExecutionDuration;
        double _maxExecutionDuration;
        int _durationsCount;

        const string SYSTEM_SUFFIX = "System";

        public SystemInfo(ComponentSystem system) {
            _system = system;
            var debugSystem = system as DebugSystems;
            if (debugSystem != null) {
                _systemName = debugSystem.name;
            } else {
                var systemType = system.GetType();
                _systemName = systemType.Name.EndsWith(SYSTEM_SUFFIX, StringComparison.Ordinal)
                    ? systemType.Name.Substring(0, systemType.Name.Length - SYSTEM_SUFFIX.Length)
                    : systemType.Name;
            }

            isActive = true;
        }

        public void AddExecutionDuration(double executionDuration) {
            if (executionDuration < _minExecutionDuration || _minExecutionDuration == 0) {
                _minExecutionDuration = executionDuration;
            }
            if (executionDuration > _maxExecutionDuration) {
                _maxExecutionDuration = executionDuration;
            }

            _accumulatedExecutionDuration += executionDuration;
            _durationsCount += 1;
        }

        public void ResetDuration() {
            _accumulatedExecutionDuration = 0;
            _durationsCount = 0;
        }
    }
}