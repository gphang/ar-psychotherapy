using UnityEngine;
using AvatarSDK.MetaPerson.Loader;
using AvatarSDK.MetaPerson.Sample;
using System.Threading.Tasks;        // for concurrent loading of avatars

public class AvatarInteract : MonoBehaviour
{
    public MetaPersonLoader childMetaPersonLoader;
    public MetaPersonLoader therapistMetaPersonLoader;

    async void Start()
    {
        // Load concurrently
        Task childTask = LoadChildAvatarAsync();
        Task therapistTask = LoadTherapistAvatarAsync();
        
        await Task.WhenAll(childTask, therapistTask);
        Debug.Log("Both avatars finished loading.");
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

        bool isLoaded = await childMetaPersonLoader.LoadModelAsync(childAvatarUrl);
        if (isLoaded && childMetaPersonLoader.transform.childCount > 0)
        {
            GameObject childAvatar = childMetaPersonLoader.transform.GetChild(0).gameObject;

            // Disable avatar ability to rotate
            var rotationScript = childAvatar.GetComponent<CameraController>();
            if (rotationScript != null) rotationScript.enabled = false;
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

        bool isLoaded = await therapistMetaPersonLoader.LoadModelAsync(therapistAvatarUrl);
        if (isLoaded && therapistMetaPersonLoader.transform.childCount > 0)
        {
            // The position/rotation code is removed here as well.
            GameObject therapistAvatar = therapistMetaPersonLoader.transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("Failed to load Therapist Avatar from URL: " + therapistAvatarUrl);
        }
    }
}