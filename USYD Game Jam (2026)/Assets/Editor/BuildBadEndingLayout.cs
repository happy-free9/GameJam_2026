using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildBadEndingLayout
{
    private const string RequiredSceneName = "BadEnding_XW";
    private const string MenuPath = "Tools/Hotel Hunger/Build Bad Ending";
    private const string UndoName = "Build Bad Ending";

    private static readonly string[] CopiedLayoutRoots =
    {
        "WaitingRoomEntrance",
        "DiningExit",
        "StaffOnlyDoor",
        "DiningTable_A",
        "DiningTable_B",
        "DiningTable_C",
        "DiningCarpet",
        "SpilledPlant",
        "WetFloorSign",
        "FallenChair",
        "BrokenLight",
        "RedStain",
        "ChaseStartZone",
        "MonsterSpawnPoint",
        "EscapeExitZone",
        "EscapeSign"
    };

    private static readonly Color BackdropColor = new Color(0.025f, 0.005f, 0.012f, 1f);
    private static readonly Color MouthColor = new Color(0.2f, 0.015f, 0.025f, 1f);
    private static readonly Color TeethColor = new Color(0.72f, 0.64f, 0.56f, 1f);
    private static readonly Color EyeColor = new Color(0.42f, 0.035f, 0.045f, 1f);
    private static readonly Color TitleColor = new Color(0.78f, 0.42f, 0.38f, 1f);
    private static readonly Color SubtitleColor = new Color(0.5f, 0.5f, 0.53f, 1f);
    private static readonly Color SmallTextColor = new Color(0.34f, 0.025f, 0.04f, 1f);

    [MenuItem(MenuPath)]
    private static void BuildOrUpdate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Bad Ending Layout",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Bad Ending Layout",
                "Could not find a SpriteRenderer with a square sprite on Player or LobbyFloor.",
                "OK");
            return;
        }

        Material spriteMaterial = FindSpriteUnlitDefaultMaterial(referenceRenderer);

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        RemoveCopiedLayoutRoots(scene);
        BuildBackdrop(scene, referenceRenderer.sprite, spriteMaterial);
        BuildMouth(scene, referenceRenderer.sprite, spriteMaterial);
        BuildEndingText(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    private static void RemoveCopiedLayoutRoots(Scene scene)
    {
        for (int i = 0; i < CopiedLayoutRoots.Length; i++)
        {
            GameObject root = FindRootGameObject(scene, CopiedLayoutRoots[i]);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }
        }
    }

    private static void BuildBackdrop(Scene scene, Sprite sprite, Material material)
    {
        GameObject backdrop = GetOrCreateRoot(scene, "BadEndingBackdrop");
        ConfigureTransform(backdrop.transform, Vector3.zero, Quaternion.identity, new Vector3(14.5f, 8.5f, 1f), false);
        ConfigureSprite(backdrop, sprite, material, BackdropColor, 10);
        RemoveComponentsIfPresent<Collider2D>(backdrop);
        RemoveComponentsIfPresent<Collider>(backdrop);
    }

    private static void BuildMouth(Scene scene, Sprite sprite, Material material)
    {
        GameObject mouth = GetOrCreateRoot(scene, "BadEndingMouth");
        ConfigureTransform(mouth.transform, new Vector3(0f, 0.2f, -0.1f), Quaternion.identity, Vector3.one, false);
        RemoveComponentsIfPresent<SpriteRenderer>(mouth);
        RemoveComponentsIfPresent<Collider2D>(mouth);
        RemoveComponentsIfPresent<Collider>(mouth);

        GameObject mouthShape = GetOrCreateChild(mouth.transform, "MouthShape");
        ConfigureTransform(mouthShape.transform, new Vector3(0f, -0.15f, 0f), Quaternion.Euler(0f, 0f, -2f), new Vector3(3.4f, 0.72f, 1f), true);
        ConfigureSprite(mouthShape, sprite, material, MouthColor, 11);

        BuildTooth(mouth.transform, sprite, material, "ToothA", new Vector3(-1.15f, 0.28f, 0f), 9f);
        BuildTooth(mouth.transform, sprite, material, "ToothB", new Vector3(-0.38f, 0.32f, 0f), -5f);
        BuildTooth(mouth.transform, sprite, material, "ToothC", new Vector3(0.42f, 0.32f, 0f), 6f);
        BuildTooth(mouth.transform, sprite, material, "ToothD", new Vector3(1.18f, 0.26f, 0f), -8f);
        BuildTooth(mouth.transform, sprite, material, "LowerToothA", new Vector3(-0.72f, -0.62f, 0f), -6f);
        BuildTooth(mouth.transform, sprite, material, "LowerToothB", new Vector3(0.75f, -0.62f, 0f), 7f);

        GameObject leftEye = GetOrCreateChild(mouth.transform, "LeftEye");
        ConfigureTransform(leftEye.transform, new Vector3(-0.7f, 1.05f, 0f), Quaternion.identity, new Vector3(0.34f, 0.18f, 1f), true);
        ConfigureSprite(leftEye, sprite, material, EyeColor, 12);

        GameObject rightEye = GetOrCreateChild(mouth.transform, "RightEye");
        ConfigureTransform(rightEye.transform, new Vector3(0.7f, 1.05f, 0f), Quaternion.identity, new Vector3(0.34f, 0.18f, 1f), true);
        ConfigureSprite(rightEye, sprite, material, EyeColor, 12);

        RemoveAllCollidersInChildren(mouth);
    }

    private static void BuildTooth(Transform parent, Sprite sprite, Material material, string toothName, Vector3 localPosition, float zRotation)
    {
        GameObject tooth = GetOrCreateChild(parent, toothName);
        ConfigureTransform(tooth.transform, localPosition, Quaternion.Euler(0f, 0f, zRotation), new Vector3(0.18f, 0.42f, 1f), true);
        ConfigureSprite(tooth, sprite, material, TeethColor, 12);
    }

    private static void BuildEndingText(Scene scene)
    {
        ConfigureTextRoot(
            scene,
            "BadEndingTitle",
            "YOU BECAME THE FEAST",
            new Vector3(0f, 2.65f, -0.2f),
            TitleColor,
            0.08f,
            100);

        ConfigureTextRoot(
            scene,
            "BadEndingSubtitle",
            "The hotel is still hungry.",
            new Vector3(0f, -2.35f, -0.2f),
            SubtitleColor,
            0.045f,
            80);

        ConfigureTextRoot(
            scene,
            "BadEndingSmallText",
            "ONE OF US",
            new Vector3(0f, -3f, -0.2f),
            SmallTextColor,
            0.035f,
            80);
    }

    private static void ConfigureSprite(GameObject gameObject, Sprite sprite, Material material, Color color, int sortingOrder)
    {
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

    private static void ConfigureTextRoot(
        Scene scene,
        string objectName,
        string text,
        Vector3 worldPosition,
        Color color,
        float characterSize,
        int fontSize)
    {
        GameObject textObject = GetOrCreateRoot(scene, objectName);
        ConfigureTransform(textObject.transform, worldPosition, Quaternion.identity, Vector3.one, false);

        TextMesh textMesh = GetOrAddComponent<TextMesh>(textObject);
        Undo.RecordObject(textMesh, UndoName);
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = characterSize;

        Font builtInFont = GetBuiltInFont();
        if (builtInFont != null)
        {
            textMesh.font = builtInFont;
        }

        MeshRenderer meshRenderer = GetOrAddComponent<MeshRenderer>(textObject);
        Undo.RecordObject(meshRenderer, UndoName);
        if (textMesh.font != null)
        {
            meshRenderer.sharedMaterial = textMesh.font.material;
        }

        meshRenderer.sortingOrder = 13;
        RemoveComponentsIfPresent<SpriteRenderer>(textObject);
        RemoveComponentsIfPresent<Collider2D>(textObject);
        RemoveComponentsIfPresent<Collider>(textObject);
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
