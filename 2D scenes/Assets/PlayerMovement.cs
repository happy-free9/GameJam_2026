using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -35f;
    public float jumpHeight = 1.5f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("Interaction")]
    public float interactDistance = 2f;

    [Header("Audio Sources")]
    public AudioSource movementAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Audio Clips")]
    public AudioClip movementClip;
    public AudioClip interactClip;
    public AudioClip elevatorClip;

    [Header("Audio Settings")]
    public float walkFootstepPitch = 1f;
    public float runFootstepPitch = 1.35f;

    private CharacterController controller;
    private float cameraPitch = 0f;
    private Vector3 velocity;

    private MenuInteract currentHoveredMenu;
    private ElevatorDoorInteract currentHoveredElevator;
    private LevelBDoorTeleport currentHoveredLevelBDoor;
    private LuggageEnding2Interact currentHoveredLuggage;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (movementAudioSource != null)
        {
            movementAudioSource.loop = true;
            movementAudioSource.playOnAwake = false;
        }

        if (sfxAudioSource != null)
        {
            sfxAudioSource.loop = false;
            sfxAudioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (Ending1Manager.Instance != null && Ending1Manager.Instance.IsEndingPlaying)
        {
            StopMovementAudio();
            return;
        }

        if (Ending2Manager.Instance != null && Ending2Manager.Instance.IsEnding2Active)
        {
            StopMovementAudio();
            return;
        }

        if (MenuPopupUI.Instance != null && MenuPopupUI.Instance.IsOpen)
        {
            StopMovementAudio();

            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayInteractSound();
                MenuPopupUI.Instance.HideMenu();
            }

            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsDialogueActive)
        {
            StopMovementAudio();

            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayInteractSound();
                GameManager.Instance.AdvanceDialogue();
            }

            return;
        }

        LookAround();
        MovePlayer();
        HandleJumpAndGravity();
        HandleMovementAudio();
        UpdateHoverTarget();
        HandleInteraction();
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleJumpAndGravity()
    {
        bool grounded = IsPlayerGrounded();

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    bool IsPlayerGrounded()
    {
        if (controller.isGrounded)
            return true;

        float rayLength = 1.3f;
        return Physics.Raycast(transform.position, Vector3.down, rayLength);
    }

    void HandleMovementAudio()
    {
        if (movementAudioSource == null || movementClip == null)
            return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
        bool grounded = IsPlayerGrounded();
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (isMoving && grounded)
        {
            movementAudioSource.pitch = isRunning ? runFootstepPitch : walkFootstepPitch;

            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.clip = movementClip;
                movementAudioSource.Play();
            }
        }
        else
        {
            StopMovementAudio();
        }
    }

    void StopMovementAudio()
    {
        if (movementAudioSource != null && movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
        }
    }

    void PlayInteractSound()
    {
        if (sfxAudioSource != null && interactClip != null)
        {
            sfxAudioSource.PlayOneShot(interactClip);
        }
    }

    void PlayElevatorSound()
    {
        if (sfxAudioSource != null && elevatorClip != null)
        {
            sfxAudioSource.PlayOneShot(elevatorClip);
        }
    }

    void UpdateHoverTarget()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        MenuInteract newHoveredMenu = null;
        ElevatorDoorInteract newHoveredElevator = null;
        LevelBDoorTeleport newHoveredLevelBDoor = null;
        LuggageEnding2Interact newHoveredLuggage = null;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            newHoveredMenu = hit.collider.GetComponentInParent<MenuInteract>();
            newHoveredElevator = hit.collider.GetComponentInParent<ElevatorDoorInteract>();
            newHoveredLevelBDoor = hit.collider.GetComponentInParent<LevelBDoorTeleport>();
            newHoveredLuggage = hit.collider.GetComponentInParent<LuggageEnding2Interact>();
        }

        if (currentHoveredMenu != null && currentHoveredMenu != newHoveredMenu)
        {
            currentHoveredMenu.SetHighlight(false);
        }

        currentHoveredMenu = newHoveredMenu;

        if (currentHoveredMenu != null)
        {
            currentHoveredMenu.SetHighlight(true);
        }

        if (currentHoveredElevator != null && currentHoveredElevator != newHoveredElevator)
        {
            currentHoveredElevator.SetHighlight(false);
        }

        currentHoveredElevator = newHoveredElevator;

        if (currentHoveredElevator != null)
        {
            currentHoveredElevator.SetHighlight(true);
        }

        if (currentHoveredLevelBDoor != null && currentHoveredLevelBDoor != newHoveredLevelBDoor)
        {
            currentHoveredLevelBDoor.SetHighlight(false);
        }

        currentHoveredLevelBDoor = newHoveredLevelBDoor;

        if (currentHoveredLevelBDoor != null)
        {
            currentHoveredLevelBDoor.SetHighlight(true);
        }

        if (currentHoveredLuggage != null && currentHoveredLuggage != newHoveredLuggage)
        {
            currentHoveredLuggage.SetHighlight(false);
        }

        currentHoveredLuggage = newHoveredLuggage;

        if (currentHoveredLuggage != null)
        {
            currentHoveredLuggage.SetHighlight(true);
        }
    }

    void HandleInteraction()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            LevelBDoorTeleport levelBDoor = hit.collider.GetComponentInParent<LevelBDoorTeleport>();

            if (levelBDoor != null)
            {
                PlayInteractSound();
                levelBDoor.TryTeleport();
                return;
            }

            LuggageEnding2Interact luggage = hit.collider.GetComponentInParent<LuggageEnding2Interact>();

            if (luggage != null)
            {
                PlayInteractSound();
                luggage.Interact();
                return;
            }

            ElevatorDoorInteract elevator = hit.collider.GetComponentInParent<ElevatorDoorInteract>();

            if (elevator != null)
            {
                elevator.TryEnterElevator();
                return;
            }

            DoorInteract door = hit.collider.GetComponentInParent<DoorInteract>();

            if (door != null)
            {
                PlayInteractSound();
                door.TryToggleDoor();
                return;
            }

            MenuInteract menu = hit.collider.GetComponentInParent<MenuInteract>();

            if (menu != null)
            {
                PlayInteractSound();
                menu.ReadMenu();
                return;
            }
        }
    }
}