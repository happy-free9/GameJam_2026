using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class ConfigureHorrorChase
{
    private const string RequiredSceneName = "Guest3HorrorChase_XW";
    private const string BadEndingSceneName = "BadEnding_XW";
    private const string BadEndingScenePath = "Assets/Scenes/BadEnding_XW.unity";
    private const string MenuPath = "Tools/Hotel Hunger/Configure Horror Chase";
    private const string UndoName = "Configure Horror Chase";
    private const string MonsterName = "HorrorMonster";
    private const string MonsterVisualName = "MonsterVisual";
    private const string BadEndingTransitionName = "BadEndingTransitionTrigger";
    private const string MonsterSpawnPointName = "MonsterSpawnPoint";
    private const string ChaseStartZoneName = "ChaseStartZone";
    private const string BadEndingSpawnName = "Spawn_FromHorrorChase";
    private const string BadEndingSpawnId = "FromHorrorChase";

    private static readonly Color MonsterColor = new Color(0.12f, 0.015f, 0.025f, 1f);

    [MenuItem(MenuPath)]
    private static void Configure()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Configure Horror Chase",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        string originalScenePath = activeScene.path;
        string originalSceneName = activeScene.name;

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        bool horrorSceneModified = false;
        bool badEndingModified = false;
        List<string> warnings = new();

        try
        {
            horrorSceneModified = ConfigureHorrorScene(activeScene, warnings);
            if (horrorSceneModified)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
            }

            badEndingModified = EnsureBadEndingSpawnPoint(warnings);
        }
        finally
        {
            RestoreOriginalScene(originalScenePath, originalSceneName);
            Undo.CollapseUndoOperations(undoGroup);
        }

        string warningText = warnings.Count == 0 ? "No warnings." : string.Join("\n", warnings);
        Debug.Log(
            "Configure Horror Chase complete.\n" +
            $"Horror scene modified: {horrorSceneModified}\n" +
            $"Bad ending scene modified: {badEndingModified}\n" +
            "Monster: HorrorMonster with BoxCollider2D trigger, SimpleChaseAgent2D, MonsterVisual, and hidden bad-ending transition trigger.\n" +
            "Chase start: ChaseStartZone uses InteractionTarget TriggerEnter to call SimpleChaseAgent2D.StartChase.\n" +
            "Bad ending: SimpleChaseAgent2D.OnPlayerCaught calls SceneTransitionTrigger.TriggerTransition to load BadEnding_XW / FromHorrorChase.\n" +
            $"Warnings:\n{warningText}");
    }

    private static bool ConfigureHorrorScene(Scene scene, List<string> warnings)
    {
        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            warnings.Add($"{RequiredSceneName}: could not find a square SpriteRenderer on Player or LobbyFloor.");
            return false;
        }

        GameObject monsterSpawnPoint = FindSceneObject(scene, MonsterSpawnPointName);
        if (monsterSpawnPoint == null)
        {
            warnings.Add($"{RequiredSceneName}: {MonsterSpawnPointName} was not found; {MonsterName} was not configured.");
            return false;
        }

        GameObject player = FindSceneObject(scene, "Player");
        if (player == null)
        {
            warnings.Add($"{RequiredSceneName}: Player was not found; chase target was left to auto-find at runtime.");
        }

        bool modified = false;
        GameObject monster = GetOrCreateRoot(scene, MonsterName, ref modified);
        modified |= ConfigureTransform(monster.transform, monsterSpawnPoint.transform.position, Quaternion.identity, Vector3.one, false);

        GameObject monsterVisual = GetOrCreateChild(monster.transform, MonsterVisualName, ref modified);
        modified |= ConfigureSprite(
            monsterVisual,
            referenceRenderer,
            Vector3.zero,
            new Vector3(0.95f, 1.65f, 1f),
            MonsterColor,
            4);
        modified |= RemoveComponentsIfPresent<Collider2D>(monsterVisual);
        modified |= RemoveComponentsIfPresent<Collider>(monsterVisual);

        BoxCollider2D monsterCollider = EnsureSingleComponent<BoxCollider2D>(monster, ref modified);
        modified |= ConfigureBoxCollider(monsterCollider, true, new Vector2(0.85f, 1.35f));

        SimpleChaseAgent2D chaseAgent = EnsureSingleComponent<SimpleChaseAgent2D>(monster, ref modified);
        modified |= ConfigureChaseAgent(chaseAgent, player);

        SceneTransitionTrigger badEndingTransition = ConfigureBadEndingTransition(monster.transform, ref modified);
        modified |= EnsurePersistentListener(
            chaseAgent.OnPlayerCaught,
            chaseAgent,
            badEndingTransition,
            nameof(SceneTransitionTrigger.TriggerTransition),
            badEndingTransition.TriggerTransition);

        modified |= ConfigureChaseStartZone(scene, monster, chaseAgent, warnings);
        if (monster.activeSelf)
        {
            Undo.RecordObject(monster, UndoName);
            monster.SetActive(false);
            modified = true;
        }

        return modified;
    }

    private static bool ConfigureChaseAgent(SimpleChaseAgent2D chaseAgent, GameObject player)
    {
        bool modified = false;
        bool serializedFieldsModified = false;
        SerializedObject serializedObject = new(chaseAgent);
        SerializedProperty target = serializedObject.FindProperty("target");
        SerializedProperty autoFindPlayerTarget = serializedObject.FindProperty("autoFindPlayerTarget");
        SerializedProperty moveSpeed = serializedObject.FindProperty("moveSpeed");
        SerializedProperty stopOnPlayerCaught = serializedObject.FindProperty("stopOnPlayerCaught");

        if (autoFindPlayerTarget != null && !autoFindPlayerTarget.boolValue)
        {
            Undo.RecordObject(chaseAgent, UndoName);
            autoFindPlayerTarget.boolValue = true;
            serializedFieldsModified = true;
            modified = true;
        }

        if (moveSpeed != null && !Mathf.Approximately(moveSpeed.floatValue, 3.2f))
        {
            Undo.RecordObject(chaseAgent, UndoName);
            moveSpeed.floatValue = 3.2f;
            serializedFieldsModified = true;
            modified = true;
        }

        if (stopOnPlayerCaught != null && !stopOnPlayerCaught.boolValue)
        {
            Undo.RecordObject(chaseAgent, UndoName);
            stopOnPlayerCaught.boolValue = true;
            serializedFieldsModified = true;
            modified = true;
        }

        if (serializedFieldsModified)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(chaseAgent);
        }

        if (player != null && target != null && target.objectReferenceValue != player.transform)
        {
            Undo.RecordObject(chaseAgent, UndoName);
            chaseAgent.SetTarget(player.transform);
            EditorUtility.SetDirty(chaseAgent);
            modified = true;
        }

        return modified;
    }

    private static SceneTransitionTrigger ConfigureBadEndingTransition(Transform monster, ref bool modified)
    {
        GameObject transitionObject = GetOrCreateChild(monster, BadEndingTransitionName, ref modified);
        modified |= ConfigureTransform(transitionObject.transform, Vector3.zero, Quaternion.identity, Vector3.one, true);
        modified |= RemoveComponentsIfPresent<SpriteRenderer>(transitionObject);

        BoxCollider2D transitionCollider = EnsureSingleComponent<BoxCollider2D>(transitionObject, ref modified);
        modified |= ConfigureBoxCollider(transitionCollider, true, Vector2.one);
        if (transitionCollider.enabled)
        {
            Undo.RecordObject(transitionCollider, UndoName);
            transitionCollider.enabled = false;
            modified = true;
        }

        SceneTransitionTrigger transition = EnsureSingleComponent<SceneTransitionTrigger>(transitionObject, ref modified);
        modified |= ConfigureSceneTransitionTrigger(transition, BadEndingSceneName, BadEndingSpawnId, "Bad ending");
        return transition;
    }

    private static bool ConfigureChaseStartZone(Scene scene, GameObject monster, SimpleChaseAgent2D chaseAgent, List<string> warnings)
    {
        GameObject chaseStartZone = FindSceneObject(scene, ChaseStartZoneName);
        if (chaseStartZone == null)
        {
            warnings.Add($"{RequiredSceneName}: {ChaseStartZoneName} was not found; chase start was not configured.");
            return false;
        }

        bool modified = false;
        BoxCollider2D collider = chaseStartZone.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(chaseStartZone);
            modified = true;
        }

        if (!collider.isTrigger)
        {
            Undo.RecordObject(collider, UndoName);
            collider.isTrigger = true;
            modified = true;
        }

        modified |= RemoveComponentsIfPresent<SceneTransitionTrigger>(chaseStartZone);

        InteractionTarget interactionTarget = EnsureSingleComponent<InteractionTarget>(chaseStartZone, ref modified);
        modified |= ConfigureInteractionTargetForTriggerEnter(interactionTarget);
        modified |= ConfigureChaseStartListeners(interactionTarget, monster, chaseAgent);

        return modified;
    }

    private static bool ConfigureInteractionTargetForTriggerEnter(InteractionTarget interactionTarget)
    {
        bool modified = false;
        SerializedObject serializedObject = new(interactionTarget);
        SerializedProperty activationMode = serializedObject.FindProperty("activationMode");
        SerializedProperty promptText = serializedObject.FindProperty("promptText");
        SerializedProperty disableAfterInteract = serializedObject.FindProperty("disableAfterInteract");

        if (activationMode != null && activationMode.enumValueIndex != 1)
        {
            Undo.RecordObject(interactionTarget, UndoName);
            activationMode.enumValueIndex = 1;
            modified = true;
        }

        if (promptText != null && promptText.stringValue != string.Empty)
        {
            Undo.RecordObject(interactionTarget, UndoName);
            promptText.stringValue = string.Empty;
            modified = true;
        }

        if (disableAfterInteract != null && !disableAfterInteract.boolValue)
        {
            Undo.RecordObject(interactionTarget, UndoName);
            disableAfterInteract.boolValue = true;
            modified = true;
        }

        if (modified)
        {
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
        string currentSceneName = ReadStringProperty(transition, "targetSceneName");
        string currentSpawnPointId = ReadStringProperty(transition, "targetSpawnPointId");
        string currentPromptText = ReadStringProperty(transition, "promptText");

        bool modified =
            currentSceneName != targetSceneName ||
            currentSpawnPointId != targetSpawnPointId ||
            currentPromptText != promptText;

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

    private static bool ConfigureChaseStartListeners(
        InteractionTarget interactionTarget,
        GameObject monster,
        SimpleChaseAgent2D chaseAgent)
    {
        UnityEvent unityEvent = interactionTarget.OnInteract;
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            Object persistentTarget = unityEvent.GetPersistentTarget(i);
            string persistentMethod = unityEvent.GetPersistentMethodName(i);
            if ((persistentTarget == monster && persistentMethod == nameof(GameObject.SetActive)) ||
                (persistentTarget == chaseAgent && persistentMethod == nameof(SimpleChaseAgent2D.StartChase)))
            {
                Undo.RecordObject(interactionTarget, UndoName);
                UnityEventTools.RemovePersistentListener(unityEvent, i);
            }
        }

        Undo.RecordObject(interactionTarget, UndoName);
        UnityEventTools.AddBoolPersistentListener(unityEvent, monster.SetActive, true);
        UnityEventTools.AddPersistentListener(unityEvent, chaseAgent.StartChase);
        EditorUtility.SetDirty(interactionTarget);
        return true;
    }

    private static bool EnsureBadEndingSpawnPoint(List<string> warnings)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BadEndingScenePath) == null)
        {
            warnings.Add($"{BadEndingSceneName}: scene asset was not found; {BadEndingSpawnName} was not configured.");
            return false;
        }

        Scene badEndingScene = EditorSceneManager.OpenScene(BadEndingScenePath, OpenSceneMode.Single);
        bool modified = false;

        GameObject spawnObject = FindSceneObject(badEndingScene, BadEndingSpawnName);
        if (spawnObject == null)
        {
            spawnObject = new GameObject(BadEndingSpawnName);
            SceneManager.MoveGameObjectToScene(spawnObject, badEndingScene);
            Undo.RegisterCreatedObjectUndo(spawnObject, UndoName);
            modified = true;

            GameObject player = FindSceneObject(badEndingScene, "Player");
            if (player != null)
            {
                modified |= ConfigureTransform(spawnObject.transform, player.transform.position, Quaternion.identity, Vector3.one, false);
            }
        }

        modified |= RemoveComponentsIfPresent<SpriteRenderer>(spawnObject);
        modified |= RemoveComponentsIfPresent<Collider2D>(spawnObject);
        modified |= RemoveComponentsIfPresent<Collider>(spawnObject);

        SpawnPoint spawnPoint = EnsureSingleComponent<SpawnPoint>(spawnObject, ref modified);
        if (spawnPoint.SpawnPointId != BadEndingSpawnId)
        {
            Undo.RecordObject(spawnPoint, UndoName);
            spawnPoint.SetSpawnPointId(BadEndingSpawnId);
            EditorUtility.SetDirty(spawnPoint);
            modified = true;
        }

        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(badEndingScene);
            EditorSceneManager.SaveScene(badEndingScene);
        }

        return modified;
    }

    private static bool ConfigureSprite(
        GameObject gameObject,
        SpriteRenderer referenceRenderer,
        Vector3 localPosition,
        Vector3 localScale,
        Color color,
        int sortingOrder)
    {
        bool modified = false;
        modified |= ConfigureTransform(gameObject.transform, localPosition, Quaternion.identity, localScale, true);

        SpriteRenderer spriteRenderer = EnsureSingleComponent<SpriteRenderer>(gameObject, ref modified);
        if (spriteRenderer.sprite != referenceRenderer.sprite ||
            spriteRenderer.sharedMaterial != referenceRenderer.sharedMaterial ||
            spriteRenderer.color != color ||
            spriteRenderer.sortingOrder != sortingOrder)
        {
            Undo.RecordObject(spriteRenderer, UndoName);
            spriteRenderer.sprite = referenceRenderer.sprite;
            spriteRenderer.sharedMaterial = referenceRenderer.sharedMaterial;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
            EditorUtility.SetDirty(spriteRenderer);
            modified = true;
        }

        return modified;
    }

    private static bool ConfigureBoxCollider(BoxCollider2D collider, bool isTrigger, Vector2 size)
    {
        bool modified = false;
        if (collider.isTrigger != isTrigger || collider.size != size || collider.offset != Vector2.zero)
        {
            Undo.RecordObject(collider, UndoName);
            collider.isTrigger = isTrigger;
            collider.size = size;
            collider.offset = Vector2.zero;
            EditorUtility.SetDirty(collider);
            modified = true;
        }

        return modified;
    }

    private static bool ConfigureTransform(Transform transform, Vector3 position, Quaternion rotation, Vector3 scale, bool useLocalPosition)
    {
        bool modified = false;
        Vector3 currentPosition = useLocalPosition ? transform.localPosition : transform.position;
        if (currentPosition != position || transform.localRotation != rotation || transform.localScale != scale)
        {
            Undo.RecordObject(transform, UndoName);
            if (useLocalPosition)
            {
                transform.localPosition = position;
            }
            else
            {
                transform.position = position;
            }

            transform.localRotation = rotation;
            transform.localScale = scale;
            modified = true;
        }

        return modified;
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

    private static GameObject GetOrCreateChild(Transform parent, string childName, ref bool modified)
    {
        Transform existingChild = parent.Find(childName);
        if (existingChild != null)
        {
            return existingChild.gameObject;
        }

        GameObject child = new GameObject(childName);
        Undo.RegisterCreatedObjectUndo(child, UndoName);
        child.transform.SetParent(parent, false);
        modified = true;
        return child;
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

    private static string ReadStringProperty(Object target, string propertyName)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property == null ? null : property.stringValue;
    }

    private static SpriteRenderer FindReferenceRenderer(Scene scene)
    {
        SpriteRenderer playerRenderer = FindRendererOnSceneObject(scene, "Player");
        if (playerRenderer != null && playerRenderer.sprite != null)
        {
            return playerRenderer;
        }

        SpriteRenderer floorRenderer = FindRendererOnSceneObject(scene, "LobbyFloor");
        if (floorRenderer != null && floorRenderer.sprite != null)
        {
            return floorRenderer;
        }

        return null;
    }

    private static SpriteRenderer FindRendererOnSceneObject(Scene scene, string objectName)
    {
        GameObject gameObject = FindSceneObject(scene, objectName);
        if (gameObject == null)
        {
            return null;
        }

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer;
        }

        return gameObject.GetComponentInChildren<SpriteRenderer>(true);
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

        Debug.LogWarning($"Configure Horror Chase could not restore original scene '{originalSceneName}' because it has no valid saved path.");
    }
}
