using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ECS.VisualDebugging {
    [CustomEditor(typeof(DebugSystemBehaviour))]
    public class DebugSystemInspector : Editor {
        enum SortMethod {
            OrderOfOccurrence,

            Name,
            NameDescending,

            ExecutionTime,
            ExecutionTimeDescending
        }


        private const int SYSTEM_MONITOR_DATA_LENGTH = 60;
        
        private SystemsMonitor _updateSystemsMonitor;
        private SystemsMonitor _fixedUpdateSystemsMonitor;
        private Queue<float> _updateSystemMonitorData;
        private Queue<float> _fixedUpdateSystemMonitorData;
        private int _lastRenderedFrameCount;


        private float _threshold;
        private SortMethod _systemSortMethod;
        private static bool _hideEmptySystems = true;
        private static string _systemNameSearchString = string.Empty;
        private bool _showStartSystems = true;
        private bool _showUpdateSystems = true;
        private bool _showFixedUpdateSystems = true;

        public override void OnInspectorGUI() {
            var debugSystemsBehaviour = (DebugSystemBehaviour)target;
            var system = debugSystemsBehaviour.system;
            drawSystemsOverview(system);
            drawSystemsMonitor(system);
            drawSystemList(system);
            EditorUtility.SetDirty(target);
        }

        static void drawSystemsOverview(DebugSystems systems) {
            CustomEditorLayout.BeginVerticalBox();
            {
                EditorGUILayout.LabelField(systems.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Start Systems", systems.totalOnStartSystemsCount.ToString());
                EditorGUILayout.LabelField("Update Systems", systems.totalOnUpdateSystemsCount.ToString());
                EditorGUILayout.LabelField("FixedUpdate Systems", systems.totalOnFixedUpdateSystemsCount.ToString());
            }
            CustomEditorLayout.EndVertical();
        }

        void drawSystemsMonitor(DebugSystems systems) {
            if (_updateSystemsMonitor == null) {
                _updateSystemsMonitor = new SystemsMonitor(SYSTEM_MONITOR_DATA_LENGTH);
                _fixedUpdateSystemsMonitor = new SystemsMonitor(SYSTEM_MONITOR_DATA_LENGTH);
                _updateSystemMonitorData = new Queue<float>(new float[SYSTEM_MONITOR_DATA_LENGTH]);
                _fixedUpdateSystemMonitorData = new Queue<float>(new float[SYSTEM_MONITOR_DATA_LENGTH]);
            }

            CustomEditorLayout.BeginVerticalBox();
            {
                EditorGUILayout.LabelField("Execution duration", EditorStyles.boldLabel);

                CustomEditorLayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Execution duration: ");

                    var buttonStyle = new GUIStyle(GUI.skin.button);
                    if (systems.paused) {
                        buttonStyle.normal = GUI.skin.button.active;
                    }
                    if (GUILayout.Button("▌▌", buttonStyle, GUILayout.Width(50))) {
                        systems.paused = !systems.paused;
                    }
                    if (GUILayout.Button("Step", GUILayout.Width(50))) {
                        systems.paused = true;
                        systems.StepExecuteOnUpdate();
                        systems.StepExecuteOnFixedUpdate();
                        addDuration(_updateSystemMonitorData, (float)systems.updateExecutionDuration);
                        addDuration(_fixedUpdateSystemMonitorData, (float)systems.fixedUpdateExecutionDuration);
                    }
                }
                CustomEditorLayout.EndHorizontal();

                if (!EditorApplication.isPaused && !systems.paused) {
                    addDuration(_updateSystemMonitorData, (float)systems.updateExecutionDuration);
                    addDuration(_fixedUpdateSystemMonitorData, (float)systems.fixedUpdateExecutionDuration);
                    _lastRenderedFrameCount = Time.renderedFrameCount;
                }

                EditorGUILayout.LabelField("Update Execution duration:", systems.updateExecutionDuration.ToString() +" ms");
                _updateSystemsMonitor.Draw(_updateSystemMonitorData.ToArray(), 80f);

                EditorGUILayout.LabelField("Fixed Update Execution duration: ", systems.fixedUpdateExecutionDuration.ToString() + " ms");
                _fixedUpdateSystemsMonitor.Draw(_fixedUpdateSystemMonitorData.ToArray(), 80f);
            }
            CustomEditorLayout.EndVertical();
        }

        void addDuration(Queue<float> systemMontiorData, float duration) {
            // OnInspectorGUI is called twice per frame - only add duration once
            if (Time.renderedFrameCount != _lastRenderedFrameCount) {
                if (systemMontiorData.Count >= SYSTEM_MONITOR_DATA_LENGTH) {
                    systemMontiorData.Dequeue();
                }
                systemMontiorData.Enqueue(duration);
            }
        }

        private void drawSystemList(DebugSystems system) {
            CustomEditorLayout.BeginVerticalBox();
            {
                CustomEditorLayout.BeginHorizontal();
                {
                    DebugSystems.avgResetInterval = (AvgResetInterval)EditorGUILayout.EnumPopup("Reset average duration Ø", DebugSystems.avgResetInterval);
                    if (GUILayout.Button("Reset Ø now", GUILayout.Width(88), GUILayout.Height(14))) {
                        system.ResetDuration();
                    }
                }
                CustomEditorLayout.EndHorizontal();

                _threshold = EditorGUILayout.Slider("Threshold Ø ms", _threshold, 0f, 33f);
                _systemSortMethod = (SortMethod)EditorGUILayout.EnumPopup("Sort by ", _systemSortMethod);
                _hideEmptySystems = EditorGUILayout.Toggle("Hide empty systems", _hideEmptySystems);
                EditorGUILayout.Space();

                _systemNameSearchString = CustomEditorLayout.SearchTextField(_systemNameSearchString);
                EditorGUILayout.Space();

                _showStartSystems = CustomEditorLayout.Foldout(_showStartSystems, "OnStart Systems");
                if (_showStartSystems && shouldShowSystems(system, start: true)) {
                    CustomEditorLayout.BeginVerticalBox();
                    {
                        var systemsDrawn = drawSystemInfos(system, false,  start: true);
                        if (systemsDrawn == 0) {
                            EditorGUILayout.LabelField(string.Empty);
                        }
                    }
                    CustomEditorLayout.EndVertical();
                }

                _showUpdateSystems = CustomEditorLayout.Foldout(_showUpdateSystems, "OnUpdate Systems");
                if (_showUpdateSystems && shouldShowSystems(system, update:true)) {
                    CustomEditorLayout.BeginVerticalBox();
                    {
                        var systemsDrawn = drawSystemInfos(system, false, update: true);
                        if (systemsDrawn == 0) {
                            EditorGUILayout.LabelField(string.Empty);
                        }
                    }
                    CustomEditorLayout.EndVertical();
                }

                _showFixedUpdateSystems = CustomEditorLayout.Foldout(_showFixedUpdateSystems, "OnFixedUpdate Systems");
                if (_showFixedUpdateSystems && shouldShowSystems(system, fixedUpdate:true)) {
                    CustomEditorLayout.BeginVerticalBox();
                    {
                        var systemsDrawn = drawSystemInfos(system, false, fixedUpdate: true);
                        if (systemsDrawn == 0) {
                            EditorGUILayout.LabelField(string.Empty);
                        }
                    }
                    CustomEditorLayout.EndVertical();
                }                
            }
            CustomEditorLayout.EndVertical();
        }

        private bool shouldShowSystems(DebugSystems system, bool start = false, bool update = false, bool fixedUpdate = false) {
            if (!_hideEmptySystems) {
                return true;
            }

            if (start) {
                return system.totalOnStartSystemsCount > 0;
            }

            if (update) {
                return system.totalOnUpdateSystemsCount > 0;
            }

            if (fixedUpdate) {
                return system.totalOnFixedUpdateSystemsCount > 0;
            }
            return true;
        }
        int drawSystemInfos(DebugSystems systems, bool isChildSystem, bool start = false, bool update = false, bool fixedUpdate = false) {
            SystemInfo[] systemInfos = null;

            var drawExecutionDuration = false;
            if (start) {
                systemInfos = systems.startSystemInfos;
            }
            if (update ) {
                systemInfos = systems.updateSystemInfos;
                drawExecutionDuration = true;
            }
            if (fixedUpdate ) {
                systemInfos = systems.fixedUpdateSystemInfos;
                drawExecutionDuration = true;
            }

            systemInfos = systemInfos
                .Where(systemInfo => systemInfo.AverageExecutionDuration >= _threshold)
                .ToArray();

            systemInfos = getSortedSystemInfos(systemInfos, _systemSortMethod);

            var systemsDrawn = 0;
            foreach (var systemInfo in systemInfos) {
                var debugSystems = systemInfo.system as DebugSystems;
                if (debugSystems != null) {
                    if (!shouldShowSystems(debugSystems, start, update, fixedUpdate)) {
                        continue;
                    }
                }

                //if (!shouldShowSystems(systems, start, update, fixedUpdate)) {
                //    continue;
                //}
                

                if (CustomEditorLayout.MatchesSearchString(systemInfo.systemName.ToLower(), _systemNameSearchString.ToLower())) {
                    CustomEditorLayout.BeginHorizontal();
                    {
                        EditorGUI.BeginDisabledGroup(isChildSystem);
                        {
                            systemInfo.isActive = EditorGUILayout.Toggle(systemInfo.isActive, GUILayout.Width(20));
                        }
                        EditorGUI.EndDisabledGroup();

                        if (drawExecutionDuration) {
                            var avg = string.Format("Ø {0:00.000 ms}", systemInfo.AverageExecutionDuration).PadRight(12);
                            var min = string.Format("▼ {0:00.000 ms}", systemInfo.MinExecutionDuration).PadRight(12);
                            var max = string.Format("▲ {0:00.000 ms}", systemInfo.MaxExecutionDuration);
                            EditorGUILayout.LabelField(systemInfo.systemName, avg + min + max, getSystemStyle(systemInfo));
                        } else {
                            EditorGUILayout.LabelField(systemInfo.systemName, getSystemStyle(systemInfo));
                        }
                    }
                    CustomEditorLayout.EndHorizontal();

                    systemsDrawn += 1;
                }

                var debugSystem = systemInfo.system as DebugSystems;
                if (debugSystem != null) {
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel += 1;
                    systemsDrawn += drawSystemInfos(debugSystem, true, start, update, fixedUpdate);
                    EditorGUI.indentLevel = indent;
                }
            }

            return systemsDrawn;
        }

        static GUIStyle getSystemStyle(SystemInfo systemInfo) {
            var style = new GUIStyle(GUI.skin.label);
            var color = EditorGUIUtility.isProSkin
                            ? Color.white
                            : style.normal.textColor;

            style.normal.textColor = color;
            return style;
        }

        static SystemInfo[] getSortedSystemInfos(SystemInfo[] systemInfos, SortMethod sortMethod) {
            if (sortMethod == SortMethod.Name) {
                return systemInfos
                    .OrderBy(systemInfo => systemInfo.systemName)
                    .ToArray();
            }

            if (sortMethod == SortMethod.NameDescending) {
                return systemInfos
                    .OrderByDescending(systemInfo => systemInfo.systemName)
                    .ToArray();
            }

            if (sortMethod == SortMethod.ExecutionTime) {
                return systemInfos
                    .OrderBy(systemInfo => systemInfo.AverageExecutionDuration)
                    .ToArray();
            }

            if (sortMethod == SortMethod.ExecutionTimeDescending) {
                return systemInfos
                    .OrderByDescending(systemInfo => systemInfo.AverageExecutionDuration)
                    .ToArray();
            }

            return systemInfos;
        }
    }
}
