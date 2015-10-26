using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

    [SerializeField]
    private float explosionRadius;

    [SerializeField]
    private float baseDamage;

    [SerializeField]
    private float lifeTime;

    [SerializeField]
    private Animator explosionEffectPrefab;

    [SerializeField]
    private AudioClip explodeSound;


    private AudioSource audioPlayer;

    private float explodedTime;
    private bool hasExploded = false;



	private void Update() {
        if(hasExploded && Time.time - explodedTime > lifeTime) {
            Destroy(gameObject);
        }
	}


    public void Explode(Weapon sourceWeapon) {
        if(hasExploded || sourceWeapon == null) {
            return;
        }

        audioPlayer = GetComponent<AudioSource>();

        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        for(int i = 0; i < overlaps.Length; i++) {
            Actor mech = overlaps[i].GetComponent<Actor>();
            if(mech != null) {
                mech.TakeDamage(baseDamage, sourceWeapon.owner.GetComponent<MechController>(), sourceWeapon);
            }
        }

        // Create the effect
        Animator explosionAnimator = (Animator)Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        Destroy(explosionAnimator.gameObject, 0.3f);

        if(audioPlayer != null && explodeSound != null) {
            audioPlayer.PlayOneShot(explodeSound);
        }

        explodedTime = Time.time;
        hasExploded = true;
    }



}
