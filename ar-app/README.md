# AR Application Interface for SIHP

Open this folder in Unity version 2022.3.6f1 to launch the AR environment.

Users begin by selecting from a set of pre-loaded MetaPerson avatars or customizing their own. The chosen avatar acts as the virtual psychotherapist within the AR space.

MetaPerson Avatar Documentation: https://docs.metaperson.avatarsdk.com/

Store your client ID and secret in a env.txt file, kept under the Resources folder.

## Project Structure
- `Assets/` - Main Unity assets including scenes, scripts, and prefabs
- `Scripts/` - C# scripts for avatar logic, emotion recognition integration, and scene control
- `Scenes/` - Unity scenes used in AR rendering
- `TextMesh Pro/` - Font rendering (core files only, examples/docs excluded)
- `.gitignore` - Configured to avoid Unity-generated and large unnecessary files


## Cloud Deployment
This application was created using Google Cloud services. To run it on the cloud, please store your API key in your env.txt file enable the following services on your Google Cloud console:
* Generative Language API
* Cloud Speech-to-Text API
* Cloud Text-to-Speech API


## Local Deployment

### Running the Chatbot
Download Ollama from: https://ollama.com/download
Download ngrok following: https://gist.github.com/wosephjeber/aa174fb851dfe87e644e

In terminal, first run Ollama on your local host. Then, run it on ngrok before deploying on your mobile devices
```
OLLAMA_HOST="0.0.0.0" ollama serve
ngrok http 11434
```

Running ngrok would provide you with a public URL, which you would have to use to update the webRequest in Unity script (OllamaController.cs), using the format: "<ngrok_url>/v1/completions"


### Running the Emotion Classifier
Download all requirements.txt

In terminal, run the following to host the emotion classifier on a Flask app:
```
cd emotion_classifier
python app.py
```

- To deploy to iOS:
  - Open in Xcode via Unity > Build Settings > iOS
  - Configure signing & provisioning profile

- Android:
  - Configure ARCore and SDK settings
  - Build APK and install on Android device
