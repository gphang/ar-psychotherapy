using AvatarSDK.MetaPerson.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarInteract : MonoBehaviour
{
    public GameObject avatarPrefab;  // Assign in Inspector
    public Transform avatarSpawnPoint;  // Where the avatar should appear

    private GameObject currentAvatarInstance;

    void Start()
    {
        GameObject existingAvatar = GameObject.Find("MetaPersonModel");

        // existingAvatar.transform.position = Vector3.zero;
        // existingAvatar.transform.rotation = Quaternion.Euler(0, 0, 0); // Face forward

        existingAvatar.transform.position = avatarSpawnPoint.position;
        existingAvatar.transform.rotation = avatarSpawnPoint.rotation;


        DontDestroyOnLoad(existingAvatar); // persist avatar between scenes
    }

    // private void LoadAvatar(int index)
    // {
    //     // Instantiate new avatar with 180° rotation to face forward
    //     currentAvatarInstance = Instantiate(
    //         avatarPrefab,
    //         avatarSpawnPoint.position,
    //         Quaternion.Euler(0, 0, 0) // Apply Y-axis rotation here
    //     );


    //     // Scale avatar to correct size
    //     // currentAvatarInstance.transform.localScale = new Vector3(10f, 10f, 10f);
    // }
}
