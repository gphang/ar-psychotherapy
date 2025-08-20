//////////////////////// CLOUD DEPLOYMENT: USING GEMINI API ////////////////////////

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.SceneManagement;


// for Gemini API (chatbot)
[System.Serializable]
public class GeminiRequest { public Content[] contents; }
[System.Serializable]
public class Content { public Part[] parts; }
[System.Serializable]
public class Part { public string text; }
[System.Serializable]
public class GeminiResponse { public Candidate[] candidates; }
[System.Serializable]
public class Candidate { public Content content; }


// for Google Speech-To-Text API
[System.Serializable]
public class SpeechRequest
{
    public RecognitionConfig config;
    public RecognitionAudio audio;
}
[System.Serializable]
public class RecognitionConfig
{
    public string encoding = "LINEAR16";
    public int sampleRateHertz;
    public string languageCode = "en-US";
}
[System.Serializable]
public class RecognitionAudio
{
    public string content;
}

[System.Serializable]
public class SpeechResponse
{
    public SpeechRecognitionResult[] results;
}
[System.Serializable]
public class SpeechRecognitionResult
{
    public SpeechRecognitionAlternative[] alternatives;
}
[System.Serializable]
public class SpeechRecognitionAlternative
{
    public string transcript;
    public float confidence;
}


// for Google Text-To-Speech API
[System.Serializable]
public class TextToSpeechRequest
{
    public SynthesisInput input;
    public VoiceSelectionParams voice;
    public AudioConfig audioConfig;
}
[System.Serializable]
public class SynthesisInput { public string text; }
[System.Serializable]
public class VoiceSelectionParams 
{ 
    public string languageCode = "en-US"; 
    public string ssmlGender;
}
[System.Serializable]
public class AudioConfig { public string audioEncoding = "LINEAR16"; }
[System.Serializable]
public class TextToSpeechResponse { public string audioContent; }



public class ChatbotController : MonoBehaviour
{
    public string geminiApiKey; 
    private string geminiModel = "gemini-2.5-flash";
    private string geminiUrl;
    private string speechToTextUrl;
    private string textToSpeechUrl;

    [Header("Scene References")]
    public EmotionClassifier emotionClassifier;
    public Button sendButton;
    public TMP_InputField inputField;
    public Button recordButton;        // to enable STT
    public AudioSource audioSource;

    [Header("Chat UI")]
    public Transform chatContainer;
    public GameObject userMessagePrefab;
    public GameObject botMessagePrefab;
    public ScrollRect scrollRect;

    // for microphone recording
    private AudioClip recording;
    private string microphoneDeviceName;
    private bool isRecording = false;
    private GameObject listeningIndicatorBubble;
    
    void Awake()
    {
        // get API key from .env file
        geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrEmpty(geminiApiKey))
        {
            Debug.LogError("GEMINI_API_KEY not found. Make sure it is set in your .env file.");
        }
    }

    void Start()
    {
        geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent";
        speechToTextUrl = $"https://speech.googleapis.com/v1/speech:recognize?key={geminiApiKey}";
        textToSpeechUrl = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={geminiApiKey}";

        // for text input
        if (sendButton != null) sendButton.onClick.AddListener(SendMessageToGemini);
        if (inputField != null) inputField.onSubmit.AddListener((_) => SendMessageToGemini());

        // for voice input
        if (recordButton != null)
        {
            // triggers for press + release record button
            EventTrigger trigger = recordButton.gameObject.AddComponent<EventTrigger>();
            
            // press
            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((data) => { StartRecording(); });
            trigger.triggers.Add(pointerDown);

            // release
            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((data) => { StopRecording(); });
            trigger.triggers.Add(pointerUp);
        }

        // initialise microphone
        if (Microphone.devices.Length > 0) microphoneDeviceName = Microphone.devices[0];
        else Debug.LogError("No microphone found.");

        string welcomeMessage = $"Hi {GameData.UserName}, I'm {GameData.TherapistName}! Unmute your phone and turn up your volume to hear me, and hold the mic icon (top right) to talk instead of type. Tap the cube icon below anytime to see your childhood avatar come to life.\n\nSo, how are you feeling today?";

        // Call Text-To-Speech function
        StartCoroutine(SynthesizeAndPlaySpeech(welcomeMessage));
        CreateMessageBubble(botMessagePrefab, welcomeMessage);
    }

    // logic purely for text from input field
    public void SendMessageToGemini()
    {
        // Stop TTS audio when users send a message
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();

        string userText = inputField.text;
        if (string.IsNullOrWhiteSpace(userText) || string.IsNullOrWhiteSpace(geminiApiKey)) return;
        
        inputField.text = "";
        ProcessUserMessage(userText);
    }

    // handles logic for text & voice
    private void ProcessUserMessage(string userText)
    {
        CreateMessageBubble(userMessagePrefab, userText);
        if (emotionClassifier != null) emotionClassifier.RequestEmotion(userText);
        StartCoroutine(Send(userText)); 
    }

    // sends user's message to Gemini
    private IEnumerator Send(string promptText)
    {
        var request = new GeminiRequest
        {
            contents = new Content[]
            {
                new Content
                {
                    parts = new Part[]
                    {
                        new Part { text = $"You are an empathetic psychotherapist. The user's name is {GameData.UserName}. Recommend psychotherapy exercises based on user's message, keep your response concise and under 60 words. USER: {promptText}" }
                    }
                }
            }
        };

        string jsonBody = JsonUtility.ToJson(request);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(geminiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("X-goog-api-key", geminiApiKey);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Gemini Error: {webRequest.error}\nResponse: {webRequest.downloadHandler.text}");
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(responseJson);

                if (response.candidates != null && response.candidates.Length > 0)
                {
                    string botResponseText = response.candidates[0].content.parts[0].text;
                    CreateMessageBubble(botMessagePrefab, botResponseText.Trim());

                    // Call Text-To-Speech function
                    StartCoroutine(SynthesizeAndPlaySpeech(botResponseText));
                }
                else
                {
                    Debug.LogError("Invalid response from Gemini API: " + responseJson);
                }
            }
        }
    }


    ////////////////////// SPEECH-TO-TEXT API FUNCTIONS //////////////////////

    public void StartRecording()
    {
        // Stop TTS audio when users start recording message
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();

        if (isRecording || microphoneDeviceName == null) return;
        
        Debug.Log("Recording started...");
        isRecording = true;

        // Start recording with a max length of 30 seconds
        recording = Microphone.Start(microphoneDeviceName, false, 30, 44100);

        listeningIndicatorBubble = CreateMessageBubble(userMessagePrefab, "I'm listening...");
    }

    public void StopRecording()
    {
        if (!isRecording) return;
        
        Debug.Log("Recording stopped...");
        Microphone.End(microphoneDeviceName);
        isRecording = false;

        // process audio
        if (recording != null) StartCoroutine(SendAudioToSpeechAPI(recording));
    }

    private IEnumerator SendAudioToSpeechAPI(AudioClip clip)
    {
        // Convert audio clip to WAV byte array and then to Base64
        byte[] wavData = AudioToWav(clip);
        string base64Audio = Convert.ToBase64String(wavData);

        // Create the request payload
        var request = new SpeechRequest
        {
            config = new RecognitionConfig { sampleRateHertz = clip.frequency, },
            audio = new RecognitionAudio { content = base64Audio }
        };

        string jsonBody = JsonUtility.ToJson(request);

        using (UnityWebRequest webRequest = new UnityWebRequest(speechToTextUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            // Destroy bubble showing users system is listening
            if (listeningIndicatorBubble != null) Destroy(listeningIndicatorBubble);

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Speech-to-Text Error: {webRequest.error}\nResponse: {webRequest.downloadHandler.text}");
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                SpeechResponse response = JsonUtility.FromJson<SpeechResponse>(responseJson);

                if (response.results != null && response.results.Length > 0)
                {
                    // Get the most likely transcript
                    string transcript = response.results[0].alternatives[0].transcript;
                    Debug.Log("Transcript: " + transcript);
                    
                    // Send transcribed message directly to the chatbot
                    ProcessUserMessage(transcript);
                }
                else
                {
                    Debug.LogWarning("Speech-to-Text: No transcript returned.");
                }
            }
        }
    }


    ////////////////////// TEXT-TO-SPEECH API FUNCTIONS //////////////////////

    private IEnumerator SynthesizeAndPlaySpeech(string text)
    {
        // Create the request payload with avatar's gender
        var request = new TextToSpeechRequest
        {
            input = new SynthesisInput { text = text },
            voice = new VoiceSelectionParams { ssmlGender = GameData.TherapistGender },
            audioConfig = new AudioConfig()
        };

        string jsonBody = JsonUtility.ToJson(request);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(textToSpeechUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Text-to-Speech Error: {webRequest.error}\nResponse: {webRequest.downloadHandler.text}");
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                TextToSpeechResponse response = JsonUtility.FromJson<TextToSpeechResponse>(responseJson);
                
                byte[] audioBytes = Convert.FromBase64String(response.audioContent);
                AudioClip audioClip = WavUtility.ToAudioClip(audioBytes);

                if (audioClip != null && audioSource != null)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
            }
        }
    }

    
    //////////////////////////// UTILITY FUNCTIONS ////////////////////////////

    private GameObject CreateMessageBubble(GameObject prefab, string message)
    {
        GameObject newBubble = Instantiate(prefab, chatContainer);
        TMP_Text messageText = newBubble.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = message;
        }
        StartCoroutine(ForceScrollDown());

        return newBubble;
    }

    IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    byte[] AudioToWav(AudioClip clip)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                // WAV header
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + clip.samples * 2);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((ushort)1); // Audio format 1 (PCM)
                writer.Write((ushort)clip.channels);
                writer.Write(clip.frequency);
                writer.Write(clip.frequency * clip.channels * 2); // Byte rate
                writer.Write((ushort)(clip.channels * 2)); // Block align
                writer.Write((ushort)16); // Bits per sample
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(clip.samples * clip.channels * 2);

                // Audio data
                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);
                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * 32767.0f));
                }
            }
            return memoryStream.ToArray();
        }
    }
}








//////////////////////// LOCAL DEPLOYMENT: USING OLLAMA ////////////////////////

// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections;
// using System.Linq;
// using System.Text;
// using UnityEngine.UI;
// using TMPro;

// [System.Serializable]
// public class OllamaRequest{
//     public string model;
//     public string prompt;
// }
// [System.Serializable]
// public class OllamaResponse
// {
//     public string id;
//     public string @object;
//     public long created;
//     public string model;
//     public string system_fingerprint;
//     public OllamaChoice[] choices;
//     public OllamaUsage usage;
// }
// [System.Serializable]
// public class OllamaChoice
// {
//     public string text;
//     public int index;
//     public string finish_reason;
// }
// [System.Serializable]
// public class OllamaUsage
// {
//     public int prompt_tokens;
//     public int completion_tokens;
//     public int total_tokens;
// }


// public class ChatbotController : MonoBehaviour
// {
//     public string ollamaUrl = "https://798c-31-205-214-84.ngrok-free.app/v1/completions";
//     public EmotionClassifier emotionClassifier;

//     // public TMP_Text responseTextBox;
//     public Button sendButton;
//     public TMP_InputField inputField;

//     // Chat bubble
//     public Transform chatContainer;
//     public GameObject userMessagePrefab;
//     public GameObject botMessagePrefab;
//     public ScrollRect scrollRect;

//     void Start()
//     {
//         // Send button: call SendMessageToOllama when clicked
//         if (sendButton != null)
//         {
//             sendButton.onClick.AddListener(SendMessageToOllama);
//         }
        
//         // Allow sending by pressing 'Enter' in the input field
//         if (inputField != null)
//         {
//             inputField.onSubmit.AddListener((_) => SendMessageToOllama());
//         }

//         string therapistName = GameData.TherapistName;
//         string userName = GameData.UserName;
//         string welcomeMessage = $"Nice to meet you, {userName}! My name is {therapistName}. How are you feeling today?";
//         CreateMessageBubble(botMessagePrefab, welcomeMessage);
//     }

//     public void SendMessageToOllama()
//     {
//         // Debug.Log("1. SendMessageToOllama function was called!");

//         string userText = inputField.text;

//         if (string.IsNullOrWhiteSpace(userText))
//         {
//             return;
//         }
//         StartCoroutine(Send());

//         // Get user's emotional state from EmotionClassifier
//         if (emotionClassifier != null)
//         {
//             emotionClassifier.RequestEmotion(userText);
//         }
//         else
//         {
//             Debug.LogWarning("EmotionClassifier reference not set in the Inspector.");
//         }
//     }

//     private IEnumerator Send()
//     {
//         // Debug.Log("2. 'Send' coroutine has started, preparing web request...");

//         OllamaRequest request = new OllamaRequest();
//         request.model = "llama3.2:latest";

//         string modifiedPrompt = $"You are an empathetic psychotherapist.\n{inputField.text}\nLimit your response to 250 characters.";
//         request.prompt = modifiedPrompt;

//         CreateMessageBubble(userMessagePrefab, inputField.text);  // bubble for user's message
//         inputField.text = "";  // clear input field

//         string jsonToSend = JsonUtility.ToJson(request);

//         Debug.Log("URL being sent: " + ollamaUrl);
//         Debug.Log("JSON being sent: " + jsonToSend);


//         using (UnityWebRequest webRequest = new UnityWebRequest(ollamaUrl, "POST"))
//         {
//             byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonToSend);
//             webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             webRequest.downloadHandler = new DownloadHandlerBuffer();
//             webRequest.SetRequestHeader("Content-Type", "application/json");
//             yield return webRequest.SendWebRequest();
//             if (webRequest.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("Error: " + webRequest.error);
//             }
//             else
//             {
//                 string responseJson = webRequest.downloadHandler.text;
//                 OllamaResponse response =
//                     JsonUtility
//                         .FromJson<OllamaResponse>(responseJson);
//                 // botMessage = response.choices[0].text;

//                 CreateMessageBubble(botMessagePrefab, response.choices[0].text);
//             }
//         }
//     }

//     private void CreateMessageBubble(GameObject prefab, string message)
//     {
//         // Instantiate the bubble and make it a child of the container
//         GameObject newBubble = Instantiate(prefab, chatContainer);
        
//         // Find the TextMeshPro component in the prefab's children and set its text
//         TMP_Text messageText = newBubble.GetComponentInChildren<TMP_Text>();
//         if (messageText != null)
//         {
//             messageText.text = message;
//         } else {
//             Debug.LogError("Could not find a TMP_Text component on the message bubble prefab.", newBubble);
//         }
        
//         // Wait a frame for the UI to update, then scroll to the bottom
//         StartCoroutine(ForceScrollDown());
//     }

//     IEnumerator ForceScrollDown()
//     {
//         yield return new WaitForEndOfFrame();
//         scrollRect.verticalNormalizedPosition = 0f;
//     }
// }