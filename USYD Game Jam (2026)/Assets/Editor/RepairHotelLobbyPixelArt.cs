using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RepairHotelLobbyPixelArt
{
    private const string MenuPath = "Tools/Hotel Hunger/Repair Hotel Lobby Pixel Art";
    private const string UndoName = "Repair Hotel Lobby Pixel Art";
    private const string LobbyScenePath = "Assets/Scenes/HotelLobby_XW.unity";
    private const float PixelsPerUnit = 18f;

    private const string CleanLobbyBackgroundPath = "Assets/Pixel Art/Hotel_Lobby/Generated/Final_NoSeatedGuest.png";
    private const string ConciergeSpritePath = "Assets/Pixel Art/Characters/Concierge_char.png";
    private const string Guest1SpritePath = "Assets/Pixel Art/Characters/Guest_1_char.png";
    private const string GlassSpritePath = "Assets/Pixel Art/Hotel_Lobby/Crystal_glass.png";
    private const string StrawSpritePath = "Assets/Pixel Art/Hotel_Lobby/Golden_straw.png";
    private const string SyrupSpritePath = "Assets/Pixel Art/Hotel_Lobby/Red_Syrup.png";
    private const string UmbrellaSpritePath = "Assets/Pixel Art/Hotel_Lobby/Little_umbrella.png";
    private const string FloorSpritePath = "Assets/Pixel Art/Hotel_Lobby/Floor.png";
    private const string TableChairsSpritePath = "Assets/Pixel Art/Hotel_Lobby/Table+Chairs.png";
    private const string CarpetSpritePath = "Assets/Pixel Art/Hotel_Lobby/Carpet.png";
    private const string PlantsSpritePath = "Assets/Pixel Art/Hotel_Lobby/Plants.png";

    private static readonly string[] RequiredSpritePaths =
    {
        CleanLobbyBackgroundPath,
        ConciergeSpritePath,
        Guest1SpritePath,
        GlassSpritePath,
        StrawSpritePath,
        SyrupSpritePath,
        UmbrellaSpritePath,
        FloorSpritePath,
        TableChairsSpritePath,
        CarpetSpritePath,
        PlantsSpritePath
    };

    [InitializeOnLoadMethod]
    private static void RegisterSceneViewLabels()
    {
        SceneView.duringSceneGui -= DrawIngredientTriggerLabels;
        SceneView.duringSceneGui += DrawIngredientTriggerLabels;
    }

    [MenuItem(MenuPath)]
    private static void Repair()
    {
        AssetDatabase.Refresh();

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != LobbyScenePath)
        {
            EditorUtility.DisplayDialog(
                "Repair Hotel Lobby Pixel Art",
                "Open HotelLobby_XW before running this repair utility.",
                "OK");
            return;
        }

        if (!VerifyRequiredSpritesExist())
        {
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Repair Hotel Lobby Pixel Art",
                "This utility will repair and save HotelLobby_XW only.\n\n" +
                "It will update pixel-art visuals, replace the lobby background with Final_NoSeatedGuest.png, " +
                "repair character aspect ratios, and rebuild the lobby furniture collision layout.",
                "Repair and Save",
                "Cancel"))
        {
            Debug.Log("Repair Hotel Lobby Pixel Art cancelled before scene changes.");
            return;
        }

        ConfigureSpriteImporters();
        AssetDatabase.Refresh();

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            RepairScene(scene);
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Repair Hotel Lobby Pixel Art",
            "HotelLobby_XW pixel-art presentation was repaired and saved.",
            "OK");
    }

    private static void RepairScene(Scene scene)
    {
        GameObject visualRoot = EnsureSingleRoot(scene, "PixelArtVisuals_HotelLobby");
        SetBackground(visualRoot, CleanLobbyBackgroundPath);

        HidePlaceholderRenderers(scene);
        RepairCharacters(scene);
        RepairIngredients(scene);
        DisableObsoleteSolidColliders(scene);
        RebuildCollisionLayout(scene);
    }

    private static void HidePlaceholderRenderers(Scene scene)
    {
        HideRenderers(scene, "LobbyFloor", "TopWall", "BottomWall", "LeftWall", "RightWall");
        HideRenderers(scene, "CounterVisual", "DrinkStationVisual", "ElevatorVisual", "ElevatorSign");
        HideChildRenderers(scene, "ReceptionCounter", "Body", "Head");
    }

    private static void RepairCharacters(Scene scene)
    {
        GameObject player = FindByName(scene, "Player");
        HideOwnRenderer(player);
        ConfigureCharacterVisual(player, "PixelArtConciergeVisual", ConciergeSpritePath, 1f, 12);

        GameObject guest1 = FindByName(scene, "Guest1");
        HideOwnRenderer(guest1);
        HideOwnRenderer(FindChildByName(guest1, "Guest1Visual"));
        HideOwnRenderer(FindChildByName(guest1, "PixelArtSprite"));
        ConfigureCharacterVisual(guest1, "PixelArtGuest1Visual", Guest1SpritePath, 1f, 12);
    }

    private static void ConfigureCharacterVisual(
        GameObject parent,
        string childName,
        string spritePath,
        float uniformWorldScale,
        int sortingOrder)
    {
        if (parent == null)
        {
            return;
        }

        GameObject visual = EnsureDirectChild(parent, childName);
        RemoveVisualOnlyForbiddenComponents(visual);
        SetLocalPosition(visual, Vector3.zero);
        SetCompensatedWorldScale(visual.transform, uniformWorldScale);
        AssignSprite(visual, LoadSprite(spritePath), sortingOrder, Color.white);
    }

    private static void RepairIngredients(Scene scene)
    {
        ConfigureIngredient(
            scene,
            "GoldStraw",
            "GoldStrawInteractionZone",
            PixelToWorld(213f, 50f),
            new Vector2(24f, 28f));

        ConfigureIngredient(
            scene,
            "Glass",
            "GlassInteractionZone",
            PixelToWorld(246.5f, 10.5f),
            new Vector2(28f, 28f));

        ConfigureIngredient(
            scene,
            "HouseSpecialSyrup",
            "HouseSpecialSyrupInteractionZone",
            PixelToWorld(274f, 11f),
            new Vector2(26f, 28f));

        ConfigureIngredient(
            scene,
            "Umbrella",
            "UmbrellaInteractionZone",
            PixelToWorld(302f, 10f),
            new Vector2(30f, 28f));
    }

    private static void ConfigureIngredient(
        Scene scene,
        string objectName,
        string interactionZoneName,
        Vector3 bakedVisualWorldPosition,
        Vector2 triggerSizePixels)
    {
        GameObject ingredient = FindByName(scene, objectName);
        if (ingredient == null)
        {
            return;
        }

        SetWorldPosition(ingredient, bakedVisualWorldPosition);
        SetLocalScale(ingredient, Vector3.one);
        HideRenderersInChildren(ingredient);

        GameObject interactionZone = FindByName(scene, interactionZoneName);
        ConfigureIngredientInteractionZone(interactionZone, bakedVisualWorldPosition, triggerSizePixels);
    }

    private static void ConfigureIngredientInteractionZone(
        GameObject interactionZone,
        Vector3 triggerWorldPosition,
        Vector2 triggerSizePixels)
    {
        if (interactionZone == null)
        {
            return;
        }

        SetWorldPosition(interactionZone, triggerWorldPosition);
        SetLocalScale(interactionZone, Vector3.one);

        InteractionTarget target = interactionZone.GetComponent<InteractionTarget>();
        if (target != null)
        {
            Undo.RecordObject(target, UndoName);
            target.enabled = true;
            EditorUtility.SetDirty(target);
        }

        BoxCollider2D primaryCollider = interactionZone.GetComponent<BoxCollider2D>();
        if (primaryCollider == null)
        {
            primaryCollider = Undo.AddComponent<BoxCollider2D>(interactionZone);
        }

        Collider2D[] colliders = interactionZone.GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Undo.RecordObject(colliders[i], UndoName);

            if (colliders[i] == primaryCollider)
            {
                colliders[i].enabled = true;
                colliders[i].isTrigger = true;
                continue;
            }

            colliders[i].enabled = false;
        }

        Undo.RecordObject(primaryCollider, UndoName);
        primaryCollider.enabled = true;
        primaryCollider.isTrigger = true;
        primaryCollider.offset = Vector2.zero;
        primaryCollider.size = new Vector2(
            triggerSizePixels.x / PixelsPerUnit,
            triggerSizePixels.y / PixelsPerUnit);
        EditorUtility.SetDirty(primaryCollider);
    }

    private static void DisableObsoleteSolidColliders(Scene scene)
    {
        DisableNonTriggerColliders(scene, "CounterVisual", "DrinkStationVisual", "ElevatorVisual");
    }

    private static void RebuildCollisionLayout(Scene scene)
    {
        GameObject root = EnsureSingleRoot(scene, "PixelArtCollisionLayout_HotelLobby");
        RemoveVisualOnlyForbiddenComponents(root);
        ClearChildren(root);

        AddSolidBox(root, "UpperLeftRoundTable", 57f, 47f, 44f, 28f);
        AddSolidBox(root, "UpperLeftTopChair", 55f, 23f, 20f, 24f);
        AddSolidBox(root, "UpperLeftLeftChair", 20f, 62f, 22f, 28f);
        AddSolidBox(root, "UpperLeftRightChair", 94f, 73f, 24f, 28f);

        AddSolidBox(root, "LowerLeftTable", 28f, 128f, 38f, 24f);
        AddSolidBox(root, "LowerLeftChair", 66f, 151f, 22f, 25f);

        AddSolidBox(root, "UpperRightLeftDiagonalCounter", 187f, 55f, 58f, 16f, -45f);
        AddSolidBox(root, "UpperRightCentralCounter", 244f, 64f, 75f, 16f);
        AddSolidBox(root, "UpperRightShortCounter", 312f, 64f, 17f, 22f);

        AddSolidBox(root, "LowerCenterFlowerPot", 187f, 162f, 34f, 31f);
        AddSolidBox(root, "LowerRightFlowerPot", 229f, 162f, 34f, 31f);
    }

    private static void AddSolidBox(
        GameObject parent,
        string name,
        float centerPixelX,
        float centerPixelY,
        float widthPixels,
        float heightPixels,
        float rotationDegrees = 0f)
    {
        GameObject colliderObject = new(name);
        Undo.RegisterCreatedObjectUndo(colliderObject, UndoName);
        Undo.SetTransformParent(colliderObject.transform, parent.transform, UndoName);
        SetWorldPosition(colliderObject, PixelToWorld(centerPixelX, centerPixelY));
        SetLocalScale(colliderObject, Vector3.one);
        Undo.RecordObject(colliderObject.transform, UndoName);
        colliderObject.transform.rotation = Quaternion.Euler(0f, 0f, rotationDegrees);
        EditorUtility.SetDirty(colliderObject.transform);

        BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(colliderObject);
        Undo.RecordObject(collider, UndoName);
        collider.isTrigger = false;
        collider.size = new Vector2(widthPixels / PixelsPerUnit, heightPixels / PixelsPerUnit);
        collider.offset = Vector2.zero;
        EditorUtility.SetDirty(collider);
    }

    private static void SetBackground(GameObject visualRoot, string spritePath)
    {
        GameObject background = EnsureDirectChild(visualRoot, "PixelArtBackground");
        RemoveVisualOnlyForbiddenComponents(background);
        SetLocalPosition(background, Vector3.zero);
        SetLocalScale(background, Vector3.one);
        AssignSprite(background, LoadSprite(spritePath), -20, Color.white);
    }

    private static GameObject EnsureSingleRoot(Scene scene, string rootName)
    {
        List<GameObject> roots = FindAllByName(scene, rootName);
        GameObject root = roots.Count > 0 ? roots[0] : null;

        if (root == null)
        {
            root = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(root, UndoName);
            SceneManager.MoveGameObjectToScene(root, scene);
        }

        for (int i = 1; i < roots.Count; i++)
        {
            Undo.DestroyObjectImmediate(roots[i]);
        }

        SetWorldPosition(root, Vector3.zero);
        SetLocalScale(root, Vector3.one);
        return root;
    }

    private static GameObject EnsureDirectChild(GameObject parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        GameObject child = null;
        List<GameObject> duplicates = new();

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform candidate = parent.transform.GetChild(i);
            if (candidate.name != childName)
            {
                continue;
            }

            if (child == null)
            {
                child = candidate.gameObject;
            }
            else
            {
                duplicates.Add(candidate.gameObject);
            }
        }

        if (child == null)
        {
            child = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(child, UndoName);
            Undo.SetTransformParent(child.transform, parent.transform, UndoName);
        }

        for (int i = 0; i < duplicates.Count; i++)
        {
            Undo.DestroyObjectImmediate(duplicates[i]);
        }

        return child;
    }

    private static void AssignSprite(GameObject gameObject, Sprite sprite, int sortingOrder, Color color)
    {
        if (gameObject == null || sprite == null)
        {
            return;
        }

        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(gameObject);
        }

        Undo.RecordObject(renderer, UndoName);
        renderer.enabled = true;
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = sortingOrder;
        EditorUtility.SetDirty(renderer);
    }

    private static void HideRenderers(Scene scene, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            List<GameObject> objects = FindAllByName(scene, names[i]);
            for (int j = 0; j < objects.Count; j++)
            {
                HideOwnRenderer(objects[j]);
            }
        }
    }

    private static void HideChildRenderers(Scene scene, string parentName, params string[] childNames)
    {
        GameObject parent = FindByName(scene, parentName);
        if (parent == null)
        {
            return;
        }

        for (int i = 0; i < childNames.Length; i++)
        {
            HideOwnRenderer(FindChildByName(parent, childNames[i]));
        }
    }

    private static void HideOwnRenderer(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            return;
        }

        Undo.RecordObject(renderer, UndoName);
        renderer.enabled = false;
        EditorUtility.SetDirty(renderer);
    }

    private static void HideRenderersInChildren(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        SpriteRenderer[] renderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Undo.RecordObject(renderers[i], UndoName);
            renderers[i].enabled = false;
            EditorUtility.SetDirty(renderers[i]);
        }
    }

    private static void DisableNonTriggerColliders(Scene scene, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            List<GameObject> objects = FindAllByName(scene, names[i]);
            for (int j = 0; j < objects.Count; j++)
            {
                Collider2D[] colliders = objects[j].GetComponents<Collider2D>();
                for (int k = 0; k < colliders.Length; k++)
                {
                    if (colliders[k].isTrigger)
                    {
                        continue;
                    }

                    Undo.RecordObject(colliders[k], UndoName);
                    colliders[k].enabled = false;
                    EditorUtility.SetDirty(colliders[k]);
                }
            }
        }
    }

    private static void RemoveVisualOnlyForbiddenComponents(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        if (renderer != null && gameObject.name.Contains("CollisionLayout"))
        {
            Undo.DestroyObjectImmediate(renderer);
        }

        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            Undo.DestroyObjectImmediate(rigidbody);
        }

        Collider2D[] colliders = gameObject.GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Undo.DestroyObjectImmediate(colliders[i]);
        }
    }

    private static void ClearChildren(GameObject parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parent.transform.GetChild(i).gameObject);
        }
    }

    private static GameObject FindByName(Scene scene, string objectName)
    {
        List<GameObject> matches = FindAllByName(scene, objectName);
        return matches.Count > 0 ? matches[0] : null;
    }

    private static List<GameObject> FindAllByName(Scene scene, string objectName)
    {
        List<GameObject> matches = new();
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                if (transforms[j].name == objectName)
                {
                    matches.Add(transforms[j].gameObject);
                }
            }
        }

        return matches;
    }

    private static GameObject FindChildByName(GameObject parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        Transform[] transforms = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == childName)
            {
                return transforms[i].gameObject;
            }
        }

        return null;
    }

    private static void SetWorldPosition(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
        {
            return;
        }

        Undo.RecordObject(gameObject.transform, UndoName);
        gameObject.transform.position = position;
        EditorUtility.SetDirty(gameObject.transform);
    }

    private static void SetLocalPosition(GameObject gameObject, Vector3 position)
    {
        if (gameObject == null)
        {
            return;
        }

        Undo.RecordObject(gameObject.transform, UndoName);
        gameObject.transform.localPosition = position;
        EditorUtility.SetDirty(gameObject.transform);
    }

    private static void SetLocalScale(GameObject gameObject, Vector3 scale)
    {
        if (gameObject == null)
        {
            return;
        }

        Undo.RecordObject(gameObject.transform, UndoName);
        gameObject.transform.localScale = scale;
        EditorUtility.SetDirty(gameObject.transform);
    }

    private static void SetCompensatedWorldScale(Transform transform, float uniformWorldScale)
    {
        if (transform == null)
        {
            return;
        }

        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        Vector3 compensatedScale = new(
            SafeScale(uniformWorldScale, parentScale.x),
            SafeScale(uniformWorldScale, parentScale.y),
            1f);

        Undo.RecordObject(transform, UndoName);
        transform.localScale = compensatedScale;
        EditorUtility.SetDirty(transform);
    }

    private static float SafeScale(float desiredWorldScale, float parentAxisScale)
    {
        if (Mathf.Approximately(parentAxisScale, 0f))
        {
            return desiredWorldScale;
        }

        return desiredWorldScale / parentAxisScale;
    }

    private static Vector3 PixelToWorld(float pixelX, float pixelY)
    {
        return new Vector3((pixelX - 160f) / PixelsPerUnit, (90f - pixelY) / PixelsPerUnit, 0f);
    }

    private static Sprite LoadSprite(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static bool VerifyRequiredSpritesExist()
    {
        for (int i = 0; i < RequiredSpritePaths.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(RequiredSpritePaths[i]) != null)
            {
                continue;
            }

            EditorUtility.DisplayDialog(
                "Repair Hotel Lobby Pixel Art",
                $"Missing required sprite asset:\n{RequiredSpritePaths[i]}",
                "OK");
            return false;
        }

        return true;
    }

    private static void DrawIngredientTriggerLabels(SceneView sceneView)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != LobbyScenePath)
        {
            return;
        }

        DrawIngredientTriggerLabel(scene, "GoldStrawInteractionZone");
        DrawIngredientTriggerLabel(scene, "GlassInteractionZone");
        DrawIngredientTriggerLabel(scene, "HouseSpecialSyrupInteractionZone");
        DrawIngredientTriggerLabel(scene, "UmbrellaInteractionZone");
    }

    private static void DrawIngredientTriggerLabel(Scene scene, string objectName)
    {
        GameObject target = FindByName(scene, objectName);
        if (target == null || !IsSelected(target))
        {
            return;
        }

        BoxCollider2D trigger = target.GetComponent<BoxCollider2D>();
        if (trigger == null || !trigger.enabled)
        {
            return;
        }

        Bounds bounds = trigger.bounds;
        Handles.color = new Color(1f, 0.82f, 0.18f, 1f);
        Handles.DrawWireCube(bounds.center, bounds.size);
        Handles.Label(bounds.center + Vector3.up * 0.25f, objectName);
    }

    private static bool IsSelected(GameObject target)
    {
        Transform selected = Selection.activeTransform;
        if (selected == null)
        {
            return false;
        }

        return selected == target.transform ||
            selected.IsChildOf(target.transform) ||
            target.transform.IsChildOf(selected);
    }

    private static void ConfigureSpriteImporters()
    {
        for (int i = 0; i < RequiredSpritePaths.Length; i++)
        {
            TextureImporter importer = AssetImporter.GetAtPath(RequiredSpritePaths[i]) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
