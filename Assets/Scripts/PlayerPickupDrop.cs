using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerPickupDrop : MonoBehaviour
{
    [Header("Core Setup")]
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private LayerMask pickupLayerMask;

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponGrabPointTransform;
    [SerializeField] private float shootForce = 30f;
    [SerializeField] private GameObject pumpkinPrefab;

    [Header("Food Settings")]
    [SerializeField] private Transform foodGrabPointTransform;

    [Header("UI")]
    [SerializeField] private GameObject pickupPrompt;

    [Header("Parameters")]
    [SerializeField] private float pickupRange = 5f;
    [SerializeField] private float pickupRadius = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    private AudioSource audioSource;

    private ObjectGrabble objectHeld;
    private PowerupItem foodHeld;
    private PlayerBuffManager buffManager;

    private bool hasInfiniteAmmo = false;
    private bool isRespawningPumpkin = false;

    void Start()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
        audioSource = GetComponent<AudioSource>();
        buffManager = GetComponent<PlayerBuffManager>();
    }

    void Update()
    {
        if (objectHeld != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ShootPumpkin();
            }
        }
        else if (foodHeld != null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                ConsumeFood();
            }
        }
        else
        {
            if (hasInfiniteAmmo && !isRespawningPumpkin)
            {
                StartCoroutine(RespawnPumpkin());
            }

            if (!buffManager.IsBuffActive)
            {
                CheckForPickups();
            }
        }
    }

    private void ShootPumpkin()
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

    private void ConsumeFood()
    {
        buffManager.ConsumePowerup(foodHeld);
        foodHeld = null;
    }

    private void CheckForPickups()
    {
        if (Physics.SphereCast(playerCameraTransform.position, pickupRadius, playerCameraTransform.forward, out RaycastHit raycastHit, pickupRange, pickupLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out ObjectGrabble pumpkin))
            {
                pickupPrompt.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    objectHeld = pumpkin;
                    objectHeld.Grab(weaponGrabPointTransform);
                    pickupPrompt.SetActive(false);
                }
            }
            else if (raycastHit.transform.TryGetComponent(out PowerupItem food))
            {
                pickupPrompt.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    foodHeld = food;
                    foodHeld.Grab(foodGrabPointTransform);
                    pickupPrompt.SetActive(false);
                }
            }
            else
            {
                pickupPrompt.SetActive(false);
            }
        }
        else
        {
            pickupPrompt.SetActive(false);
        }
    }

    public void SetInfiniteAmmo(bool status)
    {
        hasInfiniteAmmo = status;
    }

    private IEnumerator RespawnPumpkin()
    {
        isRespawningPumpkin = true;
        yield return new WaitForSeconds(1.5f);

        if (objectHeld == null && foodHeld == null && hasInfiniteAmmo)
        {
            GameObject newPumpkin = Instantiate(pumpkinPrefab, weaponGrabPointTransform.position, weaponGrabPointTransform.rotation);
            objectHeld = newPumpkin.GetComponent<ObjectGrabble>();
            objectHeld.Grab(weaponGrabPointTransform);
        }
        isRespawningPumpkin = false;
    }
}