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

    private bool isDead;
    public bool IsDead {
        get { return isDead; }
    }


	protected virtual void Start() {
        health = maxhealth;
        isDead = false;
    }


    protected virtual void Update() {
	    
	}




    /// <summary>
    /// Applies the damage to the actor
    /// </summary>
    public void TakeDamage(float damageAmount, MechController instigator, Weapon weaponUsed) {
        if(isDead) {
            return;
        }

        health -= damageAmount;
        
        if(health <= 0.0f) {
            health = 0.0f;
            Died();
        }
    }




    public virtual void Died() {
        isDead = true;
    }

}
