using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildGuest3HorrorChaseLayout
{
    private const string RequiredSceneName = "Guest3HorrorChase_XW";
    private const string MenuPath = "Tools/Hotel Hunger/Build Guest 3 Horror Chase";
    private const string UndoName = "Build Guest 3 Horror Chase";

    private static readonly Color FloorColor = new Color(0.08f, 0.11f, 0.16f, 1f);
    private static readonly Color WallColor = new Color(0.03f, 0.04f, 0.06f, 1f);
    private static readonly Color CarpetColor = new Color(0.08f, 0.01f, 0.025f, 1f);
    private static readonly Color TableColor = new Color(0.055f, 0.035f, 0.02f, 1f);
    private static readonly Color EntranceColor = new Color(0.09f, 0.07f, 0.045f, 1f);
    private static readonly Color ExitColor = new Color(0.055f, 0.005f, 0.012f, 1f);
    private static readonly Color StaffDoorColor = new Color(0.055f, 0.058f, 0.065f, 1f);
    private static readonly Color SignColor = new Color(0.48f, 0.44f, 0.43f, 1f);
    private static readonly Color ChairColor = new Color(0.035f, 0.025f, 0.018f, 1f);
    private static readonly Color BrokenLightColor = new Color(0.55f, 0.52f, 0.42f, 1f);
    private static readonly Color RedStainColor = new Color(0.18f, 0.015f, 0.015f, 1f);

    [MenuItem(MenuPath)]
    private static void BuildOrUpdate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Horror Chase Layout",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Horror Chase Layout",
                "Could not find a SpriteRenderer with a square sprite on Player or LobbyFloor.",
                "OK");
            return;
        }

        Material spriteMaterial = FindSpriteUnlitDefaultMaterial(referenceRenderer);

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        ApplyHorrorStyle(scene);
        UpdateExistingLabels(scene);
        BuildFallenChair(scene, referenceRenderer.sprite, spriteMaterial);
        BuildBrokenLight(scene, referenceRenderer.sprite, spriteMaterial);
        BuildRedStain(scene, referenceRenderer.sprite, spriteMaterial);
        BuildFutureMarkers(scene);
        RemoveEscapeSign(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    private static void ApplyHorrorStyle(Scene scene)
    {
        SetSpriteColor(scene, "LobbyFloor", FloorColor);
        SetSpriteColor(scene, "TopWall", WallColor);
        SetSpriteColor(scene, "BottomWall", WallColor);
        SetSpriteColor(scene, "LeftWall", WallColor);
        SetSpriteColor(scene, "RightWall", WallColor);
        SetSpriteColor(scene, "DiningCarpet", CarpetColor);

        SetTableVisualColor(scene, "DiningTable_A");
        SetTableVisualColor(scene, "DiningTable_B");
        SetTableVisualColor(scene, "DiningTable_C");

        SetSpriteColor(scene, "EntranceVisual", EntranceColor);
        SetSpriteColor(scene, "ExitVisual", ExitColor);
        SetSpriteColor(scene, "StaffDoorVisual", StaffDoorColor);
    }

    private static void UpdateExistingLabels(Scene scene)
    {
        ConfigureExistingTextSign(scene, "EntranceSign", "DINING ROOM", SignColor, 0.036f);
        ConfigureExistingTextSign(scene, "ExitSign", "EXIT", SignColor, 0.042f);
        ConfigureExistingTextSign(scene, "StaffDoorSign", "STAFF ONLY", SignColor, 0.034f);
    }

    private static void BuildFallenChair(Scene scene, Sprite sprite, Material material)
    {
        GameObject chair = GetOrCreateRoot(scene, "FallenChair");
        ConfigureTransform(chair.transform, new Vector3(-1.2f, -0.9f, 0f), Quaternion.Euler(0f, 0f, -18f), Vector3.one, false);

        GameObject seat = GetOrCreateChild(chair.transform, "ChairSeat");
        ConfigureSprite(seat, sprite, material, Vector3.zero, new Vector3(0.75f, 0.18f, 1f), ChairColor, 3);

        GameObject back = GetOrCreateChild(chair.transform, "ChairBack");
        ConfigureSprite(back, sprite, material, new Vector3(-0.26f, 0.22f, 0f), new Vector3(0.18f, 0.62f, 1f), ChairColor, 3);
        Undo.RecordObject(back.transform, UndoName);
        back.transform.localRotation = Quaternion.Euler(0f, 0f, 72f);

        RemoveAllCollidersInChildren(chair);
    }

    private static void BuildBrokenLight(Scene scene, Sprite sprite, Material material)
    {
        GameObject light = GetOrCreateRoot(scene, "BrokenLight");
        ConfigureTransform(light.transform, new Vector3(1.4f, 2.6f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject fixture = GetOrCreateChild(light.transform, "LightFixture");
        ConfigureSprite(fixture, sprite, material, Vector3.zero, new Vector3(0.5f, 0.12f, 1f), BrokenLightColor, 3);

        GameObject cord = GetOrCreateChild(light.transform, "BrokenCord");
        ConfigureSprite(cord, sprite, material, new Vector3(0.08f, -0.18f, 0f), new Vector3(0.06f, 0.42f, 1f), WallColor, 3);
        Undo.RecordObject(cord.transform, UndoName);
        cord.transform.localRotation = Quaternion.Euler(0f, 0f, -16f);

        GameObject shard = GetOrCreateChild(light.transform, "DimShard");
        ConfigureSprite(shard, sprite, material, new Vector3(-0.24f, -0.18f, 0f), new Vector3(0.18f, 0.06f, 1f), BrokenLightColor, 3);
        Undo.RecordObject(shard.transform, UndoName);
        shard.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);

        RemoveAllCollidersInChildren(light);
    }

    private static void BuildRedStain(Scene scene, Sprite sprite, Material material)
    {
        GameObject stain = GetOrCreateRoot(scene, "RedStain");
        ConfigureTransform(stain.transform, new Vector3(1.9f, -1.25f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject main = GetOrCreateChild(stain.transform, "RedStainMain");
        ConfigureSprite(main, sprite, material, Vector3.zero, new Vector3(0.82f, 0.28f, 1f), RedStainColor, 2);
        Undo.RecordObject(main.transform, UndoName);
        main.transform.localRotation = Quaternion.Euler(0f, 0f, -8f);

        GameObject spotA = GetOrCreateChild(stain.transform, "RedStainSpotA");
        ConfigureSprite(spotA, sprite, material, new Vector3(-0.32f, 0.12f, 0f), new Vector3(0.25f, 0.14f, 1f), RedStainColor, 2);

        GameObject spotB = GetOrCreateChild(stain.transform, "RedStainSpotB");
        ConfigureSprite(spotB, sprite, material, new Vector3(0.35f, -0.08f, 0f), new Vector3(0.2f, 0.12f, 1f), RedStainColor, 2);

        RemoveAllCollidersInChildren(stain);
    }

    private static void BuildFutureMarkers(Scene scene)
    {
        BuildTriggerMarker(scene, "ChaseStartZone", new Vector3(-0.8f, -1.9f, 0f), new Vector2(2f, 1f));
        BuildEmptyMarker(scene, "MonsterSpawnPoint", new Vector3(-5.5f, 1.2f, 0f));
        BuildTriggerMarker(scene, "EscapeExitZone", new Vector3(6f, -0.3f, 0f), new Vector2(2.3f, 2f));
    }

    private static void BuildTriggerMarker(Scene scene, string objectName, Vector3 worldPosition, Vector2 colliderSize)
    {
        GameObject marker = GetOrCreateRoot(scene, objectName);
        ConfigureTransform(marker.transform, worldPosition, Quaternion.identity, Vector3.one, false);
        RemoveComponentsIfPresent<SpriteRenderer>(marker);
        ConfigureBoxCollider(marker, true, colliderSize);
    }

    private static void BuildEmptyMarker(Scene scene, string objectName, Vector3 worldPosition)
    {
        GameObject marker = GetOrCreateRoot(scene, objectName);
        ConfigureTransform(marker.transform, worldPosition, Quaternion.identity, Vector3.one, false);
        RemoveComponentsIfPresent<SpriteRenderer>(marker);
        RemoveComponentsIfPresent<Collider2D>(marker);
        RemoveComponentsIfPresent<Collider>(marker);
    }

    private static void RemoveEscapeSign(Scene scene)
    {
        Transform escapeSign = FindTransformByName(scene, "EscapeSign");
        if (escapeSign != null)
        {
            Undo.DestroyObjectImmediate(escapeSign.gameObject);
        }
    }

    private static void SetTableVisualColor(Scene scene, string tableName)
    {
        Transform table = FindTransformByName(scene, tableName);
        if (table == null)
        {
            return;
        }

        Transform visual = table.Find("TableVisual");
        if (visual == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = visual.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        Undo.RecordObject(spriteRenderer, UndoName);
        spriteRenderer.color = TableColor;
    }

    private static void SetSpriteColor(Scene scene, string objectName, Color color)
    {
        Transform target = FindTransformByName(scene, objectName);
        if (target == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = target.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (spriteRenderer == null)
        {
            return;
        }

        Undo.RecordObject(spriteRenderer, UndoName);
        spriteRenderer.color = color;
    }

    private static void ConfigureExistingTextSign(Scene scene, string objectName, string text, Color color, float characterSize)
    {
        Transform signTransform = FindTransformByName(scene, objectName);
        if (signTransform == null)
        {
            return;
        }

        ConfigureTextSign(signTransform.gameObject, text, color, signTransform.localPosition, characterSize);
    }

    private static void ConfigureTextSign(GameObject sign, string text, Color color, Vector3 localPosition, float characterSize)
    {
        ConfigureTransform(sign.transform, localPosition, Quaternion.identity, Vector3.one, true);

        TextMesh textMesh = GetOrAddComponent<TextMesh>(sign);
        Undo.RecordObject(textMesh, UndoName);
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.fontSize = 80;
        textMesh.characterSize = characterSize;

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

    private static void RemoveAllCollidersInChildren(GameObject root)
    {
        Collider2D[] collider2Ds = root.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < collider2Ds.Length; i++)
        {
            Undo.DestroyObjectImmediate(collider2Ds[i]);
        }

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Undo.DestroyObjectImmediate(colliders[i]);
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
