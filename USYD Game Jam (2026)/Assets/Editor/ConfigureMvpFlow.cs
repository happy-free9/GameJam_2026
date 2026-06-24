using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConfigureMvpFlow
{
    private const string MenuPath = "Tools/Hotel Hunger/Configure MVP Flow";
    private const string UndoName = "Configure MVP Flow";

    private static readonly SceneSpec[] MvpScenes =
    {
        new("HotelLobby_XW", "Assets/Scenes/HotelLobby_XW.unity"),
        new("Guest1CartHallway_XW", "Assets/Scenes/Guest1CartHallway_XW.unity"),
        new("Guest1WaitingHallway_XW", "Assets/Scenes/Guest1WaitingHallway_XW.unity"),
        new("Guest3DiningRoom_XW", "Assets/Scenes/Guest3DiningRoom_XW.unity"),
        new("Guest3HorrorChase_XW", "Assets/Scenes/Guest3HorrorChase_XW.unity"),
        new("GoodEnding_XW", "Assets/Scenes/GoodEnding_XW.unity"),
        new("BadEnding_XW", "Assets/Scenes/BadEnding_XW.unity")
    };

    private static readonly string[] PlayableScenes =
    {
        "HotelLobby_XW",
        "Guest1CartHallway_XW",
        "Guest1WaitingHallway_XW",
        "Guest3DiningRoom_XW",
        "Guest3HorrorChase_XW"
    };

    private static readonly SpawnSpec[] SpawnPoints =
    {
        new("Guest1CartHallway_XW", "Spawn_FromHotelLobby", "FromHotelLobby"),
        new("Guest1WaitingHallway_XW", "Spawn_FromCartHallway", "FromCartHallway"),
        new("Guest3DiningRoom_XW", "Spawn_FromWaitingHallway", "FromWaitingHallway"),
        new("Guest3HorrorChase_XW", "Spawn_FromDiningRoom", "FromDiningRoom"),
        new("GoodEnding_XW", "Spawn_FromHorrorChase", "FromHorrorChase"),
        new("BadEnding_XW", "Spawn_FromHorrorChase", "FromHorrorChase")
    };

    private static readonly TransitionZoneSpec[] TransitionZones =
    {
        new("HotelLobby_XW", "ElevatorInteractionZone", "Guest1CartHallway_XW", "FromHotelLobby", "Enter elevator"),
        new("Guest1CartHallway_XW", "DepartureCartInteractionZone", "Guest1WaitingHallway_XW", "FromCartHallway", "Choose Departure Cart"),
        new("Guest1WaitingHallway_XW", "WaitingRoomInteractionZone", "Guest3DiningRoom_XW", "FromWaitingHallway", "Enter waiting room"),
        new("Guest3DiningRoom_XW", "DiningExitInteractionZone", "Guest3HorrorChase_XW", "FromDiningRoom", "Leave dining room"),
        new("Guest3HorrorChase_XW", "EscapeExitZone", "GoodEnding_XW", "FromHorrorChase", "Escape")
    };

    private static readonly InteractionZoneSpec[] NormalInteractionZones =
    {
        new("HotelLobby_XW", "Guest1InteractionZone", "Talk to guest"),
        new("HotelLobby_XW", "GlassInteractionZone", "Inspect glass"),
        new("HotelLobby_XW", "GoldStrawInteractionZone", "Inspect gold straw"),
        new("HotelLobby_XW", "HouseSpecialSyrupInteractionZone", "Inspect syrup"),
        new("HotelLobby_XW", "UmbrellaInteractionZone", "Inspect umbrella"),
        new("Guest1CartHallway_XW", "ArrivalCartInteractionZone", "Inspect Arrival Cart"),
        new("Guest1CartHallway_XW", "RoomServiceCartInteractionZone", "Inspect Room Service Cart"),
        new("Guest1CartHallway_XW", "ElevatorInteractionZone", "Call elevator"),
        new("Guest1WaitingHallway_XW", "PrivateElevatorInteractionZone", "Inspect private elevator"),
        new("Guest1WaitingHallway_XW", "Guest1LuggageInteractionZone", "Inspect luggage"),
        new("Guest3DiningRoom_XW", "WaitingRoomEntranceInteractionZone", "Inspect entrance"),
        new("Guest3DiningRoom_XW", "StaffOnlyDoorInteractionZone", "Inspect staff door")
    };

    [MenuItem(MenuPath)]
    private static void Configure()
    {
        Debug.Log("CONFIGURE MVP FLOW V2 STARTED");

        if (!ValidateSceneAssets())
        {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("Configure MVP Flow was cancelled before opening project scenes.");
            return;
        }

        string originalScenePath = SceneManager.GetActiveScene().path;
        string originalSceneName = SceneManager.GetActiveScene().name;

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        List<string> modifiedScenes = new();
        List<string> warnings = new();
        List<string> verificationFailures = new();
        List<string> enabledBuildSceneOrder = new();

        try
        {
            ConfigureBuildSettings();

            for (int i = 0; i < MvpScenes.Length; i++)
            {
                SceneSpec sceneSpec = MvpScenes[i];
                Scene scene = EditorSceneManager.OpenScene(sceneSpec.Path, OpenSceneMode.Single);

                bool modified = false;
                modified |= ConfigurePlayerIfNeeded(scene, sceneSpec.Name, warnings);
                modified |= ConfigureSpawnPointIfNeeded(scene, sceneSpec.Name, warnings);
                modified |= ConfigureUiIfNeeded(scene, sceneSpec.Name);
                modified |= ConfigureTransitionZones(scene, sceneSpec.Name, warnings);
                modified |= ConfigureNormalInteractionZones(scene, sceneSpec.Name, warnings);

                if (modified)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    modifiedScenes.Add(sceneSpec.Name);
                }
            }

            verificationFailures = VerifyConfiguredFlow(out enabledBuildSceneOrder);
        }
        finally
        {
            RestoreOriginalScene(originalScenePath, originalSceneName);
            Undo.CollapseUndoOperations(undoGroup);
        }

        ReportResult(modifiedScenes, warnings, verificationFailures, enabledBuildSceneOrder);
    }

    private static bool ValidateSceneAssets()
    {
        List<string> missingScenes = new();
        for (int i = 0; i < MvpScenes.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MvpScenes[i].Path) == null)
            {
                missingScenes.Add(MvpScenes[i].Name);
            }
        }

        if (missingScenes.Count == 0)
        {
            return true;
        }

        string message = "Configure MVP Flow cannot run because these scenes are missing:\n" + string.Join("\n", missingScenes);
        EditorUtility.DisplayDialog("Configure MVP Flow", message, "OK");
        Debug.LogWarning(message);
        return false;
    }

    private static void ConfigureBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new();
        HashSet<string> mvpScenePaths = new();

        for (int i = 0; i < MvpScenes.Length; i++)
        {
            scenes.Add(new EditorBuildSettingsScene(MvpScenes[i].Path, true));
            mvpScenePaths.Add(NormalizeAssetPath(MvpScenes[i].Path));
        }

        EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < existingScenes.Length; i++)
        {
            string existingPath = NormalizeAssetPath(existingScenes[i].path);
            if (!mvpScenePaths.Contains(existingPath))
            {
                scenes.Add(existingScenes[i]);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static List<string> VerifyConfiguredFlow(out List<string> enabledBuildSceneOrder)
    {
        List<string> failures = new();
        enabledBuildSceneOrder = new List<string>();

        for (int i = 0; i < TransitionZones.Length; i++)
        {
            TransitionZoneSpec transitionSpec = TransitionZones[i];
            SceneSpec sceneSpec = FindSceneSpec(transitionSpec.SceneName);
            if (sceneSpec.Name == null)
            {
                failures.Add($"{transitionSpec.SceneName}/{transitionSpec.ZoneName}: scene is not registered in the MVP scene list.");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(sceneSpec.Path, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                failures.Add($"{transitionSpec.SceneName}/{transitionSpec.ZoneName}: scene could not be opened for verification.");
                continue;
            }

            VerifyTransitionZone(scene, transitionSpec, failures);
        }

        VerifyBuildSettings(failures, enabledBuildSceneOrder);
        return failures;
    }

    private static void VerifyTransitionZone(Scene scene, TransitionZoneSpec spec, List<string> failures)
    {
        string context = $"{spec.SceneName}/{spec.ZoneName}";
        GameObject zone = FindSceneObject(scene, spec.ZoneName);
        if (zone == null)
        {
            failures.Add($"{context}: transition zone was not found.");
            return;
        }

        SceneTransitionTrigger[] transitions = zone.GetComponents<SceneTransitionTrigger>();
        if (transitions.Length != 1)
        {
            failures.Add($"{context}: expected exactly one SceneTransitionTrigger, found {transitions.Length}.");
        }
        else
        {
            string targetSceneName = ReadTransitionString(transitions[0], "targetSceneName");
            string targetSpawnPointId = ReadTransitionString(transitions[0], "targetSpawnPointId");

            if (targetSceneName != spec.TargetSceneName)
            {
                failures.Add($"{context}: target scene expected '{spec.TargetSceneName}', found '{targetSceneName}'.");
            }

            if (targetSpawnPointId != spec.TargetSpawnPointId)
            {
                failures.Add($"{context}: target spawn point id expected '{spec.TargetSpawnPointId}', found '{targetSpawnPointId}'.");
            }
        }

        InteractionTarget[] interactionTargets = zone.GetComponents<InteractionTarget>();
        if (interactionTargets.Length != 0)
        {
            failures.Add($"{context}: expected no InteractionTarget on the transition zone, found {interactionTargets.Length}.");
        }

        BoxCollider2D collider = zone.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            failures.Add($"{context}: missing BoxCollider2D.");
        }
        else if (!collider.isTrigger)
        {
            failures.Add($"{context}: BoxCollider2D Is Trigger is disabled.");
        }
    }

    private static void VerifyBuildSettings(List<string> failures, List<string> enabledBuildSceneOrder)
    {
        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
        List<string> enabledBuildScenePaths = new();

        for (int i = 0; i < buildScenes.Length; i++)
        {
            if (!buildScenes[i].enabled)
            {
                continue;
            }

            string path = NormalizeAssetPath(buildScenes[i].path);
            enabledBuildScenePaths.Add(path);
            enabledBuildSceneOrder.Add(SceneNameFromPath(path));
        }

        for (int i = 0; i < MvpScenes.Length; i++)
        {
            if (enabledBuildScenePaths.Count <= i)
            {
                failures.Add($"Build Settings: missing enabled scene at index {i}; expected {MvpScenes[i].Name}.");
                continue;
            }

            string expectedPath = NormalizeAssetPath(MvpScenes[i].Path);
            string actualPath = enabledBuildScenePaths[i];
            if (actualPath != expectedPath)
            {
                failures.Add($"Build Settings: enabled scene index {i} expected {MvpScenes[i].Name} ({expectedPath}), found {SceneNameFromPath(actualPath)} ({actualPath}).");
            }
        }
    }

    private static bool ConfigurePlayerIfNeeded(Scene scene, string sceneName, List<string> warnings)
    {
        if (!Contains(PlayableScenes, sceneName))
        {
            return false;
        }

        GameObject player = FindSceneObject(scene, "Player");
        if (player == null)
        {
            warnings.Add($"{sceneName}: Player was not found; PlayerInteractor was not configured.");
            return false;
        }

        bool modified = false;

        if (!player.CompareTag("Player"))
        {
            Undo.RecordObject(player, UndoName);
            player.tag = "Player";
            modified = true;
        }

        PlayerInteractor[] interactors = player.GetComponents<PlayerInteractor>();
        if (interactors.Length == 0)
        {
            if (player.GetComponent<Collider2D>() == null)
            {
                warnings.Add($"{sceneName}: Player has no Collider2D; PlayerInteractor was not added to avoid changing Player collider setup.");
                return modified;
            }

            Undo.AddComponent<PlayerInteractor>(player);
            modified = true;
        }
        else if (interactors.Length > 1)
        {
            for (int i = 1; i < interactors.Length; i++)
            {
                Undo.DestroyObjectImmediate(interactors[i]);
                modified = true;
            }
        }

        return modified;
    }

    private static bool ConfigureSpawnPointIfNeeded(Scene scene, string sceneName, List<string> warnings)
    {
        SpawnSpec spawnSpec = FindSpawnSpec(sceneName);
        if (spawnSpec.SceneName == null)
        {
            return false;
        }

        GameObject player = FindSceneObject(scene, "Player");
        if (player == null)
        {
            warnings.Add($"{sceneName}: Player was not found; {spawnSpec.ObjectName} was not configured.");
            return false;
        }

        bool modified = false;
        GameObject spawnObject = FindSceneObject(scene, spawnSpec.ObjectName);
        if (spawnObject == null)
        {
            spawnObject = new GameObject(spawnSpec.ObjectName);
            SceneManager.MoveGameObjectToScene(spawnObject, scene);
            Undo.RegisterCreatedObjectUndo(spawnObject, UndoName);
            modified = true;
        }

        modified |= ConfigureTransform(spawnObject.transform, player.transform.position, Quaternion.identity, Vector3.one, false);
        modified |= RemoveComponentsIfPresent<SpriteRenderer>(spawnObject);
        modified |= RemoveComponentsIfPresent<Collider2D>(spawnObject);
        modified |= RemoveComponentsIfPresent<Collider>(spawnObject);

        SpawnPoint spawnPoint = EnsureSingleComponent<SpawnPoint>(spawnObject, ref modified);
        string previousSpawnId = spawnPoint.SpawnPointId;
        if (previousSpawnId != spawnSpec.SpawnPointId)
        {
            Undo.RecordObject(spawnPoint, UndoName);
            spawnPoint.SetSpawnPointId(spawnSpec.SpawnPointId);
            modified = true;
        }

        return modified;
    }

    private static bool ConfigureUiIfNeeded(Scene scene, string sceneName)
    {
        if (sceneName != "HotelLobby_XW")
        {
            return false;
        }

        bool modified = false;
        GameObject uiRoot = FindSceneObject(scene, "CoreUIRoot");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("CoreUIRoot");
            SceneManager.MoveGameObjectToScene(uiRoot, scene);
            Undo.RegisterCreatedObjectUndo(uiRoot, UndoName);
            modified = true;
        }

        EnsureSingleComponent<CoreUIRoot>(uiRoot, ref modified);
        return modified;
    }

    private static bool ConfigureTransitionZones(Scene scene, string sceneName, List<string> warnings)
    {
        bool modified = false;
        for (int i = 0; i < TransitionZones.Length; i++)
        {
            TransitionZoneSpec spec = TransitionZones[i];
            if (spec.SceneName != sceneName)
            {
                continue;
            }

            GameObject zone = FindSceneObject(scene, spec.ZoneName);
            if (zone == null)
            {
                warnings.Add($"{sceneName}: transition zone {spec.ZoneName} was not found.");
                continue;
            }

            modified |= EnsureTriggerCollider(zone);
            modified |= RemoveComponentsIfPresent<InteractionTarget>(zone);

            SceneTransitionTrigger transition = EnsureSingleComponent<SceneTransitionTrigger>(zone, ref modified);
            modified |= ConfigureSceneTransitionTrigger(transition, spec.TargetSceneName, spec.TargetSpawnPointId, spec.PromptText);
        }

        return modified;
    }

    private static bool ConfigureNormalInteractionZones(Scene scene, string sceneName, List<string> warnings)
    {
        bool modified = false;
        for (int i = 0; i < NormalInteractionZones.Length; i++)
        {
            InteractionZoneSpec spec = NormalInteractionZones[i];
            if (spec.SceneName != sceneName)
            {
                continue;
            }

            GameObject zone = FindSceneObject(scene, spec.ZoneName);
            if (zone == null)
            {
                warnings.Add($"{sceneName}: normal interaction zone {spec.ZoneName} was not found.");
                continue;
            }

            modified |= EnsureTriggerCollider(zone);
            modified |= RemoveComponentsIfPresent<SceneTransitionTrigger>(zone);

            InteractionTarget interactionTarget = EnsureSingleComponent<InteractionTarget>(zone, ref modified);
            modified |= ConfigureInteractionTarget(interactionTarget, spec.PromptText);
        }

        return modified;
    }

    private static bool ConfigureInteractionTarget(InteractionTarget interactionTarget, string promptText)
    {
        bool modified = false;
        SerializedObject serializedObject = new(interactionTarget);
        SerializedProperty activationMode = serializedObject.FindProperty("activationMode");
        SerializedProperty prompt = serializedObject.FindProperty("promptText");
        SerializedProperty disableAfterInteract = serializedObject.FindProperty("disableAfterInteract");
        SerializedProperty onInteract = serializedObject.FindProperty("onInteract");

        if (activationMode != null && activationMode.enumValueIndex != 0)
        {
            activationMode.enumValueIndex = 0;
            modified = true;
        }

        if (prompt != null && prompt.stringValue != promptText)
        {
            prompt.stringValue = promptText;
            modified = true;
        }

        if (disableAfterInteract != null && disableAfterInteract.boolValue)
        {
            disableAfterInteract.boolValue = false;
            modified = true;
        }

        SerializedProperty calls = onInteract?.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (calls != null && calls.arraySize > 0)
        {
            calls.ClearArray();
            modified = true;
        }

        if (modified)
        {
            Undo.RecordObject(interactionTarget, UndoName);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(interactionTarget);
        }

        return modified;
    }

    private static bool ConfigureSceneTransitionTrigger(
        SceneTransitionTrigger transition,
        string targetSceneName,
        string targetSpawnPointId,
        string promptText)
    {
        string currentPrompt = ReadTransitionString(transition, "promptText");
        string currentSceneName = ReadTransitionString(transition, "targetSceneName");
        string currentSpawnPointId = ReadTransitionString(transition, "targetSpawnPointId");

        bool modified =
            currentPrompt != promptText ||
            currentSceneName != targetSceneName ||
            currentSpawnPointId != targetSpawnPointId;

        if (modified)
        {
            Undo.RecordObject(transition, UndoName);
        }

        transition.Configure(targetSceneName, targetSpawnPointId);
        transition.SetPromptText(promptText);

        if (modified)
        {
            EditorUtility.SetDirty(transition);
        }

        return modified;
    }

    private static string ReadTransitionString(SceneTransitionTrigger transition, string propertyName)
    {
        SerializedObject serializedObject = new(transition);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property == null ? null : property.stringValue;
    }

    private static bool EnsureTriggerCollider(GameObject gameObject)
    {
        bool modified = false;
        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(gameObject);
            modified = true;
        }

        if (!collider.isTrigger)
        {
            Undo.RecordObject(collider, UndoName);
            collider.isTrigger = true;
            modified = true;
        }

        return modified;
    }

    private static bool ConfigureTransform(Transform transform, Vector3 position, Quaternion rotation, Vector3 scale, bool useLocalPosition)
    {
        bool modified = false;
        Undo.RecordObject(transform, UndoName);

        if (useLocalPosition)
        {
            if (transform.localPosition != position)
            {
                transform.localPosition = position;
                modified = true;
            }
        }
        else if (transform.position != position)
        {
            transform.position = position;
            modified = true;
        }

        if (transform.localRotation != rotation)
        {
            transform.localRotation = rotation;
            modified = true;
        }

        if (transform.localScale != scale)
        {
            transform.localScale = scale;
            modified = true;
        }

        return modified;
    }

    private static T EnsureSingleComponent<T>(GameObject gameObject, ref bool modified) where T : Component
    {
        T[] components = gameObject.GetComponents<T>();
        if (components.Length == 0)
        {
            modified = true;
            return Undo.AddComponent<T>(gameObject);
        }

        for (int i = 1; i < components.Length; i++)
        {
            Undo.DestroyObjectImmediate(components[i]);
            modified = true;
        }

        return components[0];
    }

    private static bool RemoveComponentsIfPresent<T>(GameObject gameObject) where T : Component
    {
        bool modified = false;
        T[] components = gameObject.GetComponents<T>();
        for (int i = 0; i < components.Length; i++)
        {
            Undo.DestroyObjectImmediate(components[i]);
            modified = true;
        }

        return modified;
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Transform match = FindTransformByName(rootObject.transform, objectName);
            if (match != null)
            {
                return match.gameObject;
            }
        }

        return null;
    }

    private static Transform FindTransformByName(Transform root, string objectName)
    {
        if (root.name == objectName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform match = FindTransformByName(root.GetChild(i), objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static SpawnSpec FindSpawnSpec(string sceneName)
    {
        for (int i = 0; i < SpawnPoints.Length; i++)
        {
            if (SpawnPoints[i].SceneName == sceneName)
            {
                return SpawnPoints[i];
            }
        }

        return default;
    }

    private static SceneSpec FindSceneSpec(string sceneName)
    {
        for (int i = 0; i < MvpScenes.Length; i++)
        {
            if (MvpScenes[i].Name == sceneName)
            {
                return MvpScenes[i];
            }
        }

        return default;
    }

    private static bool Contains(string[] values, string value)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == value)
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeAssetPath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static string SceneNameFromPath(string path)
    {
        string normalizedPath = NormalizeAssetPath(path);
        int slashIndex = normalizedPath.LastIndexOf('/');
        string fileName = slashIndex >= 0 ? normalizedPath.Substring(slashIndex + 1) : normalizedPath;
        const string unityExtension = ".unity";
        return fileName.EndsWith(unityExtension) ? fileName.Substring(0, fileName.Length - unityExtension.Length) : fileName;
    }

    private static void RestoreOriginalScene(string originalScenePath, string originalSceneName)
    {
        if (!string.IsNullOrEmpty(originalScenePath) &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScenePath) != null)
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            return;
        }

        Debug.LogWarning($"Configure MVP Flow could not restore original scene '{originalSceneName}' because it has no valid saved path.");
    }

    private static void ReportResult(
        List<string> modifiedScenes,
        List<string> warnings,
        List<string> verificationFailures,
        List<string> enabledBuildSceneOrder)
    {
        string modifiedSceneText = modifiedScenes.Count == 0 ? "No scene changes were needed." : string.Join(", ", modifiedScenes);
        string warningText = warnings.Count == 0 ? "No warnings." : string.Join("\n", warnings);
        string enabledBuildSceneOrderText = enabledBuildSceneOrder.Count == 0 ? "No enabled scenes." : string.Join(", ", enabledBuildSceneOrder);
        string verificationText = verificationFailures.Count == 0
            ? "MVP FLOW VERIFY PASS"
            : "MVP FLOW VERIFY FAILED\n" + string.Join("\n", verificationFailures);

        string report =
            "Configure MVP Flow complete.\n" +
            $"Modified scenes: {modifiedSceneText}\n" +
            "Transitions: HotelLobby_XW/ElevatorInteractionZone -> Guest1CartHallway_XW, " +
            "Guest1CartHallway_XW/DepartureCartInteractionZone -> Guest1WaitingHallway_XW, " +
            "Guest1WaitingHallway_XW/WaitingRoomInteractionZone -> Guest3DiningRoom_XW, " +
            "Guest3DiningRoom_XW/DiningExitInteractionZone -> Guest3HorrorChase_XW, " +
            "Guest3HorrorChase_XW/EscapeExitZone -> GoodEnding_XW.\n" +
            $"Enabled Build Settings scene order: {enabledBuildSceneOrderText}\n" +
            $"Warnings:\n{warningText}\n" +
            verificationText;

        if (verificationFailures.Count == 0)
        {
            Debug.Log(report);
            Debug.Log("MVP FLOW VERIFY PASS");
        }
        else
        {
            Debug.LogError(report);
            Debug.LogError("MVP FLOW VERIFY FAILED");
        }
    }

    private readonly struct SceneSpec
    {
        public readonly string Name;
        public readonly string Path;

        public SceneSpec(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }

    private readonly struct SpawnSpec
    {
        public readonly string SceneName;
        public readonly string ObjectName;
        public readonly string SpawnPointId;

        public SpawnSpec(string sceneName, string objectName, string spawnPointId)
        {
            SceneName = sceneName;
            ObjectName = objectName;
            SpawnPointId = spawnPointId;
        }
    }

    private readonly struct TransitionZoneSpec
    {
        public readonly string SceneName;
        public readonly string ZoneName;
        public readonly string TargetSceneName;
        public readonly string TargetSpawnPointId;
        public readonly string PromptText;

        public TransitionZoneSpec(string sceneName, string zoneName, string targetSceneName, string targetSpawnPointId, string promptText)
        {
            SceneName = sceneName;
            ZoneName = zoneName;
            TargetSceneName = targetSceneName;
            TargetSpawnPointId = targetSpawnPointId;
            PromptText = promptText;
        }
    }

    private readonly struct InteractionZoneSpec
    {
        public readonly string SceneName;
        public readonly string ZoneName;
        public readonly string PromptText;

        public InteractionZoneSpec(string sceneName, string zoneName, string promptText)
        {
            SceneName = sceneName;
            ZoneName = zoneName;
            PromptText = promptText;
        }
    }
}
