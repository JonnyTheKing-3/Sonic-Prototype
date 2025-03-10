using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float CameraMoveSpeed;
    public GameObject CameraFollowObj;
    private Vector3 FollowPos;
    public float ClampAngle;
    public float InputSensitivity;
    public GameObject CameraObj;
    public GameObject PlayerObj;
    public float CamDistanceXToPlayer;
    public float CamDistanceYToPlayer;
    public float CamDistanceZToPlayer;
    public float MouseX;
    public float MouseY;
    public float FinalInputX;
    public float FinalInputZ;
    public float SmoothX;
    public float SmoothY;
    private float rotY = 0.0f;
    private float rotX = 0.0f;

    private bool overrideCamera = false;
    private Quaternion overrideRotation;
    private float overrideDuration = 1.0f; // Adjust time to your liking
    private float overrideTimer = 0f;

    void Start()
    {
        // Initialize rotations
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetTemporaryCameraDirection(Quaternion newRotation, float duration)
    {
        overrideCamera = true;
        overrideRotation = newRotation;
        overrideDuration = duration;
        overrideTimer = 0f;
        // Debug.Log("Camera setup complete. Starting moving camera");
    }

    void Update()
    {
        // Don't update camera if it's being overriden   
        if (overrideCamera)
        {
            CameraOverriding();
            return;
        }

        float inputX = 0f;
        float inputZ = 0f;
        
        if (Input.GetKey(KeyCode.LeftArrow))
            inputX = -1f;
        if (Input.GetKey(KeyCode.RightArrow))
            inputX = 1f;
        if (Input.GetKey(KeyCode.UpArrow))
            inputZ = 1f;
        if (Input.GetKey(KeyCode.DownArrow))
            inputZ = -1f;

        MouseX = Input.GetAxis("Mouse X");
        MouseY = Input.GetAxis("Mouse Y");

        FinalInputX = inputX + MouseX;
        FinalInputZ = inputZ + MouseY;

        rotY += FinalInputX * InputSensitivity * Time.deltaTime;
        rotX += FinalInputZ * InputSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -ClampAngle, ClampAngle);
        
        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;
    }

    public void CameraOverriding()
    {
        // Debug.Log("Override camera is on");
        overrideTimer += Time.deltaTime;
        if (overrideTimer >= overrideDuration)
        {
            overrideCamera = false; // Revert to normal control after duration
            // Debug.Log("FINISHED");
            // Capture the new rotation angles so that player control resumes from here
            Vector3 newEulerAngles = transform.rotation.eulerAngles;
            rotY = newEulerAngles.y;
            rotX = newEulerAngles.x;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, overrideRotation, Time.deltaTime * 5f);
            // Debug.Log("Rotate camera");
            return;
        }
    }

    void LateUpdate()
    {
        CameraUpdater();
    }

    void CameraUpdater()
    {
        // set object to follow
        Transform target = CameraFollowObj.transform;

        // move towards target
        float step = CameraMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }
}
