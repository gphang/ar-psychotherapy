using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class OllamaRequest{
    public string model;
    public string prompt;
}
[System.Serializable]
public class OllamaResponse
{
    public string id;
    public string @object;
    public long created;
    public string model;
    public string system_fingerprint;
    public OllamaChoice[] choices;
    public OllamaUsage usage;
}
[System.Serializable]
public class OllamaChoice
{
    public string text;
    public int index;
    public string finish_reason;
}
[System.Serializable]
public class OllamaUsage
{
    public int prompt_tokens;
    public int completion_tokens;
    public int total_tokens;
}


public class OllamaController : MonoBehaviour
{
    // public TMP_Text responseTextBox;
    public Button sendButton;
    public TMP_InputField inputField;

    // Chat bubble
    public Transform chatContainer;
    public GameObject userMessagePrefab;
    public GameObject botMessagePrefab;
    public ScrollRect scrollRect;

    void Start()
    {
        // Send button: call SendMessageToOllama when clicked
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendMessageToOllama);
        }
        
        // Allow sending by pressing 'Enter' in the input field
        if (inputField != null)
        {
            inputField.onSubmit.AddListener((_) => SendMessageToOllama());
        }

        string therapistName = GameData.TherapistName;
        string welcomeMessage = $"Nice to meet you! My name is {therapistName}. How are you feeling today?";
        CreateMessageBubble(botMessagePrefab, welcomeMessage);
    }

    public void SendMessageToOllama()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            return;
        }
        StartCoroutine(Send());
    }

    private IEnumerator Send()
    {
        OllamaRequest request = new OllamaRequest();
        request.model = "llama3.2:latest";

        string modifiedPrompt = $"{inputField.text}\n\nLimit your response to 250 characters.";
        request.prompt = modifiedPrompt;

        CreateMessageBubble(userMessagePrefab, inputField.text);  // bubble for user's message
        inputField.text = "";  // clear input field

        string jsonToSend = JsonUtility.ToJson(request);



        using (UnityWebRequest webRequest =
               new UnityWebRequest(
                //    "http://10.74.130.93:11434/v1/completions", "POST"))
                "https://2a45-31-205-214-84.ngrok-free.app/v1/completions", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonToSend);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                OllamaResponse response =
                    JsonUtility
                        .FromJson<OllamaResponse>(responseJson);
                // botMessage = response.choices[0].text;

                CreateMessageBubble(botMessagePrefab, response.choices[0].text);
            }
        }
    }

    private void CreateMessageBubble(GameObject prefab, string message)
    {
        // Instantiate the bubble and make it a child of the container
        GameObject newBubble = Instantiate(prefab, chatContainer);
        
        // Find the TextMeshPro component in the prefab's children and set its text
        TMP_Text messageText = newBubble.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = message;
        } else {
            Debug.LogError("Could not find a TMP_Text component on the message bubble prefab.", newBubble);
        }
        
        // Wait a frame for the UI to update, then scroll to the bottom
        StartCoroutine(ForceScrollDown());
    }

    IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}