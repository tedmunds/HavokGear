using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour {

    //[SerializeField]
    protected List<AudioClip> musicSegments;

    [SerializeField]
    public float segmentDelayLength;

    [SerializeField]
    public bool ShouldPlayMusic;

    protected AudioSource audioPlayer;

    protected float currentSegmentLength;
    protected int currentSegmentIndex;

    protected float lastSegmentStartTime;

	private void Start() {
	    audioPlayer = GetComponent<AudioSource>();

        musicSegments = WorldManager.instance.musicSegments;

        if(musicSegments.Count > 0 
            && ShouldPlayMusic) {

            audioPlayer.clip = musicSegments[0];
            audioPlayer.Play();
            currentSegmentLength = musicSegments[0].length;
            currentSegmentIndex = 0;
            lastSegmentStartTime = Time.realtimeSinceStartup;
        }
	}
	
	
	private void Update() {
        if(musicSegments.Count <= 0) {
            return;
        }

        if(Time.realtimeSinceStartup - lastSegmentStartTime > currentSegmentLength + segmentDelayLength
            && ShouldPlayMusic) {

            currentSegmentIndex += 1;
            if(currentSegmentIndex >= musicSegments.Count) {
                currentSegmentIndex = 0;
            }

            audioPlayer.clip = musicSegments[currentSegmentIndex];
            audioPlayer.Play();
            currentSegmentLength = musicSegments[currentSegmentIndex].length;
            lastSegmentStartTime = Time.realtimeSinceStartup;
        }    
	}





}
