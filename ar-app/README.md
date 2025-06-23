# AR Application Interface for SIHP

Open this folder in Unity version 2022.3.6f1 to launch the AR environment.

Users begin by selecting from a set of pre-loaded MetaPerson avatars or customizing their own. The chosen avatar acts as the virtual psychotherapist within the AR space.

MetaPerson Avatar Documentation: https://docs.metaperson.avatarsdk.com/


## Project Structure
- `Assets/` - Main Unity assets including scenes, scripts, and prefabs
- `Scripts/` - C# scripts for avatar logic, emotion recognition integration, and scene control
- `Scenes/` - Unity scenes used in AR rendering
- `TextMesh Pro/` - Font rendering (core files only, examples/docs excluded)
- `.gitignore` - Configured to avoid Unity-generated and large unnecessary files


## Deployment

### Running the Chatbot
Download Ollama from: https://ollama.com/download
Download ngrok following: https://gist.github.com/wosephjeber/aa174fb851dfe87e644e

In terminal, first run Ollama on your local host. Then, run it on ngrok before deploying on your mobile devices
```
OLLAMA_HOST="0.0.0.0" ollama serve
ngrok http 11434
```

Running ngrok would provide you with a public URL, which you would have to use to update the webRequest in Unity script (OllamaController.cs), using the format: "<ngrok_url>/v1/completions"


- To deploy to iOS:
  - Open in Xcode via Unity > Build Settings > iOS
  - Configure signing & provisioning profile

- Android:
  - Configure ARCore and SDK settings
  - Build APK and install on Android device
