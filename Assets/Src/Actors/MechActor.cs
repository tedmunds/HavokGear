using UnityEngine;
using System.Collections;

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
            attachment.transform.parent = leftAttachPoint;
            leftWeapon = weaponComponent;
        }
        else {
            attachment.transform.parent = rightAttachPoint;
            rightWeapon = weaponComponent;
        }

        // add whatever offset to the object
        attachment.transform.localPosition = attachOffset;

        // if it was a weapon that was attached, then set its owner to this mech
        if(weaponComponent != null) {
            weaponComponent.owner = controller;
        }
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
