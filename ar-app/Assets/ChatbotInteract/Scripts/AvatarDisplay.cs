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
    [SerializeField] private FacialSwitcher fs_child;
    [SerializeField] private FacialSwitcher fs_therapist;

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

            fs_child.Avatar = childAvatar;
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
            fs_therapist.Avatar = therapistAvatar;
        }
        else
        {
            Debug.LogError("Failed to load Therapist Avatar from URL: " + therapistAvatarUrl);
        }
    }
}