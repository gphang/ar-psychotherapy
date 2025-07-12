using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI; // For InputField and Button
using TMPro;
using UnityEngine.Networking; // For UnityWebRequest
using System.Security.Cryptography; // For MD5 hashing
using AvatarSDK.MetaPerson.Loader;

namespace AvatarSDK.MetaPerson.Sample
{
    public class TherapistAvatarSelect : MonoBehaviour
    {
        // objects from previous scenes
        public Button loadAvatarButton;
        public MetaPersonLoader childMetaPersonLoader;
        public TMP_Text instruction2;

        public Text progressText;
        [SerializeField] private MetaPersonLoader mpl;

        public Button previousAvatarButton;
        public Button nextAvatarButton;
        public TMP_Text instruction3;
        public Button continueButton;

        public TMP_Text nameInstruction;
        public TMP_InputField inputName;

        public List<string> avatarURLs = new List<string>()
        {
            "16568EC8",
            "731634B1"
        };
        private int avatarIndex = 0;  // tracks which avatar is currently loaded

        // Interval for checking the download status
        private readonly WaitForSeconds checkDownloadInterval = new WaitForSeconds(1);


        void Start()
        {
            previousAvatarButton.onClick.AddListener(OnPreviousAvatarButtonClick);
            nextAvatarButton.onClick.AddListener(OnNextAvatarButtonClick);
            continueButton.onClick.AddListener(OnContinueButtonClick);

            SetAvatarNavigationButtonsActive(false);
            SetNameButtonsActive(false);
            continueButton.gameObject.SetActive(false);
        }


        /////////////// BUTTON CLICK FUNCTIONS ///////////////

        public void OnLoadAvatarButtonClick()
        {
            SetInitialScene();
            StartCoroutine(RetrieveAvatarData(0));
        }

        public void OnPreviousAvatarButtonClick()
        {
            avatarIndex--;
            if (avatarIndex < 0)
            {
                avatarIndex = avatarURLs.Count - 1;  // Wrap around to the last avatar
            }

            StartCoroutine(RetrieveAvatarData(avatarIndex));
        }
        public void OnNextAvatarButtonClick()
        {
            avatarIndex++;
            if (avatarIndex >= avatarURLs.Count)
            {
                avatarIndex = 0;  // Wrap around to the first avatar
            }

            StartCoroutine(RetrieveAvatarData(avatarIndex));
        }

        public void OnContinueButtonClick()
        {
            SetAvatarNavigationButtonsActive(false);
            continueButton.gameObject.SetActive(false);
            SetNameButtonsActive(true);
        }


        /////////////// AVATAR LOADING FUNCTIONS ///////////////	

        private IEnumerator RetrieveAvatarData(int avatar_idx)
        {
            string code = avatarURLs[avatar_idx];
            // URL of your backend API endpoint
            string url = $"https://avatar-backend.xy2119.workers.dev/getAvatarData?code={code}";

            UnityWebRequest www = UnityWebRequest.Get(url);

            // Send the request and wait for response
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Parse JSON data
                string jsonResponse = www.downloadHandler.text;
                AvatarDataStructure avatarData = JsonUtility.FromJson<AvatarDataStructure>(jsonResponse);

                if (avatarData != null)
                {
                    // Log the JSON data being retrieved
                    Debug.Log("JSON Data being retrieved: " + avatarData.avatar_name + ", " + avatarData.avatar_url);

                    // Use the retrieved avatar URL to load the model
                    StartCoroutine(LoadModelCoroutine(avatarData.avatar_url));
                    // progressText.text += $"\nAvatar has been successfully retrieved with the code {code}.";
                    Debug.Log("Avatar retrieved successfully!");

                    // avatarNameInputField.text = "";
                    // avatarCodeInputField.text = "";
                }
                // else
                // {
                //     statusText.text = "We're sorry, but we couldn't parse avatar data. Please ensure the code is correct and try again.";
                // }
            }
            else
            {
                Debug.LogError("Error retrieving avatar data: " + www.error);
                // statusText.text = "We're sorry, but we couldn't retrieve the avatar. Please ensure the code is correct and try again.";
            }
        }

        // Coroutine to load the model from a URL
        private IEnumerator LoadModelCoroutine(string modelUrl)
        {
            // Remove existing avatar model if it exists
            ClearExistingAvatar();

            progressText.gameObject.SetActive(true);

            // Start downloading the model asynchronously and track progress
            Task<bool> downloadTask = mpl.LoadModelAsync(modelUrl, progress =>
            {
                // Update the download progress percentage
                progressText.text = $"Downloading avatar: {(int)(progress * 100)}%";
            });

            // Wait until the download task is complete
            while (!downloadTask.IsCompleted)
            {
                yield return checkDownloadInterval; // Wait for the specified interval
            }

            if (downloadTask.IsCompletedSuccessfully)
            {
                // Update the status to indicate a successful download
                // statusText.text = "Avatar downloaded successfully! You can now explore various animations and view your avatar in different scenes.";

                progressText.gameObject.SetActive(false);

                // Retrieve the loaded model from the MetaPersonLoader
                GameObject model = mpl.transform.GetChild(0).gameObject;

                // Update the AvatarManager with the current avatar URL
                // AvatarManager.Instance.SetCurrentAvatar(modelUrl);
                AvatarManager.Instance.SetTherapistAvatarUrl(modelUrl);


                // Check if the model has an Animator component
                if (!model.TryGetComponent(out Animator modelAnimator))
                {
                    Debug.LogError("Model does not have an Animator component.");
                    yield break; // Exit the coroutine
                }

                // Bind the model's Animator avatar to the AnimationManager
                // am.BindAnimationSwitcher(modelAnimator.avatar);
                // Destroy(modelAnimator); // Remove the Animator to avoid conflicts

                // Assign the loaded model to the FacialSwitcher for facial expressions
                // facialSwitcher.Avatar = model;
            }
            else
            {
                // Display error if the download failed
                // statusText.text = "We're sorry, but we couldn't download the avatar. Please ensure the code is correct and try again.";

            }
        }

        private void ClearExistingAvatar()
        {
            // Check if MetaPersonLoader has any child objects (avatar models)
            if (mpl.transform.childCount > 0)
            {
                // Destroy all child objects (existing avatars)
                for (int i = mpl.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(mpl.transform.GetChild(i).gameObject);
                }
                Debug.Log("Existing avatar model removed.");
            }
        }

        
        // Serializable class matching the JSON data structure from the backend
        [Serializable]
        public class AvatarDataStructure
        {
            public string code;
            public string mp_code;
            public string avatar_url;
            public string avatar_name;
        }


        //////////////////////////// SET SCENE OBJECTS ////////////////////////////

        private void SetAvatarNavigationButtonsActive(bool active)
        {
            instruction3.gameObject.SetActive(active);
            previousAvatarButton.gameObject.SetActive(active);
            nextAvatarButton.gameObject.SetActive(active);
        }

        private void SetNameButtonsActive(bool active)
        {
            nameInstruction.gameObject.SetActive(active);
            inputName.gameObject.SetActive(active);
        }

        private void SetInitialScene()
        {
            loadAvatarButton.gameObject.SetActive(false);
            childMetaPersonLoader.gameObject.SetActive(false);
            instruction2.gameObject.SetActive(false);

            progressText.gameObject.SetActive(false);
            SetAvatarNavigationButtonsActive(true);
            continueButton.gameObject.SetActive(true);
        }
    }
}