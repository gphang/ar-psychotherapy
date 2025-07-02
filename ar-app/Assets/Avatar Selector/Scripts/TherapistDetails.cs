using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public static class GameData
{
    public static string TherapistName = "Therapist";  // default name if empty
}


public class TherapistDetails : MonoBehaviour
{
    // Phase 1
    public Button confirmButtonPhase1;
    public Button nextAvatarButton;
    public Button previousAvatarButton;

    // Phase 2
    public TMP_Text therapistInputInstructions;
    public TMP_InputField therapistNameInputField;
    public Button confirmButtonPhase2;

    public GameObject selectedAvatar;

    private void Start()
    {
        ShowPhase1UI();
    }

    // --- Phase 1: Avatar Selection Confirmation ---
    public void OnConfirmAvatarSelection()
    {
        // Hide Phase 1 UI elements
        confirmButtonPhase1.gameObject.SetActive(false);
        nextAvatarButton.gameObject.SetActive(false);
        previousAvatarButton.gameObject.SetActive(false);

        // Show Phase 2 UI elements
        ShowPhase2UI();

        // // Optional: Update status text
        // if (statusText != null)
        // {
        //     statusText.text = "Please enter the therapist's name:";
        //     statusText.gameObject.SetActive(true);
        // }
    }

    // --- Phase 2: Therapist Name Confirmation ---
    public void OnConfirmTherapistName()
    {
        string therapistName = therapistNameInputField.text.Trim(); // Get input and remove leading/trailing spaces
        if (!string.IsNullOrEmpty(therapistName))
        {
            GameData.TherapistName = therapistName;
        }

        // if (string.IsNullOrEmpty(therapistName))
        // {
        //     Debug.LogWarning("Therapist name cannot be empty!");
        //     if (statusText != null)
        //     {
        //         statusText.text = "Therapist name cannot be empty. Please enter a name.";
        //         statusText.color = Color.red; // Make text red for warning
        //     }
        //     return; // Don't proceed if name is empty
        // }

        // Debug.Log("Therapist name confirmed: " + therapistName);

        HidePhase2UI();

        // Transitioning to AR scene for psychotherapy
        // statusText.gameObject.SetActive(false);
        // statusText.text = "Loading AR experience...";
        // statusText.color = Color.yellow;
        // statusText.gameObject.SetActive(true);

        DontDestroyOnLoad(selectedAvatar);
        SceneManager.LoadScene("ChatbotScene");
    }

    // Helper to show Phase 1 UI elements
    private void ShowPhase1UI()
    {
        confirmButtonPhase1.gameObject.SetActive(true);
        nextAvatarButton.gameObject.SetActive(true);
        previousAvatarButton.gameObject.SetActive(true);

        HidePhase2UI();
    }

    // Helper method to show Phase 2 UI elements
    private void ShowPhase2UI()
    {
        therapistNameInputField.gameObject.SetActive(true);
        therapistNameInputField.text = ""; // Clear previous input
        therapistInputInstructions.gameObject.SetActive(true);
        confirmButtonPhase2.gameObject.SetActive(true);
    }

    // Helper method to hide Phase 2 UI elements
    private void HidePhase2UI()
    {
        therapistNameInputField.gameObject.SetActive(false);
        therapistInputInstructions.gameObject.SetActive(false);
        confirmButtonPhase2.gameObject.SetActive(false);
    }
}