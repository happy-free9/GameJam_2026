using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureDiningRoomToZiaGuide
{
    private const string MenuPath = "Tools/Hotel Hunger/Configure Dining Room To Zia Guide";
    private const string UndoName = "Configure Dining Room To Zia Guide";
    private const string DiningRoomSceneName = "Guest3DiningRoom_XW";
    private const string DiningRoomScenePath = "Assets/Scenes/Guest3DiningRoom_XW.unity";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu_XW.unity";
    private const string FirstZiaSceneName = "Pov1_Guest3_Guide_Task1_WetFloorSign";

    private static readonly string[] ZiaScenePaths =
    {
        "Assets/Scenes/Pov1_Guest3_Guide_Task1_WetFloorSign.unity",
        "Assets/Scenes/Pov1_Guest3_Guide_Task2_PottedPlant.unity",
        "Assets/Scenes/Pov1_Guest3_Guide_Task3_LuggageCart.unity",
        "Assets/Scenes/Guest3_Final_Chasing.unity"
    };

    [MenuItem(MenuPath)]
    private static void Configure()
    {
        if (!VerifyZiaScenesExist())
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != DiningRoomSceneName)
        {
            EditorUtility.DisplayDialog(
                "Configure Dining Room To Zia Guide",
                $"Open {DiningRoomSceneName} before running this utility.",
                "OK");
            return;
        }

        DiningRoomChaseCutscene cutscene = FindCutsceneInScene(scene);
        if (cutscene == null)
        {
            EditorUtility.DisplayDialog(
                "Configure Dining Room To Zia Guide",
                $"No DiningRoomChaseCutscene component was found in {DiningRoomSceneName}.",
                "OK");
            return;
        }

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            ConfigureCutscene(cutscene);
            ConfigureBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log(
                "Dining Room now points to Pov1_Guest3_Guide_Task1_WetFloorSign. The scene was marked dirty; save it manually.",
                cutscene);
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }

        EditorUtility.DisplayDialog(
            "Configure Dining Room To Zia Guide",
            "Configuration complete. Save Guest3DiningRoom_XW to keep the cutscene handoff.",
            "OK");
    }

    private static bool VerifyZiaScenesExist()
    {
        for (int i = 0; i < ZiaScenePaths.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ZiaScenePaths[i]) != null)
            {
                continue;
            }

            EditorUtility.DisplayDialog(
                "Configure Dining Room To Zia Guide",
                $"Missing required scene:\n{ZiaScenePaths[i]}",
                "OK");
            return false;
        }

        return true;
    }

    private static DiningRoomChaseCutscene FindCutsceneInScene(Scene scene)
    {
        DiningRoomChaseCutscene[] cutscenes = Object.FindObjectsByType<DiningRoomChaseCutscene>(
            FindObjectsInactive.Include);

        for (int i = 0; i < cutscenes.Length; i++)
        {
            if (cutscenes[i] != null && cutscenes[i].gameObject.scene == scene)
            {
                return cutscenes[i];
            }
        }

        return null;
    }

    private static void ConfigureCutscene(DiningRoomChaseCutscene cutscene)
    {
        Undo.RecordObject(cutscene, UndoName);

        SerializedObject serializedCutscene = new SerializedObject(cutscene);
        SetBool(serializedCutscene, "loadNextSceneOnComplete", true);
        SetString(serializedCutscene, "nextSceneName", FirstZiaSceneName);
        SetString(serializedCutscene, "nextSpawnId", string.Empty);
        serializedCutscene.ApplyModifiedProperties();

        EditorUtility.SetDirty(cutscene);
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Configure Dining Room To Zia Guide: missing serialized bool '{propertyName}'.");
            return;
        }

        property.boolValue = value;
    }

    private static void SetString(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Configure Dining Room To Zia Guide: missing serialized string '{propertyName}'.");
            return;
        }

        property.stringValue = value;
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
        List<EditorBuildSettingsScene> rebuiltScenes = new List<EditorBuildSettingsScene>();

        EditorBuildSettingsScene mainMenuScene = FindScene(originalScenes, MainMenuScenePath);
        if (mainMenuScene != null || AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath) != null)
        {
            rebuiltScenes.Add(new EditorBuildSettingsScene(MainMenuScenePath, true));
        }

        for (int i = 0; i < originalScenes.Length; i++)
        {
            EditorBuildSettingsScene scene = originalScenes[i];
            string normalizedPath = NormalizeScenePath(scene.path);
            if (normalizedPath == MainMenuScenePath || IsZiaScenePath(normalizedPath))
            {
                continue;
            }

            rebuiltScenes.Add(new EditorBuildSettingsScene(scene.path, scene.enabled));
        }

        int insertIndex = FindSceneIndex(rebuiltScenes, DiningRoomScenePath);
        if (insertIndex < 0)
        {
            Debug.LogWarning(
                $"Configure Dining Room To Zia Guide: {DiningRoomScenePath} was not found in Build Settings. Appending Zia scenes.");
            insertIndex = rebuiltScenes.Count - 1;
        }

        for (int i = 0; i < ZiaScenePaths.Length; i++)
        {
            rebuiltScenes.Insert(insertIndex + 1 + i, new EditorBuildSettingsScene(ZiaScenePaths[i], true));
        }

        EditorBuildSettings.scenes = rebuiltScenes.ToArray();
    }

    private static EditorBuildSettingsScene FindScene(EditorBuildSettingsScene[] scenes, string path)
    {
        string normalizedTarget = NormalizeScenePath(path);
        for (int i = 0; i < scenes.Length; i++)
        {
            if (NormalizeScenePath(scenes[i].path) == normalizedTarget)
            {
                return scenes[i];
            }
        }

        return null;
    }

    private static int FindSceneIndex(List<EditorBuildSettingsScene> scenes, string path)
    {
        string normalizedTarget = NormalizeScenePath(path);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (NormalizeScenePath(scenes[i].path) == normalizedTarget)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsZiaScenePath(string path)
    {
        string normalizedPath = NormalizeScenePath(path);
        for (int i = 0; i < ZiaScenePaths.Length; i++)
        {
            if (NormalizeScenePath(ZiaScenePaths[i]) == normalizedPath)
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeScenePath(string path)
    {
        return path.Replace('\\', '/');
    }
}
