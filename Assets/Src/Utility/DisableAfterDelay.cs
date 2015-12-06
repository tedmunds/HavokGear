using UnityEngine;
using System.Collections;

public class DisableAfterDelay : MonoBehaviour {

    [SerializeField]
    public float disableDelay = 0.5f;

    private float enabledTime;

    private void OnEnable() {
        enabledTime = Time.time;
    }

    private void Update() {
        float elapsed = Time.time - enabledTime;
        if(elapsed > disableDelay) {
            gameObject.SetActive(false);
        }
    }
}
