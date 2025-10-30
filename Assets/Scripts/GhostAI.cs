using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GhostState { Idle, Chase, PrepareDash, Dashing }

[RequireComponent(typeof(AudioSource))]
public class GhostAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float aggroRange = 15f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Dash Attack Settings")]
    [SerializeField] private float dashPrepareRange = 7f;
    [SerializeField] private float dashChargeTime = 1.0f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float playerTargetHeightOffset = 1.0f;
    [SerializeField] private float dashHitRadius = 1.0f;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("UI Feedback")]
    [SerializeField] private Image aggroIcon;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRangeX = 5f;
    [SerializeField] private float wanderRangeZ = 5f;
    [SerializeField][Range(0f, 1f)] private float wanderWeight = 0.3f;
    [SerializeField][Range(0f, 1f)] private float chaseWeight = 0.7f;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip dashSound;

    private Transform playerTransform;
    private Collider ghostCollider;
    private Vector3 wanderDir;
    private float wanderTimer;
    private Vector3 startPosition;
    private GhostState currentState;
    private bool canAttack = true;
    private bool hasHitPlayerThisDash = false;
    private bool isDying = false;

    private AudioSource audioSource;

    void Start()
    {
        playerTransform = PlayerHealth.Instance.transform;
        startPosition = transform.position;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

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

        GameManager.Instance.RegisterGhost();
    }

    void Update()
    {
        if (playerTransform == null) return;

        wanderTimer += Time.deltaTime;
        if (wanderTimer > 2f)
        {
            PickWanderDirection();
            wanderTimer = 0;
        }

        if (aggroIcon != null && aggroIcon.gameObject.activeInHierarchy)
        {
            aggroIcon.transform.parent.LookAt(Camera.main.transform);
        }

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
                break;
        }
    }

    private void HandleIdleState()
    {
        transform.position += wanderDir * moveSpeed * Time.deltaTime;

        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, startPosition.x - wanderRangeX, startPosition.x + wanderRangeX);
        pos.y = Mathf.Clamp(pos.y, 2, 5);
        pos.z = Mathf.Clamp(pos.z, startPosition.z - wanderRangeZ, startPosition.z + wanderRangeZ);
        transform.position = pos;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= aggroRange)
        {
            ChangeState(GhostState.Chase);
        }
    }

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

    private void HandlePrepareDashState()
    {
        Vector3 targetPosition = playerTransform.position + (Vector3.up * playerTargetHeightOffset);
        Vector3 lookDirection = targetPosition - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    private IEnumerator DashAttack()
    {
        canAttack = false;
        hasHitPlayerThisDash = false;

        ChangeState(GhostState.PrepareDash);

        yield return new WaitForSeconds(dashChargeTime);

        Vector3 dashTarget = playerTransform.position + (Vector3.up * playerTargetHeightOffset);
        Vector3 dashDirection = (dashTarget - transform.position).normalized;

        ChangeState(GhostState.Dashing);

        if (dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }

        float dashTimer = 0;
        while (dashTimer < dashDuration)
        {
            if (currentState != GhostState.Dashing) yield break;

            float distanceThisFrame = dashSpeed * Time.deltaTime;

            if (!hasHitPlayerThisDash && Physics.SphereCast(transform.position, dashHitRadius, dashDirection, out RaycastHit hit, distanceThisFrame, playerLayerMask))
            {
                Debug.Log("GHOST: Successfully hit the Player!");
                hasHitPlayerThisDash = true;
                PlayerHealth.Instance.TakeDamage(attackDamage);
            }

            transform.position += dashDirection * distanceThisFrame;

            dashTimer += Time.deltaTime;
            yield return null;
        }

        ChangeState(GhostState.Chase);
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

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
        if (isDying) return;
        isDying = true;

        GameManager.Instance.OnGhostKilled();

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

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

