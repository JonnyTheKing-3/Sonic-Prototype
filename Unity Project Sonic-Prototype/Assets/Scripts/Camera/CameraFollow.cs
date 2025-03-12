using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraFollow : MonoBehaviour
{
    public float CameraMoveSpeed;
    public GameObject CameraFollowObj;
    private Vector3 FollowPos;
    public float ClampAngle;
    public float InputSensitivity;
    public GameObject CameraObj;
    public GameObject PlayerObj;
    public SonicMovement playerScript;
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

    public bool OnLoopPath = false;

    void Start()
    {
        // Initialize rotations
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerScript = PlayerObj.GetComponent<SonicMovement>();
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

        // if the camera is suppoesd to follow the loop path, then simply stick the camera to the spline and don't do anything else
        if (playerScript.loopInfo != null)
        {
            if (playerScript.loopInfo.ActivateCameraLoopPath)
            {
                OnLoopPath = true;

                CameraLoopDeLoop camPath = PlayerObj.GetComponent<SonicMovement>().loopInfo.cameraLoopCart;
            
                transform.position = camPath.transform.position;
                CameraObj.transform.position = transform.position;

                CameraObj.transform.LookAt(playerScript.transform.position);

                return;
            }
            else
            {
                if (OnLoopPath) 
                {
                    SetCameraBackToPlayer();
                    return;
                }    
            }
        }
        else 
        {
            if (OnLoopPath) 
            {
                SetCameraBackToPlayer();
                return;
            }     
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

    public void SetCameraBackToPlayer()
    {
        OnLoopPath = false;
        transform.position = CameraFollowObj.transform.position;

        Vector3 viewDirAfterLoop = playerScript.rb.linearVelocity;

        // Calculate an upward offset perpendicular to the movement direction
        Vector3 rightDir = Vector3.Cross(viewDirAfterLoop, Vector3.up); // Right direction relative to movement
        Vector3 upOffset = Vector3.Cross(rightDir, viewDirAfterLoop).normalized * -31f; // Up-left offset

        viewDirAfterLoop += upOffset; // Apply the offset
        
        viewDirAfterLoop.Normalize();

        // Notice the minus sign: now the parent’s forward is “backwards”
        Quaternion targetRotation = Quaternion.LookRotation(viewDirAfterLoop, Vector3.up);
        Vector3 targetEuler = targetRotation.eulerAngles;

        rotX += Mathf.DeltaAngle(rotX, targetEuler.x);
        rotY += Mathf.DeltaAngle(rotY, targetEuler.y);
        transform.rotation = Quaternion.Euler(rotX, rotY, 0f);

        // Child is at (0,0,-20). But since the parent is reversed, 
        // in world space the camera is actually behind the player 
        // and looking forward at them.
        CameraObj.transform.localPosition = new Vector3(0, 0, -20);
        CameraObj.transform.localRotation = Quaternion.identity;
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
            Vector3 newEulerAngles = overrideRotation.eulerAngles;
            rotX = Mathf.DeltaAngle(0, newEulerAngles.x);
            rotY = Mathf.DeltaAngle(0, newEulerAngles.y);


            // Debug.Log("NEW ANGLES HERE!!!!!!!!!!!! ---- y: " + rotY + " --- x: " + rotX);
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
        if (OnLoopPath) { return; }

        // Debug.Log("Follow player");
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
