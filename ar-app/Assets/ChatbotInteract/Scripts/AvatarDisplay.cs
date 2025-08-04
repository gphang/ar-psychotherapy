// using UnityEngine;
// using AvatarSDK.MetaPerson.Loader;
// using AvatarSDK.MetaPerson.Sample;
// using System.Threading.Tasks;        // for concurrent loading of avatars
// using UnityEngine.UI;                   // for progress text

// public class AvatarDisplay : MonoBehaviour
// {
//     public MetaPersonLoader childMetaPersonLoader;
//     public MetaPersonLoader therapistMetaPersonLoader;

//     [SerializeField] private AnimationManager am;
//     [SerializeField] private FacialSwitcher facialSwitcher;

//     public Text childProgressText;
//     public Text therapistProgressText;
//     // private float childLoadProgress = 0f;
//     // private float therapistLoadProgress = 0f;
//     private readonly WaitForSeconds checkDownloadInterval = new WaitForSeconds(1);

//     async void Start()
//     {
//         // Load concurrently
//         childProgressText.gameObject.SetActive(true);
//         therapistProgressText.gameObject.SetActive(true);
//         Task childTask = LoadChildAvatarAsync();
//         Task therapistTask = LoadTherapistAvatarAsync();
        
//         await Task.WhenAll(childTask, therapistTask);
//         Debug.Log("Both avatars finished loading.");
//         // progressText.gameObject.SetActive(false);
//         childProgressText.gameObject.SetActive(false);
//         therapistProgressText.gameObject.SetActive(false);
//     }

//     // private void UpdateProgress()
//     // {
//     //     progressText = $"Loading child avatar: {childLoadProgress}\nLoading therapist avatar:{therapistLoadProgress}";
//     // }

//     private async Task LoadChildAvatarAsync()
//     {
//         string childAvatarUrl = AvatarManager.Instance.ChildAvatarUrl;
//         if (string.IsNullOrEmpty(childAvatarUrl))
//         {
//             Debug.LogError("Child Avatar URL not found in AvatarManager.");
//             return;
//         }

//         foreach (Transform child in childMetaPersonLoader.transform)
//         {
//             Destroy(child.gameObject);
//         }

//         // bool isLoaded = await childMetaPersonLoader.LoadModelAsync(childAvatarUrl);

//         Task<bool> childDownload = childMetaPersonLoader.LoadModelAsync(childAvatarUrl, progress =>
//         {
//             childProgressText.text = $"Downloading child avatar: {(int)(progress * 100)}%";
//         });
//         while (!childDownload.IsCompleted)
//         {
//             yield return checkDownloadInterval;
//         }

//         if (isLoaded && childMetaPersonLoader.transform.childCount > 0)
//         {
//             childProgressText.gameObject.SetActive(false);

//             GameObject childAvatar = childMetaPersonLoader.transform.GetChild(0).gameObject;

//             // Disable avatar ability to rotate
//             var rotationScript = childAvatar.GetComponent<CameraController>();
//             if (rotationScript != null) rotationScript.enabled = false;

//             // Set animation + expression models
//             if (!childAvatar.TryGetComponent(out Animator modelAnimator)) return;
//             am.BindAnimationSwitcher(modelAnimator.avatar);
//             Destroy(modelAnimator);
//             facialSwitcher.Avatar = childAvatar;
//         }
//         else
//         {
//             Debug.LogError("Failed to load Child Avatar from URL: " + childAvatarUrl);
//         }
//     }

//     private async Task LoadTherapistAvatarAsync()
//     {
//         string therapistAvatarUrl = AvatarManager.Instance.TherapistAvatarUrl;
//         if (string.IsNullOrEmpty(therapistAvatarUrl))
//         {
//             Debug.LogError("Therapist Avatar URL not found in AvatarManager.");
//             return;
//         }

//         foreach (Transform child in therapistMetaPersonLoader.transform)
//         {
//             Destroy(child.gameObject);
//         }

//         // bool isLoaded = await therapistMetaPersonLoader.LoadModelAsync(therapistAvatarUrl);

//         Task<bool> therapistDownload = therapistMetaPersonLoader.LoadModelAsync(therapistAvatarUrl, progress =>
//         {
//             therapistProgressText.text = $"Downloading therapist avatar: {(int)(progress * 100)}%";
//         });
//         while (!therapistDownload.IsCompleted)
//         {
//             yield return checkDownloadInterval;
//         }

//         if (isLoaded && therapistMetaPersonLoader.transform.childCount > 0)
//         {
//             // The position/rotation code is removed here as well.
//             GameObject therapistAvatar = therapistMetaPersonLoader.transform.GetChild(0).gameObject;
//         }
//         else
//         {
//             Debug.LogError("Failed to load Therapist Avatar from URL: " + therapistAvatarUrl);
//         }
//     }
// }







using UnityEngine;
using System.Threading.Tasks;
using AvatarSDK.MetaPerson.Loader;
using AvatarSDK.MetaPerson.Sample;
using TMPro;

public class AvatarDisplay : MonoBehaviour
{
    public RuntimeAnimatorController avatarController;

    
    // Changed back to a single TextMeshProUGUI reference
    [Header("UI Elements")]
    public TextMeshProUGUI progressText;

    [Header("Avatar Loaders")]
    public MetaPersonLoader childMetaPersonLoader;
    public MetaPersonLoader therapistMetaPersonLoader;

    [SerializeField] private AnimationManager am;
    [SerializeField] private FacialSwitcher facialSwitcher;

    private float childLoadProgress = 0f;
    private float therapistLoadProgress = 0f;

    async void Start()
    {
        // Now using the component reference to set the GameObject active
        progressText.gameObject.SetActive(true);
        UpdateProgressText();

        Task childTask = LoadChildAvatarAsync();
        Task therapistTask = LoadTherapistAvatarAsync();

        await Task.WhenAll(childTask, therapistTask);

        Debug.Log("Both avatars finished loading.");
        // Using the component reference to set the GameObject inactive
        progressText.gameObject.SetActive(false);
    }

    private void UpdateProgressText()
    {
        string childProgressString = $"Child: {(int)(childLoadProgress * 100)}%";
        string therapistProgressString = $"Therapist: {(int)(therapistLoadProgress * 100)}%";
        progressText.text = $"Loading Avatars...\n{childProgressString}\n{therapistProgressString}";
    }

    private async Task LoadChildAvatarAsync()
    {
        string childAvatarUrl = AvatarManager.Instance.ChildAvatarUrl;
        if (string.IsNullOrEmpty(childAvatarUrl))
        {
            Debug.LogError("Child Avatar URL not found in AvatarManager.");
            return;
        }

        foreach (Transform child in childMetaPersonLoader.transform)
        {
            Destroy(child.gameObject);
        }

        bool isLoaded = await childMetaPersonLoader.LoadModelAsync(childAvatarUrl, p =>
        {
            childLoadProgress = p;
            UpdateProgressText();
        });

        // if (isLoaded && childMetaPersonLoader.transform.childCount > 0)
        // {
        //     GameObject childAvatar = childMetaPersonLoader.transform.GetChild(0).gameObject;

        //     var rotationScript = childAvatar.GetComponent<CameraController>();
        //     if (rotationScript != null) rotationScript.enabled = false;

        //     // if (!childAvatar.TryGetComponent(out Animator modelAnimator))
        //     // {
        //     //     return;
        //     // }
        //     // am.BindAnimationSwitcher(modelAnimator.avatar);
        //     // Destroy(modelAnimator);
        //     // facialSwitcher.Avatar = childAvatar;

        //     // vvv--------- THIS IS THE FIX ---------vvv
        //     Debug.Log("Initializing animation and facial controllers for Child Avatar.");

        //     // 1. Find the Animator on the new avatar, bind it to the AnimationManager, then destroy it.
        //     //    (Your AnimationManager uses its own system after this point).
        //     if (childAvatar.TryGetComponent(out Animator modelAnimator))
        //     {
        //         am.BindAnimationSwitcher(modelAnimator.avatar);
        //         Destroy(modelAnimator);
        //     }
        //     else
        //     {
        //         Debug.LogError("Animator not found on the loaded child avatar!");
        //     }

        //     // 2. Assign this avatar GameObject to the FacialSwitcher.
        //     facialSwitcher.Avatar = childAvatar;
        //     // ^^^-----------------------------------^^^
        // }


        if (isLoaded && childMetaPersonLoader.transform.childCount > 0)
        {
            GameObject childAvatar = childMetaPersonLoader.transform.GetChild(0).gameObject;

            if (childAvatar.TryGetComponent(out Animator modelAnimator))
            {
                modelAnimator.runtimeAnimatorController = avatarController;
                am.BindAnimator(modelAnimator);
            }
            else
            {
                Debug.LogError("Animator not found on the loaded child avatar!");
            }

            facialSwitcher.Avatar = childAvatar;
        }
        else
        {
            Debug.LogError("Failed to load Child Avatar from URL: " + childAvatarUrl);
        }
    }

    private async Task LoadTherapistAvatarAsync()
    {
        string therapistAvatarUrl = AvatarManager.Instance.TherapistAvatarUrl;
        if (string.IsNullOrEmpty(therapistAvatarUrl))
        {
            Debug.LogError("Therapist Avatar URL not found in AvatarManager.");
            return;
        }

        foreach (Transform child in therapistMetaPersonLoader.transform)
        {
            Destroy(child.gameObject);
        }
        
        bool isLoaded = await therapistMetaPersonLoader.LoadModelAsync(therapistAvatarUrl, p =>
        {
            therapistLoadProgress = p;
            UpdateProgressText();
        });

        if (isLoaded && therapistMetaPersonLoader.transform.childCount > 0)
        {
            GameObject therapistAvatar = therapistMetaPersonLoader.transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("Failed to load Therapist Avatar from URL: " + therapistAvatarUrl);
        }
    }
}