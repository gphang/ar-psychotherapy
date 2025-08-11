/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, October 2023
*/

using AvatarSDK.MetaPerson.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// NEW ADDITION: gets names for user and therapist (default = User, Therapist)
public static class GameData
{
    public static string UserName = "User";
	public static string TherapistName = "Therapist";
}

namespace AvatarSDK.MetaPerson.MobileIntegrationSample
{
	public class MobileUnitySampleHandler : MonoBehaviour
	{
		public AccountCredentials credentials;
		public MetaPersonLoader metaPersonLoader;
		public GameObject uniWebViewGameObject;
		public GameObject importControls;
		public Button getAvatarButton;
		public Text progressText;

		public TMP_Text title;
		public TMP_Text instruction1;
		public Image home_image;
		public TMP_Text instruction2;
		public TMP_InputField userNameInputField;
		public Button selectTherapistButton;

		private void Start()
		{
			SetScene(true);
			if (credentials.IsEmpty())
			{
				progressText.text = "Account credentials are not provided!";
				getAvatarButton.interactable = false;
			}
		}

		public void OnGetAvatarButtonClick()
		{
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

		// Set user's name for later use
		public void OnContinueButtonClick()
		{
			string userName = userNameInputField.text.Trim();
			if (!string.IsNullOrEmpty(userName))
			{
				GameData.UserName = userName;
				Debug.Log($"<color=cyan>SCENE 1:</color> Updating UserName from to '{userName}'");
			}
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
				Debug.LogWarningFormat("Start avatar loading from url: {0}", message.Args["url"]);

				webView.Hide();
				getAvatarButton.interactable = false;

				// NEW: hide instructions so can see progressText
				instruction1.gameObject.SetActive(false);

				bool isLoaded = await metaPersonLoader.LoadModelAsync(message.Args["url"], p => progressText.text = string.Format("Downloading avatar: {0}%", (int)(p * 100)));
				if (isLoaded)
				{
					progressText.text = string.Empty;
					// importControls.SetActive(false);

					// EXTRA: adding instruction/continue button to move to select therapist
					importControls.SetActive(true);
					SetScene(false);

					// sets child avatar to be called later in next scene
					AvatarManager.Instance.SetChildAvatarUrl(message.Args["url"]);
				}
				else
				{
					getAvatarButton.interactable = true;
					progressText.text = "Unable to load the model";
					importControls.SetActive(true);
				}
			}
		}


		//////////////////////////// SET SCENE OBJECTS ////////////////////////////

		private void SetScene(bool active)
        {
			title.gameObject.SetActive(active);
			instruction1.gameObject.SetActive(active);
			home_image.gameObject.SetActive(active);
			getAvatarButton.gameObject.SetActive(active);
			instruction2.gameObject.SetActive(!active);
			userNameInputField.gameObject.SetActive(!active);
			selectTherapistButton.gameObject.SetActive(!active);
        }
	}
}
