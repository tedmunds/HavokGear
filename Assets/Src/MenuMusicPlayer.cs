using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuMusicPlayer : MusicPlayer {

    [SerializeField]
    public List<AudioClip> menuMusicSegments;


	private void Start() {
        audioPlayer = GetComponent<AudioSource>();

        musicSegments = menuMusicSegments;

        if(musicSegments.Count > 0
            && ShouldPlayMusic) {

            audioPlayer.clip = musicSegments[0];
            audioPlayer.Play();
            currentSegmentLength = musicSegments[0].length;
            currentSegmentIndex = 0;
            lastSegmentStartTime = Time.time;
        }
	}
	
}
