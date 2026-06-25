using System.Collections.Generic;
using UnityEngine;

public class ShawnLobbyIntegrationBootstrap : MonoBehaviour
{
    private enum SceneVariant
    {
        Lobby = 0,
        Target = 1
    }

    [Header("Scene Variant")]
    [SerializeField] private SceneVariant sceneVariant;

    private Sprite cachedSquareSprite;

    private void Awake()
    {
        EnsurePersistentSystems();
        EnsureCamera();
        BuildRoom();

        if (sceneVariant == SceneVariant.Lobby)
        {
            BuildLobbyScene();
        }
        else
        {
            BuildTargetScene();
        }
    }

    private void EnsurePersistentSystems()
    {
        if (SceneTransitionManager.Instance == null)
        {
            new GameObject("SceneTransitionManager").AddComponent<SceneTransitionManager>();
        }

        if (FindFirstObjectByType<CoreUIRoot>() == null)
        {
            new GameObject("CoreUIRoot").AddComponent<CoreUIRoot>();
        }
    }

    private void EnsureCamera()
    {
        Camera existingCamera = FindFirstObjectByType<Camera>();
        if (existingCamera != null)
        {
            existingCamera.orthographic = true;
            existingCamera.orthographicSize = 7f;
            existingCamera.transform.position = new Vector3(0f, 0f, -10f);
            return;
        }

        GameObject cameraObject = new("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 7f;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void BuildRoom()
    {
        CreateVisibleBox("Floor", Vector2.zero, new Vector2(24f, 14f), new Color(0.13f, 0.14f, 0.18f, 1f), false);
        CreateWall("TopWall", new Vector2(0f, 7.1f), new Vector2(24f, 0.4f));
        CreateWall("BottomWall", new Vector2(0f, -7.1f), new Vector2(24f, 0.4f));
        CreateWall("LeftWall", new Vector2(-12.1f, 0f), new Vector2(0.4f, 14f));
        CreateWall("RightWall", new Vector2(12.1f, 0f), new Vector2(0.4f, 14f));
        CreatePlayer();
    }

    private void BuildLobbyScene()
    {
        ObjectivePanelController.Instance?.SetObjective("Walk through Shawn-style trigger zones and test the elevator.");

        CreateSpawnPoint("DefaultSpawn", new Vector2(-9f, -2.2f), Color.white);

        CreateInteractionZone(
            "Guest1InteractionZone",
            new Vector2(-7.5f, 2.2f),
            new Vector2(2.6f, 2f),
            new Color(0.76f, 0.64f, 0.91f, 0.5f),
            "Press E to talk",
            () =>
            {
                DialoguePanelController.Instance?.StartDialogue(new List<DialogueLine>
                {
                    new() { speakerName = "Guest 1", bodyText = "This is a sandbox dialogue hook for Shawn's lobby zone." },
                    new() { speakerName = "System", bodyText = "Later, Shawn can wire the real Guest 1 flow in the Inspector." }
                });
                ObjectivePanelController.Instance?.SetObjective("Guest 1 tested. Try the collection zones next.");
            });

        CreateInteractionZone(
            "GlassInteractionZone",
            new Vector2(-3.8f, 2.2f),
            new Vector2(2.2f, 2f),
            new Color(0.35f, 0.85f, 0.92f, 0.5f),
            "Press E to collect glass",
            () => ObjectivePanelController.Instance?.SetObjective("Collected glass"));

        CreateInteractionZone(
            "GoldStrawInteractionZone",
            new Vector2(0f, 2.2f),
            new Vector2(2.2f, 2f),
            new Color(0.93f, 0.76f, 0.24f, 0.5f),
            "Press E to collect gold straw",
            () => ObjectivePanelController.Instance?.SetObjective("Collected gold straw"));

        CreateInteractionZone(
            "HouseSpecialSyrupInteractionZone",
            new Vector2(3.8f, 2.2f),
            new Vector2(2.6f, 2f),
            new Color(0.95f, 0.48f, 0.48f, 0.5f),
            "Press E to collect syrup",
            () => ObjectivePanelController.Instance?.SetObjective("Collected syrup"));

        CreateInteractionZone(
            "UmbrellaInteractionZone",
            new Vector2(7.6f, 2.2f),
            new Vector2(2.4f, 2f),
            new Color(0.48f, 0.88f, 0.56f, 0.5f),
            "Press E to collect umbrella",
            () => ObjectivePanelController.Instance?.SetObjective("Collected umbrella"));

        CreateDoorZone(
            "ElevatorInteractionZone",
            new Vector2(8.6f, -2.2f),
            new Vector2(2.4f, 3f),
            "Press E to enter",
            "game_core_target",
            "FromLobby");
    }

    private void BuildTargetScene()
    {
        CreateSpawnPoint("FromLobby", new Vector2(-8f, 0f), Color.green);
        CreateSpawnPoint("DefaultSpawn", new Vector2(0f, 0f), Color.yellow);

        ObjectivePanelController.Instance?.SetObjective("Arrived from Lobby");

        CreateDoorZone(
            "ReturnDoor",
            new Vector2(8.6f, 0f),
            new Vector2(2.2f, 3f),
            "Press E to return",
            "game_core_test",
            "DefaultSpawn");
    }

    private void CreatePlayer()
    {
        if (FindFirstObjectByType<PlayerInteractor>() != null)
        {
            return;
        }

        GameObject player = CreateVisibleBox("Player", new Vector2(-9f, -2.2f), new Vector2(0.8f, 1.1f), new Color(0.95f, 0.86f, 0.28f, 1f), true);
        player.tag = "Player";

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        player.AddComponent<TopDownPlayerController>();
        player.AddComponent<PlayerInteractor>();
    }

    private void CreateWall(string objectName, Vector2 position, Vector2 size)
    {
        GameObject wall = CreateVisibleBox(objectName, position, size, new Color(0.35f, 0.35f, 0.4f, 1f), true);
        wall.GetComponent<Collider2D>().isTrigger = false;
    }

    private void CreateInteractionZone(
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color,
        string promptText,
        UnityEngine.Events.UnityAction action)
    {
        GameObject zone = CreateVisibleBox(objectName, position, size, color, true);
        BoxCollider2D collider2D = zone.GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;

        InteractionTarget interactionTarget = zone.AddComponent<InteractionTarget>();
        interactionTarget.SetPromptText(promptText);
        interactionTarget.OnInteract.AddListener(action);
    }

    private void CreateDoorZone(
        string objectName,
        Vector2 position,
        Vector2 size,
        string promptText,
        string targetSceneName,
        string targetSpawnPointId)
    {
        GameObject zone = CreateVisibleBox(objectName, position, size, new Color(0.92f, 0.92f, 0.96f, 1f), true);
        BoxCollider2D collider2D = zone.GetComponent<BoxCollider2D>();
        collider2D.isTrigger = true;

        SceneTransitionTrigger transitionTrigger = zone.AddComponent<SceneTransitionTrigger>();
        transitionTrigger.Configure(targetSceneName, targetSpawnPointId);
        transitionTrigger.SetPromptText(promptText);
    }

    private void CreateSpawnPoint(string spawnPointId, Vector2 position, Color color)
    {
        GameObject spawn = CreateVisibleBox($"SpawnPoint_{spawnPointId}", position, new Vector2(0.85f, 0.85f), color, false);
        SpawnPoint spawnPoint = spawn.AddComponent<SpawnPoint>();
        spawnPoint.SetSpawnPointId(spawnPointId);
    }

    private GameObject CreateVisibleBox(string objectName, Vector2 position, Vector2 size, Color color, bool addCollider)
    {
        GameObject go = new(objectName);
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSquareSprite();
        spriteRenderer.color = color;

        if (addCollider)
        {
            go.AddComponent<BoxCollider2D>();
        }

        return go;
    }

    private Sprite GetSquareSprite()
    {
        if (cachedSquareSprite != null)
        {
            return cachedSquareSprite;
        }

        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        cachedSquareSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);

        return cachedSquareSprite;
    }
}
