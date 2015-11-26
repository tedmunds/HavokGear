using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class UI_EventPopup : MonoBehaviour {


    [SerializeField]
    public float fadeOutTime;


    private float lastStartTime;

    private Text textField;

    private void Start() {
        textField = GetComponent<Text>();
        textField.enabled = false;
    }


    private void Update() {
        float elapsed = Time.time - lastStartTime;

        if(elapsed > fadeOutTime && textField.enabled) {
            textField.color = new Color(255, 255, 255, 0);
            textField.enabled = false;
        }
        else if(elapsed > fadeOutTime / 2.0f) {
            float alpha = 1.0f - (elapsed - fadeOutTime / 2.0f) / (fadeOutTime / 2.0f);

            // Square it for a more exponential fade out
            //alpha = alpha * alpha; 
            textField.color = new Color(1.0f, 1.0f, 1.0f, alpha);
        }
        else {
            textField.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }


    public void DoAnnouncement(string newText) {
        lastStartTime = Time.time;
        textField.text = newText;
        textField.enabled = true;
    }

}
