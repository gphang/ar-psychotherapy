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