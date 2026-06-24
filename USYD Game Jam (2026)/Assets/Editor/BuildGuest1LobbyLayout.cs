using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildGuest1LobbyLayout
{
    private const string RequiredSceneName = "HotelLobby_XW";
    private const string MenuPath = "Tools/Hotel Hunger/Build or Update Guest 1 Lobby Layout";
    private const string UndoName = "Build or Update Guest 1 Lobby Layout";

    private static readonly Color DarkWarmBrown = new Color(0.23f, 0.12f, 0.05f, 1f);
    private static readonly Color WarmGoldBrown = new Color(0.62f, 0.39f, 0.14f, 1f);
    private static readonly Color GuestBlue = new Color(0.1f, 0.32f, 0.85f, 1f);
    private static readonly Color GuestHead = new Color(0.94f, 0.82f, 0.66f, 1f);
    private static readonly Color PaleBlue = new Color(0.58f, 0.84f, 0.95f, 1f);
    private static readonly Color Gold = new Color(0.95f, 0.73f, 0.18f, 1f);
    private static readonly Color DarkRed = new Color(0.45f, 0.04f, 0.04f, 1f);
    private static readonly Color Pink = new Color(0.95f, 0.42f, 0.68f, 1f);
    private static readonly Color ElevatorDark = new Color(0.18f, 0.16f, 0.15f, 1f);
    private static readonly Color ElevatorSignLight = new Color(0.9f, 0.78f, 0.48f, 1f);

    [MenuItem(MenuPath)]
    private static void BuildOrUpdate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name != RequiredSceneName)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Lobby Layout",
                $"Open the {RequiredSceneName} scene before running this utility.",
                "OK");
            return;
        }

        SpriteRenderer referenceRenderer = FindReferenceRenderer(scene);
        if (referenceRenderer == null || referenceRenderer.sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Lobby Layout",
                "Could not find a SpriteRenderer with a square sprite on Player or LobbyFloor.",
                "OK");
            return;
        }

        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        BuildReceptionCounter(scene, referenceRenderer);
        BuildDrinkStation(scene, referenceRenderer);
        BuildGuest1(scene, referenceRenderer);
        BuildElevatorDoor(scene, referenceRenderer);

        EditorSceneManager.MarkSceneDirty(scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    private static void BuildReceptionCounter(Scene scene, SpriteRenderer referenceRenderer)
    {
        GameObject receptionCounter = GetOrCreateRoot(scene, "ReceptionCounter");
        ConfigureTransform(receptionCounter.transform, new Vector3(-4.1f, 0.55f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject counterVisual = GetOrCreateChild(receptionCounter.transform, "CounterVisual");
        ConfigureSprite(
            counterVisual,
            referenceRenderer,
            Vector3.zero,
            new Vector3(0.8f, 3.4f, 1f),
            DarkWarmBrown,
            1,
            true);
        ConfigureBoxCollider(counterVisual, false, Vector2.one);
    }

    private static void BuildDrinkStation(Scene scene, SpriteRenderer referenceRenderer)
    {
        GameObject drinkStation = GetOrCreateRoot(scene, "DrinkStation");
        ConfigureTransform(drinkStation.transform, new Vector3(-1.75f, -1.1f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject drinkStationVisual = GetOrCreateChild(drinkStation.transform, "DrinkStationVisual");
        ConfigureSprite(
            drinkStationVisual,
            referenceRenderer,
            Vector3.zero,
            new Vector3(4.4f, 0.8f, 1f),
            WarmGoldBrown,
            1,
            true);
        ConfigureBoxCollider(drinkStationVisual, false, Vector2.one);

        BuildIngredient(drinkStation.transform, referenceRenderer, "Glass", new Vector3(-3.25f, -0.15f, 0f), new Vector3(-3.25f, 0.2f, 0f), PaleBlue);
        BuildIngredient(drinkStation.transform, referenceRenderer, "GoldStraw", new Vector3(-2.35f, -0.15f, 0f), new Vector3(-2.35f, 0.2f, 0f), Gold);
        BuildIngredient(drinkStation.transform, referenceRenderer, "HouseSpecialSyrup", new Vector3(-1.45f, -0.15f, 0f), new Vector3(-1.45f, 0.2f, 0f), DarkRed);
        BuildIngredient(drinkStation.transform, referenceRenderer, "Umbrella", new Vector3(-0.55f, -0.15f, 0f), new Vector3(-0.55f, 0.2f, 0f), Pink);
    }

    private static void BuildGuest1(Scene scene, SpriteRenderer referenceRenderer)
    {
        GameObject guest1 = GetOrCreateRoot(scene, "Guest1");
        ConfigureTransform(guest1.transform, new Vector3(-5.25f, 0.5f, 0f), Quaternion.identity, Vector3.one, false);

        GameObject body = GetOrCreateChild(guest1.transform, "Body");
        ConfigureSprite(
            body,
            referenceRenderer,
            new Vector3(0f, 0f, 0f),
            new Vector3(0.45f, 0.75f, 1f),
            GuestBlue,
            2,
            true);

        GameObject head = GetOrCreateChild(guest1.transform, "Head");
        ConfigureSprite(
            head,
            referenceRenderer,
            new Vector3(0f, 0.55f, 0f),
            new Vector3(0.32f, 0.32f, 1f),
            GuestHead,
            3,
            true);

        GameObject interactionZone = GetOrCreateChild(guest1.transform, "Guest1InteractionZone");
        ConfigureTransform(interactionZone.transform, guest1.transform.InverseTransformPoint(new Vector3(-3.1f, 0.5f, 0f)), Quaternion.identity, Vector3.one, true);
        ConfigureBoxCollider(interactionZone, true, new Vector2(1f, 1.5f));
    }

    private static void BuildElevatorDoor(Scene scene, SpriteRenderer referenceRenderer)
    {
        GameObject elevatorDoor = FindRootGameObject(scene, "ElevatorDoor");
        if (elevatorDoor == null)
        {
            EditorUtility.DisplayDialog(
                "Hotel Hunger Lobby Layout",
                "Could not find the existing ElevatorDoor root in the active scene.",
                "OK");
            return;
        }

        ConfigureTransform(elevatorDoor.transform, Vector3.zero, Quaternion.identity, Vector3.one, false);

        GameObject elevatorVisual = GetOrCreateChild(elevatorDoor.transform, "ElevatorVisual");
        ConfigureSprite(
            elevatorVisual,
            referenceRenderer,
            new Vector3(8.28f, 1.2f, 0f),
            new Vector3(0.55f, 1.5f, 1f),
            ElevatorDark,
            1,
            true);
        RemoveComponentsIfPresent<BoxCollider2D>(elevatorVisual);

        GameObject elevatorSign = GetOrCreateChild(elevatorDoor.transform, "ElevatorSign");
        ConfigureSprite(
            elevatorSign,
            referenceRenderer,
            new Vector3(8.05f, 2.05f, 0f),
            new Vector3(0.75f, 0.18f, 1f),
            ElevatorSignLight,
            2,
            true);
        RemoveComponentsIfPresent<Collider2D>(elevatorSign);

        GameObject elevatorInteractionZone = GetOrCreateChild(elevatorDoor.transform, "ElevatorInteractionZone");
        ConfigureTransform(
            elevatorInteractionZone.transform,
            new Vector3(7.65f, 1.2f, 0f),
            Quaternion.identity,
            Vector3.one,
            true);
        RemoveComponentsIfPresent<SpriteRenderer>(elevatorInteractionZone);
        ConfigureBoxCollider(elevatorInteractionZone, true, new Vector2(1.2f, 1.7f));
    }

    private static void BuildIngredient(
        Transform drinkStation,
        SpriteRenderer referenceRenderer,
        string ingredientName,
        Vector3 visualWorldPosition,
        Vector3 zoneWorldPosition,
        Color color)
    {
        GameObject ingredient = GetOrCreateChild(drinkStation, ingredientName);
        ConfigureSprite(
            ingredient,
            referenceRenderer,
            drinkStation.InverseTransformPoint(visualWorldPosition),
            new Vector3(0.25f, 0.25f, 1f),
            color,
            2,
            true);

        GameObject interactionZone = GetOrCreateChild(drinkStation, ingredientName + "InteractionZone");
        ConfigureTransform(
            interactionZone.transform,
            drinkStation.InverseTransformPoint(zoneWorldPosition),
            Quaternion.identity,
            Vector3.one,
            true);
        RemoveComponentsIfPresent<SpriteRenderer>(interactionZone);
        ConfigureBoxCollider(interactionZone, true, new Vector2(0.5f, 0.5f));
    }

    private static void ConfigureSprite(
        GameObject gameObject,
        SpriteRenderer referenceRenderer,
        Vector3 localPosition,
        Vector3 localScale,
        Color color,
        int sortingOrder,
        bool useLocalPosition)
    {
        ConfigureTransform(gameObject.transform, localPosition, Quaternion.identity, localScale, useLocalPosition);

        SpriteRenderer spriteRenderer = GetOrAddComponent<SpriteRenderer>(gameObject);
        Undo.RecordObject(spriteRenderer, UndoName);
        spriteRenderer.sprite = referenceRenderer.sprite;
        spriteRenderer.sharedMaterial = referenceRenderer.sharedMaterial;
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
