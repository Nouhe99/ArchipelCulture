using System.Collections;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    public AudioSource audioSourceFx;
    public AudioSource audioSourceMusique;
    public PlaySound bank;

    private float volumeSfx;

    public void EnableMusic(bool play = true)
    {
        audioSourceMusique.mute = !play;
    }
    public void EnableSfx(bool play = true)
    {
        audioSourceFx.mute = !play;
        if (GameObject.FindGameObjectWithTag("PlayerBoat")) GameObject.FindGameObjectWithTag("PlayerBoat").GetComponent<AudioSource>().enabled = play;//grincement du bateau, double audiosource dans la scène
    }
    public bool IsSfxEnabled()
    {
        return !audioSourceFx.mute;
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (clip != null)
        {
            audioSourceFx.volume = volumeSfx;
            audioSourceFx.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioClip music, string source = "music")
    {
        if (source == "fx") StartCoroutine(PlayMusicAsync(music, audioSourceFx));
        else StartCoroutine(PlayMusicAsync(music, audioSourceMusique));

    }

    private IEnumerator PlayMusicAsync(AudioClip music, AudioSource audioSource)
    {
        if (audioSource.isPlaying)
        {
            float volumeMusic = audioSource.volume;
            float step = volumeMusic / 6;
            for (float i = volumeMusic; i > 0; i -= step)
            {
                audioSource.volume = i;
                yield return new WaitForSeconds(0.1f);
            }
            audioSource.volume = volumeMusic;
        }

        audioSource.Stop();
        audioSource.clip = music;
        audioSource.Play();
    }

    public static PlayAudio Instance;
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        if (PlayerPrefs.GetInt("MusicPref") == 2) EnableMusic(false);
        if (PlayerPrefs.GetInt("SoundPref") == 2) EnableSfx(false);
        audioSourceMusique.loop = true;
        volumeSfx = audioSourceFx.volume;
    }

}
