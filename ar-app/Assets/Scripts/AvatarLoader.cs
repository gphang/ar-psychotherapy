using System.Collections;
using System.Collections.Generic;
using AvatarSDK.MetaPerson.Loader;
using UnityEngine;
using UnityEngine.UI;

public class AvatarLoader : MonoBehaviour
{
    public GameObject avatarCardPrefab; // A prefab for displaying each avatar
    public Transform avatarGrid; // The parent transform for the avatar cards
    public Button homeButton;

    private readonly string[] avatarUrls = {
        "https://api.metaperson.avatarsdk.com/avatars/your_avatar_url_1.glb",
        "https://api.metaperson.avatarsdk.com/avatars/your_avatar_url_2.glb",
        "https://api.metaperson.avatarsdk.com/avatars/your_avatar_url_3.glb",
        // Add more avatar URLs as needed
    };

    void Start()
    {
        homeButton.onClick.AddListener(() => SceneManager.LoadScene("Homepage"));
        LoadAvatars();
    }

    private void LoadAvatars()
    {
        foreach (string url in avatarUrls)
        {
            var avatarLoader = new MetaPersonLoader();
            avatarLoader.LoadFromFile(url, OnAvatarLoaded);
        }
    }

    private void OnAvatarLoaded(GameObject avatar, MetaPersonLoader.Error error)
    {
        if (error == null)
        {
            GameObject card = Instantiate(avatarCardPrefab, avatarGrid);
            // You'll need to set up the card to display the avatar
            // This might involve rendering the avatar to a texture or using a snapshot
            // For simplicity, we'll just name the card for now
            card.name = avatar.name;

            // Add a button to the card to select this avatar
            Button selectButton = card.GetComponentInChildren<Button>();
            selectButton.onClick.AddListener(() => SelectAvatar(avatar));
        }
        else
        {
            Debug.LogError($"Failed to load avatar: {error.message}");
        }
    }

    private void SelectAvatar(GameObject selectedAvatar)
    {
        // Store the selected avatar's information (e.g., in PlayerPrefs)
        PlayerPrefs.SetString("SelectedAvatarURL", selectedAvatar.name); // Using name as a proxy for URL here
        PlayerPrefs.Save();

        // Load your main AR scene
        SceneManager.LoadScene("ARScene");
    }
}