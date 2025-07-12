using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour {
    
    [SerializeField] private AnimationSwitcher ans;

    public AnimationClip idle;
    public AnimationClip dance;
    public AnimationClip happy;
    public AnimationClip cry;
    public AnimationClip angry;
    public AnimationClip surprise;
    public AnimationClip fearful;
    public AnimationClip disgusted;

    public void BindAnimationSwitcher(Avatar avatar) {
        ans.SwitchAvatar(avatar);
    }
    
    public void SetIdle() {
        ans.PlayAnimationClipLooped(idle);
    }

    public void Dance() {
        ans.PlayAnimationClip(dance);
    }

    public void Happy() {
        ans.PlayAnimationClip(happy);
    }
    
    public void Cry() {
        ans.PlayAnimationClip(cry);
    }
    
    public void Angry() {
        ans.PlayAnimationClip(angry);
    }
    
    public void Surprise() {
        ans.PlayAnimationClip(surprise);
    }
    
    public void Fearful() {
        ans.PlayAnimationClip(fearful);
    }
    
    public void Disgusted() {
        ans.PlayAnimationClip(disgusted);
    }
}
