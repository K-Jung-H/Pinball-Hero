using UnityEngine;

[DisallowMultipleComponent]
public sealed class BallCollisionAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public void Play()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.Play();
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
