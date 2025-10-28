using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectGrabble : MonoBehaviour
{
    public Rigidbody objectRigidbody { get; private set; }

    [SerializeField] private GameObject hitEffectPrefab;

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        objectRigidbody.isKinematic = true;
        this.transform.SetParent(objectGrabPointTransform);
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        this.transform.SetParent(null);
        objectRigidbody.isKinematic = false;
    }

    // --- THIS IS THE FIX ---
    // The parameter is now 'Collider other' instead of 'Collision collision'
    private void OnTriggerEnter(Collider other)
    {
        // We only care about hits *after* we've been shot
        if (transform.parent != null)
        {
            return;
        }

        // Check if the thing we hit is tagged "Ghost"
        if (other.gameObject.CompareTag("Collectible"))
        {
            // --- 1. Show the nice UI/Effect ---
            if (hitEffectPrefab != null)
                // We don't have a contact point, so just spawn at the ghost's position
                Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
        }

        // --- 2. Tell the ghost it's been hit ---
        if (other.TryGetComponent(out GhostAI ghost))
        {
            ghost.TakeHit(); // Call the ghost's death function
        }
        else
        {
            // Fallback in case the ghost doesn't have the script (it should)
            Destroy(other.gameObject);
        }

        // --- 3. Make the pumpkin disappear ---
        Destroy(this.gameObject);
    }
}

