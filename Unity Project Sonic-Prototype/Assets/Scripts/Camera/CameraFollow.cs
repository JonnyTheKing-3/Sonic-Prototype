using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraFollow : MonoBehaviour
{
    public float CameraMoveSpeed;
    public GameObject CameraFollowObj;
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
    private Quaternion overrideRotation;
    private float overrideDuration = 1.0f; // Adjust time to your liking
    private float overrideTimer = 0f;
    public enum CameraState { Regular, TransitioningToLoop, Loop, TransitioningOutOfLoop, Overriding }

    [Space]
    public float coroutineDuration;
    public float coroutineOutDuration =.2f;

    public CameraLoopDeLoop camPath = null;
    public CameraState cameraState;

    public Vector3 velocityAfterLoop;

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
        cameraState = CameraState.Regular;
        camPath = null;
    }

    public void SetTemporaryCameraDirection(Quaternion newRotation, float duration)
    {
        cameraState = CameraState.Overriding;
        overrideRotation = newRotation;
        overrideDuration = duration;
        overrideTimer = 0f;
        // Debug.Log("Camera setup complete. Starting moving camera");
    }

    void Update()
    {
        switch (cameraState) 
        {
            case CameraState.Regular:
                if (transitionOutCoroutine != null)
                {
                    StopCoroutine(transitionOutCoroutine);
                    transitionOutCoroutine = null;
                }

                if (playerScript.loopInfo != null)
                {
                    if (playerScript.loopInfo.ActivateCameraLoopPath)
                    {
                        camPath = playerScript.loopInfo.cameraLoopCart;
                        cameraState = CameraState.TransitioningToLoop;
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
                break;
            
            case CameraState.Loop:
            // if the camera is suppoesd to follow the loop path, then simply stick the camera to the spline and don't do anything else
                
                if (playerScript.loopInfo == null) 
                {
                    velocityAfterLoop = playerScript.rb.linearVelocity;
                    cameraState = CameraState.TransitioningOutOfLoop;
                    return;
                }
            
                // Gradually move the transform.position towards camPath.transform.position
                transform.position = camPath.transform.position;
                CameraObj.transform.position = transform.position;
                
                CameraObj.transform.LookAt(playerScript.transform.position);

                break;
            
            case CameraState.Overriding:
                CameraOverriding();
                break;
        }
    }

    IEnumerator TransitionToLoopState(float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Retrieve the current target position from the moving spline.
            Vector3 targetPos = camPath.transform.position;

            // Calculate the current target rotation (looking at the player, for instance)
            Quaternion targetRot = Quaternion.LookRotation(playerScript.transform.position - targetPos, Vector3.up);

            // Interpolate from start to the current target values.
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            // move and rotate camera object
            CameraObj.transform.position = Vector3.Lerp(CameraObj.transform.position, transform.position, t);
            Transform modifiedCamRot = CameraObj.transform;
            modifiedCamRot.LookAt(playerScript.transform.position);
            CameraObj.transform.rotation = Quaternion.Slerp(CameraObj.transform.rotation, modifiedCamRot.rotation, t);

            yield return null;
        }

        // Final update to match the target exactly, if needed.
        transform.position = camPath.transform.position;
        transform.rotation = Quaternion.LookRotation(playerScript.transform.position - camPath.transform.position, Vector3.up);

        // Switch state to loop.
        cameraState = CameraState.Loop;
    }

    public IEnumerator TransitionOutOfLoopState(float duration)
    {
        // Keep orientaion the same route
        PlayerObj.transform.parent.GetChild(0).forward = velocityAfterLoop.normalized;

        // Capture starting state
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startCamObjPos = CameraObj.transform.localPosition;
        Quaternion startCamObjRot = CameraObj.transform.localRotation;


        // Determine target position (player's position)
        Vector3 targetPos = CameraFollowObj.transform.position;

        // Calculate the target rotation based on the player's velocity
        Vector3 rightDir = Vector3.Cross(velocityAfterLoop, Vector3.up);
        Vector3 upOffset = Vector3.Cross(rightDir, velocityAfterLoop).normalized * -25f;
        velocityAfterLoop += upOffset;
        velocityAfterLoop.Normalize();
        Quaternion targetRot = Quaternion.LookRotation(velocityAfterLoop, Vector3.up);

        // Smoothly interpolate from current to target state over 'duration' seconds
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smoothly interpolate position and rotation
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            Quaternion nextRot = Quaternion.Slerp(startRot, targetRot, t);
            transform.rotation = nextRot;

            // move and rotate camera object
            CameraObj.transform.localPosition = Vector3.Lerp(startCamObjPos, new Vector3(0f, 0f, -20f), t);
            CameraObj.transform.localRotation = Quaternion.Slerp(startCamObjRot, Quaternion.identity, t);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        CameraObj.transform.localPosition = new Vector3(0, 0, -20);
        CameraObj.transform.localRotation = Quaternion.identity;

        Vector3 finalEuler = transform.rotation.eulerAngles;
        rotX = finalEuler.x;
        rotY = finalEuler.y;

        cameraState = CameraState.Regular;
    }


    public void CameraOverriding()
    {
        // Debug.Log("Override camera is on");
        overrideTimer += Time.deltaTime;
        if (overrideTimer >= overrideDuration)
        {
            cameraState = CameraState.Regular;
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

    private Coroutine transitionOutCoroutine = null;
    void LateUpdate()
    {
        switch (cameraState)
        {
            case CameraState.TransitioningToLoop:
                StartCoroutine(TransitionToLoopState(coroutineDuration));
                return;

            case CameraState.Loop:
                return;

            case CameraState.TransitioningOutOfLoop:
                if (transitionOutCoroutine == null)
                {
                    transitionOutCoroutine = StartCoroutine(TransitionOutOfLoopState(coroutineOutDuration));
                }
                return; 

            default:
                CameraUpdater();
                break; 
        }
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
