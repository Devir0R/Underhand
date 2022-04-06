
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

    private bool muteAudio;
    private bool muteSound;
    

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
        if(muteAudio) return;
        audioSource.Stop();
        audioSource.clip = Asterope;
        audioSource.loop = true;
        audioSource.Play();
        audioSource.PlayOneShot(Fireplace);
    }

    public void FlipAudio(){
        muteAudio=!muteAudio;
        if(muteAudio){
            audioSource.Stop();
        }
        else{
            PlayMenuMusic();
        }
    }
    public void FlipSound()=>muteSound=!muteSound;

    public void PlayTrackSlide(){
        PlayTrack(SlideSound);
    }

    public void PlayTrack(AudioClip clip){
        if(!muteSound) audioSource.PlayOneShot(clip);
    }

    public void PlayGameMusic(){
        if(muteAudio) return;

        audioSource.Stop();
        audioSource.clip = Dances;
        audioSource.loop = true;
        audioSource.Play();

    }

    public bool isAudioOn()=>!muteAudio;
    public bool isSoundOn()=>!muteSound;


}
