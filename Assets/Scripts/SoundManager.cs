using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	public static SoundManager instance;

    private AudioSource menuMusicPlayer;
    private AudioSource gameplayMusicPlayer;
    private MenuSoundKind menuSoundKind = MenuSoundKind.menuSelect;
    private AudioClip menuOKSound;
    private AudioClip gameplayMusic;
	private AudioClip menuMusic;
	// Use this for initialization
	void Awake(){
		if (instance == null) {
			instance = this;
		} else {
			Debug.Log("Sound manager created another instance, destroying.");
			Destroy(gameObject);
		}
	}
	void Start () {
		DontDestroyOnLoad(gameObject);
		InitializeMusic();
	}
	
	// Update is called once per frame
	void Update () {
		
	}   

	void CreateMusicGameObjects(){
	    GameObject tempGO = new GameObject("gameplayMusicPlayer");
        tempGO.transform.SetParent(transform);
        gameplayMusicPlayer = tempGO.AddComponent<AudioSource>() as AudioSource;
        gameplayMusicPlayer.volume = 0.2f;
        gameplayMusicPlayer.loop = true;

		GameObject tempGO2 = new GameObject("menuMusicPlayer");
		tempGO2.transform.SetParent(transform);
		menuMusicPlayer = tempGO2.AddComponent<AudioSource>() as AudioSource;
		menuMusicPlayer.volume = 0.2f;
		menuMusicPlayer.loop = true;
    }

	void CreateMusicAudioClips(){
		gameplayMusic = (AudioClip)Resources.Load("Music/" + MusicKind.gameplayMusic);
		menuMusic = (AudioClip)Resources.Load("Music/" + MusicKind.mainMenuMusic);
	}

	void InitializeMusic(){
		CreateMusicGameObjects();
		CreateMusicAudioClips();
		menuMusicPlayer.clip = menuMusic;
		gameplayMusicPlayer.clip = gameplayMusic;
	}

	public IEnumerator FadeIntoNewSong(AudioSource fadeFromMe, AudioSource startMe){
        if(fadeFromMe.volume > 0){
            float startVolume = fadeFromMe.volume;
                    while (fadeFromMe.volume > 0){
            fadeFromMe.volume -= startVolume * Time.deltaTime / 1.0f;

            yield return null;
        }
            fadeFromMe.Stop();
            fadeFromMe.volume = startVolume;
            startMe.Play();
        } else {
            fadeFromMe.Stop();
            startMe.Play();
        }

    }

	public void PlayBattleMusic() {
        StartCoroutine(FadeIntoNewSong(menuMusicPlayer, gameplayMusicPlayer));
    }

	public void PlayMenuMusic(){
		StartCoroutine(FadeIntoNewSong(gameplayMusicPlayer, menuMusicPlayer));
	}

	public void PlayAudioClip(AudioClip clip, float atVol = 1.0f, bool pitchModulation = false) {
		GameObject tempGO = new GameObject("TempAudio"); // create the temp object
		
        tempGO.transform.SetParent(Camera.main.transform);

		AudioSource aSource = tempGO.AddComponent<AudioSource>() as AudioSource; // add an audio source
		aSource.clip = clip; // define the clip
		aSource.volume = atVol;
        if(pitchModulation != false){  // e.g. we don't want to modulate voices
            aSource.pitch = Random.Range(0.7f,1.4f);
        }
		// set other aSource properties here, if desired
		aSource.Play(); // start the sound
		Destroy(tempGO, clip.length/aSource.pitch); // destroy object after clip duration
	}

    public void PlayClipDelayed(float delay, AudioClip clip, float atVol = 1.0f, bool pitchModulation = false) {
		StartCoroutine(WaitThenPlaySound(delay, clip, atVol, pitchModulation));
	}

    IEnumerator WaitThenPlaySound(float waitSec, AudioClip clip, float atVol, bool pitchModulation) {
        yield return new WaitForSeconds(waitSec);
        PlayAudioClip(clip, atVol, pitchModulation);
    }
}
