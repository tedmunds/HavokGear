using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class UI_EventPopup : MonoBehaviour {


    [SerializeField]
    public float fadeOutTime;

    [SerializeField]
    private Text textField;

    private Text backgroundTextfield;

    private float lastStartTime;

    private void Start() {
        backgroundTextfield = GetComponent<Text>();
        backgroundTextfield.enabled = false;
        textField.enabled = false;
    }


    private void Update() {
        float elapsed = Time.time - lastStartTime;

        if(elapsed > fadeOutTime && textField.enabled) {
            textField.color = new Color(255, 255, 255, 0);
            backgroundTextfield.color = new Color(0, 0, 0, 0);
            textField.enabled = false;
            backgroundTextfield.enabled = false;
        }
        else if(elapsed > fadeOutTime / 2.0f) {
            float alpha = 1.0f - (elapsed - fadeOutTime / 2.0f) / (fadeOutTime / 2.0f);

            // Square it for a more exponential fade out
            textField.color = new Color(1.0f, 1.0f, 1.0f, 0);
            backgroundTextfield.color = new Color(0, 0, 0, 0);
        }
        else {
            textField.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            backgroundTextfield.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        }
    }


    public void DoAnnouncement(string newText) {
        lastStartTime = Time.time;
        textField.text = newText;
        backgroundTextfield.text = newText;
        textField.enabled = true;
        backgroundTextfield.enabled = true;
    }

}
