using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 3f;
    public Transform cameraTransform;

    public AudioClip collectSound; 
    private AudioSource audioSource;

    private Rigidbody rb;
    private bool grounded = true;

    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if (cameraTransform != null)
        {
            Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;
            Vector3 movement = (camForward * moveZ + camRight * moveX) * moveSpeed;
            rb.AddForce(movement);
        }
        else
        {
            Debug.LogWarning("Player Controller is missing a camera transform");
            Vector3 movement = new Vector3(moveX, 0, moveZ) * moveSpeed;
            rb.AddForce(movement);
        }

        if (grounded && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * (jumpForce * 2) / 3, ForceMode.Impulse);
            grounded = false;
        }
        else if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            grounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            if (gameManager != null)
            {
                gameManager.OnGhostCaptured();
            }

            Destroy(other.gameObject);

            audioSource.PlayOneShot(collectSound);

        }
    }
}