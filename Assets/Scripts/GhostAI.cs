using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for the aggro icon

public class GhostAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float aggroRange = 15f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 2f; // Time between attacks

    [Header("UI Feedback")]
    [SerializeField] private Image aggroIcon; // Drag the "!" icon Image here

    private Transform playerTransform;
    private bool isAggro = false;
    private bool canAttack = true;

    // Make sure your Ghost's main collider is a Trigger
    private Collider ghostCollider;

    void Start()
    {
        // Find the player using the PlayerHealth singleton
        playerTransform = PlayerHealth.Instance.transform;

        if (aggroIcon != null)
        {
            aggroIcon.gameObject.SetActive(false); // Hide icon at start
        }

        ghostCollider = GetComponent<Collider>();
        if (!ghostCollider.isTrigger)
        {
            Debug.LogWarning("GhostAI: Collider on " + name + " is not set to 'Is Trigger = true'. Attacking may fail.");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // --- Check for Aggro ---
        if (!isAggro && distanceToPlayer <= aggroRange)
        {
            // Player entered range, get angry!
            isAggro = true;
            if (aggroIcon != null)
            {
                aggroIcon.gameObject.SetActive(true);
            }
        }

        // --- Handle AI States ---
        if (isAggro)
        {
            // Make the icon always face the camera
            if (aggroIcon != null)
            {
                aggroIcon.transform.parent.LookAt(Camera.main.transform);
            }

            // --- Attack State ---
            if (distanceToPlayer <= attackRange)
            {
                if (canAttack)
                {
                    StartCoroutine(AttackPlayer());
                }
            }
            // --- Chase State ---
            else
            {
                // Move towards the player
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
                // Look at the player
                transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
            }
        }
        // --- Idle State ---
        else
        {
            // TODO: Add wandering logic here if you want.
            // For now, they just float in place.
        }
    }

    private IEnumerator AttackPlayer()
    {
        canAttack = false;

        // The ghost "attacks" by simply being inside the player's trigger
        // We'll check if the player is *still* in range
        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange + 0.5f) // A little buffer
        {
            PlayerHealth.Instance.TakeDamage(attackDamage);
            // TODO: Play attack sound/animation
        }

        // Wait for cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // This is for the *pumpkin* hitting the *ghost*
    public void TakeHit()
    {
        // This function is called by ObjectGrabble
        // You can add particle effects or sounds here

        // For example, disable the aggro icon
        if (aggroIcon != null)
        {
            aggroIcon.gameObject.SetActive(false);
        }

        // Disable the AI and collider so it can't attack while "dying"
        this.enabled = false;
        ghostCollider.enabled = false;

        // TODO: Play a "poof" particle effect

        // Destroy the ghost
        Destroy(gameObject, 0.2f); // Short delay for particle effect
    }
}
