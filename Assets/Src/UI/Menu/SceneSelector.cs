using UnityEngine;
using System.Collections;

public class SceneSelector : MonoBehaviour {



    public void GoToScene(string sceneName) {
        Debug.Log(sceneName + " selected: Transitioning there now...");
        Application.LoadLevel(sceneName);
    }

}
