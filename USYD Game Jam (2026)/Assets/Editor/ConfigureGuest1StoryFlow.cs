using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class ConfigureGuest1StoryFlow
{
    private const string MenuPath = "Tools/Hotel Hunger/Configure Guest 1 Story Flow";
    private const string UndoName = "Configure Guest 1 Story Flow";

    private static readonly SceneSpec[] Guest1Scenes =
    {
        new("HotelLobby_XW", "Assets/Scenes/HotelLobby_XW.unity"),
        new("Guest1CartHallway_XW", "Assets/Scenes/Guest1CartHallway_XW.unity"),
        new("Guest1WaitingHallway_XW", "Assets/Scenes/Guest1WaitingHallway_XW.unity")
    };

    [MenuItem(MenuPath)]
    private static void Configure()
    {
        if (!ValidateSceneAssets())
        {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("Configure Guest 1 Story Flow was cancelled before opening project scenes.");
            return;
        }

        string originalScenePath = SceneManager.GetActiveScene().path;
        string originalSceneName = SceneManager.GetActiveScene().name;

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        List<string> modifiedScenes = new();
        List<string> warnings = new();

        try
        {
            for (int i = 0; i < Guest1Scenes.Length; i++)
            {
                SceneSpec sceneSpec = Guest1Scenes[i];
                Scene scene = EditorSceneManager.OpenScene(sceneSpec.Path, OpenSceneMode.Single);

                bool modified = sceneSpec.Name switch
                {
                    "HotelLobby_XW" => ConfigureHotelLobby(scene, warnings),
                    "Guest1CartHallway_XW" => ConfigureCartHallway(scene, warnings),
                    "Guest1WaitingHallway_XW" => ConfigureWaitingHallway(scene, warnings),
                    _ => false
                };

                if (modified)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    modifiedScenes.Add(sceneSpec.Name);
                }
            }
        }
        finally
        {
            RestoreOriginalScene(originalScenePath, originalSceneName);
            Undo.CollapseUndoOperations(undoGroup);
        }

        string modifiedSceneText = modifiedScenes.Count == 0 ? "No scene changes were needed." : string.Join(", ", modifiedScenes);
        string warningText = warnings.Count == 0 ? "No warnings." : string.Join("\n", warnings);
        Debug.Log(
            "Configure Guest 1 Story Flow complete.\n" +
            $"Modified scenes: {modifiedSceneText}\n" +
            "Controllers: Guest1LobbyStoryController, Guest1CartStoryController, Guest1WaitingStoryController.\n" +
            "UI: existing ObjectivePanelController and DialoguePanelController public APIs.\n" +
            $"Warnings:\n{warningText}");
    }

    private static bool ConfigureHotelLobby(Scene scene, List<string> warnings)
    {
        bool modified = false;
        GameObject controllerObject = GetOrCreateRoot(scene, "Guest1LobbyStoryController", ref modified);
        Guest1LobbyFlow controller = EnsureSingleComponent<Guest1LobbyFlow>(controllerObject, ref modified);

        GameObject guestZone = RequireSceneObject(scene, "Guest1InteractionZone", warnings);
        GameObject glassZone = RequireSceneObject(scene, "GlassInteractionZone", warnings);
        GameObject goldStrawZone = RequireSceneObject(scene, "GoldStrawInteractionZone", warnings);
        GameObject syrupZone = RequireSceneObject(scene, "HouseSpecialSyrupInteractionZone", warnings);
        GameObject umbrellaZone = RequireSceneObject(scene, "UmbrellaInteractionZone", warnings);
        GameObject elevatorZone = RequireSceneObject(scene, "ElevatorInteractionZone", warnings);

        modified |= SetObjectReference(controller, "glassVisual", FindSceneObject(scene, "Glass"));
        modified |= SetObjectReference(controller, "goldStrawVisual", FindSceneObject(scene, "GoldStraw"));
        modified |= SetObjectReference(controller, "houseSpecialSyrupVisual", FindSceneObject(scene, "HouseSpecialSyrup"));
        modified |= SetObjectReference(controller, "umbrellaVisual", FindSceneObject(scene, "Umbrella"));

        InteractionTarget glassInteraction = ConfigureNormalInteraction(glassZone, "Inspect glass", controller, nameof(Guest1LobbyFlow.CollectGlass), controller.CollectGlass, ref modified);
        InteractionTarget goldStrawInteraction = ConfigureNormalInteraction(goldStrawZone, "Inspect gold straw", controller, nameof(Guest1LobbyFlow.CollectGoldStraw), controller.CollectGoldStraw, ref modified);
        InteractionTarget syrupInteraction = ConfigureNormalInteraction(syrupZone, "Inspect syrup", controller, nameof(Guest1LobbyFlow.CollectHouseSpecialSyrup), controller.CollectHouseSpecialSyrup, ref modified);
        InteractionTarget umbrellaInteraction = ConfigureNormalInteraction(umbrellaZone, "Inspect umbrella", controller, nameof(Guest1LobbyFlow.CollectUmbrella), controller.CollectUmbrella, ref modified);

        ConfigureNormalInteraction(guestZone, "Talk to Guest 1", controller, nameof(Guest1LobbyFlow.InteractWithGuest), controller.InteractWithGuest, ref modified);

        modified |= SetObjectReference(controller, "glassInteraction", glassInteraction);
        modified |= SetObjectReference(controller, "goldStrawInteraction", goldStrawInteraction);
        modified |= SetObjectReference(controller, "houseSpecialSyrupInteraction", syrupInteraction);
        modified |= SetObjectReference(controller, "umbrellaInteraction", umbrellaInteraction);

        SceneTransitionTrigger elevatorTransition = ConfigureTransitionGate(elevatorZone, false, "HotelLobby_XW/ElevatorInteractionZone", warnings, ref modified);
        modified |= SetObjectReference(controller, "elevatorTransition", elevatorTransition);

        return modified;
    }

    private static bool ConfigureCartHallway(Scene scene, List<string> warnings)
    {
        bool modified = false;
        GameObject controllerObject = GetOrCreateRoot(scene, "Guest1CartStoryController", ref modified);
        Guest1CartFlow controller = EnsureSingleComponent<Guest1CartFlow>(controllerObject, ref modified);

        GameObject arrivalZone = RequireSceneObject(scene, "ArrivalCartInteractionZone", warnings);
        GameObject roomServiceZone = RequireSceneObject(scene, "RoomServiceCartInteractionZone", warnings);
        GameObject elevatorZone = RequireSceneObject(scene, "ElevatorInteractionZone", warnings);
        GameObject departureZone = RequireSceneObject(scene, "DepartureCartInteractionZone", warnings);

        ConfigureNormalInteraction(arrivalZone, "Inspect Arrival Cart", controller, nameof(Guest1CartFlow.InspectArrivalCart), controller.InspectArrivalCart, ref modified);
        ConfigureNormalInteraction(roomServiceZone, "Inspect Room Service Cart", controller, nameof(Guest1CartFlow.InspectRoomServiceCart), controller.InspectRoomServiceCart, ref modified);
        ConfigureNormalInteraction(elevatorZone, "Call elevator", controller, nameof(Guest1CartFlow.InspectElevator), controller.InspectElevator, ref modified);

        ConfigureTransitionGate(departureZone, true, "Guest1CartHallway_XW/DepartureCartInteractionZone", warnings, ref modified);
        return modified;
    }

    private static bool ConfigureWaitingHallway(Scene scene, List<string> warnings)
    {
        bool modified = false;
        GameObject controllerObject = GetOrCreateRoot(scene, "Guest1WaitingStoryController", ref modified);
        Guest1WaitingFlow controller = EnsureSingleComponent<Guest1WaitingFlow>(controllerObject, ref modified);

        GameObject privateElevatorZone = RequireSceneObject(scene, "PrivateElevatorInteractionZone", warnings);
        GameObject luggageZone = RequireSceneObject(scene, "Guest1LuggageInteractionZone", warnings);
        GameObject waitingRoomZone = RequireSceneObject(scene, "WaitingRoomInteractionZone", warnings);

        ConfigureNormalInteraction(privateElevatorZone, "Inspect private elevator", controller, nameof(Guest1WaitingFlow.InspectPrivateElevator), controller.InspectPrivateElevator, ref modified);
        InteractionTarget luggageInteraction = ConfigureNormalInteraction(luggageZone, "Inspect luggage", controller, nameof(Guest1WaitingFlow.CarryLuggage), controller.CarryLuggage, ref modified);

        modified |= SetObjectReference(controller, "luggageVisual", FindSceneObject(scene, "LuggageVisual"));
        modified |= SetObjectReference(controller, "luggageInteraction", luggageInteraction);

        SceneTransitionTrigger waitingRoomTransition = ConfigureTransitionGate(waitingRoomZone, false, "Guest1WaitingHallway_XW/WaitingRoomInteractionZone", warnings, ref modified);
        modified |= SetObjectReference(controller, "waitingRoomTransition", waitingRoomTransition);

        return modified;
    }

    private static InteractionTarget ConfigureNormalInteraction(
        GameObject zone,
        string promptText,
        Object listenerTarget,
        string listenerMethodName,
        UnityAction listenerAction,
        ref bool modified)
    {
        if (zone == null)
        {
            return null;
        }

        BoxCollider2D collider = zone.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(zone);
            modified = true;
        }

        if (!collider.isTrigger)
        {
            Undo.RecordObject(collider, UndoName);
            collider.isTrigger = true;
            EditorUtility.SetDirty(collider);
            modified = true;
        }

        InteractionTarget interactionTarget = EnsureSingleComponent<InteractionTarget>(zone, ref modified);
        modified |= ConfigureInteractionTarget(interactionTarget, promptText);
        modified |= EnsurePersistentListener(
            interactionTarget.OnInteract,
            interactionTarget,
            listenerTarget,
            listenerMethodName,
            listenerAction);

        return interactionTarget;
    }

    private static SceneTransitionTrigger ConfigureTransitionGate(
        GameObject zone,
        bool enabled,
        string context,
        List<string> warnings,
        ref bool modified)
    {
        if (zone == null)
        {
            return null;
        }

        SceneTransitionTrigger[] transitions = zone.GetComponents<SceneTransitionTrigger>();
        if (transitions.Length == 0)
        {
            warnings.Add($"{context}: existing SceneTransitionTrigger was not found; story flow did not create a new transition.");
            return null;
        }

        for (int i = 1; i < transitions.Length; i++)
        {
            Undo.DestroyObjectImmediate(transitions[i]);
            modified = true;
        }

        InteractionTarget[] interactionTargets = zone.GetComponents<InteractionTarget>();
        for (int i = 0; i < interactionTargets.Length; i++)
        {
            Undo.DestroyObjectImmediate(interactionTargets[i]);
            modified = true;
        }

        SceneTransitionTrigger transition = transitions[0];
        if (transition.enabled != enabled)
        {
            Undo.RecordObject(transition, UndoName);
            transition.enabled = enabled;
            EditorUtility.SetDirty(transition);
            modified = true;
        }

        return transition;
    }

    private static bool ConfigureInteractionTarget(InteractionTarget interactionTarget, string promptText)
    {
        bool modified = false;
        SerializedObject serializedObject = new(interactionTarget);
        SerializedProperty activationMode = serializedObject.FindProperty("activationMode");
        SerializedProperty prompt = serializedObject.FindProperty("promptText");
        SerializedProperty disableAfterInteract = serializedObject.FindProperty("disableAfterInteract");

        if (activationMode != null && activationMode.enumValueIndex != 0)
        {
            Undo.RecordObject(interactionTarget, UndoName);
            activationMode.enumValueIndex = 0;
            modified = true;
        }

        if (prompt != null && prompt.stringValue != promptText)
        {
            Undo.RecordObject(interactionTarget, UndoName);
            prompt.stringValue = promptText;
            modified = true;
        }

        if (disableAfterInteract != null && disableAfterInteract.boolValue)
        {
            Undo.RecordObject(interactionTarget, UndoName);
            disableAfterInteract.boolValue = false;
            modified = true;
        }

        if (modified)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(interactionTarget);
        }

        return modified;
    }

    private static bool EnsurePersistentListener(
        UnityEvent unityEvent,
        Object eventOwner,
        Object target,
        string methodName,
        UnityAction action)
    {
        bool modified = false;
        bool foundListener = false;

        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            Object persistentTarget = unityEvent.GetPersistentTarget(i);
            string persistentMethod = unityEvent.GetPersistentMethodName(i);
            if (persistentTarget != target || persistentMethod != methodName)
            {
                continue;
            }

            if (!foundListener)
            {
                foundListener = true;
                continue;
            }

            Undo.RecordObject(eventOwner, UndoName);
            UnityEventTools.RemovePersistentListener(unityEvent, i);
            modified = true;
        }

        if (!foundListener)
        {
            Undo.RecordObject(eventOwner, UndoName);
            UnityEventTools.AddPersistentListener(unityEvent, action);
            modified = true;
        }

        if (modified)
        {
            EditorUtility.SetDirty(eventOwner);
        }

        return modified;
    }

    private static bool SetObjectReference(Object target, string propertyName, Object value)
    {
        if (target == null)
        {
            return false;
        }

        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || property.objectReferenceValue == value)
        {
            return false;
        }

        Undo.RecordObject(target, UndoName);
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        return true;
    }

    private static bool ValidateSceneAssets()
    {
        List<string> missingScenes = new();
        for (int i = 0; i < Guest1Scenes.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Guest1Scenes[i].Path) == null)
            {
                missingScenes.Add(Guest1Scenes[i].Name);
            }
        }

        if (missingScenes.Count == 0)
        {
            return true;
        }

        string message = "Configure Guest 1 Story Flow cannot run because these scenes are missing:\n" + string.Join("\n", missingScenes);
        EditorUtility.DisplayDialog("Configure Guest 1 Story Flow", message, "OK");
        Debug.LogWarning(message);
        return false;
    }

    private static GameObject RequireSceneObject(Scene scene, string objectName, List<string> warnings)
    {
        GameObject sceneObject = FindSceneObject(scene, objectName);
        if (sceneObject == null)
        {
            warnings.Add($"{scene.name}: required object '{objectName}' was not found.");
        }

        return sceneObject;
    }

    private static GameObject GetOrCreateRoot(Scene scene, string objectName, ref bool modified)
    {
        GameObject existingRoot = FindRootGameObject(scene, objectName);
        if (existingRoot != null)
        {
            return existingRoot;
        }

        GameObject gameObject = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(gameObject, scene);
        Undo.RegisterCreatedObjectUndo(gameObject, UndoName);
        modified = true;
        return gameObject;
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

    private static GameObject FindRootGameObject(Scene scene, string objectName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name == objectName)
            {
                return rootObject;
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

    private static void RestoreOriginalScene(string originalScenePath, string originalSceneName)
    {
        if (!string.IsNullOrEmpty(originalScenePath) &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScenePath) != null)
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            return;
        }

        Debug.LogWarning($"Configure Guest 1 Story Flow could not restore original scene '{originalSceneName}' because it has no valid saved path.");
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
}
