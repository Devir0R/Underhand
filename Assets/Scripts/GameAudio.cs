
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
public class GameAudio : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip Dances;
    public AudioClip Fight;
    public AudioClip Asterope;
    public AudioClip FantasyMenu;
    public AudioClip SlideSound;
    private static GameAudio _instance;
    public static GameAudio Instance{ get { return _instance; } }

    public List<OptionClip> optionsAudio = new List<OptionClip>();

    private bool muteAudio;
    private bool muteSound;
    private Dictionary<int,List<AudioClip>> audioQueue = new Dictionary<int, List<AudioClip>>();

    private bool playingRadio = false;
    

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
        audioSource.clip = GameState.GameMode==Mode.FightCult? FantasyMenu: Asterope;
        audioSource.loop = true;
        audioSource.volume = Loader.settings.master_volume;
        audioSource.Play();
    }
    
    public void AddToQueue(string cardName, int optionNum){
        if(UnityEngine.Random.value>=.5) return;
        int waitNCards = Mathf.CeilToInt(UnityEngine.Random.Range(0,5));
        if(!audioQueue.ContainsKey(waitNCards)){
            audioQueue[waitNCards] = new List<AudioClip>();
        }
        foreach(OptionClip opClip in optionsAudio){
            if(opClip.cardName==cardName && opClip.option==optionNum){
                audioQueue[waitNCards].Add(opClip.clip);
                break;
            }
        }
    }

    public void playNextOnQueue(){
        if(AudioQueueEmpty()) return;

        if(audioQueue.ContainsKey(0)&& audioQueue[0].Count>0){
            if(!playingRadio){
                playingRadio = true;
                PlayTrack(audioQueue[0][0]);
                StartCoroutine(turnOffRadioIn(audioQueue[0][0].length));
            }
            audioQueue[0].RemoveAt(0);
        }
        else{
            List<int> queueKeys = audioQueue.Keys.OrderBy(i=>i).ToList();
            foreach(int key in queueKeys){
                audioQueue[key-1] = audioQueue[key];
                audioQueue.Remove(key);
            }
        }
    }

    IEnumerator turnOffRadioIn(float seconds){
        yield return new WaitForSeconds(seconds);
        playingRadio = false;
    }

    private bool AudioQueueEmpty(){
        return audioQueue.Keys.All(key=>(!audioQueue.ContainsKey(key))||audioQueue[key].Count==0);
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
        if(!muteSound) audioSource.PlayOneShot(clip,Loader.settings.master_volume);
    }

    public void PlayGameMusic(){
        if(muteAudio) return;

        audioSource.Stop();
        audioSource.clip = GameState.GameMode==Mode.FightCult? Fight: Dances;
        audioSource.loop = true;
        audioSource.volume = Loader.settings.master_volume/2f;
        audioSource.Play();

    }

    public bool isAudioOn()=>!muteAudio;
    public bool isSoundOn()=>!muteSound;


}

 [Serializable]
 public struct OptionClip {
    public string cardName;

    public int option;
    public AudioClip clip;
 }
