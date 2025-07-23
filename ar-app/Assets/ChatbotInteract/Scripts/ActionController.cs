using UnityEngine;

public class ActionController : MonoBehaviour
{
    public AnimationManager am;
    public FacialSwitcher fs;

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

        if (am == null || fs == null)
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
                fs.SetIdle();
                break;

            case "fear":
                am.SetFear();
                fs.SetFear();
                break;

            case "love":
                am.SetLove();
                fs.SetLove();
                break;

            case "instability":
                am.SetInstability();
                fs.SetInstability();
                break;

            case "disgust":
                am.SetDisgust();
                fs.SetDisgust();
                break;

            case "disappointment":
                am.SetDisappointment();
                fs.SetDisappointment();
                break;

            case "shame":
                am.SetShame();
                fs.SetShame();
                break;

            case "anger":
                am.SetAnger();
                fs.SetAnger();
                break;

            case "jealous":
                am.SetJealous();
                fs.SetJealous();
                break;

            case "sadness":
                am.SetSadness();
                fs.SetSadness();
                break;

            case "envy":
                am.SetEnvy();
                fs.SetEnvy();
                break;

            case "joy":
                am.SetJoy();
                fs.SetJoy();
                break;

            case "guilt":
                am.SetGuilt();
                fs.SetGuilt();
                break;
            
        }
    }
}