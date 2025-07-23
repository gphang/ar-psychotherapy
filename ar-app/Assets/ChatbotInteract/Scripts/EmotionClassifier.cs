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

    // To announce emotion state of user
    public static event System.Action<string> OnEmotionClassified;

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
                Debug.Log($"User's detected emotion: {response.emotion}");

                // Announce user's emotional state to listening script (EmotionController.cs)
                OnEmotionClassified?.Invoke(response.emotion);


                // TODO: change logic to tailor to specific emotion class
                // Emotion states from classifier: fear, love, instability, disgust, disappointment, shame, anger, jealous, sadness, envy, joy, guilt
                // current animation states: idle, dance, happy, cry, angry, surprise, fearful, disgusted
                // current expression states: idle, happy, sad, angry, fearful, disgusted, surprise
                // if (text.Contains("joy") || text.Contains("love"))
                //     return "happy";
                // if (text.Contains("fear") || text.Contains("instability") || text.Contains("furious"))
                //     return "fearful";
                // if (text.Contains("sad") || text.Contains("cry") || text.Contains("unhappy"))
                //     return "Sad";
                // if (text.Contains("angry") || text.Contains("mad") || text.Contains("furious"))
                //     return "Angry";
                
            }
        }
    }
}