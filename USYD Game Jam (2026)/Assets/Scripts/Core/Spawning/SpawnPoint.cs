using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Identity")]
    [Tooltip("String id used by SceneTransitionManager to place the player after a scene load.")]
    [SerializeField] private string spawnPointId = "DefaultSpawn";
    [Tooltip("Scene-view color only. This does not affect gameplay.")]
    [SerializeField] private Color gizmoColor = Color.green;

    public string SpawnPointId => spawnPointId;

    public void SetSpawnPointId(string newSpawnPointId)
    {
        spawnPointId = newSpawnPointId;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(spawnPointId))
        {
            Debug.LogWarning($"SpawnPoint on '{name}' is missing a spawnPointId.", this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.75f);
    }
}
