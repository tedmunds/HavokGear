using UnityEngine;

public class SetInactiveOnStart : MonoBehaviour {

	private void Start() {
        gameObject.SetActive(false);
	}
}
