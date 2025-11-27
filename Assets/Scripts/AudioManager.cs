using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Header("Music Tracks")]
    public List<AudioClip> songs = new List<AudioClip>();

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    public bool loopPlaylist = true;

    private AudioSource audioSource;
    private List<AudioClip> playlist = new List<AudioClip>();
    private int currentSongIndex = 0;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) 
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = false; 
        audioSource.playOnAwake = false;
        
        // Removed DontDestroyOnLoad to ensure music restarts on level reload
    }

    void Start()
    {
        if (songs.Count > 0)
        {
            StartPlaylist();
        }
    }

    void Update()
    {
        audioSource.volume = volume;

        if (!audioSource.isPlaying && audioSource.clip != null && audioSource.time == 0f)
        {
             if (audioSource.clip.length - audioSource.time <= 0.1f || !audioSource.isPlaying)
             {
                 PlayNextSong();
             }
        }
    }

    void StartPlaylist()
    {
        playlist = new List<AudioClip>(songs);
        ShufflePlaylist();
        
        currentSongIndex = 0;
        if (playlist.Count > 0)
        {
            PlaySong(playlist[0]);
        }
    }

    void ShufflePlaylist()
    {
        for (int i = 0; i < playlist.Count; i++)
        {
            AudioClip temp = playlist[i];
            int randomIndex = Random.Range(i, playlist.Count);
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
    }

    void PlayNextSong()
    {
        currentSongIndex++;

        if (currentSongIndex >= playlist.Count)
        {
            if (loopPlaylist)
            {
                ShufflePlaylist();
                currentSongIndex = 0;
            }
            else
            {
                return; 
            }
        }
        PlaySong(playlist[currentSongIndex]);
    }

    void PlaySong(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }
}