using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI; // For InputField and Button
using TMPro; // For TextMeshPro components
using UnityEngine.Networking; // For UnityWebRequest
using AvatarSDK.MetaPerson.Loader; // Assuming this is your avatar loader

public class HomeGenerateAvatar : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text instruction1;
    public Image home_image;
    public Button newAvatarButton;
    public TMP_Text shareCodeInstruction;
    public TMP_InputField shareCodeInput;
    public Button shareCodeSubmit;
    public TMP_Text loadStatusText;

    public Button continueSelectTherapist;
    public Button returnHome;  // only if users have trouble loading model

    [SerializeField] private MetaPersonLoader mpl;

    // Interval for checking the download status
    private readonly WaitForSeconds checkDownloadInterval = new WaitForSeconds(1);


    void Start()
    {
        SetScene(true);
        newAvatarButton.interactable = true;
        shareCodeSubmit.interactable = true;
    }

    public void OnClick_NewAvatarButton()
    {
        SetScene(false);
        shareCodeSubmit.gameObject.SetActive(false);
        shareCodeInput.gameObject.SetActive(false);
    }
    public void OnClick_continueSelectTherapistButton()
    {
        loadStatusText.gameObject.SetActive(false);
    }
    public void OnClick_returnHome()
    {
        SetScene(true);
        loadStatusText.text = "";
        returnHome.gameObject.SetActive(false);
    }

    // Triggered when share code button is clicked
    public void OnClick_LoadModelButton()
    {
        // Get input from the avatar code input field
        string input = shareCodeInput.text;
        returnHome.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(input))
        {
            shareCodeInstruction.text = "Please enter your avatar's code. If you don't have one, follow the instructions below to create one!";
            return;
        }

        // clear scene
        SetScene(false);
        shareCodeSubmit.interactable = false;
        shareCodeInput.interactable = false;

        if (IsValidUrl(input))
        {
            // Input is a URL; load the model from the URL
            StartCoroutine(LoadModelCoroutine(input));
        }
        else
        {
            // Input is assumed to be a code; retrieve the avatar URL from the backend
            StartCoroutine(RetrieveAvatarData(input));
        }
    }


    ////////////////////////////// LOADING LOGIC //////////////////////////////

    [Serializable]
    public class AvatarDataStructure
    {
        public string code;
        public string mp_code;
        public string avatar_url;
        public string avatar_name;
    }

    // Coroutine to load the model from a URL
    private IEnumerator LoadModelCoroutine(string modelUrl)
    {
        // Update the status text to indicate the start of the download process
        loadStatusText.text = "Downloading avatar...";
        ClearExistingAvatar();

        // Start downloading the model asynchronously and track progress
        Task<bool> downloadTask = mpl.LoadModelAsync(modelUrl, progress =>
        {
            loadStatusText.text = $"Downloading avatar: {(int)(progress * 100)}%";
        });

        while (!downloadTask.IsCompleted) yield return checkDownloadInterval;

        // Check if the download was successful
        if (downloadTask.IsCompletedSuccessfully)
        {
            // Update the status to indicate a successful download
            loadStatusText.text = "Avatar downloaded successfully! Click 'Continue' below to start today's session!";

            // Retrieve the loaded model from the MetaPersonLoader
            GameObject model = mpl.transform.GetChild(0).gameObject;

            // Update the AvatarManager with the current avatar URL
            AvatarManager.Instance.SetChildAvatarUrl(modelUrl);

            // Move on to next scene
            shareCodeSubmit.gameObject.SetActive(false);
            shareCodeInput.gameObject.SetActive(false);
            continueSelectTherapist.gameObject.SetActive(true);
        }
        else
        {
            ErrorLoading();
            yield break;
        }
    }


    // Coroutine to retrieve avatar data from backend using the code
    private IEnumerator RetrieveAvatarData(string code)
    {
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
                loadStatusText.text += $"\nAvatar '{avatarData.avatar_name}' has been successfully retrieved with the code {code}.";
                GameData.UserName = avatarData.avatar_name;
                Debug.Log("Avatar retrieved successfully!");
            }
            else
            {
                ErrorLoading();
                yield break;
            }
        }
        else
        {
            Debug.LogError("Error retrieving avatar data: " + www.error);
            ErrorLoading();
            yield break;
        }
    }


    //////////////////////////// HELPER FUNCTIONS ////////////////////////////

    // Method to clear existing avatar from the scene
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

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private void ErrorLoading()
    {
        loadStatusText.text = "We're sorry, but we couldn't download your avatar.\n\nPlease ensure your code is correct, or return to the home page and create a new one!";
        shareCodeSubmit.interactable = true;
        shareCodeInput.interactable = true;
        returnHome.gameObject.SetActive(true);
        continueSelectTherapist.gameObject.SetActive(false);
    }


    ////////////////////////////// SCENE OBJECTS //////////////////////////////

    private void SetScene(bool active)
    {
        title.gameObject.SetActive(active);
        instruction1.gameObject.SetActive(active);
        home_image.gameObject.SetActive(active);
        newAvatarButton.gameObject.SetActive(active);
        shareCodeInstruction.gameObject.SetActive(active);
    }
}
