using UnityEngine;

public class SimpleChaseTestBootstrap : MonoBehaviour
{
    private Sprite cachedSquareSprite;

    private void Awake()
    {
        EnsurePersistentSystems();
        EnsureCamera();
        BuildArena();
    }

    private void EnsurePersistentSystems()
    {
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

    private void BuildArena()
    {
        ObjectivePanelController.Instance?.SetObjective("Press E at the orange zone to start the two-person chase.");

        CreateVisibleBox("Floor", Vector2.zero, new Vector2(24f, 14f), new Color(0.11f, 0.12f, 0.16f, 1f), false);
        CreateWall("TopWall", new Vector2(0f, 7.1f), new Vector2(24f, 0.4f));
        CreateWall("BottomWall", new Vector2(0f, -7.1f), new Vector2(24f, 0.4f));
        CreateWall("LeftWall", new Vector2(-12.1f, 0f), new Vector2(0.4f, 14f));
        CreateWall("RightWall", new Vector2(12.1f, 0f), new Vector2(0.4f, 14f));

        GameObject player = CreatePlayer();
        SimpleChaseAgent2D chaseAgent = CreateChaseAgent(new Vector2(8.4f, 0f), player.transform);

        CreateStartChaseZone(new Vector2(-8.2f, 0f), chaseAgent);
    }

    private GameObject CreatePlayer()
    {
        if (FindFirstObjectByType<PlayerInteractor>() != null)
        {
            return FindFirstObjectByType<PlayerInteractor>().gameObject;
        }

        GameObject player = CreateVisibleBox("Player", new Vector2(-10f, -4f), new Vector2(0.8f, 1.1f), new Color(0.94f, 0.85f, 0.25f, 1f), true);
        player.tag = "Player";

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        player.AddComponent<TopDownPlayerController>();
        player.AddComponent<PlayerInteractor>();
        return player;
    }

    private SimpleChaseAgent2D CreateChaseAgent(Vector2 position, Transform playerTarget)
    {
        GameObject chaser = CreateVisibleBox("ChaseAgent", position, new Vector2(0.95f, 0.95f), new Color(0.9f, 0.2f, 0.2f, 1f), true);
        BoxCollider2D trigger = chaser.GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;

        SimpleChaseAgent2D chaseAgent = chaser.AddComponent<SimpleChaseAgent2D>();
        chaseAgent.SetTarget(playerTarget);
        chaseAgent.OnPlayerCaught.AddListener(() =>
        {
            ObjectivePanelController.Instance?.SetObjective("Caught. Press Play again or restart the scene to retry.");
            DialoguePanelController.Instance?.ShowSingleLine("System", "Simple chase catch event fired.");
        });

        return chaseAgent;
    }

    private void CreateStartChaseZone(Vector2 position, SimpleChaseAgent2D chaseAgent)
    {
        GameObject zone = CreateVisibleBox("StartChaseZone", position, new Vector2(2.5f, 2.5f), new Color(0.94f, 0.55f, 0.18f, 0.55f), true);
        zone.GetComponent<BoxCollider2D>().isTrigger = true;

        InteractionTarget interactionTarget = zone.AddComponent<InteractionTarget>();
        interactionTarget.SetPromptText("Press E to start chase");
        interactionTarget.OnInteract.AddListener(() =>
        {
            ObjectivePanelController.Instance?.SetObjective("Run. The red character will chase until it catches you.");
            chaseAgent.StartChase();
        });
    }

    private void CreateWall(string objectName, Vector2 position, Vector2 size)
    {
        GameObject wall = CreateVisibleBox(objectName, position, size, new Color(0.35f, 0.35f, 0.42f, 1f), true);
        wall.GetComponent<Collider2D>().isTrigger = false;
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
