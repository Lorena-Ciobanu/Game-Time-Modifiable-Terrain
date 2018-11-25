using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float stickMinZoom = -250f;

    [SerializeField]
    private float stickMaxZoom = -45f;

    [SerializeField]
    private float swivelMinZoom = 90;

    [SerializeField]
    private float swivelMaxZoom = 45;

    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private float rotationSpeed;


    private Transform swivel, stick;
    private float zoom = 1f;
    private float rotationAngle;



    private void Awake()
    {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    private void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if(zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");        // Edit -> Project Settings -> Input
        if(rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if(xDelta != 0 || zDelta != 0)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }


    /* Adjust Zoom */
    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }


    /* Adjust Position */
    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = moveSpeed * damping * Time.deltaTime;
        transform.localPosition += direction * distance;
    }


    /* Adjust Rotation */
    void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if(rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }else if(rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }

        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }
}
