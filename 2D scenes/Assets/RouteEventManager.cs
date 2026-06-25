using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RouteEventManager : MonoBehaviour
{
    [Header("UI")]
    public Text objectiveText;

    [Header("Tree / Plant Fall")]
    public Transform treePivot; // Drag Tree Pivot here
    public float treeFallAngle = 90f;
    public float treeFallSpeed = 2f;

    [Header("Trolley Block")]
    public Transform trolley;
    public Transform trolleyBlockPoint;
    public float trolleyMoveSpeed = 3f;

    [Header("Objectives")]
    public string objectiveAfterTreeFalls = "Objective: Keep running";
    public string objectiveAfterTrolleyBlocks = "Objective: Find another way";

    private bool treeEventDone = false;
    private bool trolleyEventDone = false;

    public bool TreeHasFallen
    {
        get { return treeEventDone; }
    }

    public void ActivateTrigger(int triggerNumber)
    {
        Debug.Log("RouteEventManager received trigger: " + triggerNumber);

        if (triggerNumber == 1)
        {
            StartTreeFallEvent();
        }
        else if (triggerNumber == 2)
        {
            StartTrolleyBlockEvent();
        }
    }

    void StartTreeFallEvent()
    {
        if (treeEventDone)
            return;

        treeEventDone = true;

        Debug.Log("Tree fall event started.");

        if (objectiveText != null)
        {
            objectiveText.text = objectiveAfterTreeFalls;
        }

        if (treePivot != null)
        {
            StartCoroutine(FallTreeSideways());
        }
        else
        {
            Debug.LogWarning("Tree Pivot is not assigned in RouteEventManager.");
        }
    }

    IEnumerator FallTreeSideways()
    {
        Quaternion startRotation = treePivot.localRotation;

        // Z rotation makes it fall sideways left/right.
        Quaternion targetRotation = Quaternion.Euler(
            treePivot.localEulerAngles.x,
            treePivot.localEulerAngles.y,
            treePivot.localEulerAngles.z + treeFallAngle
        );

        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * treeFallSpeed;

            treePivot.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                timer
            );

            yield return null;
        }

        treePivot.localRotation = targetRotation;

        Debug.Log("Tree finished falling sideways.");
    }

    void StartTrolleyBlockEvent()
    {
        if (trolleyEventDone)
            return;

        if (!treeEventDone)
        {
            Debug.Log("Trolley trigger ignored because tree event has not happened yet.");
            return;
        }

        trolleyEventDone = true;

        Debug.Log("Trolley block event started.");

        if (objectiveText != null)
        {
            objectiveText.text = objectiveAfterTrolleyBlocks;
        }

        if (trolley != null && trolleyBlockPoint != null)
        {
            StartCoroutine(MoveTrolleyToMiddle());
        }
        else
        {
            Debug.LogWarning("Trolley or Trolley Block Point is not assigned in RouteEventManager.");
        }
    }

    IEnumerator MoveTrolleyToMiddle()
    {
        // Moves trolley sideways only.
        // It uses the X position of Trolley Block Point.
        // It keeps current Y and Z, so it will not roll toward the player.
        Vector3 targetPosition = new Vector3(
            trolleyBlockPoint.position.x,
            trolley.position.y,
            trolley.position.z
        );

        Quaternion originalRotation = trolley.rotation;

        while (Vector3.Distance(trolley.position, targetPosition) > 0.05f)
        {
            trolley.position = Vector3.MoveTowards(
                trolley.position,
                targetPosition,
                trolleyMoveSpeed * Time.deltaTime
            );

            // Keeps trolley straight.
            trolley.rotation = originalRotation;

            yield return null;
        }

        trolley.position = targetPosition;
        trolley.rotation = originalRotation;

        Debug.Log("Trolley finished moving to middle.");
    }
}