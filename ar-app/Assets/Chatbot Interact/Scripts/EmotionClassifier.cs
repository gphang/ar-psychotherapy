using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

// --- Data Structures for the Emotion API ---
[System.Serializable]
public class EmotionRequest {
    public string text;
}

[System.Serializable]
public class EmotionResponse {
    public string emotion;
}


public class EmotionClassifier : MonoBehaviour
{
    public string emotionClassifierUrl = "http://127.0.0.1:5001/evaluate";

    // Public "entry point" called by OllamaController.cs
    public void RequestEmotion(string prompt)
    {
        StartCoroutine(Coroutine_RequestEmotion(prompt));
    }

    // This private coroutine does the actual web request work
    private IEnumerator Coroutine_RequestEmotion(string prompt)
    {
        EmotionRequest request = new EmotionRequest { text = prompt };
        string jsonToSend = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonToSend);

        using (UnityWebRequest webRequest = new UnityWebRequest(emotionClassifierUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Emotion API Error: " + webRequest.error);
            }
            else
            {
                EmotionResponse response = JsonUtility.FromJson<EmotionResponse>(webRequest.downloadHandler.text);
                // The emotion is now available. You can use it for game logic.
                Debug.Log($"User's detected emotion: {response.emotion}");
                // TODO: You could create an event here to notify other systems of the result.
            }
        }
    }
}