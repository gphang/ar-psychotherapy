using AvatarSDK.MetaPerson.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AvatarSDK.MetaPerson.Sample
{
	public class MetaPersonSample : MonoBehaviour
	{
		public Button loadAvatarButton;
		public Text progressText;
		public TMP_Text titleText;
		public TMP_Text descriptionText;
		public MetaPersonLoader metaPersonLoader;

		public Button previousAvatarButton;
		public Button nextAvatarButton;
		public Button continueButton;

		public TMP_Text nameInstruction;
		public TMP_InputField inputName;

		// private GameObject currentLoadedAvatar;

		public List<string> avatarURLs = new List<string>()
		{
			"https://metaperson.avatarsdk.com/avatars/22816b47-c188-41d6-9069-efbcb69f9f6e/model.glb",
			"https://metaperson.avatarsdk.com/avatars/c458d7cf-0a98-4dbb-9519-74c367ff2fcd/model.glb"
		};
		private int avatarIndex = 0;  // tracks which avatar is currently loaded


		void Start()
		{
            // if (previousAvatarButton != null)
            //     previousAvatarButton.onClick.AddListener(OnPreviousAvatarButtonClick);
            // if (nextAvatarButton != null)
            //     nextAvatarButton.onClick.AddListener(OnNextAvatarButtonClick);
			// if (continueButton != null)
            //     continueButton.onClick.AddListener(OnContinueButtonClick);

			SetAvatarNavigationButtonsActive(false);
			SetNameButtonsActive(false);
			continueButton.gameObject.SetActive(false);
		}


		public void OnLoadAvatarButtonClick()
		{
			LoadAvatarByIndex(0);
		}

		// public void SetModelUrl(string modelUrl)
		// {
		// 	this.modelUrl = modelUrl;
		// }

		// private async void LoadAvatar()
		// {
		// 	progressText.text = string.Empty;
		// 	loadAvatarButton.interactable = false;
		// 	bool isModelLoaded = await metaPersonLoader.LoadModelAsync(modelUrl, p => progressText.text = string.Format("Downloading avatar: {0}%", (int)(p * 100)));
		// 	if (isModelLoaded)
		// 	{
		// 		loadAvatarButton.gameObject.SetActive(false);
		// 		progressText.gameObject.SetActive(false);
		// 		titleText.gameObject.SetActive(false);
		// 		descriptionText.gameObject.SetActive(false);
		// 	}
		// 	else
		// 		progressText.text = "Unable to load the model";
		// 	loadAvatarButton.interactable = true;
		// }

		private async void LoadAvatarByIndex(int index)
		{
			// ensure progress text is visible and cleared
			progressText.gameObject.SetActive(true);
			progressText.text = string.Empty;

			// disable all interactive ui elements during load
			loadAvatarButton.interactable = false;
			previousAvatarButton.interactable = false;
			nextAvatarButton.interactable = false;
			SetAvatarNavigationButtonsActive(false);

			// destroy previous avatar loaded if exists
			// if (currentLoadedAvatar != null)
			// {
			// 	Destroy(currentLoadedAvatar);
			// 	currentLoadedAvatar = null;
			// }
			
			string modelUrl = avatarURLs[index];
			bool isModelLoaded = await metaPersonLoader.LoadModelAsync(modelUrl, p => progressText.text = string.Format("Downloading avatar: {0}%", (int)(p * 100)));

			if (isModelLoaded)
			{
				// currentLoadedAvatar = GameObject.Find("MetaPersonModel");

				loadAvatarButton.gameObject.SetActive(false);
				progressText.gameObject.SetActive(false);
				titleText.gameObject.SetActive(false);
				descriptionText.gameObject.SetActive(false);

				SetAvatarNavigationButtonsActive(true);
			}
			else
				progressText.text = "Unable to load the model";
			loadAvatarButton.interactable = true;
			continueButton.gameObject.SetActive(true);
		}

		private void SetAvatarNavigationButtonsActive(bool active)
        {
            if (previousAvatarButton != null)
                previousAvatarButton.gameObject.SetActive(active);
            if (nextAvatarButton != null)
                nextAvatarButton.gameObject.SetActive(active);
        }
		private void SetNameButtonsActive(bool active)
		{
			if (nameInstruction != null)
				nameInstruction.gameObject.SetActive(active);
			if (inputName != null)
				inputName.gameObject.SetActive(active);
		}


        public void OnPreviousAvatarButtonClick()
        {
            if (avatarURLs.Count == 0) return;

            avatarIndex--;
            if (avatarIndex < 0)
            {
                avatarIndex = avatarURLs.Count - 1;  // Wrap around to the last avatar
            }

			GameObject currentLoadedAvatar = GameObject.Find("MetaPersonModel");
			Destroy(currentLoadedAvatar);
			
            LoadAvatarByIndex(avatarIndex);
        }
        public void OnNextAvatarButtonClick()
        {
            if (avatarURLs.Count == 0) return;

            avatarIndex++;
            if (avatarIndex >= avatarURLs.Count)
            {
                avatarIndex = 0;  // Wrap around to the first avatar
            }

			GameObject currentLoadedAvatar = GameObject.Find("MetaPersonModel");
			Destroy(currentLoadedAvatar);

            LoadAvatarByIndex(avatarIndex);
        }

		public void OnContinueButtonClick()
		{
			SetAvatarNavigationButtonsActive(false);
			continueButton.gameObject.SetActive(false);
			SetNameButtonsActive(true);
		}

		// private void UpdateNavigationButtonState()
        // {
        //     if (avatarUrls.Count <= 1)
        //     {
        //         // If 0 or 1 avatar, disable both navigation buttons
        //         if (previousAvatarButton != null) previousAvatarButton.interactable = false;
        //         if (nextAvatarButton != null) nextAvatarButton.interactable = false;
        //     }
        //     else
        //     {
        //         // Both are interactable if more than one avatar, wrapping takes care of ends
        //         if (previousAvatarButton != null) previousAvatarButton.interactable = true;
        //         if (nextAvatarButton != null) nextAvatarButton.interactable = true;
        //     }
        // }
	}
}
