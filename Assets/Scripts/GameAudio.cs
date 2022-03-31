
using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip Fireplace;
    public AudioClip Dances;
    public AudioClip Asterope;
    public AudioClip SlideSound;
    private static GameAudio _instance;
    public static GameAudio Instance{ get { return _instance; } }

    private void Awake(){
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMenuMusic(){
        audioSource.Stop();
        audioSource.PlayOneShot(Fireplace);
        audioSource.PlayOneShot(Asterope);
    }

    public void PlayTrackSlide()=>audioSource.PlayOneShot(SlideSound);

    public void PlayTrack(AudioClip clip)=>audioSource.PlayOneShot(clip);

    public void PlayGameMusic(){
        audioSource.Stop();
        audioSource.PlayOneShot(Dances);

    }


}
