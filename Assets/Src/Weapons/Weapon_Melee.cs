using UnityEngine;
using System.Collections;

public class Weapon_Melee : Weapon {

    [SerializeField]
    public float baseDamage;

    [SerializeField]
    public float maxHitArc;

    [SerializeField]
    public bool applyForce;




    protected override void Start() {
        base.Start();
    }



    protected override void OnDisable() {
        base.OnDisable();

    }


    protected override void Update() {
        base.Update();

    }


    protected override void Fire() {
        base.Fire();

    }
}
