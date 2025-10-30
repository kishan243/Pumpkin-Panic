using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectGrabble : MonoBehaviour
{
    public Rigidbody objectRigidbody { get; private set; }

    [SerializeField] private GameObject hitEffectPrefab;

    private bool isArmed = false;

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        isArmed = false;
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

    public void ArmPumpkin()
    {
        isArmed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isArmed)
        {
            return;
        }

        if (other.gameObject.CompareTag("Collectible"))
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
            }

            if (other.TryGetComponent(out GhostAI ghost))
            {
                ghost.TakeHit();
            }
            else
            {
                Destroy(other.gameObject);
            }

            Destroy(this.gameObject);
        }
    }
}