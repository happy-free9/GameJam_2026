using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BGMPlayer : MonoBehaviour
{
    [Header("Playback")]
    [Tooltip("Default volume applied on Awake. Designers can adjust this on the prefab.")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.4f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = true;
        audioSource.loop = true;
        audioSource.volume = defaultVolume;

        if (audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
