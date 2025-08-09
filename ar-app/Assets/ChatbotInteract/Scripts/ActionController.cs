using UnityEngine;

public class ActionController : MonoBehaviour
{
    public AnimationManager am;
    public FacialSwitcher fs_child;
    public FacialSwitcher fs_therapist;

    // This method is called when the script is enabled
    private void OnEnable()
    {
        // Subscribe to the event from the classifier.
        EmotionClassifier.OnEmotionClassified += HandleEmotion;

        // This message should appear in your console the moment you start the scene.
        Debug.Log("<color=cyan>ActionController is AWAKE and LISTENING for emotions.</color>", this.gameObject);
        EmotionClassifier.OnEmotionClassified += HandleEmotion;
    }

    // This is called when the script is disabled. Always unsubscribe to prevent errors.
    private void OnDisable()
    {
        EmotionClassifier.OnEmotionClassified -= HandleEmotion;
    }

    // This method runs when an emotion is detected and announced.
    private void HandleEmotion(string emotion)
    {
        Debug.Log("Emotion '" + emotion + "' detected. Triggering action.");

        if (am == null || fs_child == null || fs_therapist == null)
        {
            Debug.LogError("CRITICAL: AnimationManager (am) or FacialSwitcher (fs) is NOT ASSIGNED in the ActionController's Inspector!");
            return;
        }

        if (am == null)
        {
            Debug.LogError("AnimationManager is not assigned in the EmotionActionController!");
            return;
        }

        // Set facial expression + animation of same name
        switch(emotion)
        {
            case "idle":
                am.SetIdle();
                fs_child.SetIdle();
                fs_therapist.SetIdle();
                break;

            case "fear":
                am.SetFear();
                fs_child.SetFear();
                fs_therapist.SetFear();
                break;

            case "love":
                am.SetLove();
                fs_child.SetLove();
                fs_therapist.SetLove();
                break;

            case "instability":
                am.SetInstability();
                fs_child.SetInstability();
                fs_therapist.SetInstability();
                break;

            case "disgust":
                am.SetDisgust();
                fs_child.SetDisgust();
                fs_therapist.SetDisgust();
                break;

            case "disappointment":
                am.SetDisappointment();
                fs_child.SetDisappointment();
                fs_therapist.SetDisappointment();
                break;

            case "shame":
                am.SetShame();
                fs_child.SetShame();
                fs_therapist.SetShame();
                break;

            case "anger":
                am.SetAnger();
                fs_child.SetAnger();
                fs_therapist.SetAnger();
                break;

            case "jealous":
                am.SetJealous();
                fs_child.SetJealous();
                fs_therapist.SetJealous();
                break;

            case "sadness":
                am.SetSadness();
                fs_child.SetSadness();
                fs_therapist.SetSadness();
                break;

            case "envy":
                am.SetEnvy();
                fs_child.SetEnvy();
                fs_therapist.SetEnvy();
                break;

            case "joy":
                am.SetJoy();
                fs_child.SetJoy();
                fs_therapist.SetJoy();
                break;

            case "guilt":
                am.SetGuilt();
                fs_child.SetGuilt();
                fs_therapist.SetGuilt();
                break;
            
        }
    }
}