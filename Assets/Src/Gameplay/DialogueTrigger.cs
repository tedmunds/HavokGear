using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour {

    [System.Serializable]
    public class DialoguePair {
        [SerializeField]
        public string text;

        [SerializeField]
        public Sprite characterPortrait;
    }


    [SerializeField]
    public string DialogueBoxTag = "UI_DialogueBox";

    [SerializeField]
    public List<DialoguePair> dialogueSequnce;

    private bool hasBeenTriggered;

    // UI elements found on start
    GameObject menuObject;
    private Text textBox;
    private Image potraitImage;

    private int currentSequenceIdx;
    private bool isActiveDialogue;

    private void Awake() {
        menuObject = GameObject.FindWithTag(DialogueBoxTag);

        // find the next button to add the on click listener too
        GameObject nextButtonObject = GameObject.FindWithTag("UI_DialogueNext");
        Button nextButton = nextButtonObject.GetComponent<Button>();
        nextButton.onClick.AddListener(() => NextDialoguePair());

        if(menuObject != null) {
            textBox = menuObject.GetComponentInChildren<Text>();

            Image[] imagesInHierarchy = menuObject.GetComponentsInChildren<Image>();
            foreach(Image img in imagesInHierarchy) {
                if(img.gameObject != menuObject) {
                    potraitImage = img;
                    break;
                }
            }
        }
        else {
            Debug.LogWarning(name + " could not find dialoge box object with the tag [" + DialogueBoxTag + "]");
        }

        hasBeenTriggered = false;
    }


    public void OnTriggerEnter2D(Collider2D other) {
        if(hasBeenTriggered) {
            return;
        }

        if(other.gameObject.GetComponent<PlayerController>() != null && menuObject != null) {
            hasBeenTriggered = true;

            WorldManager.instance.PauseGame(false);

            menuObject.SetActive(true);

            // Set hte intial text and stuff
            if(dialogueSequnce.Count > 0) {
                textBox.text = dialogueSequnce[0].text;

                if(dialogueSequnce[0].characterPortrait != null) {
                    potraitImage.sprite = dialogueSequnce[0].characterPortrait;
                    potraitImage.enabled = true;
                }
                else {
                    potraitImage.enabled = false;
                }
            }
            else {
                textBox.text = "";
                potraitImage.enabled = false;
            }
            isActiveDialogue = true;
        }
    }


    public void NextDialoguePair() {
        if(!hasBeenTriggered || !isActiveDialogue) {
            return;
        }

        currentSequenceIdx += 1;

        if(dialogueSequnce.Count > currentSequenceIdx) {
            textBox.text = dialogueSequnce[currentSequenceIdx].text;

            if(dialogueSequnce[currentSequenceIdx].characterPortrait != null) {
                potraitImage.sprite = dialogueSequnce[currentSequenceIdx].characterPortrait;
                potraitImage.enabled = true;
            }
            else {
                potraitImage.enabled = false;
            }
        }
        else {
            isActiveDialogue = false;
            menuObject.SetActive(false);
            WorldManager.instance.UnpauseGame();
        }
    }
}
