using System.Collections.Generic;
using UnityEngine;

public class CoreSystemsTestBootstrap : MonoBehaviour
{
    private enum TestSceneVariant
    {
        SceneA = 0,
        SceneB = 1
    }

    [SerializeField] private TestSceneVariant sceneVariant;

    private Sprite cachedSquareSprite;

    private void Awake()
    {
        EnsurePersistentSystems();
        EnsureCamera();
        BuildCommonRoom();

        if (sceneVariant == TestSceneVariant.SceneA)
        {
            BuildSceneA();
        }
        else
        {
            BuildSceneB();
        }
    }

    private void EnsurePersistentSystems()
    {
        // The reusable systems are designed to work in real scenes too, but the test scenes
        // create a minimal runtime harness so nobody has to wire everything by hand first.
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
            existingCamera.orthographicSize = 6f;
            existingCamera.transform.position = new Vector3(0f, 0f, -10f);
            return;
        }

        GameObject cameraObject = new("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 6f;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void BuildCommonRoom()
    {
        CreateVisibleBox("Floor", Vector2.zero, new Vector2(18f, 12f), new Color(0.15f, 0.15f, 0.18f, 1f), false);
        CreateWall("TopWall", new Vector2(0f, 6.1f), new Vector2(18f, 0.4f));
        CreateWall("BottomWall", new Vector2(0f, -6.1f), new Vector2(18f, 0.4f));
        CreateWall("LeftWall", new Vector2(-9.1f, 0f), new Vector2(0.4f, 12f));
        CreateWall("RightWall", new Vector2(9.1f, 0f), new Vector2(0.4f, 12f));
        CreatePlayer();
    }

    private void BuildSceneA()
    {
        ObjectivePanelController.Instance?.SetObjective("Walk to the cyan trigger, press E, then use the door.");

        CreateSpawnPoint("DefaultSpawn", new Vector2(-6.5f, 0f), Color.green);
        CreateSpawnPoint("FromB", new Vector2(6.4f, -1.5f), Color.yellow);

        InteractionTarget testTarget = CreateInteractionZone(
            "TestInteraction",
            new Vector2(-2.2f, 1.4f),
            new Vector2(2f, 2f),
            new Color(0.2f, 0.8f, 0.9f, 0.5f),
            "Press E");

        testTarget.OnInteract.AddListener(() =>
        {
            ObjectivePanelController.Instance?.SetObjective("Good. Now use the white door on the right.");
            DialoguePanelController.Instance?.StartDialogue(new List<DialogueLine>
            {
                new() { speakerName = "System", bodyText = "Interaction fired successfully." },
                new() { speakerName = "System", bodyText = "Doors, NPCs, items, and endings can all use this same trigger flow." }
            });
        });

        SimpleChaseAgent2D chaseAgent = CreateChaseAgent(new Vector2(4f, 2.8f));
        chaseAgent.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.15f, 0.15f, 1f);
        chaseAgent.OnPlayerCaught.AddListener(() =>
        {
            ObjectivePanelController.Instance?.SetObjective("Caught! Re-enter the scene or press Play again to retry.");
            DialoguePanelController.Instance?.ShowSingleLine("Ending", "Bad ending test triggered.");
        });
        chaseAgent.StopChase();

        InteractionTarget chaseTrigger = CreateInteractionZone(
            "StartChaseTrigger",
            new Vector2(1.4f, -2.8f),
            new Vector2(2.4f, 2f),
            new Color(0.95f, 0.5f, 0.2f, 0.45f),
            "Press E");

        chaseTrigger.OnInteract.AddListener(() =>
        {
            ObjectivePanelController.Instance?.SetObjective("Run from the red chaser or use the door.");
            DialoguePanelController.Instance?.ShowSingleLine("System", "Chase started.");
            chaseAgent.StartChase();
        });

        CreateDoor(
            "DoorToSceneB",
            new Vector2(7.6f, 0f),
            new Vector2(1.2f, 3f),
            "CoreSystems_Test_B",
            "FromA");
    }

    private void BuildSceneB()
    {
        ObjectivePanelController.Instance?.SetObjective("You spawned at the configured point. Use the door to return.");

        CreateSpawnPoint("DefaultSpawn", new Vector2(0f, 0f), Color.green);
        CreateSpawnPoint("FromA", new Vector2(-6.6f, 0f), Color.cyan);

        InteractionTarget dialogueTarget = CreateInteractionZone(
            "SceneBDialogueTrigger",
            new Vector2(1.6f, 1.6f),
            new Vector2(2f, 2f),
            new Color(0.5f, 0.35f, 0.9f, 0.45f),
            "Press E");

        dialogueTarget.OnInteract.AddListener(() =>
        {
            DialoguePanelController.Instance?.StartDialogue(new List<DialogueLine>
            {
                new() { speakerName = "System", bodyText = "Scene transitions can target any scene in Build Settings." },
                new() { speakerName = "System", bodyText = "Spawn points are matched by the string id you set in the Inspector." }
            });
        });

        CreateDoor(
            "DoorBackToSceneA",
            new Vector2(7.6f, 0f),
            new Vector2(1.2f, 3f),
            "CoreSystems_Test",
            "FromB");
    }

    private void CreatePlayer()
    {
        if (FindFirstObjectByType<PlayerInteractor>() != null)
        {
            return;
        }

        GameObject player = CreateVisibleBox("Player", new Vector2(-6.5f, 0f), new Vector2(0.8f, 1.1f), new Color(0.95f, 0.85f, 0.2f, 1f), true);
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

    private InteractionTarget CreateInteractionZone(string objectName, Vector2 position, Vector2 size, Color color, string promptText)
    {
        GameObject zone = CreateVisibleBox(objectName, position, size, color, true);
        zone.GetComponent<Collider2D>().isTrigger = true;

        InteractionTarget interactionTarget = zone.AddComponent<InteractionTarget>();
        interactionTarget.SetPromptText(promptText);
        return interactionTarget;
    }

    private void CreateDoor(string objectName, Vector2 position, Vector2 size, string targetSceneName, string targetSpawnPointId)
    {
        GameObject door = CreateVisibleBox(objectName, position, size, new Color(0.9f, 0.9f, 0.95f, 1f), true);
        door.GetComponent<Collider2D>().isTrigger = true;

        InteractionTarget interactionTarget = door.AddComponent<InteractionTarget>();
        interactionTarget.SetPromptText("Press E");

        SceneTransitionTrigger trigger = door.AddComponent<SceneTransitionTrigger>();
        trigger.Configure(targetSceneName, targetSpawnPointId);
    }

    private void CreateSpawnPoint(string spawnPointId, Vector2 position, Color color)
    {
        GameObject spawn = CreateVisibleBox($"SpawnPoint_{spawnPointId}", position, new Vector2(0.8f, 0.8f), color, false);
        SpawnPoint spawnPoint = spawn.AddComponent<SpawnPoint>();
        spawnPoint.SetSpawnPointId(spawnPointId);
    }

    private SimpleChaseAgent2D CreateChaseAgent(Vector2 position)
    {
        GameObject chaser = CreateVisibleBox("ChaseAgent", position, new Vector2(0.9f, 0.9f), new Color(0.9f, 0.2f, 0.2f, 1f), true);
        chaser.GetComponent<Collider2D>().isTrigger = true;
        return chaser.AddComponent<SimpleChaseAgent2D>();
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
