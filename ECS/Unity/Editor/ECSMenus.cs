using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ECSSetup : Editor {

#if UNITY_EDITOR_WIN
    private const string unityTemplateDir = @"\Editor\Data\Resources\ScriptTemplates";
    private const string unityRegistryKey = @"Software\Unity Technologies\Installer\Unity\";
    private const string unityRegistryValue = @"Location x64";
#endif

    static readonly string[] files = {
            "91-ECS__Wrapped Component Class-NewComponent.cs",
            "91-ECS__Component Class-NewComponent.cs",
            "91-ECS__Wrapped Component Struct-NewComponent.cs",
            "91-ECS__Component Struct-NewComponent.cs",
            "91-ECS__System-NewSystem.cs",
            "91-ECS__SystemController-NewSystemController.cs",
        };


    [MenuItem("BrokenBricks/ECS/Setup")]
    private static void SetupECS() {
        try {

#if UNITY_EDITOR_WIN
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(unityRegistryKey, false);
            string unityInstallDir = (string)key.GetValue(unityRegistryValue);
            string destTempFolderPath = unityInstallDir + unityTemplateDir;
#elif UNITY_EDITOR_OSX
            string destTempFolderPath = @"/Applications/Unity/Unity.app/Contents/Resources/ScriptTemplates";
#else
            string destTempFolderPath =""
            EditorUtility.DisplayDialog("ECS Installation", "Setup not supported for your OS!\n Please copy the template folder content manualy to:\n"+ unityTemplateDir + "\n\n and restart unity!", "ok");           
            return;
#endif

            foreach (var file in files) {
                var assetGUID = AssetDatabase.FindAssets(file)[0];
                var filePath = AssetDatabase.GUIDToAssetPath(assetGUID);
                File.Copy(Path.Combine(Directory.GetParent(Application.dataPath).FullName, filePath), Path.Combine(destTempFolderPath, Path.GetFileName(filePath)), true);
                //var filePath = AssetDatabase.GetAssetPath();
            }
            EditorUtility.DisplayDialog("ECS Setup", "Installation completed!\nPlease restart unity to access all functionalities!", "ok");
        }
        catch (UnauthorizedAccessException)
        {
            EditorUtility.DisplayDialog("ECS Setup", "You need access privileges to the Unity install folder.\nStart Unity as Administrator and try again.", "ok");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("ECS Setup", "SSomething went wrong!\n\n Error:\n" + ex.Message, "ok");
        }
    }

    [MenuItem("BrokenBricks/ECS/Uninstall")]
    private static void UninstallECS() {
#if UNITY_EDITOR_WIN
        Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(unityRegistryKey, false);
        string unityInstallDir = (string)key.GetValue(unityRegistryValue);
#elif UNITY_EDITOR_OSX
        string unityInstallDir = @"\Applications\Unity";
#else
        string unityInstallDir =""
        EditorUtility.DisplayDialog("ECS Uninstallation", "Uninstallation not supported for your OS!\n Please delete the template folder content manualy from:\n"+ unityTemplateDir + "\n\n and restart unity!", "ok");           
        return;           
#endif

        string destTempFolderPath = unityInstallDir + unityTemplateDir;

        try
        {
            foreach (var file in files)
            {
                File.Delete(Path.Combine(destTempFolderPath, file + ".txt"));
            }
            EditorUtility.DisplayDialog("ECS Uninstallation", "Uninstallation completed!\nPlease restart unity!", "ok");
        }
        catch (UnauthorizedAccessException)
        {
            EditorUtility.DisplayDialog("ECS Setup", "You need access privileges to the Unity install folder.\nStart Unity as Administrator and try again.", "ok");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("ECS Setup", "SSomething went wrong!\n\n Error:\n" + ex.Message, "ok");
        }
    }


    [MenuItem("BrokenBricks/ECS/Enable Visual Debuggin")]
    private static void EnableVisualDebugging() {
        foreach (BuildTargetGroup buildTarget in Enum.GetValues(typeof(BuildTargetGroup))) {
            if (buildTarget == BuildTargetGroup.Unknown) {
                continue;
            }
            var scriptSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget).Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (!scriptSymbols.Contains("ECS_DEBUG")) {
                scriptSymbols.Add("ECS_DEBUG");
            }

            try {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", scriptSymbols.ToArray()));
            } catch { }
        }
    }

    [MenuItem("BrokenBricks/ECS/Enable Visual Debuggin", isValidateFunction:true)]
    private static bool ValidateEnableVisualDebugging() {
       return !PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains("ECS_DEBUG");
    }

    [MenuItem("BrokenBricks/ECS/Disable Visual Debuggin")]
    private static void DisableVisualDebugging() {
        foreach (BuildTargetGroup buildTarget in Enum.GetValues(typeof(BuildTargetGroup))) {
            if (buildTarget == BuildTargetGroup.Unknown) {
                continue;
            }
            var scriptSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget).Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();
            scriptSymbols.Remove("ECS_DEBUG");
            try {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", scriptSymbols.ToArray()));
            } catch { }
        }
    }

    [MenuItem("BrokenBricks/ECS/Disable Visual Debuggin", isValidateFunction: true)]
    private static bool ValidateDisableVisualDebugging() {
        return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains("ECS_DEBUG");
    }
}
