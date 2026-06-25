using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupDiningRoomChaseCutscene
{
    private const string RequiredSceneName = "Guest3DiningRoom_XW";
    private const string MenuPath = "Tools/Hotel Hunger/Setup Dining Room Chase Cutscene";
    private const string UndoName = "Setup Dining Room Chase Cutscene";

    private static readonly string[] OldRootsToRemove =
    {
        "WaitingRoomEntrance",
        "StaffOnlyDoor",
        "DiningTable_A",
        "DiningTable_B",
        "DiningTable_C",
        "DiningCarpet",
        "SpilledPlant",
        "WetFloorSign"
    };

    private static readonly string[] NewRootsToRecreate =
    {
        "LongDiningTable",
        "DiningChairs",
        "DiningTableSettings",
        "CutsceneGuest2",
        "CutsceneHotelStaff",
        "DiningRoomChaseDirector",
        "DiningRoomChaseWaypoints"
    };

    private static readonly Color DoorColor = new Color(0.13f, 0.02f, 0.04f, 1f);
    private static readonly Color TableColor = new Color(0.26f, 0.15f, 0.08f, 1f);
    private static readonly Color TableHighlightColor = new Color(0.45f, 0.27f, 0.13f, 1f);
    private static readonly Color ChairColor = new Color(0.19f, 0.1f, 0.05f, 1f);
    private static readonly Color PlateColor = new Color(0.88f, 0.84f, 0.72f, 1f);
    private static readonly Color GlassColor = new Color(0.55f, 0.78f, 0.92f, 0.82f);
    private static readonly Color CutleryColor = new Color(0.78f, 0.78f, 0.72f, 1f);
    private static readonly Color MenuColor = new Color(0.93f, 0.78f, 0.46f, 1f);
    private static readonly Color GuestColor = new Color(0.72f, 0.86f, 0.96f, 1f);
    private static readonly Color GuestHeadColor = new Color(0.93f, 0.76f, 0.58f, 1f);
    private static readonly Color StaffColor = new Color(0.09f, 0.1f, 0.13f, 1f);
    private static readonly Color StaffHeadColor = new Color(0.62f, 0.48f, 0.38f, 1f);
    private static readonly Color SignColor = new Color(0.95f, 0.86f, 0.58f, 1f);

    private static readonly Vector3 DoorVisualScale = new Vector3(1.55f, 2.45f, 1f);
    private const float DoorY = 0.55f;
    private const float FallbackRightWallInnerFaceX = 7.12f;

    private readonly struct DoorLayout
    {
        public DoorLayout(float innerFaceX, float centerX, float y)
        {
            InnerFaceX = innerFaceX;
            CenterX = centerX;
            Y = y;
        }

        public float InnerFaceX { get; }
        public float CenterX { get; }
        public float Y { get; }
        public float StaffStartX => InnerFaceX - 0.35f;
        public float StaffDoorInsideX => CenterX - 0.28f;
        public float ExitDoorX => CenterX;
        public float ExitHiddenX => InnerFaceX - 0.35f;
    }

    [MenuItem(MenuPath)]
    private static void Setup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Dining Room Chase Cutscene",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Dining Room Chase Cutscene",
                "Could not find a SpriteRenderer with a square sprite on Player or LobbyFloor.",
                "OK");
            return;
        }

        Material spriteMaterial = FindSpriteUnlitDefaultMaterial(referenceRenderer);

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            DoorLayout doorLayout = CreateDoorLayout(scene, referenceRenderer.sprite);

            RemoveOldLayout(scene);
            RecreateCutsceneRoots(scene);

            ConfigureDiningExit(scene, referenceRenderer.sprite, spriteMaterial, doorLayout);
            BuildLongDiningTable(scene, referenceRenderer.sprite, spriteMaterial);
            BuildDiningChairs(scene, referenceRenderer.sprite, spriteMaterial);
            BuildDiningTableSettings(scene, referenceRenderer.sprite, spriteMaterial);

            Transform guest2 = BuildActor(
                scene,
                "CutsceneGuest2",
                referenceRenderer.sprite,
                spriteMaterial,
                new Vector3(-3.15f, 1.05f, 0f),
                GuestColor,
                GuestHeadColor,
                false);

            Transform staff = BuildActor(
                scene,
                "CutsceneHotelStaff",
                referenceRenderer.sprite,
                spriteMaterial,
                new Vector3(doorLayout.StaffStartX, doorLayout.Y, 0f),
                StaffColor,
                StaffHeadColor,
                true);

            BuildWaypoints(scene, doorLayout, out Transform[] staffEntry, out Transform[] guestRun, out Transform[] staffChase);
            BuildDirector(scene, guest2, staff, staffEntry, guestRun, staffChase);
            SetGameObjectActive(staff.gameObject, false);

            EditorSceneManager.MarkSceneDirty(scene);
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }

        EditorUtility.DisplayDialog(
            "Dining Room Chase Cutscene",
            "Dining Room cutscene setup complete. Save the scene, then play from Guest3DiningRoom_XW.",
            "OK");
    }

    private static DoorLayout CreateDoorLayout(Scene scene, Sprite doorSprite)
    {
        float innerFaceX = FallbackRightWallInnerFaceX;
        SpriteRenderer rightWallRenderer = FindRendererOnSceneObject(scene, "RightWall");
        if (rightWallRenderer != null && rightWallRenderer.bounds.size.x > 0.01f)
        {
            innerFaceX = rightWallRenderer.bounds.min.x;
        }

        float doorWidth = DoorVisualScale.x;
        if (doorSprite != null && doorSprite.bounds.size.x > 0.01f)
        {
            doorWidth = Mathf.Abs(doorSprite.bounds.size.x * DoorVisualScale.x);
        }

        float doorCenterX = innerFaceX - (doorWidth * 0.5f);
        return new DoorLayout(innerFaceX, doorCenterX, DoorY);
    }

    private static void RemoveOldLayout(Scene scene)
    {
        for (int i = 0; i < OldRootsToRemove.Length; i++)
        {
            GameObject root = FindRootGameObject(scene, OldRootsToRemove[i]);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }
        }

        GameObject diningExit = FindRootGameObject(scene, "DiningExit");
        if (diningExit != null)
        {
            Transform interactionZone = diningExit.transform.Find("DiningExitInteractionZone");
            if (interactionZone != null)
            {
                Undo.DestroyObjectImmediate(interactionZone.gameObject);
            }

            RemoveComponentsInChildren<SceneTransitionTrigger>(diningExit);
            RemoveComponentsInChildren<InteractionTarget>(diningExit);
            RemoveComponentsInChildren<Collider2D>(diningExit);
            RemoveComponentsInChildren<Collider>(diningExit);
        }
    }

    private static void RecreateCutsceneRoots(Scene scene)
    {
        for (int i = 0; i < NewRootsToRecreate.Length; i++)
        {
            GameObject root = FindRootGameObject(scene, NewRootsToRecreate[i]);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }
        }
    }

    private static void ConfigureDiningExit(Scene scene, Sprite sprite, Material material, DoorLayout doorLayout)
    {
        GameObject diningExit = GetOrCreateRoot(scene, "DiningExit");
        ConfigureTransform(diningExit.transform, new Vector3(doorLayout.CenterX, doorLayout.Y, 0f), Quaternion.identity, Vector3.one, false);
        RemoveChildrenExcept(diningExit.transform, "ExitVisual", "ExitSign");

        GameObject visual = GetOrCreateChild(diningExit.transform, "ExitVisual");
        ConfigureSprite(visual, sprite, material, Vector3.zero, DoorVisualScale, DoorColor, 1);

        GameObject sign = GetOrCreateChild(diningExit.transform, "ExitSign");
        ConfigureTextSign(sign, "DINING EXIT", new Vector3(0f, 1.5f, 0f), 0.05f);

        RemoveComponentsInChildren<SceneTransitionTrigger>(diningExit);
        RemoveComponentsInChildren<InteractionTarget>(diningExit);
        RemoveComponentsInChildren<Collider2D>(diningExit);
        RemoveComponentsInChildren<Collider>(diningExit);
    }

    private static void BuildLongDiningTable(Scene scene, Sprite sprite, Material material)
    {
        GameObject table = GetOrCreateRoot(scene, "LongDiningTable");
        ConfigureTransform(table.transform, new Vector3(0f, 0.05f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject top = GetOrCreateChild(table.transform, "TableTop");
        ConfigureSprite(top, sprite, material, Vector3.zero, new Vector3(7.25f, 1.18f, 1f), TableColor, 1);

        GameObject highlight = GetOrCreateChild(table.transform, "TableRunner");
        ConfigureSprite(highlight, sprite, material, new Vector3(0f, 0.02f, 0f), new Vector3(6.65f, 0.14f, 1f), TableHighlightColor, 2);

        RemoveGameplayComponents(table);
    }

    private static void BuildDiningChairs(Scene scene, Sprite sprite, Material material)
    {
        GameObject chairs = GetOrCreateRoot(scene, "DiningChairs");
        ConfigureTransform(chairs.transform, Vector3.zero, Quaternion.identity, Vector3.one, false);

        float[] xs = { -3.05f, -1.8f, -0.55f, 0.7f, 1.95f, 3.2f };
        for (int i = 0; i < xs.Length; i++)
        {
            BuildChair(chairs.transform, sprite, material, $"TopChair_{i + 1}", new Vector3(xs[i], 1.0f, 0f), true);
            BuildChair(chairs.transform, sprite, material, $"BottomChair_{i + 1}", new Vector3(xs[i], -0.9f, 0f), false);
        }

        RemoveGameplayComponents(chairs);
    }

    private static void BuildChair(Transform parent, Sprite sprite, Material material, string name, Vector3 localPosition, bool topSide)
    {
        GameObject chair = GetOrCreateChild(parent, name);
        ConfigureTransform(chair.transform, localPosition, Quaternion.identity, Vector3.one, true);

        GameObject seat = GetOrCreateChild(chair.transform, "Seat");
        ConfigureSprite(seat, sprite, material, Vector3.zero, new Vector3(0.55f, 0.28f, 1f), ChairColor, 0);

        float backY = topSide ? 0.22f : -0.22f;
        GameObject back = GetOrCreateChild(chair.transform, "Back");
        ConfigureSprite(back, sprite, material, new Vector3(0f, backY, 0f), new Vector3(0.62f, 0.14f, 1f), ChairColor, 0);
    }

    private static void BuildDiningTableSettings(Scene scene, Sprite sprite, Material material)
    {
        GameObject settings = GetOrCreateRoot(scene, "DiningTableSettings");
        ConfigureTransform(settings.transform, Vector3.zero, Quaternion.identity, Vector3.one, false);

        float[] xs = { -3.0f, -1.8f, -0.6f, 0.6f, 1.8f, 3.0f };
        for (int i = 0; i < xs.Length; i++)
        {
            BuildPlaceSetting(settings.transform, sprite, material, $"TopSetting_{i + 1}", new Vector3(xs[i], 0.34f, 0f));
            BuildPlaceSetting(settings.transform, sprite, material, $"BottomSetting_{i + 1}", new Vector3(xs[i], -0.26f, 0f));
        }

        GameObject menu = GetOrCreateChild(settings.transform, "CenterMenu");
        ConfigureSprite(menu, sprite, material, new Vector3(-0.35f, 0.03f, 0f), new Vector3(0.42f, 0.25f, 1f), MenuColor, 3);
        Undo.RecordObject(menu.transform, UndoName);
        menu.transform.localRotation = Quaternion.Euler(0f, 0f, -8f);

        GameObject candle = GetOrCreateChild(settings.transform, "CenterCandle");
        ConfigureSprite(candle, sprite, material, new Vector3(0.35f, 0.03f, 0f), new Vector3(0.16f, 0.38f, 1f), SignColor, 3);

        RemoveGameplayComponents(settings);
    }

    private static void BuildPlaceSetting(Transform parent, Sprite sprite, Material material, string name, Vector3 localPosition)
    {
        GameObject setting = GetOrCreateChild(parent, name);
        ConfigureTransform(setting.transform, localPosition, Quaternion.identity, Vector3.one, true);

        GameObject plate = GetOrCreateChild(setting.transform, "Plate");
        ConfigureSprite(plate, sprite, material, Vector3.zero, new Vector3(0.34f, 0.22f, 1f), PlateColor, 3);

        GameObject glass = GetOrCreateChild(setting.transform, "Glass");
        ConfigureSprite(glass, sprite, material, new Vector3(0.25f, 0.02f, 0f), new Vector3(0.13f, 0.2f, 1f), GlassColor, 4);

        GameObject fork = GetOrCreateChild(setting.transform, "Fork");
        ConfigureSprite(fork, sprite, material, new Vector3(-0.26f, 0f, 0f), new Vector3(0.04f, 0.3f, 1f), CutleryColor, 4);

        GameObject knife = GetOrCreateChild(setting.transform, "Knife");
        ConfigureSprite(knife, sprite, material, new Vector3(0.42f, 0f, 0f), new Vector3(0.04f, 0.3f, 1f), CutleryColor, 4);
    }

    private static Transform BuildActor(
        Scene scene,
        string actorName,
        Sprite sprite,
        Material material,
        Vector3 position,
        Color bodyColor,
        Color headColor,
        bool addStaffCap)
    {
        GameObject actor = GetOrCreateRoot(scene, actorName);
        ConfigureTransform(actor.transform, position, Quaternion.identity, Vector3.one, false);

        GameObject body = GetOrCreateChild(actor.transform, "Body");
        ConfigureSprite(body, sprite, material, new Vector3(0f, -0.18f, 0f), new Vector3(0.42f, 0.72f, 1f), bodyColor, 5);

        GameObject head = GetOrCreateChild(actor.transform, "Head");
        ConfigureSprite(head, sprite, material, new Vector3(0f, 0.3f, 0f), new Vector3(0.34f, 0.3f, 1f), headColor, 6);

        if (addStaffCap)
        {
            GameObject cap = GetOrCreateChild(actor.transform, "Cap");
            ConfigureSprite(cap, sprite, material, new Vector3(0f, 0.5f, 0f), new Vector3(0.42f, 0.12f, 1f), bodyColor, 7);
        }

        RemoveGameplayComponents(actor);
        return actor.transform;
    }

    private static void BuildWaypoints(
        Scene scene,
        DoorLayout doorLayout,
        out Transform[] staffEntry,
        out Transform[] guestRun,
        out Transform[] staffChase)
    {
        GameObject root = GetOrCreateRoot(scene, "DiningRoomChaseWaypoints");
        ConfigureTransform(root.transform, Vector3.zero, Quaternion.identity, Vector3.one, false);

        Transform staffHiddenDoor = CreateWaypoint(root.transform, "Staff_HiddenInsideRightDoor", new Vector3(doorLayout.StaffStartX, doorLayout.Y, 0f));
        Transform staffDoor = CreateWaypoint(root.transform, "Staff_RightDoorInside", new Vector3(doorLayout.StaffDoorInsideX, doorLayout.Y, 0f));
        Transform staffNotice = CreateWaypoint(root.transform, "Staff_NoticeUpperRight", new Vector3(2.9f, 1.12f, 0f));

        Transform guestUpperLeft = CreateWaypoint(root.transform, "Guest_UpperLeft", new Vector3(-3.15f, 1.05f, 0f));
        Transform guestLeftSide = CreateWaypoint(root.transform, "Guest_LeftSideDown", new Vector3(-4.2f, 0.0f, 0f));
        Transform guestBottomLeft = CreateWaypoint(root.transform, "Guest_BottomLeft", new Vector3(-3.35f, -1.22f, 0f));
        Transform guestBottomMiddle = CreateWaypoint(root.transform, "Guest_BottomMiddle", new Vector3(0.0f, -1.45f, 0f));
        Transform guestBottomRight = CreateWaypoint(root.transform, "Guest_BottomRight", new Vector3(3.45f, -1.22f, 0f));
        Transform guestRightSide = CreateWaypoint(root.transform, "Guest_RightSideUp", new Vector3(4.35f, 0.15f, 0f));
        Transform guestDoor = CreateWaypoint(root.transform, "Guest_RightDoorInside", new Vector3(doorLayout.ExitDoorX, doorLayout.Y, 0f));
        Transform guestHiddenDoor = CreateWaypoint(root.transform, "Guest_HiddenInsideRightDoor", new Vector3(doorLayout.ExitHiddenX, doorLayout.Y, 0f));

        Transform staffUpperLeft = CreateWaypoint(root.transform, "Staff_UpperLeftJoin", new Vector3(-3.05f, 1.08f, 0f));
        Transform staffLeftSide = CreateWaypoint(root.transform, "Staff_LeftSideDown", new Vector3(-4.12f, -0.05f, 0f));
        Transform staffBottomLeft = CreateWaypoint(root.transform, "Staff_BottomLeft", new Vector3(-3.2f, -1.14f, 0f));
        Transform staffBottomMiddle = CreateWaypoint(root.transform, "Staff_BottomMiddle", new Vector3(0.1f, -1.36f, 0f));
        Transform staffBottomRight = CreateWaypoint(root.transform, "Staff_BottomRight", new Vector3(3.55f, -1.14f, 0f));
        Transform staffRightSide = CreateWaypoint(root.transform, "Staff_RightSideUp", new Vector3(4.45f, 0.18f, 0f));
        Transform staffExitDoor = CreateWaypoint(root.transform, "Staff_RightDoorExit", new Vector3(doorLayout.ExitDoorX, doorLayout.Y, 0f));
        Transform staffHiddenExit = CreateWaypoint(root.transform, "Staff_HiddenInsideRightDoorExit", new Vector3(doorLayout.ExitHiddenX, doorLayout.Y, 0f));

        staffEntry = new[] { staffHiddenDoor, staffDoor, staffNotice };
        guestRun = new[]
        {
            guestUpperLeft,
            guestLeftSide,
            guestBottomLeft,
            guestBottomMiddle,
            guestBottomRight,
            guestRightSide,
            guestDoor,
            guestHiddenDoor
        };

        staffChase = new[]
        {
            staffNotice,
            staffUpperLeft,
            staffLeftSide,
            staffBottomLeft,
            staffBottomMiddle,
            staffBottomRight,
            staffRightSide,
            staffExitDoor,
            staffHiddenExit
        };
    }

    private static void BuildDirector(
        Scene scene,
        Transform guest2,
        Transform staff,
        Transform[] staffEntry,
        Transform[] guestRun,
        Transform[] staffChase)
    {
        GameObject director = GetOrCreateRoot(scene, "DiningRoomChaseDirector");
        ConfigureTransform(director.transform, Vector3.zero, Quaternion.identity, Vector3.one, false);

        DiningRoomChaseCutscene cutscene = GetOrAddComponent<DiningRoomChaseCutscene>(director);
        Undo.RecordObject(cutscene, UndoName);

        GameObject player = FindRootGameObject(scene, "Player");
        SidePlayerController playerController = player != null ? player.GetComponent<SidePlayerController>() : null;
        PlayerInteractor playerInteractor = player != null ? player.GetComponent<PlayerInteractor>() : null;
        Rigidbody2D playerBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
        SpriteRenderer[] playerRenderers = player != null ? player.GetComponentsInChildren<SpriteRenderer>(true) : new SpriteRenderer[0];

        cutscene.Configure(
            player,
            playerController,
            playerInteractor,
            playerBody,
            playerRenderers,
            guest2,
            staff,
            staffEntry,
            guestRun,
            staffChase,
            false,
            string.Empty,
            string.Empty);

        EditorUtility.SetDirty(cutscene);
    }

    private static Transform CreateWaypoint(Transform parent, string name, Vector3 worldPosition)
    {
        GameObject waypoint = GetOrCreateChild(parent, name);
        ConfigureTransform(waypoint.transform, worldPosition, Quaternion.identity, Vector3.one, false);
        return waypoint.transform;
    }

    private static void SetGameObjectActive(GameObject gameObject, bool isActive)
    {
        if (gameObject == null || gameObject.activeSelf == isActive)
        {
            return;
        }

        Undo.RecordObject(gameObject, UndoName);
        gameObject.SetActive(isActive);
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

        RemoveComponentsIfPresent<Collider2D>(gameObject);
        RemoveComponentsIfPresent<Collider>(gameObject);
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

    private static void RemoveChildrenExcept(Transform parent, params string[] childNamesToKeep)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (!Contains(childNamesToKeep, child.name))
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
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

    private static void RemoveGameplayComponents(GameObject root)
    {
        RemoveComponentsInChildren<PlayerInteractor>(root);
        RemoveComponentsInChildren<SidePlayerController>(root);
        RemoveComponentsInChildren<SceneTransitionTrigger>(root);
        RemoveComponentsInChildren<InteractionTarget>(root);
        RemoveComponentsInChildren<Rigidbody2D>(root);
        RemoveComponentsInChildren<Collider2D>(root);
        RemoveComponentsInChildren<Collider>(root);
    }

    private static void RemoveComponentsInChildren<T>(GameObject root) where T : Component
    {
        T[] components = root.GetComponentsInChildren<T>(true);
        for (int i = 0; i < components.Length; i++)
        {
            Undo.DestroyObjectImmediate(components[i]);
        }
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
