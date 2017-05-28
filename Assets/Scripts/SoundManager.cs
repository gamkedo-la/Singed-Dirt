using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance;

    private AudioSource menuMusicPlayer;
    private AudioSource gameplayMusicPlayer;
    private MenuSoundKind menuSoundKind = MenuSoundKind.menuSelect;
    private AudioClip menuOKSound;
    private AudioClip gameplayMusic;
    private AudioClip menuMusic;
    public float masterVolume = 1.0f;
    public float musicVolume = 0.2f;
    public float SFXVolume = 0.5f;
    Slider masterVolumeSlider;
    Slider musicVolumeSlider;
    Slider SFXVolumeSlider;
    private bool justStarted = true;
    private AudioClip intro1;
    private AudioClip intro2;

    void Awake(){
        if (instance == null) {
            instance = this;
        } else {
            // Debug.Log("Sound manager created another instance, destroying.");
            Destroy(gameObject);
        }
    }

    void Start () {
        DontDestroyOnLoad(gameObject);
        InitializeMusic();
		if (justStarted == true)
		{
			if (Random.Range(1, 2) == 1)
			{
				intro1 = (AudioClip)Resources.Load("Voiceovers/" + VoiceoversKind.voice_hi_intro_1_dry);
				PlayAudioClip(intro1);
				justStarted = false;
			}
			else
			{
				intro2 = (AudioClip)Resources.Load("Voiceovers/" + VoiceoversKind.voice_hi_intro_2_dry);
				PlayAudioClip(intro2);
				justStarted = false;
			}
		}
    }

    // Update is called once per frame
    public void ChangeMusicVolume (float volume) {
        gameplayMusicPlayer.volume = volume;
        menuMusicPlayer.volume = volume;
    }

    public void ChangeSFXVolume(float volume){
        SFXVolume = volume;
    }

    public void ChangeMasterVolume(float volume){
        AudioListener.volume = volume;
    }

    public void SetVolumeSliders(){
        masterVolumeSlider = GameObject.Find("MasterVolumeSlider").GetComponent<Slider>();
        masterVolumeSlider.value = masterVolume;
        masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);

        musicVolumeSlider = GameObject.Find("MusicVolumeSlider").GetComponent<Slider>();
        musicVolumeSlider.value = musicVolume;
        musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);

        SFXVolumeSlider = GameObject.Find("SFXVolumeSlider").GetComponent<Slider>();
        SFXVolumeSlider.value = SFXVolume;
        SFXVolumeSlider.onValueChanged.AddListener(ChangeSFXVolume);
    }

    void CreateMusicGameObjects(){
        GameObject tempGO = new GameObject("gameplayMusicPlayer");
        tempGO.transform.SetParent(transform);
        gameplayMusicPlayer = tempGO.AddComponent<AudioSource>() as AudioSource;
        gameplayMusicPlayer.volume = musicVolume;
        gameplayMusicPlayer.loop = true;

        GameObject tempGO2 = new GameObject("menuMusicPlayer");
        tempGO2.transform.SetParent(transform);
        menuMusicPlayer = tempGO2.AddComponent<AudioSource>() as AudioSource;
        menuMusicPlayer.volume = musicVolume;
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

    public void PlayAudioClip(SingedMessages.PlayAudioClipMessage msg) {
        var clip = PrefabRegistry.singleton.GetResource<AudioClip>(msg.resourcePath);
        if (clip != null) {
            if (msg.delay != 0f) {
                PlayClipDelayed(msg.delay, clip, msg.pitchModulation);
            } else {
                PlayAudioClip(clip, msg.pitchModulation);
            }
        }
    }

    AudioSource CreateAudioSource(GameObject parent, string name, string resourcePath) {
        // create new game object w/ given name
        GameObject audioGO = new GameObject(name);
        audioGO.transform.SetParent((parent != null) ? parent.transform: Camera.main.transform);
         // add the audio source
        AudioSource audioSource = audioGO.AddComponent<AudioSource>();
        // set volume and clip to play
        audioSource.volume = SFXVolume;
        audioSource.clip = PrefabRegistry.singleton.GetResource<AudioClip>(resourcePath);
        return audioSource;
    }

    public void StartAudioLoop(GameObject parent, string name, string resourcePath) {
        //Debug.Log("StartAudioLoop: " + name);
        AudioSource audioSource;
        var audioTrans = parent.transform.Find(name);
        if (audioTrans == null) {
            audioSource = CreateAudioSource(parent, name, resourcePath);
        } else {
            audioSource = audioTrans.gameObject.GetComponent<AudioSource>();
        }
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }

    public void StopAudioLoop(GameObject parent, string name) {
        //Debug.Log("StopAudioLoop: " + name);
        var audioTrans = parent.transform.Find(name);
        if (audioTrans != null) {
            var audioSource = audioTrans.gameObject.GetComponent<AudioSource>();
            audioSource.Stop();
        }
    }

    public void PlayBattleMusic() {
        StartCoroutine(FadeIntoNewSong(menuMusicPlayer, gameplayMusicPlayer));
    }

    public void PlayMenuMusic(){
        StartCoroutine(FadeIntoNewSong(gameplayMusicPlayer, menuMusicPlayer));
    }

    public void PlayAudioClip(AudioClip clip, bool pitchModulation = false) {
        GameObject tempGO = new GameObject("TempAudio"); // create the temp object

        tempGO.transform.SetParent(Camera.main.transform);

        AudioSource aSource = tempGO.AddComponent<AudioSource>() as AudioSource; // add an audio source
        aSource.clip = clip; // define the clip
        aSource.volume = SFXVolume;
        if(pitchModulation != false){  // e.g. we don't want to modulate voices
            aSource.pitch = Random.Range(0.7f,1.4f);
        }
        // set other aSource properties here, if desired
        aSource.Play(); // start the sound
        Destroy(tempGO, clip.length/aSource.pitch); // destroy object after clip duration
    }

    public void PlayClipDelayed(float delay, AudioClip clip, bool pitchModulation = false) {
        StartCoroutine(WaitThenPlaySound(delay, clip, pitchModulation));
    }

    IEnumerator WaitThenPlaySound(float waitSec, AudioClip clip, bool pitchModulation) {
        yield return new WaitForSeconds(waitSec);
        PlayAudioClip(clip, pitchModulation);
    }
}
