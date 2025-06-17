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

- To deploy to iOS:
  - Open in Xcode via Unity > Build Settings > iOS
  - Configure signing & provisioning profile

- Android:
  - Configure ARCore and SDK settings
  - Build APK and install on Android device
