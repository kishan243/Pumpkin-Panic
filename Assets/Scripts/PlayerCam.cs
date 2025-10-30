using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCAM : MonoBehaviour
{
    // Drag your Player Body/Orientation object here
    public Transform orientation;

    // These store the total rotation, just like your original script
    private float xRotation = 0f;
    private float yRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Get the sensitivity from our global GameSettings
        float mouseSensitivity = 2.0f; // Default value if GameSettings isn't loaded
        if (GameSettings.Instance != null)
        {
            mouseSensitivity = GameSettings.Instance.mouseSensitivity;
        }

        // 2. Get mouse input (Time.deltaTime is correctly removed)
        // This is the REAL fix for the itch.io vs. local sensitivity difference.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 3. --- THIS IS YOUR ORIGINAL, PERFECT LOGIC ---
        // We accumulate the total rotation
        yRotation += mouseX;
        xRotation -= mouseY;

        // Clamp the up/down (X) rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply the full rotation (up/down and left/right) to the Camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Apply ONLY the left/right rotation to the Player Body (orientation)
        // This is what your movement script reads for A/D strafing.
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}

