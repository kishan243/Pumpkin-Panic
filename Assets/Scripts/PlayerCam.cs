using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCAM : MonoBehaviour
{

    public float senx;
    public float seny;

    public Transform orientation; 

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * senx * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * seny * Time.deltaTime;
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
