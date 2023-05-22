using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;
    
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField] private AudioSource goalAudioSource;
    [SerializeField] private AudioSource explosionAudioSource;

    public void PlayMusicAudioSource()
    {
        musicAudioSource.Play();
    }


    public void PlayMoveSound()
    {
        moveAudioSource.Play();
    }

    public void PlayTargetPickupSound(int blockDistance)
    {
        targetAudioSource.PlayDelayed(0.15f * blockDistance);
    }
    
    public void PlayGoalSound()
    {
        goalAudioSource.Play();
    }

    public void PlayExplosionSound()
    {
        explosionAudioSource.Play();
    }
}
