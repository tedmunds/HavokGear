using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour {

    //[SerializeField]
    private List<AudioClip> musicSegments;

    [SerializeField]
    public float segmentDelayLength;

    [SerializeField]
    public bool ShouldPlayMusic;

    private AudioSource audioPlayer;

    private float currentSegmentLength;
    private int currentSegmentIndex;
    
    private float lastSegmentStartTime;

	private void Start() {
	    audioPlayer = GetComponent<AudioSource>();

        musicSegments = WorldManager.instance.musicSegments;

        if(musicSegments.Count > 0 
            && ShouldPlayMusic) {

            audioPlayer.clip = musicSegments[0];
            audioPlayer.Play();
            currentSegmentLength = musicSegments[0].length;
            currentSegmentIndex = 0;
            lastSegmentStartTime = Time.time;
        }
	}
	
	
	private void Update() {
        if(musicSegments.Count <= 0) {
            return;
        }

        if(Time.time - lastSegmentStartTime > currentSegmentLength + segmentDelayLength
            && ShouldPlayMusic) {

            currentSegmentIndex += 1;
            if(currentSegmentIndex >= musicSegments.Count) {
                currentSegmentIndex = 0;
            }

            audioPlayer.clip = musicSegments[currentSegmentIndex];
            audioPlayer.Play();
            currentSegmentLength = musicSegments[currentSegmentIndex].length;
            lastSegmentStartTime = Time.time;
        }    
	}





}
