using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AvatarSDK.MetaPerson.Loader;

public class ARPlacementController : MonoBehaviour
{
    [Header("Avatar Loading")]
    public TextMeshProUGUI progressText;
    public MetaPersonLoader childMetaPersonLoader;
    public TextMeshProUGUI instructionText;
    public Button backButton;

    [Header("AR Placement")]
    public ARRaycastManager raycastManager; // Assign this in the Inspector
    public GameObject placementIndicator; // Assign a visual prefab for this

    private GameObject loadedAvatar;
    private bool isAvatarPlaced = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public Vector3 avatarScale = new Vector3(2.5f, 2.5f, 2.5f);

    async void Start()
    {
        // Hide placement objects initially
        placementIndicator.SetActive(false);

        progressText.gameObject.SetActive(true);
        loadedAvatar = await LoadChildAvatarAsync();
        progressText.gameObject.SetActive(false);
        instructionText.gameObject.SetActive(true);

        if (backButton != null)
        {
            backButton.onClick.AddListener(GoToChatbot);
        }

        if (loadedAvatar != null)
        {
            loadedAvatar.SetActive(false); // Keep it inactive until placed
        }
    }

    public void GoToChatbot()
    {
        SceneManager.LoadScene(1);
    }

    void Update()
    {
        if (isAvatarPlaced || loadedAvatar == null) return;

        // Keep updating the visual indicator based on the center of the screen
        UpdatePlacementIndicator();

        // Check for a tap, try to place the avatar at the tap's location
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceAvatar(Input.GetTouch(0).position);
        }
    }

    private void UpdatePlacementIndicator()
    {
        // Raycast from the center of the screen
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            // If we hit a plane, move the indicator to the hit position
            var hitPose = hits[0].pose;
            placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            placementIndicator.SetActive(true);
            instructionText.text = "Tap to place the avatar!";
        }
        else
        {
            // If we don't hit a plane, hide the indicator
            placementIndicator.SetActive(false);
            instructionText.text = "Move your phone to find a surface...";
        }
    }

    private void PlaceAvatar(Vector2 touchPosition)
    {
        // Raycast from the position the user tapped on the screen.
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            // Set initial position and rotation from the plane hit
            loadedAvatar.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

            // Adjust rotation + scaling of avatar
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 lookAtPosition = new Vector3(cameraPosition.x, loadedAvatar.transform.position.y, cameraPosition.z);
            loadedAvatar.transform.LookAt(lookAtPosition);
            loadedAvatar.transform.Rotate(0f, 0f, 0f);
            loadedAvatar.transform.localScale = avatarScale;

            loadedAvatar.SetActive(true);
            isAvatarPlaced = true;
            placementIndicator.SetActive(false);
        }

        instructionText.gameObject.SetActive(false);
    }

    private async Task<GameObject> LoadChildAvatarAsync()
    {
        string childAvatarUrl = AvatarManager.Instance.ChildAvatarUrl;
        if (string.IsNullOrEmpty(childAvatarUrl))
        {
            Debug.LogError("Child Avatar URL not found in AvatarManager.");
            return null;
        }

        // Clear any previous models
        foreach (Transform child in childMetaPersonLoader.transform)
        {
            Destroy(child.gameObject);
        }

        float childLoadProgress = 0f;
        UpdateProgressText(childLoadProgress);

        bool isLoaded = await childMetaPersonLoader.LoadModelAsync(childAvatarUrl, p =>
        {
            childLoadProgress = p;
            UpdateProgressText(childLoadProgress);
        });

        if (isLoaded && childMetaPersonLoader.transform.childCount > 0)
        {
            // Return the loaded avatar GameObject
            return childMetaPersonLoader.transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogError("Failed to load Child Avatar from URL: " + childAvatarUrl);
            return null;
        }
    }
    
    private void UpdateProgressText(float progress)
    {
        progressText.text = $"Loading avatar... {(int)(progress * 100)}%";
    }
}
























// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// // This script requires that you have the AR Foundation package installed.
// // It also requires that the AR Session Origin GameObject has an ARRaycastManager component.

// [RequireComponent(typeof(ARRaycastManager))]
// public class ARPlacementController : MonoBehaviour
// {
//     // We no longer need a prefab, as we will find the avatar that already exists.
//     // This will be a reference to the avatar object loaded from the previous scene.
//     private GameObject avatarToPlace;
    
//     // A reference to the ARRaycastManager component.
//     private ARRaycastManager arRaycastManager;

//     // A list to hold the raycast hits.
//     private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

//     void Awake()
//     {
//         // Get the ARRaycastManager component when the script starts.
//         arRaycastManager = GetComponent<ARRaycastManager>();

//         // --- NEW: Find the persistent avatar when the AR scene starts ---
//         // This finds the GameObject named "MetaPersonModel" that you marked with DontDestroyOnLoad.
//         avatarToPlace = GameObject.Find("MetaPersonModel");

//         if (avatarToPlace == null)
//         {
//             Debug.LogError("ARPlacementController could not find the 'MetaPersonModel'. Make sure it was loaded from the previous scene and not destroyed.");
//         }
//         else
//         {
//             // Optional: Hide the avatar initially until the user places it for the first time.
//             avatarToPlace.SetActive(false);
//             Debug.Log("'MetaPersonModel' found and is ready to be placed in AR.");
//         }
//     }

//     void Update()
//     {
//         // Check for user touch input.
//         if (Input.touchCount > 0)
//         {
//             Touch touch = Input.GetTouch(0);

//             // Only process the touch when it begins.
//             if (touch.phase == TouchPhase.Began)
//             {
//                 // Perform a raycast from the touch position against detected planes.
//                 if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
//                 {
//                     // The raycast hit a plane. Check if we have a valid avatar to place.
//                     if (avatarToPlace != null)
//                     {
//                         // Get the position and rotation of the hit.
//                         Pose hitPose = hits[0].pose;

//                         // --- MODIFIED LOGIC ---
//                         // Instead of instantiating, we move the existing avatar to the hit position.
//                         Transform cam = Camera.main.transform;
//                         Vector3 pos = cam.position + cam.forward * 1.5f;
//                         avatarToPlace.transform.position = pos;

//                         // avatarToPlace.transform.position = hitPose.position;

//                         // Ensure the avatar is visible now that it has been placed.
//                         avatarToPlace.SetActive(true);

//                         // --- Align the avatar to face the camera horizontally ---
//                         Vector3 cameraPosition = Camera.main.transform.position;
//                         Vector3 directionToCamera = cameraPosition - avatarToPlace.transform.position;
//                         directionToCamera.y = 0; // Ensures rotation is only on the horizontal axis
                        
//                         Quaternion lookRotation = Quaternion.LookRotation(directionToCamera);
//                         avatarToPlace.transform.rotation = lookRotation;
//                     }
//                 }
//             }
//         }
//     }
// }