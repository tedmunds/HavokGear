using UnityEngine;
using System.Collections;

public class BossFightManager : MonoBehaviour {

    [SerializeField]
    public float restartDelayTime = 1.0f;

    private PlayerController player;
	
	private void Start() {
        StartCoroutine(FindPlayer());
	}
	

    private IEnumerator FindPlayer() {
        for(; player == null; ) {
            player = FindObjectOfType<PlayerController>();
            yield return null;
        }

        player.MechComponent.RegisterDeathListener(OnPlayerDied);
    }


    public void OnPlayerDied(Actor victim) {
        StartCoroutine(ReloadLevelAfterDelay(restartDelayTime));
    }


    public IEnumerator ReloadLevelAfterDelay(float delay) {
        float initTime = Time.time;

        for(; Time.time - initTime < delay; ) {
            yield return null;
        }

        // Just reload the entire scne... cause I'm lazy
        string bossFightScene = Application.loadedLevelName;
        Application.LoadLevel(bossFightScene);
    }
}
