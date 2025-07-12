using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance { get; private set; }

    // public string CurrentAvatarUrl { get; private set; }
    public string ChildAvatarUrl { get; private set; }
    public string TherapistAvatarUrl { get; private set; }
     

    private void Awake()
    {
        // Ensure only one instance of AvatarManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional, if you want it to persist across scenes
            Debug.Log("[AvatarManager] Instance is set.");
        }
        else
        {
            Debug.Log("[AvatarManager] Another instance of AvatarManager exists. Destroying this one.");
            Destroy(gameObject);
        }
    }

    // public void SetCurrentAvatar(string avatarUrl)
    // {
    //     // Debug log to confirm method is being called
    //     Debug.Log("SetCurrentAvatar called with avatar URL: " + avatarUrl);

    //     // Check for null values
    //     if (string.IsNullOrEmpty(avatarUrl))
    //     {
    //         Debug.LogError("SetCurrentAvatar received null avatar or empty URL.");
    //         return;
    //     }
 
    //     CurrentAvatarUrl = avatarUrl;
    //     Debug.Log($"AvatarManager: SetCurrentAvatar called with URL: {avatarUrl}");
    // }

    // A dedicated method for setting the child's URL
    public void SetChildAvatarUrl(string avatarUrl)
    {
        if (string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("SetChildAvatarUrl received a null or empty URL.");
            return;
        }
        ChildAvatarUrl = avatarUrl;
        Debug.Log("Child Avatar URL set: " + avatarUrl);
    }

    // A dedicated method for setting the therapist's URL
    public void SetTherapistAvatarUrl(string avatarUrl)
    {
        if (string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("SetTherapistAvatarUrl received a null or empty URL.");
            return;
        }
        TherapistAvatarUrl = avatarUrl;
        Debug.Log("Therapist Avatar URL set: " + avatarUrl);
    }
}
