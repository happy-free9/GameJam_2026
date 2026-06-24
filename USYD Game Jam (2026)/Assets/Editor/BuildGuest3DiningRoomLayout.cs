using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildGuest3DiningRoomLayout
{
    private const string RequiredSceneName = "Guest3DiningRoom_XW";
    private const string MenuPath = "Tools/Hotel Hunger/Build Guest 3 Dining Room";
    private const string UndoName = "Build Guest 3 Dining Room";

    private static readonly string[] CopiedGuest1Roots =
    {
        "PrivateElevator",
        "Guest1Luggage",
        "WaitingRoomDoor",
        "HallwayRunner"
    };

    private static readonly Color EntranceColor = new Color(0.28f, 0.2f, 0.11f, 1f);
    private static readonly Color ExitColor = new Color(0.13f, 0.02f, 0.04f, 1f);
    private static readonly Color StaffDoorColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    private static readonly Color TableColor = new Color(0.26f, 0.15f, 0.08f, 1f);
    private static readonly Color CarpetColor = new Color(0.32f, 0.08f, 0.07f, 1f);
    private static readonly Color PlantPotColor = new Color(0.12f, 0.07f, 0.04f, 1f);
    private static readonly Color PlantLeafColor = new Color(0.16f, 0.45f, 0.17f, 1f);
    private static readonly Color WetFloorColor = new Color(0.95f, 0.58f, 0.08f, 1f);
    private static readonly Color SignColor = new Color(0.95f, 0.86f, 0.58f, 1f);

    [MenuItem(MenuPath)]
    private static void BuildOrUpdate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Dining Room Layout",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Dining Room Layout",
                "Could not find a SpriteRenderer with a square sprite on Player or LobbyFloor.",
                "OK");
            return;
        }

        Material spriteMaterial = FindSpriteUnlitDefaultMaterial(referenceRenderer);

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        RemoveCopiedGuest1Roots(scene);
        MovePlayer(scene, new Vector3(-5f, -2.3f, 0f));

        BuildWaitingRoomEntrance(scene, referenceRenderer.sprite, spriteMaterial);
        BuildDiningExit(scene, referenceRenderer.sprite, spriteMaterial);
        BuildStaffOnlyDoor(scene, referenceRenderer.sprite, spriteMaterial);
        BuildDiningTables(scene, referenceRenderer.sprite, spriteMaterial);
        BuildDiningCarpet(scene, referenceRenderer.sprite, spriteMaterial);
        BuildSpilledPlant(scene, referenceRenderer.sprite, spriteMaterial);
        BuildWetFloorSign(scene, referenceRenderer.sprite, spriteMaterial);

        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    private static void RemoveCopiedGuest1Roots(Scene scene)
    {
        for (int i = 0; i < CopiedGuest1Roots.Length; i++)
        {
            GameObject rootObject = FindRootGameObject(scene, CopiedGuest1Roots[i]);
            if (rootObject != null)
            {
                Undo.DestroyObjectImmediate(rootObject);
            }
        }
    }

    private static void MovePlayer(Scene scene, Vector3 worldPosition)
    {
        Transform player = FindTransformByName(scene, "Player");
        if (player == null)
        {
            return;
        }

        Undo.RecordObject(player, UndoName);
        player.position = worldPosition;
    }

    private static void BuildWaitingRoomEntrance(Scene scene, Sprite sprite, Material material)
    {
        GameObject entrance = GetOrCreateRoot(scene, "WaitingRoomEntrance");
        ConfigureTransform(entrance.transform, new Vector3(-6.3f, 1f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(entrance.transform, "EntranceVisual");
        ConfigureSprite(visual, sprite, material, Vector3.zero, new Vector3(1.8f, 2.8f, 1f), EntranceColor, 1);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);

        GameObject sign = GetOrCreateChild(entrance.transform, "EntranceSign");
        ConfigureTextSign(sign, "FROM WAITING ROOM", new Vector3(0f, 1.75f, 0f), 0.043f);

        GameObject zone = GetOrCreateChild(entrance.transform, "WaitingRoomEntranceInteractionZone");
        ConfigureTransform(zone.transform, new Vector3(0f, -1f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(zone);
        ConfigureBoxCollider(zone, true, new Vector2(2.5f, 1.2f));
    }

    private static void BuildDiningExit(Scene scene, Sprite sprite, Material material)
    {
        GameObject diningExit = GetOrCreateRoot(scene, "DiningExit");
        ConfigureTransform(diningExit.transform, new Vector3(6.3f, 1f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(diningExit.transform, "ExitVisual");
        ConfigureSprite(visual, sprite, material, Vector3.zero, new Vector3(1.9f, 2.9f, 1f), ExitColor, 1);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);

        GameObject sign = GetOrCreateChild(diningExit.transform, "ExitSign");
        ConfigureTextSign(sign, "DINING EXIT", new Vector3(0f, 1.8f, 0f), 0.05f);

        GameObject zone = GetOrCreateChild(diningExit.transform, "DiningExitInteractionZone");
        ConfigureTransform(zone.transform, new Vector3(0f, -1f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(zone);
        ConfigureBoxCollider(zone, true, new Vector2(2.6f, 1.2f));
    }

    private static void BuildStaffOnlyDoor(Scene scene, Sprite sprite, Material material)
    {
        GameObject staffDoor = GetOrCreateRoot(scene, "StaffOnlyDoor");
        ConfigureTransform(staffDoor.transform, new Vector3(3.4f, 1f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(staffDoor.transform, "StaffDoorVisual");
        ConfigureSprite(visual, sprite, material, Vector3.zero, new Vector3(1.25f, 2.4f, 1f), StaffDoorColor, 1);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);

        GameObject sign = GetOrCreateChild(staffDoor.transform, "StaffDoorSign");
        ConfigureTextSign(sign, "STAFF ONLY", new Vector3(0f, 1.48f, 0f), 0.04f);

        GameObject zone = GetOrCreateChild(staffDoor.transform, "StaffOnlyDoorInteractionZone");
        ConfigureTransform(zone.transform, new Vector3(0f, -0.9f, 0f), Quaternion.identity, Vector3.one, true);
        RemoveComponentsIfPresent<SpriteRenderer>(zone);
        ConfigureBoxCollider(zone, true, new Vector2(1.7f, 1.1f));
    }

    private static void BuildDiningTables(Scene scene, Sprite sprite, Material material)
    {
        BuildDiningTable(scene, sprite, material, "DiningTable_A", new Vector3(-3.1f, 0.9f, 0f));
        BuildDiningTable(scene, sprite, material, "DiningTable_B", new Vector3(0f, 1.35f, 0f));
        BuildDiningTable(scene, sprite, material, "DiningTable_C", new Vector3(3.5f, -0.1f, 0f));
    }

    private static void BuildDiningTable(Scene scene, Sprite sprite, Material material, string tableName, Vector3 worldPosition)
    {
        GameObject table = GetOrCreateRoot(scene, tableName);
        ConfigureTransform(table.transform, worldPosition, Quaternion.identity, Vector3.one, false);

        GameObject visual = GetOrCreateChild(table.transform, "TableVisual");
        ConfigureSprite(visual, sprite, material, Vector3.zero, new Vector3(1.9f, 1f, 1f), TableColor, 1);
        RemoveComponentsIfPresent<Collider2D>(visual);
        RemoveComponentsIfPresent<Collider>(visual);
        ConfigureBoxCollider(table, false, new Vector2(1.9f, 1f));
    }

    private static void BuildDiningCarpet(Scene scene, Sprite sprite, Material material)
    {
        GameObject carpet = GetOrCreateRoot(scene, "DiningCarpet");
        ConfigureSprite(carpet, sprite, material, new Vector3(0f, -1.55f, 0f), new Vector3(12.6f, 1.05f, 1f), CarpetColor, -5);
        RemoveComponentsIfPresent<Collider2D>(carpet);
        RemoveComponentsIfPresent<Collider>(carpet);
    }

    private static void BuildSpilledPlant(Scene scene, Sprite sprite, Material material)
    {
        GameObject plant = GetOrCreateRoot(scene, "SpilledPlant");
        ConfigureTransform(plant.transform, new Vector3(-1.8f, -1.35f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject pot = GetOrCreateChild(plant.transform, "PlantPot");
        ConfigureSprite(pot, sprite, material, new Vector3(0f, -0.08f, 0f), new Vector3(0.42f, 0.25f, 1f), PlantPotColor, 2);

        GameObject leafLeft = GetOrCreateChild(plant.transform, "LeafLeft");
        ConfigureSprite(leafLeft, sprite, material, new Vector3(-0.18f, 0.18f, 0f), new Vector3(0.36f, 0.18f, 1f), PlantLeafColor, 3);
        Undo.RecordObject(leafLeft.transform, UndoName);
        leafLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);

        GameObject leafRight = GetOrCreateChild(plant.transform, "LeafRight");
        ConfigureSprite(leafRight, sprite, material, new Vector3(0.18f, 0.18f, 0f), new Vector3(0.36f, 0.18f, 1f), PlantLeafColor, 3);
        Undo.RecordObject(leafRight.transform, UndoName);
        leafRight.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);

        RemoveAllCollidersInChildren(plant);
    }

    private static void BuildWetFloorSign(Scene scene, Sprite sprite, Material material)
    {
        GameObject wetFloorSign = GetOrCreateRoot(scene, "WetFloorSign");
        ConfigureTransform(wetFloorSign.transform, new Vector3(0.5f, -1.35f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject baseVisual = GetOrCreateChild(wetFloorSign.transform, "WetFloorSignVisual");
        ConfigureSprite(baseVisual, sprite, material, Vector3.zero, new Vector3(0.42f, 0.58f, 1f), WetFloorColor, 3);

        GameObject topStripe = GetOrCreateChild(wetFloorSign.transform, "WetFloorSignTop");
        ConfigureSprite(topStripe, sprite, material, new Vector3(0f, 0.2f, 0f), new Vector3(0.5f, 0.08f, 1f), SignColor, 4);

        RemoveAllCollidersInChildren(wetFloorSign);
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

    private static void ConfigureTextSign(GameObject sign, string text, Vector3 localPosition, float characterSize)
    {
        ConfigureTransform(sign.transform, localPosition, Quaternion.identity, Vector3.one, true);

        TextMesh textMesh = GetOrAddComponent<TextMesh>(sign);
        Undo.RecordObject(textMesh, UndoName);
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = SignColor;
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
