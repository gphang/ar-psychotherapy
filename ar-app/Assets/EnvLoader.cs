using UnityEngine;
using System;
using System.IO;

public class EnvLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadEnv()
    {
        // Load the text asset from the Resources folder
        // Note: Do not include the .txt file extension in the path
        TextAsset envTextAsset = Resources.Load<TextAsset>("env");

        if (envTextAsset == null)
        {
            Debug.LogError("env.txt file not found in any Resources folder. Please create it at 'Assets/Resources/env.txt'");
            return;
        }

        // Parse the text asset line by line
        foreach (var line in envTextAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(
                '=',
                2, // Split only on the first equals sign
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}