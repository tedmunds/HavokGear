using UnityEngine;
using System.Collections;

/// <summary>
/// Actor is the base class for all objects in the game taht can take damage and stuff
/// </summary>
public class Actor : MonoBehaviour {
    
    [SerializeField]
    public float maxhealth;

    // Current health
    private float health;
	
	protected virtual void Start() {
        health = maxhealth;
    }


    protected virtual void Update() {
	    
	}
}
