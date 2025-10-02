using UnityEngine;

public class ThirdPersonOrbitCamera : MonoBehaviour
{

    public Transform target;

    public float rotationSpeed = 2.0f;

    public float minVerticalAngle = -45.0f;
    public float maxVerticalAngle = 45.0f;

    private float yaw = 0.0f;   
    private float pitch = 0.0f; 

    void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;


        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }


    void LateUpdate()
    {
        if (target != null)
        {

            transform.position = target.position;


            yaw += rotationSpeed * Input.GetAxis("Mouse X");
            pitch -= rotationSpeed * Input.GetAxis("Mouse Y");

            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

            Quaternion finalRotation = Quaternion.Euler(pitch, yaw, 0);

            transform.rotation = finalRotation;
        }
    }
}