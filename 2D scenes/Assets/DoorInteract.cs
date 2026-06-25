using UnityEngine;

public class DoorInteract : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 3f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(transform.localEulerAngles + new Vector3(0f, openAngle, 0f));
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    public void TryToggleDoor()
    {
        if (GameManager.Instance != null && !GameManager.Instance.DoorUnlocked)
        {
            GameManager.Instance.ShowTemporaryMessage("The door will not open yet.");
            return;
        }

        isOpen = !isOpen;
    }
}