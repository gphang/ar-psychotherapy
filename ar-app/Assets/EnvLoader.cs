using UnityEngine;
using System.IO;

public class EnvLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadEnv()
    {
        // Path to your .env file inside Assets/
        string envFilePath = Path.Combine(Application.dataPath, ".env");
        DotEnv.Load(envFilePath);
    }
}