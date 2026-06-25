using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ApplyGuest1PixelArtLayout
{
    private const string MenuPath = "Tools/Hotel Hunger/Apply Guest 1 Pixel Art Layout";
    private const string UndoName = "Apply Guest 1 Pixel Art Layout";
    private const float PixelsPerUnit = 18f;

    private const string ConciergeSpritePath = "Assets/Pixel Art/Characters/Concierge_char.png";
    private const string Guest1SpritePath = "Assets/Pixel Art/Characters/Guest_1_char.png";
    private const string LobbyBackgroundPath = "Assets/Pixel Art/Hotel_Lobby/Final.png";
    private const string GlassSpritePath = "Assets/Pixel Art/Hotel_Lobby/Crystal_glass.png";
    private const string StrawSpritePath = "Assets/Pixel Art/Hotel_Lobby/Golden_straw.png";
    private const string SyrupSpritePath = "Assets/Pixel Art/Hotel_Lobby/Red_Syrup.png";
    private const string UmbrellaSpritePath = "Assets/Pixel Art/Hotel_Lobby/Little_umbrella.png";
    private const string CartHallBackgroundPath = "Assets/Pixel Art/4th_Hotel_hall/Final.png";
    private const string TrolleySpritePath = "Assets/Pixel Art/4th_Hotel_hall/Trolly_cart.png";
    private const string ElevatorSignSpritePath = "Assets/Pixel Art/4th_Hotel_hall/Elevator_sign.png";
    private const string WaitingHallBackgroundPath = "Assets/Pixel Art/3rd_Hotel_hall/Final.png";

    private static readonly string[] RequiredSpritePaths =
    {
        ConciergeSpritePath,
        Guest1SpritePath,
        LobbyBackgroundPath,
        GlassSpritePath,
        StrawSpritePath,
        SyrupSpritePath,
        UmbrellaSpritePath,
        CartHallBackgroundPath,
        TrolleySpritePath,
        ElevatorSignSpritePath,
        WaitingHallBackgroundPath
    };

    [MenuItem(MenuPath)]
    private static void Apply()
    {
        if (!EditorUtility.DisplayDialog(
                "Apply Guest 1 Pixel Art Layout",
                "This utility will apply and save pixel-art layout changes to:\n\n" +
                "HotelLobby_XW\n" +
                "Guest1CartHallway_XW\n" +
                "Guest1WaitingHallway_XW\n\n" +
                "It preserves existing gameplay objects, colliders, interactions, transitions, objectives, and scripts.",
                "Apply and Save",
                "Cancel"))
        {
            Debug.Log("Apply Guest 1 Pixel Art Layout cancelled before any scene changes.");
            return;
        }

        if (AnyOpenSceneIsDirty())
        {
            EditorUtility.DisplayDialog(
                "Apply Guest 1 Pixel Art Layout",
                "One or more currently open scenes have unsaved changes. Save or discard them manually, then run this utility again.",
                "OK");
            return;
        }

        AssetDatabase.Refresh();

        if (!VerifyRequiredSpritesExist())
        {
            return;
        }

        ConfigureSpriteImporters();
        AssetDatabase.Refresh();

        string originalScenePath = SceneManager.GetActiveScene().path;

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            ApplyHotelLobby();
            ApplyCartHallway();
            ApplyWaitingHallway();
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!string.IsNullOrWhiteSpace(originalScenePath) &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScenePath) != null)
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        EditorUtility.DisplayDialog(
            "Apply Guest 1 Pixel Art Layout",
            "Guest 1 pixel-art layout was applied and the three target scenes were saved.",
            "OK");
    }

    private static void ApplyHotelLobby()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/HotelLobby_XW.unity", OpenSceneMode.Single);
        GameObject visualRoot = EnsureVisualRoot(scene, "PixelArtVisuals_HotelLobby");
        SetBackground(visualRoot, LobbyBackgroundPath);

        HideRenderers(scene, "LobbyFloor", "TopWall", "BottomWall", "LeftWall", "RightWall");
        HideRenderers(scene, "CounterVisual", "DrinkStationVisual", "ElevatorVisual", "ElevatorSign");
        HideChildRenderers(scene, "ReceptionCounter", "Body", "Head");

        Sprite concierge = LoadSprite(ConciergeSpritePath);
        Sprite guest1 = LoadSprite(Guest1SpritePath);

        GameObject player = FindByName(scene, "Player");
        AssignSprite(player, concierge, 5);
        SetWorldPosition(player, PixelToWorld(160f, 135f));

        GameObject guest = FindByName(scene, "Guest1");
        SetWorldPosition(guest, PixelToWorld(105f, 112f));
        GameObject guestVisual = FindChildByName(guest, "Guest1Visual");
        AssignSprite(guestVisual, guest1, 5);
        SetWorldPosition(FindByName(scene, "Guest1InteractionZone"), PixelToWorld(105f, 126f));

        ConfigureFullCanvasObject(scene, "Glass", "GlassInteractionZone", GlassSpritePath, PixelToWorld(246.5f, 10.5f), 3);
        ConfigureFullCanvasObject(scene, "GoldStraw", "GoldStrawInteractionZone", StrawSpritePath, PixelToWorld(213f, 50f), 3);
        ConfigureFullCanvasObject(scene, "HouseSpecialSyrup", "HouseSpecialSyrupInteractionZone", SyrupSpritePath, PixelToWorld(274f, 11f), 3);
        ConfigureFullCanvasObject(scene, "Umbrella", "UmbrellaInteractionZone", UmbrellaSpritePath, PixelToWorld(302f, 10f), 3);

        GameObject receptionCounter = FindByName(scene, "ReceptionCounter");
        SetWorldPosition(receptionCounter, PixelToWorld(73f, 89f));

        GameObject elevatorDoor = FindByName(scene, "ElevatorDoor");
        SetWorldPosition(elevatorDoor, PixelToWorld(288f, 98f));
        SetLocalPosition(FindChildByName(elevatorDoor, "ElevatorInteractionZone"), new Vector3(0f, -0.45f, 0f));

        SaveScene(scene);
    }

    private static void ApplyCartHallway()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Guest1CartHallway_XW.unity", OpenSceneMode.Single);
        GameObject visualRoot = EnsureVisualRoot(scene, "PixelArtVisuals_CartHallway");
        SetBackground(visualRoot, CartHallBackgroundPath);

        HideRenderers(scene, "LobbyFloor", "TopWall", "BottomWall", "LeftWall", "RightWall", "ElevatorVisual");

        Sprite concierge = LoadSprite(ConciergeSpritePath);
        GameObject player = FindByName(scene, "Player");
        AssignSprite(player, concierge, 5);
        SetWorldPosition(player, PixelToWorld(160f, 132f));
        SetWorldPosition(FindByName(scene, "Spawn_FromHotelLobby"), PixelToWorld(160f, 132f));

        ConfigureCart(scene, "ArrivalCart", PixelToWorld(74f, 124f));
        ConfigureCart(scene, "RoomServiceCart", PixelToWorld(160f, 124f));
        ConfigureCart(scene, "DepartureCart", PixelToWorld(246f, 124f));

        GameObject elevatorDoor = FindByName(scene, "ElevatorDoor");
        SetWorldPosition(elevatorDoor, PixelToWorld(288f, 96f));
        SetLocalPosition(FindChildByName(elevatorDoor, "ElevatorInteractionZone"), new Vector3(0f, -0.8f, 0f));
        EnsureOffsetVisual(elevatorDoor, "PixelArtElevatorSign", ElevatorSignSpritePath, PixelToWorld(12f, 44f), new Vector3(0f, 1.35f, 0f), 3);

        SaveScene(scene);
    }

    private static void ApplyWaitingHallway()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Guest1WaitingHallway_XW.unity", OpenSceneMode.Single);
        GameObject visualRoot = EnsureVisualRoot(scene, "PixelArtVisuals_WaitingHallway");
        SetBackground(visualRoot, WaitingHallBackgroundPath);

        HideRenderers(scene, "LobbyFloor", "TopWall", "BottomWall", "LeftWall", "RightWall", "HallwayRunner");
        HideRenderers(scene, "PrivateElevatorVisual", "WaitingRoomDoorVisual", "PrivateElevatorSign", "WaitingRoomSign");

        Sprite concierge = LoadSprite(ConciergeSpritePath);
        GameObject player = FindByName(scene, "Player");
        AssignSprite(player, concierge, 5);
        SetWorldPosition(player, PixelToWorld(160f, 132f));
        SetWorldPosition(FindByName(scene, "Spawn_FromCartHallway"), PixelToWorld(160f, 132f));

        GameObject luggage = FindByName(scene, "Guest1Luggage");
        SetWorldPosition(luggage, PixelToWorld(126f, 127f));
        SetLocalPosition(FindChildByName(luggage, "Guest1LuggageInteractionZone"), new Vector3(0f, -0.55f, 0f));

        GameObject privateElevator = FindByName(scene, "PrivateElevator");
        SetWorldPosition(privateElevator, PixelToWorld(45f, 94f));
        SetLocalPosition(FindChildByName(privateElevator, "PrivateElevatorInteractionZone"), new Vector3(0f, -0.8f, 0f));

        GameObject waitingRoomDoor = FindByName(scene, "WaitingRoomDoor");
        SetWorldPosition(waitingRoomDoor, PixelToWorld(278f, 94f));
        SetLocalPosition(FindChildByName(waitingRoomDoor, "WaitingRoomInteractionZone"), new Vector3(0f, -0.8f, 0f));

        SaveScene(scene);
    }

    private static void ConfigureCart(Scene scene, string cartName, Vector3 worldPosition)
    {
        GameObject cart = FindByName(scene, cartName);
        SetWorldPosition(cart, worldPosition);

        GameObject cartVisual = FindChildByName(cart, "CartVisual");
        HideRenderer(cartVisual);

        EnsureOffsetVisual(cart, "PixelArtCart", TrolleySpritePath, PixelToWorld(247.5f, 83f), Vector3.zero, 3);

        GameObject interaction = FindChildByName(cart, cartName.Replace("Cart", "CartInteractionZone"));
        if (interaction != null)
        {
            SetLocalPosition(interaction, new Vector3(0f, -0.95f, 0f));
        }
    }

    private static void ConfigureFullCanvasObject(
        Scene scene,
        string objectName,
        string interactionZoneName,
        string spritePath,
        Vector3 visualCenterWorld,
        int sortingOrder)
    {
        GameObject owner = FindByName(scene, objectName);
        SetWorldPosition(owner, visualCenterWorld);
        HideRenderer(owner);
        EnsureOffsetVisual(owner, "PixelArtSprite", spritePath, visualCenterWorld, Vector3.zero, sortingOrder);

        GameObject interactionZone = FindByName(scene, interactionZoneName);
        if (interactionZone == null)
        {
            return;
        }

        Vector3 interactionPosition = visualCenterWorld + new Vector3(0f, -0.75f, 0f);
        if (interactionPosition.y > 3.6f)
        {
            interactionPosition.y = 3.6f;
        }

        SetWorldPosition(interactionZone, interactionPosition);
    }

    private static void EnsureOffsetVisual(
        GameObject parent,
        string childName,
        string spritePath,
        Vector3 sourceVisualCenterWorld,
        Vector3 desiredLocalCenter,
        int sortingOrder)
    {
        if (parent == null)
        {
            return;
        }

        GameObject child = EnsureDirectChild(parent, childName);
        SetLocalPosition(child, desiredLocalCenter - sourceVisualCenterWorld);
        SetLocalScale(child, Vector3.one);
        AssignSprite(child, LoadSprite(spritePath), sortingOrder);
    }

    private static void SetBackground(GameObject visualRoot, string spritePath)
    {
        GameObject background = EnsureDirectChild(visualRoot, "PixelArtBackground");
        SetLocalPosition(background, Vector3.zero);
        SetLocalScale(background, Vector3.one);
        AssignSprite(background, LoadSprite(spritePath), -20);
    }

    private static GameObject EnsureVisualRoot(Scene scene, string rootName)
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

    private static void AssignSprite(GameObject gameObject, Sprite sprite, int sortingOrder)
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
                HideRenderer(objects[j]);
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
            GameObject child = FindChildByName(parent, childNames[i]);
            HideRenderer(child);
        }
    }

    private static void HideRenderer(GameObject gameObject)
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

    private static Vector3 PixelToWorld(float pixelX, float pixelY)
    {
        return new Vector3((pixelX - 160f) / PixelsPerUnit, (90f - pixelY) / PixelsPerUnit, 0f);
    }

    private static Sprite LoadSprite(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static void SaveScene(Scene scene)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static bool AnyOpenSceneIsDirty()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).isDirty)
            {
                return true;
            }
        }

        return false;
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
                "Apply Guest 1 Pixel Art Layout",
                $"Missing required sprite asset:\n{RequiredSpritePaths[i]}",
                "OK");
            return false;
        }

        return true;
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
