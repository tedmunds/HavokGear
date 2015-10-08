using UnityEngine;
using System.Collections;


/// <summary>
/// MechActors are the main actors in the game. It contains all state for mechs like their health, armor
/// weapons and etc. The controller is the thing that actually uses these states however, and the Actor 
/// should never actually do stuff on its own.
/// </summary>
public class MechActor : Actor {

    public enum EAttachSide {
        Left, Right
    }

    /// <summary>
    /// Attachment points for guns or whatever
    /// </summary>
    [SerializeField]
    public Transform leftAttachPoint;

    [SerializeField]
    public Transform rightAttachPoint;

    /// <summary>
    /// Cached weapon reference for the item attached to each side
    /// </summary>
    [HideInInspector]
    public Weapon leftWeapon;

    [HideInInspector]
    public Weapon rightWeapon;

    private MechController controller;


	protected override void Start() {
        base.Start();
    }
    

    /// <summary>
    /// Called from controller component when this mech is spawned by world manager
    /// </summary>
    public virtual void OnSpawnInitialization() {
        controller = GetComponent<MechController>();
    }


    protected override void Update() {
        base.Update();

	}

    /// <summary>
    /// Attaches the input item to the mechs attach point indicated
    /// </summary>
    public void DoAttachment(EAttachSide attachSide, GameObject attachment, Vector3 attachOffset) {
        if(attachment == null) {
            return;
        }

        Weapon weaponComponent = attachment.GetComponent<Weapon>();

        // do the attachment through parenting
        if(attachSide == EAttachSide.Left) {
            if(leftWeapon != null) {
                Detach(leftWeapon.gameObject);
            }

            attachment.transform.parent = leftAttachPoint;
            leftWeapon = weaponComponent;
        }
        else {
            if(rightWeapon != null) {
                Detach(rightWeapon.gameObject);
            }

            attachment.transform.parent = rightAttachPoint;
            rightWeapon = weaponComponent;
        }

        // add whatever offset to the object
        attachment.transform.localPosition = attachOffset;
        attachment.transform.localRotation = Quaternion.identity;

        // if it was a weapon that was attached, then set its owner to this mech
        if(weaponComponent != null) {
            weaponComponent.owner = controller;
        }
    }


    /// <summary>
    /// Attempts to detach the input oobject if it is attached 
    /// </summary>
    public GameObject Detach(GameObject detachTarget) {
        if(detachTarget == null) {
            return null;
        }

        if(leftWeapon.gameObject == detachTarget) {
            GameObject detached = leftWeapon.gameObject;
            detached.transform.parent = null;
            leftWeapon = null;

            return detached;
        }
        else if(rightWeapon.gameObject == detachTarget) {
            GameObject detached = rightWeapon.gameObject;
            detached.transform.parent = null;
            rightWeapon = null;

            return detached;
        }

        return null;
    }



    /// <summary>
    /// Called when the mechs health goes below 0
    /// </summary>
    public override void Died() {
        base.Died();

        // TODO: death effects and stuff
        Destroy(gameObject);
    }

}
