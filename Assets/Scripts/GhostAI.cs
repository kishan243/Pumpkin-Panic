using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for the aggro icon

// --- A state machine to manage the ghost's behavior ---
public enum GhostState { Idle, Chase, PrepareDash, Dashing }

public class GhostAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float aggroRange = 15f;
    [SerializeField] private float attackCooldown = 2f; // Time between attacks

    [Header("Dash Attack Settings")]
    [SerializeField] private float dashPrepareRange = 7f; // How close to get before preparing to dash
    [SerializeField] private float dashChargeTime = 1.0f; // How long to "wind up" the dash
    [SerializeField] private float dashSpeed = 15f; // How fast the dash is
    [SerializeField] private float dashDuration = 1.5f; // How long the dash lasts
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float playerTargetHeightOffset = 1.0f;
    [SerializeField] private float dashHitRadius = 1.0f; // --- NEW: The "width" of the dash attack ---
    [SerializeField] private LayerMask playerLayerMask; // --- NEW: So the cast only hits the player ---

    [Header("UI Feedback")]
    [SerializeField] private Image aggroIcon; // Drag the "!" icon Image here

    [Header("Wander Settings")]
    [SerializeField] private float wanderRangeX = 5f;
    [SerializeField] private float wanderRangeZ = 5f;
    [SerializeField][Range(0f, 1f)] private float wanderWeight = 0.3f;
    [SerializeField][Range(0f, 1f)] private float chaseWeight = 0.7f;

    private Transform playerTransform;
    private Collider ghostCollider;
    private Vector3 wanderDir;
    private float wanderTimer;
    private Vector3 startPosition;

    private GhostState currentState;
    private bool canAttack = true;
    private bool hasHitPlayerThisDash = false;

    void Start()
    {
        playerTransform = PlayerHealth.Instance.transform;
        startPosition = transform.position;

        if (aggroIcon != null)
        {
            aggroIcon.gameObject.SetActive(false);
        }

        ghostCollider = GetComponent<Collider>();
        if (!ghostCollider.isTrigger)
        {
            Debug.LogWarning("GhostAI: Collider on " + name + " is not set to 'Is Trigger = true'. Attacking may fail.");
        }

        PickWanderDirection();
        currentState = GhostState.Idle;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // --- Handle Wander Timer ---
        wanderTimer += Time.deltaTime;
        if (wanderTimer > 2f)
        {
            PickWanderDirection();
            wanderTimer = 0;
        }

        // --- Handle Aggro Icon Facing Camera ---
        if (aggroIcon != null && aggroIcon.gameObject.activeInHierarchy)
        {
            aggroIcon.transform.parent.LookAt(Camera.main.transform);
        }

        // --- Main State Machine Logic ---
        switch (currentState)
        {
            case GhostState.Idle:
                HandleIdleState();
                break;
            case GhostState.Chase:
                HandleChaseState();
                break;
            case GhostState.PrepareDash:
                HandlePrepareDashState();
                break;
            case GhostState.Dashing:
                // Dash logic is now handled by the coroutine
                break;
        }
    }

    // --- STATE 1: IDLE ---
    private void HandleIdleState()
    {
        transform.position += wanderDir * moveSpeed * Time.deltaTime;

        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, startPosition.x - wanderRangeX, startPosition.x + wanderRangeX);
        pos.y = Mathf.Clamp(pos.y, 2, 5); // Using the 2-5 Y range
        pos.z = Mathf.Clamp(pos.z, startPosition.z - wanderRangeZ, startPosition.z + wanderRangeZ);
        transform.position = pos;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= aggroRange)
        {
            ChangeState(GhostState.Chase);
        }
    }

    // --- STATE 2: CHASE (Floaty follow) ---
    private void HandleChaseState()
    {
        Vector3 targetPosition = playerTransform.position + (Vector3.up * playerTargetHeightOffset);
        Vector3 chaseDirection = (targetPosition - transform.position).normalized;

        Vector3 finalDirection = (wanderDir * wanderWeight + chaseDirection * chaseWeight).normalized;
        transform.position += finalDirection * moveSpeed * Time.deltaTime;
        transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= dashPrepareRange && canAttack)
        {
            StartCoroutine(DashAttack());
        }
        else if (distanceToPlayer > aggroRange)
        {
            ChangeState(GhostState.Idle);
        }
    }

    // --- STATE 3: PREPARE DASH (Stops and aims) ---
    private void HandlePrepareDashState()
    {
        Vector3 targetPosition = playerTransform.position + (Vector3.up * playerTargetHeightOffset);
        Vector3 lookDirection = targetPosition - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    // --- THE ATTACK COROUTINE ---
    private IEnumerator DashAttack()
    {
        canAttack = false;
        hasHitPlayerThisDash = false;

        ChangeState(GhostState.PrepareDash);

        yield return new WaitForSeconds(dashChargeTime);

        Vector3 dashTarget = playerTransform.position + (Vector3.up * playerTargetHeightOffset);
        Vector3 dashDirection = (dashTarget - transform.position).normalized;

        ChangeState(GhostState.Dashing);

        // --- 3. Dash (NEW LOGIC with SphereCast) ---
        float dashTimer = 0;
        while (dashTimer < dashDuration)
        {
            if (currentState != GhostState.Dashing) yield break;

            float distanceThisFrame = dashSpeed * Time.deltaTime;

            // Check if we hit the player in the path of this frame's movement
            if (!hasHitPlayerThisDash && Physics.SphereCast(transform.position, dashHitRadius, dashDirection, out RaycastHit hit, distanceThisFrame, playerLayerMask))
            {
                // We hit the player!
                Debug.Log("GHOST: Successfully hit the Player!");
                hasHitPlayerThisDash = true; // Only hit once
                PlayerHealth.Instance.TakeDamage(attackDamage);
            }

            // Move the ghost
            transform.position += dashDirection * distanceThisFrame;

            dashTimer += Time.deltaTime;
            yield return null;
        }

        // --- 4. Cooldown ---
        ChangeState(GhostState.Chase);
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // --- THIS FUNCTION IS NO LONGER NEEDED ---
    // private void OnTriggerEnter(Collider other)
    // {
    //     ...
    // }

    // --- Helper function to manage state changes and UI ---
    private void ChangeState(GhostState newState)
    {
        currentState = newState;

        if (currentState == GhostState.Idle)
        {
            if (aggroIcon != null)
            {
                aggroIcon.gameObject.SetActive(false);
            }
        }
        else if (currentState == GhostState.Chase)
        {
            if (aggroIcon != null)
            {
                aggroIcon.gameObject.SetActive(true);
            }
        }
    }

    void PickWanderDirection()
    {
        wanderDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    public void TakeHit()
    {
        if (aggroIcon != null)
        {
            aggroIcon.gameObject.SetActive(false);
        }
        StopAllCoroutines();
        this.enabled = false;
        ghostCollider.enabled = false;
        Destroy(gameObject, 0.2f);
    }
}

