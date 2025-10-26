using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupDrop : MonoBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private LayerMask pickupLayerMask;

    [SerializeField] private GameObject pickupPrompt;

    [SerializeField] private float pickupRange = 5f;
    [SerializeField] private float pickupRadius = 0.5f;

    [SerializeField] private float shootForce = 30f;

    // We don't need the shootRaycastDistance anymore
    // [SerializeField] private float shootRaycastDistance = 100f; 

    private ObjectGrabble objectToGrab;
    private ObjectGrabble objectHeld;

    void Start()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // --- 1. LOGIC FOR SHOOTING AN OBJECT ---
        if (objectHeld != null)
        {
            // Left mouse button
            if (Input.GetMouseButtonDown(0))
            {
                // --- NEW "VALORANT-STYLE" AIMING LOGIC ---

                // 1. Tell the object to drop (this un-parents it and turns on physics)
                objectHeld.Drop();

                // 2. CRITICAL FIX: Teleport the pumpkin to be directly in front of the camera.
                // This guarantees it flies from the center dot.
                // We move it 1 unit forward so it doesn't spawn *inside* the player.
                objectHeld.transform.position = playerCameraTransform.position + playerCameraTransform.forward * 1.0f;

                // 3. Apply force DIRECTLY forward from the camera
                objectHeld.objectRigidbody.AddForce(playerCameraTransform.forward * shootForce, ForceMode.Impulse);

                // 4. Forget the object we were holding
                objectHeld = null;
            }
            return; // Stop here if we're holding something
        }

        // --- 2. LOGIC FOR FINDING AND PICKING UP AN OBJECT ---
        // (This part is unchanged)
        if (Physics.SphereCast(playerCameraTransform.position, pickupRadius, playerCameraTransform.forward, out RaycastHit raycastHit, pickupRange, pickupLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out objectToGrab))
            {
                if (pickupPrompt != null)
                {
                    pickupPrompt.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    objectHeld = objectToGrab;
                    objectHeld.Grab(objectGrabPointTransform);

                    if (pickupPrompt != null)
                    {
                        pickupPrompt.SetActive(false);
                    }
                }
            }
            else
            {
                objectToGrab = null;
                if (pickupPrompt != null)
                {
                    pickupPrompt.SetActive(false);
                }
            }
        }
        else
        {
            objectToGrab = null;
            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(false);
            }
        }
    }
}

