using UnityEngine;
using System.Collections;

public class AIController : MechController {

    /// <summary>
    /// The mech that this Ai is targetting for shooting etc
    /// </summary>
    private MechController target;



	// Use this for initialization
	protected override void Start() {
        base.Start();
	}


    /// <summary>
    /// Instructs the ai to start sensing for a target (does not mean it will actually start chasing and shooting)
    /// </summary>
    public void AiStartSensing() {

    }


    // Update is called once per frame
    protected override void Update() {
        base.Update();
	}
}
