using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Behaviour state machine for AI's: Is a stack based state machine
/// </summary>
public class BehaviourSM  {

    /// <summary>
    /// Base class for all ai states.
    /// Each update, the state sends a response which can contain a new state with instructions on how to transition to it
    /// </summary>
    public abstract class BehaviourState {
        public abstract StateResponse Update(AIController controller);
    }

    /// <summary>
    /// State response contains what state to add, and how to go to that state
    /// </summary>
    public struct StateResponse {
        public BehaviourState newState;
        public TransitionMode transitionMode;

        public StateResponse(TransitionMode transitionMode, BehaviourState newState = null) {
            this.transitionMode = transitionMode;
            this.newState = newState;
        }
    }

    public enum TransitionMode {
        NoChange,           // Does not do anything
        PopPrevious,        // pops the previous state, destroying the current state and not using any new state
        PushCurrent,        // pushes the current state onto the stack and goes to the new state
        AbandonCurrent,     // just goes to the new state, destroying the current state
    }


    /// <summary>
    /// The fallback state that always lives at the bottom of the stack
    /// </summary>
    private BehaviourState baseState;

    /// <summary>
    /// Main stack implementation of stack based state machine
    /// </summary>
    private Stack<BehaviourState> stateStack;

    /// <summary>
    /// The state that is currently being updated
    /// </summary>
    private BehaviourState currentState;

    /// <summary>
    /// The ai controller that is using this state machine
    /// </summary>
    private AIController owner;



    public BehaviourSM(AIController owner) {
        this.owner = owner;

        stateStack = new Stack<BehaviourState>();
        baseState = new Behaviour_Idle();
        currentState = baseState;
    }


    public void ResetDefault() {
        stateStack.Clear();
        baseState = new Behaviour_Idle();
        currentState = baseState;
    }



    /// <summary>
    /// Main update for state machine: will update the currently active state and do transitions to other states
    /// </summary>
    public void UpdateState() {
        if(currentState != null) {
            StateResponse response = currentState.Update(owner);

            switch(response.transitionMode) {
                case TransitionMode.PopPrevious:
                    // Go back to the previous state on the stack
                    BehaviourState nextState = stateStack.Pop();
                    if(nextState != null) {
                        currentState = nextState;
                    }
                    else {
                        // in case there was no state on the stack, go back to base state
                        currentState = baseState;
                    }
                    break;
                case TransitionMode.PushCurrent:
                    // Pushes the current state onto the stack and goes to the returned next state
                    stateStack.Push(currentState);
                    currentState = response.newState;
                    if(currentState == null) {
                        // If a null state was added, go back tot eh previous one
                        currentState = stateStack.Pop();
                    }
                    break;
                case TransitionMode.AbandonCurrent:
                    // Go to the new state, without pushing the current one on the stack
                    if(response.newState != null) {
                        currentState = response.newState;
                    }
                    break;
            }

        }
    }
}
