using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] musicArray;
    public AudioSource source;
    int CurrentSong;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        CurrentSong = Random.Range(0, musicArray.Length);
        source.loop = true;
        source.clip = musicArray[CurrentSong];
        source.Play();
    }
}
