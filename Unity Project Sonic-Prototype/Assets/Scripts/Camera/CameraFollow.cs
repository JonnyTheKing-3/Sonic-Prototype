using System;
using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
        // Replace 0 later with rightstick controls for a gamepad
        float inputX = 0f;
        float inputZ = 0f;

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
