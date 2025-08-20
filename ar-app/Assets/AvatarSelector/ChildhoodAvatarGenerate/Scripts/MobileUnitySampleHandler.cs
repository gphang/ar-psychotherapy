using AvatarSDK.MetaPerson.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Text;
using System.Threading.Tasks;

using UnityEngine.Networking; // For UnityWebRequest
using System.Security.Cryptography; // For MD5 hashing


namespace AvatarSDK.MetaPerson.MobileIntegrationSample
{
	public class MobileUnitySampleHandler : MonoBehaviour
	{
		[Header("Generating Metaperson")]
		public AccountCredentials credentials;
		public MetaPersonLoader metaPersonLoader;
		public GameObject uniWebViewGameObject;
		public GameObject importControls;
		public Button getAvatarButton;
		public Text progressText;

		[Header("Getting User Name")]
		public TMP_Text instruction2;
		public TMP_InputField userNameInputField;

		[Header("Generating Share Code")]
		public Button generateShareCodeButton;
		public TMP_Text shareCodeInfo;
		public Button selectTherapistButton;
		public Button returnHome;

		private string childAvatarURL = "";

		private void Start()
		{
			credentials.clientId = Environment.GetEnvironmentVariable("METAPERSON_CLIENT_ID");
			credentials.clientSecret = Environment.GetEnvironmentVariable("METAPERSON_CLIENT_SECRET");
			
			if (credentials.IsEmpty())
			{
				progressText.text = "Account credentials are not provided!";
				getAvatarButton.interactable = false;
			}
		}

		public void OnGetAvatarButtonClick()
		{
			instruction2.gameObject.SetActive(true);
			userNameInputField.gameObject.SetActive(true);
			generateShareCodeButton.gameObject.SetActive(true);

			UniWebView uniWebView = uniWebViewGameObject.GetComponent<UniWebView>();
			if (uniWebView == null)
			{
				uniWebView = uniWebViewGameObject.AddComponent<UniWebView>();
				uniWebView.EmbeddedToolbar.Hide();
				uniWebView.Frame = new Rect(0, 0, Screen.width, Screen.height);
			}

			uniWebView.OnPageFinished += OnPageFinished;
			uniWebView.OnMessageReceived += OnMessageReceived;
			uniWebView.Load("https://mobile.metaperson.avatarsdk.com/generator");
			uniWebView.Show();
		}

		private void OnPageFinished(UniWebView webView, int statusCode, string url)
		{
			string javaScriptCode = @"
					{
						function sendConfigurationParams() {
							console.log('sendConfigurationParams - called');

							const CLIENT_ID = '" + credentials.clientId + @"';
							const CLIENT_SECRET = '" + credentials.clientSecret + @"';

							let authenticationMessage = {
								'eventName': 'authenticate',
								'clientId': CLIENT_ID,
								'clientSecret': CLIENT_SECRET
							};
							window.postMessage(authenticationMessage, '*');

							let exportParametersMessage = {
								'eventName': 'set_export_parameters',
								'format': 'glb',
								'lod': 2,
								'textureProfile': '1K.jpg'
							};
							window.postMessage(exportParametersMessage, '*');

							let uiParametersMessage = {
								'eventName': 'set_ui_parameters',
								'isExportButtonVisible' : true,
								'isLoginButtonVisible': true
							};
							window.postMessage(uiParametersMessage, '*');
						}

						function onWindowMessage(evt) {
							if (evt.type === 'message') {
								if (evt.data?.source === 'metaperson_creator') {
									let data = evt.data;
									let evtName = data?.eventName;
									if (evtName === 'unity_loaded' ||
										evtName === 'mobile_loaded') {
										console.log('got mobile_loaded event');
										sendConfigurationParams();
									} else if (evtName === 'model_exported') {
										console.log('got model_exported event');
										const params = new URLSearchParams();
										params.append('url', data.url);
										params.append('gender', data.gender);
										params.append('avatarCode', data.avatarCode);
										window.location.href = 'uniwebview://model_exported?' + params.toString();
									}
								}
							}
						}
						window.addEventListener('message', onWindowMessage);

						sendConfigurationParams();
					}
				";

			webView.AddJavaScript(javaScriptCode, payload => Debug.LogWarningFormat("JS exection result: {0}", payload.resultCode));
		}

		private async void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
		{
			if (message.Path == "model_exported")
			{
				// set URL to use later for storing in db
				childAvatarURL = message.Args["url"];

				Debug.LogWarningFormat("Start avatar loading from url: {0}", childAvatarURL);

				webView.Hide();
				getAvatarButton.interactable = false;

				// NEW: hide instructions so can see progressText
				// instruction1.gameObject.SetActive(false);

				bool isLoaded = await metaPersonLoader.LoadModelAsync(childAvatarURL, p => progressText.text = string.Format("Downloading avatar: {0}%", (int)(p * 100)));
				if (isLoaded)
				{
					progressText.text = string.Empty;
					// importControls.SetActive(false);

					// adding instruction/continue button to move to select therapist
					importControls.SetActive(true);

					// sets child avatar to be called later in next scene
					AvatarManager.Instance.SetChildAvatarUrl(childAvatarURL);
				}
				else
				{
					getAvatarButton.interactable = true;
					progressText.text = "Unable to load the model";
					importControls.SetActive(true);
				}
			}
		}


		//////////////////////// STORING AVATAR LOGIC ////////////////////////
		
		// Serializable class matching the JSON data structure from the backend
		[Serializable]
		public class AvatarDataStructure
		{
			public string code;
			public string mp_code;
			public string avatar_url;
			public string avatar_name;
		}
		

		// Triggered when the "Generate Share Code" button is clicked
		public void OnClick_GenerateShareCodeButton()
		{
			// Get user's name from text input
			string userName = userNameInputField.text.Trim();
			if (!string.IsNullOrEmpty(userName))
			{
				GameData.UserName = userName;
				Debug.Log($"<color=cyan>SCENE 1:</color> Updating UserName from to '{userName}'");
			}
			else userName = "User";

			Debug.Log($"Retrieved Avatar URL: {childAvatarURL}");
			shareCodeInfo.text = "";

			// Generate a unique 8-character code
			string uniqueCode = GenerateUniqueCode(childAvatarURL);

			// Store the code, avatar URL, and avatar name in the database
			StartCoroutine(StoreAvatarData(uniqueCode, childAvatarURL, userName));

			TransitionScene();
		}

		// Method to generate a unique 8-character code based on input string
		private string GenerateUniqueCode(string input)
		{
			using (MD5 md5 = MD5.Create())
			{
				byte[] inputBytes = Encoding.ASCII.GetBytes(input + DateTime.Now.Ticks);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert hash to hexadecimal string and take first 8 characters
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < 4; i++) // 4 bytes * 2 hex chars = 8 characters
				{
					sb.Append(hashBytes[i].ToString("X2"));
				}
				return sb.ToString();
			}
		}

		// Method to extract metaperson code from avatar_url
		private string ExtractMetapersonCode(string avatarUrl)
		{
			try
			{
				Uri uri = new Uri(avatarUrl);
				string[] segments = uri.AbsolutePath.Split('/');
				int avatarsIndex = Array.IndexOf(segments, "avatars");
				if (avatarsIndex != -1 && segments.Length > avatarsIndex + 1)
				{
					return segments[avatarsIndex + 1];
				}
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}


		// Coroutine to store avatar data in the database
		private IEnumerator StoreAvatarData(string code, string avatarUrl, string avatarName)
		{
			// Extract mp_code from avatarUrl
			string metapersonCode = ExtractMetapersonCode(avatarUrl);

			if (string.IsNullOrEmpty(metapersonCode))
			{
				Debug.Log("Invalid avatar URL. Cannot extract Metaperson code.");
				shareCodeInfo.text = "We're sorry, but we couldn't extract your avatar's link. Please try again.";
				selectTherapistButton.gameObject.SetActive(false);
				returnHome.gameObject.SetActive(true);
				yield break;
			}

			// Create an instance of the AvatarDataToStore class
			AvatarDataStructure data = new AvatarDataStructure
			{
				code = code,
				mp_code = metapersonCode,
				avatar_url = avatarUrl,
				avatar_name = avatarName
			};

			string jsonData = JsonUtility.ToJson(data);

			// Log the JSON data being sent
			Debug.Log("JSON Data being sent: " + jsonData);

			// URL of your backend API endpoint for storing data
			string storeUrl = "https://avatar-backend.xy2119.workers.dev/storeAvatarData";

			UnityWebRequest storeRequest = new UnityWebRequest(storeUrl, "POST");
			byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
			storeRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
			storeRequest.downloadHandler = new DownloadHandlerBuffer();
			storeRequest.SetRequestHeader("Content-Type", "application/json");

			// Send the POST request and wait for response
			yield return storeRequest.SendWebRequest();

			if (storeRequest.result == UnityWebRequest.Result.Success)
			{
				Debug.Log("Avatar data stored successfully.");

				// Display the unique code to the user
				shareCodeInfo.text = $"Your unique code: {code}\n\nPlease save this to load your avatar directly in the future :)";
			}
			else
			{
				Debug.LogError("Error storing avatar data: " + storeRequest.error);
				Debug.LogError("Response: " + storeRequest.downloadHandler.text);
				shareCodeInfo.text += "\nWe're sorry, but we couldn't save your avatar data. Please try again.";
				selectTherapistButton.gameObject.SetActive(false);
				returnHome.gameObject.SetActive(true);
			}    
		}


		////////////////////////// SET SCENE OBJECTS //////////////////////////

		private void TransitionScene()
		{
			instruction2.gameObject.SetActive(false);
			userNameInputField.gameObject.SetActive(false);
			generateShareCodeButton.gameObject.SetActive(false);
			selectTherapistButton.gameObject.SetActive(true);
			shareCodeInfo.gameObject.SetActive(true);
		}
	}
}
