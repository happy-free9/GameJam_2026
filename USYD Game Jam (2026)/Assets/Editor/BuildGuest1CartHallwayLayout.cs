using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildGuest1CartHallwayLayout
{
    private const string RequiredSceneName = "Guest1CartHallway_XW";
    private const string MenuPath = "Tools/Hotel Hunger/Build Guest 1 Cart Hallway";
    private const string DecorateMenuPath = "Tools/Hotel Hunger/Decorate Guest 1 Cart Hallway";
    private const string UndoName = "Build Guest 1 Cart Hallway";
    private const string DecorateUndoName = "Decorate Guest 1 Cart Hallway";

    private static readonly Color RoomServiceCartColor = new Color(0.16f, 0.21f, 0.27f, 1f);
    private static readonly Color DepartureCartColor = new Color(0.42f, 0.12f, 0.16f, 1f);
    private static readonly Color ElevatorColor = new Color(0.18f, 0.17f, 0.16f, 1f);
    private static readonly Color LabelColor = new Color(0.95f, 0.86f, 0.58f, 1f);
    private static readonly Color SuitcaseColor = new Color(0.08f, 0.06f, 0.04f, 1f);
    private static readonly Color HintColor = new Color(0.95f, 0.82f, 0.38f, 1f);

    [MenuItem(MenuPath)]
    private static void BuildOrUpdate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Cart Hallway Layout",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Cart Hallway Layout",
                "Could not find a SpriteRenderer with a square sprite on Player or LobbyFloor.",
                "OK");
            return;
        }

        Material spriteMaterial = FindSpriteUnlitDefaultMaterial(referenceRenderer);

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        BuildCart(
            scene,
            referenceRenderer.sprite,
            spriteMaterial,
            "RoomServiceCart",
            new Vector3(-0.5f, 0.3f, 0f),
            RoomServiceCartColor,
            "RoomServiceCartInteractionZone");

        BuildCart(
            scene,
            referenceRenderer.sprite,
            spriteMaterial,
            "DepartureCart",
            new Vector3(3.5f, 0.3f, 0f),
            DepartureCartColor,
            "DepartureCartInteractionZone");

        BuildElevatorDoor(scene, referenceRenderer.sprite, spriteMaterial);

        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    [MenuItem(DecorateMenuPath)]
    private static void Decorate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Cart Hallway Decoration",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Cart Hallway Decoration",
                "Could not find a SpriteRenderer with a square sprite on Player or LobbyFloor.",
                "OK");
            return;
        }

        Material spriteMaterial = FindSpriteUnlitDefaultMaterial(referenceRenderer);

        Undo.SetCurrentGroupName(DecorateUndoName);
        int undoGroup = Undo.GetCurrentGroup();

        MovePlayerIfNeeded(scene, new Vector3(0f, -2.3f, 0f));
        BuildCartLabel(scene, "ArrivalCart", "ARRIVAL");
        BuildCartLabel(scene, "RoomServiceCart", "ROOM SERVICE");
        BuildCartLabel(scene, "DepartureCart", "DEPARTURE");
        BuildGuest1Suitcase(scene, referenceRenderer.sprite, spriteMaterial);
        BuildDepartureCartHint(scene, referenceRenderer.sprite, spriteMaterial);

        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    private static void BuildCart(
        Scene scene,
        Sprite sprite,
        Material material,
        string rootName,
        Vector3 worldPosition,
        Color visualColor,
        string interactionZoneName)
    {
        GameObject cart = GetOrCreateSceneObject(scene, rootName);
        ConfigureTransform(cart.transform, worldPosition, Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(cart.transform, "CartVisual");
        ConfigureSprite(
            visual,
            sprite,
            material,
            Vector3.zero,
            new Vector3(1.8f, 0.8f, 1f),
            visualColor,
            1);
        ConfigureBoxCollider(visual, false, Vector2.one);

        GameObject interactionZone = GetOrCreateChild(cart.transform, interactionZoneName);
        ConfigureTransform(interactionZone.transform, new Vector3(0f, -0.9f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(interactionZone);
        ConfigureBoxCollider(interactionZone, true, new Vector2(2.2f, 1f));
    }

    private static void BuildElevatorDoor(Scene scene, Sprite sprite, Material material)
    {
        GameObject elevatorDoor = GetOrCreateSceneObject(scene, "ElevatorDoor");
        ConfigureTransform(elevatorDoor.transform, new Vector3(6.5f, 1.2f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(elevatorDoor.transform, "ElevatorVisual");
        ConfigureSprite(
            visual,
            sprite,
            material,
            Vector3.zero,
            new Vector3(1.8f, 2.8f, 1f),
            ElevatorColor,
            1);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);

        GameObject interactionZone = GetOrCreateChild(elevatorDoor.transform, "ElevatorInteractionZone");
        ConfigureTransform(interactionZone.transform, new Vector3(0f, -1f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(interactionZone);
        ConfigureBoxCollider(interactionZone, true, new Vector2(2.5f, 1.2f));
    }

    private static void MovePlayerIfNeeded(Scene scene, Vector3 worldPosition)
    {
        Transform player = FindTransformByName(scene, "Player");
        if (player == null || (player.position - worldPosition).sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Undo.RecordObject(player, DecorateUndoName);
        player.position = worldPosition;
    }

    private static void BuildCartLabel(Scene scene, string cartName, string labelText)
    {
        Transform cart = FindTransformByName(scene, cartName);
        if (cart == null)
        {
            Debug.LogWarning($"Could not create {cartName}/CartLabel because {cartName} was not found in {RequiredSceneName}.");
            return;
        }

        GameObject label = GetOrCreateChild(cart, "CartLabel");
        ConfigureTextLabel(label, labelText, new Vector3(0f, 0.85f, 0f));
    }

    private static void BuildGuest1Suitcase(Scene scene, Sprite sprite, Material material)
    {
        GameObject suitcase = GetOrCreateRoot(scene, "Guest1Suitcase");
        ConfigureTransform(suitcase.transform, new Vector3(-2.6f, -1.45f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(suitcase.transform, "SuitcaseVisual");
        ConfigureSprite(
            visual,
            sprite,
            material,
            Vector3.zero,
            new Vector3(0.9f, 0.55f, 1f),
            SuitcaseColor,
            2);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);

        GameObject interactionZone = GetOrCreateChild(suitcase.transform, "Guest1SuitcaseInteractionZone");
        ConfigureTransform(interactionZone.transform, new Vector3(0f, -0.55f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(interactionZone);
        ConfigureBoxCollider(interactionZone, true, new Vector2(1.5f, 0.9f));
    }

    private static void BuildDepartureCartHint(Scene scene, Sprite sprite, Material material)
    {
        Transform departureCart = FindTransformByName(scene, "DepartureCart");
        if (departureCart == null)
        {
            Debug.LogWarning($"Could not create DepartureCartHint because DepartureCart was not found in {RequiredSceneName}.");
            return;
        }

        GameObject hint = GetOrCreateChild(departureCart, "DepartureCartHint");
        ConfigureSprite(
            hint,
            sprite,
            material,
            new Vector3(0f, 1.25f, 0f),
            new Vector3(0.22f, 0.22f, 1f),
            HintColor,
            3);
        Undo.RecordObject(hint.transform, DecorateUndoName);
        hint.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        RemoveComponentsIfPresent<Collider2D>(hint);
        RemoveComponentsIfPresent<Collider>(hint);
    }

    private static void ConfigureTextLabel(GameObject label, string text, Vector3 localPosition)
    {
        ConfigureTransform(label.transform, localPosition, Quaternion.identity, Vector3.one, true);

        TextMesh textMesh = GetOrAddComponent<TextMesh>(label);
        Undo.RecordObject(textMesh, DecorateUndoName);
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = LabelColor;
        textMesh.fontSize = 64;
        textMesh.characterSize = 0.055f;

        Font builtInFont = GetBuiltInFont();
        if (builtInFont != null)
        {
            textMesh.font = builtInFont;
        }

        MeshRenderer meshRenderer = GetOrAddComponent<MeshRenderer>(label);
        Undo.RecordObject(meshRenderer, DecorateUndoName);
        if (textMesh.font != null)
        {
            meshRenderer.sharedMaterial = textMesh.font.material;
        }

        meshRenderer.sortingOrder = 5;
        RemoveComponentsIfPresent<Collider2D>(label);
        RemoveComponentsIfPresent<Collider>(label);
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

    private static GameObject GetOrCreateSceneObject(Scene scene, string objectName)
    {
        Transform existingTransform = FindTransformByName(scene, objectName);
        if (existingTransform != null)
        {
            return existingTransform.gameObject;
        }

        GameObject gameObject = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(gameObject, scene);
        Undo.RegisterCreatedObjectUndo(gameObject, UndoName);
        return gameObject;
    }

    private static GameObject GetOrCreateRoot(Scene scene, string objectName)
    {
        Transform existingTransform = FindTransformByName(scene, objectName);
        if (existingTransform != null)
        {
            if (existingTransform.parent != null)
            {
                Undo.SetTransformParent(existingTransform, null, DecorateUndoName);
                SceneManager.MoveGameObjectToScene(existingTransform.gameObject, scene);
            }

            return existingTransform.gameObject;
        }

        GameObject gameObject = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(gameObject, scene);
        Undo.RegisterCreatedObjectUndo(gameObject, DecorateUndoName);
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
        Transform match = FindTransformByName(scene, objectName);
        if (match == null)
        {
            return null;
        }

        SpriteRenderer spriteRenderer = match.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer;
        }

        return match.GetComponentInChildren<SpriteRenderer>(true);
    }

    private static Transform FindTransformByName(Scene scene, string objectName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Transform match = FindTransformByName(rootObject.transform, objectName);
            if (match != null)
            {
                return match;
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
