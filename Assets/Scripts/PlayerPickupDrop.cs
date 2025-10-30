using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerPickupDrop : MonoBehaviour
{
    [Header("Core Setup")]
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private LayerMask pickupLayerMask;

    [Header("UI")]
    [SerializeField] private GameObject pickupPrompt;

    [Header("Parameters")]
    [SerializeField] private float pickupRange = 5f;
    [SerializeField] private float pickupRadius = 0.5f;
    [SerializeField] private float shootForce = 30f;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    private AudioSource audioSource;

    private ObjectGrabble objectToGrab;
    private ObjectGrabble objectHeld;

    void Start()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (objectHeld != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                objectHeld.Drop();

                objectHeld.transform.position = playerCameraTransform.position + playerCameraTransform.forward * 1.0f;

                objectHeld.ArmPumpkin();

                objectHeld.objectRigidbody.AddForce(playerCameraTransform.forward * shootForce, ForceMode.Impulse);

                if (shootSound != null)
                {
                    audioSource.PlayOneShot(shootSound);
                }

                objectHeld = null;
            }
            return;
        }

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