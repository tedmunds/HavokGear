using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour {

    [SerializeField]
    public string DialogueBoxTag = "UI_DialogueBox";

    [SerializeField]
    public string dialogueText;

    [SerializeField]
    public Sprite characterSprite;

    private bool hasBeenTriggered;

    // UI elements found on start
    GameObject menuObject;
    private Text textBox;
    private Image potraitImage;

    private void Awake() {
        menuObject = GameObject.FindWithTag(DialogueBoxTag);
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

            textBox.text = dialogueText;
            if(characterSprite != null) {
                potraitImage.sprite = characterSprite;
                potraitImage.enabled = true;
            }
            else {
                potraitImage.enabled = false;
            }
        }
    }
}
