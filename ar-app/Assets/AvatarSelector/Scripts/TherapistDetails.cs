using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


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

    // takes place after the user selects the therapist's avatar
    public void OnConfirmAvatarSelection()
    {
        // Hide initial elements
        confirmButtonPhase1.gameObject.SetActive(false);
        nextAvatarButton.gameObject.SetActive(false);
        previousAvatarButton.gameObject.SetActive(false);

        // Show elements for getting therapist's name
        ShowPhase2UI();
    }



    public void OnConfirmTherapistName()
    {
        // Set therapist's name for later use
        string therapistName = therapistNameInputField.text.Trim();
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

        SceneManager.LoadScene("ChatbotScene");
    }


    // -------------------- HELPER FUNCTIONS --------------------

    private void ShowPhase1UI()
    {
        confirmButtonPhase1.gameObject.SetActive(true);
        nextAvatarButton.gameObject.SetActive(true);
        previousAvatarButton.gameObject.SetActive(true);

        HidePhase2UI();
    }

    private void ShowPhase2UI()
    {
        therapistNameInputField.gameObject.SetActive(true);
        therapistNameInputField.text = "";
        therapistInputInstructions.gameObject.SetActive(true);
        confirmButtonPhase2.gameObject.SetActive(true);
    }

    private void HidePhase2UI()
    {
        therapistNameInputField.gameObject.SetActive(false);
        therapistInputInstructions.gameObject.SetActive(false);
        confirmButtonPhase2.gameObject.SetActive(false);
    }
}