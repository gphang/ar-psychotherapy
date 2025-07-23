// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class AnimationManager : MonoBehaviour {
    
//     [SerializeField] private AnimationSwitcher ans;

//     public AnimationClip idle;
//     public AnimationClip fear;
//     public AnimationClip love;
//     public AnimationClip instability;
//     public AnimationClip disgust;
//     public AnimationClip disappointment;
//     public AnimationClip shame;
//     public AnimationClip anger;
//     public AnimationClip jealous;
//     public AnimationClip sadness;
//     public AnimationClip envy;
//     public AnimationClip joy;
//     public AnimationClip guilt;

//     public void BindAnimationSwitcher(Avatar avatar) {
//         ans.SwitchAvatar(avatar);
//     }
    
//     public void SetIdle() {
//         ans.PlayAnimationClipLooped(idle);
//     }

//     public void SetFear() {
//         ans.PlayAnimationClip(fear);
//     }

//     public void SetLove() {
//         ans.PlayAnimationClip(love);
//     }
    
//     public void SetInstability() {
//         ans.PlayAnimationClip(instability);
//     }
    
//     public void SetDisgust() {
//         ans.PlayAnimationClip(disgust);
//     }
    
//     public void SetDisappointment() {
//         ans.PlayAnimationClip(disappointment);
//     }
    
//     public void SetShame() {
//         ans.PlayAnimationClip(shame);
//     }
    
//     public void SetAnger() {
//         ans.PlayAnimationClip(anger);
//     }

//     public void SetJealous() {
//         ans.PlayAnimationClip(jealous);
//     }
    
//     public void SetSadness() {
//         ans.PlayAnimationClip(sadness);
//     }
    
//     public void SetEnvy() {
//         ans.PlayAnimationClip(envy);
//     }
    
//     public void SetJoy() {
//         ans.PlayAnimationClip(joy);
//     }
    
//     public void SetGuilt() {
//         ans.PlayAnimationClip(guilt);
//     }
// }


using UnityEngine;

public class AnimationManager : MonoBehaviour 
{
    // This will hold a direct reference to the avatar's animator component
    private Animator avatarAnimator;
    
    // This method will be called by AvatarDisplay to give us the animator
    public void BindAnimator(Animator animatorToControl) 
    {
        avatarAnimator = animatorToControl;
        Debug.Log("<color=green>SUCCESS:</color> AnimationManager is now bound to an animator.", avatarAnimator.gameObject);
    }
    
    // A helper method to ensure we don't try to animate a null object
    // and to reset other triggers for a clean transition.
    private void Play(string triggerName)
    {
        if (avatarAnimator == null)
        {
            Debug.LogError("Cannot play animation because the Avatar's Animator has not been bound!");
            return;
        }

        // Reset all other triggers first
        foreach (var param in avatarAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                avatarAnimator.ResetTrigger(param.name);
            }
        }
        
        // Fire the new trigger
        avatarAnimator.SetTrigger(triggerName);
    }
    
    // --- Public Methods for ActionController to Call ---

    public void SetIdle()           { Play("TriggerIdle"); }
    public void SetFear()           { Play("TriggerFear"); }
    public void SetLove()           { Play("TriggerLove"); }
    public void SetInstability()    { Play("TriggerInstability"); }
    public void SetDisgust()        { Play("TriggerDisgust"); }
    public void SetDisappointment() { Play("TriggerDisappointment"); }
    public void SetShame()          { Play("TriggerShame"); }
    public void SetAnger()          { Play("TriggerAnger"); }
    public void SetJealous()        { Play("TriggerJealous"); }
    public void SetSadness()        { Play("TriggerSadness"); }
    public void SetEnvy()           { Play("TriggerEnvy"); }
    public void SetJoy()            { Play("TriggerJoy"); }
    public void SetGuilt()          { Play("TriggerGuilt"); }
}