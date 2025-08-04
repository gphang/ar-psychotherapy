using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// This script requires that you have the AR Foundation package installed.
// It also requires that the AR Session Origin GameObject has an ARRaycastManager component.

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlacementController : MonoBehaviour
{
    // We no longer need a prefab, as we will find the avatar that already exists.
    // This will be a reference to the avatar object loaded from the previous scene.
    private GameObject avatarToPlace;
    
    // A reference to the ARRaycastManager component.
    private ARRaycastManager arRaycastManager;

    // A list to hold the raycast hits.
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        // Get the ARRaycastManager component when the script starts.
        arRaycastManager = GetComponent<ARRaycastManager>();

        // --- NEW: Find the persistent avatar when the AR scene starts ---
        // This finds the GameObject named "MetaPersonModel" that you marked with DontDestroyOnLoad.
        avatarToPlace = GameObject.Find("MetaPersonModel");

        if (avatarToPlace == null)
        {
            Debug.LogError("ARPlacementController could not find the 'MetaPersonModel'. Make sure it was loaded from the previous scene and not destroyed.");
        }
        else
        {
            // Optional: Hide the avatar initially until the user places it for the first time.
            avatarToPlace.SetActive(false);
            Debug.Log("'MetaPersonModel' found and is ready to be placed in AR.");
        }
    }

    void Update()
    {
        // Check for user touch input.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Only process the touch when it begins.
            if (touch.phase == TouchPhase.Began)
            {
                // Perform a raycast from the touch position against detected planes.
                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    // The raycast hit a plane. Check if we have a valid avatar to place.
                    if (avatarToPlace != null)
                    {
                        // Get the position and rotation of the hit.
                        Pose hitPose = hits[0].pose;

                        // --- MODIFIED LOGIC ---
                        // Instead of instantiating, we move the existing avatar to the hit position.
                        Transform cam = Camera.main.transform;
                        Vector3 pos = cam.position + cam.forward * 1.5f;
                        avatarToPlace.transform.position = pos;

                        // avatarToPlace.transform.position = hitPose.position;

                        // Ensure the avatar is visible now that it has been placed.
                        avatarToPlace.SetActive(true);

                        // --- Align the avatar to face the camera horizontally ---
                        Vector3 cameraPosition = Camera.main.transform.position;
                        Vector3 directionToCamera = cameraPosition - avatarToPlace.transform.position;
                        directionToCamera.y = 0; // Ensures rotation is only on the horizontal axis
                        
                        Quaternion lookRotation = Quaternion.LookRotation(directionToCamera);
                        avatarToPlace.transform.rotation = lookRotation;
                    }
                }
            }
        }
    }
}














// using AvatarSDK.MetaPerson.Loader;
// using System;
// using System.Collections;
// using TMPro;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// [RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
// public class ARAvatarPlacement : MonoBehaviour
// {
//     // public GameObject avatarPrefab;  // Assign in Inspector
//     // private GameObject currentAvatarInstance;
//     public float avatarScale = 0.1f;
//     public float verticalOffset = -1500f;
//     public float placementDistance = -500000000f;

//     // public Transform avatarSpawnPoint;  // Where the avatar should appear
//     private GameObject avatarToPlace;

//     private ARRaycastManager arRaycastManager;
//     private static List<ARRaycastHit> hits = new List<ARRaycastHit>();


//     void Awake()
//     {
//         arRaycastManager = GetComponent<ARRaycastManager>();

//         // --- NEW: Find the persistent avatar when the AR scene starts ---
//         // This finds the GameObject named "MetaPersonModel" that you marked with DontDestroyOnLoad.
//         avatarToPlace = GameObject.Find("MetaPersonModel");

//         if (avatarToPlace == null)
//         {
//             Debug.LogError("No 'MetaPersonModel' found");
//         }
//         else
//         {
//             // Optional: Hide the avatar initially until the user places it for the first time.
//             avatarToPlace.SetActive(false);
//             Debug.Log("'MetaPersonModel' found and is ready to be placed in AR.");
//         }
//     }

//     // void Update()
//     // {
//     //     // Check for user touch input.
//     //     if (Input.touchCount > 0)
//     //     {
//     //         Touch touch = Input.GetTouch(0);

//     //         // Only process the touch when it begins.
//     //         if (touch.phase == TouchPhase.Began)
//     //         {
//     //             // PlaceAvatar(Input.GetTouch(0).position);
//     //             // Perform a raycast from the touch position against detected planes.
//     //             if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
//     //             {
//     //                 // The raycast hit a plane. Check if we have a valid avatar to place.
//     //                 if (avatarToPlace != null)
//     //                 {
//     //                     // Pose hitPose = hits[0].pose;

//     //                     // // Place the avatar at the point where the user tapped.
//     //                     // // This is more intuitive than a fixed distance.
//     //                     // // Apply the vertical offset to prevent floating.
//     //                     // Vector3 targetPosition = hitPose.position + new Vector3(placementDistance, verticalOffset, 0);
//     //                     // avatarToPlace.transform.position = targetPosition;

//     //                     // // Reorient the avatar to face the camera.
//     //                     // Vector3 cameraPosition = Camera.main.transform.position;
//     //                     // Vector3 lookAtPosition = new Vector3(cameraPosition.x, targetPosition.y, cameraPosition.z);
//     //                     // avatarToPlace.transform.LookAt(lookAtPosition);

//     //                     // // Rotate the avatar 180 degrees on the Y axis to flip it if it's backward.
//     //                     // avatarToPlace.transform.Rotate(0f, 180f, 0f);

//     //                     // // Apply the scale.
//     //                     // avatarToPlace.transform.localScale = Vector3.one * avatarScale;
                        
//     //                     // // Ensure the avatar is visible now that it has been placed.
//     //                     // avatarToPlace.SetActive(true);






//     //                     // 1. Get the ground level (Y-coordinate) from where the user tapped.
//     //                     Pose hitPose = hits[0].pose;
//     //                     float groundLevelY = hitPose.position.y;

//     //                     // 2. Get the camera's position and forward direction.
//     //                     Transform cameraTransform = Camera.main.transform;
//     //                     Vector3 cameraPosition = cameraTransform.position;
//     //                     Vector3 cameraForward = cameraTransform.forward;
                        
//     //                     // 3. Flatten the camera's forward vector so it's parallel to the ground.
//     //                     cameraForward.y = 0;
//     //                     cameraForward.Normalize();

//     //                     // 4. Calculate the target position: a set distance in front of the camera, at the ground level.
//     //                     Vector3 targetPosition = cameraPosition + (cameraForward * placementDistance);
//     //                     targetPosition.y = groundLevelY + verticalOffset; // Apply ground level and vertical offset.

//     //                     // 5. Apply the new position to the avatar.
//     //                     avatarToPlace.transform.position = targetPosition;

//     //                     // --- NEW ORIENTATION LOGIC ---
//     //                     // The hitPose.rotation contains the orientation of the detected plane.
//     //                     // This will make the avatar stand upright on the plane, with its forward
//     //                     // direction determined by the plane's own coordinate system.
//     //                     avatarToPlace.transform.rotation = hitPose.rotation;
                        
//     //                     // --- SCALE ---
//     //                     avatarToPlace.transform.localScale = Vector3.one * avatarScale;
                        
//     //                     // Ensure the avatar is visible.
//     //                     avatarToPlace.SetActive(true);

//     //                     Debug.Log($"Placed avatar at {targetPosition}. Ground level was {groundLevelY}.");
//     //                 }
//     //             }
//     //         }
//     //     }
//     // }


//     // void Update()
//     // {
//     //     // Check for user touch input.
//     //     if (Input.touchCount > 0)
//     //     {
//     //         Touch touch = Input.GetTouch(0);

//     //         // Only process the touch when it begins.
//     //         if (touch.phase == TouchPhase.Began)
//     //         {
//     //             // Perform a raycast from the touch position against detected planes.
//     //             if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
//     //             {
//     //                 // The raycast hit a plane. Check if we have a valid avatar to place.
//     //                 if (avatarToPlace != null)
//     //                 {
//     //                     // --- NEW PLACEMENT LOGIC ---
//     //                     // Get the exact position and rotation where the user tapped on the plane.
//     //                     Pose hitPose = hits[0].pose;

//     //                     // 1. Set the avatar's position directly to the hit point.
//     //                     // We also apply the vertical offset to fine-tune its height.
//     //                     Vector3 targetPosition = hitPose.position + new Vector3(0, verticalOffset, placementDistance);
//     //                     avatarToPlace.transform.position = targetPosition;


//     //                     // --- NEW ORIENTATION LOGIC ---
//     //                     // 2. Orient the avatar to face the user, but stay level with the ground.
//     //                     Transform cameraTransform = Camera.main.transform;
//     //                     Vector3 lookAtPosition = new Vector3(cameraTransform.position.x, targetPosition.y, cameraTransform.position.z);
//     //                     avatarToPlace.transform.LookAt(lookAtPosition);

//     //                     // 3. Rotate the avatar 180 degrees if its back is facing the camera.
//     //                     // This is a common fix for models whose "forward" direction is backwards.
//     //                     avatarToPlace.transform.Rotate(0f, 180f, 0f);

                        
//     //                     // --- SCALE and VISIBILITY ---
//     //                     avatarToPlace.transform.localScale = Vector3.one * avatarScale;
//     //                     avatarToPlace.SetActive(true);

//     //                     Debug.Log($"Placed avatar at {targetPosition}.");
//     //                 }
//     //             }
//     //         }
//     //     }
//     // }
// }
   
    

//     // void PlaceAvatar(Vector2 touchPosition)
//     // {
//     //     var rayHits = new List<ARRaycastHit>();
//     //     ARRaycastManager.Raycast(touchPosition, rayHits, TrackableType.AllTypes);

//     //     if (rayHits.Count > 0)
//     //     {
//     //         Vector3 hitPosePosition = rayHits[0].pose.position;
//     //         Quaternion hitPoseRotation = rayHits[0].pose.rotation;
//     //         Instantiate(ARRaycastManager.raycastPrefab, hitPosePosition, hitPoseRotation);
//     //     }

//     //     StartCoroutine(SetDelay());
//     // }

//     // IEnumerator SetDelay()
//     // {
//     //     yield return new WaitForSeconds(0.25f);
//     // }
// }





// // [RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
// // public class PlaceObjects : MonoBehaviour
// // {
// //     public GameObject objectToPlace;
// //     public ARRaycastManager raycastManager;
// //     private List<ARRaycastHit> hits = new List<ARRaycastHit>();

// //     void Update()
// //     {
// //         if (Input.touchCount > 0)
// //         {
// //             Touch touch = Input.GetTouch(0);
// //             if (touch.phase == TouchPhase.Began)
// //             {
// //                 if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
// //                 {
// //                     Pose hitPose = hits[0].pose;
// //                     Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
// //                 }
// //             }
// //         }
// //     }
// // }














// public class ARAvatarPlacement : MonoBehaviour
// {
//     [SerializeField] private ARRaycastManager raycastManager;
//     bool isPlacing = false;

//     void Update() {
//         if (!raycastManager) return;
//         if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
//         {
//             isPlacing = true;
//             PlaceObject(Input.GetTouch(0).position);
//         }
//     }

//     void PlaceObject(Vector2 touchPosition)
//     {
//         var rayHits = new List<ARRaycastHit>();
//         raycastManager.Raycast(touchPosition, rayHits, TrackableType.AllTypes);

//         if (rayHits.Count > 0)
//         {
//             Vector3 hitPosePosition = rayHits[0].pose.position;
//             Quaternion hitPoseRotation = rayHits[0].pose.rotation;
//             Instantiate(raycastManager.raycastPrefab, hitPosePosition, hitPoseRotation);
//         }

//         StartCoroutine(SetIsPlacingFalse());
//     }

//     IEnumerator SetIsPlacingFalse() {
//         yield return new WaitForSeconds(0.25f);
//         isPlacing = false;
//     }
// }