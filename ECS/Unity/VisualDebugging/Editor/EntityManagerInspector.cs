using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace ECS.VisualDebugging {

#if ECS_DEBUG
    [CustomEditor(typeof(DebugEntityManagerBehaviour))]
    public class EntityManagerInspector : Editor {
        private static Dictionary<Type, GUIStyle> _coloredBoxStyles = new Dictionary<Type, GUIStyle>();
        private static string _filteredComponents = "";

        public override void OnInspectorGUI() {
            var debugEntityManagerBehaviour = (DebugEntityManagerBehaviour)target;
            var entityManager = debugEntityManagerBehaviour.EntityManager;
            drawEntityOverview(entityManager);
            drawEntityList(entityManager);
            EditorUtility.SetDirty(target);
        }

        static void drawEntityOverview(UnityEntityManager entityManager) {
            CustomEditorLayout.BeginVerticalBox();
            {
                EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Entities", entityManager.TotalEntities.ToString());
                EditorGUILayout.LabelField("Component Types", entityManager.TotalComponentTypes.ToString());
            }
            CustomEditorLayout.EndVertical();
        }


        static void drawEntityList(UnityEntityManager entityManager) {
            CustomEditorLayout.BeginVerticalBox();
            EditorGUILayout.LabelField("Entity Inspector", EditorStyles.boldLabel);

            _filteredComponents = CustomEditorLayout.SearchTextField(_filteredComponents);
            IEnumerable<string> componentNames = _filteredComponents.Split('-', ' ', ';', ',')
                .Where(name=> !string.IsNullOrEmpty(name))
                .Select(name => name.ToLower());

            foreach (EntityInfo entityInfo in entityManager.EntityList) {

                IEnumerable<IComponent> components = entityManager.GetComponents(entityInfo);
                entityInfo.Name = "{" + string.Join("} - {", components.Select(x => x.GetType().Name).ToArray()) + "}";
                var entityName = entityInfo.Name.ToLower();
                if (componentNames.Any() && componentNames.Where(name => entityName.Contains(name)).Count() != componentNames.Count()) {
                    continue;
                }

                entityInfo.expanded = EditorGUILayout.BeginToggleGroup(entityInfo.Name, entityInfo.expanded);
                EditorGUI.indentLevel++;
                if (entityInfo.expanded) {
                    foreach (IComponent component in components) {
                        var style = GetColoredBoxStyle(component);
                        EditorGUILayout.BeginVertical(style, GUILayout.Height(50));
                        EditorGUILayout.LabelField(component.GetType().Name, EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndToggleGroup();
            }
            CustomEditorLayout.EndVertical();
        }
        
       static GUIStyle GetColoredBoxStyle(IComponent component) {
            if (_coloredBoxStyles.Count == 0) {
                Type iComponentType = typeof(IComponent);
                Type[] types = typeof(DebugEntityManagerBehaviour).Assembly.GetTypes()
                    .Where(type => iComponentType.IsAssignableFrom(type))
                    .ToArray();

                for (int i = 0; i < types.Length; i++) {
                    var hue = i / (float)types.Length;
                    var componentColor = Color.HSVToRGB(hue, 0.7f, 1f);
                    componentColor.a = 0.15f;
                    var style = new GUIStyle(GUI.skin.box);
                    style.normal.background = CreateTexture(2, 2, componentColor);
                    _coloredBoxStyles.Add(types[i], style);
                }
            }

            return _coloredBoxStyles[component.GetType()];
        }

        static Texture2D CreateTexture(int width, int height, Color color) {
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; ++i) {
                pixels[i] = color;
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
    }
#endif
}