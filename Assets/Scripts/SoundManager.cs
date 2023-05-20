using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;
    
    public void PlayMusicAudioSource()
    {
        musicAudioSource.Play();
    }
}
