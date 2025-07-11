using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSDK.MetaPerson.Sample
{
	public class CameraController : MonoBehaviour
	{
		public Camera mainCamera;

		public int targetScreenWidth;
		public int targetScreenHeight;

		private Vector3 targetCameraPosition;

		private int screenWidth = Screen.width;
		private int screenHeight = Screen.height;

		private void Start()
		{
			targetCameraPosition = mainCamera.transform.position;
			screenWidth = Screen.width;
			screenHeight = Screen.height;
			UpdateCameraPosition();
		}

		private void Update()
		{
			if (screenWidth != Screen.width || screenHeight != Screen.height)
			{
				screenWidth = Screen.width;
				screenHeight = Screen.height;
				UpdateCameraPosition();
			}
		}

		private void UpdateCameraPosition()
		{
			mainCamera.transform.position = ComputeCameraPositionForResolution(mainCamera.transform.rotation.eulerAngles.x, screenWidth, screenHeight);
		}

		private Vector3 ComputeCameraPositionForResolution(float xRotationEuler, int currentScreenWidth, int currentScreenHeight)
		{
			float aspect = ((float)targetScreenHeight / targetScreenWidth) * ((float)currentScreenWidth / currentScreenHeight);
			float z = targetCameraPosition.z * aspect;
			float deltaZ = (z - targetCameraPosition.z);
			float deltaY = deltaZ * (float)Math.Tan(xRotationEuler * Math.PI / 180.0f);
			return new Vector3(targetCameraPosition.x, targetCameraPosition.y + deltaY, z);
		}
	}
}
