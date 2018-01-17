using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ECSSetup : Editor {
#if UNITY_EDITOR_WIN
    private static string unityInstallDir = string.Empty;
    private const string unityTemplateDir = @"Editor\Data\Resources\ScriptTemplates";
    private const string unityRegistryKey = @"Software\Unity Technologies\Installer\Unity\";
    private const string unityRegistryValue = @"Location x64";
#elif UNITY_EDITOR_OSX
    private const string unityInstallDir = @"/Applications/Unity/Unity.app/Contents";
    private const string unityTemplateDir = @"Resources/ScriptTemplates";
#else
    private const string unityInstallDir = @"/opt/Unity";
    private const string unityTemplateDir = @"Editor/Data/Resources/ScriptTemplates";
#endif
    private static bool enableTemplateValidation = true;

    private static string TemplatePath {
        get {
            #if UNITY_EDITOR_WIN
            if (string.IsNullOrEmpty(unityInstallDir)) {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(unityRegistryKey, false);
                unityInstallDir = (string)key.GetValue(unityRegistryValue);
            }
            #endif
            return Path.Combine(unityInstallDir, unityTemplateDir);
        }
    }

    static readonly string[] files = {
            "91-ECS__Wrapped Component Class-NewComponent.cs",
            "91-ECS__Component Class-NewComponent.cs",
            "91-ECS__Wrapped Component Struct-NewComponent.cs",
            "91-ECS__Component Struct-NewComponent.cs",
            "91-ECS__System-NewSystem.cs",
            "91-ECS__ECSController-NewECSController.cs",
        };

    [MenuItem("BrokenBricks/ECS/Install Templates", isValidateFunction: true)]
    private static bool ValidateInstallTemplates() {
        if (!enableTemplateValidation)
            return true;

        // Only allow installation if *none* of the files exist
        if (files.All(s => !File.Exists(Path.Combine(TemplatePath, s + ".txt"))) && CanCreateFileInTemplates())
            return true;

        return false;
    }

    [MenuItem("BrokenBricks/ECS/Uninstall Templates", isValidateFunction: true)]
    private static bool ValidateUninstallTemplates() {
        if (!enableTemplateValidation)
            return true;

        // To make sure we can clean up properly, check if *any* of the template files exist
        if (files.Any(s => File.Exists(Path.Combine(TemplatePath, s + ".txt"))) && CanCreateFileInTemplates())
            return true;

        return false;
    }

    private static bool CanCreateFileInTemplates() {
        try {
            var testfile = Path.Combine(TemplatePath, ".test");
            var stream = File.Create(testfile);
            stream.Close();
            File.Delete(testfile);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    [MenuItem("BrokenBricks/ECS/Install Templates")]
    private static void InstallTemplates() {
        try {
            var unityDir = TemplatePath;
            if(!Directory.Exists(unityDir))
            {
				EditorUtility.DisplayDialog("ECS Installation", "Setup not supported for your OS!\n Please copy the template folder content manually to:\n<UnityInstall>/" + unityTemplateDir + "\n\n and restart unity!", "ok");           
                return;
            }

            foreach (var file in files) {
                var assetGUID = AssetDatabase.FindAssets(file)[0];
                var filePath = AssetDatabase.GUIDToAssetPath(assetGUID);
                File.Copy(Path.Combine(Directory.GetParent(Application.dataPath).FullName, filePath), Path.Combine(unityDir, Path.GetFileName(filePath)), true);
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

    [MenuItem("BrokenBricks/ECS/Uninstall Templates")]
    private static void UninstallECS() {
        string unityDir = TemplatePath;
        if(!Directory.Exists(unityDir))
        {
            EditorUtility.DisplayDialog("ECS Uninstallation", "Uninstallation not supported for your OS!\n Please delete the template folder content manually from:\n<Unityinstall>/"+ unityTemplateDir + "\n\n and restart unity!", "ok");           
            return;
        }

        try {
            foreach (var file in files)
            {
                File.Delete(Path.Combine(unityDir, file + ".txt"));
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


    [MenuItem("BrokenBricks/ECS/Enable Visual Debugging")]
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

    [MenuItem("BrokenBricks/ECS/Enable Visual Debugging", isValidateFunction:true)]
    private static bool ValidateEnableVisualDebugging() {
       return !PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains("ECS_DEBUG");
    }

    [MenuItem("BrokenBricks/ECS/Disable Visual Debugging")]
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

    [MenuItem("BrokenBricks/ECS/Disable Visual Debugging", isValidateFunction: true)]
    private static bool ValidateDisableVisualDebugging() {
        return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains("ECS_DEBUG");
    }
    
}
