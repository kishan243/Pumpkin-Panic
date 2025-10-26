using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectGrabble : MonoBehaviour
{
    public Rigidbody objectRigidbody { get; private set; }

    // --- NEW ---
    // Drag your "Hit" particle effect or UI prefab here
    [SerializeField] private GameObject hitEffectPrefab;
    // -----------

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        // Turn off physics so the object doesn't fall
        objectRigidbody.isKinematic = true;

        // Make the object a child of the grab point so it moves with it
        this.transform.SetParent(objectGrabPointTransform);

        // Snap the object's position and rotation to the grab point
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        // Un-parent the object so it stops following
        this.transform.SetParent(null);

        // Turn physics back on so it can fall
        objectRigidbody.isKinematic = false;
    }

    // --- NEW: This function runs when the pumpkin hits another collider ---
    private void OnCollisionEnter(Collision collision)
    {
        // We only care about collisions *after* we've been shot.
        // If we are parented to the grab point, don't do anything.
        if (transform.parent != null)
        {
            return;
        }

        // Check if the thing we hit is tagged "Ghost"
        if (collision.gameObject.CompareTag("Collectible"))
        {
            // --- 1. Show the nice UI ---
            if (hitEffectPrefab != null)
            {
                // Get the exact point of contact
                ContactPoint contact = collision.contacts[0];

                // Spawn the hit effect at that point, rotated to face away from the surface
                Instantiate(hitEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
            }

            // --- 2. Make the ghost disappear ---
            Destroy(collision.gameObject);

            // --- 3. Make the pumpkin disappear too ---
            // (Optional, but makes sense for a "bullet")
            Destroy(this.gameObject);
        }
    }
}