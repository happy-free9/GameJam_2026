using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildGuest1WaitingHallwayLayout
{
    private const string RequiredSceneName = "Guest1WaitingHallway_XW";
    private const string MenuPath = "Tools/Hotel Hunger/Build Guest 1 Waiting Hallway";
    private const string UndoName = "Build Guest 1 Waiting Hallway";

    private static readonly string[] CopiedLayoutRoots =
    {
        "ArrivalCart",
        "RoomServiceCart",
        "DepartureCart",
        "ElevatorDoor",
        "Guest1Suitcase"
    };

    private static readonly Color PrivateElevatorColor = new Color(0.2f, 0.18f, 0.13f, 1f);
    private static readonly Color LuggageColor = new Color(0.08f, 0.06f, 0.04f, 1f);
    private static readonly Color WaitingRoomDoorColor = new Color(0.28f, 0.05f, 0.08f, 1f);
    private static readonly Color RunnerColor = new Color(0.34f, 0.09f, 0.08f, 1f);
    private static readonly Color SignColor = new Color(0.95f, 0.86f, 0.58f, 1f);

    [MenuItem(MenuPath)]
    private static void BuildOrUpdate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Waiting Hallway Layout",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Waiting Hallway Layout",
                "Could not find a SpriteRenderer with a square sprite on Player or LobbyFloor.",
                "OK");
            return;
        }

        Material spriteMaterial = FindSpriteUnlitDefaultMaterial(referenceRenderer);

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        RemoveCopiedLayoutRoots(scene);
        BuildPrivateElevator(scene, referenceRenderer.sprite, spriteMaterial);
        BuildGuest1Luggage(scene, referenceRenderer.sprite, spriteMaterial);
        BuildWaitingRoomDoor(scene, referenceRenderer.sprite, spriteMaterial);
        BuildHallwayRunner(scene, referenceRenderer.sprite, spriteMaterial);

        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    private static void RemoveCopiedLayoutRoots(Scene scene)
    {
        for (int i = 0; i < CopiedLayoutRoots.Length; i++)
        {
            GameObject rootObject = FindRootGameObject(scene, CopiedLayoutRoots[i]);
            if (rootObject != null)
            {
                Undo.DestroyObjectImmediate(rootObject);
            }
        }
    }

    private static void BuildPrivateElevator(Scene scene, Sprite sprite, Material material)
    {
        GameObject privateElevator = GetOrCreateRoot(scene, "PrivateElevator");
        ConfigureTransform(privateElevator.transform, new Vector3(-6.4f, 1f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(privateElevator.transform, "PrivateElevatorVisual");
        ConfigureSprite(
            visual,
            sprite,
            material,
            Vector3.zero,
            new Vector3(1.8f, 2.8f, 1f),
            PrivateElevatorColor,
            1);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);

        GameObject sign = GetOrCreateChild(privateElevator.transform, "PrivateElevatorSign");
        ConfigureTextSign(sign, "PRIVATE ELEVATOR", new Vector3(0f, 1.75f, 0f));

        GameObject interactionZone = GetOrCreateChild(privateElevator.transform, "PrivateElevatorInteractionZone");
        ConfigureTransform(interactionZone.transform, new Vector3(0f, -1f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(interactionZone);
        ConfigureBoxCollider(interactionZone, true, new Vector2(2.5f, 1.2f));
    }

    private static void BuildGuest1Luggage(Scene scene, Sprite sprite, Material material)
    {
        GameObject luggage = GetOrCreateRoot(scene, "Guest1Luggage");
        ConfigureTransform(luggage.transform, new Vector3(-2.8f, -1.65f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(luggage.transform, "LuggageVisual");
        ConfigureSprite(
            visual,
            sprite,
            material,
            Vector3.zero,
            new Vector3(0.9f, 0.55f, 1f),
            LuggageColor,
            2);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);

        GameObject interactionZone = GetOrCreateChild(luggage.transform, "Guest1LuggageInteractionZone");
        ConfigureTransform(interactionZone.transform, new Vector3(0f, -0.55f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(interactionZone);
        ConfigureBoxCollider(interactionZone, true, new Vector2(1.5f, 0.9f));
    }

    private static void BuildWaitingRoomDoor(Scene scene, Sprite sprite, Material material)
    {
        GameObject waitingRoomDoor = GetOrCreateRoot(scene, "WaitingRoomDoor");
        ConfigureTransform(waitingRoomDoor.transform, new Vector3(6.2f, 1f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(waitingRoomDoor.transform, "WaitingRoomDoorVisual");
        ConfigureSprite(
            visual,
            sprite,
            material,
            Vector3.zero,
            new Vector3(1.9f, 2.9f, 1f),
            WaitingRoomDoorColor,
            1);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);

        GameObject sign = GetOrCreateChild(waitingRoomDoor.transform, "WaitingRoomSign");
        ConfigureTextSign(sign, "WAITING ROOM", new Vector3(0f, 1.8f, 0f));

        GameObject interactionZone = GetOrCreateChild(waitingRoomDoor.transform, "WaitingRoomInteractionZone");
        ConfigureTransform(interactionZone.transform, new Vector3(0f, -1f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(interactionZone);
        ConfigureBoxCollider(interactionZone, true, new Vector2(2.6f, 1.2f));
    }

    private static void BuildHallwayRunner(Scene scene, Sprite sprite, Material material)
    {
        GameObject runner = GetOrCreateRoot(scene, "HallwayRunner");
        ConfigureSprite(
            runner,
            sprite,
            material,
            new Vector3(0f, -1.55f, 0f),
            new Vector3(10.8f, 0.65f, 1f),
            RunnerColor,
            -5);
        RemoveComponentsIfPresent<Collider2D>(runner);
        RemoveComponentsIfPresent<Collider>(runner);
    }

    private static void ConfigureSprite(
        GameObject gameObject,
        Sprite sprite,
        Material material,
        Vector3 localPosition,
        Vector3 localScale,
        Color color,
        int sortingOrder)
    {
        ConfigureTransform(gameObject.transform, localPosition, Quaternion.identity, localScale, true);

        SpriteRenderer spriteRenderer = GetOrAddComponent<SpriteRenderer>(gameObject);
        Undo.RecordObject(spriteRenderer, UndoName);
        spriteRenderer.sprite = sprite;
        if (material != null)
        {
            spriteRenderer.sharedMaterial = material;
        }

        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    private static void ConfigureTextSign(GameObject sign, string text, Vector3 localPosition)
    {
        ConfigureTransform(sign.transform, localPosition, Quaternion.identity, Vector3.one, true);

        TextMesh textMesh = GetOrAddComponent<TextMesh>(sign);
        Undo.RecordObject(textMesh, UndoName);
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = SignColor;
        textMesh.fontSize = 80;
        textMesh.characterSize = 0.045f;

        Font builtInFont = GetBuiltInFont();
        if (builtInFont != null)
        {
            textMesh.font = builtInFont;
        }

        MeshRenderer meshRenderer = GetOrAddComponent<MeshRenderer>(sign);
        Undo.RecordObject(meshRenderer, UndoName);
        if (textMesh.font != null)
        {
            meshRenderer.sharedMaterial = textMesh.font.material;
        }

        meshRenderer.sortingOrder = 5;
        RemoveComponentsIfPresent<SpriteRenderer>(sign);
        RemoveComponentsIfPresent<Collider2D>(sign);
        RemoveComponentsIfPresent<Collider>(sign);
    }

    private static void ConfigureTransform(Transform transform, Vector3 position, Quaternion rotation, Vector3 scale, bool useLocalPosition)
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
    }

    private static void ConfigureBoxCollider(GameObject gameObject, bool isTrigger, Vector2 size)
    {
        BoxCollider2D collider = GetOrAddComponent<BoxCollider2D>(gameObject);
        Undo.RecordObject(collider, UndoName);
        collider.isTrigger = isTrigger;
        collider.size = size;
        collider.offset = Vector2.zero;
    }

    private static GameObject GetOrCreateRoot(Scene scene, string objectName)
    {
        GameObject existingRoot = FindRootGameObject(scene, objectName);
        if (existingRoot != null)
        {
            return existingRoot;
        }

        GameObject gameObject = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(gameObject, scene);
        Undo.RegisterCreatedObjectUndo(gameObject, UndoName);
        return gameObject;
    }

    private static GameObject GetOrCreateChild(Transform parent, string childName)
    {
        Transform existingChild = parent.Find(childName);
        if (existingChild != null)
        {
            return existingChild.gameObject;
        }

        GameObject child = new GameObject(childName);
        Undo.RegisterCreatedObjectUndo(child, UndoName);
        child.transform.SetParent(parent, false);
        return child;
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

    private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return Undo.AddComponent<T>(gameObject);
    }

    private static void RemoveComponentsIfPresent<T>(GameObject gameObject) where T : Component
    {
        T[] components = gameObject.GetComponents<T>();
        for (int i = 0; i < components.Length; i++)
        {
            Undo.DestroyObjectImmediate(components[i]);
        }
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

    private static Material FindSpriteUnlitDefaultMaterial(SpriteRenderer referenceRenderer)
    {
        Material packageMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
        if (packageMaterial != null)
        {
            return packageMaterial;
        }

        string[] materialGuids = AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material");
        for (int i = 0; i < materialGuids.Length; i++)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material != null)
            {
                return material;
            }
        }

        return referenceRenderer.sharedMaterial;
    }

    private static Font GetBuiltInFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            return font;
        }

        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static SpriteRenderer FindRendererOnSceneObject(Scene scene, string objectName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Transform match = FindTransformByName(rootObject.transform, objectName);
            if (match == null)
            {
                continue;
            }

            SpriteRenderer spriteRenderer = match.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer;
            }

            spriteRenderer = match.GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer != null)
            {
                return spriteRenderer;
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
}
