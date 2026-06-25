using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PersistentBGMPlayer : MonoBehaviour
{
    private static readonly Dictionary<string, PersistentBGMPlayer> ActivePlayersById = new();

    [Header("BGM Identity")]
    [Tooltip("Music id used to prevent duplicate persistent BGM objects across scene loads.")]
    [SerializeField] private string musicId = "POV1";

    [Header("Playback")]
    [Tooltip("Default volume applied on Awake. Designers can adjust this on the prefab.")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.4f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(musicId))
        {
            musicId = gameObject.name;
        }

        if (ActivePlayersById.TryGetValue(musicId, out PersistentBGMPlayer existingPlayer) &&
            existingPlayer != null &&
            existingPlayer != this)
        {
            Destroy(gameObject);
            return;
        }

        ActivePlayersById[musicId] = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = true;
        audioSource.loop = true;
        audioSource.volume = defaultVolume;

        if (audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrWhiteSpace(musicId) &&
            ActivePlayersById.TryGetValue(musicId, out PersistentBGMPlayer existingPlayer) &&
            existingPlayer == this)
        {
            ActivePlayersById.Remove(musicId);
        }
    }
}
